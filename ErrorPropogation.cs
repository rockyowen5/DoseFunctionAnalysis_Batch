using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFH_Initiate
{
    class ErrorPropogation
    {
        public static Dictionary<string, Dictionary<string, double[]>> DoseIntensityUncertainty = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> doseIntensityErrorDictionary;

        public static Dictionary<string, Dictionary<string, double[]>> DoseResponseUncertainty = new Dictionary<string, Dictionary<string, double[]>>();
        private Dictionary<string, double[]> doseResponseErrorDictionary;


        double[] doseIntensityError;
        double[] doseResponseError;
        int[] voxels;
        string[] structureList = new string[] { DataProcess.lungID, DataProcess.rightLungID, DataProcess.leftLungID };
        public void DoseIntensityError()
        {
            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;
            for (int i = 0; i < perfTimepoints.Count; i++)
            {
                Dictionary<string, double[,,]> intensityDictionary = DataProcess.FunctionalMaps[perfTimepoints[i]];
                Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTimepoints[i]];
                Dictionary<string, double[]> doseIntensityDictionary = DataProcess.DoseIntensity[perfTimepoints[i]];
                doseIntensityErrorDictionary = new Dictionary<string, double[]>();
                for (int j = 0; j < structureList.Length; j++)
                {
                    double[] binAvgs = doseIntensityDictionary[structureList[j]];
                    doseIntensityError = new double[binAvgs.Length];
                    voxels = new int[binAvgs.Length];
                    double[] doseBins = DataProcess.doseBinsMap[structureList[j]];
                    double[,,] intensity = intensityDictionary[structureList[j]];
                    double[,,] dose = doseDictionary[structureList[j]];
                    int xCount = intensity.GetLength(0);
                    int yCount = intensity.GetLength(1);
                    int zCount = intensity.GetLength(2);

                    for (int z = 0; z < zCount; z++)
                    {
                        for (int y = 0; y < yCount; y++)
                        {
                            for (int x = 0; x < xCount; x++)
                            {
                                for (int k = 0; k < doseBins.Length; k++)
                                {
                                    if (!double.IsNaN(intensity[x, y, z]) && dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                    {
                                        doseIntensityError[k] += Math.Pow((intensity[x, y, z] - binAvgs[k]), 2);
                                        voxels[k]++;
                                    }
                                }
                            }
                        }
                    }
                    for (int k = 0; k < binAvgs.Length; k++)
                    {
                        doseIntensityError[k] /= (voxels[k] - 1);
                        doseIntensityError[k] = Math.Sqrt((doseIntensityError[k] / voxels[k]));
                    }
                    doseIntensityErrorDictionary.Add(structureList[j], doseIntensityError);
                }
                DoseIntensityUncertainty.Add(perfTimepoints[i], doseIntensityErrorDictionary);
            }

            for (int i = 0; i < ventTimepoints.Count; i++)
            {
                Dictionary<string, double[,,]> intensityDictionary = DataProcess.FunctionalMaps[ventTimepoints[i]];
                Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTimepoints[i]];
                Dictionary<string, double[]> doseIntensityDictionary = DataProcess.DoseIntensity[ventTimepoints[i]];
                doseIntensityErrorDictionary = new Dictionary<string, double[]>();
                for (int j = 0; j < structureList.Length; j++)
                {
                    double[] binAvgs = doseIntensityDictionary[structureList[j]];
                    doseIntensityError = new double[binAvgs.Length];
                    voxels = new int[binAvgs.Length];
                    double[] doseBins = DataProcess.doseBinsMap[structureList[j]];
                    double[,,] intensity = intensityDictionary[structureList[j]];
                    double[,,] dose = doseDictionary[structureList[j]];
                    int xCount = intensity.GetLength(0);
                    int yCount = intensity.GetLength(1);
                    int zCount = intensity.GetLength(2);

                    for (int z = 0; z < zCount; z++)
                    {
                        for (int y = 0; y < yCount; y++)
                        {
                            for (int x = 0; x < xCount; x++)
                            {
                                for (int k = 0; k < doseBins.Length; k++)
                                {
                                    if (!double.IsNaN(intensity[x, y, z]) && dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                    {
                                        doseIntensityError[k] += Math.Pow((intensity[x, y, z] - binAvgs[k]), 2);
                                        voxels[k]++;
                                    }
                                }
                            }
                        }
                    }
                    for (int k = 0; k < binAvgs.Length; k++)
                    {
                        doseIntensityError[k] /= (voxels[k] - 1);
                        doseIntensityError[k] = Math.Sqrt((doseIntensityError[k] / voxels[k]));
                    }
                    doseIntensityErrorDictionary.Add(structureList[j], doseIntensityError);
                }
                DoseIntensityUncertainty.Add(ventTimepoints[i], doseIntensityErrorDictionary);
            }
        }

        public void FunctionalChangeError()
        {
            List<string> perfTimepoints = LoadAllImages.perfTimepoints;
            List<string> ventTimepoints = LoadAllImages.ventTimepoints;
            for (int i = 0; i < perfTimepoints.Count; i++)
            {
                if (FunctionalChange.doseResponseDictionary != null)
                {
                    Dictionary<string, double[,,]> pointsDictionary = FunctionalChange.doseResponsePoints[perfTimepoints[i]];
                    Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTimepoints[i]];
                    Dictionary<string, double[]> doseResponseDictionary = FunctionalChange.doseResponseDictionary[perfTimepoints[i]];
                    Dictionary<string, int[]> voxelsDictionary = FunctionalChange.voxelsDictionary[perfTimepoints[i]];
                    doseResponseErrorDictionary = new Dictionary<string, double[]>();
                    for (int j = 0; j < structureList.Length; j++)
                    {
                        int[] voxels = voxelsDictionary[structureList[j]];
                        double[] binAvgs = doseResponseDictionary[structureList[j]];
                        for (int a = 0; a < binAvgs.Length; a++)
                        {
                            binAvgs[a] /= voxels[a];
                        }
                        doseResponseError = new double[binAvgs.Length];
                        voxels = new int[binAvgs.Length];
                        double[] doseBins = DataProcess.doseBinsMap[structureList[j]];
                        double[,,] points = pointsDictionary[structureList[j]];
                        double[,,] dose = doseDictionary[structureList[j]];
                        int xCount = points.GetLength(0);
                        int yCount = points.GetLength(1);
                        int zCount = points.GetLength(2);

                        for (int z = 0; z < zCount; z++)
                        {
                            for (int y = 0; y < yCount; y++)
                            {
                                for (int x = 0; x < xCount; x++)
                                {
                                    for (int k = 0; k < doseBins.Length; k++)
                                    {
                                        if (!double.IsNaN(points[x, y, z]) && dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                        {
                                            doseResponseError[k] += Math.Pow((points[x, y, z] - binAvgs[k]), 2);
                                            voxels[k]++;
                                        }
                                    }
                                }
                            }
                        }
                        for (int k = 0; k < binAvgs.Length; k++)
                        {
                            doseResponseError[k] /= (voxels[k] - 1);
                            doseResponseError[k] = Math.Sqrt(doseResponseError[k]);
                            //doseResponseError[k] = Math.Sqrt((doseResponseError[k] / voxels[k]));
                        }
                        doseResponseErrorDictionary.Add(structureList[j], doseResponseError);
                    }
                    DoseResponseUncertainty.Add(perfTimepoints[i], doseResponseErrorDictionary);
                }
                else
                {
                    DoseResponseUncertainty = null;
                }
            }

            for (int i = 0; i < ventTimepoints.Count; i++)
            {
                if (FunctionalChange.doseResponseDictionary != null)
                {
                    Dictionary<string, double[,,]> pointsDictionary = FunctionalChange.doseResponsePoints[ventTimepoints[i]];
                    Dictionary<string, double[,,]> doseDictionary = DataProcess.DoseMaps[perfTimepoints[i]];
                    Dictionary<string, double[]> doseResponseDictionary = FunctionalChange.doseResponseDictionary[ventTimepoints[i]];
                    Dictionary<string, int[]> voxelsDictionary = FunctionalChange.voxelsDictionary[perfTimepoints[i]];
                    doseResponseErrorDictionary = new Dictionary<string, double[]>();
                    for (int j = 0; j < structureList.Length; j++)
                    {
                        int[] voxels = voxelsDictionary[structureList[j]];
                        double[] binAvgs = doseResponseDictionary[structureList[j]];
                        for (int a = 0; a < binAvgs.Length; a++)
                        {
                            binAvgs[a] /= voxels[a];
                        }
                        doseResponseError = new double[binAvgs.Length];
                        voxels = new int[binAvgs.Length];
                        double[] doseBins = DataProcess.doseBinsMap[structureList[j]];
                        double[,,] points = pointsDictionary[structureList[j]];
                        double[,,] dose = doseDictionary[structureList[j]];
                        int xCount = points.GetLength(0);
                        int yCount = points.GetLength(1);
                        int zCount = points.GetLength(2);

                        for (int z = 0; z < zCount; z++)
                        {
                            for (int y = 0; y < yCount; y++)
                            {
                                for (int x = 0; x < xCount; x++)
                                {
                                    for (int k = 0; k < doseBins.Length; k++)
                                    {
                                        if (!double.IsNaN(points[x, y, z]) && dose[x, y, z] >= doseBins[k] && dose[x, y, z] < doseBins[k + 1])
                                        {
                                            doseResponseError[k] += Math.Pow((points[x, y, z] - binAvgs[k]), 2);
                                            voxels[k]++;
                                        }
                                    }
                                }
                            }
                        }
                        for (int k = 0; k < binAvgs.Length; k++)
                        {
                            doseResponseError[k] /= (voxels[k] - 1);
                            doseResponseError[k] = Math.Sqrt(doseResponseError[k]);
                        }
                        doseResponseErrorDictionary.Add(structureList[j], doseResponseError);
                    }
                    DoseResponseUncertainty.Add(ventTimepoints[i], doseResponseErrorDictionary);
                }
                else
                {
                    DoseResponseUncertainty = null;
                }
            }
        }
    }
}
