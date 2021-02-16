using DFHAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Forms;
using System.IO;

namespace DFH_Initiate
{
    class DataProcess
    {
        private static Dictionary<string, Dictionary<string, double[,,]>> v_FunctionalMaps = new Dictionary<string, Dictionary<string, double[,,]>>();
        public static Dictionary<string, Dictionary<string, double[,,]>> FunctionalMaps
        {
            get { return v_FunctionalMaps; }
            set { v_FunctionalMaps = value; }
        }
        private static Dictionary<string, Dictionary<string, double[,,]>> v_DoseMaps = new Dictionary<string, Dictionary<string, double[,,]>>();
        public static Dictionary<string, Dictionary<string, double[,,]>> DoseMaps
        {
            get { return v_DoseMaps; }
            set { v_DoseMaps = value; }
        }
        private static Dictionary<string, Dictionary<string, double>> v_MaxIntensityMap = new Dictionary<string, Dictionary<string, double>>();
        public static Dictionary<string, Dictionary<string, double>> MaxIntensityMap
        {
            get { return v_MaxIntensityMap; }
            set { v_MaxIntensityMap = value; }
        }
        private static Dictionary<string, Dictionary<string, double[]>> v_DoseIntensity = new Dictionary<string, Dictionary<string, double[]>>();
        public static Dictionary<string, Dictionary<string, double[]>> DoseIntensity
        {
            get { return v_DoseIntensity; }
            set { v_DoseIntensity = value; }
        }

        private double[] doseOrigin;
        private double[] doseRes;
        private int[] doseSize;
        private double alphaBetaValue;
        private double lungsMeanIntensity;
        private int lungsImageVoxels;
        private double lungsMaxDose;
        private double lungsMaxIntensity;
        private VVector lungsMaxIntensityLocation;
        private double lungsStdvImage;
        private double rightLungMeanIntensity;
        private double rightLungMeanDose;
        private int rightLungImageVoxels;
        private int rightLungDoseVoxels;
        private double rightLungMaxDose;
        private double rightLungMaxIntensity;
        private double rightLungStdvImage;
        private double leftLungMeanIntensity;
        private double leftLungMeanDose;
        private int leftLungImageVoxels;
        private int leftLungDoseVoxels;
        private double leftLungMaxDose;
        private double leftLungMaxIntensity;
        private double leftLungStdvImage;
        public static double intensityNormalizer;
        public static string contralateralLung;
        private string name;
        public static double totalCleanedVoxels;
        private Dictionary<string, double[]> doseIntensityMap;
        public static Dictionary<string, double[]> doseBinsMap;
        private Dictionary<string, double[,,]> intensity;
        private Dictionary<string, double[,,]> dose;
        private Dictionary<string, double> maxIntensityMap;
        public static double leftLungCutoff;
        public static double rightLungCutoff;
        public static string lungID = "LUNGS_4mm";
        public static string rightLungID = "RIGHT_LUNG_4mm";
        public static string leftLungID = "LEFT_LUNG_4mm";
        public static string heartID = "Heart";


        public void ProcessData(Dose patientDose, Image patientSPECT, Registration planRegistration, Image patientCT)
        {
            name = patientSPECT.Series.Id;
            doseSize = new int[] { patientDose.XSize, patientDose.YSize, patientDose.ZSize };
            doseOrigin = new double[] { patientDose.Origin.x, patientDose.Origin.y, patientDose.Origin.z };
            doseRes = new double[] { patientDose.XRes, patientDose.YRes, patientDose.ZRes };
            lungsMaxDose = 0.0;
            lungsMaxIntensity = 0.0;
            lungsMeanIntensity = 0.0;
            lungsImageVoxels = 0;
            lungsStdvImage = 0.0;
            rightLungMaxDose = 0.0;
            rightLungMaxIntensity = 0.0;
            rightLungMeanDose = 0.0;
            rightLungMeanIntensity = 0.0;
            rightLungDoseVoxels = 0;
            rightLungImageVoxels = 0;
            rightLungStdvImage = 0.0;
            leftLungMaxDose = 0.0;
            leftLungMaxIntensity = 0.0;
            leftLungMeanDose = 0.0;
            leftLungMeanIntensity = 0.0;
            leftLungDoseVoxels = 0;
            leftLungImageVoxels = 0;
            leftLungStdvImage = 0.0;
            totalCleanedVoxels = 0.0;
            doseBinsMap = new Dictionary<string, double[]>();
            doseIntensityMap = new Dictionary<string, double[]>();
            intensity = new Dictionary<string, double[,,]>();
            dose = new Dictionary<string, double[,,]>();
            maxIntensityMap = new Dictionary<string, double>();
            lungsMaxIntensityLocation = new VVector();

            int fractionNumber = LoadDosePlan.fractionNumber;
            string useLQCorrection = Batch.bioCorrection;
            StructureSet structureSet = LoadDosePlan.structureSet;
            //string lungID = "LUNGS-GTV";
            //string rightLungID = "RIGHT_LUNG-GTV";
            //string leftLungID = "LEFT_LUNG-GTV";

            /*
            string lungID;
            string rightLungID;
            string leftLungID;
            if (name.Contains("Initial"))
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

            Structure lungsStructure = structureSet.Structures.First(w => w.Id.ToUpper() == lungID.ToUpper());
            Structure rightLungStructure = structureSet.Structures.First(w => w.Id.ToUpper() == rightLungID.ToUpper());
            Structure leftLungStructure = structureSet.Structures.First(w => w.Id.ToUpper() == leftLungID.ToUpper());
            Structure heartStructure = structureSet.Structures.First(w => w.Id.ToUpper() == heartID.ToUpper());
            MeshGeometry3D structureMesh = lungsStructure.MeshGeometry;
            Rect3D structureBox = lungsStructure.MeshGeometry.Bounds;
            Point3D structureLocation = structureBox.Location;
            Size3D boxSize = structureBox.Size;
            int xcount = (int)Math.Ceiling((boxSize.X / doseRes[0]));
            int ycount = (int)Math.Ceiling((boxSize.Y / doseRes[1]));
            int zcount = (int)Math.Ceiling((boxSize.Z / doseRes[2]));
            double xstart = Math.Ceiling((structureLocation.X - doseOrigin[0]) / doseRes[0]) * doseRes[0] + doseOrigin[0];
            double ystart = Math.Ceiling((structureLocation.Y - doseOrigin[1]) / doseRes[1]) * doseRes[1] + doseOrigin[1];
            double zstart = Math.Ceiling((structureLocation.Z - doseOrigin[2]) / doseRes[2]) * doseRes[2] + doseOrigin[2];

            VVector doseStart = new VVector();
            VVector doseStop = new VVector();
            VVector imageStart = new VVector();
            VVector imageStop = new VVector();
            double[,,] imageData = new double[xcount, ycount, zcount];
            double[,,] doseData = new double[xcount, ycount, zcount];
            double[,,] CTdata = new double[xcount, ycount, zcount];
            double[,,] rightLungImageData = new double[xcount, ycount, zcount];
            double[,,] rightLungDoseData = new double[xcount, ycount, zcount];
            double[,,] rightLungCTdata = new double[xcount, ycount, zcount];
            double[,,] leftLungImageData = new double[xcount, ycount, zcount];
            double[,,] leftLungDoseData = new double[xcount, ycount, zcount];
            double[,,] leftLungCTdata = new double[xcount, ycount, zcount];
            double[,,] heartImageData = new double[xcount, ycount, zcount];
            double[,,] heartDoseData = new double[xcount, ycount, zcount];
            double[,,] heartCTdata = new double[xcount, ycount, zcount];


            VVector startVector = new VVector(94.2, -133.1, 1502.1);
            VVector stopVector = new VVector(94.4, -133.1, 1502.1);
            double[] equalVectore = new double[3];
            patientSPECT.GetImageProfile(startVector, stopVector, equalVectore);
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
                        BitArray lungsBitArray = new BitArray(xcount);
                        SegmentProfile lungsProfile = lungsStructure.GetSegmentProfile(doseStart, doseStop, lungsBitArray);
                        BitArray rightLungBitArray = new BitArray(xcount);
                        SegmentProfile rightLungProfile = rightLungStructure.GetSegmentProfile(doseStart, doseStop, rightLungBitArray);
                        BitArray leftLungBitArray = new BitArray(xcount);
                        SegmentProfile leftLungProfile = leftLungStructure.GetSegmentProfile(doseStart, doseStop, leftLungBitArray);
                        BitArray heartBitArray = new BitArray(xcount);
                        SegmentProfile heartProfile = heartStructure.GetSegmentProfile(doseStart, doseStop, heartBitArray);
                        double[] doseArray = new double[xcount];
                        DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                        double[] imageArray = new double[xcount];
                        ImageProfile imageProfile = patientSPECT.GetImageProfile(imageStart, imageStop, imageArray);
                        double[] CTimageArray = new double[xcount];
                        ImageProfile CTimageProfile = patientCT.GetImageProfile(doseStart, doseStop, CTimageArray);
                        for (int x = 0; x < xcount; x++)
                        {
                            // RIGHT_LUNG-GTV data
                            if (rightLungProfile[x].Value == true)
                            {
                                imageData[x, y, z] = imageProfile[x].Value;
                                doseData[x, y, z] = doseProfile[x].Value;
                                CTdata[x, y, z] = CTimageProfile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                    lungsMaxIntensityLocation.x = imageStart.x + x * doseRes[0];
                                    lungsMaxIntensityLocation.y = imageStart.y;
                                    lungsMaxIntensityLocation.z = imageStart.z;
                                }
                                rightLungImageData[x, y, z] = imageProfile[x].Value;
                                rightLungDoseData[x, y, z] = doseProfile[x].Value;
                                rightLungCTdata[x, y, z] = CTimageProfile[x].Value;
                                if (rightLungImageData[x, y, z] > rightLungMaxIntensity)
                                {
                                    rightLungMaxIntensity = rightLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                rightLungDoseData[x, y, z] = double.NaN;
                                rightLungImageData[x, y, z] = double.NaN;
                                rightLungCTdata[x, y, z] = double.NaN;
                            }

                            // LEFT_LUNG-GTV data
                            if (leftLungProfile[x].Value == true)
                            {
                                imageData[x, y, z] = imageProfile[x].Value;
                                doseData[x, y, z] = doseProfile[x].Value;
                                CTdata[x, y, z] = CTimageProfile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                }

                                leftLungImageData[x, y, z] = imageProfile[x].Value;
                                leftLungDoseData[x, y, z] = doseProfile[x].Value;
                                leftLungCTdata[x, y, z] = CTimageProfile[x].Value;
                                if (leftLungImageData[x, y, z] > leftLungMaxIntensity)
                                {
                                    leftLungMaxIntensity = leftLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                leftLungDoseData[x, y, z] = double.NaN;
                                leftLungImageData[x, y, z] = double.NaN;
                                leftLungCTdata[x, y, z] = double.NaN;
                            }

                            // HEART data
                            if (heartProfile[x].Value == true)
                            {
                                heartImageData[x, y, z] = imageProfile[x].Value;
                                heartDoseData[x, y, z] = doseProfile[x].Value;
                                heartCTdata[x, y, z] = CTimageProfile[x].Value;
                            }
                            else
                            {
                                heartImageData[x, y, z] = double.NaN;
                                heartDoseData[x, y, z] = double.NaN;
                                heartCTdata[x, y, z] = double.NaN;
                            }

                            if (leftLungProfile[x].Value == false && rightLungProfile[x].Value == false)
                            {
                                doseData[x, y, z] = double.NaN;
                                imageData[x, y, z] = double.NaN;
                                CTdata[x, y, z] = double.NaN;
                            }
                        }
                    }
                }
            }
            else
            {
                alphaBetaValue = 2.5;
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
                        BitArray lungsBitArray = new BitArray(xcount);
                        SegmentProfile lungsProfile = lungsStructure.GetSegmentProfile(doseStart, doseStop, lungsBitArray);
                        BitArray rightLungBitArray = new BitArray(xcount);
                        SegmentProfile rightLungProfile = rightLungStructure.GetSegmentProfile(doseStart, doseStop, rightLungBitArray);
                        BitArray leftLungBitArray = new BitArray(xcount);
                        SegmentProfile leftLungProfile = leftLungStructure.GetSegmentProfile(doseStart, doseStop, leftLungBitArray);
                        BitArray heartBitArray = new BitArray(xcount);
                        SegmentProfile heartProfile = heartStructure.GetSegmentProfile(doseStart, doseStop, heartBitArray);
                        double[] doseArray = new double[xcount];
                        DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                        double[] imageArray = new double[xcount];
                        ImageProfile imageProfile = patientSPECT.GetImageProfile(imageStart, imageStop, imageArray);
                        double[] CTimageArray = new double[xcount];
                        ImageProfile CTimageProfile = patientCT.GetImageProfile(doseStart, doseStop, CTimageArray);
                        for (int x = 0; x < xcount; x++)
                        {
                            // RIGHT_LUNG-GTV data
                            if (rightLungProfile[x].Value == true)
                            {
                                double pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                imageData[x, y, z] = imageProfile[x].Value;
                                doseData[x, y, z] = pointDose;
                                CTdata[x, y, z] = CTimageProfile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                    lungsMaxIntensityLocation.x = imageStart.x + x * doseRes[0];
                                    lungsMaxIntensityLocation.y = imageStart.y;
                                    lungsMaxIntensityLocation.z = imageStart.z;
                                }
                                rightLungImageData[x, y, z] = imageProfile[x].Value;
                                rightLungDoseData[x, y, z] = pointDose;
                                rightLungCTdata[x, y, z] = CTimageProfile[x].Value;
                                if (rightLungImageData[x, y, z] > rightLungMaxIntensity)
                                {
                                    rightLungMaxIntensity = rightLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                rightLungDoseData[x, y, z] = double.NaN;
                                rightLungImageData[x, y, z] = double.NaN;
                                rightLungCTdata[x, y, z] = double.NaN;
                            }

                            // LEFT_LUNG-GTV data
                            if (leftLungProfile[x].Value == true)
                            {
                                double pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                imageData[x, y, z] = imageProfile[x].Value;
                                doseData[x, y, z] = pointDose;
                                CTdata[x, y, z] = CTimageProfile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                    lungsMaxIntensityLocation.x = imageStart.x + x * doseRes[0];
                                    lungsMaxIntensityLocation.y = imageStart.y;
                                    lungsMaxIntensityLocation.z = imageStart.z;
                                }
                                leftLungImageData[x, y, z] = imageProfile[x].Value;
                                leftLungDoseData[x, y, z] = pointDose;
                                leftLungCTdata[x, y, z] = CTimageProfile[x].Value;
                                if (leftLungImageData[x, y, z] > leftLungMaxIntensity)
                                {
                                    leftLungMaxIntensity = leftLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                leftLungDoseData[x, y, z] = double.NaN;
                                leftLungImageData[x, y, z] = double.NaN;
                                leftLungCTdata[x, y, z] = double.NaN;
                            }

                            // HEART data
                            if (heartProfile[x].Value == true)
                            {
                                double pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                heartImageData[x, y, z] = imageProfile[x].Value;
                                heartDoseData[x, y, z] = pointDose;
                                heartCTdata[x, y, z] = CTimageProfile[x].Value;
                            }
                            else
                            {
                                heartImageData[x, y, z] = double.NaN;
                                heartDoseData[x, y, z] = double.NaN;
                                heartCTdata[x, y, z] = double.NaN;
                            }


                            if (leftLungProfile[x].Value == false && rightLungProfile[x].Value == false)
                            {
                                doseData[x, y, z] = double.NaN;
                                imageData[x, y, z] = double.NaN;
                                CTdata[x, y, z] = double.NaN;
                            }
                        }
                    }
                }
            }


            double lungsUpperLimit;
            double rightLungUpperLimit;
            double leftLungUpperLimit;

            if (name.Contains("VENT"))
            {
                lungsUpperLimit = Math.Min(lungsMaxIntensity * 0.7, 750);
                rightLungUpperLimit = Math.Min(rightLungMaxIntensity * 0.7, 750);
                leftLungUpperLimit = Math.Min(leftLungMaxIntensity * 0.7, 750);
            }
            else
            {
                lungsUpperLimit = Math.Min(lungsMaxIntensity * 0.7, 1500);
                rightLungUpperLimit = Math.Min(rightLungMaxIntensity * 0.7, 1500);
                leftLungUpperLimit = Math.Min(leftLungMaxIntensity * 0.7, 1500);
            }

            double lowerLimit = 100;
            for (int z = 0; z < zcount; z++)
            {
                for (int y = 0; y < ycount; y++)
                {
                    for (int x = 0; x < xcount; x++)
                    {
                        if (!double.IsNaN(imageData[x, y, z]) && imageData[x, y, z] < lungsUpperLimit && imageData[x, y, z] > lowerLimit)
                        {
                            lungsMeanIntensity += imageData[x, y, z];
                            lungsImageVoxels++;
                        }
                        if (!double.IsNaN(rightLungImageData[x, y, z]) && rightLungImageData[x, y, z] < rightLungUpperLimit && rightLungImageData[x, y, z] > lowerLimit)
                        {
                            rightLungMeanIntensity += rightLungImageData[x, y, z];
                            rightLungImageVoxels++;
                        }
                        if (!double.IsNaN(rightLungDoseData[x, y, z]))
                        {
                            rightLungMeanDose += rightLungDoseData[x, y, z];
                            rightLungDoseVoxels++;
                        }
                        if (!double.IsNaN(leftLungImageData[x, y, z]) && leftLungImageData[x, y, z] < leftLungUpperLimit && leftLungImageData[x, y, z] > lowerLimit)
                        {
                            leftLungMeanIntensity += leftLungImageData[x, y, z];
                            leftLungImageVoxels++;
                        }
                        if (!double.IsNaN(leftLungDoseData[x, y, z]))
                        {
                            leftLungMeanDose += leftLungDoseData[x, y, z];
                            leftLungDoseVoxels++;
                        }
                    }
                }
            }
            lungsMeanIntensity /= lungsImageVoxels;
            rightLungMeanIntensity /= rightLungImageVoxels;
            leftLungMeanIntensity /= leftLungImageVoxels;

            rightLungMeanDose /= rightLungDoseVoxels;
            leftLungMeanDose /= leftLungDoseVoxels;


            for (int z = 0; z < zcount; z++)
            {
                for (int y = 0; y < ycount; y++)
                {
                    for (int x = 0; x < xcount; x++)
                    {
                        if (!double.IsNaN(imageData[x, y, z]) && imageData[x, y, z] < lungsUpperLimit && imageData[x, y, z] > lowerLimit)
                        {
                            lungsStdvImage += Math.Pow(imageData[x, y, z] - lungsMeanIntensity, 2);
                        }
                        if (!double.IsNaN(rightLungImageData[x, y, z]) && rightLungImageData[x, y, z] < rightLungUpperLimit && rightLungImageData[x, y, z] > lowerLimit)
                        {
                            rightLungStdvImage += Math.Pow(rightLungImageData[x, y, z] - rightLungMeanIntensity, 2);
                        }
                        if (!double.IsNaN(leftLungImageData[x, y, z]) && leftLungImageData[x, y, z] < leftLungUpperLimit && leftLungImageData[x, y, z] > lowerLimit)
                        {
                            leftLungStdvImage += Math.Pow(leftLungImageData[x, y, z] - leftLungMeanIntensity, 2);
                        }
                    }
                }
            }
            lungsStdvImage = Math.Sqrt(lungsStdvImage / lungsImageVoxels);
            rightLungStdvImage = Math.Sqrt(rightLungStdvImage / rightLungImageVoxels);
            leftLungStdvImage = Math.Sqrt(leftLungStdvImage / leftLungImageVoxels);

            double leftCleaningCutoff;
            double rightCleaningCutoff;

            if (name.Contains("VENT"))
            {
                leftCleaningCutoff = Math.Max(leftLungMeanIntensity + leftLungStdvImage * 3, 350);
                rightCleaningCutoff = Math.Max(rightLungMeanIntensity + rightLungStdvImage * 3, 350);
            }
            else
            {
                leftCleaningCutoff = leftLungMeanIntensity + leftLungStdvImage * 3;
                rightCleaningCutoff = rightLungMeanIntensity + rightLungStdvImage * 3;
            }

            leftLungCutoff = leftCleaningCutoff;
            rightLungCutoff = rightCleaningCutoff;


            double normalizerCutoff;
            int rightLungClean = 0;
            int leftLungClean = 0;
            int rightLungVoxels = 0;
            int leftLungVoxels = 0;
            int totalVoxels = 0;

            lungsMaxIntensity = 0.0;
            rightLungMaxIntensity = 0.0;
            leftLungMaxIntensity = 0.0;


            // Cleanse data of outliers
            for (int z = 0; z < zcount; z++)
            {
                for (int y = 0; y < ycount; y++)
                {
                    for (int x = 0; x < xcount; x++)
                    {
                        if (!double.IsNaN(rightLungImageData[x, y, z]))
                        {
                            rightLungVoxels++;
                        }
                        if (!double.IsNaN(leftLungImageData[x, y, z]))
                        {
                            leftLungVoxels++;
                        }
                        if (rightLungImageData[x, y, z] > rightCleaningCutoff)
                        {
                            rightLungImageData[x, y, z] = double.NaN;
                            imageData[x, y, z] = double.NaN;
                            rightLungClean++;
                        }
                        if (leftLungImageData[x, y, z] > leftCleaningCutoff)
                        {
                            leftLungImageData[x, y, z] = double.NaN;
                            imageData[x, y, z] = double.NaN;
                            leftLungClean++;
                        }
                        if (!double.IsNaN(imageData[x, y, z]))
                        {
                            totalVoxels++;
                        }
                        if (imageData[x, y, z] > lungsMaxIntensity)
                        {
                            lungsMaxIntensity = imageData[x, y, z];
                        }
                        if (doseData[x, y, z] > lungsMaxDose)
                        {
                            lungsMaxDose = doseData[x, y, z];
                        }
                        if (rightLungImageData[x, y, z] > rightLungMaxIntensity)
                        {
                            rightLungMaxIntensity = rightLungImageData[x, y, z];
                        }
                        if (rightLungDoseData[x, y, z] > rightLungMaxDose)
                        {
                            rightLungMaxDose = rightLungDoseData[x, y, z];
                        }
                        if (leftLungImageData[x, y, z] > leftLungMaxIntensity)
                        {
                            leftLungMaxIntensity = leftLungImageData[x, y, z];
                        }
                        if (leftLungDoseData[x, y, z] > leftLungMaxDose)
                        {
                            leftLungMaxDose = leftLungDoseData[x, y, z];
                        }
                    }
                }
            }
            double RLpercentCleaned = Math.Round((double)rightLungClean / rightLungVoxels * 100, 4);
            double LLpercentCleaned = Math.Round((double)leftLungClean / leftLungVoxels * 100, 4);
            totalCleanedVoxels = Math.Round((double)(rightLungClean + leftLungClean) / (rightLungVoxels + leftLungVoxels) * 100, 2);
            
            //MessageBox.Show("Right Lung: Cutoff = " + rightCleaningCutoff.ToString() + ", % Cleaned = " + rightLungClean.ToString() + "(" + RLpercentCleaned.ToString() + ")" + Environment.NewLine
            //    + "Left Lung: Cutoff = " + leftCleaningCutoff.ToString() + ", % Cleaned = " + leftLungClean.ToString() + "(" + LLpercentCleaned.ToString() + ")");
            

            Normalizer newNormalizer = new Normalizer();
            if (rightLungMeanDose < leftLungMeanDose)
            {
                contralateralLung = rightLungID;
                normalizerCutoff = rightCleaningCutoff;
                newNormalizer.ThresholdRelative(rightLungDoseData, rightLungImageData, Batch.doseThreshold, rightLungMaxIntensity);
                intensityNormalizer = newNormalizer.intensityNormalizer;
            }
            else if (leftLungMeanDose < rightLungMeanDose)
            {
                contralateralLung = leftLungID;
                normalizerCutoff = leftCleaningCutoff;
                newNormalizer.ThresholdRelative(leftLungDoseData, leftLungImageData, Batch.doseThreshold, leftLungMaxIntensity);
                intensityNormalizer = newNormalizer.intensityNormalizer;
            }
            else
            {
                if (rightLungStructure.IsEmpty)
                {
                    contralateralLung = leftLungID;
                    normalizerCutoff = leftCleaningCutoff;
                    newNormalizer.ThresholdRelative(leftLungDoseData, leftLungImageData, Batch.doseThreshold, leftLungMaxIntensity);
                    intensityNormalizer = newNormalizer.intensityNormalizer;
                }
                else if (leftLungStructure.IsEmpty)
                {
                    contralateralLung = rightLungID;
                    normalizerCutoff = rightCleaningCutoff;
                    newNormalizer.ThresholdRelative(rightLungDoseData, rightLungImageData, Batch.doseThreshold, rightLungMaxIntensity);
                    intensityNormalizer = newNormalizer.intensityNormalizer;
                }
                else
                {
                    MessageBox.Show("ERROR: Normalization structure not found.");
                }
            }
            //MessageBox.Show("Normalizer = " + intensityNormalizer.ToString());
            // Start
            int binNumber = 11;
            int ipsBinSeparator = 5;
            int contBinSeparator = 1;
            int binSeparator;

            // LUNGS-GTV
            binSeparator = ipsBinSeparator;
            double[] intensityHistogram = new double[binNumber - 1];
            double[] intensityDoseHistogram = new double[binNumber - 1];
            int[] aveIntensityVoxels = new int[binNumber - 1];
            int maxDoseBinNumber = Convert.ToInt16(Math.Ceiling(lungsMaxDose / binSeparator));
            double[] doseIntensityHistogram = new double[maxDoseBinNumber];
            double[] cDoseIntensityHistogram = new double[maxDoseBinNumber];
            int[] aveDoseVoxels = new int[maxDoseBinNumber];
            double[] doseBins = new double[maxDoseBinNumber + 1];
            for (int i = 0; i < maxDoseBinNumber + 1; i++)
            {
                doseBins[i] = i * binSeparator;
            }
            double[] intensityBins = new double[binNumber];
            double intensitySeparator = 0.0;

            // RIGHT_LUNG-GTV
            if (contralateralLung == rightLungID)
            {
                binSeparator = contBinSeparator;
            }
            else
            {
                binSeparator = ipsBinSeparator;
            }
            double[] RLintensityHistogram = new double[binNumber - 1];
            double[] RLintensityDoseHistogram = new double[binNumber - 1];
            int[] RLaveIntensityVoxels = new int[binNumber - 1];
            int RLmaxDoseBinNumber = Convert.ToInt16(Math.Ceiling(rightLungMaxDose / binSeparator));
            double[] RLdoseIntensityHistogram = new double[RLmaxDoseBinNumber];
            double[] cRLdoseIntensityHistogram = new double[RLmaxDoseBinNumber];

            int[] RLaveDoseVoxels = new int[RLmaxDoseBinNumber];
            double[] RLdoseBins = new double[RLmaxDoseBinNumber + 1];
            for (int i = 0; i < RLmaxDoseBinNumber + 1; i++)
            {
                RLdoseBins[i] = i * binSeparator;
            }
            double[] RLintensityBins = new double[binNumber];
            double RLintensitySeparator = 0.0;

            // LEFT_LUNG-GTV
            if (contralateralLung == leftLungID)
            {
                binSeparator = contBinSeparator;
            }
            else
            {
                binSeparator = ipsBinSeparator;
            }
            double[] LLintensityHistogram = new double[binNumber - 1];
            double[] LLintensityDoseHistogram = new double[binNumber - 1];
            int[] LLaveIntensityVoxels = new int[binNumber - 1];
            int LLmaxDoseBinNumber = Convert.ToInt16(Math.Ceiling(leftLungMaxDose / binSeparator));
            double[] LLdoseIntensityHistogram = new double[LLmaxDoseBinNumber];
            double[] cLLdoseIntensityHistogram = new double[LLmaxDoseBinNumber];
            int[] LLaveDoseVoxels = new int[LLmaxDoseBinNumber];
            double[] LLdoseBins = new double[LLmaxDoseBinNumber + 1];
            for (int i = 0; i < LLmaxDoseBinNumber + 1; i++)
            {
                LLdoseBins[i] = i * binSeparator;
            }
            double[] LLintensityBins = new double[binNumber];
            double LLintensitySeparator = 0.0;

            double imageSum = 0.0;
            double RLimageSum = 0.0;
            double LLimageSum = 0.0;


            double[] intensityArray = new double[totalVoxels];
            int counter = 0;
            if (SelectOptions.IntensityNormalize == "Relative")
            {
                lungsMaxIntensity = lungsMaxIntensity / intensityNormalizer;
                rightLungMaxIntensity = rightLungMaxIntensity / intensityNormalizer;
                leftLungMaxIntensity = leftLungMaxIntensity / intensityNormalizer;

                intensitySeparator = lungsMaxIntensity / (binNumber - 1);
                for (int i = 0; i < binNumber; i++)
                {
                    intensityBins[i] = intensitySeparator * i;
                }

                RLintensitySeparator = rightLungMaxIntensity / (binNumber - 1);
                for (int i = 0; i < binNumber; i++)
                {
                    RLintensityBins[i] = RLintensitySeparator * i;
                }

                LLintensitySeparator = leftLungMaxIntensity / (binNumber - 1);
                for (int i = 0; i < binNumber; i++)
                {
                    LLintensityBins[i] = LLintensitySeparator * i;
                }
                

                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string timepoint = patientSPECT.Series.Id.Replace(" ","_");
                string intensityFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_" + timepoint + ".csv";
                string doseFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_Dose.csv";
                string CTfileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_CT.csv";
                string filePathI = System.IO.Path.Combine(path, intensityFileName);
                string filePathII = System.IO.Path.Combine(path, doseFileName);
                string CTfilePath = System.IO.Path.Combine(path, CTfileName);
                StringBuilder dataBuilder = new StringBuilder();
                StringBuilder doseBuilder = new StringBuilder();
                StringBuilder CTdataBuilder = new StringBuilder();
                bool firstLine = true;
                for (int z = 0; z < zcount; z++)
                {
                    for (int y = 0; y < ycount; y++)
                    {
                        for (int x = 0; x < xcount; x++)
                        {

                            // Normalize intensity values to equilibrate between timepoints.
                            imageData[x, y, z] /= intensityNormalizer;
                            rightLungImageData[x, y, z] /= intensityNormalizer;
                            leftLungImageData[x, y, z] /= intensityNormalizer;
                            if (!double.IsNaN(imageData[x,y,z]))
                            {
                                intensityArray[counter] = imageData[x, y, z];
                                counter++;
                            }

                            // Write dose-function matrices to csv to facilitate imporation into MATLAB
                            if (firstLine)
                            {
                                dataBuilder.Append(timepoint + "," + xcount + "," + ycount + "," + zcount);
                                doseBuilder.Append(Program.planName + "," + xcount + "," + ycount + "," + zcount);
                                CTdataBuilder.Append(Program.patientID + "," + xcount + "," + ycount + "," + zcount);
                                for (int w = 0; w < xcount - 6; w++)
                                {
                                    dataBuilder.Append(",");
                                    doseBuilder.Append(",");
                                    CTdataBuilder.Append(",");
                                }
                                dataBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);
                                doseBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);
                                CTdataBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);

                                dataBuilder.AppendLine();
                                doseBuilder.AppendLine();
                                CTdataBuilder.AppendLine();
                                firstLine = false;
                            }
                            // Lungs-GTV Writing
                            dataBuilder.Append(imageData[x, y, z] + ",");
                            doseBuilder.Append(doseData[x, y, z] + ",");
                            CTdataBuilder.Append(CTdata[x, y, z] + ",");

                            /*
                            // Ipsilateral_Lung-GTV Writing
                            if (contralateralLung == "LEFT_LUNG-GTV")
                            {
                                dataBuilder.Append(rightLungImageData[x, y, z] + ",");
                                doseBuilder.Append(rightLungDoseData[x, y, z] + ",");
                                CTdataBuilder.Append(rightLungCTdata[x, y, z] + ",");

                            }
                            else if (contralateralLung == "RIGHT_LUNG-GTV")
                            {
                                dataBuilder.Append(leftLungImageData[x, y, z] + ",");
                                doseBuilder.Append(leftLungDoseData[x, y, z] + ",");
                                CTdataBuilder.Append(leftLungCTdata[x, y, z] + ",");
                            }
                            */


                            // Generate image sums for relative dose function curves
                            if (!double.IsNaN(imageData[x, y, z]))
                            {
                                imageSum += imageData[x, y, z];
                            }
                            if (!double.IsNaN(rightLungImageData[x, y, z]))
                            {
                                RLimageSum += rightLungImageData[x, y, z];
                            }
                            if (!double.IsNaN(leftLungImageData[x, y, z]))
                            {
                                LLimageSum += leftLungImageData[x, y, z];
                            }


                            // Intensity/Dose Curves
                            for (int i = 0; i < intensityHistogram.Length; i++)
                            {
                                if (imageData[x, y, z] >= intensityBins[i] && imageData[x, y, z] < intensityBins[i + 1])
                                {
                                    intensityHistogram[i]++;
                                    intensityDoseHistogram[i] += doseData[x, y, z];
                                    aveIntensityVoxels[i]++;
                                }
                                if (i < RLintensityHistogram.Length)
                                {
                                    if (rightLungImageData[x, y, z] >= RLintensityBins[i] && rightLungImageData[x, y, z] < RLintensityBins[i + 1])
                                    {
                                        RLintensityHistogram[i]++;
                                        RLintensityDoseHistogram[i] += rightLungDoseData[x, y, z];
                                        RLaveIntensityVoxels[i]++;
                                    }
                                }
                                if (i < LLintensityHistogram.Length)
                                {
                                    if (leftLungImageData[x, y, z] >= LLintensityBins[i] && leftLungImageData[x, y, z] < LLintensityBins[i + 1])
                                    {
                                        LLintensityHistogram[i]++;
                                        LLintensityDoseHistogram[i] += leftLungDoseData[x, y, z];
                                        LLaveIntensityVoxels[i]++;
                                    }
                                }
                            }
                            for (int i = 0; i < maxDoseBinNumber; i++)
                            {
                                if (!double.IsNaN(imageData[x, y, z]) && doseData[x, y, z] >= doseBins[i] && doseData[x, y, z] < doseBins[i + 1])
                                {
                                    doseIntensityHistogram[i] += imageData[x, y, z];
                                    aveDoseVoxels[i]++;
                                }
                                if (!double.IsNaN(imageData[x, y, z]) && doseData[x, y, z] >= doseBins[i])
                                {
                                    cDoseIntensityHistogram[i] += imageData[x, y, z];
                                }
                            }
                            for (int i = 0; i < RLmaxDoseBinNumber; i++)
                            {
                                if (!double.IsNaN(rightLungImageData[x, y, z]) && rightLungDoseData[x, y, z] >= RLdoseBins[i] && rightLungDoseData[x, y, z] < RLdoseBins[i + 1])
                                {
                                    RLdoseIntensityHistogram[i] += rightLungImageData[x, y, z];
                                    RLaveDoseVoxels[i]++;
                                }
                                if (!double.IsNaN(rightLungImageData[x, y, z]) && rightLungDoseData[x, y, z] >= RLdoseBins[i])
                                {
                                    cRLdoseIntensityHistogram[i] += rightLungImageData[x, y, z];
                                }

                            }
                            for (int i = 0; i < LLmaxDoseBinNumber; i++)
                            {
                                if (!double.IsNaN(leftLungImageData[x, y, z]) && leftLungDoseData[x, y, z] >= LLdoseBins[i] && leftLungDoseData[x, y, z] < LLdoseBins[i + 1])
                                {
                                    LLdoseIntensityHistogram[i] += leftLungImageData[x, y, z];
                                    LLaveDoseVoxels[i]++;
                                }
                                if (!double.IsNaN(leftLungImageData[x, y, z]) && leftLungDoseData[x, y, z] >= LLdoseBins[i])
                                {
                                    cLLdoseIntensityHistogram[i] += leftLungImageData[x, y, z];
                                }
                            }

                        }
                        dataBuilder.AppendLine();
                        doseBuilder.AppendLine();
                        CTdataBuilder.AppendLine();
                    }
                    dataBuilder.AppendLine();
                    doseBuilder.AppendLine();
                    CTdataBuilder.AppendLine();
                }
                dataBuilder.AppendLine();
                doseBuilder.AppendLine();
                CTdataBuilder.AppendLine();
                dataBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);
                doseBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);
                CTdataBuilder.Append(doseRes[0] + "," + doseRes[1] + "," + doseRes[2]);
                if (timepoint.Contains("PERF") && Program.printCT == true)
                {
                    File.AppendAllText(filePathI, dataBuilder.ToString());
                    File.AppendAllText(filePathII, doseBuilder.ToString());
                    File.AppendAllText(CTfilePath, CTdataBuilder.ToString());
                }
                else if (timepoint.Contains("PERF"))
                {
                    File.AppendAllText(filePathI, dataBuilder.ToString());
                }
            }
            else
            {
                for (int z = 0; z < zcount; z++)
                {
                    for (int y = 0; y < ycount; y++)
                    {
                        for (int x = 0; x < xcount; x++)
                        {
                            intensityArray[counter] = imageData[x, y, z];
                            counter++;
                        }
                    }
                }
            }

            Array.Sort(intensityArray);
            Array.Reverse(intensityArray);
            //int medianVoxelNumber = Convert.ToInt32(0.1 * totalVoxels) / 2;
            //double intensityCutoff = intensityArray[medianVoxelNumber] *0.6;

            for (int i = 0; i < intensityHistogram.Length; i++)
            {
                intensityHistogram[i] /= intensitySeparator;
                intensityDoseHistogram[i] /= aveIntensityVoxels[i];
            }
            for (int i = 0; i < RLintensityHistogram.Length; i++)
            {
                RLintensityHistogram[i] /= RLintensitySeparator;
                RLintensityDoseHistogram[i] /= RLaveIntensityVoxels[i];
            }
            for (int i = 0; i < LLintensityHistogram.Length; i++)
            {
                LLintensityHistogram[i] /= LLintensitySeparator;
                LLintensityDoseHistogram[i] /= LLaveIntensityVoxels[i];
            }
            for (int i = 0; i < maxDoseBinNumber; i++)
            {
                doseIntensityHistogram[i] /= aveDoseVoxels[i];
                cDoseIntensityHistogram[i] = cDoseIntensityHistogram[i] / imageSum * 100;
            }
            for (int i = 0; i < RLmaxDoseBinNumber; i++)
            {
                RLdoseIntensityHistogram[i] /= RLaveDoseVoxels[i];
                cRLdoseIntensityHistogram[i] = cRLdoseIntensityHistogram[i] / RLimageSum * 100;
            }
            for (int i = 0; i < LLmaxDoseBinNumber; i++)
            {
                LLdoseIntensityHistogram[i] /= LLaveDoseVoxels[i];
                cLLdoseIntensityHistogram[i] = cLLdoseIntensityHistogram[i] / LLimageSum * 100;
            }
            for (int i = 0; i < binNumber; i++)
            {
                intensityBins[i] /= lungsMaxIntensity;
                RLintensityBins[i] /= rightLungMaxIntensity;
                LLintensityBins[i] /= leftLungMaxIntensity;
            }

            string[] structureList = new string[] { lungID, rightLungID, leftLungID, heartID };

            dose.Add(structureList[0], doseData);
            dose.Add(structureList[1], rightLungDoseData);
            dose.Add(structureList[2], leftLungDoseData);
            dose.Add(structureList[3], heartDoseData);

            intensity.Add(structureList[0], imageData);
            intensity.Add(structureList[1], rightLungImageData);
            intensity.Add(structureList[2], leftLungImageData);
            intensity.Add(structureList[3], heartImageData);

            doseBinsMap.Add(structureList[0], doseBins);
            doseBinsMap.Add(structureList[1], RLdoseBins);
            doseBinsMap.Add(structureList[2], LLdoseBins);

            doseIntensityMap.Add(structureList[0], doseIntensityHistogram);
            doseIntensityMap.Add(structureList[1], RLdoseIntensityHistogram);
            doseIntensityMap.Add(structureList[2], LLdoseIntensityHistogram);

            maxIntensityMap.Add(structureList[0], lungsMaxIntensity);
            maxIntensityMap.Add(structureList[1], lungsMaxIntensity);
            maxIntensityMap.Add(structureList[2], lungsMaxIntensity);

            //MessageBox.Show("Lungs Max Intensity = " + lungsMaxIntensity.ToString());
            MetricAnalysis runMetrics = new MetricAnalysis();
            if (!lungsStructure.IsEmpty)
            {
                runMetrics.Analyze(lungsStructure.Id.ToUpper(), doseData, imageData, lungsMaxIntensity, structureSet);
            }
            else
            {
                runMetrics.EmptyStructure(lungsStructure.Id.ToUpper());
            }
            if (!rightLungStructure.IsEmpty)
            {
                runMetrics.Analyze(rightLungStructure.Id.ToUpper(), rightLungDoseData, rightLungImageData, lungsMaxIntensity, structureSet);
            }
            else
            {
                runMetrics.EmptyStructure(rightLungStructure.Id.ToUpper());
            }
            if (!leftLungStructure.IsEmpty)
            {
                runMetrics.Analyze(leftLungStructure.Id.ToUpper(), leftLungDoseData, leftLungImageData, lungsMaxIntensity, structureSet);
            }
            else
            {
                runMetrics.EmptyStructure(leftLungStructure.Id.ToUpper());
            }
            if (!heartStructure.IsEmpty)
            {
                runMetrics.Analyze(heartStructure.Id.ToUpper(), heartDoseData, heartImageData, lungsMaxIntensity, structureSet);
            }
            else
            {
                runMetrics.EmptyStructure(heartStructure.Id.ToUpper());
            }
            /*
            //string[] regionalStructures = new string[] { "Z_RUL", "Z_RML", "Z_RLL", "Z_LUL", "Z_LML", "Z_LLL" };
            string[] regionalStructures = new string[] { "Z_RUL_4mm", "Z_RML_4mm", "Z_RLL_4mm", "Z_LUL_4mm", "Z_LML_4mm", "Z_LLL_4mm" };
            //string[] regionalStructures = new string[] { "PRE_LUNGS_VENT", "PRE_RIGHT_VENT", "PRE_LEFT_VENT", "PRE_LF_VENT", "PRE_F_VENT", "PRE_HF_VENT" };
            //string[] regionalStructures = new string[] { "PRE_LUNGS_PERF", "PRE_RIGHT_PERF", "PRE_LEFT_PERF", "PRE_QUAD1_PERF", "PRE_QUAD2_PERF", "PRE_QUAD3_PERF", "PRE_QUAD4_PERF" };

            for (int i = 0; i < regionalStructures.Length; i++)
            {
                Structure regionStructure = structureSet.Structures.First(w => w.Id.ToUpper() == regionalStructures[i].ToUpper());
                double[,,] structureDoseData = new double[xcount, ycount, zcount];
                double[,,] structureImageData = new double[xcount, ycount, zcount];
                double[,,] structureCTData = new double[xcount, ycount, zcount];
                double structureMaxIntensity = 0.0;
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
                        SegmentProfile structureProfile = regionStructure.GetSegmentProfile(doseStart, doseStop, structureBitArray);
                        for (int x = 0; x < xcount; x++)
                        {
                            if (structureProfile[x].Value == true)
                            {
                                structureDoseData[x, y, z] = doseData[x, y, z];
                                structureImageData[x, y, z] = imageData[x, y, z];
                                structureCTData[x, y, z] = CTdata[x, y, z];
                                
                                if (structureImageData[x,y,z] > structureMaxIntensity)
                                {
                                    structureMaxIntensity = structureImageData[x, y, z];
                                }
                                
                            }
                            else
                            {
                                structureDoseData[x, y, z] = double.NaN;
                                structureImageData[x, y, z] = double.NaN;
                                structureCTData[x, y, z] = double.NaN;
                            }
                        }
                    }
                }
                string regionID = regionalStructures[i];

                
                if (regionID[2] == 'R')
                {
                    structureMaxIntensity = rightLungMaxIntensity;
                }
                else
                {
                    structureMaxIntensity = leftLungMaxIntensity;
                }
                
                runMetrics.Analyze(regionalStructures[i].ToUpper(), structureDoseData, structureImageData, lungsMaxIntensity, structureSet);
            }
            */
            

            string seriesName = patientSPECT.Series.ToString();
            DoseMaps.Add(seriesName, dose);
            FunctionalMaps.Add(seriesName, intensity);
            MaxIntensityMap.Add(seriesName, maxIntensityMap);
            DoseIntensity.Add(seriesName, doseIntensityMap);
        }

    }
}
