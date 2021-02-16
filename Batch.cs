using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace DFHAnalysis
{
    public partial class Batch : Form
    {
        public static double doseThreshold = 5;
        public static string patientDataFile;
        private Excel.Application excelApp = new Excel.Application();
        private Excel.Workbook excelWork;
        private Excel.Sheets excelSheets;
        private Excel.Worksheet patientData;
        public static List<string> patientIDs = new List<string>();
        public static List<string> patientCourses = new List<string>();
        public static List<string> planTypes = new List<string>();
        public static List<string> planNames = new List<string>();
        public static List<string> SBRT = new List<string>();
        public static List<string> CTstudy = new List<string>();
        public static List<string> CTseries = new List<string>();
        public static List<string> CTname = new List<string>();
        public static List<string> preTxPERF = new List<string>();
        public static List<string> preTxVENT = new List<string>();
        public static List<string> midTxPERF = new List<string>();
        public static List<string> midTxVENT = new List<string>();
        public static string bioCorrection;
        public static string perfVar;
        public static string ventVar;
        public static DialogResult formResult;

        public Batch()
        {
            InitializeComponent();
        }

        private void Batch_Load(object sender, EventArgs e)
        {
            this.checkedListBox1.Items.Add("Volume[cm\xB3]");
            this.checkedListBox1.Items.Add("Max Dose[Gy]");
            this.checkedListBox1.Items.Add("Mean Dose[Gy]");
            this.checkedListBox1.Items.Add("StDv Dose[Gy]");
            this.checkedListBox1.Items.Add("gEUD[Gy]");
            this.checkedListBox1.Items.Add("V20[%]");
            this.checkedListBox1.Items.Add("Max Intensity");
            this.checkedListBox1.Items.Add("Mean Intensity");
            this.checkedListBox1.Items.Add("StDv Intensity");
            this.checkedListBox1.Items.Add("gEUfD");
            this.checkedListBox1.Items.Add("fVD[%]");
            this.checkedListBox1.Items.Add("MfLD[Gy]");
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
            this.AcceptButton = button4;
            radioButton7.Checked = true;
            label12.Text = null;


        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            label9.Visible = true;
            label8.Visible = true;
            textBox2.Visible = true;
            textBox2.Text = doseThreshold.ToString();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            label8.Visible = false;
            label9.Visible = false;
            textBox2.Visible = false;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            label8.Visible = false;
            label9.Visible = false;
            textBox2.Visible = false;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (label12.Text == null)
            {
                MessageBox.Show("Please select a patient data file before continuing.");
            }
            else
            {
            excelWork = excelApp.Workbooks.Open(patientDataFile);
            excelSheets = excelWork.Worksheets;
            patientData = excelSheets.Item["Patient Data"];
            var patientIDRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["E"]);
            var courseRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["F"]);
            var planTypeRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["G"]);
            var planNameRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["H"]);
            var SBRTrange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["I"]);
            var CTstudyRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["J"]);
            var CTseriesRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["K"]);
            var CTnameRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["L"]);
            var preTxPERFRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["M"]);
            var preTxVENTRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["N"]);
            var midTxPERFRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["O"]);
            var midTxVENTRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["P"]);
            var patientIDValue = patientIDRange.Value2;
            var courseValue = courseRange.Value2;
            var planTypeValue = planTypeRange.Value2;
            var planNameValue = planNameRange.Value2;
            var SBRTvalue = SBRTrange.Value2;
            var CTstudyValue = CTstudyRange.Value2;
            var CTseriesValue = CTseriesRange.Value2;
            var CTnameValue = CTnameRange.Value2;
            var preTxPERFValue = preTxPERFRange.Value2;
            var preTxVENTValue = preTxVENTRange.Value2;
            var midTxPERFValue = midTxPERFRange.Value2;
            var midTxVENTValue = midTxVENTRange.Value2;
            var patientValue2 = (object[,])patientIDValue;
            var courseValue2 = (object[,])courseValue;
            var planTypeValue2 = (object[,])planTypeValue;
            var planNameValue2 = (object[,])planNameValue;
            var SBRTvalue2 = (object[,])SBRTvalue;
            var CTstudyValue2 = (object[,])CTstudyValue;
            var CTseriesValue2 = (object[,])CTseriesValue;
            var CTnameValue2 = (object[,])CTnameValue;
            var preTxPERFValue2 = (object[,])preTxPERFValue;
            var preTxVENTValue2 = (object[,])preTxVENTValue;
            var midTxPERFValue2 = (object[,])midTxPERFValue;
            var midTxVENTValue2 = (object[,])midTxVENTValue;
            var rowCount = patientValue2.GetLength(0);
            for (int row = 2; row < rowCount + 1; row++)
            {
                var patientID = patientValue2[row,1];
                var patientCourse = courseValue2[row, 1];
                var planType = planTypeValue2[row, 1];
                var planName = planNameValue2[row, 1];
                var SBRTstring = SBRTvalue2[row, 1];
                var CTstudyString = CTstudyValue2[row, 1];
                var CTseriesString = CTseriesValue2[row, 1];
                var CTnameString = CTnameValue2[row, 1];
                var preTxPERFString = preTxPERFValue2[row, 1];
                var preTxVENTString = preTxVENTValue2[row, 1];
                var midTxPERFString = midTxPERFValue2[row, 1];
                var midTxVENTString = midTxVENTValue2[row, 1];
                    if (patientID != null && patientCourse != null)
                {
                        patientIDs.Add(patientID.ToString());
                        patientCourses.Add(patientCourse.ToString());
                        planTypes.Add(planType.ToString());
                        planNames.Add(planName.ToString());
                        SBRT.Add(SBRTstring.ToString());
                        CTstudy.Add(CTstudyString.ToString());
                        CTseries.Add(CTseriesString.ToString());
                        CTname.Add(CTnameString.ToString());
                        preTxPERF.Add(preTxPERFString.ToString());
                        preTxVENT.Add(preTxVENTString.ToString());
                        midTxPERF.Add(midTxPERFString.ToString());
                        midTxVENT.Add(midTxVENTString.ToString());
                    }
                }
            bioCorrection = groupBox2.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked).Text;
                if (checkBox1.Checked)
                {
                    perfVar = checkBox1.Text.Substring(0,4).ToUpper();
                }
                if (checkBox2.Checked)
                {
                    ventVar = checkBox2.Text.Substring(0, 4).ToUpper();
                }
                formResult = DialogResult.OK;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false && checkBox2.Checked == false)
            {
                button4.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false && checkBox2.Checked == false)
            {
                button4.Enabled = false;
            }
            if (checkBox1.Checked == true || checkBox2.Checked == true)
            {
                button4.Enabled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var dataFileDialog = new OpenFileDialog();
            DialogResult result = dataFileDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dataFileDialog.ToString()))
            {
                patientDataFile = dataFileDialog.FileName;
                label12.Text = patientDataFile;
                button4.Enabled = true;
                this.AcceptButton = button4;
            }
            else
            {
                label12.Text = null;
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            formResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MetricOptions openMetricOptions = new MetricOptions();
            openMetricOptions.ShowDialog();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            doseThreshold = Convert.ToDouble(textBox2.Text);
        }

        /*
        private static IEnumerable<object> GetNonNullValuesInColumn(Excel.Application excelApp, Excel.Worksheet excelWork, string patientDataFile)
        {
            excelWork = excelApp.Workbooks.Open(patientDataFile);
            excelSheets = excelWork.Worksheets;
            patientData = excelSheets.Item["Patient Data"];
            var tempRange = excelApp.Intersect(patientData.UsedRange, (Excel.Range)patientData.Columns["E"]);
            var value = tempRange.Value2;
            var value2 = (object[,])value;
            var rowCount = value2.GetLength(0);
            for (int row = 0; row < rowCount; row++)
            {
                var v = value2[row, 1];
                if (v != null)
                {
                    yield return v;
                }
            }
        }
        */
    }
}
