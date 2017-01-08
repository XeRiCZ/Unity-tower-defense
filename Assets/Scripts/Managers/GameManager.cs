using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Main class - have references on other managers
public class GameManager : MonoBehaviour {

    public nagivation_core navigation;
    public CellManager cellManager;
    public EnemyManager enemyManager;
    public FileManager fileManager;
    public TowerManager towerManager;
    public ShotsManager shotsManager;

    public enum gameMode
    {
        none,
        building,
        menu
    };

    // Represents actual state of game
    private gameMode mode = gameMode.none;
    public gameMode Mode
    {
        get { return mode; }
        set
        {
            mode = value;
            switch (mode)
            {
                case (gameMode.building):
                    cellManager.ShowGrid = true;
                    break;
                case (gameMode.menu):
                    break;
                default:
                    cellManager.ShowGrid = false;
                    break;
            }
        }
    }

    [HideInInspector]
    public GameObject newTowerGO;

    // Reference to UI texts
    public UnityEngine.UI.Text livesText;
    public UnityEngine.UI.Text goldText;


    private float gold = 0;
    public float Gold
    {
        get { return gold; }
        set { gold = value; }
    }

    private float lives = 15;
    public float Lives
    {
        get { return lives; }
        set { lives = value; }
    }

    [HideInInspector]
    public bool collisionCellsHaveCollision = false;

	// Use this for initialization
	void Start () {
        // Synchronize all cells
        navigation.setAllCellsAndGrid(ref cellManager.allCells,cellManager.N);

        // Load enemies for this level
        fileManager.Parse();
        updateUI();
	}

    // Just updatest texts
    public void updateUI()
    {
        livesText.text = ""+ lives;
        goldText.text = ""+ gold;
    }

    // If the new tower was build and some enemies have this new tower in their way
    // new Astar calculation is neccesary for those enemies
    void PathIntersectingEnemies(Tower newTower)
    {
        List<Enemy> intersectingEnemies = new List<Enemy>();
        // Go through all enemies cells in his path and check if it has this new collision
        foreach (Enemy e in enemyManager.allEnemies)
        {
            foreach (Cell c in e.pathToGoal)
            {
                if (c.Collision)
                {
                    intersectingEnemies.Add(e);
                    break;
                }
            }
        }

        // Recalculate Astar by enemy manager - it will not recalculate all at same time, but iteratively through time
        enemyManager.RecalculateA_star(intersectingEnemies);
    }

    // Update is called once per frame
	void Update () {
        if (lives <= 0)
            Application.Quit();

        if (mode == gameMode.building)
        {
            // Update builded tower position under cursor by raycasting on terrain
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = (1 << 8);  // terrain layer mask

            if (Physics.Raycast(ray, out hit, 9999, layerMask))
            {
                newTowerGO.transform.position =
                    new Vector3(hit.point.x, Terrain.activeTerrain.SampleHeight(hit.point), hit.point.z);
            }

            // Get list of all affected cells by tower collider
            List<Cell> buildingAffectedCells = new List<Cell>();
            Bounds bounds = newTowerGO.GetComponent<Collider>().bounds;

            foreach (Cell c in cellManager.allCells)
            {
                if (c.center.x >= bounds.min.x && c.center.x <= bounds.max.x &&
                    c.center.z >= bounds.min.z && c.center.z <= bounds.max.z)
                    buildingAffectedCells.Add(c);
            }

            // Check if all new cells have no collison
            collisionCellsHaveCollision = false;
            foreach (Cell c in buildingAffectedCells)
            {
                if (c.Collision)
                {
                    collisionCellsHaveCollision = true;
                    break;
                }
            }
            // Now check if some enemy isnt blocking tower
            foreach (Enemy e in enemyManager.allEnemies)
            {
                Vector3 enemyPosition = e.transform.position;
                if (enemyPosition.x >= bounds.min.x && enemyPosition.x <= bounds.max.x &&
                    enemyPosition.z >= bounds.min.z && enemyPosition.z <= bounds.max.z)
                {
                    collisionCellsHaveCollision = true;
                    break;
                }
            }
            cellManager.newBuildingCells = buildingAffectedCells;


            // Controls
            if (Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.Escape))  // Cancel
            {
                cellManager.newBuildingCells = new List<Cell>();
                cellManager.ShowGrid = false;
                mode = gameMode.none;
                Destroy(newTowerGO);
            }

            else if (Input.GetMouseButton(0) && !collisionCellsHaveCollision)  // left mouse pressed - building tower
            {
                Tower newTower = newTowerGO.GetComponent<Tower>();
                Gold -= newTower.goldCost;

                cellManager.newBuildingCells = new List<Cell>();
                mode = gameMode.none;
                cellManager.ShowGrid = false;

                // Set the new collision for cells where tower stands
                foreach (Cell c in buildingAffectedCells)
                    c.Collision = true;

                // Update variables
                newTower.collisionCells = buildingAffectedCells;
                newTower.State = Tower.towerState.building;
                newTower.enemyManager = enemyManager;
                towerManager.allTowers.Add(newTower);

                // Check intersecting enemies paths with this tower
                PathIntersectingEnemies(newTower);
                updateUI();
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Escape))
                Application.Quit();

        }

	}
}
