using UnityEngine;
using System.Collections;

// (c) Jan Urubek

// Button interatcion
public class button : MonoBehaviour {


    public GameManager gameManager;
    public GameObject towerPrefab;

    public Texture2D defaultTexture;
    public Texture2D overlayTexture;
    public Texture2D clickTexture;
    UnityEngine.UI.RawImage thisImage;
    buttonTrigger thisButtonTrigger;

	// Use this for initialization
	void Start () {
        thisButtonTrigger = GetComponent<buttonTrigger>();
        thisImage = GetComponent<UnityEngine.UI.RawImage>();

        
	}
	
	// Update is called once per frame
	void Update () {
        switch (thisButtonTrigger.thisState)
        {
            case triggerState.state_beginClicked:
                thisImage.texture = clickTexture;
                break;
            case triggerState.state_endClicked:

                // First check gold
                int goldCost = towerPrefab.GetComponent<Tower>().goldCost;
                if (gameManager.Gold < goldCost)
                    return;

                // Building new tower - clicked 
                gameManager.Mode = GameManager.gameMode.building;
                gameManager.newTowerGO = GameManager.Instantiate<GameObject>(towerPrefab);
                thisButtonTrigger.thisState = triggerState.state_default;
                thisImage.texture = defaultTexture;
                break;
            case triggerState.state_overlay:
                thisImage.texture = overlayTexture;
                break;
            default:
                thisImage.texture = defaultTexture;
                break;
        }
	}
}
