[CmdletBinding()]
    param(
        [System.IO.File]$Path,
        [switch]$AddToCA,
        [switch]$Restart
    )
    $ErrorActionPreference = "Stop"

    if (!(Get-Service CertSvc -ErrorAction SilentlyContinue)) {
        Write-Error "Certification Authority is not installed."
        return
    }

    $regTemplate = @'
REGEDIT4

[HKEY_CLASSES_ROOT\PKISolutions_SID.Policy]
@="ADCS.SidExtension.PolicyModule.Policy"

[HKEY_CLASSES_ROOT\PKISolutions_SID.Policy\CLSID]
@="{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}]
@="ADCS.SidExtension.PolicyModule.Policy"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.SidExtension.PolicyModule.Policy"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5db31a0b7668d81"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}\InprocServer32\1.0.0.0]
"Class"="ADCS.SidExtension.PolicyModule.Policy"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5db31a0b7668d81"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}\ProgId]
@="PKISolutions_SID.Policy"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]


[HKEY_CLASSES_ROOT\PKISolutions_SID.PolicyManage]
@="ADCS.SidExtension.PolicyModule.PolicyManage"

[HKEY_CLASSES_ROOT\PKISolutions_SID.PolicyManage\CLSID]
@="{{6369E566-25A1-456C-8CD8-C00D07E59B99}}"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}]
@="ADCS.SidExtension.PolicyModule.PolicyManage"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.SidExtension.PolicyModule.PolicyManage"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5db31a0b7668d81"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}\InprocServer32\1.0.0.0]
"Class"="ADCS.SidExtension.PolicyModule.PolicyManage"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5db31a0b7668d81"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}\ProgId]
@="PKISolutions_SID.PolicyManage"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]
'@
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
function Register-COM($finalPath) {
    $finalPath = $finalPath -replace "\\","/"
    $tempFile = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tempFile, ($regTemplate -f $finalPath))
    [void](reg import $tempFile)
    Remove-Item $tempFile -Force
}
function Copy-Registry() {
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    $script:regPolTemplate += "\$Active\PolicyModules"
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    $src = $regPolTemplate + "\$Active"
    $dest = $regPolTemplate + "\PKISolutions_SID.Policy"
    Copy-Item $src -Destination $dest -Recurse -Force
}

$finalPath = if ($Path.Exists) {
    $Path.FullName
} else {
    $pwd.Path + "\ADCS.SidExtension.PolicyModule.dll"
}
if (!(Test-Path $finalPath)) {
    throw New-Object System.IO.FileNotFoundException "Policy module file is not found."
}
if (!$finalPath.EndsWith("\ADCS.SidExtension.PolicyModule.dll")) {
    throw New-Object System.ArgumentException "Specified file is not Policy module file."
}
Register-COM $finalPath
Copy-Registry

if ($AddToCA) {
    Set-ItemProperty $regPolTemplate -Name "Active" -Value "PKISolutions_SID.Policy"
    if ($Restart) {
        Restart-Service CertSvc
    }
}