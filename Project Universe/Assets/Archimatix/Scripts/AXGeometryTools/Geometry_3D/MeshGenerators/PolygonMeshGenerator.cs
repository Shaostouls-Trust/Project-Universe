using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;

namespace AX
{
    /// <summary>
    /// 
    /// 
    /// Mesh a Polygon
    /// 
    /// 
    /// </summary>

    public class PolygonMeshGenerator
    {

        // GENERATE
        public static Mesh generateMesh(
            Curve2D planCurve2,
            bool addEdgeWear = false
            )
        {
            Mesh mesh = new Mesh();




            return mesh;
        }
    }
}
