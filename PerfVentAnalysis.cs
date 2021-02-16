using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFH_Initiate
{
    class PerfVentAnalysis
    {
        public static Dictionary<string, Dictionary<string, double[]>> VolumeVQ = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> VQvolume;

        public static Dictionary<string, Dictionary<string, double[]>> DoseVQ = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> VQdose;
        public static List<string> TimepointsVQ = new List<string>();

        public void CalculateVQ()
        {
            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;
            string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };
            string[] timepoints = new string[] { "Initial ", "1Month " };
            string perf = "PERF";
            string vent = "VENT";

            for (int j = 0; j < timepoints.Length; j++)
            {
                string perfTime = timepoints[j] + perf;
                string ventTime = timepoints[j] + vent;
                if (perfTimepoints.Contains(perfTime) && ventTimepoints.Contains(ventTime))
                {
                    TimepointsVQ.Add(timepoints[j]);
                    VQvolume = new Dictionary<string, double[]>();
                    VQdose = new Dictionary<string, double[]>();

                    Dictionary<string, double[,,]> perfDictionary = DataProcess.FunctionalMaps[perfTime];
                    Dictionary<string, double[,,]> ventDictionary = DataProcess.FunctionalMaps[ventTime];
                    Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTime];
                    Dictionary<string, double> perfMaxDictionary = DataProcess.MaxIntensityMap[perfTime];
                    Dictionary<string, double> ventMaxDictionary = DataProcess.MaxIntensityMap[ventTime];
                    for (int i = 0; i < structureList.Length; i++)
                    {
                        double[,,] perfData = perfDictionary[structureList[i]];
                        double[,,] ventData = ventDictionary[structureList[i]];
                        double[,,] doseData = doseDictionary[structureList[i]];
                        int xCount = perfData.GetLength(0);
                        int yCount = perfData.GetLength(1);
                        int zCount = perfData.GetLength(2);
                        double[,,] points = new double[xCount, yCount, zCount];
                        double perfMax = perfMaxDictionary[structureList[i]];
                        double ventMax = ventMaxDictionary[structureList[i]];

                        double[] volumeVQ = new double[9];
                        double[] doseVQ = new double[10];
                        int VQvoxels = 0;
                        int lowVQ = 0;
                        int lowV = 0;
                        int lowQ = 0;
                        int midVQ = 0;
                        int midQ = 0;
                        int midV = 0;
                        int highVQ = 0;
                        int highV = 0;
                        int highQ = 0;
                        int mismatchQ = 0;
                        int mismatchV = 0;
                        double mismatchQdose = 0;
                        double mismatchVdose = 0;
                        double lowVQdose = 0;
                        double midVQdose = 0;
                        double highVQdose = 0;
                        double lowVQ20 = 0;
                        double midVQ20 = 0;
                        double highVQ20 = 0;
                        double fVQ = 0;
                        double overlapVQ = 0;
                        double sumVQ = 0;
                        double intensityVQ = 0;
                        int iVQcounter = 0;
                        for (int z = 0; z < zCount; z++)
                        {
                            for (int y = 0; y < yCount; y++)
                            {
                                for (int x = 0; x < xCount; x++)
                                {
                                    if (!double.IsNaN(perfData[x, y, z]) && !double.IsNaN(ventData[x, y, z]))
                                    {
                                        VQvoxels++;
                                        sumVQ = perfData[x, y, z] + ventData[x, y, z];
                                        intensityVQ += sumVQ;
                                        if (doseData[x,y,z] >= 20.0)
                                        {
                                            iVQcounter++;
                                            fVQ += sumVQ;
                                        }
                                        if (perfData[x,y,z] <= perfMax * 0.15)
                                        {
                                            lowQ++;
                                        }
                                        else if (perfData[x,y,z] > perfMax * 0.15 && perfData[x,y,z] <= perfMax * 0.7)
                                        {
                                            midQ++;
                                        }
                                        else if (perfData[x,y,z] > perfMax * 0.7)
                                        {
                                            highQ++;
                                        }
                                        if (ventData[x,y,z] <= ventMax * 0.15)
                                        {
                                            lowV++;
                                        }
                                        else if (ventData[x,y,z] > ventMax * 0.15 && ventData[x,y,z] <= ventMax * 0.7)
                                        {
                                            midV++;
                                        }
                                        else if (ventData[x,y,z] > ventMax * 0.7)
                                        {
                                            highV++;
                                        }
                                        if (perfData[x, y, z] <= perfMax * 0.15 && ventData[x, y, z] <= ventMax * 0.15)
                                        {
                                            lowVQ++;
                                            overlapVQ++;
                                            lowVQdose += doseData[x, y, z];
                                            if (doseData[x,y,z] >= 20.0)
                                            {
                                                lowVQ20++;
                                            }
                                        }
                                        else if (perfData[x, y, z] > perfMax * 0.15 && perfData[x, y, z] <= perfMax * 0.7 && ventData[x,y,z] > ventMax * 0.15 && ventData[x,y,z] <= ventMax * 0.7)
                                        {
                                            midVQ++;
                                            overlapVQ++;
                                            midVQdose += doseData[x, y, z];
                                            if (doseData[x, y, z] >= 20.0)
                                            {
                                                midVQ20++;
                                            }
                                        }
                                        else if (perfData[x, y, z] > perfMax * 0.7 && ventData[x, y, z] > ventMax * 0.7)
                                        {
                                            highVQ++;
                                            overlapVQ++;
                                            highVQdose += doseData[x, y, z];
                                            if (doseData[x, y, z] >= 20.0)
                                            {
                                                highVQ20++;
                                            }
                                        }
                                        else if (perfData[x,y,z] <= perfMax * 0.15 && ventData[x,y,z] > ventMax * 0.15)
                                        {
                                            mismatchQ++;
                                            mismatchQdose += doseData[x, y, z];
                                        }
                                        else if (ventData[x,y,z] <= ventMax * 0.15 && perfData[x,y,z] > perfData[x,y,z] * 0.15)
                                        {
                                            mismatchV++;
                                            mismatchVdose += doseData[x, y, z];
                                        }
                                    }
                                }
                            }
                        }
                        volumeVQ[0] = (double)lowVQ / VQvoxels;
                        volumeVQ[1] = (double)midVQ / VQvoxels;
                        volumeVQ[2] = (double)highVQ / VQvoxels;
                        volumeVQ[3] = (double)mismatchQ / VQvoxels;
                        volumeVQ[4] = (double)mismatchV / VQvoxels;
                        volumeVQ[5] = (double)overlapVQ / VQvoxels;
                        volumeVQ[6] = (double)2 * lowVQ / (lowQ + lowV);
                        volumeVQ[7] = (double)2 * midVQ / (midQ + midV);
                        volumeVQ[8] = (double)2 * highVQ / (highQ + highV);
                        doseVQ[0] = lowVQdose / lowVQ;
                        doseVQ[1] = midVQdose / midVQ;
                        doseVQ[2] = highVQdose / highVQ;
                        doseVQ[3] = lowVQ20 / VQvoxels;
                        doseVQ[4] = midVQ20 / VQvoxels;
                        doseVQ[5] = highVQ20 / VQvoxels;
                        doseVQ[6] = mismatchQdose / mismatchQ;
                        doseVQ[7] = mismatchVdose / mismatchV;
                        doseVQ[8] = fVQ / intensityVQ;
                        doseVQ[9] = (double) fVQ / iVQcounter;

                        VQvolume.Add(structureList[i], volumeVQ);
                        VQdose.Add(structureList[i], doseVQ);
                    }
                    VolumeVQ.Add(timepoints[j], VQvolume);
                    DoseVQ.Add(timepoints[j], VQdose);
                }
            }
        }

    }
}
