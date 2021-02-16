using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DFHAnalysis
{
    class LoadDosePlan
    {
        // Global variables.
        private static Dictionary<int, string> v_NameDictionary = new Dictionary<int, string>();
        public static Dictionary<int, string> NameDictionary
        {
            get { return v_NameDictionary; }
            set { v_NameDictionary = value; }
        }
        private static Dictionary<string,StructureSet> v_StructureSets = new Dictionary<string, StructureSet>();
        public static Dictionary<string,StructureSet> StructureSets
        {
            get { return v_StructureSets; }
            set { v_StructureSets = value; }
        }
        private static Dictionary<int, Dose> v_DosePlans = new Dictionary<int, Dose>();
        public static Dictionary<int, Dose> DosePlans
        {
            get { return v_DosePlans; }
            set { v_DosePlans = value; }
        }
        private static Dictionary<int, string> v_GraphNameDictionary = new Dictionary<int, string>();
        public static Dictionary<int, string> GraphNameDictionary
        {
            get { return v_GraphNameDictionary; }
            set { v_GraphNameDictionary = value; }
        }
        private static Dictionary<string, Registration> v_RegistrationDictionary = new Dictionary<string, Registration>();
        public static Dictionary<string, Registration> RegistrationDictionary
        {
            get { return v_RegistrationDictionary; }
            set { v_RegistrationDictionary = value; }
        }
        private static Dictionary<string, int> v_FractionDictionary = new Dictionary<string, int>();
        public static Dictionary<string, int> FractionDictionary
        {
            get { return v_FractionDictionary; }
            set { v_FractionDictionary = value; }
        }
        private static Dictionary<string, double> v_MaximumDose = new Dictionary<string, double>();
        public static Dictionary<string, double> MaximumDose
        {
            get { return v_MaximumDose; }
            set { v_MaximumDose = value; }
        }

        // Local variables
        private Image patientSPECT;
        private Registration planRegistration;
        private int fractionNumber;
        private string graphName;
        private double maximumDose;
        private double alphaBetaEarly;
        private double alphaBetaLate;
        private double maxEarlyDose;
        private double maxLateDose;

        public void SetPlanSetup(Course selectedDoseCourse, string selectedDosePlanName, Image selectedImage,
            Registration selectedRegistration, string selectedBioCorrection, int fileIndex)
        {
            MessageBox.Show("Here");
            // Obtain information from selected dose PlanSetup.
            PlanSetup patientPlan = selectedDoseCourse.PlanSetups.
                First(s => s.Id == selectedDosePlanName);
            patientPlan.DoseValuePresentation = DoseValuePresentation.Absolute;
            Dose patientDose = patientPlan.Dose;
            fractionNumber = patientPlan.NumberOfFractions.Value;
            StructureSet structureSet = patientPlan.StructureSet;
            IEnumerator<Structure> patientStructureEnum = structureSet.Structures.GetEnumerator();
            List<string> structureStrings = new List<string>(structureSet.Structures.Count());
            while (patientStructureEnum.MoveNext())
            {
                structureStrings.Add(patientStructureEnum.Current.Id);
            }
            if (UserInterface.ListBox1.Items.Count == 0)
            {
                foreach (string structureString in structureStrings)
                {
                    UserInterface.ListBox1.Items.Add(structureString);
                }
            }
            else
            {
                List<string> listBoxStrings = new List<string>(UserInterface.ListBox1.Items.Count);
                foreach (string listBoxString in UserInterface.ListBox1.Items)
                {
                    listBoxStrings.Add(listBoxString);
                }
                UserInterface.ListBox1.Items.Clear();
                IEnumerable<string> intersectStructures = listBoxStrings.Intersect(structureStrings);
                foreach (string intersectStructure in intersectStructures)
                {
                    UserInterface.ListBox1.Items.Add(intersectStructure);
                }
                UserInterface.IntersectWarning.Visible = true;
            }
            

            // Obtain information from selected SPECT image.
            patientSPECT = selectedImage;
            planRegistration = selectedRegistration;

            // Develop naming.
            if (selectedBioCorrection == "No")
            {
                maximumDose = patientDose.DoseMax3D.Dose;

                if (patientSPECT.Series.Id.Substring(0, 7) == "Initial")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(t=0)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "1Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Mo)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "3Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(3Mo)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 5) == "1Year")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Yr)" + "/" + selectedDosePlanName;
                }
            }
            else if (selectedBioCorrection == "Yes")
            {
                alphaBetaEarly = 10.0;
                alphaBetaLate = 2.5;
                maxEarlyDose = patientDose.DoseMax3D.Dose * ((patientDose.DoseMax3D.Dose / fractionNumber + alphaBetaEarly) / (2.0 + alphaBetaEarly));
                maxLateDose = patientDose.DoseMax3D.Dose * ((patientDose.DoseMax3D.Dose / fractionNumber + alphaBetaLate) / (2.0 + alphaBetaLate));
                maximumDose = Math.Max(maxEarlyDose, maxLateDose);

                if (patientSPECT.Series.Id.Substring(0, 7) == "Initial")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(t=0)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "1Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Mo)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "3Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(3Mo)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 5) == "1Year")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Yr)" + "/" + selectedDosePlanName + "(EQD2)";
                }
            }
            if (GraphNameDictionary.ContainsKey(fileIndex) == true)
            {
                GraphNameDictionary[fileIndex] = graphName;
            }
            else
            {
                GraphNameDictionary.Add(fileIndex, graphName);
            }
            if (DosePlans.ContainsKey(fileIndex) == true)
            {
                DosePlans[fileIndex] = patientDose;
            }
            else
            {
                DosePlans.Add(fileIndex, patientDose);
            }
            if (SelectSPECTImage.SPECTDictionary.ContainsKey(fileIndex))
            {
                SelectSPECTImage.SPECTDictionary[fileIndex] = selectedImage;
                SelectSPECTImage.ImageNameDictionary[fileIndex] = selectedImage.Id;
            }
            else
            {
                SelectSPECTImage.SPECTDictionary.Add(fileIndex, selectedImage);
                SelectSPECTImage.ImageNameDictionary.Add(fileIndex, selectedImage.Id);
            }
            if (SelectDosePlan.DoseNameDictionary.ContainsKey(fileIndex))
            {
                SelectDosePlan.DoseNameDictionary[fileIndex] = selectedDoseCourse + "/" + selectedDosePlanName;
            }
            else
            {
                SelectDosePlan.DoseNameDictionary.Add(fileIndex, selectedDoseCourse + "/" + selectedDosePlanName);
            }
            if (SelectBioCorrection.BioCorrectDictionary.ContainsKey(fileIndex))
            {
                SelectBioCorrection.BioCorrectDictionary[fileIndex] = selectedBioCorrection;
            }
            else
            {
                SelectBioCorrection.BioCorrectDictionary.Add(fileIndex, selectedBioCorrection);
            }
            string name = patientSPECT.Series.Id + "/" + selectedDosePlanName + "/" + selectedBioCorrection;
            if (NameDictionary.ContainsKey(fileIndex) == true)
            {
                string oldName = NameDictionary[fileIndex];
                NameDictionary[fileIndex] = name;
                StructureSets.Remove(oldName);
                StructureSets.Add(name, structureSet);
                RegistrationDictionary.Remove(oldName);
                RegistrationDictionary.Add(name, planRegistration);
                FractionDictionary.Remove(oldName);
                FractionDictionary.Add(name, fractionNumber);
                MaximumDose.Remove(oldName);
                MaximumDose.Add(name, maximumDose);
            }
            else
            {
                NameDictionary.Add(fileIndex, name);
                StructureSets.Add(name, structureSet);
                RegistrationDictionary.Add(name, planRegistration);
                FractionDictionary.Add(name, fractionNumber);
                MaximumDose.Add(name, maximumDose);
                UserInterface.LoadedPlans++;
            }
        }

        public void SetPlanSum(Course selectedDoseCourse, string selectedDosePlanName, Image selectedImage,
            Registration selectedRegistration, string selectedBioCorrection, int fileIndex)
        {
            MessageBox.Show("Success");
            // Obtain information from selected dose PlanSum.
            PlanSum patientPlan = selectedDoseCourse.PlanSums.
                First(s => s.Id == selectedDosePlanName);
            patientPlan.DoseValuePresentation = DoseValuePresentation.Absolute;
            Dose patientDose = patientPlan.Dose;
            IEnumerator<PlanSetup> planSetups = patientPlan.PlanSetups.GetEnumerator();
            while (planSetups.MoveNext())
            {
                fractionNumber += planSetups.Current.NumberOfFractions.Value;
            }
            MessageBox.Show(fractionNumber.ToString());
            StructureSet structureSet = patientPlan.StructureSet;
            IEnumerator<Structure> patientStructureEnum = structureSet.Structures.GetEnumerator();
            List<string> structureStrings = new List<string>(structureSet.Structures.Count());
            while (patientStructureEnum.MoveNext())
            {
                structureStrings.Add(patientStructureEnum.Current.Id);
            }
            if (UserInterface.ListBox1.Items.Count == 0)
            {
                foreach (string structureString in structureStrings)
                {
                    UserInterface.ListBox1.Items.Add(structureString);
                }
            }
            else
            {
                List<string> listBoxStrings = new List<string>(UserInterface.ListBox1.Items.Count);
                foreach (string listBoxString in UserInterface.ListBox1.Items)
                {
                    listBoxStrings.Add(listBoxString);
                }
                UserInterface.ListBox1.Items.Clear();
                IEnumerable<string> intersectStructures = listBoxStrings.Intersect(structureStrings);
                foreach (string intersectStructure in intersectStructures)
                {
                    UserInterface.ListBox1.Items.Add(intersectStructure);
                }
                UserInterface.IntersectWarning.Visible = true;
            }

            // Obtain information from selected image.
            patientSPECT = selectedImage;
            planRegistration = selectedRegistration;

            // Develop naming.
            if (selectedBioCorrection == "No")
            {
                maximumDose = patientDose.DoseMax3D.Dose;

                if (patientSPECT.Series.Id.Substring(0, 7) == "Initial")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(t=0)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "1Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Mo)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "3Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(3Mo)" + "/" + selectedDosePlanName;
                }
                else if (patientSPECT.Series.Id.Substring(0, 5) == "1Year")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Yr)" + "/" + selectedDosePlanName;
                }
            }
            else if (selectedBioCorrection == "Yes")
            {
                alphaBetaEarly = 10.0;
                alphaBetaLate = 2.5;
                maxEarlyDose = patientDose.DoseMax3D.Dose * ((patientDose.DoseMax3D.Dose / fractionNumber + alphaBetaEarly) / (2.0 + alphaBetaEarly));
                maxLateDose = patientDose.DoseMax3D.Dose * ((patientDose.DoseMax3D.Dose / fractionNumber + alphaBetaLate) / (2.0 + alphaBetaLate));
                maximumDose = Math.Max(maxEarlyDose, maxLateDose);
                if (patientSPECT.Series.Id.Substring(0, 7) == "Initial")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(t=0)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "1Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Mo)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 6) == "3Month")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(3Mo)" + "/" + selectedDosePlanName + "(EQD2)";
                }
                else if (patientSPECT.Series.Id.Substring(0, 5) == "1Year")
                {
                    graphName = patientSPECT.Id.Substring(patientSPECT.Id.Length - 4) + "(1Yr)" + "/" + selectedDosePlanName + "(EQD2)";
                }
            }
            if (GraphNameDictionary.ContainsKey(fileIndex) == true)
            {
                GraphNameDictionary[fileIndex] = graphName;
            }
            else
            {
                GraphNameDictionary.Add(fileIndex, graphName);
            }
            if (DosePlans.ContainsKey(fileIndex) == true)
            {
                DosePlans[fileIndex] = patientDose;
            }
            else
            {
                DosePlans.Add(fileIndex, patientDose);
            }
            if (SelectSPECTImage.SPECTDictionary.ContainsKey(fileIndex))
            {
                SelectSPECTImage.SPECTDictionary[fileIndex] = selectedImage;
                SelectSPECTImage.ImageNameDictionary[fileIndex] = selectedImage.Id;
            }
            else
            {
                SelectSPECTImage.SPECTDictionary.Add(fileIndex, selectedImage);
                SelectSPECTImage.ImageNameDictionary.Add(fileIndex, selectedImage.Id);
            }
            if (SelectDosePlan.DoseNameDictionary.ContainsKey(fileIndex))
            {
                SelectDosePlan.DoseNameDictionary[fileIndex] = selectedDoseCourse + "/" + selectedDosePlanName;
            }
            else
            {
                SelectDosePlan.DoseNameDictionary.Add(fileIndex, selectedDoseCourse + "/" + selectedDosePlanName);
            }
            if (SelectBioCorrection.BioCorrectDictionary.ContainsKey(fileIndex))
            {
                SelectBioCorrection.BioCorrectDictionary[fileIndex] = selectedBioCorrection;
            }
            else
            {
                SelectBioCorrection.BioCorrectDictionary.Add(fileIndex, selectedBioCorrection);
            }
            string name = patientSPECT.Series.Id + "/" + selectedDosePlanName + "/" + selectedBioCorrection;
            if (NameDictionary.ContainsKey(fileIndex) == true)
            {
                string oldName = NameDictionary[fileIndex];
                NameDictionary[fileIndex] = name;
                StructureSets.Remove(oldName);
                StructureSets.Add(name, structureSet);
                RegistrationDictionary.Remove(oldName);
                RegistrationDictionary.Add(name, planRegistration);
                FractionDictionary.Remove(oldName);
                FractionDictionary.Add(name, fractionNumber);
                MaximumDose.Remove(oldName);
                MaximumDose.Add(name, maximumDose);
            }
            else
            {
                NameDictionary.Add(fileIndex, name);
                StructureSets.Add(name, structureSet);
                RegistrationDictionary.Add(name, planRegistration);
                FractionDictionary.Add(name, fractionNumber);
                MaximumDose.Add(name, maximumDose);
                UserInterface.LoadedPlans++;
            }
        }

    }
}
