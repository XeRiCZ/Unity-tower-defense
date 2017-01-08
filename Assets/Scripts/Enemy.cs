using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Base class for simple - walking enemy
public class Enemy : MonoBehaviour {

    // Basic properties
    private float maximumHealth;        // maximum health
    public float actualHealth;          // actual health
    private float speed;                // his speed
    private float gold;                 // how much gold he gives to player if he dies
    private bool smart;                 // doesnt work yet, but smart enemy will try to go around the towers (more safe way)
    private float animSpeed;            // animation speed

    private int pathIterator = 0;       // iterator ID for list of cells which represents path to goal

    // Animation names
    public string idleAnimation_name;
    public string walkAnimation_name;
    public string runAnimation_name;
    public string deathAnimation_name;

    private Animation animController;

    // Controlling properties
    private bool spawned = false;                                   // was enemy spawned
    private float finalSpawningTime;                                // time when enemy spawns on map
    [HideInInspector]
    public List<Cell> pathToGoal = new List<Cell>();                // list of cells -> path to goal
    public List<Renderer> allRendereres = new List<Renderer>();     // all enemies renderers
    private nagivation_core Navigation;                             // navigation reference
    private GameManager gameManager;                                // gameManager reference
    private bool destroy = false;                                   // destroy game object?

    private bool destroying = false;
    
    private Cell spawningCell;          // cell where enemy started (was spawned)
    private Cell goalCell;              // cell where enemy wants to go (goal)


    // Enemy initialization
    public void initEnemy(float _hea,float _spe,float _gol,float _time, bool _sma,
        float _anim, nagivation_core _nav, Cell _spawCe, Cell _goCel, GameManager _gam)
    {
        animController = GetComponent<Animation>();


        maximumHealth = _hea;
        actualHealth = maximumHealth;
        speed = _spe;
        gold = _gol;
        smart = _sma;
        animSpeed = _anim;
        gameManager = _gam;
        Navigation = _nav;

        spawningCell = _spawCe;
        goalCell = _goCel;

        animController[walkAnimation_name].speed = animSpeed;
        animController[runAnimation_name].speed = animSpeed - 1;

        finalSpawningTime = Time.time + _time;

    }

    void Start()
    {
        foreach (Renderer r in allRendereres)
            r.enabled = false;
    }

    // New a* calculation
    public void recalculatePath()
    {
        Cell startingCell = pathToGoal[pathIterator];
        pathIterator = 0;
        pathToGoal = Navigation.calculatePath(startingCell, goalCell);
    }

    // Spawning
    void Spawn()
    {
        
        foreach (Renderer r in allRendereres)
            r.enabled = true;
        spawned = true;

        pathToGoal = gameManager.navigation.calculatePath(spawningCell, goalCell);  // A* calculation
        gameManager.enemyManager.allEnemies.Add(this);
    }

    // No need for physx - movement is done in update()
    void UpdateMovement()
    {
        // Update rotation of enemy towards the path
        Quaternion slerpWhere = Quaternion.LookRotation(pathToGoal[pathIterator].center - transform.position);
        Vector3 newRotation = Quaternion.Lerp(transform.rotation, slerpWhere, 9 * Time.deltaTime).eulerAngles;
        newRotation.x = 0; newRotation.z = 0;
        transform.rotation = Quaternion.Euler(newRotation);

        // Update position
        Vector3 newPosition = transform.position + (transform.forward * (Time.deltaTime * speed));
        newPosition.y = Terrain.activeTerrain.SampleHeight(newPosition) + 0.25f;
        transform.position = newPosition;

        // Iterate through path
        if (Vector3.Distance(transform.position, pathToGoal[pathIterator].center) <= 1.4f)
        {
            pathToGoal[pathIterator].C -= 5000;      // remove old collision value
            pathIterator++;
        }

        if (animSpeed >= 2)
            animController.CrossFade(runAnimation_name);
        else animController.CrossFade(walkAnimation_name);
    }

  

	// Update is called once per frame
	void Update () {
        if (Time.time < 5 || destroying) // 5 sec for preparation
            return;
        else if (!spawned)
            return;

        // Reached goal
        if (pathIterator >= pathToGoal.Count)
        {
            gameManager.Lives--;
            gameManager.updateUI();
            gameManager.enemyManager.allEnemies.Remove(this);
            Destroy(this.gameObject);
            return;
        }
        else if (actualHealth <= 0)     // enemy died
        {
            gameManager.Gold += gold;
            gameManager.updateUI();
            animController.CrossFade(deathAnimation_name, 0.1f);
            gameManager.enemyManager.allEnemies.Remove(this);
            Invoke("DestroyNow", 4.2f);
            destroying = true;
            return;
        }

        UpdateMovement();
	}

    // Destroying it (dealocation)
    void DestroyNow()
    {    
        Destroy(this.gameObject);
    }
}
