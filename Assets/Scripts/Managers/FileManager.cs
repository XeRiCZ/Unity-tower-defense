using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

// (c) Jan Urubek

// XML parser
// Reads script for current level
// this script describes what enemies will appear and when they appear

public class FileManager : MonoBehaviour {

    public GameManager gameManager;
    public GameObject prefab_zombie;
    public int actualLevel = 1;

    // Parse throug all enemies and also it creates them
    void parseEnemies(XmlReader reader)
    {
        
        reader.ReadToFollowing("Unit");
        do
        {
            // Parsing all needed attributes for new enemy
            reader.MoveToFirstAttribute();
            string type = reader.Value;
            reader.MoveToNextAttribute();
            int health = int.Parse(reader.Value);
            reader.MoveToNextAttribute();
            float speed = float.Parse(reader.Value);
            reader.MoveToNextAttribute();
            int gold = int.Parse(reader.Value);
            reader.MoveToNextAttribute();
            int spawn = int.Parse(reader.Value);
            reader.MoveToNextAttribute();
            float timeToSpawn = float.Parse(reader.Value);
            reader.MoveToNextAttribute();
            float animSpeed = float.Parse(reader.Value);
            reader.MoveToNextAttribute();
            bool smart = false;
            if (reader.Value.ToLower() == "true")
                smart = true;
            print("Parsing unit " + type + " time = " + timeToSpawn);


            // Creation of new enemy based on parsed informations
            GameObject newEnemy;
            newEnemy = GameObject.Instantiate<GameObject>(prefab_zombie);

            newEnemy.transform.position
                = gameManager.cellManager.spawnTransforms[spawn - 1].position;
            newEnemy.GetComponent<Enemy>().initEnemy
                (health, speed, gold, timeToSpawn, smart, animSpeed,
                gameManager.navigation, gameManager.cellManager.spawnCells[spawn - 1], gameManager.cellManager.goalCell, gameManager);
            newEnemy.GetComponent<Enemy>().Invoke("Spawn", timeToSpawn);
            
        }
        while (reader.ReadToFollowing("Unit"));
    }

    public void Parse()
    {
        // Load XML from resources folder as TextAsset
        TextAsset textAsset = (TextAsset)Resources.Load("level" + actualLevel);
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(textAsset.text);

        string obsahXML = xmldoc.InnerXml;

        // Parsing through XmlReader
        using (XmlReader reader = XmlReader.Create(new StringReader(obsahXML)))
        {
            // Set initial gold and lives from XML
            reader.ReadToFollowing("Script");
            reader.MoveToFirstAttribute();
            gameManager.Gold = int.Parse(reader.Value);
            reader.MoveToNextAttribute();
            gameManager.Lives = int.Parse(reader.Value);

            print("Parsed " + gameManager.Gold + " gold and " + gameManager.Lives + " lives .");


            parseEnemies(reader);
        }

    }
}
