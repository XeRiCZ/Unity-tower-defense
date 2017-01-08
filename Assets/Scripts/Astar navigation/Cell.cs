using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Basic class for one cell in game grid

public class Cell {


    public List<Cell> diagonalNeighbours         // all NE,SE,SW,NW neighbour cels  - movenent cost 14
        = new List<Cell>();
    public List<Cell> basicNeighbours            // all N,E,S,W neighbour cells     - movement cost 10
        = new List<Cell>();           
    public int ID;                               // ID of cell

    public float H, G, F;                        // basic A* values (H = heuristic calculated by Manhattan, G = , F = H+G)
    public float T, C;                           // additive values T = tower avoidance, C = colision between enemies avoidance
    
    public Cell parent;                          // parent cell - calculation of this cells values, begined from parent cell

    public Vector3 boundsMin, boundsMax;         // global space bounds of the cell

    public Vector3 center;                       // Center point of the cell

    public Vector2 gridPosition;

    public bool isInClosedList = false;


    private bool collision = false;
    public bool Collision
    {
        get { return collision; }
        set { collision = value;}
    }

    public Cell (int _id)
    { ID = _id; }
}
