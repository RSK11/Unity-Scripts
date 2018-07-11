using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util {
    private static readonly Vector3 OneScale = new Vector3(1, 1, 1);

	public static List<Vector3> TR(List<Vector3> verts, Vector3 tran, Vector3 rot)
    {
        Matrix4x4 mat = Matrix4x4.TRS(tran, Quaternion.Euler(rot), OneScale);
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = mat.MultiplyPoint3x4(verts[i]);
        }
        return verts;
    }

    public static List<Vector3> Tran(List<Vector3> verts, Vector3 tran)
    {
        Matrix4x4 mat = Matrix4x4.Translate(tran);
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = mat.MultiplyPoint3x4(verts[i]);
        }
        return verts;
    }

    public static List<Vector3> Rot(List<Vector3> verts, Vector3 rot)
    {
        Matrix4x4 mat = Matrix4x4.Rotate(Quaternion.Euler(rot));
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = mat.MultiplyPoint3x4(verts[i]);
        }
        return verts;
    }
}
