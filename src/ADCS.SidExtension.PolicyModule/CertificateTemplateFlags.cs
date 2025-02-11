﻿using System;

namespace ADCS.SidExtension.PolicyModule;

[Flags]
public enum CertificateTemplateFlags {
    /// <summary>
    /// None.
    /// </summary>
    None = 0,
    /// <summary>
    /// This flag is reserved and not used.
    /// </summary>
    EnrolleeSuppliesSubject = 0x1,
    /// <summary>
    /// This flag is reserved and not used.
    /// </summary>
    AddEmail                = 0x2,
    /// <summary>
    /// This flag is reserved and not used.
    /// </summary>
    AddObjectIdentifier     = 0x4,
    /// <summary>
    /// This flag is reserved and not used.
    /// </summary>
    DsPublish               = 0x8,
    /// <summary>
    /// This flag is reserved and not used.
    /// </summary>
    AllowKeyExport          = 0x10,
    /// <summary>
    /// This flag indicates whether clients can perform autoenrollment for the specified template.
    /// </summary>
    Autoenrollment          = 0x20,
    /// <summary>
    /// This flag indicates that this certificate template is for an end entity that represents a machine.
    /// </summary>
    MachineType             = 0x40,
    /// <summary>
    /// This flag indicates a certificate request for a CA certificate.
    /// </summary>
    IsCA                    = 0x80,
    /// <summary>
    /// Adds requester distinguished name.
    /// </summary>
    AddDirectoryPath        = 0x100,
    /// <summary>
    /// This flag indicates that a certificate based on this section needs to include a template name certificate extension.
    /// </summary>
    AddTemplateName         = 0x200,
    /// <summary>
    /// Adds requester distinguished name.
    /// </summary>
    AddSubjectDirectoryPath = 0x400,
    /// <summary>
    /// This flag indicates a certificate request for cross-certifying a certificate.
    /// </summary>
    IsCrossCA               = 0x800,
    /// <summary>
    /// This flag indicates that the record of a certificate request for a certificate that is issued need not be persisted by the CA.
    /// <para><strong>Windows Server 2003, Windows Server 2008</strong> - this flag is not supported.</para>
    /// </summary>
    DoNotPersistInDB        = 0x1000,
    /// <summary>
    /// This flag indicates that the template SHOULD not be modified in any way.
    /// </summary>
    IsDefault               = 0x10000,
    /// <summary>
    /// This flag indicates that the template MAY be modified if required.
    /// </summary>
    IsModified              = 0x20000,
    /// <summary>
    /// N/A
    /// </summary>
    IsDeleted               = 0x40000,
    /// <summary>
    /// N/A
    /// </summary>
    PolicyMismatch          = 0x80000
}