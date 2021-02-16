using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class FunctionWrite
    {
        private static string currentPatient = null;
        private static bool firstLineI = true;
        private static bool firstLineII = true;
        string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };

        public void writeCSV(string SBRT)
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string lungsPathI = System.IO.Path.Combine(path, "Lungs_Intensity.csv");
            string lungsPathII = System.IO.Path.Combine(path, "Lungs_Function.csv");
            string ipsPathI = System.IO.Path.Combine(path, "Ips_Intensity.csv");
            string ipsPathII = System.IO.Path.Combine(path, "Ips_Function.csv");
            string contPathI = System.IO.Path.Combine(path, "Cont_Intensity.csv");
            string contPathII = System.IO.Path.Combine(path, "Cont_Function.csv");
            string testRetestPERF = "TestRetestPERF_";
            string testRetestVENT = "TestRetestVENT_";

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
                    intensityWritePath = contPathI;
                    functionWritePath = contPathII;
                }
                else if (structureList[i] == structureList[0])
                {
                    intensityWritePath = lungsPathI;
                    functionWritePath = lungsPathII;
                    //WriteTestRetestData(path, testRetestPERF, structureList[i], perfTimepoints);
                    //WriteTestRetestData(path, testRetestVENT, structureList[i], ventTimepoints);
                }
                else
                {
                    intensityWritePath = ipsPathI;
                    functionWritePath = ipsPathII;
                }
                for (int j = 0; j < perfTimepoints.Count; j++)
                {
                    WriteIntensityData(intensityWritePath, structureList[i], perfTimepoints[j], SBRT);
                    WriteFunctionalData(functionWritePath, structureList[i], perfTimepoints[j], SBRT);
                }
                for (int j = 0; j < ventTimepoints.Count; j++)
                {
                    WriteIntensityData(intensityWritePath, structureList[i], ventTimepoints[j], SBRT);
                    WriteFunctionalData(functionWritePath, structureList[i], ventTimepoints[j], SBRT);
                }
                firstLineI = true;
                firstLineII = true;
            }
        }

        private void WriteIntensityData(string writePath, string structure, string timepoint, string SBRT)
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
                        dataBuilder.Append("I" + (bins[i] + binWidth).ToString() + ",");
                    }
                    firstLineI = false;
                }

                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + "," + SBRT +",");
                Dictionary<string, double[]> doseIntensityDictionary = DataProcess.DoseIntensity[timepoint];
                foreach (double doseIntensity in doseIntensityDictionary[structure])
                {
                    dataBuilder.Append(doseIntensity.ToString() + ",");
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

        private void WriteFunctionalData(string writePath, string structure, string timepoint, string SBRT)
        {
            StringBuilder dataBuilder = new StringBuilder();
            double[] bins = DataProcess.doseBinsMap[structure];
            if (bins.Length > 1)
            {
                if (firstLineII)
                {
                    dataBuilder.AppendLine();
                    dataBuilder.Append("Patient,Image/Dose,Structure,SBRT,Total Damage,Total Functional Damage,H2L,H2M,M2L,M2H,L2M,L2H,L2L,M2M,H2H,");
                    double binWidth = (bins[1] - bins[0]) / 2;
                    for (int i = 0; i < bins.Length - 1; i++)
                    {
                        dataBuilder.Append("N" + (bins[i] + binWidth).ToString() + ",");
                        dataBuilder.Append("I" + (bins[i] + binWidth).ToString() + ",");
                        dataBuilder.Append("F" + (bins[i] + binWidth).ToString() + ",");
                    }
                    firstLineII = false;
                }

                dataBuilder.AppendLine();
                dataBuilder.Append(currentPatient + ",");
                dataBuilder.Append(timepoint + ",");
                dataBuilder.Append(structure + "," + SBRT + ",");
                if (FunctionalChange.doseResponseDictionary != null)
                {
                    Dictionary<string, double> totalDamageDictionary = FunctionalChange.TotalDamageDictionary[timepoint];
                    Dictionary<string, double> totalFunctionalDamageDictionary = FunctionalChange.TotalFunctionalDamageDictionary[timepoint];
                    Dictionary<string, double[]> functionalChangeMetrics = FunctionalChange.functionalChangeMetricsDictionary[timepoint];
                    double totalDamage = totalDamageDictionary[structure];
                    double functionalDamage = totalFunctionalDamageDictionary[structure];
                    double[] fcMetrics = functionalChangeMetrics[structure];
                    dataBuilder.Append(totalDamage.ToString() + "," + functionalDamage.ToString() + "," + fcMetrics[0].ToString() + "," + fcMetrics[1].ToString() + "," + fcMetrics[2].ToString() + "," + fcMetrics[3].ToString() + "," + fcMetrics[4].ToString() + "," + fcMetrics[5].ToString() + ",");
                    dataBuilder.Append(fcMetrics[6].ToString() + "," + fcMetrics[7].ToString() + "," + fcMetrics[8].ToString() + ",");
                    Dictionary<string, int[]> voxelsDictionary = FunctionalChange.voxelsDictionary[timepoint];
                    int[] responseVoxels = voxelsDictionary[structure];
                    Dictionary<string, double[]> doseResponseDictionary = FunctionalChange.doseResponseDictionary[timepoint];
                    Dictionary<string, double[]> functionalResponseDictionary = FunctionalChange.functionalResponseDictionary[timepoint];
                    double[] doseResponse = doseResponseDictionary[structure];
                    double[] functionalResponse = functionalResponseDictionary[structure];
                    for (int i = 0; i < doseResponse.Length; i++)
                    {
                        dataBuilder.Append(responseVoxels[i].ToString() + ",");
                        dataBuilder.Append(doseResponse[i].ToString() + ",");
                        dataBuilder.Append(functionalResponse[i].ToString() + ",");
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

        private void WriteTestRetestData(string writePath, string fileName, string structure, List<string> timepoints)
        {
            if (FunctionalChange.doseResponsePoints != null)
            {
                StringBuilder dataBuilder = new StringBuilder();
                dataBuilder.AppendLine(currentPatient);
                List<double[,,]> doseList = new List<double[,,]>();
                List<double[,,]> responseList = new List<double[,,]>();
                int xCount = 0;
                int yCount = 0;
                int zCount = 0;

                dataBuilder.AppendLine();
                for (int i = 0; i < timepoints.Count; i++)
                {
                    string timepoint = timepoints[i];
                    dataBuilder.Append("Dose," + timepoint + "/" + structure);
                    Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[timepoint];
                    Dictionary<string, double[,,]> pointsDictionary = FunctionalChange.doseResponsePoints[timepoint];
                    double[,,] responsePoints = pointsDictionary[structure];
                    double[,,] dose = doseDictionary[structure];
                    if (responsePoints.GetLength(0) > xCount)
                    {
                        xCount = responsePoints.GetLength(0);
                    }
                    if (responsePoints.GetLength(1) > yCount)
                    {
                        yCount = responsePoints.GetLength(1);
                    }
                    if (responsePoints.GetLength(2) > zCount)
                    {
                        zCount = responsePoints.GetLength(2);
                    }
                    doseList.Add(dose);
                    responseList.Add(responsePoints);
                }
                for (int x = 0; x < xCount; x++)
                {
                    for (int y = 0; y < yCount; y++)
                    {
                        for (int z = 0; z < zCount; z++)
                        {
                            dataBuilder.AppendLine();
                            for (int i = 0; i < timepoints.Count; i++)
                            {
                                double[,,] dose = doseList[i];
                                double[,,] responsePoints = responseList[i];
                                if (!double.IsNaN(responsePoints[x, y, z]))
                                {
                                    dataBuilder.Append(dose[x, y, z].ToString() + "," + responsePoints[x, y, z].ToString() + ",");
                                }
                                else
                                {
                                    dataBuilder.Append("-," + "-,");
                                }
                            }
                        }
                    }
                }
                fileName = string.Concat(fileName, currentPatient, ".csv");
                writePath = System.IO.Path.Combine(writePath, fileName);
                File.AppendAllText(writePath, dataBuilder.ToString());
            }
        }
    }
}
