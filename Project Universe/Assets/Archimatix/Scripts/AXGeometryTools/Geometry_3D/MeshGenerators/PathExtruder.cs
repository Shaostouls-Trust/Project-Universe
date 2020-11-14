using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    ///

    



    public class PathExtrudeMeshGenerator
    {





        public static PolyTree difference(Paths paths, Paths cutters)
        {
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.AddPaths(cutters, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return polyTree;
        }



        public static Mesh ExtrudePath(Path path, float _elev, float _base = 0)
        {

            Mesh m = new Mesh();

            Path plan = Pather.clone(path, Vector2.zero);

            if (Pather.Equals(plan.First(), plan.Last()))
            {
                plan.RemoveAt(plan.Count - 1);
                //plan.Add(new IntPoint(plan.First().X, plan.First().Y));
            }

           
            // VERTICES
            List<Vector3> verts = Pather.path2Vector3List(plan, _base);
            verts.AddRange(Pather.path2Vector3List(plan, _elev));

            m.vertices = verts.ToArray();

           
            // UVS
            m.uv = new Vector2[m.vertices.Length];


            // TRIANGLES
            int triCount = 2 * 3 * (plan.Count);
            int[] triangles = new int[triCount];

            //Debug.Log("triCount="+triCount + " ---- "+ m.vertices.Length);

            int triCt = 0;
            int sidesCt = plan.Count;

            int LL, LR, UL, UR;

            for (int i=1; i< plan.Count; i++)
            {
                LL = i-1;
                LR = i;
                UL = LL + sidesCt;
                UR = LR + sidesCt;

                triangles[triCt++] = LL;
                triangles[triCt++] = UR;
                triangles[triCt++] = LR;

                triangles[triCt++] = LL;
                triangles[triCt++] = UL;
                triangles[triCt++] = UR;
            }

            LL = plan.Count - 1;
            LR = 0;
            UL = LL + sidesCt;
            UR = LR + sidesCt;

            triangles[triCt++] = LL;
            triangles[triCt++] = UR;
            triangles[triCt++] = LR;

            triangles[triCt++] = LL;
            triangles[triCt++] = UL;
            triangles[triCt++] = UR;



            m.triangles = triangles;


            return m;


        }


        // GENERATE
        public static Mesh generateSidesMesh(
            Paths paths,
            Paths cutters,
            float _elev,
            float _base = 0
            )
        {
            

            CombineInstance[] combine = new CombineInstance[paths.Count];
            int c = 0;

            foreach (Path path in paths)
            {
                Mesh m = ExtrudePath(path, _elev, _base);
               
                combine[c].mesh = m;
                combine[c].transform = Matrix4x4.identity;
                c++;
            }

            Mesh mesh = new Mesh();
            
            mesh.CombineMeshes(combine);

           

            return mesh;
        }
    }
}
