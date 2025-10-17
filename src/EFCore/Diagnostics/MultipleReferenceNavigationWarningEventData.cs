// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     multiple dependents exist for a unique foreign key (one-to-one relationship).
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MultipleReferenceNavigationWarningEventData : NavigationBaseEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="principalEntityType">The principal entity type.</param>
    /// <param name="dependentCount">The number of dependent entities found.</param>
    public MultipleReferenceNavigationWarningEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        INavigationBase navigation,
        IEntityType principalEntityType,
        int dependentCount)
        : base(eventDefinition, messageGenerator, navigation)
    {
        PrincipalEntityType = principalEntityType;
        DependentCount = dependentCount;
    }

    /// <summary>
    ///     The principal entity type.
    /// </summary>
    public virtual IEntityType PrincipalEntityType { get; }

    /// <summary>
    ///     The number of dependent entities found.
    /// </summary>
    public virtual int DependentCount { get; }
}
