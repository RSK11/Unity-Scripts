using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]

// Generates procedural cartoon-like clouds.
// The cloud is a procedural cube that is rounded using Catlike Coding's rounding technique, except with random variation for the movement along the normal per vertex.
public class Cloud : MonoBehaviour {

    // The cloud's size in world space
    private Vector3 dimensions;
    // The number of segments to use for each dimension
    private Vector3Int steps;
    // The roundness factor, according to Catlike Coding, it should be half or less of smallest dimension
    private float roundness = 1f;
    // The range for the random roundness scalar, recommended [0.7-1.0]
    private Vector2 randomOffsetRange;

    // The data structures containing the mesh data
    private Mesh mesh;
    private List<Vector3> verts;
    private List<Vector3> norms;
    private List<int> tris;

    // Generate the cloud mesh with properties matching the variables above
    public void Generate(Vector3 dims, Vector3Int stepCount, float round, Vector2 growRange)
    {
        dimensions = dims;
        steps = stepCount;
        roundness = round;
        randomOffsetRange = growRange;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "CloudMesh";

            GenVerts();
            GenTris();

            GetComponent<MeshFilter>().mesh = mesh;

            // Resize the box collider to fit the cloud
            float shrink = 2 * (randomOffsetRange.y - randomOffsetRange.x) * roundness;
            GetComponent<BoxCollider>().size = new Vector3(dimensions.x - shrink, dimensions.y - shrink, dimensions.z - shrink);
        }
    }

    // Clear the cloud mesh, for potential gameobject reuse
    public void Clear()
    {
        mesh.Clear();
        mesh = null;
        verts.Clear();
        norms.Clear();
        tris.Clear();
    }

    // Generate the cloud vertices and normals
    private void GenVerts()
    {
        verts = new List<Vector3>();
        norms = new List<Vector3>();

        for (int indz = 0; indz <= steps.z; indz++)
        {
            GenWall(indz);
        }

        GenEnds();

        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
    }

    // Generate the vertices of the cloud in the x and y directions
    private void GenWall(int zstep)
    {
        Vector3 bottomleft = new Vector3(-dimensions.x / 2, -dimensions.y / 2, (-dimensions.z / 2) + (dimensions.z * zstep / steps.z));
        for (int y = 0; y <= steps.y; y++)
        {
            RoundVertex(bottomleft.x, bottomleft.y + dimensions.y * y / steps.y, bottomleft);
        }
        for (int x = 1; x <= steps.x; x++)
        {
            RoundVertex(bottomleft.x + dimensions.x * x / steps.x, -bottomleft.y, bottomleft);
        }
        for (int ytwo = steps.y - 1; ytwo >= 0; ytwo--)
        {
            RoundVertex(-bottomleft.x, bottomleft.y + dimensions.y * ytwo / steps.y, bottomleft);
        }
        for (int xtwo = steps.x - 1; xtwo > 0; xtwo--)
        {
            RoundVertex(bottomleft.x + dimensions.x * xtwo / steps.x, bottomleft.y, bottomleft);
        }
    }

    // Generate the vertices of the cloud in the z direction, ignoring the outer rings which were already generated
    private void GenEnds()
    {

        Vector3 bottomleft = new Vector3(-dimensions.x / 2, -dimensions.y / 2, -dimensions.z / 2);
        for (int indy = 1; indy < steps.y; indy++)
        {
            for (int indx = 1; indx < steps.x; indx++)
            {
                RoundVertex(bottomleft.x + dimensions.x * indx / steps.x, bottomleft.y + dimensions.y * indy / steps.y, bottomleft);
            }
        }

        bottomleft.z = -bottomleft.z;
        for (int indy = 1; indy < steps.y; indy++)
        {
            for (int indx = 1; indx < steps.x; indx++)
            {
                RoundVertex(bottomleft.x + dimensions.x * indx / steps.x, bottomleft.y + dimensions.y * indy / steps.y, bottomleft);
            }
        }
    }

    // Generate the cloud triangles
    private void GenTris()
    {
        tris = new List<int>();

        for (int indz = 0; indz < steps.z; indz++)
        {
            GenWallTris(indz);
        }

        GenEndTris();

        mesh.SetTriangles(tris, 0);
    }

    // Generate the triangles for the left, right, top, and bottom faces (The wall rings)
    private void GenWallTris(int zstep)
    {
        int num = 2 * (steps.x + steps.y);
        int start = zstep * num;

        for (int ind = 0; ind < num; ind++)
        {
            tris.Add(start + ind);
            tris.Add(start + num + ind);
            tris.Add(start + (ind + 1) % num);

            tris.Add(start + num + ind);
            tris.Add(start + num + (ind + 1) % num);
            tris.Add(start + (ind + 1) % num);
        }
    }
    
    // Generate the triangles for the front and back faces
    private void GenEndTris()
    {
        for (int indx = 0; indx < steps.x; indx++)
        {
            for (int indy = 0; indy < steps.y; indy++)
            {
                int a = Lookup(indx, indy, false);
                int b = Lookup(indx, indy + 1, false);
                int c = Lookup(indx + 1, indy, false);
                int d = Lookup(indx + 1, indy + 1, false);

                tris.Add(a);
                tris.Add(c);
                tris.Add(b);
                tris.Add(b);
                tris.Add(c);
                tris.Add(d);

                a = Lookup(indx, indy, true);
                b = Lookup(indx, indy + 1, true);
                c = Lookup(indx + 1, indy, true);
                d = Lookup(indx + 1, indy + 1, true);

                tris.Add(a);
                tris.Add(b);
                tris.Add(c);
                tris.Add(b);
                tris.Add(d);
                tris.Add(c);
            }
        }
    }

    // Offset the vertex based on its position and a random scalar, and use the offset direction as the normal
    // Based on Catlike Coding's rounded cube technique
    // BL is the bottom left point of current ring being generated
    private void RoundVertex(float x, float y, Vector3 bl)
    {
        // Add the vertex
        int current = verts.Count;
        verts.Add(new Vector3(x, y, bl.z));

        // Inner is representative of the same point on a scaled down equivalent cube
        Vector3 inner = verts[current];

        // If the vertex is on the left or right, shrink the inner x coordinate
        if (x < bl.x + roundness)
        {
            inner.x = bl.x + roundness;
        }
        else if (x > -bl.x - roundness)
        {
            inner.x = -bl.x - roundness;
        }

        // If the vertex is on the top or bottom, shrink the inner y coordinate
        if (y < bl.y + roundness)
        {
            inner.y = bl.y + roundness;
        }
        else if (y > -bl.y - roundness)
        {
            inner.y = -bl.y - roundness;
        }

        // If the vertex is on the front or back, shrink the inner z coordinate
        float zdim = dimensions.z / 2;
        if (bl.z < -zdim + roundness)
        {
            inner.z = -zdim + roundness;
        }
        else if (bl.z > zdim - roundness)
        {
            inner.z = zdim - roundness;
        }

        // Use the direction between the shrunken cube and the vertex and the normal
        norms.Add((verts[current] - inner).normalized);
        // Move the vertex along the normal using the roundness factor and a random scalar
        verts[current] = inner + norms[current] * roundness * Random.Range(randomOffsetRange.x,randomOffsetRange.y);
    }

    // Lookup a point on the front or back faces, for easier triangle generation
    private int Lookup(int x, int y, bool front)
    {
        int res = 0;

        // If the point is on the outer ring
        if (x == 0 || x == steps.x || y == 0 || y == steps.y)
        {
            if (x == 0)
            {
                res = y;
            }
            else if (y == steps.y)
            {
                res = steps.y + x;
            }
            else if (x == steps.x)
            {
                res = steps.y + steps.x + (steps.y - y);
            }
            else
            {
                res = 2 * steps.y + steps.x + (steps.x - x);
            }
            if (!front)
            {
                res += (steps.z * 2 * (steps.x + steps.y));
            }
        }
        // If the point is on the inside of the face
        else
        {
            int count = (steps.x - 1) * (steps.y - 1);
            res = (steps.x - 1) * (y - 1) + (x - 1);
            res = verts.Count - count + res;
            if (front)
            {
                res -= count;
            }
        }

        return res;
    }
}
