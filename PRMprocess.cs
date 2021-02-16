using DFHAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFH_Initiate
{
    class PRMprocess
    {
        public void PRMdataProcess(Dose patientDose, Image patientSPECT, Image patientPRM, Registration imageRegistration, Registration planRegistration, Registration PRMregistration)
        {
            string name = patientSPECT.Series.Id;
            string PRMseries = patientPRM.Series.Id;
            double alphaBetaValue;
            //double[] imageRes = new double[] { patientSPECT.XRes, patientSPECT.YRes, patientSPECT.ZRes };
            //double[] imageOrigin = new double[] { patientSPECT.Origin.x, patientSPECT.Origin.y, patientSPECT.Origin.z };
            double[] PRMres = new double[] { patientPRM.XRes, patientPRM.YRes, patientPRM.ZRes };
            double[] PRMorigin = new double[] { patientPRM.Origin.x, patientPRM.Origin.y, patientPRM.Origin.z };
            int[]PRMsize = new int[] { patientPRM.XSize, patientPRM.YSize, patientPRM.ZSize };
            StructureSet structureSet = LoadDosePlan.structureSet;

            string lungID = DataProcess.lungID;
            string rightLungID = DataProcess.rightLungID;
            string leftLungID = DataProcess.leftLungID;
            Structure lungsStructure = structureSet.Structures.First(w => w.Id.ToUpper() == lungID.ToUpper());
            Structure rightLungStructure = structureSet.Structures.First(w => w.Id.ToUpper() == rightLungID.ToUpper());
            Structure leftLungStructure = structureSet.Structures.First(w => w.Id.ToUpper() == leftLungID.ToUpper());
            MeshGeometry3D structureMesh = lungsStructure.MeshGeometry;
            Rect3D structureBox = lungsStructure.MeshGeometry.Bounds;



            VVector structureLocation = new VVector( PRMorigin[0], PRMorigin[1], PRMorigin[2] );
            VVector transferLocation = new VVector
            {
                x = structureLocation.x,
                y = structureLocation.y,
                z = structureLocation.z
            };
            //transferLocation = perfRegistration.TransformPoint(transferLocation);
            transferLocation = imageRegistration.TransformPoint(transferLocation);
            Size3D boxSize = structureBox.Size;
            int xcount = PRMsize[0];
            int ycount = PRMsize[1];
            int zcount = PRMsize[2];
            double xstart = transferLocation.x + PRMres[0] / 2;
            double ystart = transferLocation.y + PRMres[1] / 2;
            double zstart = transferLocation.z + PRMres[2] / 2;

            MessageBox.Show("PRM Origin = " + PRMorigin[0].ToString() + ", " + PRMorigin[1].ToString() + ", " + PRMorigin[2].ToString() + Environment.NewLine +
                "Transfer Location = " + transferLocation.x.ToString() + ", " + transferLocation.y.ToString()+ ", " + transferLocation.z.ToString() + Environment.NewLine +
                "PRM Size = " + patientPRM.XSize.ToString() + ", " + patientPRM.YSize.ToString() + ", " + patientPRM.ZSize.ToString() + Environment.NewLine +
                "Transfer Location = " + transferLocation.x.ToString() + ", " + transferLocation.y.ToString() + ", " + transferLocation.z.ToString() + Environment.NewLine +
                "Start Location = " + xstart.ToString() + ", " + ystart.ToString() + "," + zstart.ToString());

            VVector imageStart = new VVector();
            VVector imageStop = new VVector();
            VVector PRMstart = new VVector();
            VVector PRMstop = new VVector();
            VVector doseStart = new VVector();
            VVector doseStop = new VVector();

            double[,,] imageData = new double[xcount, ycount, zcount];
            double[,,] rightLungImageData = new double[xcount, ycount, zcount];
            double[,,] leftLungImageData = new double[xcount, ycount, zcount];
            double[,,] PRMdata = new double[xcount, ycount, zcount];
            double[,,] rightLungPRMdata = new double[xcount, ycount, zcount];
            double[,,] leftLungPRMdata = new double[xcount, ycount, zcount];
            double[,,] doseData = new double[xcount, ycount, zcount];
            double[,,] rightLungDoseData = new double[xcount, ycount, zcount];
            double[,,] leftLungDoseData = new double[xcount, ycount, zcount];

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);



            double PRMmax = 0.0;
            double lungsMaxIntensity = 0.0;
            double rightLungMaxIntensity = 0.0;
            double leftLungMaxIntensity = 0.0;
            double lungsMeanIntensity = 0.0;
            double rightLungMeanIntensity = 0.0;
            double leftLungMeanIntensity = 0.0;
            int lungsImageVoxels = 0;
            int rightLungImageVoxels = 0;
            int leftLungImageVoxels = 0;
            double rightLungMeanDose = 0.0;
            double leftLungMeanDose = 0.0;
            int rightLungDoseVoxels = 0;
            int leftLungDoseVoxels = 0;
            double lungsStdvImage = 0.0;
            double rightLungStdvImage = 0.0;
            double leftLungStdvImage = 0.0;
            double lungsMaxDose = 0.0;
            double rightLungMaxDose = 0.0;
            double leftLungMaxDose = 0.0;
            string useLQCorrection = Batch.bioCorrection;
            if (useLQCorrection == "No")
            {
                for (int z = 0; z < zcount; z++)
                {
                    for (int y = 0; y < ycount; y++)
                    {
                        PRMstart.x = xstart;
                        PRMstart.y = ystart + y * PRMres[1];
                        PRMstart.z = zstart + z * PRMres[2];
                        PRMstop.x = PRMstart.x + (xcount - 1) * PRMres[0];
                        PRMstop.y = PRMstart.y;
                        PRMstop.z = PRMstart.z;
                        imageStart = imageRegistration.TransformPoint(PRMstart);
                        imageStop = imageRegistration.TransformPoint(PRMstop);
                        doseStart = planRegistration.TransformPoint(PRMstart);
                        doseStop = planRegistration.TransformPoint(PRMstop);
                        BitArray lungsBitArray = new BitArray(xcount);
                        SegmentProfile lungsProfile = lungsStructure.GetSegmentProfile(doseStart, doseStop, lungsBitArray);
                        BitArray rightLungBitArray = new BitArray(xcount);
                        SegmentProfile rightLungProfile = rightLungStructure.GetSegmentProfile(doseStart, doseStop, rightLungBitArray);
                        BitArray leftLungBitArray = new BitArray(xcount);
                        SegmentProfile leftLungProfile = leftLungStructure.GetSegmentProfile(doseStart, doseStop, leftLungBitArray);
                        double[] doseArray = new double[xcount];
                        DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                        double[] SPECTarray = new double[xcount];
                        ImageProfile SPECTprofile = patientSPECT.GetImageProfile(imageStart, imageStop, SPECTarray);
                        double[] PRMarray = new double[xcount];
                        ImageProfile PRMprofile = patientPRM.GetImageProfile(PRMstart, PRMstop, PRMarray);
                        for (int x = 0; x < xcount; x++)
                        {
                            // RIGHT_LUNG-GTV data
                            if (rightLungProfile[x].Value == true)
                            {
                                imageData[x, y, z] = SPECTprofile[x].Value;
                                doseData[x, y, z] = doseProfile[x].Value;
                                PRMdata[x, y, z] = PRMprofile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                }
                                rightLungImageData[x, y, z] = SPECTprofile[x].Value;
                                rightLungDoseData[x, y, z] = doseProfile[x].Value;
                                rightLungPRMdata[x, y, z] = PRMprofile[x].Value;
                                if (rightLungImageData[x, y, z] > rightLungMaxIntensity)
                                {
                                    rightLungMaxIntensity = rightLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                rightLungDoseData[x, y, z] = double.NaN;
                                rightLungImageData[x, y, z] = double.NaN;
                                rightLungPRMdata[x, y, z] = double.NaN;
                            }

                            // LEFT_LUNG-GTV data
                            if (leftLungProfile[x].Value == true)
                            {
                                imageData[x, y, z] = SPECTprofile[x].Value;
                                doseData[x, y, z] = doseProfile[x].Value;
                                PRMdata[x, y, z] = PRMprofile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                }

                                leftLungImageData[x, y, z] = SPECTprofile[x].Value;
                                leftLungDoseData[x, y, z] = doseProfile[x].Value;
                                leftLungPRMdata[x, y, z] = PRMprofile[x].Value;
                                if (leftLungImageData[x, y, z] > leftLungMaxIntensity)
                                {
                                    leftLungMaxIntensity = leftLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                leftLungDoseData[x, y, z] = double.NaN;
                                leftLungImageData[x, y, z] = double.NaN;
                                leftLungPRMdata[x, y, z] = double.NaN;
                            }

                            if (leftLungProfile[x].Value == false && rightLungProfile[x].Value == false)
                            {
                                doseData[x, y, z] = double.NaN;
                                imageData[x, y, z] = double.NaN;
                                PRMdata[x, y, z] = double.NaN;
                            }

                        }
                    }
                }
            }
            else
            {
                alphaBetaValue = 2.5;
                int fractionNumber = LoadDosePlan.fractionNumber;
                for (int z = 0; z < zcount; z++)
                {
                    for (int y = 0; y < ycount; y++)
                    {
                        PRMstart.x = xstart;
                        PRMstart.y = ystart + y * PRMres[1];
                        PRMstart.z = zstart + z * PRMres[2];
                        PRMstop.x = PRMstart.x + (xcount - 1) * PRMres[0];
                        PRMstop.y = PRMstart.y;
                        PRMstop.z = PRMstart.z;
                        imageStart = imageRegistration.TransformPoint(PRMstart);
                        imageStop = imageRegistration.TransformPoint(PRMstop);
                        doseStart = planRegistration.TransformPoint(PRMstart);
                        doseStop = planRegistration.TransformPoint(PRMstop);
                        BitArray lungsBitArray = new BitArray(xcount);
                        SegmentProfile lungsProfile = lungsStructure.GetSegmentProfile(doseStart, doseStop, lungsBitArray);
                        BitArray rightLungBitArray = new BitArray(xcount);
                        SegmentProfile rightLungProfile = rightLungStructure.GetSegmentProfile(doseStart, doseStop, rightLungBitArray);
                        BitArray leftLungBitArray = new BitArray(xcount);
                        SegmentProfile leftLungProfile = leftLungStructure.GetSegmentProfile(doseStart, doseStop, leftLungBitArray);
                        double[] doseArray = new double[xcount];
                        DoseProfile doseProfile = patientDose.GetDoseProfile(doseStart, doseStop, doseArray);
                        double[] SPECTarray = new double[xcount];
                        ImageProfile SPECTprofile = patientSPECT.GetImageProfile(imageStart, imageStop, SPECTarray);
                        double[] PRMarray = new double[xcount];
                        ImageProfile PRMprofile = patientPRM.GetImageProfile(PRMstart, PRMstop, PRMarray);
                        for (int x = 0; x < xcount; x++)
                        {
                            // RIGHT_LUNG-GTV data
                            if (rightLungProfile[x].Value == true)
                            {
                                double pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                imageData[x, y, z] = SPECTprofile[x].Value;
                                doseData[x, y, z] = pointDose;
                                PRMdata[x, y, z] = PRMprofile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                }
                                if (PRMdata[x,y,z] > PRMmax)
                                {
                                    PRMmax = PRMdata[x, y, z];
                                }
                                rightLungImageData[x, y, z] = SPECTprofile[x].Value;
                                rightLungDoseData[x, y, z] = doseProfile[x].Value;
                                rightLungPRMdata[x, y, z] = PRMprofile[x].Value;
                                if (rightLungImageData[x, y, z] > rightLungMaxIntensity)
                                {
                                    rightLungMaxIntensity = rightLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                rightLungDoseData[x, y, z] = double.NaN;
                                rightLungImageData[x, y, z] = double.NaN;
                                rightLungPRMdata[x, y, z] = double.NaN;
                            }

                            // LEFT_LUNG-GTV data
                            if (leftLungProfile[x].Value == true)
                            {
                                double pointDose = doseProfile[x].Value * ((doseProfile[x].Value / fractionNumber + alphaBetaValue) / (2.0 + alphaBetaValue));
                                imageData[x, y, z] = SPECTprofile[x].Value;
                                doseData[x, y, z] = pointDose;
                                PRMdata[x, y, z] = PRMprofile[x].Value;
                                if (imageData[x, y, z] > lungsMaxIntensity)
                                {
                                    lungsMaxIntensity = imageData[x, y, z];
                                }
                                if (PRMdata[x, y, z] > PRMmax)
                                {
                                    PRMmax = PRMdata[x, y, z];
                                }
                                leftLungImageData[x, y, z] = SPECTprofile[x].Value;
                                leftLungDoseData[x, y, z] = doseProfile[x].Value;
                                leftLungPRMdata[x, y, z] = PRMprofile[x].Value;
                                if (leftLungImageData[x, y, z] > leftLungMaxIntensity)
                                {
                                    leftLungMaxIntensity = leftLungImageData[x, y, z];
                                }
                            }
                            else
                            {
                                leftLungDoseData[x, y, z] = double.NaN;
                                leftLungImageData[x, y, z] = double.NaN;
                                leftLungPRMdata[x, y, z] = double.NaN;
                            }

                            if (leftLungProfile[x].Value == false && rightLungProfile[x].Value == false)
                            {
                                doseData[x, y, z] = double.NaN;
                                imageData[x, y, z] = double.NaN;
                                PRMdata[x, y, z] = double.NaN;
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
                leftCleaningCutoff = leftLungMeanIntensity + leftLungStdvImage * 3.5;
                rightCleaningCutoff = rightLungMeanIntensity + rightLungStdvImage * 3.5;
            }

            double leftLungCutoff = leftCleaningCutoff;
            double rightLungCutoff = rightCleaningCutoff;


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
            double totalCleanedVoxels = Math.Round((double)(rightLungClean + leftLungClean) / (rightLungVoxels + leftLungVoxels) * 100, 2);

            //MessageBox.Show("Right Lung: Cutoff = " + rightCleaningCutoff.ToString() + ", % Cleaned = " + rightLungClean.ToString() + "(" + RLpercentCleaned.ToString() + ")" + Environment.NewLine
            //    + "Left Lung: Cutoff = " + leftCleaningCutoff.ToString() + ", % Cleaned = " + leftLungClean.ToString() + "(" + LLpercentCleaned.ToString() + ")");

            string contralateralLung;
            double intensityNormalizer = 0.0;
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

            if (SelectOptions.IntensityNormalize == "Relative")
            {
                lungsMaxIntensity = lungsMaxIntensity / intensityNormalizer;
                rightLungMaxIntensity = rightLungMaxIntensity / intensityNormalizer;
                leftLungMaxIntensity = leftLungMaxIntensity / intensityNormalizer;

                //string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string timepoint = patientSPECT.Series.Id.Replace(" ", "_");
                string intensityFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_" + timepoint + "_" + PRMseries +".csv";
                string doseFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_Dose_" + PRMseries + ".csv";
                string PRMfileName = "Latest\\DoseFunctionExport\\" + Program.patientID + "_" + PRMseries + ".csv";
                string metricFileName = "Latest\\DoseFunctionExport\\" + Program.patientID + PRMseries + "_lookup.csv";
                string filePathI = System.IO.Path.Combine(path, intensityFileName);
                string filePathII = System.IO.Path.Combine(path, doseFileName);
                string PRMfilePath = System.IO.Path.Combine(path, PRMfileName);
                string metricFilePath = System.IO.Path.Combine(path, metricFileName);
                StringBuilder dataBuilder = new StringBuilder();
                StringBuilder doseBuilder = new StringBuilder();
                StringBuilder PRMdataBuilder = new StringBuilder();
                StringBuilder metricBuilder = new StringBuilder();
                bool firstLine = true;
                int voxels = 0;
                double meanIntensity = 0.0;
                int lowFunctionVoxels = 0;
                double lowFunctionMean = 0.0;
                int lowFunctionOverlap = 0;
                double lowOverlapPct = 0.0;
                double lowFunctionOverlapMean = 0.0;
                int highFunctionOverlap = 0;
                double highOverlapPct = 0.0;
                double highFunctionOverlapMean = 0.0;
                int normalPRMvoxels = 0;
                double normalIntensityMean = 0.0;
                int fSADvoxels = 0;
                int emphysemaVoxels = 0;
                int parenchymalDiseaseVoxels = 0;
                bool normalBool = true;
                double volume = 0.0;
                double normalPRM = 0.0;
                double fSADPRM = 0.0;
                double emphysemaPRM = 0.0;
                double parenchymalDiseasePRM = 0.0;
                double dose2normal = 0.0;
                double dose2fSAD = 0.0;
                double dose2emphysema = 0.0;
                double dose2parenchymalDisease = 0.0;
                double dose2dysfunction = 0.0;
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

                            if (!double.IsNaN(imageData[x,y,z]) && PRMdata[x,y,z] > 0)
                            {
                                voxels++;
                                meanIntensity += imageData[x, y, z];
                                if (PRMdata[x,y,z] < 600)
                                {
                                    normalPRMvoxels++;
                                    normalIntensityMean += imageData[x, y, z];
                                    dose2normal += doseData[x, y, z];
                                    if (imageData[x,y,z] > lungsMaxIntensity * 0.15)
                                    {
                                        highFunctionOverlap++;
                                        highFunctionOverlapMean += imageData[x, y, z];
                                    }
                                }
                                else if (PRMdata[x,y,z] > 600 && PRMdata[x,y,z] < 900)
                                {
                                    fSADvoxels++;
                                    dose2fSAD += doseData[x, y, z];
                                    normalBool = false;
                                }
                                else if (PRMdata[x,y,z] > 900 && PRMdata[x,y,z] < 1400)
                                {
                                    emphysemaVoxels++;
                                    dose2emphysema += doseData[x, y, z];
                                    normalBool = false;
                                }
                                else if (PRMdata[x,y,z] > 2000)
                                {
                                    parenchymalDiseaseVoxels++;
                                    dose2parenchymalDisease += doseData[x, y, z];
                                    normalBool = false;
                                }


                                if (imageData[x,y,z] < lungsMaxIntensity * 0.15)
                                {
                                    lowFunctionVoxels++;
                                    lowFunctionMean += imageData[x, y, z];
                                    if (normalBool == false)
                                    {
                                        lowFunctionOverlap++;
                                        lowFunctionOverlapMean += imageData[x, y, z];
                                    }
                                }
                            }



                            // Write dose-function matrices to csv to facilitate imporation into MATLAB
                            if (firstLine)
                            {
                                dataBuilder.Append(timepoint + "," + xcount + "," + ycount + "," + zcount);
                                doseBuilder.Append(Program.planName + "," + xcount + "," + ycount + "," + zcount);
                                PRMdataBuilder.Append(Program.patientID + "," + xcount + "," + ycount + "," + zcount);
                                metricBuilder.Append("Patient,Volume,MeanIntensity,NormalPRMIntensity,NormalFunction,NormalFunctionPct,LowFunctionIntensity,LowFunctionDysfunctionPRM,LowFunctionDysfunctionPRMPct,Normal,dose2Normal,fSAD,dose2fSAD,Emphysema,dose2emphysema,PD,dose2PD");
                                for (int w = 0; w < xcount - 6; w++)
                                {
                                    dataBuilder.Append(",");
                                    doseBuilder.Append(",");
                                    PRMdataBuilder.Append(",");
                                }
                                dataBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);
                                doseBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);
                                PRMdataBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);


                                dataBuilder.AppendLine();
                                doseBuilder.AppendLine();
                                PRMdataBuilder.AppendLine();
                                metricBuilder.AppendLine();
                                firstLine = false;
                            }
                            // Lungs-GTV Writing
                            dataBuilder.Append(imageData[x, y, z] + ",");
                            doseBuilder.Append(doseData[x, y, z] + ",");
                            PRMdataBuilder.Append(PRMdata[x, y, z] + ",");


                            /* Ipsilateral_Lung-GTV Writing
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


                        }
                        dataBuilder.AppendLine();
                        doseBuilder.AppendLine();
                        PRMdataBuilder.AppendLine();
                    }
                    dataBuilder.AppendLine();
                    doseBuilder.AppendLine();
                    PRMdataBuilder.AppendLine();
                }
                dataBuilder.AppendLine();
                doseBuilder.AppendLine();
                PRMdataBuilder.AppendLine();
                dataBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);
                doseBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);
                PRMdataBuilder.Append(PRMres[0] + "," + PRMres[1] + "," + PRMres[2]);

                volume = voxels * PRMres[0] * PRMres[1] * PRMres[2];
                meanIntensity /= voxels;
                normalIntensityMean /= normalPRMvoxels;
                highFunctionOverlapMean /= highFunctionOverlap;
                highOverlapPct = (double)highFunctionOverlap / (double)voxels;
                lowFunctionMean /= lowFunctionVoxels;
                lowFunctionOverlapMean /= lowFunctionOverlap;
                lowOverlapPct = (double)lowFunctionOverlap / (double)voxels;
                normalPRM = (double)normalPRMvoxels / (double)voxels;
                dose2normal /= normalPRMvoxels;
                fSADPRM = (double)fSADvoxels / (double)voxels;
                dose2fSAD /= fSADvoxels;
                emphysemaPRM = (double)emphysemaVoxels / (double)voxels;
                dose2emphysema /= emphysemaVoxels;
                parenchymalDiseasePRM = (double)parenchymalDiseaseVoxels / (double)voxels;
                dose2parenchymalDisease /= parenchymalDiseaseVoxels;
                metricBuilder.Append(Program.patientID.ToString() + "," + volume.ToString() + "," + meanIntensity.ToString() + "," + normalIntensityMean.ToString() + "," + highFunctionOverlapMean.ToString() + "," + highOverlapPct.ToString() + "," + lowFunctionMean.ToString() + "," + lowFunctionOverlapMean.ToString() + "," + lowOverlapPct.ToString() + "," + normalPRM.ToString() + "," + dose2normal.ToString() + "," + fSADPRM.ToString() + "," + dose2fSAD + "," + emphysemaPRM.ToString() + "," + dose2emphysema + "," + parenchymalDiseasePRM.ToString() + "," + dose2parenchymalDisease.ToString());

                File.AppendAllText(filePathI, dataBuilder.ToString());
                File.AppendAllText(filePathII, doseBuilder.ToString());
                File.AppendAllText(PRMfilePath, PRMdataBuilder.ToString());
                File.AppendAllText(metricFilePath, metricBuilder.ToString());

            }
        }



    }
}
