using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Petal {

    public Vector3 Angle;
    private int SpineCount;
    private int Offset;
    private Root Root;
	
	public Petal(float len, float wid, float thick, Vector3 ang, float div)
    {
        Angle = ang;
        SpineCount = Mathf.RoundToInt(len / div);
        float ratio = Mathf.Sin(Mathf.PI * (.5f) / SpineCount);
        float segLength = len / (SpineCount + 1);

        Root = new Root(Vector3.zero, Vector3.zero, ratio * wid, ratio * thick, div);
        for (int i = 0; i < SpineCount; i++)
        {
            ratio = Mathf.Sin(Mathf.PI * (i + 1.0f) / SpineCount);
            if (i == SpineCount - 1)
            {
                Root.Append(new End(segLength, 0f, 0f, div));
            }
            else
            {
                Root.Append(new SpinePoint(new Vector3(segLength, 0f), new Vector3(Random.Range(-25f,25f),Random.Range(-25f,25f)),
                    ratio * wid, ratio * thick, div));
            }
        }
    }

    public void SetOffset(int off)
    {
        Offset = off;
    }

    public List<Vector3> Vertices()
    {
        return Util.Rot(Root.Vertices(), Angle);
    }

    public List<int> Triangles()
    {
        return Root.Triangles(Offset);
    }
}
