using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
    public partial class LoadAllSolution : Form
    {
        private static ListViewItem v_FileName = null;
        public static ListViewItem FileName
        {
            get { return v_FileName; }
            set { v_FileName = value; }
        }

        private Patient currentPatient;
        private Course loadCourse;
        private string loadPlan;
        private string loadPlanName;
        private string bioCorrectionLoad;
        private static ImageList imageList;
        private List<string> planNames = new List<string>();
        private Stopwatch loadTime;

        public LoadAllSolution()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadPlan = planNames[comboBox1.SelectedIndex];
            loadPlanName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bioCorrectionLoad = groupBox2.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked).Text;
            string compVarPerf = "";
            if (checkBox1.Checked)
            {
                compVarPerf = checkBox1.Text.Substring(0, 4).ToUpper();
            }
            string compVarVent = "";
            if (checkBox2.Checked)
            {
                compVarVent = checkBox2.Text.Substring(0, 4).ToUpper();
            }
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    string initial = "Initial ";
                    string perfName = initial + compVarPerf;
                    string ventName = initial + compVarVent;
                    LoadPlan(perfName, ventName);
                }
                else if (i == 1)
                {
                    string oneMonth = "1Month ";
                    string perfName = oneMonth + compVarPerf;
                    string ventName = oneMonth + compVarVent;
                    LoadPlan(perfName, ventName);
                }
                else if (i == 2)
                {
                    string threeMonth = "3Month ";
                    string perfName = threeMonth + compVarPerf;
                    string ventName = threeMonth + compVarVent;
                    LoadPlan(perfName, ventName);
                }
                else if (i == 3)
                {
                    string oneYear = "1Year ";
                    string perfName = oneYear + compVarPerf;
                    string ventName = oneYear + compVarVent;
                    LoadPlan(perfName, ventName);
                }
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoadAllSolution_Load(object sender, EventArgs e)
        {
            currentPatient = Script.CurrentPatient;
            loadCourse = Script.CurrentCourse;
            IEnumerator<PlanSetup> patientPlanSetups = loadCourse.PlanSetups.GetEnumerator();
            IEnumerator<PlanSum> patientPlanSums = loadCourse.PlanSums.GetEnumerator();
            while (patientPlanSetups.MoveNext())
            {
                string planName = patientPlanSetups.Current.PlanType.ToString() + ": " + patientPlanSetups.Current.Id.ToString();
                comboBox1.Items.Add(planName);
                planNames.Add(patientPlanSetups.Current.Id);
            }
            while (patientPlanSums.MoveNext())
            {
                string planName = "PlanSum: " + patientPlanSums.Current.Id;
                comboBox1.Items.Add(planName);
                planNames.Add(patientPlanSums.Current.Id);
            }
            if (comboBox1.Items.Count == 1)
            {
                comboBox1.SelectedItem = comboBox1.Items[0];
            }
            else if (comboBox1.FindString("PlanSum") != -1)
            {
                comboBox1.SelectedItem = comboBox1.Items[comboBox1.FindString("PlanSum")];
            }
            this.AcceptButton = button1;

            System.Drawing.Image greenCheck = System.Drawing.Image.
                FromFile(@"\\ROFILE\EclipseScripts\Aria13\Development\DFHAnalysis\DFH Analysis-1.1.0.0\Resources\ListView Images\Success.png");
            imageList = new ImageList();
            imageList.Images.Add("Success", greenCheck);
        }

        private void LoadPlan(string perfName,string ventName)
        {
            IEnumerator<Study> studyEnumerate = currentPatient.Studies.GetEnumerator();
            while (studyEnumerate.MoveNext())
            {
                if (studyEnumerate.Current.Id.Equals(perfName) || studyEnumerate.Current.Id.Equals(ventName))
                {
                    LoadDosePlan newLoad = new LoadDosePlan();
                    int start = studyEnumerate.Current.Id.Length - 4;
                    string compVar = studyEnumerate.Current.Id.Substring(start, 4);
                    Study studyGrab = currentPatient.Studies.First(s => s.Id == studyEnumerate.Current.Id);
                    Series seriesGrab = studyGrab.Series.First(s => s.Id == studyEnumerate.Current.Id);
                    VMS.TPS.Common.Model.API.Image imageGrab = seriesGrab.Images.First(j => j.Id.Contains(compVar));
                    Registration registrationGrab = currentPatient.Registrations.First(r => r.Id == studyEnumerate.Current.Id.ToUpper());
                    ListViewItem fileUpload = UserInterface.imageDoseList.Items.Add(studyGrab.Id);
                    for (int j = 0; j < UserInterface.imageDoseList.Columns.Count - 1; j++)
                    {
                        fileUpload.SubItems.Add("");
                    }
                    fileUpload.SubItems[1].Text = seriesGrab.Id;
                    fileUpload.SubItems[2].Text = imageGrab.Id;
                    fileUpload.SubItems[3].Text = loadCourse.Id;
                    fileUpload.SubItems[4].Text = loadPlanName;
                    fileUpload.SubItems[5].Text = registrationGrab.Id;
                    fileUpload.SubItems[6].Text = bioCorrectionLoad;
                    fileUpload.ImageIndex = imageList.Images.IndexOfKey("Success");

                    loadTime = new Stopwatch();
                    loadTime.Start();
                    string[] colonSeparator = loadPlanName.Split(':');
                    if (colonSeparator[0] == "ExternalBeam")
                    {
                        newLoad.SetPlanSetup(loadCourse, loadPlan, imageGrab, registrationGrab, bioCorrectionLoad, UserInterface.LoadedPlans);
                    }
                    else if (colonSeparator[0] == "PlanSum")
                    {
                        newLoad.SetPlanSum(loadCourse, loadPlan, imageGrab, registrationGrab, bioCorrectionLoad, UserInterface.LoadedPlans);
                    }
                    loadTime.Stop();
                    string name = LoadDosePlan.NameDictionary[UserInterface.LoadedPlans - 1];
                    double elapsedTime = Math.Round(Convert.ToDouble(loadTime.Elapsed.TotalSeconds), 1);
                    UserInterface.LoadPlanTimes.Add(name, elapsedTime);
                }
            }
        }
    }
}
