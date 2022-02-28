Author: Charles Li, Fall 2021
University of Utah
-------------------------------------------------------------------------------

1. Design Decisions
	10/22/21
	a. When implementing the GUI for Spreadsheet I wanted to keep track of the column and row numbers 
	for each cell, there were many ways of solving this but I chose to change my PS5 cell class so that 
	instead of just having a name, value and content I could also give a col and row to a cell. I made getters and setters for both column and row
	when recaucluating I can now correctly place new values in their appropriate locations on the spreadsheet.
	10/22/21
	b.In my form class, I used helper methods validVarCellName(to see if cell is not A1-Z99) and ssPopulate, which popluates a cell 
	aswell as recalculating dependency related cells. I also had helper methods in Form that converted cell names to their
	appropriate locations.

2. External Code Resources
	10/21/21
	a. I used code from the PS6Skeleton code in the example repository, specifically the grid used for the spreadsheet
	was taken from SpreadSheetPanel and DemoApplicationContext/Program classes were taken directly from the PS6 Skeleton
	10/21/21
	b. Code examples from Microsoft Docs were used to help implement much of the Form code

3. Implementation Notes
	10/21/21
	a. The underlying logic of the spreadsheet is already finished, so I concerned myself with designer problems
	10/21/21
	b.Demo from PS6 Skeleton gives good examples on how buttons send events and how handlers will listen to them, also showed 
	very useful methods like getSelection
	10/21/21
	c.Many handlers will just auto-implement themselves by just double clicking their UI components in Design
	10/21/21
	d.Most information about how to implement specific GUI tools/methods will be easily found online
	10/21/21
	e. UI should be clear and easy to understand

 4. Problems
    10/22/21
	a. Finding the locations for each cell in the grid after recalculating values
	 10/22/21
	b.Case insensitivity i.e a2 is treated like A2 and vise versa
	 10/22/21
	c.Finding out which file was most recently saved
	 10/22/21
	d.Debugging for large problems is very hard

 5.GUI additional features
     10/22/21
	a.There is a very simple addition to the Spreadsheet that lets the user change the color of non panel UI