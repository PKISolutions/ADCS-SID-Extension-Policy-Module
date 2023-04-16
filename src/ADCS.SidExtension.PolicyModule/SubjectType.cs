namespace ADCS.SidExtension.PolicyModule;

/// <summary>
/// Contains enumeration of certificate template subject types.
/// </summary>
public enum SubjectType {
    /// <summary>
    /// Enrollment recipient is user entity.
    /// </summary>
    User = 0,
    /// <summary>
    /// Enrollment recipient is computer or device.
    /// </summary>
    Computer = 1,
    /// <summary>
    /// Enrollment recipient is Certification Authority.
    /// </summary>
    CA = 2,
    /// <summary>
    /// Enrollment recipient is Cross-CertificationAuthority.
    /// </summary>
    CrossCA = 3
}