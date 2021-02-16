using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class PerfVentWrite
    {
        private static string currentPatient = null;
        private static bool firstLine = true;
        string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };

        public void WriteVQ(string SBRT)
        {
            List<string> Timepoints = PerfVentAnalysis.TimepointsVQ;
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = System.IO.Path.Combine(path, "DataVQ.csv");
            StringBuilder builder = new StringBuilder();
            StringBuilder builderII = new StringBuilder();
            string[] timepoints = new string[] { "Initial ", "1Month " };
            string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };
            string perf = "PERF";
            string vent = "VENT";
            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;
            string contralateral = DataProcess.contralateralLung;

            for (int j = 0; j < timepoints.Length; j++)
            {
                string intensityFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_" + timepoints[j] + "VQ" + ".csv";
                string filePathII = System.IO.Path.Combine(path, intensityFileName);


                string perfTime = timepoints[j] + perf;
                string ventTime = timepoints[j] + vent;
                if (perfTimepoints.Contains(perfTime) && ventTimepoints.Contains(ventTime))
                {

                    Dictionary<string, double[,,]> perfDictionary = DataProcess.FunctionalMaps[perfTime];
                    Dictionary<string, double[,,]> ventDictionary = DataProcess.FunctionalMaps[ventTime];
                    Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTime];

                    // LUNGS-GTV Writing
                    double[,,] perfData = perfDictionary[structureList[0]];
                    double[,,] ventData = ventDictionary[structureList[0]];
                    double[,,] doseData = doseDictionary[structureList[0]];

                    /*
                    double[,,] perfData;
                    double[,,] ventData;
                    double[,,] doseData;
                    // Ipsilateral_Lung-GTV Writing
                    if (contralateral == structureList[1])
                    {
                        perfData = perfDictionary[structureList[2]];
                        ventData = ventDictionary[structureList[2]];
                        doseData = doseDictionary[structureList[2]];
                    }
                    else
                    {
                        perfData = perfDictionary[structureList[1]];
                        ventData = ventDictionary[structureList[1]];
                        doseData = doseDictionary[structureList[1]];
                    }
                    */

                    int xCount = perfData.GetLength(0);
                    int yCount = perfData.GetLength(1);
                    int zCount = perfData.GetLength(2);

                    for (int z = 0; z < zCount; z++)
                    {
                        for (int y = 0; y < yCount; y++)
                        {
                            for (int x = 0; x < xCount; x++)
                            {
                                if (!double.IsNaN(perfData[x, y, z]) && !double.IsNaN(ventData[x, y, z]))
                                {
                                    builderII.Append(perfData[x, y, z] + "," + ventData[x, y, z] + "," + doseData[x, y, z]);
                                    builderII.AppendLine();
                                }
                            }
                        }
                    }
                    File.AppendAllText(filePathII, builderII.ToString());

                }

            }


            if (firstLine)
            {
                builder.Append("Patient,Timepoint,Structure,SBRT,low VQ,mid VQ,high VQ,mismatch Q,mismatch V,overlap VQ,DSC_Low,DSC_Mid,DSC_High,low VQ Dose,mid VQ Dose,high VQ Dose,low VQ 20,mid VQ 20,high VQ 20,mismatch Q dose, mismatch V dose, fVQ20, iVQ20");
                firstLine = false;
            }
            if (currentPatient != Program.patientID)
            {
                currentPatient = Program.patientID;
            }
            for (int i = 0; i < Timepoints.Count; i++)
            {
                Dictionary<string, double[]> volumeVQ = PerfVentAnalysis.VolumeVQ[Timepoints[i]];
                Dictionary<string, double[]> doseVQ = PerfVentAnalysis.DoseVQ[Timepoints[i]];
                for (int j = 0; j < structureList.Length; j++)
                {
                    double[] VQvolume = volumeVQ[structureList[j]];
                    double[] VQdose = doseVQ[structureList[j]];
                    builder.AppendLine();
                    builder.Append(currentPatient + ",");
                    builder.Append(Timepoints[i] + ",");
                    builder.Append(structureList[j] + "," + SBRT + ",");
                    for (int k = 0; k < VQvolume.Length; k++)
                    {
                        builder.Append(VQvolume[k].ToString() + ",");
                    }
                    for (int k = 0; k < VQdose.Length; k++)
                    {
                        builder.Append(VQdose[k].ToString() + ",");
                    }
                }
            }
            File.AppendAllText(filePath, builder.ToString());
        }

    }
}
