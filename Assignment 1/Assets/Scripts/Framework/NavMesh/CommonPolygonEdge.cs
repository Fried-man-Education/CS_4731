using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPolygonEdge : IEquatable<CommonPolygonEdge>
{
    public Vector2Int A { get; protected set; }
    public Vector2Int B { get; protected set; }

    public CommonPolygonEdge(Vector2Int A, Vector2Int B)
    {
        this.A = A;
        this.B = B;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as CommonPolygonEdge);
    }

    public bool Equals(CommonPolygonEdge ocpe)
    {
        // vertex order doesn't matter for matching a common edge
        return ocpe != null &&
               ((this.A == ocpe.A && this.B == ocpe.B) ||
                (this.A == ocpe.B && this.B == ocpe.A));      
    }


    public override string ToString()
    {
        return $"A:({this.A.x}, {this.A.y}), B:({this.B.x}, {this.B.y})";
    }

    public override int GetHashCode()
    {
        var hashCode = -1817952719;
        hashCode = hashCode * -1521134295 + EqualityComparer<float>.Default.GetHashCode(Vector2.Dot(CG.Convert(this.A), CG.Convert(this.B)));
        //hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Mathf.RoundToInt(Vector2.Dot(this.A, this.B)));
        //hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(A);
        //hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(B);
        return hashCode;
    }


}
