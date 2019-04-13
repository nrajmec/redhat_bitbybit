using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Proto_BotForSmartCoder
{
    public partial class MainForm : Form
    {
        private List<String> m_AllCodeLines = new List<String>();
        private Dictionary<String, String> m_DictAllVars = new Dictionary<String, String>();
        private String currentVar = "";
        private String currentVarType = "";
        private List<String> suggestedVars = new List<String>();

        //private String[] oldLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)

        public MainForm()
        {
            InitializeComponent();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void txtBox_DevSpace_TextChanged(object sender, EventArgs e)
        {
            ReadAllCode();
        }

        private void ReadAllCode()
        {
            m_AllCodeLines = new List<String>();

            String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String currentLineWithoutSpace = allLines[allLines.Count() - 1].Replace(" ", "");
            String currentLine = allLines[allLines.Count() - 1];

            if (currentLineWithoutSpace.EndsWith(")"))
            {
                lstBox_UseRecommendations.Items.Clear();
                return;
            }

            currentVar = "";

            currentVarType =  "";


            if (currentLineWithoutSpace.ToLower().StartsWith("if(") | currentLineWithoutSpace.ToLower().StartsWith("for(") | currentLineWithoutSpace.ToLower().StartsWith("foreach(") |
                    currentLineWithoutSpace.ToLower().StartsWith("do") | currentLineWithoutSpace.ToLower().StartsWith("while("))
            {
                for (int i = 0; i < allLines.Count() - 1; i++)
                {
                    if (allLines[i].Trim() == "" | allLines[i].Trim() == " ")
                    {
                        continue;
                    }

                    if (allLines[i].ToLower().StartsWith("int") | allLines[i].ToLower().StartsWith("string") | allLines[i].ToLower().StartsWith("int[]") | allLines[i].ToLower().StartsWith("string[]") |
                        allLines[i].ToLower().StartsWith("list") | allLines[i].ToLower().StartsWith("dictionary") | allLines[i].ToLower().StartsWith("boolean"))
                    {
                        String[] splitFirst = allLines[i].Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                        if (splitFirst.Length > 1)
                        {
                            for (int j = 0; j < splitFirst.Length; j++)
                            {
                                SplitAddVars(splitFirst[i]);
                            }
                        }
                        else
                        {
                            SplitAddVars(allLines[i]);
                        }
                    }
                }

                int status = FindCurrVar(currentLine);

                //if (allLines[i].ToLower().StartsWith("var"))
                //{
                //    String[] splitLine = allLines[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                //    if (splitLine.Length == 2)
                //    {
                //        var assign = splitLine[1].Trim();

                //        if (assign.GetType().Equals(typeof(int)))
                //        {

                //        }
                //    }
                //}
            }
            else
            {
                lstBox_UseRecommendations.Items.Clear();
            }
        }

        private void SplitAddVars(String strLine)
        {
            String varType = "";

            List<String> splitLine = strLine.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            varType = splitLine[0].ToLower();

            if (varType.Contains("[]"))
            {
                varType = "array";
            }
            if (varType.ToLower().Contains("list"))
            {
                varType = "list";
            }

            splitLine.RemoveAt(0);

            strLine = String.Join(" ", splitLine.ToArray());


            //String[] splitVars = strLine.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);


            String[] splitVarValue = new String[1];
            //splitVarValue[0] = varName;
            if (strLine.Contains("="))
            {
                splitVarValue = strLine.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                if (varType.Trim() == "var")
                {
                    var findType = splitVarValue[1];
                    String varDatatype = findType.GetType().ToString();

                    if (varDatatype.ToLower() == "system.string")
                    {
                        varType = "string";
                    }
                    else if (varDatatype.ToLower() == "system.int32")
                    {
                        varType = "int";
                    }
                    else if (varDatatype.ToLower().Contains("[]"))
                    {
                        varType = "array";
                    }
                    else if (varDatatype.ToLower().Contains("generic.list"))
                    {
                        varType = "list";
                    }
                    else
                    {
                        varType = null;
                    }
                }
            }
            else
            {
                splitVarValue[0] = strLine.Replace(";", "");
            }

            if (!m_DictAllVars.ContainsKey(splitVarValue[0].Trim()) && varType.Trim() != null)
            {
                m_DictAllVars.Add(splitVarValue[0].Trim(), varType.Trim());
            }


        }

        private int FindCurrVar(String currLine)
        {
            List<String> splitLine = currLine.Split(new string[] { "(" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            String condType = "";

            condType = splitLine[0].Trim();
            splitLine.RemoveAt(0);

            String newLine = "";
            newLine = String.Join(" ", splitLine.ToArray());

            splitLine = newLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            
            if (condType.Trim().ToLower() == "for" | condType.Trim().ToLower() == "foreach" | condType.Trim().ToLower() == "do")
            {
                ReadFromPred(condType);
                return 0;
            }

            if (splitLine.Count() == 0)
            {
                suggestedVars.Clear();

                foreach (KeyValuePair<String, String> valPair in m_DictAllVars)
                {
                    suggestedVars.Add(valPair.Key);
                }

                lstBox_UseRecommendations.Items.Clear();
                if (suggestedVars != null && suggestedVars.Count() > 0)
                {
                    suggestedVars.ForEach(x => lstBox_UseRecommendations.Items.Add(x));
                    lstBox_UseRecommendations.SelectedIndex = 0;
                }

                return 0;
            }

            if (splitLine.Count() == 2)
            {
                suggestedVars.Clear();
                if (splitLine[0].Contains("."))
                {
                    currentVar = splitLine[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList()[0].Trim();
                    if (m_DictAllVars.ContainsKey(currentVar))
                    {
                    currentVarType = m_DictAllVars[currentVar];
                    }
                }
                else
                {
                    currentVar = splitLine[0];

                    if (m_DictAllVars.ContainsKey(currentVar))
                    {
                        currentVarType = m_DictAllVars[currentVar];
                    }
                    
                }

                foreach (KeyValuePair<String, String> valPair in m_DictAllVars)
                {
                    if (valPair.Key != currentVar)
                    {
                        suggestedVars.Add(valPair.Key);
                    }
                }

                lstBox_UseRecommendations.Items.Clear();
                if (suggestedVars != null && suggestedVars.Count() > 0)
                {
                    suggestedVars.ForEach(x => lstBox_UseRecommendations.Items.Add(x));
                    lstBox_UseRecommendations.SelectedIndex = 0;
                }

                return 0;
            }
            else if (splitLine.Count() == 1)
            {
                currentVar = splitLine[0];
                if (m_DictAllVars.ContainsKey(currentVar))
                {
                    currentVarType = m_DictAllVars[currentVar];
                }

                ReadFromPred(condType);
                return 0;
            }

            return -1;
        }

        private void ReadFromPred(String condType)
        {
            String predCSV_File = Path.Combine(@"V:\Naveen\Learning\RedHat", "testPred.csv");

            List<String> predLines = File.ReadAllLines(predCSV_File).ToList();

            String suggestionFile = Path.Combine(@"V:\Naveen\Learning\RedHat", "suggestions.txt");

            List<String> suggestionLines = File.ReadAllLines(suggestionFile).ToList();

            List<String> allSuggestions = null;

            if (predLines != null && predLines.Count() > 0)
            {
                for (int i = 1; i < predLines.Count(); i++)
                {
                    String[] splitPredLine = predLines[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    if (splitPredLine.Length == 4)
                    {
                        if (splitPredLine[1] == condType.Trim() && splitPredLine[2] == currentVarType.Trim())
                        {
                            allSuggestions = suggestionLines[Convert.ToInt32(splitPredLine[3])].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            break;
                        }

                        if (condType.Trim() == "for" | condType.Trim() == "foreach" | condType.Trim() == "do")
                        {
                            if (splitPredLine[1] == condType.Trim())
                            {
                                allSuggestions = suggestionLines[Convert.ToInt32(splitPredLine[3])].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                break;
                            }
                        }
                    }
                }

                lstBox_UseRecommendations.Items.Clear();
                if (allSuggestions != null && allSuggestions.Count() > 0)
                {
                    allSuggestions.ForEach(x => lstBox_UseRecommendations.Items.Add(x.Trim()));
                    lstBox_UseRecommendations.SelectedIndex = 0;
                }
            }
        }

        private void btn_UseRecommendation_Click(object sender, EventArgs e)
        {
            if (lstBox_UseRecommendations.SelectedIndex > -1)
            {
                String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                allLines[allLines.Count() - 1] = allLines[allLines.Count() - 1] + " " + lstBox_UseRecommendations.SelectedItem;

                txtBox_DevSpace.Text = String.Join("\r\n", allLines.ToArray());
            }
        }

        private void lstBox_UseRecommendations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstBox_UseRecommendations.SelectedIndex > -1)
            {
                String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                allLines[allLines.Count() - 1] = allLines[allLines.Count() - 1] + " " + lstBox_UseRecommendations.SelectedItem.ToString();

                txtBox_DevSpace.Text = String.Join("\r\n", allLines.ToArray());
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ProcessStartInfo proc = new ProcessStartInfo("python", @"V:\Naveen\Learning\RedHat\trainmodel.py");

            label4.Text = "Please wait...";

            proc.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(proc).WaitForExit();
            label4.Text = "";
        }

        private void btn_SaveAs_Click(object sender, EventArgs e)
        {
            String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            File.WriteAllLines(Path.Combine(@"V:\Naveen\Learning\RedHat", "generatedCode.cs"), allLines);
        }
    }
}
