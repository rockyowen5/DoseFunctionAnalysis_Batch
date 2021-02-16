using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace DFHAnalysis
{
    class ExcelWrite
    {
        private static Excel.Application v_ExcelApp = null;
        public static Excel.Application ExcelApp
        {
            get { return v_ExcelApp; }
            set { v_ExcelApp = value; }
        }
        private static Excel.Workbook v_DataSheet = null;
        public static Excel.Workbook DataSheet
        {
            get { return v_DataSheet; }
            set { v_DataSheet = value; }
        }

        private string structureIdentifier;
        private Structure currentStructure;
        private string name;
        private StructureSet patientStructureSet;
        private Excel.Workbooks tempWorkbooks = null;
        private Excel.Sheets tempWorksheets = null;
        private Excel.Worksheet currentSheet = null;
        //private Excel.Worksheet graphSheet = null;
        private Dictionary<string, double[]> metricValueDictionary;
        private int cellNumber;
        private int header;
        private int graphStart;
        private List<string> selectedStructures;
        private double[] doseBins;
        private int binNumber;
        private double[][] cumulativeDFCounts;
        private int indexer;
        private Excel.Series currentSeries;
        private Excel.Range xRange;
        private Excel.Range yRange;
        private string xColumnStart;
        private string xColumnEnd;
        private string yColumnStart;
        private string yColumnEnd;
        private Excel.Axis xAxis;
        private Excel.Axis yAxis;
        private double countsUnderCurve;
        private double[] countsArray;
        private int structureCount;
        private int columnWidth;
        private List<VVector> maxDoseLocationList;
        private List<VVector> maxIntensityLocationList;
        private double[] averageUnderDoseThreshold;
        private double[] structureNormalizationValues;

        public void ExcelWriteData()
        {
            ExcelApp = new Excel.Application();
            ExcelApp.DisplayAlerts = false;
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = System.IO.Path.Combine(path, "Data.xlsx");
            tempWorkbooks = ExcelApp.Workbooks;
            DataSheet = tempWorkbooks.Open(filePath, 0, true, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            tempWorksheets = DataSheet.Worksheets;
            metricValueDictionary = new Dictionary<string, double[]>();
            selectedStructures = UserInterface.SelectedStructures;
            structureCount = selectedStructures.Count;
            columnWidth = Math.Max((structureCount * 2 + 1), 12);

            Excel.Worksheet headerSheet = tempWorksheets.Item["Sheet1"];
            headerSheet.Name = "Research Data";
            for (int i = 1; i < 37; i++)
            {
                headerSheet.Columns[i].ColumnWidth = 11.50;
            }
            headerSheet.Cells[1, 1] = "User:";
            headerSheet.Cells[1, 2] = Script.CurrentUser.Id;
            headerSheet.Cells[2, 1] = "Patient";
            headerSheet.Cells[2, 2] = Script.CurrentPatient.Id;
            DateTime timeStamp = DateTime.Now;
            headerSheet.Cells[1, 4] = timeStamp.ToString();
            headerSheet.Cells[5, 1] = "Metric Data:";
            headerSheet.Cells[6, 1] = "Max Dose Right Lung [Gy]";
            headerSheet.Cells[6, 2] = "Mean Dose Right Lung [Gy]";
            headerSheet.Cells[6, 3] = "Max Dose Left Lung [Gy]";
            headerSheet.Cells[6, 4] = "Mean Dose Left Lung [Gy]";
            headerSheet.Cells[5, 5] = "Initial Perfusion:";
            headerSheet.Cells[6, 5] = "Right Lung gEUfD";
            headerSheet.Cells[6, 6] = "Right Lung FV20";
            headerSheet.Cells[6, 7] = "Left Lung gEUfD";
            headerSheet.Cells[6, 8] = "Left Lung FV20";
            headerSheet.Cells[5, 9] = "Initial Ventilation:";
            headerSheet.Cells[6, 9] = "Right Lung gEUfD";
            headerSheet.Cells[6, 10] = "Right Lung FV20";
            headerSheet.Cells[6, 11] = "Left Lung gEUfD";
            headerSheet.Cells[6, 12] = "Left Lung FV20";
            headerSheet.Cells[5, 13] = "1Month Perfusion:";
            headerSheet.Cells[6, 13] = "Right Lung gEUfD";
            headerSheet.Cells[6, 14] = "Right Lung FV20";
            headerSheet.Cells[6, 15] = "Left Lung gEUfD";
            headerSheet.Cells[6, 16] = "Left Lung FV20";
            headerSheet.Cells[5, 17] = "1Month Ventilation:";
            headerSheet.Cells[6, 17] = "Right Lung gEUfD";
            headerSheet.Cells[6, 18] = "Right Lung FV20";
            headerSheet.Cells[6, 19] = "Left Lung gEUfD";
            headerSheet.Cells[6, 20] = "Left Lung FV20";
            headerSheet.Cells[5, 21] = "3Month Perfusion:";
            headerSheet.Cells[6, 21] = "Right Lung gEUfD";
            headerSheet.Cells[6, 22] = "Right Lung FV20";
            headerSheet.Cells[6, 23] = "Left Lung gEUfD";
            headerSheet.Cells[6, 24] = "Left Lung FV20";
            headerSheet.Cells[5, 25] = "3Month Ventilation:";
            headerSheet.Cells[6, 25] = "Right Lung gEUfD";
            headerSheet.Cells[6, 26] = "Right Lung FV20";
            headerSheet.Cells[6, 27] = "Left Lung gEUfD";
            headerSheet.Cells[6, 28] = "Left Lung FV20";
            headerSheet.Cells[5, 29] = "1Year Perfusion:";
            headerSheet.Cells[6, 29] = "Right Lung gEUfD";
            headerSheet.Cells[6, 30] = "Right Lung FV20";
            headerSheet.Cells[6, 31] = "Left Lung gEUfD";
            headerSheet.Cells[6, 32] = "Left Lung FV20";
            headerSheet.Cells[5, 33] = "1Year Ventilation:";
            headerSheet.Cells[6, 33] = "Right Lung gEUfD";
            headerSheet.Cells[6, 34] = "Right Lung FV20";
            headerSheet.Cells[6, 35] = "Left Lung gEUfD";
            headerSheet.Cells[6, 36] = "Left Lung FV20";
            Excel.Range rng = headerSheet.Range["A5", "AJ5"];
            rng.Font.Bold = true;
            rng = headerSheet.Range["A5", "AJ6"];
            rng.WrapText = true;
            name = LoadDosePlan.NameDictionary[0];
            metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
            double[] rightLungData = metricValueDictionary["RIGHT_LUNG"];
            double[] leftLungData = metricValueDictionary["LEFT_LUNG"];
            headerSheet.Cells[7, 1] = rightLungData[2].ToString();
            headerSheet.Cells[7, 2] = rightLungData[3].ToString();
            headerSheet.Cells[7, 3] = leftLungData[2].ToString();
            headerSheet.Cells[7, 4] = leftLungData[3].ToString();
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("Initial PERF")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("Initial PERF")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 5] = rightLungData[10].ToString();
                headerSheet.Cells[7, 6] = rightLungData[11].ToString();
                headerSheet.Cells[7, 7] = leftLungData[10].ToString();
                headerSheet.Cells[7, 8] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 5] = "-";
                headerSheet.Cells[7, 6] = "-";
                headerSheet.Cells[7, 7] = "-";
                headerSheet.Cells[7, 8] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("Initial VENT")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("Initial VENT")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 9] = rightLungData[10].ToString();
                headerSheet.Cells[7, 10] = rightLungData[11].ToString();
                headerSheet.Cells[7, 11] = leftLungData[10].ToString();
                headerSheet.Cells[7, 12] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 9] = "-";
                headerSheet.Cells[7, 10] = "-";
                headerSheet.Cells[7, 11] = "-";
                headerSheet.Cells[7, 12] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("1Month PERF")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("1Month PERF")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 13] = rightLungData[10].ToString();
                headerSheet.Cells[7, 14] = rightLungData[11].ToString();
                headerSheet.Cells[7, 15] = leftLungData[10].ToString();
                headerSheet.Cells[7, 16] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 13] = "-";
                headerSheet.Cells[7, 14] = "-";
                headerSheet.Cells[7, 15] = "-";
                headerSheet.Cells[7, 16] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("1Month VENT")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("1Month VENT")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 17] = rightLungData[10].ToString();
                headerSheet.Cells[7, 18] = rightLungData[11].ToString();
                headerSheet.Cells[7, 19] = leftLungData[10].ToString();
                headerSheet.Cells[7, 20] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 17] = "-";
                headerSheet.Cells[7, 18] = "-";
                headerSheet.Cells[7, 19] = "-";
                headerSheet.Cells[7, 20] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("3Month PERF")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("3Month PERF")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 21] = rightLungData[10].ToString();
                headerSheet.Cells[7, 22] = rightLungData[11].ToString();
                headerSheet.Cells[7, 23] = leftLungData[10].ToString();
                headerSheet.Cells[7, 24] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 21] = "-";
                headerSheet.Cells[7, 22] = "-";
                headerSheet.Cells[7, 23] = "-";
                headerSheet.Cells[7, 24] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("3Month VENT")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("3Month VENT")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 25] = rightLungData[10].ToString();
                headerSheet.Cells[7, 26] = rightLungData[11].ToString();
                headerSheet.Cells[7, 27] = leftLungData[10].ToString();
                headerSheet.Cells[7, 28] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 25] = "-";
                headerSheet.Cells[7, 26] = "-";
                headerSheet.Cells[7, 27] = "-";
                headerSheet.Cells[7, 28] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("1Year PERF")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("1Year PERF")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 29] = rightLungData[10].ToString();
                headerSheet.Cells[7, 30] = rightLungData[11].ToString();
                headerSheet.Cells[7, 31] = leftLungData[10].ToString();
                headerSheet.Cells[7, 32] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 29] = "-";
                headerSheet.Cells[7, 30] = "-";
                headerSheet.Cells[7, 31] = "-";
                headerSheet.Cells[7, 32] = "-";
            }
            if (LoadDosePlan.NameDictionary.Any(n => n.Value.Contains("1Year VENT")))
            {
                name = LoadDosePlan.NameDictionary[LoadDosePlan.NameDictionary.First(n => n.Value.Contains("1Year VENT")).Key];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                rightLungData = metricValueDictionary["RIGHT_LUNG"];
                leftLungData = metricValueDictionary["LEFT_LUNG"];
                headerSheet.Cells[7, 33] = rightLungData[10].ToString();
                headerSheet.Cells[7, 34] = rightLungData[11].ToString();
                headerSheet.Cells[7, 35] = leftLungData[10].ToString();
                headerSheet.Cells[7, 36] = leftLungData[11].ToString();
            }
            else
            {
                headerSheet.Cells[7, 33] = "-";
                headerSheet.Cells[7, 34] = "-";
                headerSheet.Cells[7, 35] = "-";
                headerSheet.Cells[7, 36] = "-";
            }

            for (int j = 0; j < UserInterface.LoadedPlans; j++)
            {
                currentSheet = DataSheet.Worksheets.Add(Type.Missing, tempWorksheets[DataSheet.Worksheets.Count], Type.Missing, Type.Missing);
                for (int c = 1; c < columnWidth; c++)
                {
                    currentSheet.Columns[c].ColumnWidth = 17.25;
                }
                name = LoadDosePlan.NameDictionary[j];
                string excelName = name.Replace("/", "-");
                currentSheet.Name = excelName;
                patientStructureSet = LoadDosePlan.StructureSets[name];
                metricValueDictionary = MetricAnalysis.NameMetricDictionary[name];
                currentSheet.Cells[1, 1] = "User:";
                currentSheet.Cells[1, 2] = Script.CurrentUser.Id;
                DateTime currentTime = DateTime.Now;
                currentSheet.Cells[1, 5] = "File Created:";
                currentSheet.Cells[1, 5].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                currentSheet.Cells[1, 5].Font.Bold = true;
                currentSheet.Cells[1, 6] = currentTime.ToString();
                currentSheet.Cells[2, 5] = "Load Plan Run Time (sec)";
                currentSheet.Cells[2, 5].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                currentSheet.Cells[2, 5].Font.Bold = true;
                currentSheet.Cells[2, 6] = UserInterface.LoadPlanTimes[name];
                currentSheet.Cells[3, 5] = "Metric Run Time (sec)";
                currentSheet.Cells[3, 5].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                currentSheet.Cells[3, 5].Font.Bold = true;
                UserInterface.RunTime.Stop();
                double elapsedRunTime = Math.Round(Convert.ToDouble(UserInterface.RunTime.Elapsed.TotalSeconds), 1);
                currentSheet.Cells[3, 6] = elapsedRunTime;
                currentSheet.Cells[2, 1] = "Patient:";
                currentSheet.Cells[2, 2] = Script.CurrentPatient.Id;
                currentSheet.Cells[3, 1] = "Image File:";
                currentSheet.Cells[3, 2] = SelectSPECTImage.ImageNameDictionary[j];
                currentSheet.Cells[4, 1] = "Dose Plan:";
                currentSheet.Cells[4, 2] = SelectDosePlan.DoseNameDictionary[j];
                currentSheet.Cells[5, 1] = "Registration:";
                currentSheet.Cells[5, 2] = LoadDosePlan.RegistrationDictionary[name].Id;
                currentSheet.Cells[6, 1] = "Bio-Correction:";
                currentSheet.Cells[6, 2] = SelectBioCorrection.BioCorrectDictionary[j];
                currentSheet.Cells[7, 1] = "Normalization:";
                if (SelectOptions.NormalizeStrategy == "Structure")
                {
                    structureNormalizationValues = Normalizer.StructureNormalizationValue[name];
                    currentSheet.Cells[7, 2] = "Dose = " + SelectOptions.DoseNormalize + ", Intensity = " + SelectOptions.IntensityNormalize + ", Normalization Strategy = " +
                        SelectOptions.NormalizeStrategy + " (" + Math.Round(structureNormalizationValues[0], 2) + ", " + Math.Round(structureNormalizationValues[1], 2) + " Gy" + ")";
                }
                else if (SelectOptions.NormalizeStrategy == "Average Counts Under Dose Threshold")
                {
                    averageUnderDoseThreshold = Normalizer.AverageUnderDoseThreshold[name];
                    currentSheet.Cells[7, 2] = "Dose = " + SelectOptions.DoseNormalize + ", Intensity = " + SelectOptions.IntensityNormalize + ", Normalization Strategy = " +
                        SelectOptions.NormalizeStrategy + " of " + SelectOptions.DoseThreshold.ToString() +" Gy" +
                        " (" + Math.Round(averageUnderDoseThreshold[0], 2) + ", " + Math.Round(averageUnderDoseThreshold[1], 2) + " Gy" + ")";
                }
                else
                {
                    currentSheet.Cells[7, 2] = "Dose = " + SelectOptions.DoseNormalize + ", Intensity = " + SelectOptions.IntensityNormalize + ", Normalization Strategy = " +
                        SelectOptions.NormalizeStrategy;
                }
                currentSheet.Cells[9, 1] = "Metric Data:";
                Excel.Range header1Range = currentSheet.Range["A1", "A9"];
                header1Range.Font.Bold = true;
                header1Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                header = 10;
                currentSheet.Cells[header, 1].EntireRow.Font.Bold = true;
                currentSheet.Cells[header, 1] = "Structure";
                if (SelectBioCorrection.BioCorrectDictionary[j] == "Yes")
                {
                    currentSheet.Cells[header, 2] = "Structure \u03B1/\u03B2";
                    currentSheet.Cells[header, 3] = "a Value";
                    currentSheet.Cells[header, 4] = "% in Image FOV";
                    currentSheet.Cells[header, 5] = "Volume";
                    currentSheet.Cells[header, 6] = "Max Dose";
                    currentSheet.Cells[header, 7] = "Mean Dose";
                    currentSheet.Cells[header, 8] = "StDev Dose";
                    currentSheet.Cells[header, 9] = "gEUD";
                    currentSheet.Cells[header, 10] = "V20";
                    currentSheet.Cells[header, 11] = "Max Intensity";
                    currentSheet.Cells[header, 12] = "Mean Intensity";
                    currentSheet.Cells[header, 13] = "StDev Intensity";
                    currentSheet.Cells[header, 14] = "gEUfD";
                    currentSheet.Cells[header, 15] = "fV20";
                    currentSheet.Cells[header, 16] = "MfLD";
                    currentSheet.Cells[header, 17] = "Max Dose Location";
                    currentSheet.Cells[header, 18] = "Max Intensity Location";
                }
                else
                {
                    currentSheet.Cells[header, 2] = "a Value";
                    currentSheet.Cells[header, 3] = "% in Image FOV";
                    currentSheet.Cells[header, 4] = "Volume";
                    currentSheet.Cells[header, 5] = "Max Dose";
                    currentSheet.Cells[header, 6] = "Mean Dose";
                    currentSheet.Cells[header, 7] = "StDev Dose";
                    currentSheet.Cells[header, 8] = "gEUD";
                    currentSheet.Cells[header, 9] = "V20";
                    currentSheet.Cells[header, 10] = "Max Intensity";
                    currentSheet.Cells[header, 11] = "Mean Intensity";
                    currentSheet.Cells[header, 12] = "StDev Intensity";
                    currentSheet.Cells[header, 13] = "gEUfD";
                    currentSheet.Cells[header, 14] = "fV20";
                    currentSheet.Cells[header, 15] = "MfLD";
                    currentSheet.Cells[header, 16] = "Max Dose Location";
                    currentSheet.Cells[header, 17] = "Max Intensity Location";
                }

                cellNumber = header + 1;
                maxDoseLocationList = MetricAnalysis.MaxDoseLocations[name];
                maxIntensityLocationList = MetricAnalysis.MaxIntensityLocations[name];

                for (int i = 0; i < structureCount; i++)
                {
                    structureIdentifier = selectedStructures[i];
                    currentStructure = patientStructureSet.Structures.First(s => s.Id == structureIdentifier);
                    currentSheet.Cells[cellNumber + i, 1] = currentStructure.Id;
                    if (SelectBioCorrection.BioCorrectDictionary[j] == "Yes")
                    {
                        currentSheet.Cells[cellNumber + i, 2] = UserInterface.AlphaBetaDictionary[currentStructure.Id];
                        currentSheet.Cells[cellNumber + i, 3] = UserInterface.AValueDictionary[currentStructure.Id];
                    }
                    else
                    {
                        currentSheet.Cells[cellNumber + i, 2] = UserInterface.AValueDictionary[currentStructure.Id];
                    }
                    double[] structureValues = metricValueDictionary[currentStructure.Id];
                    for (int c = 0; c < 13; c++)
                    {
                        if (SelectBioCorrection.BioCorrectDictionary[j] == "Yes")
                        {
                            currentSheet.Cells[cellNumber + i, c + 4] = structureValues[c];
                            currentSheet.Cells[cellNumber + i, 17] = "(" + Math.Round(maxDoseLocationList[i].x, 2) + ", " + maxDoseLocationList[i].y +
                                ", " + maxDoseLocationList[i].z + ")";
                            currentSheet.Cells[cellNumber + i, 18] = "(" + Math.Round(maxIntensityLocationList[i].x, 2) + ", " + maxIntensityLocationList[i].y +
                                ", " + maxIntensityLocationList[i].z + ")";
                        }
                        else
                        {
                            currentSheet.Cells[cellNumber + i, c + 3] = structureValues[c];
                            currentSheet.Cells[cellNumber + i, 16] = "(" + Math.Round(maxDoseLocationList[i].x, 2) + ", " + maxDoseLocationList[i].y +
                                ", " + maxDoseLocationList[i].z + ")";
                            currentSheet.Cells[cellNumber + i, 17] = "(" + Math.Round(maxIntensityLocationList[i].x, 2) + ", " + maxIntensityLocationList[i].y +
                                ", " + maxIntensityLocationList[i].z + ")";
                        }
                    }
                }

                // Write Data for DFH
                binNumber = MetricAnalysis.PlanBinNumber[name];
                doseBins = MetricAnalysis.PlanDoseBins[name];
                cumulativeDFCounts = MetricAnalysis.PlanCumulativeCounts[name];
                countsArray = MetricAnalysis.CountsUnderCurve[name];
                graphStart = header + selectedStructures.Count + 6;
                currentSheet.Cells[header + structureCount + 2, 1] = "DFH Graph Data:";
                currentSheet.Cells[header + structureCount + 2, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                for (int i = 0; i < structureCount; i++)
                {
                    structureIdentifier = selectedStructures[i];
                    countsUnderCurve = Math.Round(countsArray[i], 1);
                    currentSheet.Cells[header + structureCount + 3, i * 2 + 1] = structureIdentifier;
                    currentSheet.Cells[header + structureCount + 4, i * 2 + 1] = "Area__Curve:";
                    currentSheet.Cells[header + structureCount + 4, i * 2 + 2] = countsUnderCurve;
                    if (SelectOptions.DoseNormalize == "Relative")
                    {
                        currentSheet.Cells[header + structureCount + 5, i * 2 + 1] = "% Dose";
                    }
                    else
                    {
                        currentSheet.Cells[header + structureCount + 5, i * 2 + 1] = "Dose (Gy)";
                    }
                    if (SelectOptions.IntensityNormalize == "Relative")
                    {
                        currentSheet.Cells[header + structureCount + 5, i * 2 + 2] = "% Intensity";
                    }
                    else
                    {
                        currentSheet.Cells[header + structureCount + 5, i * 2 + 2] = "Intensity Counts";
                    }
                    indexer = MetricAnalysis.StructureVariables[structureIdentifier];
                    for (int b = 0; b < binNumber + 1; b++)
                    {
                        currentSheet.Cells[graphStart + b, i * 2 + 1] = doseBins[b];
                        currentSheet.Cells[graphStart + b, i * 2 + 2] = cumulativeDFCounts[indexer][b];
                    }
                }
                currentSheet.Rows[header + structureCount + 2].Font.Bold = true;
                currentSheet.Rows[header + structureCount + 3].Font.Bold = true;
                currentSheet.Rows[header + structureCount + 5].Font.Bold = true;

                // Add Excel Chart to graph DFH Data
                Excel.ChartObjects chartsDFH = currentSheet.ChartObjects();
                Excel.ChartObject chartGraph = chartsDFH.Add(10, 80, 300, 250);
                Excel.Chart chartPage = chartGraph.Chart;
                chartPage.ChartType = Excel.XlChartType.xlXYScatterLines;
                Excel.SeriesCollection graphSeries = chartGraph.Chart.SeriesCollection(Type.Missing);
                string graphName = excelName + " DFH";
                if (graphName.Length > 30)
                {
                    graphName = graphName.Substring(0, 30);
                }
                chartPage.HasTitle = false;
                xAxis = chartPage.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary);
                xAxis.HasMajorGridlines = false;
                xAxis.MinimumScaleIsAuto = false;
                xAxis.MinimumScale = 0;
                xAxis.HasTitle = true;
                if (SelectOptions.DoseNormalize == "Relative")
                {
                    xAxis.MaximumScale = 110;
                    xAxis.AxisTitle.Text = "% Dose";
                }
                else
                {
                    xAxis.MaximumScale = binNumber;
                    xAxis.AxisTitle.Text = "Dose (Gy)";
                }
                yAxis = chartPage.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
                yAxis.HasMajorGridlines = false;
                yAxis.MinimumScaleIsAuto = false;
                yAxis.MinimumScale = 0;
                yAxis.HasTitle = true;
                if (SelectOptions.IntensityNormalize == "Relative")
                {
                    yAxis.MaximumScale = 110;
                    if (SelectOptions.NormalizeStrategy == "Total Counts/Maximum Dose")
                    {
                        yAxis.AxisTitle.Text = "% Intensity";
                    }
                    else
                    {
                        yAxis.AxisTitle.Text = "Relative Intensity";
                    }
                }
                else
                {
                    yAxis.MaximumScaleIsAuto = true;
                    yAxis.AxisTitle.Text = "Intensity Counts";
                }


                for (int s = 0; s < structureCount; s++)
                {
                    structureIdentifier = selectedStructures[s];
                    indexer = MetricAnalysis.StructureVariables[structureIdentifier];
                    currentSeries = graphSeries.NewSeries();
                    currentSeries.Name = structureIdentifier;
                    int xDividend = s * 2 + 1;
                    string xColumnName = String.Empty;
                    int xModulo;
                    while (xDividend > 0)
                    {
                        xModulo = (xDividend - 1) % 26;
                        xColumnName = Convert.ToChar(65 + xModulo).ToString() + xColumnName;
                        xDividend = (int)((xDividend - xModulo) / 26);
                    }
                    xColumnStart = xColumnName + graphStart.ToString();
                    xColumnEnd = xColumnName + (graphStart + binNumber - 1).ToString();
                    int yDividend = s * 2 + 2;
                    string yColumnName = String.Empty;
                    int yModulo;
                    while (yDividend > 0)
                    {
                        yModulo = (yDividend - 1) % 26;
                        yColumnName = Convert.ToChar(65 + yModulo).ToString() + yColumnName;
                        yDividend = (int)((yDividend - yModulo) / 26);
                    }
                    yColumnStart = yColumnName + graphStart.ToString();
                    yColumnEnd = yColumnName + (graphStart + binNumber - 1).ToString();
                    xRange = currentSheet.Range[xColumnStart, xColumnEnd];
                    yRange = currentSheet.Range[yColumnStart, yColumnEnd];
                    currentSeries.XValues = xRange;
                    currentSeries.Values = yRange;
                    currentSeries.MarkerStyle = Excel.XlMarkerStyle.xlMarkerStyleNone;
                }
                chartPage.Location(Excel.XlChartLocation.xlLocationAsNewSheet, graphName);
            }
            string updatePath = System.IO.Path.Combine(path, "DataUpdate.xlsx");
            DataSheet.SaveAs(updatePath,Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, true, false, Excel.XlSaveAsAccessMode.xlNoChange,
                Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
            DataSheet.Close();
            ExcelApp.Quit();
        }

        public void ShowExcel()
        {
            ExcelApp = new Excel.Application();
            ExcelApp.Visible = true;
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string updatePath = System.IO.Path.Combine(path, "DataUpdate.xlsx");
            tempWorkbooks = ExcelApp.Workbooks;
            DataSheet = tempWorkbooks.Open(updatePath, 0, true, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            DataSheet.Worksheets[1].Select(Type.Missing);
        }


    }
}
