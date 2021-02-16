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
    public partial class SelectDoseCourse : Form
    {
        private static Course v_PatientCourse = null;
        public static Course PatientCourse
        {
            get { return v_PatientCourse; }
            set { v_PatientCourse = value; }
        }

        private string doseCourse;
        private string selectedCourseName;

        public SelectDoseCourse()
        {
            InitializeComponent();
        }

        private void SelectDoseCourse_Load(object sender, EventArgs e)
        {
            // Allow User Access to Course Files
            IEnumerator<Course> patientDoseCourses = Script.CurrentPatient.Courses.GetEnumerator();
            while (patientDoseCourses.MoveNext())
            {
                doseCourse = patientDoseCourses.Current.Id;
                // Automatically selects Eclipse as the dose course. Comment out if automated selection is not desired.
                comboBox1.Items.Add(doseCourse);
                if (Script.CurrentCourse != null)
                {
                    if (doseCourse != null && doseCourse == Script.CurrentCourse.Id)
                    {
                        comboBox1.SelectedIndex = comboBox1.Items.IndexOf(doseCourse);
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCourseName = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            SelectSPECTStudy.ImageDoseItem.SubItems[3].Text = selectedCourseName;
            PatientCourse = Script.CurrentPatient.Courses.First(s => s.Id == selectedCourseName);
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
