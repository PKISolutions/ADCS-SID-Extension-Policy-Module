# Policy Module Configuration

This policy module can be configured in two ways:
1. using GUI interface
2. using CLI interface (`certutil.exe`)

## GUI
GUI interface is available on CAs installed on Windows Server with Desktop Experience feature enabled (full GUI). Use the following steps to invoke policy module configuration GUI:
1. Log on to CA server with CA administrator permissions;
2. Launch CA management console (`certsrv.msc)
3. Select CA node, right-click and select **Properties** menu;
4. On CA properties, switch to **Policy Modules** tab;
5. Ensure that **NTDS CA Security extension Enforcement** is current policy module. If not, press **Select** button and select **NTDS CA Security extension Enforcement** policy module.
6. Press **Configure** button

Here is an example of GUI configuration dialog:
![image](https://user-images.githubusercontent.com/6384119/223392352-a71535aa-7368-471d-82e3-bf49e9263c73.png)

You can configure the following sections:
### Untrusted SID Extension policy
This section configures policy module when incoming request includes user-crafted SID extension and request doesn't fall to Template/Requester mapping table. The following actions can be set:
Action|Description
-|-
**PassThrough** (Default)|Passes request to underlying policy module unmodified and takes no actions on request. Underlying policy module result is returned back to CA. This may result in request to place in pending request if underlying policy module asks to do so.
**Suppress**|Removes SID extension and/or SID value stored in SAN extension from request and issued certificate (if underlying policy module allows certificate issuance).
**Pending**|Puts request into pending state even if underlying policy module allows certificate issuance.
**Deny**|Forcibly denies request.

**Note:** these actions have effect only when underlying policy module allows certificate issuance and requested template if offline (accepts subject information from request). If requested template is online, then underlying policy module result is returned back to CA.

### Trusted SID Extension policy
This section configures policy module when incoming request falls to at least one Template/Requester mapping table and policy module was unable to retrieve identity account based on request information. The following actions can be set:
Action|Description
-|-
**PassThrough** (Default)|Passes request to underlying policy module unmodified and takes no actions on request. Underlying policy module result is returned back to CA. This may result in request to place in pending request if underlying policy module asks to do so.
**Suppress**|Removes SID extension and/or SID value stored in SAN extension from request and issued certificate (if underlying policy module allows certificate issuance). This action have effect only if incoming request contains SID extension.
**Pending**|Puts request into pending state even if underlying policy module allows certificate issuance.
**Deny**|Forcibly denies request.

**Note:** these actions have effect only when requested template is offline underlying policy module allows certificate issuance. If requested template is online,  then underlying policy module result is returned back to CA.

### Logging Level
Configures logging level. Policy module implements code flow logging to a file. Log file location is: `%windir%\AdcsCertMod.SID.Policy.log`. By default, logging is not enabled. By increasing logging level from Ciritical to Trace levels, the amount of log data written is increased. Be careful, because on high volume CA, Trace level will generate a lot of data and may result in full disk.

### Native Policy Module
Configures underlying policy module to use. By default, **Windows Default** policy module is used. You can select different policy module (such as CLM) if it is installed and you want to use it along with this policy module.
 se
### Active Directory
This section contains configuration about how to retrieve account information from Active Directory. The following settings are available:
- **Do not use Global Catalog**. This setting has effect only in domains with trusts (multi-domain forest or with trusts between different domains in different forests). By default (checked), this policy module will attempt to establish a LDAP connection to trusted domain and execute account search. When unchecked, policy module attempts to locate account by querying global catalog (GC) installed in current domain. See [Account Lookup](https://github.com/PKISolutions/ADCS-SID-Extension-Policy-Module/blob/master/docs/account-lookup.md) page about account lookup options and details.

### Template/Requester mapping
This setting represent a set of mappings between offline templates and original requesters. This policy module will attempt account lookup only when request matches at least one mapping: request is against offline template in **Template** column and requester matches the requester in **Requester** column.


## CLI
CLI configuration is available on all CA installations, including Server Core.

**Note:** provided certutil CLI examples are available only after policy module is configured as default policy module.

### Untrusted SID Extension policy
```
certutil -setreg policy\UntrustedSidExtensionPolicy %policy%
```
where `%policy%` is a number that maps to the following actions
%policy% value|policy name
-|-
0|PassThrough (Default)
1|Suppress
2|Pending
3|Deny

### Trusted SID Extension policy
```
certutil -setreg policy\TrustedSidExtensionPolicy %policy%
```
where `%policy%` is a number that maps to the following actions
%policy% value|policy name
-|-
0|PassThrough (Default)
1|Suppress
2|Pending
3|Deny

### Logging Level
```
certutil -setreg policy\LogLevel %level%
```
where `%level%` is a number that maps to the following actions
%level% value|Log level name
-|-
0|None (Default)
1|Trace
2|Debug
3|Information
4|Warning
5|Error
6|Critical

### Native Policy Module
This setting must be configured only when you want to use non-default policy module, for example FIM CM. This setting should be null or absent in order to use Windows Default policy module. An example to configure FIM CM policy module as underlying policy module:
```
certutil -setreg policy\NativeProgID Clm.Policy
```
Command parameter is policy module's COM ProgID. Consult with custom policy module documentation for COM ProgID.

### Active Directory
- Disable global catalog queries (use direct LDAP queries instead):
```
certutil -setreg policy\DoNotUseGC 1
```
- Enable global catalog queries (do not use direct LDAP queries):
```
certutil -setreg policy\DoNotUseGC 0
```

### Template/Requester mapping
Template/requester mapping syntax includes a string that contains template OID and requester name in a form `DomainShortName\AccountSAMName` delimited by a collon. Use the following syntax to add a new map entry for template with `OID=1.3.6.1.4.1.311.21.8.149510.7314491.15746959.9320746.3700693.37.3678593.5087990` and `CONTOSO\IntuneSvcAccount` as allowed requester for this template:
```
certutil -setreg policy\AuthorizedMap +"1.3.6.1.4.1.311.21.8.149510.7314491.15746959.9320746.3700693.37.3678593.5087990:CONTOSO\IntuneSvcAccount"
```
use the following syntax to remove map entry:
```
certutil -setreg policy\AuthorizedMap -"1.3.6.1.4.1.311.21.8.149510.7314491.15746959.9320746.3700693.37.3678593.5087990:CONTOSO\IntuneSvcAccount"
```
