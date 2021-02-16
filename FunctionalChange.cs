using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class FunctionalChange
    {
        public static Dictionary<string, Dictionary<string, double[]>> doseResponseDictionary = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> doseResponse;

        public static Dictionary<string, Dictionary<string, double[]>> functionalChangeMetricsDictionary = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> functionalChangeMetrics;

        public static Dictionary<string, Dictionary<string, double[]>> functionalResponseDictionary = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> functionalResponse;

        public static Dictionary<string, Dictionary<string, int[]>> voxelsDictionary = new Dictionary<string, Dictionary<string, int[]>>();
        private Dictionary<string, int[]> responseVoxels;

        public static Dictionary<string, Dictionary<string, double[,,]>> doseResponsePoints = new Dictionary<string, Dictionary<string, double[,,]>>();
        private Dictionary<string, double[,,]> pointsDictionary;
        double[,,] points;

        public static Dictionary<string, Dictionary<string, double>> TotalDamageDictionary = new Dictionary<string, Dictionary<string, double>>();
        private Dictionary<string, double> totalDamage;

        public static Dictionary<string, Dictionary<string, double>> TotalFunctionalDamageDictionary = new Dictionary<string, Dictionary<string, double>>();
        private Dictionary<string, double> totalFunctionalDamage;



        public void CalculateChange()
        {
            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;

            if (perfTimepoints.Contains("Initial PERF"))
            {
                int baseIndex = perfTimepoints.IndexOf("Initial PERF");
                Dictionary<string, double[,,]> baseDictionary = DataProcess.FunctionalMaps[perfTimepoints[baseIndex]];
                Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTimepoints[baseIndex]];
                Dictionary<string, double> baseMaxDictionary = DataProcess.MaxIntensityMap[perfTimepoints[baseIndex]];
                string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };
                double baseMax = baseMaxDictionary[structureList[0]];


                for (int j = 0; j < perfTimepoints.Count; j++)
                {
                    doseResponse = new Dictionary<string, double[]>();
                    functionalChangeMetrics = new Dictionary<string, double[]>();
                    functionalResponse = new Dictionary<string, double[]>();
                    responseVoxels = new Dictionary<string, int[]>();
                    pointsDictionary = new Dictionary<string, double[,,]>();
                    totalDamage = new Dictionary<string, double>();
                    totalFunctionalDamage = new Dictionary<string, double>();
                    Dictionary<string, double> compMaxDictionary = DataProcess.MaxIntensityMap[perfTimepoints[j]];
                    Dictionary<string, double[,,]> compDictionary = DataProcess.FunctionalMaps[perfTimepoints[j]];
                    double compMax = compMaxDictionary[structureList[0]];

                    for (int i = 0; i < structureList.Length; i++)
                    {
                        double[,,] baseSPECT = baseDictionary[structureList[i]];
                        double[,,] dose = doseDictionary[structureList[i]];
                        int xCount = baseSPECT.GetLength(0);
                        int yCount = baseSPECT.GetLength(1);
                        int zCount = baseSPECT.GetLength(2);
                        points = new double[xCount, yCount, zCount];
                        double[,,] compSPECT = compDictionary[structureList[i]];
                        double[] doseBins = DataProcess.doseBinsMap[structureList[i]];
                        int[] binVoxels = new int[doseBins.Length - 1];
                        double[] doseResponseCounts = new double[doseBins.Length - 1];
                        double[] functionalResponseCounts = new double[doseBins.Length - 1];
                        double intensityDamage = 0.0;
                        double functionalDamage = 0.0;
                        int low2high = 0;
                        int low2mid = 0;
                        int mid2high = 0;
                        int mid2low = 0;
                        int high2mid = 0;
                        int high2low = 0;
                        int low2low = 0;
                        int mid2mid = 0;
                        int high2high = 0;
                        int totalVoxels = 0;
                        for (int z = 0; z < zCount; z++)
                        {
                            for (int y = 0; y < yCount; y++)
                            {
                                for (int x = 0; x < xCount; x++)
                                {
                                    if (!double.IsNaN(baseSPECT[x, y, z]) && !double.IsNaN(compSPECT[x, y, z]))
                                    {
                                        double differential = (compSPECT[x, y, z] - baseSPECT[x, y, z]);
                                        intensityDamage += differential;
                                        points[x, y, z] = differential;
                                        double functionalDifferential = 0.0;
                                        totalVoxels++;
                                        if (baseSPECT[x,y,z] >= baseMax * 0.7 && compSPECT[x,y,z] < compMax * 0.15)
                                        {
                                            functionalDifferential = -2;
                                            high2low++;
                                        }
                                        else if (baseSPECT[x,y,z] >= baseMax * 0.7 && compSPECT[x,y,z] >= compMax * 0.15 && compSPECT[x,y,z] < compMax * 0.7)
                                        {
                                            functionalDifferential = -1;
                                            high2mid++;
                                        }
                                        else if (baseSPECT[x,y,z] >= baseMax * 0.15 && baseSPECT[x,y,z] < baseMax * 0.7 && compSPECT[x,y,z] < compMax * 0.15)
                                        {
                                            functionalDifferential = -1;
                                            mid2low++;
                                        }
                                        else if (baseSPECT[x,y,z] < baseMax * 0.15 && compSPECT[x,y,z] >= compMax * 0.15 && compSPECT[x,y,z] < compMax * 0.7)
                                        {
                                            functionalDifferential = 1;
                                            low2mid++;
                                        }
                                        else if (baseSPECT[x,y,z] >= baseMax * 0.15 && baseSPECT[x,y,z] < baseMax * 0.7 && compSPECT[x,y,z] >= compMax * 0.7)
                                        {
                                            functionalDifferential = 1;
                                            mid2high++;
                                        }
                                        else if (baseSPECT[x,y,z] < baseMax * 0.15 && compSPECT[x,y,z] >= compMax * 0.7)
                                        {
                                            functionalDifferential = 2;
                                            low2high++;
                                        }
                                        else if (baseSPECT[x, y, z] < baseMax * 0.15 && compSPECT[x, y, z] < compMax * 0.15)
                                        {
                                            low2low++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.15 && baseSPECT[x, y, z] < baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.15 && compSPECT[x, y, z] < compMax * 0.7)
                                        {
                                            mid2mid++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.7)
                                        {
                                            high2high++;
                                        }
                                        functionalDamage += functionalDifferential;
                                        for (int k = 0; k < doseBins.Length - 1; k++)
                                        {
                                            if (dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                            {
                                                doseResponseCounts[k] += differential;
                                                functionalResponseCounts[k] += functionalDifferential;
                                                binVoxels[k]++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        points[x, y, z] = double.NaN;
                                    }
                                }
                            }
                        }
                        double h2l = (double)high2low / totalVoxels;
                        double h2m = (double)high2mid / totalVoxels;
                        double m2l = (double)mid2low / totalVoxels;
                        double m2h = (double)mid2high / totalVoxels;
                        double l2m = (double)low2mid / totalVoxels;
                        double l2h = (double)low2high / totalVoxels;
                        double l2l = (double)low2low / totalVoxels;
                        double m2m = (double)mid2mid / totalVoxels;
                        double h2h = (double)high2high / totalVoxels;
                        double[] functionalChangeSave = new double[] { h2l, h2m, m2l, m2h, l2m, l2h, l2l, m2m, h2h };

                        doseResponse.Add(structureList[i], doseResponseCounts);
                        functionalChangeMetrics.Add(structureList[i], functionalChangeSave);
                        functionalResponse.Add(structureList[i], functionalResponseCounts);
                        responseVoxels.Add(structureList[i], binVoxels);
                        pointsDictionary.Add(structureList[i], points);
                        totalDamage.Add(structureList[i], intensityDamage);
                        totalFunctionalDamage.Add(structureList[i], functionalDamage);
                    }
                    doseResponseDictionary.Add(perfTimepoints[j], doseResponse);
                    functionalChangeMetricsDictionary.Add(perfTimepoints[j], functionalChangeMetrics);
                    functionalResponseDictionary.Add(perfTimepoints[j], functionalResponse);
                    voxelsDictionary.Add(perfTimepoints[j], responseVoxels);
                    doseResponsePoints.Add(perfTimepoints[j], pointsDictionary);
                    TotalDamageDictionary.Add(perfTimepoints[j], totalDamage);
                    TotalFunctionalDamageDictionary.Add(perfTimepoints[j], totalFunctionalDamage);
                }
            }
            else
            {
                doseResponseDictionary = null;
                functionalChangeMetricsDictionary = null;
                functionalResponseDictionary = null;
                voxelsDictionary = null;
                doseResponsePoints = null;
                TotalDamageDictionary = null;
                TotalFunctionalDamageDictionary = null;
            }

            if (ventTimepoints.Contains("Initial VENT"))
            {
                int baseIndex = ventTimepoints.IndexOf("Initial VENT");
                Dictionary<string, double[,,]> baseDictionary = DataProcess.FunctionalMaps[ventTimepoints[baseIndex]];
                Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[ventTimepoints[baseIndex]];
                Dictionary<string, double> baseMaxDictionary = DataProcess.MaxIntensityMap[ventTimepoints[baseIndex]];
                string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };
                double baseMax = baseMaxDictionary[structureList[0]];


                for (int j = 0; j < ventTimepoints.Count; j++)
                {
                    doseResponse = new Dictionary<string, double[]>();
                    functionalChangeMetrics = new Dictionary<string, double[]>();
                    functionalResponse = new Dictionary<string, double[]>();
                    responseVoxels = new Dictionary<string, int[]>();
                    pointsDictionary = new Dictionary<string, double[,,]>();
                    totalDamage = new Dictionary<string, double>();
                    totalFunctionalDamage = new Dictionary<string, double>();
                    for (int i = 0; i < structureList.Length; i++)
                    {
                        double[,,] baseSPECT = baseDictionary[structureList[i]];
                        double[,,] dose = doseDictionary[structureList[i]];
                        int xCount = baseSPECT.GetLength(0);
                        int yCount = baseSPECT.GetLength(1);
                        int zCount = baseSPECT.GetLength(2);
                        points = new double[xCount, yCount, zCount];

                        Dictionary<string, double> compMaxDictionary = DataProcess.MaxIntensityMap[ventTimepoints[j]];
                        Dictionary<string, double[,,]> compDictionary = DataProcess.FunctionalMaps[ventTimepoints[j]];
                        double compMax = compMaxDictionary[structureList[0]];
                        double[,,] compSPECT = compDictionary[structureList[i]];
                        double[] doseBins = DataProcess.doseBinsMap[structureList[i]];
                        int[] binVoxels = new int[doseBins.Length];
                        double[] doseResponseCounts = new double[doseBins.Length - 1];
                        double[] functionalResponseCounts = new double[doseBins.Length - 1];
                        double intensityDamage = 0.0;
                        double functionalDamage = 0.0;
                        int low2high = 0;
                        int low2mid = 0;
                        int mid2high = 0;
                        int mid2low = 0;
                        int high2mid = 0;
                        int high2low = 0;
                        int low2low = 0;
                        int mid2mid = 0;
                        int high2high = 0;
                        int totalVoxels = 0;

                        for (int z = 0; z < zCount; z++)
                        {
                            for (int y = 0; y < yCount; y++)
                            {
                                for (int x = 0; x < xCount; x++)
                                {
                                    if (!double.IsNaN(baseSPECT[x, y, z]) && !double.IsNaN(compSPECT[x, y, z]))
                                    {
                                        double differential = (compSPECT[x, y, z] - baseSPECT[x, y, z]);
                                        intensityDamage += differential;
                                        points[x, y, z] = differential;
                                        double functionalDifferential = 0.0;
                                        totalVoxels++;
                                        if (baseSPECT[x, y, z] >= baseMax * 0.7 && compSPECT[x, y, z] < compMax * 0.15)
                                        {
                                            functionalDifferential = -2;
                                            high2low++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.15 && compSPECT[x, y, z] < compMax * 0.7)
                                        {
                                            functionalDifferential = -1;
                                            high2mid++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.15 && baseSPECT[x, y, z] < baseMax * 0.7 && compSPECT[x, y, z] < compMax * 0.15)
                                        {
                                            functionalDifferential = -1;
                                            mid2low++;
                                        }
                                        else if (baseSPECT[x, y, z] < baseMax * 0.15 && compSPECT[x, y, z] >= compMax * 0.15 && compSPECT[x, y, z] < compMax * 0.7)
                                        {
                                            functionalDifferential = 1;
                                            low2mid++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.15 && baseSPECT[x, y, z] < baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.7)
                                        {
                                            functionalDifferential = 1;
                                            mid2high++;
                                        }
                                        else if (baseSPECT[x, y, z] < baseMax * 0.15 && compSPECT[x, y, z] >= compMax * 0.7)
                                        {
                                            functionalDifferential = 2;
                                            low2high++;
                                        }
                                        else if (baseSPECT[x, y, z] < baseMax * 0.15 && compSPECT[x, y, z] < compMax * 0.15)
                                        {
                                            low2low++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.15 && baseSPECT[x, y, z] < baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.15 && compSPECT[x, y, z] < compMax * 0.7)
                                        {
                                            mid2mid++;
                                        }
                                        else if (baseSPECT[x, y, z] >= baseMax * 0.7 && compSPECT[x, y, z] >= compMax * 0.7)
                                        {
                                            high2high++;
                                        }
                                        functionalDamage += functionalDifferential;

                                        for (int k = 0; k < doseBins.Length - 1; k++)
                                        {
                                            if (dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                            {
                                                doseResponseCounts[k] += differential;
                                                functionalResponseCounts[k] += functionalDifferential;
                                                binVoxels[k]++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        points[x, y, z] = double.NaN;
                                    }
                                }
                            }
                        }
                        double h2l = (double)high2low / totalVoxels;
                        double h2m = (double)high2mid / totalVoxels;
                        double m2l = (double)mid2low / totalVoxels;
                        double m2h = (double)mid2high / totalVoxels;
                        double l2m = (double)low2mid / totalVoxels;
                        double l2h = (double)low2high / totalVoxels;
                        double l2l = (double)low2low / totalVoxels;
                        double m2m = (double)mid2mid / totalVoxels;
                        double h2h = (double)high2high / totalVoxels;
                        double[] functionalChangeSave = new double[] { h2l, h2m, m2l, m2h, l2m, l2h, l2l, m2m, h2h };

                        doseResponse.Add(structureList[i], doseResponseCounts);
                        functionalChangeMetrics.Add(structureList[i], functionalChangeSave);
                        functionalResponse.Add(structureList[i], functionalResponseCounts);
                        responseVoxels.Add(structureList[i], binVoxels);
                        pointsDictionary.Add(structureList[i], points);
                        totalDamage.Add(structureList[i], intensityDamage);
                        totalFunctionalDamage.Add(structureList[i], functionalDamage);
                    }
                    doseResponseDictionary.Add(ventTimepoints[j], doseResponse);
                    functionalChangeMetricsDictionary.Add(ventTimepoints[j], functionalChangeMetrics);
                    functionalResponseDictionary.Add(ventTimepoints[j], functionalResponse);
                    voxelsDictionary.Add(ventTimepoints[j], responseVoxels);
                    doseResponsePoints.Add(ventTimepoints[j], pointsDictionary);
                    TotalDamageDictionary.Add(ventTimepoints[j], totalDamage);
                    TotalFunctionalDamageDictionary.Add(ventTimepoints[j], totalFunctionalDamage);
                }
            }
            else
            {
                doseResponseDictionary = null;
                functionalChangeMetricsDictionary = null;
                functionalResponseDictionary = null;
                voxelsDictionary = null;
                doseResponsePoints = null;
                TotalDamageDictionary = null;
                TotalFunctionalDamageDictionary = null;
            }

        }
    }
}
