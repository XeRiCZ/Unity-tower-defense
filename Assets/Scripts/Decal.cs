using UnityEngine;
using System.Collections;

// (c) Jan Urubek

// Projecting plane in realtime on ground
public class Decal : MonoBehaviour {

    Vector3[] initVerticies;
    MeshFilter thisMeshFilter;
    Mesh thisMesh;

    public float offset = 0.15f;
    public Vector3 scale;

    void Start()
    {
        thisMeshFilter = GetComponent<MeshFilter>();
        thisMesh = thisMeshFilter.mesh;
        initVerticies = thisMesh.vertices;
    }

    void projectDecal()
    {
        // 0. reset verticies
        thisMesh.vertices = initVerticies;
        
        // 1. Set the plane scale to match landscape size + move it above the landscape

        //transform.position = transform.position + (Vector3.up * 100);
        transform.localScale = scale;

        // 2. Raycast the verticies to match the landscape
        Vector3[] newVerticies = new Vector3[thisMesh.vertices.Length];

        for (int i = 0; i < newVerticies.Length; i++)
        {
            newVerticies[i] =
                getSampleHeight(transform.TransformPoint(thisMesh.vertices[i]));
        }

        // 3. Offset verticies and convert verticies positions from globalSpace to localSpace
        for (int i = 0; i < newVerticies.Length; i++)
        {
            newVerticies[i] = newVerticies[i] - (Vector3.up * offset);
            newVerticies[i] =
                transform.InverseTransformPoint(
                    newVerticies[i].x, newVerticies[i].y, newVerticies[i].z);
        }
        // 5. update mesh
        thisMesh.vertices = newVerticies;

        // 6. recalculate bounds of mesh
        thisMesh.RecalculateBounds();

    }

    Vector3 getSampleHeight(Vector3 originVertex)
    {
        Vector3 output = Vector3.zero;

        output = new Vector3(originVertex.x,
            Terrain.activeTerrain.SampleHeight(originVertex),
            originVertex.z);

        return output;
    }

    void Update()
    {
        projectDecal();
    }
}
