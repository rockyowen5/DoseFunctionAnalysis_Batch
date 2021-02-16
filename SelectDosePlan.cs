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
    public partial class SelectDosePlan : Form
    {
        //Global variables.
        private static string v_SelectedPlan = null;
        public static string SelectedPlan
        {
            get { return v_SelectedPlan; }
            set { v_SelectedPlan = value; }
        }
        private static string v_SelectedPlanType = null;
        public static string SelectedPlanType
        {
            get { return v_SelectedPlanType; }
            set { v_SelectedPlanType = value; }
        }
        private static Dictionary<int,string> v_DoseNameDictionary = new Dictionary<int, string>();
        public static Dictionary<int,string> DoseNameDictionary
        {
            get { return v_DoseNameDictionary; }
            set { v_DoseNameDictionary = value; }
        }

        // Local variables.
        private Dictionary<string, string> patientPlansDictionary;
        private Dictionary<string, string> patientPlanTypeDictionary;
        private string[] patientPlansList;
        private string selectedDose;

        public SelectDosePlan()
        {
            InitializeComponent();
        }

        private void SelectDosePlan_Load(object sender, EventArgs e)
        {
            SelectedPlan = null;
            SelectedPlanType = null;
            IEnumerator<PlanSetup> patientDosePlans = SelectDoseCourse.PatientCourse.PlanSetups.GetEnumerator();
            IEnumerator<PlanSum> patientDosePlanSums = SelectDoseCourse.PatientCourse.PlanSums.GetEnumerator();
            patientPlansDictionary = new Dictionary<string, string>();
            patientPlanTypeDictionary = new Dictionary<string, string>();
            int numberOfPlans = 
                SelectDoseCourse.PatientCourse.PlanSetups.Count() + SelectDoseCourse.PatientCourse.PlanSums.Count();
            patientPlansList = new string[numberOfPlans];
            int count = 0;
            while (patientDosePlans.MoveNext())
            {
                patientPlansList[count] = patientDosePlans.Current.PlanType.ToString() + " " +
                    patientDosePlans.Current.ToString();
                patientPlansDictionary.Add(count.ToString(), patientDosePlans.Current.Id.ToString());
                patientPlanTypeDictionary.Add(count.ToString(), patientDosePlans.Current.PlanType.ToString());
                count++;
            }
            while (patientDosePlanSums.MoveNext())
            {
                patientPlansList[count] = "PlanSum: " + patientDosePlanSums.Current.ToString();
                patientPlansDictionary.Add(count.ToString(), patientDosePlanSums.Current.Id.ToString());
                patientPlanTypeDictionary.Add(count.ToString(), "PlanSum");
                count++;
            }

            for (int i = 0; i < numberOfPlans; i++)
                comboBox1.Items.Add(patientPlansList[i]);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedPlanIndex = this.comboBox1.SelectedIndex;
            string selectedPlanString = this.comboBox1.GetItemText(selectedPlanIndex);
            SelectedPlan = patientPlansDictionary[selectedPlanString];
            SelectedPlanType = patientPlanTypeDictionary[selectedPlanString];
            selectedDose = patientPlansList[selectedPlanIndex];
            string doseName = SelectDoseCourse.PatientCourse.Id + "/" + selectedDose;
            DoseNameDictionary.Add(UserInterface.LoadedPlans, doseName);
            SelectSPECTStudy.ImageDoseItem.SubItems[4].Text = SelectedPlanType + ": " + SelectedPlan;
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
