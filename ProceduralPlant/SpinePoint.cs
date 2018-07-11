using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinePoint
{
    public Vector3 pos;
    public Vector3 rot;
    public float wid;
    public float thi;
    public int bra;
    public int offset;
    public int count = 0;
    public SpinePoint child = null;

    public SpinePoint(float x, float y, float z, float xr, float yr, float zr, float width, float thick, float div)
    {
        pos = new Vector3(x, y, z);
        rot = new Vector3(xr, yr, zr);
        wid = width / 2f;
        thi = thick / 2f;
        bra = Mathf.RoundToInt(width / div);
    }

    public SpinePoint(Vector3 position, Vector3 rotation, float width, float thick, float div)
    {
        pos = position;
        rot = rotation;
        wid = width / 2f;
        thi = thick / 2f;
        bra = Mathf.RoundToInt(width / div);
    }

    public virtual List<Vector3> Vertices()
    {
        List<Vector3> verts = child.Vertices();
        verts = Util.Tran(verts, pos);

        verts.Add(pos + Vector3.up * thi);
        for (float i = 1; i < bra; i++)
        {
            verts.Add(pos + (Vector3.up * thi * (1f - (i / bra))) + (Vector3.forward * -wid * (i / bra)));
        }

        verts.Add(pos + Vector3.forward * -wid);
        for (float i = 1; i < bra; i++)
        {
            verts.Add(pos + (Vector3.up * -thi * (i / bra)) + (Vector3.forward * -wid * (1f - (i / bra))));
        }

        verts.Add(pos + Vector3.up * -thi);
        for (float i = 1; i < bra; i++)
        {
            verts.Add(pos + (Vector3.up * -thi * (1f - (i / bra))) + (Vector3.forward * wid * (i / bra)));
        }

        verts.Add(pos + Vector3.forward * wid);
        for (float i = 1; i < bra; i++)
        {
            verts.Add(pos + (Vector3.up * thi * (i / bra)) + (Vector3.forward * wid * (1f - (i / bra))));
        }

        verts = Util.Rot(verts, rot);

        count = Mathf.Max(0, (bra - 1)) * 4 + 4;

        return verts;
    }

    public void Append(SpinePoint sp)
    {
        if (child != null)
        {
            child.Append(sp);
        }
        else
        {
            child = sp;
        }
    }

    public virtual List<int> Triangles(int off)
    {
        List<int> tris = child.Triangles(off);
        offset = child.offset + child.count;
        int a = 0, b = 0;
        if (child.offset == off)
        {
            b++;
        }
        while (a < count || b < child.count)
        {
            if ((float)(a + 1f) / (float)count <= (float)(b + 1f) / (float)child.count)
            {
                tris.Add(offset + a % count);
                if ((a + count / 4)  % count < count / 2 || (b + child.count / 4) % child.count < child.count / 2) {
                    tris.Add(child.offset + b % child.count);
                    tris.Add(offset + (a + 1) % count);
                }
                else {
                    tris.Add(child.offset + b % child.count);
                    tris.Add(offset + (a + 1) % count);
                    
                }
                a++;
            }
            else
            {
                tris.Add(offset + a % count);
                if ((a + count / 4) % count < count / 2 || (b + child.count / 4) % child.count < child.count / 2)
                {
                    tris.Add(child.offset + b % child.count);
                    tris.Add(child.offset + (b + 1) % child.count);
                }
                else
                {
                    tris.Add(child.offset + b % child.count);
                    tris.Add(child.offset + (b + 1) % child.count);
                }
                b++;
            }
        }
        return tris;
    }
}

public class Root : SpinePoint
{
    public Root(float x, float y, float z, float xr, float yr, float zr, float width, float thick, float div) : base(x, y, z, xr, yr, zr, width, thick, div) { }

    public Root(Vector3 position, Vector3 rotation, float width, float thick, float div) : base(position, rotation, width, thick, div) { }

    public override List<Vector3> Vertices()
    {
        List<Vector3> verts = child.Vertices();
        verts = Util.Tran(verts, pos);

        verts.Add(pos + Vector3.up * thi);
        verts.Add(pos + Vector3.forward * -wid);
        verts.Add(pos + Vector3.up * -thi);
        verts.Add(pos + Vector3.forward * wid);

        verts = Util.Rot(verts, rot);

        count = 4;
        return verts;
    }
}

public class End : SpinePoint
{
    public End(float x, float y, float z, float div) : base(x, y, z, 0f, 0f, 0f, 0f, 0f, div) { }

    public End(Vector3 position, float div) : base(position, Vector3.zero, 0f, 0f, div) { }

    public override List<Vector3> Vertices()
    {
        List<Vector3> verts = new List<Vector3>();
        verts.Add(pos);
        count = 1;
        return verts;
    }

    public override List<int> Triangles(int off)
    {
        offset = off;
        return new List<int>();
    }
}
