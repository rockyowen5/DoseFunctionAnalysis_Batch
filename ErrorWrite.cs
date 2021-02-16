using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class ErrorWrite
    {
        private static string currentPatient = null;
        private static bool firstLineI = true;
        private static bool firstLineII = true;
        string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };

        public void writeCSV(string SBRT)
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string lungsErrorPathI = System.IO.Path.Combine(path, "Lungs_Intensity_Error.csv");
            string lungsErrorPathII = System.IO.Path.Combine(path, "Lungs_Function_Error.csv");
            string ipsErrorPathI = System.IO.Path.Combine(path, "Ips_Intensity_Error.csv");
            string ipsErrorPathII = System.IO.Path.Combine(path, "Ips_Function_Error.csv");
            string contErrorPathI = System.IO.Path.Combine(path, "Cont_Intensity_Error.csv");
            string contErrorPathII = System.IO.Path.Combine(path, "Cont_Function_Error.csv");

            if (currentPatient != Program.patientID)
            {
                currentPatient = Program.patientID;
                firstLineI = true;
                firstLineII = true;
            }

            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;

            for (int i = 0; i < structureList.Length; i++)
            {
                string intensityWritePath;
                string functionWritePath;
                if (structureList[i] == DataProcess.contralateralLung)
                {
                    intensityWritePath = contErrorPathI;
                    functionWritePath = contErrorPathII;
                }
                else if (structureList[i] == structureList[0])
                {
                    intensityWritePath = lungsErrorPathI;
                    functionWritePath = lungsErrorPathII;
                }
                else
                {
                    intensityWritePath = ipsErrorPathI;
                    functionWritePath = ipsErrorPathII;
                }
                for (int j = 0; j < perfTimepoints.Count; j++)
                {
                    WriteIntensityError(intensityWritePath, structureList[i], perfTimepoints[j], SBRT);
                    WriteFunctionalError(functionWritePath, structureList[i], perfTimepoints[j], SBRT);
                }
                for (int j = 0; j < ventTimepoints.Count; j++)
                {
                    WriteIntensityError(intensityWritePath, structureList[i], ventTimepoints[j], SBRT);
                    WriteFunctionalError(functionWritePath, structureList[i], ventTimepoints[j], SBRT);
                }
                firstLineI = true;
                firstLineII = true;
            }
        }

        private void WriteIntensityError(string writePath, string structure, string timepoint, string SBRT)
        {
            StringBuilder dataBuilder = new StringBuilder();

            double[] bins = DataProcess.doseBinsMap[structure];
            if (bins.Length > 1)
            {
                if (firstLineI)
                {
                    dataBuilder.AppendLine();
                    dataBuilder.Append("Patient,Image/Dose,Structure,SBRT,");
                    double binWidth = (bins[1] - bins[0]) / 2;
                    for (int i = 0; i < bins.Length - 1; i++)
                    {
                        dataBuilder.Append("Error_I" + (bins[i] + binWidth).ToString() + ",");
                    }
                    firstLineI = false;
                }

                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + "," + SBRT + ",");
                Dictionary<string, double[]> doseIntensityErrorDictionary = ErrorPropogation.DoseIntensityUncertainty[timepoint];
                double[] doseIntensityError = doseIntensityErrorDictionary[structure];
                foreach (double error in doseIntensityError)
                {
                    dataBuilder.Append(error.ToString() + ",");
                }

                File.AppendAllText(writePath, dataBuilder.ToString());

            }
            else
            {
                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + ",");
                dataBuilder.Append("No Structure");
                File.AppendAllText(writePath, dataBuilder.ToString());
            }

        }

        private void WriteFunctionalError(string writePath, string structure, string timepoint, string SBRT)
        {
            StringBuilder dataBuilder = new StringBuilder();
            double[] bins = DataProcess.doseBinsMap[structure];
            if (bins.Length > 1)
            {
                if (firstLineII)
                {
                    dataBuilder.AppendLine();
                    dataBuilder.Append("Patient,Image/Dose,Structure,SBRT,");
                    double binWidth = (bins[1] - bins[0]) / 2;
                    for (int i = 0; i < bins.Length - 1; i++)
                    {
                        dataBuilder.Append("Error_F" + (bins[i] + binWidth).ToString() + ",");
                    }
                    firstLineII = false;
                }

                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + "," + SBRT + ",");
                if (ErrorPropogation.DoseResponseUncertainty != null)
                {
                    Dictionary<string, double[]> doseResponseErrorDictionary = ErrorPropogation.DoseResponseUncertainty[timepoint];
                    double[] functionChangeError = doseResponseErrorDictionary[structure];
                    foreach (double error in functionChangeError)
                    {
                        dataBuilder.Append(error.ToString() + ",");
                    }
                }

                File.AppendAllText(writePath, dataBuilder.ToString());
            }
            else
            {
                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + ",");
                dataBuilder.Append("No Structure");
                File.AppendAllText(writePath, dataBuilder.ToString());

            }
        }

    }
}
