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
    public partial class BackgroundWorker : Form
    {
        public BackgroundWorker()
        {
            InitializeComponent();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void BackgroundWorker_Load(object sender, EventArgs e)
        {
            label3.Text = Program.loadProgress.ToString();
            label5.Text = Batch.patientIDs.Count.ToString();
        }
    }
}
