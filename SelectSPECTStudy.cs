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
    public partial class SelectSPECTStudy : Form
    {
        private static Study v_StudySPECT = null;
        public static Study StudySPECT
        {
            get { return v_StudySPECT; }
            set { v_StudySPECT = value; }
        }
        private static ListViewItem v_ImageDoseItem = null;
        public static ListViewItem ImageDoseItem
        {
            get { return v_ImageDoseItem; }
            set { v_ImageDoseItem = value; }
        }

        public SelectSPECTStudy()
        {
            InitializeComponent();
        }

        private void SelectSPECTImage_Load(object sender, EventArgs e)
        {
            // Allow User Access to Study Files
            IEnumerator<Study> patientStudies = Script.CurrentPatient.Studies.GetEnumerator();
            while (patientStudies.MoveNext())
            {
                comboBox1.Items.Add(patientStudies.Current.Id.ToString());
            }
            ImageDoseItem = new ListViewItem();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedStudy = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            ImageDoseItem = UserInterface.imageDoseList.Items.Add(selectedStudy);
            ImageDoseItem.Checked = true;
            for (int i = 0; i < UserInterface.imageDoseList.Columns.Count - 1; i++)
            {
                ImageDoseItem.SubItems.Add("");
            }
            StudySPECT = Script.CurrentPatient.Studies.First(s => s.Id == selectedStudy);
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
