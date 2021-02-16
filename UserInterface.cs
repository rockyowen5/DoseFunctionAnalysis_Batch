using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Excel = Microsoft.Office.Interop.Excel;

namespace DFHAnalysis
{
    public partial class UserInterface : Form
    {
        private static ListView v_ImageDoseList = null;
        public static ListView imageDoseList
        {
            get { return v_ImageDoseList; }
            set { v_ImageDoseList = value; }
        }
        private static ListBox v_ListBox1 = null;
        public static ListBox ListBox1
        {
            get { return v_ListBox1; }
            set { v_ListBox1 = value; }
        }
        private static ListViewItem v_SelectedMetric = null;
        public static ListViewItem SelectedMetric
        {
            get { return v_SelectedMetric; }
            set { v_SelectedMetric = value; }
        }
        private static ListViewGroup v_MetricGroup = null;
        public static ListViewGroup MetricGroup
        {
            get { return v_MetricGroup; }
            set { v_MetricGroup = value; }
        }
        private static ListView v_ListView1 = null;
        public static ListView ListView1
        {
            get { return v_ListView1; }
            set { v_ListView1 = value; }
        }
        private static string[] v_Metrics = null;
        public static string[] Metrics
        {
            get { return v_Metrics; }
            set { v_Metrics = value; }
        }
        private static List<string> v_SelectedStructures = null;
        public static List<string> SelectedStructures
        {
            get { return v_SelectedStructures; }
            set { v_SelectedStructures = value; }
        }
        private static int v_LoadedPlans = 0;
        public static int LoadedPlans
        {
            get { return v_LoadedPlans; }
            set { v_LoadedPlans = value; }
        }
        private static Stopwatch v_RunTime = null;
        public static Stopwatch RunTime
        {
            get { return v_RunTime; }
            set { v_RunTime = value; }
        }
        private static Dictionary<string, double> v_LoadPlanTimes = null;
        public static Dictionary<string, double> LoadPlanTimes
        {
            get { return v_LoadPlanTimes; }
            set { v_LoadPlanTimes = value; }
        }
        private static Dictionary<string, double> v_AlphaBetaDictionary = new Dictionary<string, double>();
        public static Dictionary<string, double> AlphaBetaDictionary
        {
            get { return v_AlphaBetaDictionary; }
            set { v_AlphaBetaDictionary = value; }
        }
        private static string v_SelectedStructureName = null;
        public static string SelectedStructureName
        {
            get { return v_SelectedStructureName; }
            set { v_SelectedStructureName = value; }
        }
        private static Dictionary<string, double> v_AValueDictionary = new Dictionary<string, double>();
        public static Dictionary<string, double> AValueDictionary
        {
            get { return v_AValueDictionary; }
            set { v_AValueDictionary = value; }
        }
        private static Label v_IntersectWarning = null;
        public static Label IntersectWarning
        {
            get { return v_IntersectWarning; }
            set { v_IntersectWarning = value; }
        }


        private Stopwatch loadPlanRunTime;
        private ListViewItem structureItem;
        private int metricIndex;
        private string structureIdentifier;
        private string metricName;
        public static ListViewItem selectedDoseImageFile;
        public static ImageList loadStateImages;
        private bool fileError = false;
        //private string cm3 = " cm\xB3";
        //private string gy = " Gy";


        public UserInterface()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void UserInterface_Load(object sender, EventArgs e)
        {
            // Display Patient/User Information.
            string thisUser = Script.CurrentUser.Id;
            string patientID = Script.CurrentPatient.Id;
            label3.Font = new System.Drawing.Font(label3.Font, FontStyle.Bold);
            label3.Text = "Patient ID: " + patientID;
            label4.Font = new System.Drawing.Font(label4.Font, FontStyle.Bold);
            label4.Text = "User: " + thisUser;

            // User interface assignments and styling.
            imageDoseList = listView4;
            imageDoseList.Columns[0].Width = Convert.ToInt32(Math.Floor(0.125 * imageDoseList.Width));
            imageDoseList.Columns[1].Width = Convert.ToInt32(Math.Floor(0.125 * imageDoseList.Width));
            imageDoseList.Columns[2].Width = Convert.ToInt32(Math.Floor(0.125 * imageDoseList.Width));
            imageDoseList.Columns[3].Width = Convert.ToInt32(Math.Floor(0.125 * imageDoseList.Width));
            imageDoseList.Columns[4].Width = Convert.ToInt32(Math.Floor(0.25 * imageDoseList.Width));
            imageDoseList.Columns[5].Width = Convert.ToInt32(Math.Floor(0.15 * imageDoseList.Width));
            imageDoseList.Columns[6].Width = Convert.ToInt32(Math.Floor(0.10 * imageDoseList.Width));
            ListBox1 = listBox1;
            button3.Enabled = false;
            button4.Enabled = false;
            ListView1 = listView1;
            SelectedStructures = new List<string>();
            LoadedPlans = 0;
            LoadPlanTimes = new Dictionary<string, double>();
            listView2.Columns[0].Width = 123;
            listView2.Columns[1].Width = 75;
            listView2.Columns[2].Width = 118;
            listView2.Columns[3].Width = 138;
            System.Drawing.Image greenCheck = System.Drawing.Image.
                FromFile(@"\\ROFILE\EclipseScripts\Aria13\Development\DFHAnalysis\DFH Analysis-1.1.0.0\Resources\ListView Images\Success.png");
            System.Drawing.Image redError = System.Drawing.Image.
                FromFile(@"\\ROFILE\EclipseScripts\Aria13\Development\DFHAnalysis\DFH Analysis-1.1.0.0\Resources\ListView Images\error.png");
            loadStateImages = new ImageList();
            loadStateImages.Images.Add("Success", greenCheck);
            loadStateImages.Images.Add("Error", redError);
            listView4.SmallImageList = loadStateImages;
            IntersectWarning = label11;

            // Event handler creations.
            this.listBox1.MouseDoubleClick += new MouseEventHandler(listBox1_MouseDoubleClick);
            this.listView2.MouseUp += new MouseEventHandler(listView2_MouseUp);
            this.listView2.MouseDoubleClick += new MouseEventHandler(listView2_MouseDoubleClick);
            this.listView4.MouseDoubleClick += new MouseEventHandler(listView4_MouseDoubleClick);
            this.listView4.MouseUp += new MouseEventHandler(listView4_MouseUp);

            // Metric checkedListBox Items.
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
            this.checkedListBox1.Items.Add("fV20[%]");
            this.checkedListBox1.Items.Add("MfLD[Gy]");

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView4.Items.Count; i++)
            {
                if (listView4.Items[i].ImageIndex == loadStateImages.Images.IndexOfKey("Error"))
                {
                    fileError = true;
                }
            }
            if (fileError == true)
            {
                MessageBox.Show("Please complete all selections for the previous file before loading a new plan." + Environment.NewLine 
                    + "(Use 'Edit' button or double-click file item to update/add selections.)");
            }
            else
            {
            loadPlanRunTime = new Stopwatch();

            // Selects SPECT Image.
            SelectSPECTStudy selectSPECTStudy = new SelectSPECTStudy();
            selectSPECTStudy.ShowDialog();
            SelectSPECTSeries selectSPECTSeries = new SelectSPECTSeries();
            if (selectSPECTStudy.DialogResult == DialogResult.OK)
                selectSPECTSeries.ShowDialog();
            SelectSPECTImage selectSPECTImage = new SelectSPECTImage();
            if (selectSPECTSeries.DialogResult == DialogResult.OK)
                selectSPECTImage.ShowDialog();
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            // Selects Dose Profile.
            SelectDoseCourse selectCourse = new SelectDoseCourse();
            if (selectSPECTImage.DialogResult == DialogResult.OK)
                selectCourse.ShowDialog();
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            SelectDosePlan selectPlan = new SelectDosePlan();
            if (selectCourse.DialogResult == DialogResult.OK)
                selectPlan.ShowDialog();
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            SelectRegistration register = new SelectRegistration();
            if (selectPlan.DialogResult == DialogResult.OK)
                register.ShowDialog();
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            SelectBioCorrection selectBioCorrection = new SelectBioCorrection();
            if (register.DialogResult == DialogResult.OK)
            {
                selectBioCorrection.ShowDialog();
            }
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            if (selectBioCorrection.DialogResult == DialogResult.OK)
            {
                loadPlanRunTime.Start();
                LoadDosePlan loadNewFile = new LoadDosePlan();
                if (SelectDosePlan.SelectedPlanType == "ExternalBeam")
                {
                    loadNewFile.SetPlanSetup(SelectDoseCourse.PatientCourse, SelectDosePlan.SelectedPlan, SelectSPECTImage.SPECTDictionary[LoadedPlans],
                        SelectRegistration.PatientRegistration, SelectBioCorrection.BioCorrectDictionary[LoadedPlans], LoadedPlans);
                    SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Success");
                }
                else if (SelectDosePlan.SelectedPlanType == "PlanSum")
                {
                    loadNewFile.SetPlanSum(SelectDoseCourse.PatientCourse, SelectDosePlan.SelectedPlan, SelectSPECTImage.SPECTDictionary[LoadedPlans],
                        SelectRegistration.PatientRegistration, SelectBioCorrection.BioCorrectDictionary[LoadedPlans], LoadedPlans);
                    SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Success");
                }
                loadPlanRunTime.Stop();
                double elapsedRunTime = Math.Round(Convert.ToDouble(loadPlanRunTime.Elapsed.TotalSeconds), 1);
                string name = LoadDosePlan.NameDictionary[LoadedPlans - 1];
                LoadPlanTimes.Add(name, elapsedRunTime);
            }
            else
                SelectSPECTStudy.ImageDoseItem.ImageIndex = loadStateImages.Images.IndexOfKey("Error");
            button3.Enabled = true;
            button4.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_MouseDoubleClick (object sender, MouseEventArgs e)
        {
            // Add structures to list of interest by double click event.
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches && listView2.Items.ContainsKey(listBox1.Items[index].ToString()) == false)
            {
                if (button10.Enabled == false)
                {
                    button10.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                    button5.Enabled = true;
                    button11.Enabled = true;
                }
                structureIdentifier = ListBox1.Items[index].ToString();
                SelectedStructures.Add(structureIdentifier);

                double alphaBetaValue;
                if (structureIdentifier == "PTV_COMPOSITE" || structureIdentifier == "GTV_COMPOSITE")
                {
                    alphaBetaValue = 10;
                }
                else
                {
                    alphaBetaValue = 2.5;
                }

                // Add selected structure and its volume to main tab.
                structureItem = listView2.Items.Add(structureIdentifier);
                structureItem.Name = structureIdentifier;
                structureItem.SubItems.Add("");
                structureItem.SubItems.Add("");
                structureItem.SubItems.Add("");
                string name = LoadDosePlan.NameDictionary[0];
                structureItem.SubItems[1].Text = LoadDosePlan.StructureSets[name].Structures.First(s => s.Id == structureIdentifier).DicomType;
                structureItem.SubItems[2].Text = "\u03B1/\u03B2 = " + alphaBetaValue.ToString();
                AlphaBetaDictionary.Add(structureIdentifier, alphaBetaValue);
                double aValue = 1;
                structureItem.SubItems[3].Text = "a = " + aValue.ToString();
                AValueDictionary.Add(structureIdentifier, aValue);

                // Add selected structure as group header in "Metrics" tab.
                MetricGroup = listView1.Groups.Add(structureIdentifier, structureIdentifier);
            }
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem selectedStructureItem = this.listView2.GetItemAt(e.X, e.Y);
            SelectedStructureName = selectedStructureItem.Text;
            StructureProperties viewStructureProperties = new StructureProperties();
            viewStructureProperties.ShowDialog();
            selectedStructureItem.SubItems[2].Text = "\u03B1/\u03B2 = " + AlphaBetaDictionary[SelectedStructureName].ToString();
            selectedStructureItem.SubItems[3].Text = "a = " + AValueDictionary[SelectedStructureName].ToString();
        }

        private void listView4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            selectedDoseImageFile = this.listView4.GetItemAt(e.X, e.Y);
            DoseImageEdit editFile = new DoseImageEdit();
            editFile.ShowDialog();
        }

        private void listView2_MouseUp(object sender, MouseEventArgs e)
        {
            // Get the item on the row that is clicked.
            SelectedMetric = this.listView2.GetItemAt(e.X, e.Y);
            if (SelectedMetric != null)
            {
                metricIndex = SelectedMetric.Index;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Add all structures to list of interest.
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                structureIdentifier = ListBox1.Items[i].ToString();
                if (listView2.Items.ContainsKey(structureIdentifier) == false)
                {
                    SelectedStructures.Add(structureIdentifier);
                    double alphaBetaValue;
                    if (structureIdentifier == "PTV_COMPOSITE" || structureIdentifier == "GTV_COMPOSITE")
                    {
                        alphaBetaValue = 10;
                    }
                    else
                    {
                        alphaBetaValue = 2.5;
                    }

                    // Add selected structure and its volume to main tab.
                    structureItem = listView2.Items.Add(structureIdentifier);
                    structureItem.Name = structureIdentifier;
                    structureItem.SubItems.Add("");
                    structureItem.SubItems.Add("");
                    structureItem.SubItems.Add("");
                    string name = LoadDosePlan.NameDictionary[0];
                    structureItem.SubItems[1].Text = LoadDosePlan.StructureSets[name].Structures.First(s => s.Id == structureIdentifier).DicomType;
                    structureItem.SubItems[2].Text = "\u03B1/\u03B2 = " + alphaBetaValue.ToString();
                    AlphaBetaDictionary.Add(structureIdentifier, alphaBetaValue);
                    double aValue = 1;
                    structureItem.SubItems[3].Text = "a = " + aValue.ToString();
                    AValueDictionary.Add(structureIdentifier, aValue);

                    // Add selected structure as group header in "Metrics" tab.
                    MetricGroup = listView1.Groups.Add(structureIdentifier, structureIdentifier);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Clear all structures from list of interest.
            for (int j = 1; j < ListView1.Columns.Count; j++)
            {
                ListView1.Columns.Remove(ListView1.Columns[j]);
            }
            listView2.Items.Clear();
            ListView1.Items.Clear();
            SelectedStructures.Clear();
            AlphaBetaDictionary.Clear();
            AValueDictionary.Clear();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string removeStructureName = listView2.Items[metricIndex].Text;
            AlphaBetaDictionary.Remove(removeStructureName);
            listView2.Items[metricIndex].Remove();
            SelectedStructures.Remove(removeStructureName);
            AValueDictionary.Remove(removeStructureName);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (MetricAnalysis.PlanCumulativeCounts == null)
            {
                Normalizer.AverageUnderDoseThreshold.Clear();
                Normalizer.StructureNormalizationValue.Clear();
                RunTime = new Stopwatch();
                RunTime.Start();

                for (int i = 0; i < LoadedPlans; i++)
                {
                    string name = LoadDosePlan.NameDictionary[i];
                    if (ListView1.Columns.ContainsKey(name) == false)
                    {
                        ColumnHeader currentColumn = ListView1.Columns.Add(name);
                        currentColumn.Name = name;
                        currentColumn.Width = 180;
                    }
                }

                metricName = "";
                Metrics = new string[checkedListBox1.CheckedItems.Count];
                for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
                {
                    metricName = checkedListBox1.CheckedItems[i].ToString();
                    Metrics[i] = metricName;
                }

                MetricAnalysis getGraphData = new MetricAnalysis();
                getGraphData.Analyze();
                tabControl1.SelectedTab = tabControl1.TabPages[1];

                DFHGraph viewGraph = new DFHGraph();
                viewGraph.ShowDialog();
            }
            else if (!MetricAnalysis.PlanCumulativeCounts.ContainsKey(LoadDosePlan.NameDictionary[0]) || MetricAnalysis.PlanCumulativeCounts[LoadDosePlan.NameDictionary[0]].GetLength(0) * MetricAnalysis.PlanCumulativeCounts.Count != (LoadedPlans * SelectedStructures.Count))
            {
                Normalizer.AverageUnderDoseThreshold.Clear();
                Normalizer.StructureNormalizationValue.Clear();
                RunTime = new Stopwatch();
                RunTime.Start();

                for (int i = 0; i < LoadedPlans; i++)
                {
                    string name = LoadDosePlan.NameDictionary[i];
                    if (ListView1.Columns.ContainsKey(name) == false)
                    {
                        ColumnHeader currentColumn = ListView1.Columns.Add(name);
                        currentColumn.Name = name;
                        currentColumn.Width = 180;
                    }
                }

                listView1.Items.Clear();
                metricName = "";
                Metrics = new string[checkedListBox1.CheckedItems.Count];
                for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
                {
                    metricName = checkedListBox1.CheckedItems[i].ToString();
                    Metrics[i] = metricName;
                }

                MetricAnalysis getGraphData = new MetricAnalysis();
                getGraphData.Analyze();
                tabControl1.SelectedTab = tabControl1.TabPages[1];

                DFHGraph viewGraph = new DFHGraph();
                viewGraph.ShowDialog();
            }
            else
            {
                DFHGraph viewGraph = new DFHGraph();
                viewGraph.ShowDialog();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            ExcelWrite viewExcel = new ExcelWrite();
            Thread excelThread = new Thread(new ThreadStart(() =>
            {
                viewExcel.ShowExcel();
            }));
            excelThread.Start();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Normalizer.AverageUnderDoseThreshold.Clear();
            Normalizer.StructureNormalizationValue.Clear();
            RunTime = new Stopwatch();
            RunTime.Start();
            for (int i = 0; i < LoadedPlans; i++)
            {
                string name = LoadDosePlan.NameDictionary[i];
                if (ListView1.Columns.ContainsKey(name) == false)
                {
                    ColumnHeader currentColumn = ListView1.Columns.Add(name);
                    currentColumn.Name = name;
                    currentColumn.Width = 180;
                }
            }

            listView1.Items.Clear();
            metricName = "";
            Metrics = new string[checkedListBox1.CheckedItems.Count];
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                metricName = checkedListBox1.CheckedItems[i].ToString();
                Metrics[i] = metricName;
            }

            MetricAnalysis callMetrics = new MetricAnalysis();
            callMetrics.Analyze();
            tabControl1.SelectedTab = tabControl1.TabPages[1];
        }

        private void listView4_MouseUp(object sender, MouseEventArgs e)
        {
            selectedDoseImageFile = this.listView4.GetItemAt(e.X, e.Y);
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            DoseImageEdit editFile = new DoseImageEdit();
            editFile.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button10.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button5.Enabled = false;
            button11.Enabled = false;

            LoadedPlans = 0;
            for (int j = ListView1.Columns.Count - 1; j > 0 ; j--)
            {
                ListView1.Columns.Remove(ListView1.Columns[j]);
            }
            ListBox1.Items.Clear();
            ListView1.Items.Clear();
            listView2.Items.Clear();
            imageDoseList.Items.Clear();
            SelectedStructures.Clear();
            LoadDosePlan.NameDictionary.Clear();
            LoadDosePlan.StructureSets.Clear();
            LoadDosePlan.DosePlans.Clear();
            LoadDosePlan.FractionDictionary.Clear();
            LoadDosePlan.GraphNameDictionary.Clear();
            LoadDosePlan.RegistrationDictionary.Clear();
            LoadDosePlan.MaximumDose.Clear();
            SelectSPECTImage.SPECTDictionary.Clear();
            SelectSPECTImage.ImageNameDictionary.Clear();
            SelectDosePlan.DoseNameDictionary.Clear();
            SelectBioCorrection.BioCorrectDictionary.Clear();
            if (MetricAnalysis.PlanCumulativeCounts != null)
            {
                MetricAnalysis.PlanCumulativeCounts.Clear();
                MetricAnalysis.PlanCumulativeCounts = null;
            }
            AlphaBetaDictionary.Clear();
            AValueDictionary.Clear();
            LoadPlanTimes.Clear();
            tabControl1.SelectedTab = tabControl1.TabPages[0];
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SelectOptions selectOptions = new SelectOptions();
            selectOptions.ShowDialog();

            if (SelectOptions.RecalculationBool == "Yes")
            {
                if (MetricAnalysis.PlanCumulativeCounts != null)
                {
                    Normalizer.AverageUnderDoseThreshold.Clear();
                    Normalizer.StructureNormalizationValue.Clear();
                    Recalculate recalculateResults = new Recalculate();
                    recalculateResults.ShowDialog();
                    /*
                    Thread recalculateThread = new Thread(new ThreadStart(() =>
                    {
                        recalculateResults.ShowDialog();
                    }));
                    recalculateThread.Start();
                    MessageBox.Show("Beginning recalculation...");
                    */
                    if (recalculateResults.DialogResult == DialogResult.OK)
                    {
                        RunTime = new Stopwatch();
                        RunTime.Start();
                        for (int i = 0; i < LoadedPlans; i++)
                        {
                            string name = LoadDosePlan.NameDictionary[i];
                            if (ListView1.Columns.ContainsKey(name) == false)
                            {
                                ColumnHeader currentColumn = ListView1.Columns.Add(name);
                                currentColumn.Name = name;
                                currentColumn.Width = 180;
                            }
                        }

                        listView1.Items.Clear();
                        metricName = "";
                        Metrics = new string[checkedListBox1.CheckedItems.Count];
                        for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
                        {
                            metricName = checkedListBox1.CheckedItems[i].ToString();
                            Metrics[i] = metricName;
                        }

                        MetricAnalysis callMetrics = new MetricAnalysis();
                        callMetrics.Analyze();
                        tabControl1.SelectedTab = tabControl1.TabPages[1];
                    }
                }
            }


        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            ListViewItem selectedStructureItem = this.listView2.Items[metricIndex];
            SelectedStructureName = selectedStructureItem.Text;
            StructureProperties viewStructureProperties = new StructureProperties();
            viewStructureProperties.ShowDialog();
            selectedStructureItem.SubItems[2].Text = "\u03B1/\u03B2 = " + AlphaBetaDictionary[SelectedStructureName].ToString();
            selectedStructureItem.SubItems[3].Text = "a = " + AValueDictionary[SelectedStructureName].ToString();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            LoadAllSolution loadAll = new LoadAllSolution();
            loadAll.ShowDialog();
        }
    }

   
}
