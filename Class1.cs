using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using DFHAnalysis;
using Excel = Microsoft.Office.Interop.Excel;

namespace VMS.TPS
{
    public class Script
    {
        private static User v_CurrentUser = null;
        public static User CurrentUser
        {
            get { return v_CurrentUser; }
            set { v_CurrentUser = value; }
        }
        private static Patient v_CurrentPatient = null;
        public static Patient CurrentPatient
        {
            get { return v_CurrentPatient; }
            set { v_CurrentPatient = value; }
        }
        private static Course v_CurrentCourse = null;
        public static Course CurrentCourse
        {
            get { return v_CurrentCourse; }
            set { v_CurrentCourse = value; }
        }

        public void Execute(ScriptContext scriptContext)
        {
            try
            {
                // Obtain User
                CurrentUser = scriptContext.CurrentUser;

                // Obtain Patient
                CurrentPatient = scriptContext.Patient;

                // Obtain Course
                CurrentCourse = scriptContext.Course;

                // Complete User Interface
                Batch newBatch = new Batch();
                if (newBatch.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < Batch.patientIDs.Count; i++)
                    {
                        
                    }
                }

            }
            catch(Exception e)
            {
                MessageBox.Show("An error occurred: " + e.ToString());
            }
        }


    }
}
