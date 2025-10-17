// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OneToOneConstraintViolationInMemoryTest : IClassFixture<OneToOneConstraintViolationInMemoryTest.OneToOneConstraintViolationFixture>
{
    public OneToOneConstraintViolationInMemoryTest(OneToOneConstraintViolationFixture fixture)
    {
        Fixture = fixture;
    }

    protected OneToOneConstraintViolationFixture Fixture { get; }

    [ConditionalFact]
    public virtual void Warns_when_multiple_dependents_exist_for_one_to_one_relationship()
    {
        using var context = CreateContext();
        var loggerFactory = (ListLoggerFactory)context.GetService<ILoggerFactory>();
        loggerFactory.Clear();

        // Query children which includes the parent
        var children = context.Set<Child>()
            .Include(c => c.Parent)
            .ToList();

        // Should have 2 children
        Assert.Equal(2, children.Count);

        // Both children point to the same parent in the database, but only one should have the navigation set
        var childrenWithParent = children.Count(c => c.Parent != null);
        Assert.Equal(1, childrenWithParent);

        // Verify warning was logged
        var warningLog = loggerFactory.Log.FirstOrDefault(
            l => l.Id == CoreEventId.MultipleReferenceNavigationPropertiesInOneToOneRelationshipWarning);
        Assert.NotNull(warningLog);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.Contains("Child.Parent", warningLog.Message);
        Assert.Contains("Parent", warningLog.Message);
    }

    [ConditionalFact]
    public virtual void No_warning_when_one_to_one_relationship_is_valid()
    {
        using var context = CreateContext();
        var loggerFactory = (ListLoggerFactory)context.GetService<ILoggerFactory>();
        loggerFactory.Clear();

        // Query valid children (ValidChild/ValidParent have a proper one-to-one relationship)
        var validChildren = context.Set<ValidChild>()
            .Include(c => c.Parent)
            .ToList();

        // Should have 1 child with a parent
        Assert.Single(validChildren);
        Assert.NotNull(validChildren[0].Parent);

        // Verify no warning was logged
        var warningLog = loggerFactory.Log.FirstOrDefault(
            l => l.Id == CoreEventId.MultipleReferenceNavigationPropertiesInOneToOneRelationshipWarning);
        Assert.Null(warningLog);
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public class OneToOneConstraintViolationFixture : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "OneToOneConstraintViolationTest";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging().ConfigureWarnings(
                w => w.Log(CoreEventId.MultipleReferenceNavigationPropertiesInOneToOneRelationshipWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            // Configure one-to-one relationship that will be violated in the database
            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Child)
                .WithOne(c => c.Parent)
                .HasForeignKey<Child>(c => c.ParentId);

            // Configure a valid one-to-one relationship
            modelBuilder.Entity<ValidParent>()
                .HasOne(p => p.Child)
                .WithOne(c => c.Parent)
                .HasForeignKey<ValidChild>(c => c.ParentId);
        }

        protected override Task SeedAsync(DbContext context)
        {
            var parent = new Parent { Id = 1, Name = "Parent 1" };
            context.Set<Parent>().Add(parent);

            // Add two children pointing to the same parent - this violates the one-to-one constraint
            var child1 = new Child { Id = 1, Name = "Child 1", ParentId = 1 };
            var child2 = new Child { Id = 2, Name = "Child 2", ParentId = 1 };
            context.Set<Child>().AddRange(child1, child2);

            // Add valid one-to-one data
            var validParent = new ValidParent { Id = 1, Name = "Valid Parent 1" };
            context.Set<ValidParent>().Add(validParent);

            var validChild = new ValidChild { Id = 1, Name = "Valid Child 1", ParentId = 1 };
            context.Set<ValidChild>().Add(validChild);

            return context.SaveChangesAsync();
        }
    }

    protected class Parent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Child Child { get; set; }
    }

    protected class Child
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public Parent Parent { get; set; }
    }

    protected class ValidParent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ValidChild Child { get; set; }
    }

    protected class ValidChild
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public ValidParent Parent { get; set; }
    }
}
