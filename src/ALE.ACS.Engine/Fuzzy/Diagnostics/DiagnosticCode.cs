namespace ALE.ACS.Engine.Fuzzy.Diagnostics;

/// <summary>
/// Identifies the kind of diagnostic produced during validation.
/// </summary>
public enum DiagnosticCode
{
    MissingVariable,
    MissingTerm,
    DuplicateRule,
    InvalidUniverse,
    RuleLoop,
    DeadRule,
    UnreachableRule,
    OverlappingTerms
}

/// <summary>
/// Indicates the severity of a diagnostic.
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}
