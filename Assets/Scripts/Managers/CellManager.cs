using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Manages creation of the grid, projecting it on terrain and mouse cursor interaction with it
public class CellManager : MonoBehaviour {

    public GameManager gameManager;             // gameManager reference

    public int N = 64;                           // final cell count = N*N
    private bool showGrid = false;
    public bool ShowGrid                        // on/off showing grid
    {
        get { return showGrid; }
        set { showGrid = value;
            gridRenderer.enabled = showGrid;
            secondGridRenderer.enabled = showGrid;
        }
    }
    public float gridOffset = 0.5f;              // offset above the ground
    private Renderer gridRenderer;               // basic grid - black lines 
    private Renderer secondGridRenderer;         // secondary grid - colors
    private Texture2D baseGridColorsTexture;     // basic texture only with collisions
    private Texture2D gridColors;                // Texture (N*N) with pixels as colors - used for secondary grid

    [HideInInspector]
    public Cell activeCell;                     // cell overlayed with cursor
    private Cell lastActiveCell;

    // Grid colors
    public Color activeCell_color;
    public Color collisionCell_color;
    public Color defaultCell_color;
    public Color goalCell_color;
    public Color spawnCell_color;
    public Color buildingCell_color_OK;
    public Color buildingCell_color_NO;

    [HideInInspector]
    public List<Cell> allCells = new List<Cell>();              // list of all cells

    [HideInInspector]
    public Cell goalCell;                                       // goal cell
    [HideInInspector]
    public List<Cell> spawnCells = new List<Cell>();            // all spawning cells

    [HideInInspector]
    public List<Cell> newBuildingCells = new List<Cell>();      // those cells affected by new tower
    
    // Transforms in scene, which determines spawning and goal cells
    public Transform goalTransform;
    public List<Transform> spawnTransforms = new List<Transform>();

    public LayerMask collisionMasks;                            // collision masks for generating enviroment collision for cells

    // Finds all helping transforms which represents spawns or goal for enemies
    void initFunctionalTransforms()
    {
        foreach (GameObject g in FindObjectsOfType<GameObject>())
        {
            if (g.tag == "Spawn") spawnTransforms.Add(g.transform);
            else if (g.tag == "Goal") goalTransform = g.transform;
        }
    }

    void Start()
    {
        initFunctionalTransforms();
        // Disable goal and spawn transform renderers
        foreach (Transform t in spawnTransforms) t.GetComponent<Renderer>().enabled = false;
        goalTransform.GetComponent<Renderer>().enabled = false;

        // Inicialization
        createCells();
        assignNeighboursToCells();
        createGrid();
        generateCollisionForCells();
        generateBaseTexture();
        updateColorTexture(true);

        ShowGrid = false;
    }

    // Create all cells
    void createCells()
    {
        float cellSize = 128.0f / N;
        int id = 0;
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++, id++)
            {
                // Creating new cell and calculating its bounds in global space
                Cell newCell = new Cell(id);
                newCell.boundsMin = new Vector3((x-1) * cellSize, 0, y * cellSize);
                newCell.boundsMax = new Vector3((x) * cellSize, 0, (y + 1) * cellSize);
                newCell.gridPosition = new Vector2(y, x);

                // Check if its bounds are around spawn transforms - spawning cells
                foreach (Transform t in spawnTransforms)
                {
                    Vector3 spawnPos = t.transform.position;
                    if (spawnPos.x >= newCell.boundsMin.z && spawnPos.x <= newCell.boundsMax.z &&
                        spawnPos.z >= newCell.boundsMin.x && spawnPos.z <= newCell.boundsMax.x)
                        spawnCells.Add(newCell);
                }
                // Check goal cell
                Vector3 goalPos = goalTransform.transform.position;
                if (goalPos.x >= newCell.boundsMin.z && goalPos.x <= newCell.boundsMax.z &&
                    goalPos.z >= newCell.boundsMin.x && goalPos.z <= newCell.boundsMax.x)
                    goalCell = newCell;

                allCells.Add(newCell);
            }
        }
        
    }

    // Get ID based on X and Y position in grid
    Cell getCellOnPosition(int X, int Y)
    {
        if (X < 0 || X >= N || Y < 0 || Y >= N) 
            return null;

        int outputID = (X * N) + Y;
        return allCells[outputID];
    }

    // For each cell, this method assings its neighbours
    void assignNeighboursToCells()
    {
        foreach(Cell c in allCells){
			// Get X and Y grid position of this cell based on its id
            int Y = (int)c.gridPosition.y;
            int X = (int)c.gridPosition.x;

            if(getCellOnPosition(X,Y + 1) != null)
                c.basicNeighbours.Add(getCellOnPosition(X,Y + 1));           // N
            if (getCellOnPosition(X + 1, Y) != null)
                c.basicNeighbours.Add(getCellOnPosition(X + 1,Y));           // E
            if (getCellOnPosition(X, Y - 1) != null)
                c.basicNeighbours.Add(getCellOnPosition(X, Y - 1));          // S
            if (getCellOnPosition(X - 1, Y) != null)
                c.basicNeighbours.Add(getCellOnPosition(X - 1, Y));          // W
            if (getCellOnPosition(X + 1, Y + 1) != null)
                c.diagonalNeighbours.Add(getCellOnPosition(X + 1, Y+1));     // NE
            if (getCellOnPosition(X + 1, Y - 1) != null)
                c.diagonalNeighbours.Add(getCellOnPosition(X + 1, Y - 1));   // SE
            if (getCellOnPosition(X - 1, Y - 1) != null)
                c.diagonalNeighbours.Add(getCellOnPosition(X - 1, Y - 1));   // SW
            if (getCellOnPosition(X - 1, Y + 1) != null)
                c.diagonalNeighbours.Add(getCellOnPosition(X - 1, Y + 1));   // NW
		}	
    }

    // Basicaly, this method works as decal projector.
    // Plane with 32x32 verticies is projected on the landscape beneath it
    void createGrid()
    {
        // Project grid on top of the terrain
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        projectDecal(
            GetComponent<MeshFilter>(), this.transform, 
            new Vector3(terrainSize.x / 2.0f, terrainSize.z / 2.0f, 1.0f));

        // Set texture tiling to match the N*N grid
        GetComponent<Renderer>().material.mainTextureScale =
            new Vector2(N, N);

        // Create secondary grid by duplicating the first one
        GameObject secondaryGrid = GameObject.Instantiate<GameObject>(this.gameObject);
        Destroy(secondaryGrid.GetComponent<CellManager>());

        // Move it down a bit and assing new material and texture
        secondaryGrid.transform.position = transform.position + Vector3.down * 0.1f;
        Material secondaryMat = new Material(Shader.Find("Particles/Alpha Blended"));
        gridColors = new Texture2D(N, N, TextureFormat.ARGB32, false);
        gridColors.filterMode = FilterMode.Point;
        secondaryMat.mainTexture = gridColors;

        secondaryGrid.GetComponent<Renderer>().material = secondaryMat;

        gridRenderer = GetComponent<Renderer>();
        secondGridRenderer = secondaryGrid.GetComponent<Renderer>();
    }
    
    // Determines which cells have collision (enemies cannot step on it / or player cannot build towers on it)
    void generateCollisionForCells()
    {
        foreach(Cell c in allCells)
        {
            RaycastHit hit;

            float cellHaflSize = (c.boundsMax.x - c.boundsMin.x) / 2.0f;

            Vector3 cellCenter = new Vector3
            (c.boundsMin.z + cellHaflSize, 100, c.boundsMin.x + cellHaflSize);

            c.center = getSampleHeight(cellCenter) + Vector3.up * 0.5f;

            if (Physics.Raycast(cellCenter, Vector3.down, out hit, 999, collisionMasks))
                c.Collision = true;
        }

    }

    // Project mesh verticies on terrain
    void projectDecal(MeshFilter p_Mesh,Transform p_Transform,Vector3 p_Scale)
    {
        // 1. Set the plane scale to match landscape size + move it above the landscape
        
        transform.position = Vector3.zero + (Vector3.up * 100);
        transform.localScale = p_Scale;

        // 2. Raycast the verticies to match the landscape
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh gridMesh = meshFilter.mesh;
        Vector3[] newVerticies = new Vector3[gridMesh.vertices.Length];

        for (int i = 0; i < newVerticies.Length; i++)
        {
            newVerticies[i] = 
                getSampleHeight(transform.TransformPoint(gridMesh.vertices[i]));
        }

        // 3. Offset verticies and convert verticies positions from globalSpace to localSpace
        for (int i = 0; i < newVerticies.Length; i++)
        {
            newVerticies[i] = newVerticies[i] - (Vector3.down * gridOffset);
            newVerticies[i] =
                transform.InverseTransformPoint(
                    newVerticies[i].x, newVerticies[i].y, newVerticies[i].z);
        }
        // 5. update mesh
        meshFilter.mesh = gridMesh;
        meshFilter.mesh.vertices = newVerticies;

        // 6. recalculate bounds of mesh
        meshFilter.mesh.RecalculateBounds();

    }

    // SampleHeight - simple solution for getting terain height at position
    Vector3 getSampleHeight(Vector3 originVertex)
    {
        Vector3 output = Vector3.zero;

        output = new Vector3(originVertex.x, 
            Terrain.activeTerrain.SampleHeight(originVertex), 
            originVertex.z);

        return output;
    }

    // Generates basic color texture for grid
    // it contains only (red) collision cells as red pixels and (empty) default cells as transparent pixels
    void generateBaseTexture()
    {
        baseGridColorsTexture = new Texture2D(N, N, TextureFormat.ARGB32, false);
        baseGridColorsTexture.filterMode = FilterMode.Point;
        foreach (Cell c in allCells)
        {

            Color pixelColor = defaultCell_color;
            if (c.Collision)
                pixelColor = collisionCell_color;
            baseGridColorsTexture.SetPixel(N - (int)c.gridPosition.y, (int)c.gridPosition.x, pixelColor);
        }
        baseGridColorsTexture.Apply();
    }

   

    // Updates second grids texture. (add tower collision, active cell green, etc..)
    void updateColorTexture(bool force = false)
    {
        if (activeCell == lastActiveCell && !force) return;

        // Copy basic enviroment collision texture (no need to make it again)
        gridColors.SetPixels(baseGridColorsTexture.GetPixels());

        // Show builded tower collisions
        foreach (Tower t in gameManager.towerManager.allTowers)
        {
            foreach (Cell c in t.collisionCells)
                gridColors.SetPixel(N - (int)c.gridPosition.y, (int)c.gridPosition.x, collisionCell_color);
        }

        // Show active cell
        if (activeCell != null)
            gridColors.SetPixel(N - (int)activeCell.gridPosition.y, (int)activeCell.gridPosition.x, activeCell_color);

        // Show goal cell and spawning cells
        gridColors.SetPixel(N - (int)goalCell.gridPosition.y, (int)goalCell.gridPosition.x, goalCell_color);
        foreach(Cell c in spawnCells)
            gridColors.SetPixel(N - (int)c.gridPosition.y, (int)c.gridPosition.x, spawnCell_color);


        foreach (Cell c in newBuildingCells)
        {
            Color resultColor = buildingCell_color_OK;
            if (gameManager.collisionCellsHaveCollision) resultColor = buildingCell_color_NO;
            gridColors.SetPixel(N - (int)c.gridPosition.y, (int)c.gridPosition.x, resultColor);
        }

        gridColors.Apply();
        lastActiveCell = activeCell;
    }

    // Raycasting for getting active cell (which cell is beneath mouse cursor)
    void mouseInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = ( 1 << 8 );  // terrain layer mask

        if(Physics.Raycast(ray,out hit,9999,layerMask))
        {
            // Now determine which cell we hit by its X and Z bounds
            Cell hittedCell = null;
            foreach(Cell c in allCells)
            {
                if (hit.point.x >= c.boundsMin.z && hit.point.x <= c.boundsMax.z &&
                    hit.point.z >= c.boundsMin.x && hit.point.z <= c.boundsMax.x)
                {
                    hittedCell = c;
                    
                    break;
                }
            }
            if (hittedCell != null)
            {
                activeCell = hittedCell;
            }
        }
    }

    

    void Update()
    {
        mouseInteraction();
        updateColorTexture();

        if (Input.GetKeyDown(KeyCode.F))
            ShowGrid = !ShowGrid;
    }
}
