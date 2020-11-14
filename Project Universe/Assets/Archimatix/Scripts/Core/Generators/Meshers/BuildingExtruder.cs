using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


using Curve = System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;

using AXGeometry;


namespace AX.Generators
{
    /// <summary>
    ///  Lofter 
    ///  creates a mesh between two paths.
    /// 
    /// 
    /// </summary>
    public class BuildingExtruder : MesherExtruded, IMeshGenerator, ICustomNode
    {
        public override string GeneratorHandlerTypeName { get { return "BuildingExtruderHandler"; } }



        public AXParameter P_Section;
        public AXParameter sectionSrc_p;
        public AXParametricObject sectionSrc_po;



        public Paths contourPaths;
        public List<Spline> countourSplines;

        public static int cleanPolygonPrecision = 2;


        // INIT_PARAMETRIC_OBJECT
        public override void init_parametricObject()
        {
            base.init_parametricObject();

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Section"));

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));


            parametricObject.curve = new Curve(); 

            parametricObject.curve.Add(new CurveControlPoint2D(new Vector2(10, 0)));
            parametricObject.curve.Add(new CurveControlPoint2D(new Vector2(20, 5)));
            parametricObject.curve.Add(new CurveControlPoint2D(new Vector2(25, 5)));
            parametricObject.curve.Add(new CurveControlPoint2D(new Vector2(30, 0)));
        }


        // POLL INPUTS (only on graph change())
        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();

            P_Plan = parametricObject.getParameter("Plan");
            P_Section = parametricObject.getParameter("Section");



        }

        // POLL CONTROLS (every model.generate())
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

        }



        public override AXParameter getPreferredInputParameter()
        {
            return P_Section;
        }


        public override void connectionBrokenWith(AXParameter p)
        {
            base.connectionBrokenWith(p);


            switch (p.Name)
            {

                case "Plan":
                    planSrc_po = null;
                    P_Output.meshes = null;
                    break;


                case "Section":
                    sectionSrc_po = null;
                    P_Output.meshes = null;
                    break;

            }



        }


        public Mesh GenerateCountourExtrudeMesh(int index, float b, float e, Paths contours)
        {
            Mesh mesh = PathExtrudeMeshGenerator.generateSidesMesh(contours, null, e, b);
            return mesh;
        }

        public Mesh GenerateCountourBeltMesh(int index, float b, float e, Paths contours, Paths innerPaths, PolyTree polyTree)
        {

            // Debug.Log("GenerateCountourBeltMesh: b="+b+", e="+b);
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            // Debug.Log("contourPaths[0].Count" + contourPaths[0].Count);

            c.AddPaths(contours, PolyType.ptSubject, true);
            c.AddPaths(innerPaths, PolyType.ptClip, true);

            PolyTree _polyTree = new AXClipperLib.PolyTree();
            c.PreserveCollinear = true;
            c.Execute(ClipType.ctDifference, _polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            Paths tmps = Clipper.PolyTreeToPaths(_polyTree);
            //Pather.printPaths(tmps);

            //PolyTree resPolytree = Pather.differenceToPolyTree(contour, innerPaths);

            Mesh mesh = AXPolygon.triangulate(_polyTree, new AXTexCoords(), 1000000);

            Vector3[] verts = mesh.vertices;
            //Debug.Log("verts.Length="+ verts.Length);
            // Move any verts similar to ones on innerPath up.

            // Just a search PAth, so combine any Paths resulting from the offset:
            Path combinedInnerPath = new Path();
            foreach (Path p in innerPaths)
                combinedInnerPath.AddRange(p);

            Vector2[] innerVerts = Pather.path2Vector2Array(combinedInnerPath);

            //Debug.Log("verts.Length=" + verts.Length);
            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 v2 = new Vector2(verts[i].x, verts[i].z);

                //Debug.Log(i + ") " + i + " v2=" + v2.x + ", " + v2.y);
                if (innerVerts != null && innerVerts.Length > 0)
                {
                    foreach (Vector2 iv2 in innerVerts)
                    {
                        if ((v2 - iv2).sqrMagnitude < .0000001f)
                        {
                            verts[i].y = e;
                            break;
                        }
                        else
                            verts[i].y = b;
                    }
                }
                else
                {
                    verts[i].y = b;
                }

            }



            mesh.vertices = verts;

            return mesh;
        }


        public List<Vector3> MakeVertsAtElevY(Paths paths, float y)
        {
            List<Vector3> retVerts = new List<Vector3>();

            foreach (Path path in paths)
            {
                foreach (IntPoint ip in path)
                {
                    retVerts.Add(new Vector3(Pather.IntPoint2Float(ip.X), y, Pather.IntPoint2Float(ip.Y)));
                }
            }

            return retVerts;
        }


        public int GetIndexOfVertInList(List<Vector3> verts, Vector3 v)
        {

            for (int i = 0; i < verts.Count; i++)
            {
                if ((verts[i] - v).sqrMagnitude < .0000001f)
                {

                    return i;
                }
            }


            return -1;


        }

        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
        {
            if (parametricObject == null || !parametricObject.isActive)
                return null;



            P_Output.meshes = null;
            P_Output.mesh = null;


            //Debug.Log("PlanSweep::generate()");

            // RESULTING MESHES
            ax_meshes = new List<AXMesh>();


            preGenerate();


            //return null;

            // PLAN
            // The plan may have multiple paths. Each may generate a separate GO.

            if (P_Plan == null)
                return null;

            if (P_Section == null)
                return null;

            if (planSrc_p == null || !planSrc_p.parametricObject.isActive)
                return null;




            sectionSrc_p = P_Section.DependsOn;

            if (sectionSrc_p == null)
                return null;

            if ((sectionSrc_p.paths == null || sectionSrc_p.paths.Count == 0) && sectionSrc_p.polyTree == null)
                return null;




            float uShift = po_axTex.shift.x;
            float vShift = po_axTex.shift.y;
            float uScale = po_axTex.scale.x;
            float vScale = po_axTex.scale.y;



            //Paths planPaths = null;

            //if (P_Plan.polyTree != null)
            //{
            //    Debug.Log("P_Plan.polyTree=" + P_Plan.polyTree);
            //    planPaths = Clipper.PolyTreeToPaths(P_Plan.polyTree);
            //}

            //else
            //{
            //    Debug.Log("P_Plan.paths="+ P_Plan.paths);
            //    planPaths = P_Plan.paths;
            //}

            P_Plan.polyTree = null;
            AXShape.thickenAndOffset(ref P_Plan, planSrc_p);


            Paths planPaths = P_Plan.getClonePaths();

            if (planPaths == null || planPaths.Count == 0)
                return null;
            // Debug.Log();

            P_Section.polyTree = null;
            AXShape.thickenAndOffset(ref P_Section, sectionSrc_p);
            Paths secPaths = P_Section.getPaths();


            if (secPaths == null || secPaths.Count == 0)
                return null;


            // FOR NOW JUST USE FIRT PATH
            Path secPath = secPaths[0];

            if (secPath == null || secPath.Count == 0)
                return null;


            JoinType jt = AXClipperLib.JoinType.jtMiter;

            contourPaths = new Paths();


            AXParameter plan_p = P_Plan;

            // GET SEGLEN FROM SUBDIVISION
            int seglen = 9999999;


            if (planPaths != null)
            {
                if (subdivisions > 1)
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, subdivisions);
                else if (plan_p.subdivision > 0)
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, (int)plan_p.subdivision);

                //Debug.Log("+++++++ seglen" + seglen);
                planPaths = Pather.segmentPaths(planPaths, seglen, true);
            }



            Vector2 bPt = Vector2.zero;
            Paths prevContours = planPaths;
            contourPaths.AddRange(prevContours);
            List<Vector3> prevVerts = MakeVertsAtElevY(prevContours, 0);



            // MAIN VERTICES AND TRIANGLES
            List<Vector3> mainVertices = new List<Vector3>();
            mainVertices.AddRange(prevVerts);

            List<int> mainTriangles = new List<int>();

            Paths innerPaths = null;
            List<Vector3> innerVerts = null;
            Vector2 ePt = Vector2.zero;

            // CREATE MESH STRIPS & BUILD UP MAIN_MESH
            bool shouldGenerateFlatTop = true;


            PolyTree innerPolytree = null;
            // Debug.Log("=========================");

            for (int i = 1; i < secPath.Count; i++)// secPath.Count; i++)
            {

                // skip section pts until the offset is greater than some value.

                float cumOffset = 0;


                innerPolytree = null;

                if (Mathf.Approximately(ePt.x, 0))
                {
                    innerPaths = Pather.clone(prevContours, Vector2.zero);
                }
                else
                {
                    innerPolytree = Pather.offsetToPolyTree(prevContours, ePt.x - bPt.x, jt);
                    innerPaths = Clipper.ClosedPathsFromPolyTree(innerPolytree);

                }


                // Debug.Log("innerPaths="+ innerPaths.Count);
                if (innerPaths != null && innerPaths.Count > 0)
                    contourPaths.AddRange(innerPaths);



                int cursor = mainVertices.Count;
                innerVerts = MakeVertsAtElevY(innerPaths, ePt.y);



                //  ----- MESH STRIP ------
                Mesh meshStrip = null;


                //Debug.Log("i=" + i);
                //foreach (Vector3 v in prevVerts)
                //{
                //    Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                //}

                //if (i == 3)
                //{
                //    foreach (Vector3 v in innerVerts)
                //    {
                //        Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                //    }
                //    Debug.Log(bPt.x + ", " + bPt.y + " -- " + ePt.x + ", " + ePt.y);

                //}

                if (Mathf.Approximately(cumOffset, 0))
                {
                    
                }

                



                if (innerPaths == null || innerPaths.Count == 0 || innerPaths[0].Count == 0)
                {
                    shouldGenerateFlatTop = false;

                    // break loop;
                    i = secPath.Count;
                }

                cumOffset = 0;
                prevContours = innerPaths;
                prevVerts = innerVerts;

                bPt = ePt;
            }





            Mesh mainMesh = new Mesh();

            mainMesh.vertices = mainVertices.ToArray();
            mainMesh.triangles = mainTriangles.ToArray();
            mainMesh.uv = new Vector2[mainMesh.vertices.Length];

            mainMesh.RecalculateBounds();


            // ------ UVs  ------

            Vector2[] uvs = new Vector2[mainMesh.vertices.Length];


            float xmin = mainMesh.bounds.center.x - mainMesh.bounds.size.x / 2; //b.min.x;
            float xmax = xmin + mainMesh.bounds.size.x;

            float zmin = mainMesh.bounds.center.z - mainMesh.bounds.size.z / 2; //b.min.x;
            float zmax = zmin + mainMesh.bounds.size.z;



            for (int i = 0; i < mainMesh.vertices.Length; i++)
            {

                float u = uShift + (mainMesh.vertices[i].x - xmin) / (uScale * mainMesh.bounds.size.x);
                float v = vShift + (mainMesh.vertices[i].z - xmin) / (vScale * mainMesh.bounds.size.z);

                uvs[i] = new Vector2(u, v);
            }


            mainMesh.uv = uvs;






            mainMesh.RecalculateBounds();
            mainMesh.RecalculateNormals();
            mainMesh.RecalculateTangents();

            // Debug.Log("mainMesh.vertices: " + mainMesh.vertices.Length + " :: " + mainMesh.triangles.Length);
            //for(int i=0; i<mainMesh.triangles.Length; i++)
            //{
            //    Debug.Log(i + ": " + mainMesh.triangles[i]);
            //}
            AXMesh axmesh = new AXMesh(mainMesh);
            axmesh.makerPO = parametricObject;
            ax_meshes.Add(axmesh);







            // FINISH AX_MESHES

            parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);//renderToOutputParameter);

            // FINISH BOUNDING

            setBoundaryFromAXMeshes(ax_meshes);


            if (makeGameObjects)
                return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);





            return null;
        }








        // GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
        public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
        {
            //Debug.Log ("input_po.Name=" + input_po.Name + " :: planSrc_po.Name=" + planSrc_po.Name);// + " -- " + endCapHandleTransform);
            // PLAN
            if (input_po == planSrc_po)
            {
                if (P_Plan.flipX)
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1, 1, 1));
                else
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);


            }
            // SECTION
            if (input_po == sectionSrc_po)
            {
                //Debug.Log("??? " + sectionHandleTransform);
                if (P_Section.flipX)
                {
                    return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1, 1, 1));
                }
                else
                {
                    if (P_Plan.shapeState == ShapeState.Open)
                        return endCapHandleTransform;
                    else
                        return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
                }
            }
            // ENDCAP
            if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.Parent)
                return endCapHandleTransform;

            return Matrix4x4.identity;
        }
        //
    }
}
