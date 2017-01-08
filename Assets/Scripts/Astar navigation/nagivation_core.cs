using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// A star navigation calculator
public class nagivation_core : MonoBehaviour {

    List<Cell> allCells = new List<Cell>();             // list of all cells
    List<Cell> OpenList = new List<Cell>();             // open list - list of actual cells which are used for calculating the shortest path
    List<Cell> ClosedList = new List<Cell>();           // closed list - list of those cells which were already tested but aren't part of final path
    List<Cell> outputPath;                              // final path

    Cell StartCell, GoalCell, actualCell, OldCell;      // cells for work
    bool pathFound = false;                             // path was found but it wasn't reversed and finalized for
    bool pathCreated = false;                           // final state - path was found and reversed
    bool pathCannotBeFound = false;                     // path cannot be found
    int goalCell_X, goalCell_Y;                         // XY,  coordinates of GoalCell
    int gridSize;                                       

    public CellManager cellManager;                     // reference to cell manager


    public void setAllCellsAndGrid(ref List<Cell> input,int gSize)
    {
        allCells = input;
        gridSize = gSize;

        calculateH();
    }

    // Main function for calculating A*
    public List<Cell> calculatePath(Cell start, Cell goal)
    {
        // Reset A* values for new computation
        ResetASTAR(start, goal);


        // The main cycle of A* calculation
        while (!pathFound || !pathCreated)
        {
            if (pathCannotBeFound)
            {
                print("Astar-debug : path cannot be found");
                break;

            }
            if (!pathFound)
            {
                findPath();
            }
            else
            {
                print("Astar-debug : path found!");
                createPath();
            }
        }
        return outputPath;
    }

    
    void determineValues(Cell testedCell)
    {
        if (testedCell == null || testedCell.Collision)
            return;

        // Goal is found!
        if (testedCell == GoalCell)
        {
            GoalCell.parent = actualCell;
            pathFound = true;
            return;
        }

        int actualMovementCost;
        if (!testedCell.isInClosedList) // Cell in closed list was already processed
        {
            if (OpenList.Contains(testedCell)) // it is already in OpenList?
            { 

                if (actualCell.basicNeighbours.Contains(testedCell))
                    actualMovementCost = 10;        // basic cost 10
                else actualMovementCost = 14;       // diagonal cost 14

				float newG = actualCell.G + actualMovementCost; 

                // If the new G is better(lesser), than testedCell G = new G.. Also change parent of the testedCellCell and recalculate its F value
				if(newG < testedCell.G){
                    testedCell.parent = actualCell;
					testedCell.G = newG;
					testedCell.F = testedCell.G + testedCell.H + testedCell.C;
				}
            }
            else  // it has not been assinged to any list yet
            {	
                // Calculate new values for tested cell
                
                testedCell.parent = actualCell;  // calculating started from actualCell, so set testedCells parent to that actualCell

                if (actualCell.basicNeighbours.Contains(testedCell))
                    actualMovementCost = 10;        // basic cost 10
                else actualMovementCost = 14;       // diagonal cost 14

                testedCell.G = actualCell.G + actualMovementCost;
                testedCell.F = testedCell.G + testedCell.H;

                //Adding this new one cell to OpenList
                OpenList.Add(testedCell);

            }
        }

    }


    // Finds cell from OpenList with lowest F value 
    Cell findCellAccordingToF()
    {
        Cell output = null;
        // Iterate all Cells from open list and find one with lowest F value
        float lowestF = 9999;
        foreach (Cell cell in OpenList)
        {
            if (cell.F < lowestF)
            {
                output = cell;
                lowestF = cell.F;
            }
        }
        return output;
    }

    void findPath()
    {
        if (!pathFound)
        {
            // Calculate values for all connected Cells
            foreach (Cell wp in actualCell.diagonalNeighbours)
                determineValues(wp);
            foreach (Cell wp in actualCell.basicNeighbours)
                determineValues(wp);

            if (!pathFound) // Goal wasn't found, Remove actual Cell from open list and add it to closed list
            {
                ClosedList.Add(actualCell);
                actualCell.isInClosedList = true;
                OpenList.Remove(actualCell);
                OldCell = actualCell;
                actualCell = findCellAccordingToF();
                if (actualCell == null) pathCannotBeFound = true;	// Blocked path to goal, set pathCannotBeFound to true
            }
        }
    }


    // Reversing list of cells
    List<Cell> reverseList(List<Cell> input)
    {
        List<Cell> output = new List<Cell>();
        for (int i = input.Count - 1; i > -1; i--)
        {
            output.Add(input[i]);
            input[i].C += 5000;
        }
        return output;
    }


    void createPath()
    {
        // Creating path. Moving from last processed cell (goal) to the start cell by parent pointers 
        Cell wp = actualCell;
        while (wp != StartCell)
        {
            outputPath.Add(wp);
            wp = wp.parent;
        }

        if (outputPath.Count == 0) 
            outputPath.Add(GoalCell); // agent is standing on goal

        // Now reverse output List
        outputPath = reverseList(outputPath);
        pathCreated = true;
    }

    void resetVariables()
    {
        // Reset H,F,G
        foreach (Cell wp in allCells)
        {
         //   wp.H = 0;
            wp.F = 0;
            wp.G = 0;
            wp.parent = null;
            wp.isInClosedList = false;
        };
        // Reset booleans
        pathFound = false;
        pathCreated = false;
        pathCannotBeFound = false;
    }
    

    void calculateH()
    {
   		// Calculating the H value for each cell
		// Manhattan
		foreach(Cell c in allCells) {
			// this cell has collision? don't need to calculate H
			if(c.Collision){
				c.H = -1;
				continue;
			}

			// Get X,Y coordinates from cells ID
			int Y = c.ID / gridSize;
            int X = c.ID - (Y * gridSize);

			int outputH = 0;

			// X axis
			while( X != goalCell_X){
                if (X < goalCell_X) X++;
				else X--;
                outputH++;	// cyklus probehl, zvys H
			}
            // Y axis
            while (Y != goalCell_Y)
            {
                if (Y < goalCell_Y) Y++;
				else Y--;
                outputH++;
			}
            c.H = outputH;
		}
    }

    // Resets all data and prepares for new calculation
    private void ResetASTAR(Cell start, Cell goal)
    {

        StartCell = start;
        GoalCell = goal;

        resetVariables();

        calculateH();

        actualCell = StartCell;


        OpenList = new List<Cell>();
        ClosedList = new List<Cell>();
        outputPath = new List<Cell>();
    }

}
