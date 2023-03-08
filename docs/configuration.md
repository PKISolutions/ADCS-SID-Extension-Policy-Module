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
**Suppress**|Removes SID extension from request and issued certificate (if underlying policy module allows certificate issuance).
**Pending**|Puts request into pending state even if underlying policy module allows certificate issuance.
**Deny**|Forcibly denies request.

**Note:** these actions have effect only when underlying policy module allows certificate issuance and requested template if offline (accepts subject information from request). If requested template is online, then underlying policy module result is returned back to CA.

### Trusted SID Extension policy
This section configures policy module when incoming request falls to at least one Template/Requester mapping table and policy module was unable to retrieve identity account based on request information. The following actions can be set:
Action|Description
-|-
**PassThrough** (Default)|Passes request to underlying policy module unmodified and takes no actions on request. Underlying policy module result is returned back to CA. This may result in request to place in pending request if underlying policy module asks to do so.
**Suppress**|Removes SID extension from request and issued certificate (if underlying policy module allows certificate issuance). This action have effect only if incoming request contains SID extension.
**Pending**|Puts request into pending state even if underlying policy module allows certificate issuance.
**Deny**|Forcibly denies request.

**Note:** these actions have effect only when requested template is offline underlying policy module allows certificate issuance. If requested template is online,  then underlying policy module result is returned back to CA.

### Logging Level
Configures logging level. Policy module implements code flow logging to a file. Log file location is: `%windir%\AdcsCertMod.SID.Policy.log`. By default, logging is not enabled. By increasing logging level from Ciritical to Trace levels, the amount of log data written is increased. Be careful, because on high volume CA, Trace level will generate a lot of data and may result in full disk.

### Native Policy Module
Configures underlying policy module to use. By default, **Windows Default** policy module is used. You can select different policy module (such as CLM) if it is installed and you want to use it along with this policy module.

### Active Directory
This section contains configuration about how to retrieve account information from Active Directory. The following settings are available:
- **Do not use Global Catalog**. This setting has effect only in domains with trusts (multi-domain forest or with trusts between different domains in different forests). By default (unchecked), this policy module attempts to locate account by querying global catalog (GC) installed in current domain. CA doesn't require to have a direct LDAP connection to target domain if account belongs to trusted domain, this work is delegated to global catalogs. When checked, CA will attempt to establish a LDAP connection to trusted domain and execute account search.

### Template/Request mapping
This setting represent a set of mappings between offline templates and original requesters. This policy module will attempt account lookup only when request matches at least one mapping: request is against offline template in **Template** column and requester matches the requester in **Requester** column.
