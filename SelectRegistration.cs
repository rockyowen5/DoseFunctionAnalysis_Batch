using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFHAnalysis
{
    public partial class SelectRegistration : Form
    {
        private static Registration v_PatientRegistration = null;
        public static Registration PatientRegistration
        {
            get { return v_PatientRegistration; }
            set { v_PatientRegistration = value; }
        }

        private Study studySPECT;
        private string registrationName;

        public SelectRegistration()
        {
            InitializeComponent();
        }

        private void SelectRegistration_Load(object sender, EventArgs e)
        {
            studySPECT = SelectSPECTStudy.StudySPECT;
            IEnumerator<Registration> registrationSelect = Script.CurrentPatient.Registrations.GetEnumerator();
            while (registrationSelect.MoveNext())
            {
                registrationName = registrationSelect.Current.Id;
                comboBox1.Items.Add(registrationName);
                if (registrationName == studySPECT.Id.ToUpper())
                {
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(registrationName);
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRegistration = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            SelectSPECTStudy.ImageDoseItem.SubItems[5].Text = selectedRegistration;
            PatientRegistration = Script.CurrentPatient.Registrations.First(s => s.Id == selectedRegistration);
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
