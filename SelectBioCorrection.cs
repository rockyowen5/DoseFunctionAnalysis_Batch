using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFHAnalysis
{
    public partial class SelectBioCorrection : Form
    {
        private static Dictionary<int, string> v_BioCorrectDictionary = new Dictionary<int, string>();
        public static Dictionary<int, string> BioCorrectDictionary
        {
            get { return v_BioCorrectDictionary; }
            set { v_BioCorrectDictionary = value; }
        }

        private string useLQCorrection;
        private int loadedPlans;

        public SelectBioCorrection()
        {
            InitializeComponent();
        }

        private void SelectBioCorrection_Load(object sender, EventArgs e)
        {
            loadedPlans = UserInterface.LoadedPlans;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            useLQCorrection = "Yes";
            BioCorrectDictionary.Add(loadedPlans, useLQCorrection);
            SelectSPECTStudy.ImageDoseItem.SubItems[6].Text = useLQCorrection;
            this.Close();
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            useLQCorrection = "No";
            BioCorrectDictionary.Add(loadedPlans, useLQCorrection);
            SelectSPECTStudy.ImageDoseItem.SubItems[6].Text = useLQCorrection;
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
