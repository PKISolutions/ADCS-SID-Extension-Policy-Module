# Requirements
- This policy module is written in C# and .NET Framework version 4.7.2. This (or newer) framework must be installed on CA before using policy module.
- This policy module should be installed on Enterprise CAs only. Standalone CAs weren't tested and not supported.

# Installation

## Policy Module registration
Policy module is based on top of COM interfaces and must be registered in operating system in order to be visible to CA. Package contains a PowerShell script that performs COM object registration and (optionally) policy module assignment to CA. `Install-PolicyModule.ps1` script has the following parameters:

- `-Path` -- specifies the path to a policy module file, which is `ADCS.SidExtension.PolicyModule.dll`
- `-AddToCA` -- a switch parameter that assigns this policy module on CA as current policy module.
- `-Restart` -- a switch parameter that restarts CA service to immediately apply changes. This switch is used along with `-AddToCA`.

For example, you extracted policy module in `C:\CA\Policy Modules` folder and want only to register COM objects without making changes in CA:
```PowerShell
.\Install-PolicyModule -Path "C:\CA\Policy Modules\ADCS.SidExtension.PolicyModule.dll"
```
You will have to manually enable policy module on CA.

If you want to register COM objects and configure policy module as active on CA:
```PowerShell
.\Install-PolicyModule -Path "C:\CA\Policy Modules\ADCS.SidExtension.PolicyModule.dll" -AddToCA -Restart
```

## Configure CA to use new policy module
1. Log on to CA with CA Administrator permissions;
2. Launch Certification Authority MMC snap-in (`certsrv.msc`);
3. Select CA node, right-click and select **Properties** menu.
4. Switch to **Policy Modules** tab;
5. Press **Select** button;
6. Select '**NTDS CA Security extension enforcement**' module from the list and press **Ok**;
7. Press **Configure** button to configure policy module;
8. Press **Ok** button to apply changes;
9. When prompted, restart CA service.
