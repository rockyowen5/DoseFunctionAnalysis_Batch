using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using DFHAnalysis;
using System.Diagnostics;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class LoadAllImages
    {
        public static List<string> perfTimepoints = new List<string>();
        public static List<string> ventTimepoints = new List<string>();

        public void LoadAll(Patient currentPatient, string seriesName, string loadPlan, Course loadCourse, string planType, string bioCorrection, string SBRT,string[] CTstring, string SPECTstudy)
        {
            Study studyGrab = currentPatient.Studies.First(s => s.Id == SPECTstudy);
            Series seriesGrab = studyGrab.Series.First(s => s.Id == seriesName);
            int start = seriesGrab.Id.Length - 4;
            string compVar = seriesGrab.Id.Substring(start, 4);
            VMS.TPS.Common.Model.API.Image imageGrab = seriesGrab.Images.First(j => j.Id.ToUpper().Contains(compVar));

            string CTstudyID = CTstring[0];
            Study CTstudy = currentPatient.Studies.First(j => j.Id.Equals(CTstudyID));
            string CTseriesID = CTstring[1];
            Series CTseries = CTstudy.Series.First(j => j.Id.Equals(CTseriesID));
            string CTimageID = CTstring[2];
            Image CTimage = CTseries.Images.First(j => j.Id.Equals(CTimageID));
            Registration registrationGrab;
            try
            {
                registrationGrab = currentPatient.Registrations.First(r => r.Id == seriesGrab.Id.ToUpper() + " NEW");
            }
            catch
            {
                registrationGrab = currentPatient.Registrations.First(r => r.Id == seriesGrab.Id.ToUpper());
            }

            DataProcess newProcess = new DataProcess();
            Stopwatch metricTime = new Stopwatch();
            metricTime.Start();
            try
            {
                if (LoadDosePlan.patientDose != null)
                {
                    newProcess.ProcessData(LoadDosePlan.patientDose, imageGrab, registrationGrab, CTimage);
                    Program.printCT = false;
                }
                else
                {
                    MessageBox.Show("Patient dose is null.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }
            metricTime.Stop();
            double elapsedTime = metricTime.Elapsed.TotalSeconds;

            if (compVar == "PERF")
            {
                perfTimepoints.Add(seriesGrab.ToString());
            }
            else
            {
                ventTimepoints.Add(seriesGrab.ToString());
            }
            CsvWrite csvWriter = new CsvWrite();
            csvWriter.writeCSV(seriesGrab.Id, loadPlan, SBRT);
            MetricAnalysis.dataStructure.Clear();



        }
    }
}
