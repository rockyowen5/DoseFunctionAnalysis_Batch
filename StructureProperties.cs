using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DFHAnalysis
{
    public partial class StructureProperties : Form
    {
        private double alphaBetaValue;
        private double aValue;
        private string structureName;

        public StructureProperties()
        {
            InitializeComponent();
        }

        private void StructureProperties_Load(object sender, EventArgs e)
        {
            structureName = UserInterface.SelectedStructureName;
            this.Text = structureName;
            label1.Text = "Structure \u03B1/\u03B2 = ";
            textBox1.Text = UserInterface.AlphaBetaDictionary[structureName].ToString();
            textBox2.Text = UserInterface.AValueDictionary[structureName].ToString();
            this.AcceptButton = button1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                alphaBetaValue = Convert.ToDouble(this.textBox1.Text);
                UserInterface.AlphaBetaDictionary[structureName] = alphaBetaValue;
                aValue = Convert.ToDouble(this.textBox2.Text);
                UserInterface.AValueDictionary[structureName] = aValue;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Please enter only valid numerical inputs.", "Error");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
