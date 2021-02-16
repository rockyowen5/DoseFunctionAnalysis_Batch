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
    public partial class MetricOptions : Form
    {
        private static List<double> v_aParameter = new List<double>() { 1 };
        public static List<double> aParameter
        {
            get { return v_aParameter; }
            set { v_aParameter = value; }
        }
        private static List<double> v_fVDvalue = new List<double>() { 20 };
        public static List<double> fVDvalue
        {
            get { return v_fVDvalue; }
            set { v_fVDvalue = value; }
        }
        private static List<double> v_Vfvalue = new List<double>() { 20, 50 };
        public static List<double> Vfvalue
        {
            get { return v_Vfvalue; }
            set { v_Vfvalue = value; }
        }
        private static string v_aRadio = "Value";
        public static string aRadio
        {
            get { return v_aRadio; }
            set { v_aRadio = value; }
        }
        private static string v_dRadio = "Value";
        public static string dRadio
        {
            get { return v_dRadio; }
            set { v_dRadio = value; }
        }
        private static string v_lRadio = "Value";
        public static string lRadio
        {
            get { return v_lRadio; }
            set { v_lRadio = value; }
        }

        public MetricOptions()
        {
            InitializeComponent();
        }

        private void MetricOptions_Load(object sender, EventArgs e)
        {
            if (aRadio == "Value")
            {
                radioButton1.Checked = true;
                for (int i = 0; i < aParameter.Count; i++)
                {
                    listBox1.Items.Add("a = " + aParameter[i].ToString());
                }
            }
            else if (aRadio == "Range")
            {
                radioButton2.Checked = true;
                for (int i = 0; i < aParameter.Count; i++)
                {
                    listBox1.Items.Add("a = " + aParameter[i].ToString());
                }
            }
            if (dRadio == "Value")
            {
                radioButton4.Checked = true;
                for (int i = 0; i < fVDvalue.Count; i++)
                {
                    listBox2.Items.Add("D = " + fVDvalue[i].ToString());
                }
            }
            else if (dRadio == "Range")
            {
                radioButton3.Checked = true;
                for (int i = 0; i < fVDvalue.Count; i++)
                {
                    listBox2.Items.Add("D = " + fVDvalue[i].ToString());
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label5.Visible = true;
            textBox4.Visible = true;
            button1.Visible = true;
            button6.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            textBox3.Visible = false;
            button5.Visible = false;
            aRadio = "Value";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label5.Visible = false;
            textBox4.Visible = false;
            button1.Visible = false;
            button6.Visible = false;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox3.Visible = true;
            button5.Visible = true;
            aRadio = "Range";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            label7.Visible = true;
            textBox5.Visible = true;
            button2.Visible = true;
            button8.Visible = true;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
            textBox7.Visible = false;
            textBox8.Visible = false;
            button7.Visible = false;
            textBox6.Visible = false;
            dRadio = "Value";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            label7.Visible = false;
            textBox5.Visible = false;
            button2.Visible = false;
            button8.Visible = false;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            textBox7.Visible = true;
            textBox8.Visible = true;
            button7.Visible = true;
            textBox6.Visible = true;
            dRadio = "Range";
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button6;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button7;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button7;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button7;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button5;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button5;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button5;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            aParameter.Add(Convert.ToDouble(textBox4.Text));
            string itemAdd = "a = " + textBox4.Text;
            listBox1.Items.Add(itemAdd);
            textBox4.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            aParameter.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            fVDvalue.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            fVDvalue.Add(Convert.ToDouble(textBox5.Text));
            string itemAdd = "D = " + textBox5.Text;
            listBox2.Items.Add(itemAdd);
            textBox5.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Please enter upper/lower bounds for the range and the number of values to be analyzed.");
            }
            else
            {
                double startRange = Convert.ToDouble(textBox1.Text);
                double stopRange = Convert.ToDouble(textBox2.Text);
                int numVals = Convert.ToInt32(textBox3.Text);
                double interval = (stopRange - startRange) / numVals;
                listBox1.Items.Clear();
                aParameter.Clear();
                for (int i = 0; i < numVals + 1; i++)
                {
                    double a = startRange + i * interval;
                    aParameter.Add(a);
                    listBox1.Items.Add("a = " + a);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox6.Text) || string.IsNullOrEmpty(textBox7.Text) || string.IsNullOrEmpty(textBox8.Text))
            {
                MessageBox.Show("Please enter upper/lower bounds for the range and the number of values to be analyzed.");
            }
            else
            {
                double startRange = Convert.ToDouble(textBox8.Text);
                double stopRange = Convert.ToDouble(textBox7.Text);
                int numVals = Convert.ToInt32(textBox6.Text);
                double interval = (stopRange - startRange) / numVals;
                listBox2.Items.Clear();
                fVDvalue.Clear();
                for (int i = 0; i < numVals + 1; i++)
                {
                    double fVD = startRange + i * interval;
                    fVDvalue.Add(fVD);
                    listBox2.Items.Add("D = " + fVD);
                }
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button8;
        }
    }
}
