using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFHAnalysis
{
    public partial class SelectSPECTImage : Form
    {
        private static Dictionary<int,Image> v_SPECTDictionary = new Dictionary<int, Image>();
        public static Dictionary<int,Image> SPECTDictionary
        {
            get { return v_SPECTDictionary; }
            set { v_SPECTDictionary = value; }
        }
        private static Dictionary<int, string> v_ImageNameDictionary = new Dictionary<int, string>();
        public static Dictionary<int, string> ImageNameDictionary
        {
            get { return v_ImageNameDictionary; }
            set { v_ImageNameDictionary = value; }
        }
        private static DialogResult v_ImageDialogResult;
        public static DialogResult ImageDialogResult
        {
            get { return v_ImageDialogResult; }
            set { v_ImageDialogResult = value; }
        }

        private string patientImageString;
        private int firstIndex;

        public SelectSPECTImage()
        {
            InitializeComponent();
        }

        private void SelectSPECTImage_Load(object sender, EventArgs e)
        {
            // Allow User Access to Image Files
            IEnumerator<Image> patientImages = SelectSPECTSeries.SeriesSPECT.Images.GetEnumerator();
            while (patientImages.MoveNext())
            {
                patientImageString = patientImages.Current.Id;
                comboBox1.Items.Add(patientImageString);
                if (patientImageString.EndsWith("PERF") == true)
                {
                    firstIndex = comboBox1.Items.IndexOf(patientImageString);
                    comboBox1.SelectedIndex = firstIndex;
                }
                else if (patientImageString.EndsWith("VENT") == true)
                {
                    firstIndex = comboBox1.Items.IndexOf(patientImageString);
                    comboBox1.SelectedIndex = firstIndex;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedImage = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            string imageName = SelectSPECTStudy.StudySPECT.Id + "/" + SelectSPECTSeries.SeriesSPECT.Id + "/" + selectedImage;
            ImageNameDictionary.Add(UserInterface.LoadedPlans, imageName);
            Image patientSPECT = SelectSPECTSeries.SeriesSPECT.Images.First(s => s.Id == selectedImage);
            SPECTDictionary.Add(UserInterface.LoadedPlans, patientSPECT);
            SelectSPECTStudy.ImageDoseItem.SubItems[2].Text = selectedImage;
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
