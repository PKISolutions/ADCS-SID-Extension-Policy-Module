using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ADCS.CertMod.Managed;
using Microsoft.Win32;

namespace ADCS.SidExtension.PolicyModule.Forms;

partial class ConfigDialog : Form {
    const String WINDOWS_DEFAULT_PROG_ID = "CertificateAuthority_MicrosoftDefault.Policy";
    const String MY_PROG_ID = "PKISolutions_SID.Policy";

    readonly AppConfig _config;
    readonly BindingList<CertTemplateInfo> _certTemplates;

    public ConfigDialog(AppConfig config) {
        InitializeComponent();

        _config = config;
        _certTemplates = new BindingList<CertTemplateInfo>(CertTemplateCache.GetTemplateInfoList());

        chkDoNotUseGC.Checked = config.GetDsDoNotUseGC();

        cmbTrustedSidPolicy.DataSource = Enum.GetValues(typeof(SidExtensionAction));
        cmbTrustedSidPolicy.SelectedItem = config.GetTrustedSidExtensionPolicy();

        cmbUntrustedSidPolicy.DataSource = Enum.GetValues(typeof(SidExtensionAction));
        cmbUntrustedSidPolicy.SelectedItem = config.GetUntrustedSidExtensionPolicy();

        cmbLogging.DataSource = Enum.GetValues(typeof(LogLevel));
        cmbLogging.SelectedItem = config.GetLogLevel();

        cmbNativePolicy.DataSource = enumPolicyModules();
        cmbNativePolicy.DisplayMember = nameof(PolicyModuleInfo.Name);
        cmbNativePolicy.SelectedItem = PolicyModuleInfo.CreateFromProgID(config.GetNativePolicyModule() ?? WINDOWS_DEFAULT_PROG_ID);

        initDvg();
    }

    void initDvg() {
        var templateColumn = new DataGridViewComboBoxColumn {
            Name = "Template",
            HeaderText = "Template",
            DataSource = _certTemplates,
            DisplayMember = nameof(CertTemplateInfo.DisplayName),
            ValueMember = nameof(CertTemplateInfo.OID),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            Width = 79
        };
        dgvMap.Columns.Add(templateColumn);

        var requesterColumn = new DataGridViewTextBoxColumn {
            HeaderText = "Requester",
            Name = "Requester",
            MaxInputLength = 256,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };
        dgvMap.Columns.Add(requesterColumn);

        try {
            foreach (TemplateRequesterMap mapEntry in _config.GetAuthorizedMap()) {
                dgvMap.Rows.Add(_certTemplates.First(x => x.OID.Equals(mapEntry.TemplateOid)).OID,
                    mapEntry.RequesterName);
            }
        } catch (Exception ex) {
            MessageBox.Show(
                ex.Message + '\n' + ex.StackTrace,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    void btnSave_Click(Object sender, EventArgs e) {
        try {

            _config.SetDsDoNotUseGC(chkDoNotUseGC.Checked);
            _config.SetLogLevel((LogLevel)cmbLogging.SelectedItem);
            _config.SetNativePolicyModule(((PolicyModuleInfo)cmbNativePolicy.SelectedItem)?.ProgID ?? String.Empty);
            _config.SetTrustedSidExtensionPolicy((SidExtensionAction)cmbTrustedSidPolicy.SelectedItem);
            _config.SetUntrustedSidExtensionPolicy((SidExtensionAction)cmbUntrustedSidPolicy.SelectedItem);

            var mapList = new List<String>();
            foreach (DataGridViewRow dgvRow in dgvMap.Rows) {
                String requester = dgvRow.Cells["Requester"].Value as String;
                String oid = Convert.ToString(dgvRow.Cells["Template"].Value);
                CertTemplateInfo template = _certTemplates.FirstOrDefault(t => t.OID.Equals(oid, StringComparison.Ordinal));
                if (template is null || String.IsNullOrWhiteSpace(requester) || !requester.Contains('\\')) {
                    continue;
                }

                mapList.Add($"{template.OID}:{requester}");
            }
            _config.SetAuthorizedMap(mapList);

            MessageBox.Show(
                "Before the configuration changes can take effect, you must restart the Certification Authority.",
                "Policy Module",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        } catch (Exception ex) {
            MessageBox.Show(
                $"Failed to save settings:\n{ex.Message}\n{ex.StackTrace}.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    static List<PolicyModuleInfo> enumPolicyModules() {
        var regex = new Regex(@"\.Policy(\.\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var retValue = new List<PolicyModuleInfo>();
        var progIdList = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
        try {
            using (RegistryKey key = Registry.ClassesRoot) {
                foreach (String subKey in key.GetSubKeyNames()) {
                    if (regex.IsMatch(subKey)) {
                        progIdList.Add(subKey);
                    }
                }
            }

            progIdList.Remove(MY_PROG_ID);
            retValue.AddRange(progIdList.Select(PolicyModuleInfo.CreateFromProgID));
        } catch (Exception ex) {
            MessageBox.Show(
                $"Failed to enumerate registered policy modules:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        return retValue;
    }
}