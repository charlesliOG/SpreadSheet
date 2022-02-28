
   
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS
{
    //
    // Author: Charles Li, Fall 2021
    // University of Utah

    /// <summary>
    /// This class inherits abstract methods from AbstractSpreadsheet and implements them using a Dictionary mapping strings to Cells
    /// and a DependencyGraph to keep track of relations between cells
    /// 
    /// ...
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        //dictionary that maps cell names to cell contents/values
        private Dictionary<string, Cell> cellDic;
        private DependencyGraph depedencyGraph;
        private bool changed;




        /// <summary>
        /// zero argument constructor that calls 3 argument constructor with "default" delegates and version
        /// </summary>
        public Spreadsheet() : this(s => true, s => s, "default")
        {

        }

        /// <summary>
        /// 3 argument constructor that makes an empty "version" spreadsheet and uses passed in isValid/normalizer delegates
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {

            cellDic = new Dictionary<string, Cell>();
            depedencyGraph = new DependencyGraph();
            changed = false;
        }


        ///four argument constructor reading a file then creating a spreadsheet based on the xml file read
        ///uses passed in isValid/normalizer delegates and must match passed in version
        /// 
        public Spreadsheet(string pathToFile, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {

            cellDic = new Dictionary<string, Cell>();
            depedencyGraph = new DependencyGraph();

            //helper method to read spreadsheet and write to cell dictionary
            readSpreadSheet(pathToFile, isValid, normalize);
            changed = false;


        }

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        { get => changed; protected set => changed = false; }


        /// <summary>
        /// This class represents the contents of a Cell
        /// </summary>
        private class Cell
        {
            Object content;
            object value;
            int col, row;
            public Cell(Object c)
            {

                content = c;
            }
            public object Content
            {
                get { return content; }

            }

            //getters and setters for the values of a cell
            public void setValue(object value)
            {
                this.value = value;
            }

            public void setCol(int value)
            {
                this.col = value;
            }

            public void setRow(int value)
            {
                this.row = value;
            }

            public object getValue()
            {
                return value;
            }
            public int getCol()
            {
                return col;
            }
            public int getRow()
            {
                return row;
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <returns>the content of a given cell, a double, string, or a Formula</returns>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            // An empty cell(name is "") has empty contents(content is "")
            if (name == "")
            {
                return name;
            }

            //if(name is null or invalid throw exception)
            if (name is null || !validVarCellName(name) || !IsValid(name))
            {
                throw new InvalidNameException();
            }

            if (!cellDic.ContainsKey(name))
            {
                return "";
            }
            return cellDic[name].Content;

        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        /// <returns>IEnumerable of all non empty cells</returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {

            return cellDic.Keys;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">name of cell to set</param>
        ///   <param name="number">content of cell "name"</param>
        /// <returns>creates a cell using a mapping class and dependency graph</returns>
        protected override IList<string> SetCellContents(string name, double number)
        {

            List<String> returnList = new List<string>();
            /// empty cell being set should not have a relationship with other cells






            Cell doubleNum = new Cell(number);
            doubleNum.setValue(number);


            ///if cell name already exists in mapping, then replace it with new content
            if (cellDic.ContainsKey(name))
            {
                cellDic[name] = doubleNum;
            }
            else
            {
                cellDic.Add(name, doubleNum);
            }

            depedencyGraph.ReplaceDependees(name, new HashSet<String>());


            ///to populate List of name plus the names of all other cells whose value depends, 
            /// directly or indirectly, on the named cell
            try
            {

                return new List<string>(GetCellsToRecalculate(name));

            }
            catch (CircularException e)
            {

                throw e;
            }


        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">name of cell to set</param>
        ///   <param name="text">content of cell "name"</param>
        /// <returns>creates a cell using a mapping class and dependency graph</returns>
        protected override IList<string> SetCellContents(string name, string text)
        {
            List<String> returnList = new List<string>();
            /// empty cell being set should not have a relationship with other cells


            Cell cellText = new Cell(text);





            ///if cell name already exists in mapping, then replace it with new content
            if (cellDic.ContainsKey(name))
            {
                cellDic[name] = cellText;
            }
            else
            {
                cellDic.Add(name, cellText);
            }


            if (cellDic[name].Content.Equals(""))
            {
                cellDic.Remove(name);
            }


            ///to populate List of name plus the names of all other cells whose value depends, 
            /// directly or indirectly, on the named cell

            depedencyGraph.ReplaceDependees(name, new HashSet<string>());

            try
            {
                return new List<string>(GetCellsToRecalculate(name));

            }
            catch (CircularException e)
            {

                throw e;
            }





        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        ///   <param name="name">name of cell to set</param>
        ///   <param name="formula">content of cell "name"</param>
        /// <returns>creates a cell using a mapping class and dependency graph</returns>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            List<String> returnList = new List<string>();
            /// empty cell being set should not have a relationship with other cells



            IEnumerable<String> tempDep = depedencyGraph.GetDependees(name);


            depedencyGraph.ReplaceDependees(name, formula.GetVariables());

            try
            {
                GetCellsToRecalculate(name);
            }
            catch (CircularException e)
            {

                throw e;
            }




            Cell cellFormula = new Cell(formula);
            ///if cell name already exists in mapping, then replace it with new content
            if (cellDic.ContainsKey(name))
            {
                cellDic[name] = cellFormula;
            }
            else
            {
                cellDic.Add(name, cellFormula);
            }

            return new List<string>(GetCellsToRecalculate(name));




        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        ///  /// <param name="name">cell name to get direct dependents of</param>
        /// <returns>IEnumberable<String> of cells that are directly dependent of cell "name"</returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return depedencyGraph.GetDependents(name);

        }


        /// <summary>
        /// Sees if a string has the basic necessities of a variable for a spreadsheet
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>passes default requirments of a variable</returns>
        private static bool validVarCellName(string variable)
        {
            string varPattern = "^[a-zA-Z]+[0-9]+$";

            return Regex.IsMatch(variable, varPattern);

        }

        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(string filename)
        {
            string finalString = "";
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "spreadsheet":

                                return reader.GetAttribute("version");
                        }
                    }
                }
            }
            catch (Exception)
            {


                throw new SpreadsheetReadWriteException("something went wrong when reading file");

            }

            return finalString;
        }

        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>cell name goes here</name>
        /// <contents>cell contents goes here</contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    string formulaString;

                    writer.WriteStartDocument();

                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);

                    //for each cell, write xml documentation for name and content when reading
                    foreach (string cell in GetNamesOfAllNonemptyCells())
                    {
                        writer.WriteStartElement("cell");


                        writer.WriteElementString("name", cell);


                        //formula has to add = in front
                        if (cellDic[cell].Content is Formula)
                        {
                            formulaString = cellDic[cell].Content.ToString().Insert(0, "=");
                            writer.WriteElementString("contents", formulaString);
                        }
                        //otherwise just write
                        else
                        {
                            writer.WriteElementString("contents", cellDic[cell].Content.ToString());
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }
            }
            catch (Exception)
            {

                throw new SpreadsheetReadWriteException("saving to a non-existent path");
            }

            changed = false;
        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {


            name = Normalize(name);
            try
            {
                return cellDic[name].getValue();
            }
            catch (KeyNotFoundException)
            {

                return "";
            }





        }

        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            string holdFormula;
            Formula f;
            if (name is null || !validVarCellName(name) || !IsValid(name))
            {
                throw new InvalidNameException();
            }

            if (content == "")
            {
                SetCellContents(name, "");
                return new List<string>();
            }



            if (content is null)
            {
                throw new ArgumentNullException();
            }

            name = Normalize(name);


            //double
            if (double.TryParse(content, out double result))
            {
                //SetCellContents returns the list needed to loop through
                foreach (string cell in SetCellContents(name, result))
                {
                    //for each new cell, evaluate using dependencies and lookupdelegate
                    f = new Formula(cellDic[cell].Content.ToString(), Normalize, IsValid);
                    cellDic[cell].setValue(f.Evaluate(cellLookup));

                }


            }
            //formula

            else if (content.StartsWith("="))
            {
                holdFormula = content.Remove(0, 1);
                f = new Formula(holdFormula, Normalize, IsValid);
                foreach (string cell in SetCellContents(name, f))
                {
                    if (cellDic[cell].Content.ToString().StartsWith("="))
                    {
                        holdFormula = cellDic[cell].Content.ToString().Remove(0, 1);
                        f = new Formula(holdFormula, Normalize, IsValid);
                    }
                    else
                    {
                        f = new Formula(cellDic[cell].Content.ToString(), Normalize, IsValid);
                        cellDic[cell].setValue(f.Evaluate(cellLookup));

                    }



                }
            }

            //string
            else
            {

                foreach (string cell in SetCellContents(name, content))
                {
                    cellDic[cell].setValue(content);

                }



            }

            changed = true;
            return new List<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// sets row of a cell
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void setCellRow(int row, string name)
        {

            cellDic[name].setRow(row);

        }

        public int getCellRow(string name)
        {
            return cellDic[name].getRow();
        }
        /// <summary>
        /// sets col of a cell
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void setCellCol(int col, string name)
        {

            cellDic[name].setCol(col);

        }

        public int getCellCol(string name)
        {
            return cellDic[name].getCol();
        }






        /// <summary>
        /// helper method that reads an xml file then creates a spreadsheet from it
        /// </summary>
        /// <param name="pathToFile">file name</param>
        /// <param name="isValid">valid delegate</param>
        /// <param name="normalize">normalizer delegate</param>
        private void readSpreadSheet(string pathToFile, Func<string, bool> isValid, Func<string, string> normalize)
        {

            try
            {
                using (XmlReader reader = XmlReader.Create(pathToFile))
                {
                    Formula f;
                    string tempCellName = "";
                    string tempContent = "";
                    bool hasVersion = false;

                    while (reader.Read())
                    {
                        //only reads starting elements
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":



                                    if (!reader.GetAttribute("version").Equals(Version))
                                    {
                                        throw new SpreadsheetReadWriteException("wrong version of spreadsheet");
                                    }
                                    //if there is no version after reading all code then invalid
                                    hasVersion = true;
                                    break;


                                case "name":
                                    reader.Read();
                                    tempCellName = reader.Value;
                                    if (!validVarCellName(tempCellName) || !isValid(tempCellName))
                                    {
                                        throw new SpreadsheetReadWriteException("not a valid name for a cell");
                                    }
                                    //stores in a temp string for setting later when finding content
                                    tempCellName = normalize(tempCellName);
                                    break;

                                case "contents":

                                    //uses temp string when storing value
                                    reader.Read();
                                    tempContent = reader.Value;
                                    SetContentsOfCell(tempCellName, tempContent);


                                    break;

                            }
                        }

                    }
                    if (hasVersion == false)
                    {
                        throw new SpreadsheetReadWriteException("does not include version");
                    }
                }

            }
            catch (CircularException)
            {

                throw new SpreadsheetReadWriteException("circular dependency");
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("problem opening, reading or closing the file");
            }

        }

        /// <summary>
        /// lookup delegate must return a double, or throw an ArugmentException
        /// </summary>
        /// <param name="s">cell name</param>
        /// <returns>a cells double value</returns>
        private double cellLookup(string s)
        {
            try
            {

                if (cellDic[s].getValue() is FormulaError)
                {
                    throw new ArgumentException();
                }
                //double


                return (double)cellDic[s].getValue();
            }
            catch (KeyNotFoundException e)
            {
                try
                {
                    if (cellDic[s.ToUpper()].getValue() is FormulaError)
                    {
                        throw new ArgumentException();
                    }
                    return (double)cellDic[s.ToUpper()].getValue();

                }
                catch (Exception)
                {

                    throw new ArgumentException();
                }
                   

                throw new ArgumentException();
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }
        }


    }
}
