using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFHAnalysis;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class CsvWrite
    {
        private static string currentPatient = null;
        private static bool firstLineII = true;

        public void writeCSV(string imageStudy, string loadedPlan, string SBRT)
        {
            string lungID = DataProcess.lungID;
            string rightLungID = DataProcess.rightLungID;
            string leftLungID = DataProcess.leftLungID;
            string heartID = DataProcess.heartID;

            /*
            string lungID;
            string rightLungID;
            string leftLungID;
            if (imageStudy.Contains("Initial"))
            {
                lungID = "PRE_LUNGS-GTV";
                rightLungID = "PRE_RIGHT_LUNG";
                leftLungID = "PRE_LEFT_LUNG";
            }
            else
            {
                lungID = "MID_LUNGS-GTV";
                rightLungID = "MID_RIGHT_LUNG";
                leftLungID = "MID_LEFT_LUNG";
            }
            */

            string[] structureList = new string[] { lungID, rightLungID, leftLungID };
            //string[] structureList = new string[] { lungID, rightLungID, leftLungID, heartID, "Z_RUL", "Z_RML", "Z_RLL", "Z_LUL", "Z_LML", "Z_LLL" };
            //string[] structureList = new string[] { lungID, rightLungID, leftLungID, heartID, "Z_RUL_4mm", "Z_RML_4mm", "Z_RLL_4mm", "Z_LUL_4mm", "Z_LML_4mm", "Z_LLL_4mm" };
            //string[] structureList = new string[] { lungID, rightLungID, leftLungID, "PRE_LUNGS_VENT", "PRE_RIGHT_VENT", "PRE_LEFT_VENT", "PRE_LF_VENT", "PRE_F_VENT", "PRE_HF_VENT" };
            //string[] structureList = new string[] { lungID, rightLungID, leftLungID, "PRE_LUNGS_PERF", "PRE_RIGHT_PERF", "PRE_LEFT_PERF", "PRE_QUAD1_PERF", "PRE_QUAD2_PERF", "PRE_QUAD3_PERF", "PRE_QUAD4_PERF" };


            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePathII = System.IO.Path.Combine(path, "lookup.csv");
            StringBuilder builderII = new StringBuilder();
            if (firstLineII)
            {
                builderII.Append("Patient,Image/Dose,Structure,SBRT,%Cleaned,Normalizer,RightLungCleaner,LeftLungCleaner,Volume,Max Dose [Gy],Mean Dose[Gy], StDv Dose [Gy],");
                //builderII.Append("Patient,Image/Dose,Structure,SBRT,Normalizer,Right Lung Cleaner, Left Lung Cleaner,%Cleaned,Volume,Max Dose [Gy],Mean Dose[Gy], StDv Dose [Gy],");
                for (int i = 0; i < MetricOptions.aParameter.Count; i++)
                {
                    builderII.Append("gEUD (a=" + MetricOptions.aParameter[i].ToString() + ") [Gy],");
                }
                for (int i = 0; i < MetricOptions.fVDvalue.Count; i++)
                {
                    builderII.Append("V" + MetricOptions.fVDvalue[i].ToString() + " [%],");
                }
                builderII.Append("AD30,AD50,D50,D25,Max Intensity,Mean Intensity,StDv Intensity,Intensity COV,");
                for (int i = 0; i < MetricOptions.aParameter.Count; i++)
                {
                    builderII.Append("gEUfD (a=" + MetricOptions.aParameter[i].ToString() + ") [Gy],");
                    builderII.Append("gEUiD (a=" + MetricOptions.aParameter[i].ToString() + ") [Gy],");
                }
                for (int i = 0; i < MetricOptions.fVDvalue.Count; i++)
                {
                    builderII.Append("fV" + MetricOptions.fVDvalue[i].ToString() + " [%],");
                    builderII.Append("iV" + MetricOptions.fVDvalue[i].ToString() + " [%],");
                }
                builderII.Append("MfLD [Gy],");
                //builderII.Append("%LF,AD2LF [Gy],%F,AD2F [Gy],%HF,AD2HF [Gy],Total Intensity,Group1 Damage,Group1 Voxels,Group2 Damage,Group2 Voxels,Group3 Damage,Group3 Voxels,Group4 Damage,Group4 Voxels,Total Expected Damage");
                builderII.Append("%LF,AD2LF [Gy],LF20,%F,AD2F [Gy],F20,%HF,AD2HF [Gy],HF20,Upper Avg. Intensity,AD2U,Middle Avg. Intensity,AD2M,Lower Avg. Intensity,AD2L,Total Intensity,Group1 Damage,Group1 Voxels,Group2 Damage,Group2 Voxels,Group3 Damage,Group3 Voxels,Group4 Damage,Group4 Voxels,Total Expected Damage");
                firstLineII = false;
            }
            if (currentPatient != Program.patientID)
            {
                currentPatient = Program.patientID;
            }
            for (int i = 0; i < structureList.Length; i++)
            {
                builderII.AppendLine();
                builderII.Append(currentPatient + ",");
                builderII.Append(imageStudy + "/" + loadedPlan + ",");
                builderII.Append(structureList[i] + "," + SBRT + ",");
                //builderII.Append(DataProcess.intensityNormalizer.ToString() + "," + DataProcess.rightLungCutoff.ToString() + "," + DataProcess.leftLungCutoff.ToString() + ",");
                builderII.Append(DataProcess.totalCleanedVoxels.ToString() + ",");
                builderII.Append(DataProcess.intensityNormalizer.ToString() + "," + DataProcess.rightLungCutoff.ToString() + "," + DataProcess.leftLungCutoff.ToString() + ",");
                foreach (double metric in MetricAnalysis.dataStructure[structureList[i].ToUpper()])
                {
                    builderII.Append(metric.ToString() + ",");
                }
            }
            File.AppendAllText(filePathII, builderII.ToString());
        }
    }
}
