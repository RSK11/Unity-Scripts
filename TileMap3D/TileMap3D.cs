using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A 2D TileMap for Unity 3D that doesn't require Tile objects or multiple textures.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class TileMap3D : MonoBehaviour {

    // The tile Dimensions in world space
    public Vector2 tileDim = new Vector2(10, 10);
    // The texture dimensions (number of squares per row/column)
    public Vector2 texDim = new Vector2(6,3);

    // Mesh Data
    private List<Vector3> verts = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> tris = new List<int>();

    // The mesh object used for the TileMap
    private Mesh mapMesh;

    // The total vertex Count
    private int vertCount = 0;

    // The layout of the tile map. Each integer corresponds to the square to use from the texture for that given tile.
    // Tiles are generated from the top, left to right, moving to the next row when the x position is greater than the map width.
    // Contains a default map
    private int[] mapData = new int[] {10, 10, 10, 10, 10,
                                       10,  0,  1,  2, 10,
                                       10,  6,  7,  8, 10,
                                       10, 12, 13, 14, 10,
                                       10, 10, 10, 10, 10,};

    // The TileMap Dimensions (number of tiles per row/column)
    // Contains the dimensions of the default map
    private Vector2 mapDim = new Vector2(5, 5);

    // The total Y Dimension of the map in world space, for use across various methods
    private float yDim;

    // TileMap Collider
    private BoxCollider boxCollider;
    // Collider height
    private float collHeight = 1f;



    // Set the TileMap layout and dimensions
    public void SetLayout(int[] newMap, Vector2 newDim)
    {
        mapData = newMap;
        mapDim = newDim;
    }

    // Set the Texture and its dimensions
    public void SetTexture(Texture newTex, Vector2 newDim)
    {
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", newTex);
        texDim = newDim;
    }

    // Generate the TileMap
    [ContextMenu("Generate Map")]
    public void Generate()
    {
        // Get the box collider
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        // Clear the data structures
        vertCount = 0;
        mapMesh = new Mesh();
        verts.Clear();
        uvs.Clear();
        tris.Clear();

        // Calculate the total Y dimension of the TileMap
        yDim = mapDim.y * tileDim.y;

        // Generate the Tiles
        int x, y;
        for (int ind = 0; ind < mapData.Length; ind++)
        {
            x = Mathf.FloorToInt(ind % mapDim.x);
            y = Mathf.FloorToInt(ind / mapDim.x);
            Tile(x, y, mapData[ind]);
        }

        // Populate the Mesh data
        mapMesh.SetVertices(verts);
        mapMesh.SetTriangles(tris.ToArray(),0);
        mapMesh.SetUVs(0,uvs);
        mapMesh.RecalculateNormals();

        // Set the MeshFilter mesh (Required so the mesh actually renders)
        GetComponent<MeshFilter>().mesh = mapMesh;

        // Resize the collider to fit the map
        AdjustCollider();
    }

    // Computes the data for an individual Tile at x,y
    // Uses the given square from the texture (textSquare) to determine the UV coordinates.
    private void Tile(int x, int y, int textSquare)
    {
        // Add a square of vertices at map position x,y
        verts.Add(new Vector3(x * tileDim.x, 0f, yDim - y * tileDim.y));
        verts.Add(new Vector3((x + 1) * tileDim.x, 0f, yDim - y * tileDim.y));
        verts.Add(new Vector3(x * tileDim.x, 0f, yDim - (y + 1) * tileDim.y));
        verts.Add(new Vector3((x + 1) * tileDim.x, 0f, yDim - (y + 1) * tileDim.y));

        // Add the first triangle
        tris.Add(vertCount);
        tris.Add(vertCount + 1);
        tris.Add(vertCount + 2);
        // Add the second triangle
        tris.Add(vertCount + 1);
        tris.Add(vertCount + 3);
        tris.Add(vertCount + 2);

        // Increment the vertex count
        vertCount += 4;

        // Determine the location of the square from the texture to use
        int tileCol = Mathf.FloorToInt(textSquare % texDim.x);
        int tileRow = Mathf.FloorToInt(textSquare / texDim.x);

        // Calculate and add the tile UVs
        uvs.Add(new Vector2(tileCol / texDim.x, tileRow / texDim.y));
        uvs.Add(new Vector2((tileCol + 1) / texDim.x, tileRow / texDim.y));
        uvs.Add(new Vector2(tileCol / texDim.x, (tileRow + 1) / texDim.y));
        uvs.Add(new Vector2((tileCol + 1) / texDim.x, (tileRow + 1) / texDim.y));
    }

    // Computes the needed dimensions for the TileMap Collider
    private void AdjustCollider()
    {
        boxCollider.size = new Vector3(tileDim.x * mapDim.x, collHeight, yDim);
        boxCollider.center = new Vector3(boxCollider.size.x / 2, -collHeight / 2, yDim / 2);
    }
}
