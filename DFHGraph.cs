using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using VMS.TPS;

namespace DFHAnalysis
{
    public partial class DFHGraph : Form
    {
        private string name;
        private int binNumber;
        private double[] doseBins;
        private double[][] cumulativeDFCounts;
        private string graphName;
        private string structureGraphName;
        private int binMax = 0;
        private List<string> nameList;
        private int seriesIndex;
        private int seriesCount;

        public DFHGraph()
        {
            InitializeComponent();
        }

        private void DFHGraph_Load(object sender, EventArgs e)
        {
            checkBox2.Checked = true;
            checkBox2.Text = "Turn Legend OFF";
            textBox1.Text = 2.ToString();
            chart1.ChartAreas[0].AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chart1.ChartAreas[0].AxisX.Interval = 5;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Title = "% Intensity";
            if (SelectOptions.IntensityNormalize == "Absolute")
            {
                chart1.ChartAreas[0].AxisY.IsLogarithmic = true;
            }
            else
            {
                chart1.ChartAreas[0].AxisY.Maximum = 100;
            }
            if (SelectOptions.DoseNormalize == "Relative")
            {
                chart1.ChartAreas[0].AxisX.Maximum = 100;
            }

            chart1.ChartAreas[0].AxisX.Title = "Dose (Gy)";
            chart1.ChartAreas[0].AxisX.TitleFont = new Font("Rockwell", 14, FontStyle.Bold);
            chart1.ChartAreas[0].AxisY.TitleFont = new Font("Rockwell", 14, FontStyle.Bold);
            chart1.Legends[0].LegendItemOrder = System.Windows.Forms.DataVisualization.Charting.LegendItemOrder.ReversedSeriesOrder;
            chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            nameList = new List<string>();

            for (int i = 0; i < UserInterface.SelectedStructures.Count; i++)
            {
                string structureIdentifier = UserInterface.SelectedStructures[i];
                seriesCount = 0;
                for (int p = 0; p < UserInterface.LoadedPlans; p++)
                {
                    name = LoadDosePlan.NameDictionary[p];
                    binNumber = MetricAnalysis.PlanBinNumber[name];
                    if (binNumber > binMax)
                        binMax = binNumber;
                    doseBins = MetricAnalysis.PlanDoseBins[name];
                    cumulativeDFCounts = MetricAnalysis.PlanCumulativeCounts[name];
                    graphName = LoadDosePlan.GraphNameDictionary[p];
                    structureGraphName = structureIdentifier + " " + graphName;
                    this.chart1.Series.Add(structureGraphName);
                    this.chart1.Series[structureGraphName].ChartType = System.Windows.Forms.
                        DataVisualization.Charting.SeriesChartType.Spline;
                    this.chart1.Series[structureGraphName].BorderWidth = 2;
                    int indexer = MetricAnalysis.StructureVariables[structureIdentifier];
                    for (int j = 0; j < binNumber + 1; j++)
                    {
                        if (SelectOptions.IntensityNormalize == "Absolute")
                        {
                            if (cumulativeDFCounts[indexer][j] > 0.0)
                            {
                                this.chart1.Series[structureGraphName].Points.AddXY(doseBins[j], cumulativeDFCounts[indexer][j]);
                            }
                        }
                        else
                        {
                            this.chart1.Series[structureGraphName].Points.AddXY(doseBins[j], cumulativeDFCounts[indexer][j]);
                        }
                    }
                    string seriesIdentifier = structureIdentifier + name.Substring(7, name.Length - 7);
                    seriesCount = nameList.FindAll(s => s.Equals(seriesIdentifier)).Count;
                    if (seriesCount > 0)
                    {
                        seriesIndex = nameList.IndexOf(seriesIdentifier);
                        this.chart1.Series[structureGraphName].Color = this.chart1.Series[seriesIndex].Color;
                        if (seriesCount == 1)
                            this.chart1.Series[structureGraphName].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
                        if (seriesCount == 2)
                            this.chart1.Series[structureGraphName].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
                        if (seriesCount == 3)
                            this.chart1.Series[structureGraphName].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
                    }
                    else
                    {
                        chart1.ApplyPaletteColors();
                    }
                    nameList.Add(seriesIdentifier);
                }
            }
            //chart1.ChartAreas[0].AxisX.Maximum = binMax;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            }
            else
            {
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                int lineWidth = Convert.ToInt32(textBox1.Text);
                textBox1.Text = lineWidth.ToString();
                foreach (Series series in chart1.Series)
                {
                    series.BorderWidth = lineWidth;
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                chart1.Legends[0].Enabled = true;
                checkBox2.Text = "Turn Legend OFF";
            }
            else
            {
                chart1.Legends[0].Enabled = false;
                checkBox2.Text = "Turn Legend ON";
            }
        }
    }
}
