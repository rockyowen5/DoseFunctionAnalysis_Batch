using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace DFHAnalysis
{
    class MetricAnalysis
    {
        // Global Variables
        private static Dictionary<string, Dictionary<string, double[]>> v_NameMetricDictionary = null;
        public static Dictionary<string, Dictionary<string, double[]>> NameMetricDictionary
        {
            get { return v_NameMetricDictionary; }
            set { v_NameMetricDictionary = value; }
        }
        private static Dictionary<string, List<VVector>> v_MaxDoseLocations = null;
        public static Dictionary<string, List<VVector>> MaxDoseLocations
        {
            get { return v_MaxDoseLocations; }
            set { v_MaxDoseLocations = value; }
        }
        private static Dictionary<string, List<VVector>> v_MaxIntensityLocations = null;
        public static Dictionary<string, List<VVector>> MaxIntensityLocations
        {
            get { return v_MaxIntensityLocations; }
            set { v_MaxIntensityLocations = value; }
        }
        private static Dictionary<string, int> v_StructureVariables = null;
        public static Dictionary<string, int> StructureVariables
        {
            get { return v_StructureVariables; }
            set { v_StructureVariables = value; }
        }
        private static Dictionary<string, double[][]> v_PlanCumulativeCounts = null;
        public static Dictionary<string, double[][]> PlanCumulativeCounts
        {
            get { return v_PlanCumulativeCounts; }
            set { v_PlanCumulativeCounts = value; }
        }
        private static Dictionary<string, double[]> v_PlanDoseBins = null;
        public static Dictionary<string, double[]> PlanDoseBins
        {
            get { return v_PlanDoseBins; }
            set { v_PlanDoseBins = value; }
        }
        private static Dictionary<string, int> v_PlanBinNumber = null;
        public static Dictionary<string, int> PlanBinNumber
        {
            get { return v_PlanBinNumber; }
            set { v_PlanBinNumber = value; }
        }
        private static Dictionary<string, double[]> v_CountsUnderCurve = null;
        public static Dictionary<string, double[]> CountsUnderCurve
        {
            get { return v_CountsUnderCurve; }
            set { v_CountsUnderCurve = value; }
        }

        // Local Variables
        private double volume;
        private double maxDose;
        private double meanDose;
        private double stdvDose;
        private double maxIntensity;
        private double meanIntensity;
        private double stdvIntensity;
        private int[] doseSize;
        private double[,,] structureDoseData;
        private double[,,] structureImageData;
        private double[] doseOrigin;
        private double[] doseRes;
        private int structureImageVoxels;
        private int structureDoseVoxels;
        private double imageSum;
        private double stdvDoseSum;
        private double stdvImageSum;
        private double[] structureMetricValues;
        private string name;
        private Dose patientDose;
        private string structureIdentifier;
        private Structure currentStructure;
        private int structureCount;
        private StructureSet structureSet;
        private string metricName;
        private ListViewItem metricItem;
        private ListViewGroup metricGroup;
        private VVector maxDoseLocation;
        private VVector maxIntensityLocation;
        private double percentImageFOV;
        private Registration planRegistration;
        private Dictionary<string, double> structureMetricDictionary;
        private Dictionary<string, double[]> metricValueDictionary;
        private List<VVector> maxDoseLocationList;
        private List<VVector> maxIntensityLocationList;
        private int binNumber;
        private double[] doseBins;
        private double maximumPlanDose;
        private double[][] cumulativeDFCounts;
        private double[] structureCumulativeDFCount;
        private double[] countsUnderCurve;
        private Image patientSPECT;
        private string useLQCorrection;
        private int fractionNumber;
        private double alphaBetaValue;
        private double intensityNormalizer;
        private double doseNormalizer;
        private double gEUD;
        private double gEUfD;
        private double aValue;
        private double voxelVolume;
        private int updatedBinNumber;
        private double fV20;
        private double V20;
        private double MfLD;
        private string imageName;

        // Analyze metric values.
        public void Analyze()
        {
            PlanBinNumber = new Dictionary<string, int>();
            PlanDoseBins = new Dictionary<string, double[]>();
            PlanCumulativeCounts = new Dictionary<string, double[][]>();
            CountsUnderCurve = new Dictionary<string, double[]>();
            NameMetricDictionary = new Dictionary<string, Dictionary<string, double[]>>();
            structureCount = UserInterface.SelectedStructures.Count;
            MaxDoseLocations = new Dictionary<string, List<VVector>>();
            MaxIntensityLocations = new Dictionary<string, List<VVector>>();

            for (int j = 0; j < UserInterface.LoadedPlans; j++)
            {
                name = LoadDosePlan.NameDictionary[j];
                patientDose = LoadDosePlan.DosePlans[j];
                patientSPECT = SelectSPECTImage.SPECTDictionary[j];
                structureSet = LoadDosePlan.StructureSets[name];
                planRegistration = LoadDosePlan.RegistrationDictionary[name];
                doseSize = new int[] { patientDose.XSize, patientDose.YSize, patientDose.ZSize };
                doseOrigin = new double[] { patientDose.Origin.x, patientDose.Origin.y, patientDose.Origin.z };
                doseRes = new double[] { patientDose.XRes, patientDose.YRes, patientDose.ZRes };
                metricValueDictionary = new Dictionary<string, double[]>();
                maxDoseLocationList = new List<VVector>();
                maxIntensityLocationList = new List<VVector>();
                StructureVariables = new Dictionary<string, int>();
                cumulativeDFCounts = new double[structureCount][];
                countsUnderCurve = new double[structureCount];
                maximumPlanDose = LoadDosePlan.MaximumDose[name];
                binNumber = Convert.ToInt32(Math.Ceiling(maximumPlanDose));
                useLQCorrection = SelectBioCorrection.BioCorrectDictionary[j];
                fractionNumber = LoadDosePlan.FractionDictionary[name];
                voxelVolume = doseRes[0] * doseRes[1] * doseRes[2];

                Normalizer newNormalizer = new Normalizer();
                if (SelectOptions.NormalizeStrategy == "Structure")
                {
                    newNormalizer.StructureRelative(patientDose, patientSPECT, planRegistration, structureSet, SelectOptions.NormalizeStructure, name);
                    intensityNormalizer = newNormalizer.intensityNormalizer;
                    doseNormalizer = newNormalizer.doseNormalizer;
                }
                else if (SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold")
                {
                    newNormalizer.ThresholdRelative(patientDose, patientSPECT, planRegistration, structureSet, SelectOptions.DoseThreshold, name);
                    intensityNormalizer = newNormalizer.intensityNormalizer;
                    doseNormalizer = newNormalizer.doseNormalizer;
                }

                doseBins = new double[binNumber + 1];
                for (int i = 0; i < binNumber + 1; i++)
                {
                    doseBins[i] = i;
                }

                for (int s = 0; s < structureCount; s++)
                {
                    //doseLimit = 20.0;
                    percentImageFOV = 0.0;
                    maxDose = 0.0;
                    maxIntensity = 0.0;
                    structureImageVoxels = 0;
                    structureDoseVoxels = 0;
                    imageSum = 0.0;
                    stdvDoseSum = 0.0;
                    stdvImageSum = 0.0;
                    gEUD = 0.0;
                    gEUfD = 0.0;
                    fV20 = 0.0;
                    V20 = 0.0;
                    MfLD = 0.0;
                    structureCumulativeDFCount = new double[binNumber + 1];
                    maxDoseLocation = new VVector();
                    maxIntensityLocation = new VVector();
                    VVector doseStart = new VVector();
                    VVector doseStop = new VVector();
                    VVector imageStart = new VVector();
                    VVector imageStop = new VVector();
                    structureIdentifier = UserInterface.SelectedStructures[s];
                    currentStructure = structureSet.Structures.First(w => w.Id == structureIdentifier);
                    MeshGeometry3D structureMesh = currentStructure.MeshGeometry;
                    Rect3D structureBox = currentStructure.MeshGeometry.Bounds;
                    Point3D structureLocation = structureBox.Location;
                    Size3D boxSize = structureBox.Size;
                    StructureVariables.Add(structureIdentifier, s);
                    volume = Math.Round(currentStructure.Volume, 2);
                    aValue = UserInterface.AValueDictionary[currentStructure.Id];

                    int xcount = (int)Math.Ceiling((boxSize.X / doseRes[0]));
                    int ycount = (int)Math.Ceiling((boxSize.Y / doseRes[1]));
                    int zcount = (int)Math.Ceiling((boxSize.Z / doseRes[2]));

                    double xstart = Math.Ceiling((structureLocation.X - doseOrigin[0]) / doseRes[0]) * doseRes[0] + doseOrigin[0];
                    double ystart = Math.Ceiling((structureLocation.Y - doseOrigin[1]) / doseRes[1]) * doseRes[1] + doseOrigin[1];
                    double zstart = Math.Ceiling((structureLocation.Z - doseOrigin[2]) / doseRes[2]) * doseRes[2] + doseOrigin[2];

                    structureDoseData = new double[xcount, ycount, zcount];
                    structureImageData = new double[xcount, ycount, zcount];

                    if (useLQCorrection == "No")
                    {
                        for (int z = 0; z < zcount; z++)
                        {
                            for (int y = 0; y < ycount; y++)
                            {
                                doseStart.x = xstart;
                                doseStart.y = ystart + y * doseRes[1];
                                doseStart.z = zstart + z * doseRes[2];
                                doseStop.x = doseStart.x + (xcount - 1) * doseRes[0];
                                doseStop.y = doseStart.y;
                                doseStop.z = doseStart.z;
                                imageStart = planRegistration.TransformPoint(doseStart);
                                imageStop = planRegistration.TransformPoint(doseStop);
                                BitArray structureBitArray = new BitArray(xcount);
                                SegmentProfile segmentProfile = currentStructure.GetSegmentProfile(doseStart, doseStop, structureBitArray);
                                double[] doseArray = new double[xcount];
                                DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                                double[] imageArray = new double[xcount];
                                ImageProfile imageProfile = patientSPECT.GetImageProfile(imageStart, imageStop, imageArray);
                                for (int x = 0; x < xcount; x++)
                                {
                                    if (segmentProfile[x].Value == true)
                                    {
                                        if (!Double.IsNaN(imageProfile[x].Value))
                                        {
                                            structureImageVoxels++;
                                            if (SelectOptions.IntensityNormalize == "Relative" && (SelectOptions.NormalizeStrategy == "Structure" || SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold"))
                                            {
                                                structureImageData[x, y, z] = imageProfile[x].Value / intensityNormalizer;
                                            }
                                            else
                                            {
                                                structureImageData[x, y, z] = imageProfile[x].Value;
                                            }
                                            meanIntensity += structureImageData[x, y, z];
                                            imageSum += structureImageData[x, y, z];
                                            if (structureImageData[x, y, z] > maxIntensity)
                                            {
                                                maxIntensity = structureImageData[x, y, z];
                                                maxIntensityLocation = segmentProfile[x].Position;
                                            }

                                            structureDoseVoxels++;
                                            if (SelectOptions.DoseNormalize == "Relative" && (SelectOptions.NormalizeStrategy == "Structure" || SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold"))
                                            {
                                                structureDoseData[x, y, z] = doseProfile[x].Value / doseNormalizer;
                                            }
                                            else
                                            {
                                                structureDoseData[x, y, z] = doseProfile[x].Value;
                                            }
                                            gEUD += voxelVolume * Math.Pow(structureDoseData[x, y, z], aValue);
                                            gEUfD += structureImageData[x, y, z] * Math.Pow(structureDoseData[x, y, z], aValue);
                                            meanDose += structureDoseData[x, y, z];
                                            MfLD += structureImageData[x, y, z] * structureDoseData[x, y, z];
                                            if (structureDoseData[x, y, z] > maxDose)
                                            {
                                                maxDose = structureDoseData[x, y, z];
                                                maxDoseLocation = segmentProfile[x].Position;
                                            }
                                            if (structureDoseData[x,y,z] > 20.0)
                                            {
                                                fV20 += structureImageData[x, y, z];
                                                V20++;
                                            }

                                            for (int b = 0; b < binNumber + 1; b++)
                                            {
                                                if (structureDoseData[x, y, z] >= doseBins[b])
                                                {
                                                    structureCumulativeDFCount[b] += structureImageData[x, y, z];
                                                }
                                            }
                                        }
                                        else if (Double.IsNaN(imageProfile[x].Value))
                                        {
                                            structureImageData[x, y, z] = double.NaN;

                                            structureDoseVoxels++;
                                            if (SelectOptions.DoseNormalize == "Relative" && (SelectOptions.NormalizeStrategy == "Structure" || SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold"))
                                            {
                                                structureDoseData[x, y, z] = doseProfile[x].Value / doseNormalizer;
                                            }
                                            else
                                            {
                                                structureDoseData[x, y, z] = doseProfile[x].Value;
                                            }
                                            gEUD += voxelVolume * Math.Pow(structureDoseData[x, y, z], aValue);
                                            meanDose += structureDoseData[x, y, z];
                                            if (structureDoseData[x, y, z] > maxDose)
                                            {
                                                maxDose = structureDoseData[x, y, z];
                                                maxDoseLocation = segmentProfile[x].Position;
                                            }
                                            if (structureDoseData[x,y,z] > 20.0)
                                            {
                                                V20++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        structureDoseData[x, y, z] = double.NaN;
                                        structureImageData[x, y, z] = double.NaN;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        alphaBetaValue = UserInterface.AlphaBetaDictionary[currentStructure.Id];
                        for (int z = 0; z < zcount; z++)
                        {
                            for (int y = 0; y < ycount; y++)
                            {
                                doseStart.x = xstart;
                                doseStart.y = ystart + y * doseRes[1];
                                doseStart.z = zstart + z * doseRes[2];
                                doseStop.x = doseStart.x + (xcount - 1) * doseRes[0];
                                doseStop.y = doseStart.y;
                                doseStop.z = doseStart.z;
                                imageStart = planRegistration.TransformPoint(doseStart);
                                imageStop = planRegistration.TransformPoint(doseStop);
                                BitArray structureBitArray = new BitArray(xcount);
                                SegmentProfile segmentProfile = currentStructure.GetSegmentProfile(doseStart, doseStop, structureBitArray);
                                double[] doseArray = new double[xcount];
                                DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                                double[] imageArray = new double[xcount];
                                ImageProfile imageProfile = patientSPECT.GetImageProfile(imageStart, imageStop, imageArray);
                                for (int x = 0; x < xcount; x++)
                                {
                                    if (segmentProfile[x].Value == true)
                                    {
                                        double pointDose;
                                        if (SelectOptions.DoseNormalize == "Relative" && (SelectOptions.NormalizeStrategy == "Structure" || SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold"))
                                        {
                                            pointDose = (doseProfile[x].Value / doseNormalizer) * (((doseProfile[x].Value / doseNormalizer) / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                        }
                                        else
                                        {
                                            pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                        }
                                        if (!Double.IsNaN(imageProfile[x].Value))
                                        {
                                            structureImageVoxels++;
                                            if (SelectOptions.IntensityNormalize == "Relative" && (SelectOptions.NormalizeStrategy == "Structure" || SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold"))
                                            {
                                                structureImageData[x, y, z] = imageProfile[x].Value / intensityNormalizer;
                                            }
                                            else
                                            {
                                                structureImageData[x, y, z] = imageProfile[x].Value;
                                            }
                                            meanIntensity += structureImageData[x, y, z];
                                            imageSum += structureImageData[x, y, z];
                                            if (structureImageData[x, y, z] > maxIntensity)
                                            {
                                                maxIntensity = structureImageData[x, y, z];
                                                maxIntensityLocation = segmentProfile[x].Position;
                                            }

                                            structureDoseVoxels++;
                                            structureDoseData[x, y, z] = pointDose;
                                            gEUD += voxelVolume * Math.Pow(structureDoseData[x, y, z], aValue);
                                            gEUfD += structureImageData[x, y, z] * Math.Pow(structureDoseData[x, y, z], aValue);
                                            meanDose += structureDoseData[x, y, z];
                                            MfLD += structureImageData[x, y, z] * structureDoseData[x, y, z];
                                            if (structureDoseData[x, y, z] > maxDose)
                                            {
                                                maxDose = structureDoseData[x, y, z];
                                                maxDoseLocation = segmentProfile[x].Position;
                                            }
                                            if (structureDoseData[x, y, z] > 20.0)
                                            {
                                                fV20 += structureImageData[x, y, z];
                                                V20++;
                                            }

                                            for (int b = 0; b < binNumber + 1; b++)
                                            {
                                                if (structureDoseData[x, y, z] >= doseBins[b])
                                                {
                                                    structureCumulativeDFCount[b] += structureImageData[x, y, z];
                                                }
                                            }
                                        }
                                        else if (Double.IsNaN(imageProfile[x].Value))
                                        {
                                            structureImageData[x, y, z] = double.NaN;

                                            structureDoseVoxels++;
                                            structureDoseData[x, y, z] = pointDose;
                                            gEUD += voxelVolume * Math.Pow(structureDoseData[x, y, z], aValue);
                                            meanDose += structureDoseData[x, y, z];
                                            if (structureDoseData[x, y, z] > maxDose)
                                            {
                                                maxDose = structureDoseData[x, y, z];
                                                maxDoseLocation = segmentProfile[x].Position;
                                            }
                                            if (structureDoseData[x,y,z] > 20.0)
                                            {
                                                V20++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        structureDoseData[x, y, z] = double.NaN;
                                        structureImageData[x, y, z] = double.NaN;
                                    }
                                }
                            }
                        }
                    }


                    if (SelectOptions.IntensityNormalize == "Relative")
                    {
                        if (SelectOptions.NormalizeStrategy == "Total Counts/Maximum Dose")
                        {
                            for (int b = 0; b < binNumber + 1; b++)
                            {
                                structureCumulativeDFCount[b] = structureCumulativeDFCount[b] / imageSum * 100;
                                countsUnderCurve[s] = countsUnderCurve[s] + structureCumulativeDFCount[b];
                            }
                        }
                        else if (SelectOptions.NormalizeStrategy == "Structure")
                        {
                            double dfCountsMax = structureCumulativeDFCount[0];
                            for (int b = 0; b < binNumber + 1; b++)
                            {
                                structureCumulativeDFCount[b] = structureCumulativeDFCount[b] / dfCountsMax * 100;
                                countsUnderCurve[s] += structureCumulativeDFCount[b];
                            }
                        }
                        else if (SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold")
                        {
                            double dfCountsMax = structureCumulativeDFCount[0];
                            for (int b = 0; b < binNumber + 1; b++)
                            {
                                structureCumulativeDFCount[b] = structureCumulativeDFCount[b] / dfCountsMax * 100;
                                countsUnderCurve[s] += structureCumulativeDFCount[b];
                            }
                        }
                    }
                    else
                    {
                        for (int b = 0; b < binNumber + 1; b++)
                        {
                            countsUnderCurve[s] += structureCumulativeDFCount[b];
                        }
                    }
                    cumulativeDFCounts[s] = structureCumulativeDFCount;

                    maxDoseLocationList.Add(maxDoseLocation);
                    maxIntensityLocationList.Add(maxIntensityLocation);

                    percentImageFOV = Math.Round((double)structureImageVoxels / (double)structureDoseVoxels * 100, 2);
                    if (percentImageFOV < 100)
                    {
                        imageName = name.Substring(0, 11);
                        MessageBox.Show("Warning: " + structureIdentifier + " is outside of the " + imageName + " image field of view." +
                            " Only ~" + percentImageFOV.ToString() + "% of the structure will be analyzed for intensity metrics.", 
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    meanDose /= structureDoseVoxels;
                    meanIntensity /= structureImageVoxels;

                    for (int z = 0; z < zcount; z++)
                    {
                        for (int y = 0; y < ycount; y++)
                        {
                            for (int x = 0; x < xcount; x++)
                            {
                                if (double.IsNaN(structureDoseData[x, y, z]) == false)
                                {
                                    stdvDoseSum += Math.Pow(structureDoseData[x, y, z] - meanDose, 2);
                                }
                                if (double.IsNaN(structureImageData[x, y, z]) == false)
                                {
                                    stdvImageSum += Math.Pow(structureImageData[x, y, z] - meanIntensity, 2);
                                }
                            }
                        }
                    }
                    maxDose = Math.Round(maxDose, 2);
                    updatedBinNumber = Math.Max(binNumber, Convert.ToInt16(Math.Ceiling(maxDose)));
                    maxIntensity = Math.Round(maxIntensity, 2);
                    meanDose = Math.Round(meanDose, 2);
                    meanIntensity = Math.Round(meanIntensity, 2);
                    stdvDose = Math.Round(Math.Sqrt(stdvDoseSum / structureDoseVoxels), 2);
                    stdvIntensity = Math.Round(Math.Sqrt(stdvImageSum / structureImageVoxels), 2);
                    fV20 = Math.Round((fV20 / imageSum * 100), 2);
                    V20 = Math.Round((V20 / structureDoseVoxels * 100), 2);
                    gEUD /= (voxelVolume * structureDoseVoxels);
                    gEUD = Math.Round(Math.Pow(gEUD, 1 / aValue), 2);
                    gEUfD /= imageSum;
                    gEUfD = Math.Round(Math.Pow(gEUfD, 1 / aValue), 2);
                    MfLD /= imageSum;
                    MfLD = Math.Round(MfLD, 2);

                    structureMetricValues = new double[] { percentImageFOV, volume, maxDose, meanDose, stdvDose, gEUD, V20, maxIntensity, meanIntensity, stdvIntensity, gEUfD, fV20, MfLD };
                    metricValueDictionary.Add(currentStructure.Id, structureMetricValues);
                    structureMetricDictionary = new Dictionary<string, double>();
                    structureMetricDictionary.Add("Volume[cm\xB3]", volume);
                    structureMetricDictionary.Add("Max Dose[Gy]", maxDose);
                    structureMetricDictionary.Add("Mean Dose[Gy]", meanDose);
                    structureMetricDictionary.Add("StDv Dose[Gy]", stdvDose);
                    structureMetricDictionary.Add("gEUD[Gy]", gEUD);
                    structureMetricDictionary.Add("V20[%]", V20);
                    structureMetricDictionary.Add("Max Intensity", maxIntensity);
                    structureMetricDictionary.Add("Mean Intensity", meanIntensity);
                    structureMetricDictionary.Add("StDv Intensity", stdvIntensity);
                    structureMetricDictionary.Add("gEUfD", gEUfD);
                    structureMetricDictionary.Add("fV20[%]", fV20);
                    structureMetricDictionary.Add("MfLD[Gy]", MfLD);
                    double metricValue;
                    for (int i = 0; i < UserInterface.Metrics.Count(); i++)
                    {
                        metricName = UserInterface.Metrics[i];
                        metricGroup = UserInterface.ListView1.Groups[currentStructure.Id];
                        if (metricGroup.Items.ContainsKey(metricName) == false)
                        {
                            metricItem = UserInterface.ListView1.Items.Add(metricName);
                            metricItem.Name = metricName;
                            metricItem.Group = metricGroup;
                            metricItem.SubItems.Add("");
                            metricValue = structureMetricDictionary[metricName];
                            metricItem.SubItems[1].Text = metricValue.ToString(); ;
                        }
                        else
                        {
                            int index = UserInterface.ListView1.Items.IndexOfKey(metricName);
                            metricItem = metricGroup.Items[index];
                            metricItem.SubItems.Add("");
                            metricValue = structureMetricDictionary[metricName];
                            int subitemIndex = UserInterface.ListView1.Items[index].SubItems.Count - 1;
                            metricItem.SubItems[subitemIndex].Text = metricValue.ToString();
                        }
                    }
                }
                if (SelectOptions.DoseNormalize == "Relative")
                {
                    for (int i = 0; i < binNumber + 1; i++)
                    {
                        doseBins[i] = Math.Round((doseBins[i] / updatedBinNumber * 100), 2);
                    }
                }

                NameMetricDictionary.Add(name, metricValueDictionary);
                PlanBinNumber.Add(name, updatedBinNumber);
                PlanDoseBins.Add(name, doseBins);
                PlanCumulativeCounts.Add(name, cumulativeDFCounts);
                CountsUnderCurve.Add(name, countsUnderCurve);
                MaxDoseLocations.Add(name, maxDoseLocationList);
                MaxIntensityLocations.Add(name, maxIntensityLocationList);
            }

            ExcelWrite writeData = new ExcelWrite();
            writeData.ExcelWriteData();

        }

    }
}
