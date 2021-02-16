using System;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext context)
        {
            try
            {
                Process.Start(AppExePath());
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }

        private string AppExePath()
        {
            return FirstExePathIn(AssemblyDirectory());
        }

        private string FirstExePathIn(string dir)
        {
            return Directory.GetFiles(dir, "*.exe").First();
        }

        private string AssemblyDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}