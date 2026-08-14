namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Defines the lifetime of a service registration.
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// A single shared instance across the application.
    /// </summary>
    Singleton,

    /// <summary>
    /// An instance scoped to a resolution context.
    /// </summary>
    Scoped,

    /// <summary>
    /// A new instance each time it is resolved.
    /// </summary>
    Transient
}
