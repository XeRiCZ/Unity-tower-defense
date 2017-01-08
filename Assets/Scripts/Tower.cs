using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Jan Urubek

// Basic class for tower
public class Tower : MonoBehaviour {

    public enum towerState
    {
        innactive,      // player chooses where to build
        building,       // animation of building is played
        ready           // tower works normaly
    };

    private towerState state = towerState.innactive;
    public towerState State
    {
        get { return state; }
        set
        {
            state = value;
            if (state == towerState.building)
            {
                // Set the building timer and show dynamic (skinned mesh) tower
                buildingTimer = Time.time + buildingTime;
                setStaticVisibility(false);
            }
            if (state == towerState.ready) setStaticVisibility(true);
        }
    }
    
    // Basic variables for tower
    public float range_radius = 10;
    public float attackDelay = 1;
    public float damageMin = 10;
    public float damageMax = 20;
    public float buildingTime = 5.0f;
    public int goldCost = 75;

    // Black decal indicating a range radius
    public Decal radiusDecal;
    
    // Timer variables
    private float buildingTimer = 0;
    private float shootingTimer = 0;

    // Renderers
    public Renderer staticRenderer;
    public List<Renderer> dynamicRenderers = new List<Renderer>();

    // Animation
    private Animation dynamicAnimation;

    [HideInInspector]
    public List<Cell> collisionCells = new List<Cell>();        // cells where tower is standing

    [HideInInspector]
    public EnemyManager enemyManager;           // reference

    void Start()
    {
        setStaticVisibility(true);
        dynamicAnimation = dynamicRenderers[0].transform.parent.GetComponent<Animation>();
        dynamicAnimation["staveniAll"].speed = dynamicAnimation["staveniAll"].length / buildingTime;

        radiusDecal.scale =
            new Vector3(range_radius, range_radius, 1);
    }

    // switches visibility between static and dynamic renderers
    // true - static visible | false - dynamic visible
    void setStaticVisibility(bool input)
    {
        foreach (Renderer r in dynamicRenderers)
            r.enabled = !input;
        staticRenderer.enabled = input;
    }

    // In this method, tower finds target in its range and if its ready to shoot, it shoots :)
    public void shootingUpdate()
    {
        if(Time.time < shootingTimer)
            return;                         // not ready to shoot yet


        foreach (Enemy e in enemyManager.allEnemies)
        {
            Vector3 enemyPosition = e.transform.position;
            // Shooting
            if (Vector3.Distance(transform.position, enemyPosition) < range_radius)
            {
                // target is close, create new shots
                enemyManager.gameManager.shotsManager.
                    createShot(0, e, 1, Random.RandomRange(damageMin, damageMax), transform.position + Vector3.up * 4.5f);
                shootingTimer = Time.time + attackDelay;
                return;
            }
        }
    }

	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case towerState.building:
                // Play building animation
                dynamicAnimation.Play("staveniAll");
                if (Time.time > buildingTimer)
                    state = towerState.ready;   // building animation ended
                break;
            case towerState.ready:
               // shootingUpdate();
                break;
            default: break;
        }
	}
}
