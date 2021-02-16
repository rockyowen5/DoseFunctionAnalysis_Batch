using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFH_Initiate
{
    class PRMAnalysis
    {
        public void Analyze(string SPECTstudy, string PRMstudy, Patient currentPatient, string seriesName, string loadPlan, Course loadCourse, string planType, string bioCorrection, string SBRT)
        {
            Study studyGrab = currentPatient.Studies.First(s => s.Id == SPECTstudy);
            Series seriesGrab = studyGrab.Series.First(s => s.Id == seriesName);
            int start = seriesGrab.Id.Length - 4;
            string compVar = seriesGrab.Id.Substring(start, 4);
            VMS.TPS.Common.Model.API.Image imageGrab = seriesGrab.Images.First(j => j.Id.ToUpper().Contains(compVar));

            Registration registrationGrab;
            // string registrationName = "PERF TO PRM"; for SPECT reference frame
            string registrationName = "PRM TO VENT";
            registrationGrab = currentPatient.Registrations.First(r => r.Id == registrationName.ToUpper());

            Registration planRegistrationGrab;
            //string planRegistrationName = "PERF TO CT"; for SPECT reference frame
            string planRegistrationName = "PRM TO CT";
            planRegistrationGrab = currentPatient.Registrations.First(r => r.Id == planRegistrationName);

            Registration PRMregistrationGrab;
            //string perfRegistrationName = "INITIAL PERF"; for SPECT reference frame
            string ctRegistrationName = "CT TO PRM";
            PRMregistrationGrab = currentPatient.Registrations.First(r => r.Id == ctRegistrationName);

            string PRMseries = "PRM";
            Study PRMstudyGrab = currentPatient.Studies.First(s => s.Id == PRMstudy);
            Series PRMseriesGrab = PRMstudyGrab.Series.First(s => s.Id == PRMseries);
            VMS.TPS.Common.Model.API.Image PRMimageGrab = PRMseriesGrab.Images.First(j => j.Id == PRMseries);

            if (LoadDosePlan.patientDose != null)
            {
                PRMprocessII newPRMprocessII = new PRMprocessII();
                newPRMprocessII.PRMdataProcessII(LoadDosePlan.patientDose, imageGrab, PRMimageGrab, registrationGrab, planRegistrationGrab, PRMregistrationGrab);
            }
            else
            {
                MessageBox.Show("Patient dose is null.");
            }
        }
    }
}
