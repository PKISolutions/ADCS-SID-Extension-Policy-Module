namespace ADCS.SidExtension.PolicyModule;

/// <summary>
/// Specifies the SID extension handling options in incoming request.
/// </summary>
public enum SidExtensionAction {
    /// <summary>
    /// SID extension from incoming request is copied to resulting certificate unmodified.
    /// </summary>
    PassThrough = 0,
    /// <summary>
    /// SID extension from incoming request is disabled and not copied to resulting certificate.
    /// </summary>
    Suppress    = 1,
    /// <summary>
    /// The request is set unmodified into pending state for further manual review/modifications.
    /// </summary>
    Pending     = 2,
    /// <summary>
    /// Denies incoming request if it contains SID extension.
    /// </summary>
    Deny        = 3
}