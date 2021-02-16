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
using System.Diagnostics;

namespace DFH_Initiate
{
    public partial class PatientLoadStatus : Form
    {
        public static bool breakBool = false;

        public PatientLoadStatus()
        {
            InitializeComponent();
        }

        private void PatientLoadStatus_Load(object sender, EventArgs e)
        {
            label1.Text = "Patient " + Batch.patientIDs[Program.loadProgress - 1].ToString() + " was loaded.";
            Timer waitTimer = new Timer();
            waitTimer.Interval = 3000;
            waitTimer.Tick += new EventHandler(waitTimer_Tick);
            waitTimer.Start();
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Abort abortRun = new Abort();
            abortRun.ShowDialog();
            if (abortRun.DialogResult == DialogResult.Abort)
            {
                breakBool = true;
            }
        }
    }
}
