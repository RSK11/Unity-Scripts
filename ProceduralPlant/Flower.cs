using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Flower : MonoBehaviour {
    public int PetalCount = 5;
    public Vector2 PetalLength = new Vector2(1f, 2.5f);
    public float PetalThickness = .1f;
    public Vector2 PetalWidth = new Vector2(.5f, 1f);
    public Vector2 PetalTilt = new Vector2(5f, 60f);
    public float PetalDiv = .175f;
    public bool generated = false;

    private Mesh mesh;
    private List<Vector3> verts;
    private List<Petal> Petals;

	// Use this for initialization
	void Awake () {
		if (!generated)
        {
            GeneratePetals();
        }
    }

    [ContextMenu("Generate Petals")]
    void GeneratePetals()
    {
        bool clash = false;
        float Ang = 0f;
        if (generated)
        {
            mesh.Clear();
            Petals.Clear();
            verts.Clear();
        }
        else
        {
            mesh = new Mesh();
            mesh.name = "Flower";
            verts = new List<Vector3>();
            Petals = new List<Petal>();
        }

        generated = true;
        for (int i = 0; i < PetalCount; i++)
        {
            clash = true;
            while (clash)
            {
                Ang = Random.Range(0, 360f);
                clash = false;
                foreach (Petal pet in Petals)
                {
                    if (Mathf.Abs(Ang - pet.Angle.y) < 180f / PetalCount)
                    {
                        clash = true;
                    }
                }
            }
            Petals.Add(new Petal(Random.Range(PetalLength.x, PetalLength.y), Random.Range(PetalWidth.x, PetalWidth.y),
                PetalThickness, new Vector3(0f,Ang, Random.Range(PetalTilt.x,PetalTilt.y)), PetalDiv));
            Petals[i].SetOffset(verts.Count);
            verts.AddRange(Petals[i].Vertices());
        }

        mesh.SetVertices(verts);

        List<int> triangles = new List<int>();
        for (int i = 0; i < PetalCount; i++)
        {
            triangles.AddRange(Petals[i].Triangles());
        }
        mesh.SetTriangles(triangles, 0);
        
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
