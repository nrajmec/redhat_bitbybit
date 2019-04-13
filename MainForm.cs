///Date: 12-04-2019
///Theme 2: Smart Coder Bot
///Overview: This app gives intelligent suggestion for C# code entered by the user
///          This invokes Machine Learning Algorithm to predict the suggestions
///Customers:End-user would be who uses C# IDE

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
        #region Private Variables
        private List<String> m_AllCodeLines = new List<String>();
        private Dictionary<String, String> m_DictAllVars = new Dictionary<String, String>();
        private String m_CurrentVar = "";
        private String m_CurrentVarType = "";
        private List<String> m_SuggestedVars = new List<String>();
        #endregion

        #region Control Events
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// MInimizes the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Triggers the function when there is change in text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBox_DevSpace_TextChanged(object sender, EventArgs e)
        {
            ReadAllCode();
        }

        /// <summary>
        /// Append the selected recommendation to the code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_UseRecommendation_Click(object sender, EventArgs e)
        {
            if (lstBox_UseRecommendations.SelectedIndex > -1)
            {
                String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                allLines[allLines.Count() - 1] = allLines[allLines.Count() - 1] + " " + lstBox_UseRecommendations.SelectedItem;

                txtBox_DevSpace.Text = String.Join("\r\n", allLines.ToArray());
            }
        }

        /// <summary>
        /// Append the selected recommendation to the code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstBox_UseRecommendations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstBox_UseRecommendations.SelectedIndex > -1)
            {
                String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                allLines[allLines.Count() - 1] = allLines[allLines.Count() - 1] + " " + lstBox_UseRecommendations.SelectedItem.ToString();

                txtBox_DevSpace.Text = String.Join("\r\n", allLines.ToArray());
            }
        }

        /// <summary>
        /// Load the form with all the pre-requisites
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            ProcessStartInfo proc = new ProcessStartInfo("python", @"V:\Naveen\Learning\RedHat\trainmodel.py");

            label4.Text = "Please wait...";

            proc.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(proc).WaitForExit();
            label4.Text = "";
        }

        /// <summary>
        /// Saves the textbox into a .cs file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveAs_Click(object sender, EventArgs e)
        {
            String[] allLines = txtBox_DevSpace.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            File.WriteAllLines(Path.Combine(@"V:\Naveen\Learning\RedHat", "generatedCode.cs"), allLines);
        }
        #endregion        

        #region Methods
        /// <summary>
        /// Reads the code entered by user and provide suggestions
        /// </summary>
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

            m_CurrentVar = "";
            m_CurrentVarType = "";

            // If the code line startswith the following conditions, then split all the variables writter above the current line            
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

                // Find the current line and provide suggestion using ML predictions
                FindCurrVar(currentLine);
            }
            else
            {
                lstBox_UseRecommendations.Items.Clear();
            }
        }

        /// <summary>
        /// Split all the variables and add to a dictionary
        /// </summary>
        /// <param name="strLine"></param>
        private void SplitAddVars(String strLine)
        {
            try
            {
                String varType = "";

                List<String> splitLine = strLine.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                varType = splitLine[0].ToLower();

                // Check the datatype
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

                String[] splitVarValue = new String[1];

                // If the variable contains "=" then split the variable and value separately
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
            catch (Exception ex)
            {
                throw new Exception("Failed to find current variable \n" + ex.Message);
            }
        }

        /// <summary>
        /// Get All the variables declared in the code as recommendations
        /// </summary>
        private void GetAllSuggestedVars()
        {
            try
            {
                m_SuggestedVars.Clear();

                foreach (KeyValuePair<String, String> valPair in m_DictAllVars)
                {
                    m_SuggestedVars.Add(valPair.Key);
                }

                lstBox_UseRecommendations.Items.Clear();
                if (m_SuggestedVars != null && m_SuggestedVars.Count() > 0)
                {
                    m_SuggestedVars.ForEach(x => lstBox_UseRecommendations.Items.Add(x));
                    lstBox_UseRecommendations.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to find current variable \n" + ex.Message);
            }
        }

        /// <summary>
        /// Get filtered suggested variables
        /// Remove variables which is used in the same condition or loop
        /// </summary>
        /// <param name="str_Line"></param>
        private void GetFilteredSuggestions(String str_Line)
        {
            try
            {
                m_SuggestedVars.Clear();
                if (str_Line.Contains("."))
                {
                    m_CurrentVar = str_Line.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList()[0].Trim();
                    if (m_DictAllVars.ContainsKey(m_CurrentVar))
                    {
                        m_CurrentVarType = m_DictAllVars[m_CurrentVar];
                    }
                }
                else
                {
                    m_CurrentVar = str_Line;

                    if (m_DictAllVars.ContainsKey(m_CurrentVar))
                    {
                        m_CurrentVarType = m_DictAllVars[m_CurrentVar];
                    }
                }

                foreach (KeyValuePair<String, String> valPair in m_DictAllVars)
                {
                    if (valPair.Key != m_CurrentVar)
                    {
                        m_SuggestedVars.Add(valPair.Key);
                    }
                }

                lstBox_UseRecommendations.Items.Clear();
                if (m_SuggestedVars != null && m_SuggestedVars.Count() > 0)
                {
                    m_SuggestedVars.ForEach(x => lstBox_UseRecommendations.Items.Add(x));
                    lstBox_UseRecommendations.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to find current variable \n" + ex.Message);
            }
        }

        /// <summary>
        /// Find the current variable and suggestions which are predicted using ML algo
        /// </summary>
        /// <param name="currLine"></param>
        /// <returns></returns>
        private void FindCurrVar(String currLine)
        {
            try
            {
                List<String> splitLine = currLine.Split(new string[] { "(" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                String condType = "";

                condType = splitLine[0].Trim();
                splitLine.RemoveAt(0);

                String newLine = "";
                newLine = String.Join(" ", splitLine.ToArray());

                splitLine = newLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // if current line startswith these conditions then find predcitions/suggestions from the ML pred file
                if (condType.Trim().ToLower() == "for" | condType.Trim().ToLower() == "foreach" | condType.Trim().ToLower() == "do")
                {
                    ReadFromPred(condType);
                    return;
                }

                // If the line has started with a bracket, then provide suggestions for variables
                if (splitLine.Count() == 0)
                {
                    GetAllSuggestedVars();
                    return;
                }

                // If the line has started with a bracket and a variable, then provide suggestions for conditions/other variables to be used
                if (splitLine.Count() == 2)
                {
                    GetFilteredSuggestions(splitLine[0]);

                    return;
                }
                else if (splitLine.Count() == 1)
                {
                    m_CurrentVar = splitLine[0];
                    if (m_DictAllVars.ContainsKey(m_CurrentVar))
                    {
                        m_CurrentVarType = m_DictAllVars[m_CurrentVar];
                    }

                    ReadFromPred(condType);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to find current variable \n" + ex.Message);
            }

        }

        /// <summary>
        /// Runs the Python script and predicts the indices from which suggestions can be filtered
        /// </summary>
        /// <param name="condType"></param>
        private void ReadFromPred(String condType)
        {
            try
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
                            if (splitPredLine[1] == condType.Trim() && splitPredLine[2] == m_CurrentVarType.Trim())
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
            catch (Exception ex)
            {
                throw new Exception("Failed to Read from Predictions" + "\n" + ex.Message);
            }
            
        }
        
        #endregion
        
    }
}
