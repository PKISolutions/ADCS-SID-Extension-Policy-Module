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
Windows Registry Editor Version 5.00

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
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}\InprocServer32\1.0.0.0]
"Class"="ADCS.SidExtension.PolicyModule.Policy"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
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
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}\InprocServer32\1.0.0.0]
"Class"="ADCS.SidExtension.PolicyModule.PolicyManage"
"Assembly"="ADCS.SidExtension.PolicyModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
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
    Set-Content -Path $tempFile -Value ($regTemplate -f $finalPath)
    reg import $tempFile | Out-Null
    Remove-Item $tempFile -Force
}
function Copy-Registry() {
    # set active CA node
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    $script:regPolTemplate += "\$Active\PolicyModules"
    # set active policy module node
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    if ($Active -ne "PKISolutions_SID.Policy") {
        $src = $regPolTemplate + "\$Active" -replace ":"
        $dest = $regPolTemplate + "\PKISolutions_SID.Policy" -replace ":"
        reg copy "$src" "$dest" /s /f | Out-Null
    }
}

$finalPath = if ($Path.Exists) {
    $Path
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
