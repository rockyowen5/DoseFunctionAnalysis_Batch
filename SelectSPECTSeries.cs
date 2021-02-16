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
    public partial class SelectSPECTSeries : Form
    {
        private static Series v_SeriesSPECT = null;
        public static Series SeriesSPECT
        {
            get { return v_SeriesSPECT; }
            set { v_SeriesSPECT = value; }
        }

        private string imageSeries;
        private Study studySPECT;

        public SelectSPECTSeries()
        {
            InitializeComponent();
        }

        private void SelectSPECTSeries_Load(object sender, EventArgs e)
        {
            studySPECT = SelectSPECTStudy.StudySPECT;
            // Allow User Access to Series Files
            IEnumerator<Series> patientSeries = SelectSPECTStudy.StudySPECT.Series.GetEnumerator();
            while (patientSeries.MoveNext())
            {
                imageSeries = patientSeries.Current.Id;
                comboBox1.Items.Add(imageSeries);
                if (imageSeries == studySPECT.Id)
                {
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(imageSeries);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSeries = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            SelectSPECTStudy.ImageDoseItem.SubItems[1].Text = selectedSeries;
            SeriesSPECT = SelectSPECTStudy.StudySPECT.Series.First(s => s.Id == selectedSeries);
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
