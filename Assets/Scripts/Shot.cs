using UnityEngine;
using System.Collections;

// (c) Jan Urubek

// Behavior for one individual shot
public class Shot : MonoBehaviour {

    Enemy target;
    float speed;
    float damage;
    public bool avaiable = true;

    float travelProgress = 0;
    Vector3 originPosition;

    // Shot initialization and preparation for traveling to its target
    public void init(Enemy _t, float _sp, float _dam,Vector3 origin)
    {
        target = _t;
        speed = _sp;
        damage = _dam;
        originPosition = origin;
        transform.position = origin;
        travelProgress = 0;
        avaiable = false;
    }

    void Update()
    {
        if (!avaiable)
        {
            if (target == null) // Some other shot already killed enemy
            {
                avaiable = true;
                return;
            }

            transform.rotation = 
                Quaternion.LookRotation((target.transform.position - transform.position).normalized);

            // Travel progress lerps the arrow from its origin position to enemy position <0->1>
            transform.position =
                Vector3.Lerp(originPosition, target.transform.position, travelProgress);

            if (travelProgress >= 1)    // Shot reached its target
            {
                target.actualHealth -= damage;
                avaiable = true;
            }

            travelProgress += Time.deltaTime * (speed * 4.0f);
        }
        else transform.position = new Vector3       // "hide shot"
            (-666, -666, -666);
    }

}
