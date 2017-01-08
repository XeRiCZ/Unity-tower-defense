using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (c) Jan Urubek

// Manages all towers (iterates through them and checks if they can shoot)
public class TowerManager : MonoBehaviour {

    GameManager gameManager;
    public List<Tower> allTowers = new List<Tower>();
    float iterationTimer = 0;
    int it = 0;

    void Update()
    {
        if(allTowers.Count == 0) return;

        if (Time.time > iterationTimer)
        {
            if (it >= allTowers.Count-1)
                it = 0;
            else it++;

            // Shooting check
            if (allTowers[it].State == Tower.towerState.ready)
                allTowers[it].shootingUpdate();

            iterationTimer = Time.time + 0.03f;
        }
    }

}
