using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DFHAnalysis;

namespace DFH_Initiate
{
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void Loading_Load(object sender, EventArgs e)
        {
            label2.Text = Program.patientCount.ToString();
            label4.Text = Batch.analyzeStructures.Count.ToString();
        }
    }
}
