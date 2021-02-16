using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFHAnalysis
{
    class Normalizer
    {
        private static Dictionary<string, double[]> v_StructureNormalizationValue = new Dictionary<string, double[]>();
        public static Dictionary<string, double[]> StructureNormalizationValue
        {
            get { return v_StructureNormalizationValue; }
            set { v_StructureNormalizationValue = value; }
        }
        private static Dictionary<string, double[]> v_AverageUnderDoseThreshold = new Dictionary<string, double[]>();
        public static Dictionary<string, double[]> AverageUnderDoseThreshold
        {
            get { return v_AverageUnderDoseThreshold; }
            set { v_AverageUnderDoseThreshold = value; }
        }

        // Local variables
        private int[] doseSize;
        private double[] doseRes;
        private double[] doseOrigin;
        private int normalizeStructureVoxels;
        private double structureIntensityMean;
        private double structureDoseMean;
        private double averageCountsUnderDoseThreshold;
        private double averageDoseUnderDoseThreshold;
        private double[] structureNormalizationValues;
        private double[] averageUnderDoseThreshold;
        public double intensityNormalizer;
        public double doseNormalizer;

        public void StructureRelative(Dose patientDose, Image patientSPECT, Registration planRegistration, StructureSet structureSet, string normalizeStructureName, string name)
        {
            doseSize = new int[] { patientDose.XSize, patientDose.YSize, patientDose.ZSize };
            doseRes = new double[] { patientDose.XRes, patientDose.YRes, patientDose.ZRes };
            doseOrigin = new double[] { patientDose.Origin.x, patientDose.Origin.y, patientDose.Origin.z };
            VVector doseLocation = new VVector();
            VVector imageLocation = new VVector();
            VVector doseLastLocation = new VVector();
            VVector imageLastLocation = new VVector();

            Structure normalizeStructure = structureSet.Structures.First(s => s.Id == normalizeStructureName);

            normalizeStructureVoxels = 0;
            structureIntensityMean = 0.0;
            structureDoseMean = 0.0;
            for (int z = 0; z < doseSize[2]; z++)
            {
                for (int y = 0; y < doseSize[1]; y++)
                {
                    doseLocation.x = doseOrigin[0];
                    doseLocation.y = doseOrigin[1] + y * doseRes[1];
                    doseLocation.z = doseOrigin[2] + z * doseRes[2];
                    doseLastLocation.x = doseOrigin[0] + (doseSize[0] - 1) * doseRes[0];
                    doseLastLocation.y = doseLocation.y;
                    doseLastLocation.z = doseLocation.z;
                    imageLocation = planRegistration.TransformPoint(doseLocation);
                    imageLastLocation = planRegistration.TransformPoint(doseLastLocation);
                    BitArray normalizeBitArray = new BitArray(doseSize[0]);
                    SegmentProfile segmentProfile = normalizeStructure.GetSegmentProfile(doseLocation, doseLastLocation, normalizeBitArray);
                    double[] voxelDoseSpace = new double[doseSize[0]];
                    DoseProfile doseProfileData = patientDose.GetDoseProfile(doseLocation, doseLastLocation, voxelDoseSpace);
                    double[] voxelImageSpace = new double[doseSize[0]];
                    ImageProfile imageProfileData = patientSPECT.GetImageProfile(imageLocation, imageLastLocation, voxelImageSpace);
                    for (int x = 0; x < doseSize[0]; x++)
                    {
                        if (normalizeBitArray[x])
                        {
                            normalizeStructureVoxels++;
                            structureIntensityMean += imageProfileData[x].Value;
                            structureDoseMean += doseProfileData[x].Value;
                        }
                    }
                }
            }
            structureIntensityMean /= normalizeStructureVoxels;
            structureDoseMean /= normalizeStructureVoxels;
            structureNormalizationValues = new double[] { structureIntensityMean, structureDoseMean };
            StructureNormalizationValue.Add(name, structureNormalizationValues);

            intensityNormalizer = structureIntensityMean;
            doseNormalizer = structureDoseMean;
        }

        public void ThresholdRelative(Dose patientDose, Image patientSPECT, Registration planRegistration, StructureSet structureSet, double doseThreshold, string name)
        {
            doseSize = new int[] { patientDose.XSize, patientDose.YSize, patientDose.ZSize };
            doseRes = new double[] { patientDose.XRes, patientDose.YRes, patientDose.ZRes };
            doseOrigin = new double[] { patientDose.Origin.x, patientDose.Origin.y, patientDose.Origin.z };
            VVector doseLocation = new VVector();
            VVector imageLocation = new VVector();
            VVector doseLastLocation = new VVector();
            VVector imageLastLocation = new VVector();

            Structure lungStructure = structureSet.Structures.First(s => s.Id == "LUNGS-GTV");

            normalizeStructureVoxels = 0;
            averageCountsUnderDoseThreshold = 0.0;
            averageDoseUnderDoseThreshold = 0.0;
            for (int z = 0; z < doseSize[2]; z++)
            {
                for (int y = 0; y < doseSize[1]; y++)
                {
                    doseLocation.x = doseOrigin[0];
                    doseLocation.y = doseOrigin[1] + y * doseRes[1];
                    doseLocation.z = doseOrigin[2] + z * doseRes[2];
                    doseLastLocation.x = doseOrigin[0] + (doseSize[0] - 1) * doseRes[0];
                    doseLastLocation.y = doseLocation.y;
                    doseLastLocation.z = doseLocation.z;
                    imageLocation = planRegistration.TransformPoint(doseLocation);
                    imageLastLocation = planRegistration.TransformPoint(doseLastLocation);
                    BitArray normalizeBitArray = new BitArray(doseSize[0]);
                    SegmentProfile segmentProfile = lungStructure.GetSegmentProfile(doseLocation, doseLastLocation, normalizeBitArray);
                    double[] voxelDoseSpace = new double[doseSize[0]];
                    DoseProfile doseProfileData = patientDose.GetDoseProfile(doseLocation, doseLastLocation, voxelDoseSpace);
                    double[] voxelImageSpace = new double[doseSize[0]];
                    ImageProfile imageProfileData = patientSPECT.GetImageProfile(imageLocation, imageLastLocation, voxelImageSpace);
                    for (int x = 0; x < doseSize[0]; x++)
                    {
                        if (!Double.IsNaN(imageProfileData[x].Value))
                        {
                            if (doseProfileData[x].Value < doseThreshold && doseProfileData[x].Value > 0.1 && normalizeBitArray[x])
                            {
                                normalizeStructureVoxels++;
                                averageCountsUnderDoseThreshold += imageProfileData[x].Value;
                                averageDoseUnderDoseThreshold += doseProfileData[x].Value;
                            }
                        }
                    }
                }
            }
            averageCountsUnderDoseThreshold /= normalizeStructureVoxels;
            averageDoseUnderDoseThreshold /= normalizeStructureVoxels;
            averageUnderDoseThreshold = new double[] { averageCountsUnderDoseThreshold, averageDoseUnderDoseThreshold };
            AverageUnderDoseThreshold.Add(name, averageUnderDoseThreshold);

            intensityNormalizer = averageCountsUnderDoseThreshold;
            doseNormalizer = averageDoseUnderDoseThreshold;
        }

    }
}
