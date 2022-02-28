using SpreadsheetUtilities;
using SS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    //Author: Charles Li, Fall 2021
    //University of Utah

     
    /// <summary>
    /// This class represents the logic behind the GUI of the spreadsheet, it uses underlying logic
    /// from spreadsheet, dependency graph and formula
    /// </summary>
    public partial class SpreadSheetForm : Form
    {
        Spreadsheet ss = new Spreadsheet(validVarCellName, s => s.ToUpper(), "ps6");

        private char[] alphabetArr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private string mostRecentSave = "";

        /// <summary>
        /// initializes everything
        /// </summary>
        public SpreadSheetForm()
        {
            InitializeComponent();
            //make text box uneditable
            cellNameTextBox.Enabled = false;
            cellValueTextBox.Enabled = false;

            //first cell when opening up will be A1
            cellNameTextBox.Text = "A1";
            cellValueTextBox.Text = "";

            //change color of view menu items 
            redToolStripMenuItem.ForeColor = Color.Red;
            yellowToolStripMenuItem.ForeColor = Color.Yellow;
            greenToolStripMenuItem.ForeColor = Color.Green;
            blueToolStripMenuItem.ForeColor = Color.Blue;
            blackToolStripMenuItem.ForeColor = Color.Black;

            spreadsheetPanel1.SelectionChanged += OnSelectionChanged;
        }

        /// <summary>
        /// does something everytime a new cell is selected
        /// </summary>
        /// <param name="p"></param>
        public void OnSelectionChanged(SpreadsheetPanel p)
        {
            int col, row;
            string cellName;
            string cellValue;
            spreadsheetPanel1.GetSelection(out col, out row);

            invalidLabel.Text = "";

            //gets the cell name from the column and row, then sets it to cellName textbox
            cellName = alphabetArr[col].ToString() + (row + 1);
            cellNameTextBox.Text = cellName;

            //sets uneditable text box to the value of the cell of cellName
            spreadsheetPanel1.GetValue(col, row, out cellValue);
            cellValueTextBox.Text = cellValue;

            //sets content text box to whatever content is in that cell
            cellContentTextBox.Text = ss.GetCellContents(cellName).ToString();
        }

        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void spreadsheetPanel1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Everytime the Cell Change Button is pressed do work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ///helper that populates a cell on the grid
            ssPopulate();
        }

        /// <summary>
        /// Sees if a string has the basic necessities of a variable for a spreadsheet
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>passes default requirments of a variable</returns>
        private static bool validVarCellName(string variable)
        {
            string varPattern = "^[a-zA-Z][1-9][0-9]?$";

            return Regex.IsMatch(variable, varPattern);
        }

        /// <summary>
        /// Everytime the new button on the menu strip is pressed do something
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///uses PS6 Skeleton code to make new windows of empty spreadsheets
            SpreadSheetApplicationContext.getAppContext().RunForm(new SpreadSheetForm());
        }

        /// <summary>
        /// Everytime the close button on the menu strip is pressed do something
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///if the spreadsheet has changed then prompt a warning to save
            if (ss.Changed)
            {
                askToSaveFile(sender, e);
            }
            Close();
        }

        /// <summary>
        /// Everytime the save button on the menu strip is pressed do something
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///opens dialogue for user
            saveFileDialog1.ShowDialog();

            string fileName = saveFileDialog1.FileName;

            ///if file being saved on isnt the most recent then prompt a warning
            if (mostRecentSave != fileName)
            {
                DialogResult choice = MessageBox.Show("Saving this file will overwrite data of selected file, would you like to save anyways?", "File Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (choice == DialogResult.Yes)
                { }
                else if (choice == DialogResult.No)
                {
                    return;
                }
            }

            mostRecentSave = fileName;

            if (fileName == "")
            {
                return;
            }

            ///save the file, if anything happens its caught and a error message is prompted
            try
            {
                ss.Save(fileName);
            }
            catch (Exception)
            {

                MessageBox.Show("There was a problem saving this file", "Bad File", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
            }
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///if the spreadsheet has changed ask user to save before opening a new file
            if (ss.Changed)
            {
                askToSaveFile(sender, e);
            }

            openFileDialog1.ShowDialog();

            string fileName = openFileDialog1.FileName;

            mostRecentSave = fileName;

            if (fileName == "")
            {
                return;
            }
            /// adds extension is necessary if only .sprd is filtered
            if (openFileDialog1.FilterIndex == 1)
            {
                openFileDialog1.AddExtension = true;
            }


            try
            {
                ///resets grid
                foreach (string cell in ss.GetNamesOfAllNonemptyCells())
                {

                    spreadsheetPanel1.SetValue(ss.getCellCol(cell), ss.getCellRow(cell), "");
                }

                ///all the logic is done in this step
                ss = new Spreadsheet(fileName, validVarCellName, s => s.ToUpper(), "ps6");

                ///populate appropriate cells with appropriate values and contents
                foreach (string cell in ss.GetNamesOfAllNonemptyCells())
                {
                    int col = GetColFromName(cell);
                    int row = GetRowFromName(cell);
                    string cellValue;

                    ss.setCellCol(col, cell);
                    ss.setCellRow(row, cell);

                    spreadsheetPanel1.SetValue(col, row, ss.GetCellValue(cell).ToString());
                }
            }
            ///error message if something is wrong with the file being read
            catch (Exception)
            {

                MessageBox.Show("There was a problem reading this file", "Bad File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }
        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void saveFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
        }

        /// <summary>
        /// Helper that gets the column from a variable
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>column of the variable</returns>
        private int GetColFromName(string cell)
        {

            return cell.ToCharArray(0, 1)[0] - 'A';
        }

        /// <summary>
        /// Helper that gets the row from a variable
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>row of the variable</returns>
        private int GetRowFromName(string cell)
        {
            int row;

            int.TryParse(cell.Substring(1), out row);

            row--;

            return row--;
        }

        /// <summary>
        /// helper that populates a cell with the correct value and content
        /// </summary>
        private void ssPopulate()
        {
            try
            {
                int col, row;

                string text = cellContentTextBox.Text;
                string cellName;
                string cellValue;

                spreadsheetPanel1.GetSelection(out col, out row);

                cellName = alphabetArr[col].ToString() + (row + 1);

                List<string> recalculateCells = (List<string>)ss.SetContentsOfCell(cellName, text);
                ss.setCellCol(col, cellName);
                ss.setCellRow(row, cellName);

                ///For each cell that depends on the newly set cell
                foreach (string cell in recalculateCells)
                {
                    if (ss.GetCellValue(cell) is FormulaError)
                    {
                        FormulaError error = (FormulaError)ss.GetCellValue(cell);
                        spreadsheetPanel1.SetValue(GetColFromName(cell), GetRowFromName(cell), error.Reason);
                    }
                    else
                    {
                        ///use helper methods to place the value and content in the correct location
                        spreadsheetPanel1.SetValue(GetColFromName(cell), GetRowFromName(cell), ss.GetCellValue(cell).ToString());
                    }

                }

                spreadsheetPanel1.GetValue(col, row, out cellValue);
                cellValueTextBox.Text = cellValue;
            }

            catch (CircularException)
            {

                MessageBox.Show("Circular Dependency made, please revert change", "Circular Dependency", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            catch (FormulaFormatException e)
            {
                MessageBox.Show(e.Message, "Invalid Formula", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        /// <summary>
        /// helper method that asks user to save if something has changed to the spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void askToSaveFile(object sender, EventArgs e)
        {
            DialogResult choice = MessageBox.Show("You are trying to close with unsaved changes, save before closing?", "Please save unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (choice == DialogResult.Yes)
            {
                saveToolStripMenuItem_Click(sender, e);
            }
            else if (choice == DialogResult.Cancel)
            {
                return;
            }
        }

        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void Form1_Load(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// Whenever the help button is clicked, a information message will appear explaining how to use the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("In this spreadsheet application, you can select the cell " +
               "you want to edit by clicking on a sepcific grid from A1 - Z99 " +
               "after selecting a grid enter your content in the Cell Content text box " +
               "then press the Change Cell Button to successfully(or unsuccessfuly) put the value " +
               "of your content into the grid, to see a grid's name, value and content just click on an existing " +
               "grid and the information will be displayed in the two boxes in the top right corner and content box, " +
               "to save or open a .sprd file then use the save/open buttons in the file menu strip. " +
               "also included in the file menu strip, to make a new empty spreadsheet using the new button and to close a spreadsheet use the close button. "+
                "There is an addition view menu strip that lets you change the color of non panel UI", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Whenever the Form is closed via X button or something else, do work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            ///if spreadsheet has changed, prompt warning message
            if (ss.Changed)
            {
                DialogResult choice = MessageBox.Show("You are trying to close with unsaved changes, save before closing?", "Please save unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (choice == DialogResult.Yes)
                {
                    saveFileDialog1.ShowDialog();

                    string fileName = saveFileDialog1.FileName;

                    mostRecentSave = fileName;

                    if (fileName == "")
                    {
                        return;
                    }
                    ss.Save(fileName);
                }
                if (choice == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }

            }
        }

        ///I WANTED TO REMOVE THESE EMPTY STUBS BUT IT BREAKS MY CODE SO IGNORE THEM PLEASE!
        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// whenever red button is pressed, calls a method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllToColor(Color.Red);
        }

        /// <summary>
        /// Changes all non spreadsheet panel text to a certain color
        /// </summary>
        /// <param name="color"></param>
        private void changeAllToColor(Color color)
        {
            changeCellButton.ForeColor = color;
            newToolStripMenuItem.ForeColor = color;
            saveToolStripMenuItem.ForeColor = color;
            closeToolStripMenuItem.ForeColor = color;
            openToolStripMenuItem.ForeColor = color;
            fileToolStripMenuItem.ForeColor = color;
            helpToolStripMenuItem.ForeColor = color;
            viewToolStripMenuItem.ForeColor = color;
            CellContentLabel.ForeColor = color;
            cellContentTextBox.ForeColor = color;
            cellNameLabel.ForeColor = color;
            cellNameTextBox.ForeColor = color;
            cellValueLabel.ForeColor = color;
            cellValueTextBox.ForeColor = color;
        }
        /// <summary>
        /// whenever blue button is pressed, calls a method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllToColor(Color.Blue);
        }
        /// <summary>
        /// whenever green button is pressed, calls a method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllToColor(Color.Green);
        }
        /// <summary>
        /// whenever yellow button is pressed, calls a method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void yellowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllToColor(Color.Yellow);
        }
        /// <summary>
        /// whenever black button is pressed, calls a method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllToColor(Color.Black);
        }
    }

}
