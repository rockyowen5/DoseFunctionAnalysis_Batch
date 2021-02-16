using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DFH_Initiate;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Diagnostics;
using DFHAnalysis;
[assembly: ESAPIScript(IsWriteable = false)]

namespace DFH_Initiate
{
    static class Program
    {
        public static string patientID;
        public static string planName;
        public static int loadProgress;
        public static bool printCT;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            Batch newBatch = new Batch();
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] fileNames = new string[] { "Lungs_Intensity.csv", "Lungs_Function.csv", "Ips_Intensity.csv", "Ips_Function.csv", "Cont_Intensity.csv", "Cont_Function.csv", "Lungs_Intensity_Error.csv", "Lungs_Function_Error.csv", "Ips_Intensity_Error.csv", "Ips_Function_Error.csv", "Cont_Intensity_Error.csv", "Cont_Function_Error.csv", "TestRetestPERF.csv", "TestRetestVENT.csv", "DataVQ.csv" };
            for (int i = 0; i < fileNames.Length; i++)
            {
                string filePath = System.IO.Path.Combine(path, fileNames[i]);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            string filePathII = System.IO.Path.Combine(path, "lookup.csv");
            if (File.Exists(filePathII))
            {
                File.Delete(filePathII);
            }

            double time = 0.0;
            // Initializes new interface for batch script.
            System.Windows.Forms.Application.Run(newBatch);

            if (Batch.formResult == DialogResult.OK)
            {
                /*
                for (int i = 0; i < MetricOptions.aParameter.Count; i++)
                {
                    MessageBox.Show("a = " + MetricOptions.aParameter[i].ToString() + Environment.NewLine + "fV = " + MetricOptions.fVDvalue[i].ToString());
                }
                */
                // compVarPerf and compVarVent are populated by "PERF" and "VENT", respectively, if user desires to analyze both. Else, null for variable.
                string compVarPerf = Batch.perfVar;
                string compVarVent = Batch.ventVar;
                string biocorrection = Batch.bioCorrection;

                Stopwatch batchTime = new Stopwatch();
                batchTime.Start();
                
                using (var app = VMS.TPS.Common.Model.API.Application.CreateApplication())
                {
                    for (int i = 22; i < 23; i++)
                    {
                        BackgroundWorker newWorker = new BackgroundWorker();
                        loadProgress = i + 1;
                        newWorker.Show();
                        PatientLoadStatus getLoadStatus = new PatientLoadStatus();
                        getLoadStatus.ShowDialog();
                        if (PatientLoadStatus.breakBool)
                        {
                            break;
                        }
                        // planType = "ExternalBeam" || "PlanSum"
                        string planType = Batch.planTypes[i];
                        // planName = Patient's dose prescription name, e.g. AAA PLN1
                        planName = Batch.planNames[i];
                        // courseName = "Eclipse"
                        string courseName = Batch.patientCourses[i];
                        string SBRT = Batch.SBRT[i];
                        string CTstudy = Batch.CTstudy[i];
                        string CTseries = Batch.CTseries[i];
                        string CTname = Batch.CTname[i];
                        string[] CTstring = new string[] { CTstudy, CTseries, CTname };
                        string preTxPERF = Batch.preTxPERF[i];
                        string preTxVENT = Batch.preTxVENT[i];
                        string midTxPERF = Batch.midTxPERF[i];
                        string midTxVENT = Batch.midTxVENT[i];
                        // patient is patient identifier, e.g. $2006040_UM020
                        patientID = Batch.patientIDs[i];
                        var patient = app.OpenPatientById(patientID);
                        //MessageBox.Show("Success! Patient " + patient.Id.ToString() + " was loaded.");

                        Course course = patient.Courses.First(c => c.Id.ToUpper() == courseName.ToUpper());
                        LoadDosePlan doseGrab = new LoadDosePlan();
                        // Initialize new LoadDosePlan to obtain dose matrix. Only loaded once per patient. Will be further manipulated in MetricAnalysis.
                        try
                        {
                            Stopwatch loadTime = new Stopwatch();
                            loadTime.Start();
                            if (planType == "ExternalBeam")
                            {
                                doseGrab.SetPlanSetup(course, planName, biocorrection);
                            }
                            else if (planType == "PlanSum")
                            {
                                doseGrab.SetPlanSum(course, planName, biocorrection);
                            }
                            else
                            {
                                MessageBox.Show(planType);
                            }
                            loadTime.Stop();
                            double elapsedTime = Math.Round(Convert.ToDouble(loadTime.Elapsed.TotalSeconds), 1);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Failed at Dose Load");
                            MessageBox.Show(e.Message + e.StackTrace);
                            throw;
                        }
                        try
                        {
                            LoadAllImages newLoad = new LoadAllImages();
                            printCT = true;
                            string seriesName;
                            string notAvailable = "N/A";
                            // newLoad initializes load of each timepoint perf and vent images
                            for (int j = 0; j < 4; j++)
                            {
                                // Iterate through timepoints...
                                if (j == 0)
                                {
                                    string initial = "Initial ";
                                    seriesName = initial + compVarPerf;
                                    if (preTxPERF != notAvailable)
                                    {
                                        newLoad.LoadAll(patient, seriesName, planName, course, planType, biocorrection, SBRT, CTstring, preTxPERF);
                                    }
                                }
                                else if (j == 1)
                                {
                                    string oneMonth = "1Month ";
                                    seriesName = oneMonth + compVarPerf;
                                    if (midTxPERF != notAvailable)
                                    {
                                        newLoad.LoadAll(patient, seriesName, planName, course, planType, biocorrection, SBRT, CTstring, midTxPERF);
                                    }
                                }
                                else if (j == 2)
                                {
                                    string initial = "Initial ";
                                    seriesName = initial + compVarVent;
                                    if (preTxVENT != notAvailable)
                                    {
                                        newLoad.LoadAll(patient, seriesName, planName, course, planType, biocorrection, SBRT, CTstring, preTxVENT);
                                    }
                                }
                                else if (j == 3)
                                {
                                    string oneMonth = "1Month ";
                                    seriesName = oneMonth + compVarVent;
                                    if (midTxVENT != notAvailable)
                                    {
                                        newLoad.LoadAll(patient, seriesName, planName, course, planType, biocorrection, SBRT, CTstring, midTxVENT);
                                    }
                                }
                            }

                            PerfVentAnalysis newAnalysis = new PerfVentAnalysis();
                            newAnalysis.CalculateVQ();

                            PerfVentWrite writeNew = new PerfVentWrite();
                            writeNew.WriteVQ(SBRT);

                            FunctionalChange getData = new FunctionalChange();
                            getData.CalculateChange();

                            FunctionWrite newWrite = new FunctionWrite();
                            newWrite.writeCSV(SBRT);

                            ErrorPropogation propogateError = new ErrorPropogation();
                            propogateError.DoseIntensityError();
                            propogateError.FunctionalChangeError();

                            ErrorWrite writeError = new ErrorWrite();
                            writeError.writeCSV(SBRT);
                            
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Failed at Image Load");
                            MessageBox.Show(e.Message + e.StackTrace);
                        }

                        // Clear Global Variables
                        DataProcess.DoseMaps.Clear();
                        DataProcess.FunctionalMaps.Clear();
                        DataProcess.MaxIntensityMap.Clear();
                        DataProcess.DoseIntensity.Clear();
                        LoadAllImages.perfTimepoints.Clear();
                        LoadAllImages.ventTimepoints.Clear();
                        ErrorPropogation.DoseIntensityUncertainty.Clear();
                        if (FunctionalChange.doseResponseDictionary != null)
                        {
                            FunctionalChange.doseResponseDictionary.Clear();
                            FunctionalChange.functionalChangeMetricsDictionary.Clear();
                            FunctionalChange.functionalResponseDictionary.Clear();
                            FunctionalChange.voxelsDictionary.Clear();
                            FunctionalChange.doseResponsePoints.Clear();
                            FunctionalChange.TotalDamageDictionary.Clear();
                            FunctionalChange.TotalFunctionalDamageDictionary.Clear();
                            ErrorPropogation.DoseResponseUncertainty.Clear();
                        }
                        else
                        {
                            FunctionalChange.doseResponseDictionary = new Dictionary<string, Dictionary<string, double[]>>();
                            FunctionalChange.functionalChangeMetricsDictionary = new Dictionary<string, Dictionary<string, double[]>>();
                            FunctionalChange.functionalResponseDictionary = new Dictionary<string, Dictionary<string, double[]>>();
                            FunctionalChange.voxelsDictionary = new Dictionary<string, Dictionary<string, int[]>>();
                            FunctionalChange.doseResponsePoints = new Dictionary<string, Dictionary<string, double[,,]>>();
                            FunctionalChange.TotalDamageDictionary = new Dictionary<string, Dictionary<string, double>>();
                            FunctionalChange.TotalFunctionalDamageDictionary = new Dictionary<string, Dictionary<string, double>>();
                            ErrorPropogation.DoseResponseUncertainty = new Dictionary<string, Dictionary<string, double[]>>();
                        }
                        PerfVentAnalysis.DoseVQ.Clear();
                        PerfVentAnalysis.TimepointsVQ.Clear();
                        PerfVentAnalysis.VolumeVQ.Clear();

                        app.ClosePatient();
                        newWorker.Close();
                    }




                    app.ClosePatient();
                    

                }
                batchTime.Stop();
                time = batchTime.Elapsed.TotalSeconds;
            }
            MessageBox.Show("Complete" + Environment.NewLine + "Batch time = " + time.ToString());
        }
    }
}
