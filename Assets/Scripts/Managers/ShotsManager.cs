using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// (c) Jan Urubek

// Create pools for every shot type and manages shot movement
public class ShotsManager : MonoBehaviour {

    // Pool of gameobjects which are instantiates at begging, because garbage collector and allocations (instantiating) at runtime
    // may cause performance slowdowns
    class Pool
    {
        public GameObject pool;
        public List<Shot> childs = new List<Shot>();
    };


    public List<GameObject> shotsTypes = new List<GameObject>();        // all types of shots
    List<Pool> pools = new List<Pool>();                                // all pools (for every type of shot is one pool)

    public int poolSize = 30;                                           // size of pool (how many child shots are instantiatet at beginning)

    void Start()
    {
        // Create pools
        foreach (GameObject t in shotsTypes)
        {
            Pool newPool = new Pool();
            newPool.pool = new GameObject();
            newPool.pool.name = t.name + "_pool";

            // creating childs
            for (int i = 0; i < poolSize; i++)
            {
                GameObject newChild = GameObject.Instantiate<GameObject>(t);
                newPool.childs.Add(newChild.GetComponent<Shot>());
                newChild.transform.parent = newPool.pool.transform.parent;
            }

            pools.Add(newPool);
        }
    }

    // Public method for creating a new shot 
    // (typeID - type of shot, _t - enemy reference, _sp - speed of shot, _dam - damage of shot, origin - shot origin)
    public void createShot(int typeID, Enemy _t, float _sp, float _dam, Vector3 origin)
    {
        Pool chosenPool = pools[typeID];
        foreach (Shot s in chosenPool.childs)
        {
            if (s.avaiable)
            {
                s.init(_t, _sp, _dam, origin);
                return;
            }
        }
    }
	
}
