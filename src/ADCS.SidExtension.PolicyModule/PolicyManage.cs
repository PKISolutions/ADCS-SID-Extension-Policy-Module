using System;
using System.Runtime.InteropServices;
using System.Text;
using ADCS.CertMod.Managed;
using ADCS.SidExtension.PolicyModule.Forms;

namespace ADCS.SidExtension.PolicyModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("PKISolutions_SID.PolicyManage")]
[Guid("6369e566-25a1-456c-8cd8-c00d07e59b99")]
public class PolicyManage : ICertManageModule {
    readonly ILogWriter _logWriter;
    readonly AppConfig _config;
        
    public PolicyManage(ILogWriter logger, AppConfig config) {
        _logWriter = logger;
        _config = config;
    }

    /// <inheritdoc />
    public void SetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags, ref Object pvarProperty) {
        _config.InitializeConfig(strConfig);
        if (strPropertyName == "HWND") {
            var hwnd = (IntPtr)BitConverter.ToInt64(Encoding.Unicode.GetBytes((String)pvarProperty), 0);
            _logWriter.LogDebug(DebugString.EXITMANAGE_SETPROPERTY, strConfig, strStorageLocation, strPropertyName, Flags, hwnd);
        } else {
            _logWriter.LogDebug(DebugString.EXITMANAGE_SETPROPERTY, strConfig, strStorageLocation, strPropertyName, Flags, pvarProperty);
        }
        // do nothing
    }
    /// <inheritdoc />
    public void Configure(String strConfig, String strStorageLocation, Int32 Flags) {
        _config.InitializeConfig(strConfig);
        _logWriter.LogDebug(DebugString.EXITMANAGE_CONFIGURE, strConfig, strStorageLocation, Flags);
        try {
            CertTemplateCache.Start(_logWriter);
            new ConfigDialog(_config).ShowDialog();
        } catch (Exception ex) {
            _logWriter.LogError(ex, "[PolicyManage::Configure]");
        }
    }
    /// <inheritdoc />
    public Object GetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags) {
        _config.InitializeConfig(strConfig);
        _logWriter.LogDebug(DebugString.EXITMANAGE_GETPROPERTY, strConfig, strStorageLocation, strPropertyName, Flags);
        switch (strPropertyName.ToLower()) {
            case "name":
                return "NTDS CA Security extension Enforcement";
            case "description":
                return "Adds protection to CA from NTDS Security\r\nextension spoofing in offline requests.";
            case "copyright":
                return "Copyright (c) 2023, PKI Solutions LLC";
            case "file version":
                return "1.0";
            case "product version":
                return "1.0";
            default: return $"Unknown Property: {strPropertyName}";
        }
    }
}