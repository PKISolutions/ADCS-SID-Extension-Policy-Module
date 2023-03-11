# Description

This policy module is an extension for Active Directory Certificate Services (AD CS) that addresses certificate-based authentication changes on Windows domain controllers introduced in [KB5014754](https://support.microsoft.com/kb/5014754). After installing this update, numerous previously strong mapping (mapping between client certificate and account in Active Directory) types are treated as weak mapping types. To address this issue, a new certificate extension is defined that contains target account security identifier (SID) that allows KDC to bind client certificate to an account. That is, for successful authentication using client certificate, a certificate must pass two checks:

1. Make a mapping between certificate and account using any of supported mapping types (see KB article);
2. Validate if SID value encoded in certificate matches the SID of an account bound in step 1.

In other words, presence of new SID extension will make all mapping types strong.

From May 10th, 2022 till Nov 14th 2023 AD environment will work in compatibility mode, where you can enable audit logging and determine accounts/certificates that don't meet enforcement requirements. After Nov 14th, 2023 AD environment will change to enforced mode and KDC will reject all client certificates that don't meet strong mapping requirements.

# What problem this Policy Module solves?

Microsoft updated Enterprise CAs to automatically include new SID extension into certificates issued against online (where subject is built from AD) certificate templates. However there are other common use cases which are not covered by this update. This includes scenarios when identity certificates are issued using NDES/SCEP service, Microsoft Intune and others.

Microsoft Intune is able to issue certificates to non-domain devices (computers, phones, tablets, whatever else, etc.) which later are used to authenticate to domain reqources, VPNs, wireless networks using role-based authorization. That is, admins create user accounts for non-domain devices and devices are mapped to these accounts using client certificates. The problem here is that Intune uses offline (where subject is supplied in request) templates (via NDES) and Enterprise CA is not able to perform target account retrieval and new SID extension inclusion in client certificate. As the result, certificates issued through Intune will be rejected by KDC after AD is switched to enforcement mode (either, manually or after Nov 14, 2023) and role-based authorization will fail.

There is another problem with new extension and offline templates: CA will accept user-crafted SID extension supplied in request that can cause legitimate identity impersonation and privilege escalation, because CA doesn't validate extension value and blindly copy extension value from request to issued certificate. There is no way to automatically ignore manually created SID extension in incoming request.

This policy module solves both problems by providing flexible configuration and additional actions based on request details:
1. Policy module can automatically retrieve target identity account and include SID extension in certificate requested through NDES/Intune.
2. You define rules (Template name <-> Requester name) when identity retrieval is performed. Only requests that fall to configuration will be processed by policy module.
3. You define action when incoming request contains potentially fraudulent SID extension.

# How this policy module works together with custom policy modules we are using?

Unlike exit modules, Microsoft CA can run only one policy module at any time. This means that any selected policy module will be solely responsible for request validation and processing. Microsoft built-in policy module performs extensive and sophisticated validations which are error-prone in custom policy module implementation. Additional problems would arise if you already use non-default policy module, for example from Microsoft Certificate Lifecycle Manager (CLM) which adds custom functionality. By replacing CLM policy module with another, you would loose CLM functionality.

I've addressed this by chaining policy modules inside the code. That is, this policy module is implemented on top of any existing policy module. When request is arrived to CA, or CA manager calls Resubmit action on pending request, CA will notify this policy module about activity and will await for response (issue, deny, put in pending state) and allows to modify issued certificate. We call underlying policy module (configurable), pass same request details and wait for result. If underlying policy module requests to deny the request, this policy module will propagate result back to CA. If underlying policy module responds with successful status, this policy module will start its own logic granted that all mandatory validations and request processing are completed by underlying policy module.

# Licensing and security considerations

This policy module is released under MIT license and free for personal and commercial use. Given that policy module is supposed to run on a mission-critical server, I did all my best to ensure code accuracy, reliability, security and integrity. Source code is open and free for analysis or creating a derivative work.

# Installation and configuration
Follow installation guides to install and configure the policy module:
- [Installation guide](https://github.com/PKISolutions/ADCS-SID-Extension-Policy-Module/blob/master/docs/installation.md)
- [Configuration guide](https://github.com/PKISolutions/ADCS-SID-Extension-Policy-Module/blob/master/docs/configuration.md)
