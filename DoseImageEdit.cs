using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFHAnalysis
{
    public partial class DoseImageEdit : Form
    {
        public DoseImageEdit()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public ListViewItem selectedFile = UserInterface.selectedDoseImageFile;

        private void DoseImageEdit_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectionChangeCommitted += new EventHandler(comboBox1_SelectionChange);
            this.comboBox2.SelectionChangeCommitted += new EventHandler(comboBox2_SelectionChange);
            this.comboBox4.SelectionChangeCommitted += new EventHandler(comboBox4_SelectionChange);

            Study selectedStudy = Script.CurrentPatient.Studies.First(s => s.Id == selectedFile.SubItems[0].Text);
            IEnumerator<Study> patientStudies = Script.CurrentPatient.Studies.GetEnumerator();
            while (patientStudies.MoveNext())
            {
                comboBox1.Items.Add(patientStudies.Current.Id);
            }
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(selectedStudy.Id);

            Series selectedSeries = selectedStudy.Series.First(s => s.Id == selectedFile.SubItems[1].Text);
            IEnumerator<Series> patientSeries = selectedStudy.Series.GetEnumerator();
            while (patientSeries.MoveNext())
            {
                comboBox2.Items.Add(patientSeries.Current.Id);
            }
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(selectedSeries.Id);

            Image selectedImage = selectedSeries.Images.First(i => i.Id == selectedFile.SubItems[2].Text);
            IEnumerator<Image> patientImages = selectedSeries.Images.GetEnumerator();
            while (patientImages.MoveNext())
            {
                comboBox3.Items.Add(patientImages.Current.Id);
            }
            comboBox3.SelectedIndex = comboBox3.Items.IndexOf(selectedImage.Id);

            Course selectedCourse = Script.CurrentPatient.Courses.First(c => c.Id == selectedFile.SubItems[3].Text);
            IEnumerator<Course> patientCourses = Script.CurrentPatient.Courses.GetEnumerator();
            while (patientCourses.MoveNext())
            {
                comboBox4.Items.Add(patientCourses.Current.Id);
            }
            comboBox4.SelectedIndex = comboBox4.Items.IndexOf(selectedCourse.Id);

            List<string> patientPlansList = new List<string>();
            IEnumerator<PlanSetup> patientPlanSetups = selectedCourse.PlanSetups.GetEnumerator();
            while (patientPlanSetups.MoveNext())
            {
                patientPlansList.Add(patientPlanSetups.Current.PlanType + ": " + patientPlanSetups.Current.Id);
            }
            IEnumerator<PlanSum> patientPlanSums = selectedCourse.PlanSums.GetEnumerator();
            while (patientPlanSums.MoveNext())
            {
                patientPlansList.Add("PlanSum: " + patientPlanSums.Current.Id);
            }
            for (int i = 0; i < patientPlansList.Count; i++)
            {
                comboBox5.Items.Add(patientPlansList[i]);
            }
            if (selectedFile.SubItems[4].Text != "")
            {
                string selectedPlan = selectedFile.SubItems[4].Text;
                comboBox5.SelectedIndex = comboBox5.Items.IndexOf(selectedPlan);
            }

            IEnumerator<Registration> patientRegistrations = Script.CurrentPatient.Registrations.GetEnumerator();
            while (patientRegistrations.MoveNext())
            {
                comboBox6.Items.Add(patientRegistrations.Current.Id);
            }
            if (selectedFile.SubItems[5].Text != "")
            {
                Registration selectedRegistration = Script.CurrentPatient.Registrations.First(r => r.Id == selectedFile.SubItems[5].Text);
                comboBox6.SelectedIndex = comboBox6.Items.IndexOf(selectedRegistration.Id);
            }
            else
            {
                Registration selectedRegistration = Script.CurrentPatient.Registrations.First(r => r.Id == selectedFile.SubItems[1].Text.ToUpper());
                comboBox6.SelectedIndex = comboBox6.Items.IndexOf(selectedRegistration.Id);
            }

            string selectedBioCorrect = selectedFile.SubItems[6].Text;
            comboBox7.Items.Add("Yes");
            comboBox7.Items.Add("No");
            comboBox7.SelectedIndex = comboBox7.Items.IndexOf(selectedBioCorrect);

            this.AcceptButton = button1;
        }

        private void comboBox1_SelectionChange(object sender, EventArgs e)
        {
            Study selectedStudy = Script.CurrentPatient.Studies.First(s => s.Id == comboBox1.Items[comboBox1.SelectedIndex].ToString());

            comboBox2.SelectedIndex = -1;
            comboBox2.Items.Clear();
            comboBox3.SelectedIndex = -1;
            comboBox3.Items.Clear();
            IEnumerator<Series> patientSeries = selectedStudy.Series.GetEnumerator();
            while (patientSeries.MoveNext())
            {
                comboBox2.Items.Add(patientSeries.Current.Id);
                if (patientSeries.Current.Id == selectedStudy.Id)
                {
                    comboBox2.SelectedIndex = comboBox2.Items.IndexOf(selectedStudy.Id);
                    Series selectedSeries = selectedStudy.Series.First(s => s.Id == comboBox2.Items[comboBox2.SelectedIndex].ToString());
                    IEnumerator<Image> patientImages = selectedSeries.Images.GetEnumerator();
                    while (patientImages.MoveNext())
                    {
                        comboBox3.Items.Add(patientImages.Current.Id);
                        if (patientImages.Current.Id.EndsWith("PERF"))
                        {
                            comboBox3.SelectedIndex = comboBox3.Items.IndexOf(patientImages.Current.Id);
                        }
                        else if (patientImages.Current.Id.EndsWith("VENT"))
                        {
                            comboBox3.SelectedIndex = comboBox3.Items.IndexOf(patientImages.Current.Id);
                        }
                    }
                }
            }

            comboBox6.SelectedIndex = -1;
            if (comboBox6.Items.Contains(selectedStudy.Id.ToUpper()))
            {
                comboBox6.SelectedIndex = comboBox6.Items.IndexOf(selectedStudy.Id.ToUpper());
            }
        }

        private void comboBox2_SelectionChange(object sender, EventArgs e)
        {
            Study selectedStudy = Script.CurrentPatient.Studies.First(s => s.Id == comboBox1.Items[comboBox1.SelectedIndex].ToString());
            Series selectedSeries = selectedStudy.Series.First(s => s.Id == comboBox2.Items[comboBox2.SelectedIndex].ToString());

            comboBox3.SelectedIndex = -1;
            comboBox3.Items.Clear();
            IEnumerator<Image> patientImages = selectedSeries.Images.GetEnumerator();
            while (patientImages.MoveNext())
            {
                comboBox3.Items.Add(patientImages.Current.Id);
                if (patientImages.Current.Id.EndsWith("PERF"))
                {
                    comboBox3.SelectedIndex = comboBox3.Items.IndexOf(patientImages.Current.Id);
                }
                else if (patientImages.Current.Id.EndsWith("VENT"))
                {
                    comboBox3.SelectedIndex = comboBox3.Items.IndexOf(patientImages.Current.Id);
                }
            }
        }

        private void comboBox4_SelectionChange(object sender, EventArgs e)
        {
            Course selectedCourse = Script.CurrentPatient.Courses.First(c => c.Id == comboBox4.Items[comboBox4.SelectedIndex].ToString());

            comboBox5.SelectedIndex = -1;
            comboBox5.Items.Clear();
            List<string> patientPlansList = new List<string>();
            IEnumerator<PlanSetup> patientPlanSetups = selectedCourse.PlanSetups.GetEnumerator();
            while (patientPlanSetups.MoveNext())
            {
                patientPlansList.Add(patientPlanSetups.Current.PlanType + ": " + patientPlanSetups.Current.Id);
            }
            IEnumerator<PlanSum> patientPlanSums = selectedCourse.PlanSums.GetEnumerator();
            while (patientPlanSums.MoveNext())
            {
                patientPlansList.Add("PlanSum: " + patientPlanSums.Current.Id);
            }
            for (int i = 0; i < patientPlansList.Count; i++)
            {
                comboBox5.Items.Add(patientPlansList[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null || comboBox3.SelectedItem == null || 
                comboBox4.SelectedItem == null || comboBox5.SelectedItem == null || comboBox6.SelectedItem == null || comboBox7.SelectedItem == null)
            {
                MessageBox.Show("Please complete all selections for the file before continuing.");
            }
            else
            {
                Study selectedStudy = Script.CurrentPatient.Studies.First(s => s.Id == comboBox1.Items[comboBox1.SelectedIndex].ToString());
                Series selectedSeries = selectedStudy.Series.First(s => s.Id == comboBox2.Items[comboBox2.SelectedIndex].ToString());
                Image selectedImage = selectedSeries.Images.First(i => i.Id == comboBox3.Items[comboBox3.SelectedIndex].ToString());
                Course selectedCourse = Script.CurrentPatient.Courses.First(c => c.Id == comboBox4.Items[comboBox4.SelectedIndex].ToString());
                string planName = comboBox5.Items[comboBox5.SelectedIndex].ToString();
                string selectedPlan = planName.Substring(planName.IndexOf(":") + 2);
                string selectedPlanType = planName.Substring(0, planName.IndexOf(":"));
                Registration selectedRegistration = Script.CurrentPatient.Registrations.First(r => r.Id == comboBox6.Items[comboBox6.SelectedIndex].ToString());
                string selectedBioCorrect = comboBox7.Items[comboBox7.SelectedIndex].ToString();

                string name = selectedImage.Id + "/" + selectedPlan + "/" + selectedBioCorrect;
                if (LoadDosePlan.StructureSets.ContainsKey(name))
                {
                    MessageBox.Show("This plan has already been loaded. Please revise selection.");
                }
                else
                {
                    Stopwatch loadPlanRunTime = new Stopwatch();
                    loadPlanRunTime.Start();
                    LoadDosePlan editFile = new LoadDosePlan();
                    if (selectedPlanType == "ExternalBeam")
                    {
                        editFile.SetPlanSetup(selectedCourse, selectedPlan, selectedImage, selectedRegistration, selectedBioCorrect, selectedFile.Index);
                    }
                    else if (selectedPlanType == "PlanSum")
                    {
                        editFile.SetPlanSum(selectedCourse, selectedPlan, selectedImage, selectedRegistration, selectedBioCorrect, selectedFile.Index);
                    }
                    loadPlanRunTime.Stop();
                    double elapsedRunTime = Math.Round(Convert.ToDouble(loadPlanRunTime.Elapsed.TotalSeconds), 1);
                    string fileName = LoadDosePlan.NameDictionary[selectedFile.Index];
                    UserInterface.LoadPlanTimes.Add(fileName, elapsedRunTime);

                    selectedFile.ImageIndex = UserInterface.loadStateImages.Images.IndexOfKey("Success");
                    selectedFile.SubItems[0].Text = selectedStudy.Id;
                    selectedFile.SubItems[1].Text = selectedSeries.Id;
                    selectedFile.SubItems[2].Text = selectedImage.Id;
                    selectedFile.SubItems[3].Text = selectedCourse.Id;
                    selectedFile.SubItems[4].Text = selectedPlanType + ": " + selectedPlan;
                    selectedFile.SubItems[5].Text = selectedRegistration.Id;
                    selectedFile.SubItems[6].Text = selectedBioCorrect;
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
