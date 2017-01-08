using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Right now its functions is only for recalculating a* paths for enemies
// in future it could be usefull for more things

public class EnemyManager : MonoBehaviour {

    public List<Enemy> allEnemies = new List<Enemy>();  // list of all enemies (active enemies on map)
    
    List<Enemy> workingList = new List<Enemy>();        // actual list of enemies, which needs to recalculate A*
    int workingListIterator = 0;                        // iterator through working list
    bool recalculation = false;                         // is recalculation in progress?
    float recalculateA_time = 0;                        // timer for recalculation (for dividing individual enemies to more time intervals)

    public GameManager gameManager;         // referece

    // Method for initializing A* recalculation for specific group (or all) enemies
    public void RecalculateA_star(List<Enemy> specificEnemies = null) 
    {
        // if specific is empty, then recalculation for everyone
        if (specificEnemies == null)
            workingList = allEnemies;
        else workingList = specificEnemies;

        workingListIterator = 0;
        recalculation = true;
        iterateWorkingList();
    }

    // Iterate through list of enemies, which need path recalculation
    void iterateWorkingList()
    {
        if (workingListIterator >= workingList.Count)
        {
            // iteration is done
            recalculation = false;
            return;
        }

        workingList[workingListIterator].recalculatePath();
        workingListIterator++;

    }

	void Update () {
        // Don't recalculate all enemies A* at the same time
        if (Time.time > recalculateA_time && recalculation)
        {
            iterateWorkingList();
            recalculateA_time = Time.time + 0.125f;
        }
	}
}
