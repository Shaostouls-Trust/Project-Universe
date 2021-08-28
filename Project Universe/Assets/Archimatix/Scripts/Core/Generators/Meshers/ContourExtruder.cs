using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;
 

namespace AX.Generators
{
	/// <summary>
	///  Lofter 
	///  creates a mesh between two paths.
	/// 
	/// 
	/// </summary>
	public class ContourExtruder : MesherExtruded, IMeshGenerator
	{
		public override string GeneratorHandlerTypeName { get { return "ContourExtruderHandler"; } }



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


            P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			P_Output.hasInputSocket = false;
			P_Output.shapeState = ShapeState.Open;



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
            
            c.AddPaths( contours, PolyType.ptSubject,true);
            c.AddPaths(innerPaths, PolyType.ptClip, true);

            PolyTree _polyTree = new AXClipperLib.PolyTree();
            c.PreserveCollinear = true;
            c.Execute(ClipType.ctDifference, _polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            Paths tmps = Clipper.PolyTreeToPaths(_polyTree);
            //Pather.printPaths(tmps);

            //PolyTree resPolytree = Pather.differenceToPolyTree(contour, innerPaths);

            Mesh mesh = AXPolygon.triangulate(_polyTree, new AXTexCoords(), 1000000) ;

            if (mesh != null)
            {
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

            }

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

            Paths           innerPaths = null;
            List<Vector3>   innerVerts = null;
            Vector2 ePt = Vector2.zero;    

            // CREATE MESH STRIPS & BUILD UP MAIN_MESH
            bool shouldGenerateFlatTop = true;


            PolyTree innerPolytree = null;
           // Debug.Log("=========================");

            for (int i=1; i< secPath.Count; i++)// secPath.Count; i++)
            {

                // skip section pts until the offset is greater than some value.
                
                float cumOffset = 0;


                while (i < secPath.Count)
                {
                    ePt = Pather.IP2Vector2WithPrecision(secPath[i]);

                    float absOffset = Mathf.Abs(ePt.x - bPt.x);
                    cumOffset += absOffset;

                    //Debug.Log("Path " + i + "   ====" + cumOffset + ", [" + ePt.x + ", "+ ePt.y +"]================================");


                    if (Mathf.Approximately(absOffset, 0) || cumOffset > .005f)
                        break;


                    //Debug.Log("skipping i="+i);
                    i++;
                }

               

                innerPolytree = null;
                
                if (Mathf.Approximately(ePt.x, 0))
                {
                    innerPaths = Pather.clone(prevContours, Vector2.zero);
                }
                else
                {
                    innerPolytree = Pather.offsetToPolyTree(prevContours, ePt.x-bPt.x, jt);
                    innerPaths = Clipper.ClosedPathsFromPolyTree(innerPolytree);


                    if (innerPaths != null)
                    {
                       // Debug.Log("seglen" + seglen);
                        //if (subdivisions > 1)
                        //    seglen = Pather.getSegLenBasedOnSubdivision(innerPaths, subdivisions);
                        //else if (plan_p.subdivision > 0)
                        //    seglen = Pather.getSegLenBasedOnSubdivision(innerPaths, (int)plan_p.subdivision);
                        //Debug.Log("A innerPaths.Count="+ innerPaths[0].Count);
                        //innerPaths = Pather.segmentPaths(innerPaths, seglen, true);
                        //Debug.Log("B innerPaths.Count=" + innerPaths[0].Count);
                    }
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
                     //prevContours = Pather.segmentPaths(prevContours, Pather.Float2IntPoint(.01f), true);
                     meshStrip = GenerateCountourExtrudeMesh(i, bPt.y, ePt.y, prevContours);

                    //Debug.Log("ExtrudeMesh  ====" + bPt.y + ", " + ePt.y + " :: " + meshStrip.vertices.Length + " : " + meshStrip.triangles.Length);

                    //Debug.Log("EXTRUDE .... " + meshStrip.vertices.Length);
                    //foreach (Vector3 v in meshStrip.vertices)
                    //{
                    //    Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                    //}

                    //if (i == 1)
                        //for (int ind = 5; ind < meshStrip.triangles.Length; ind = ind + 6)
                        //    Debug.Log(meshStrip.triangles[ind - 5] + "-" + meshStrip.triangles[ind - 4] + "-" + meshStrip.triangles[ind - 3] + " -- " + meshStrip.triangles[ind - 2] + "-" + meshStrip.triangles[ind - 1] + "-" + meshStrip.triangles[ind]);

                }
                else
                {
                    //prevContours = Pather.segmentPaths(prevContours, Pather.Float2IntPoint(.01f), true);
                   // innerPaths = Pather.segmentPaths(innerPaths, Pather.Float2IntPoint(.01f), true);
                    meshStrip = GenerateCountourBeltMesh(i, bPt.y, ePt.y, prevContours, innerPaths, innerPolytree);
                    //Debug.Log("ContourMesh.... "+ meshStrip.vertices.Length);

                    //if (meshStrip.vertices.Length == 0)
                    //{
                    //    Pather.printPaths(prevContours);
                    //    Pather.printPaths(innerPaths);
                    //}
                    //foreach (Vector3 v in meshStrip.vertices)
                    //{
                    //    Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                    //}
                }

                if (meshStrip == null)
                    return null;
               
                //if (meshStrip.vertices.Length == 0)
                //{
                //    continue;
                //}

                //Debug.Log("meshStrip.vertices.Length="+meshStrip.vertices.Length);
                int[] indexMap = new int[meshStrip.vertices.Length];




                // ------ MAIN_VERTICES
                mainVertices.AddRange(innerVerts);

                //for(int k=0; k<mainVertices.Count; k++)
                //{
                //    Vector3 v = mainVertices[k];
                //    Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                //}

                // ------ INDEX MAPPING ---
                // CONSTRUCT TRI INDEX MAPPING: FIND INDEX OF EACH INNER_VERT IN  NEWVERTS AND MAP INTO TRIS
                Vector3[] mverts = meshStrip.vertices;
                for (int j = 0; j < mverts.Length; j++)
                {
                    int verticesIndex;

                    if (Mathf.Approximately(mverts[j].y, prevVerts[0].y))
                    {
                        verticesIndex = GetIndexOfVertInList(prevVerts, mverts[j]);

                        if (verticesIndex > -1)
                        {   // this mesh vert was fsuccessfully found on the bottom of the mesh strip
                            int mainIndex = cursor - prevVerts.Count + verticesIndex;
                            indexMap[j] = mainIndex;
                        }
                    }
                    else
                    {
                        verticesIndex = GetIndexOfVertInList(innerVerts, mverts[j]);
                        if (verticesIndex > -1)
                        {   // this mesh vert is on the top of the strip
                            int mainIndex = cursor + verticesIndex;
                            indexMap[j] = mainIndex;
                        }
                    }
                   

                       
                    //if (verticesIndex == -1)
                    //{
                    //    Debug.Log("not found ");
                    //    Vector3 v = mverts[j];
                    //    Debug.Log("[" + v.x + ", " + v.z + "], [" + v.y + "]");
                    //    Debug.Log("----------------------");
                    //    foreach (Vector3 p in prevVerts)
                    //    {
                    //        Debug.Log("[" + p.x + ", " + p.z + "], [" + p.y + "] :: " + ((p - v).sqrMagnitude));
                    //    }
                    //    foreach (Vector3 p in innerVerts)
                    //    {
                    //        Debug.Log("[" + p.x + ", " + p.z + "], [" + p.y + "] :: " + ((p - v).sqrMagnitude));
                    //    }
                    //    Debug.Log("=========================");
                    //}
                            
                     
                }


                



                // ------ TRIANGLES -----
                // USE MAP TO ADD TRIANGLES
                for (int j = 0; j < meshStrip.triangles.Length; j++)
                    mainTriangles.Add(indexMap[meshStrip.triangles[j]]);

               

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


            //int faceCur = 0;
            //        for (int ind = 5; ind < mainTriangles.Count; ind=ind+6)
            //            Debug.Log((faceCur++) + ") "+ mainTriangles[ind-5] + "-" + mainTriangles[ind-4] + "-" + mainTriangles[ind-3] + " -- " + mainTriangles[ind - 2] + "-" + mainTriangles[ind - 1] + "-" + mainTriangles[ind]);

            // LAST FLAT CONTOUR
            if (shouldGenerateFlatTop)
            {

               // Debug.Log("%%%%%%%%%%%%%%%%%%%%");
                Mesh flatMesh = AXPolygon.triangulate(innerPolytree, new AXTexCoords(),1000000, true, seglen);

                // add to main
                // Use innerVerts already in Vertices
                int cursor = mainVertices.Count;
                Vector3[] mverts = flatMesh.vertices;

                int[] indexMap = new int[mverts.Length];
                for (int j = 0; j < mverts.Length; j++)
                {
                    mverts[j].y = ePt.y;

                    int verticesIndex = GetIndexOfVertInList(innerVerts, mverts[j]);

                    if (verticesIndex > -1)
                    {   // this mesh vert is on the bottom of the strip
                        int mainIndex = cursor - prevVerts.Count + verticesIndex;
                        indexMap[j] = mainIndex;
                    }
                    else
                    {
                        // Steiner point, ADD manually
                        mainVertices.Add(mverts[j]);
                        indexMap[j] = mainVertices.Count-1;

                    }
                }
                // ------ TRIANGLES -----
                // USE MAP TO ADD TRIANGLES
                for (int j = 0; j < flatMesh.triangles.Length; j++)
                    mainTriangles.Add(indexMap[flatMesh.triangles[j]]);
            }

             




            Mesh mainMesh = new Mesh();

            mainMesh.vertices = mainVertices.ToArray();
            mainMesh.triangles = mainTriangles.ToArray();
            mainMesh.uv = new Vector2[mainMesh.vertices.Length];

            mainMesh.RecalculateBounds();


            // ------ UVs  ------

            Vector2[] uvs = new Vector2[mainMesh.vertices.Length];

            float meshWidth = mainMesh.bounds.size.x;
            float meshHeight = mainMesh.bounds.size.z;


            float xmin = mainMesh.bounds.center.x - mainMesh.bounds.size.x / 2; //b.min.x;
            float xmax = xmin + mainMesh.bounds.size.x;

            float zmin = mainMesh.bounds.center.z - mainMesh.bounds.size.z / 2; //b.min.x;
            float zmax = zmin + mainMesh.bounds.size.z;

    
            for (int i=0; i< mainMesh.vertices.Length; i++)
            {

                float u = uShift + (mainMesh.vertices[i].x - xmin) / (uScale * mainMesh.bounds.size.x);
                //u = ((mainMesh.vertices[i].x - xmin) - (meshWidth / 2 * (1 - uScale))) / (meshWidth * uScale);
                //u -= uShift;


                float v = vShift + (mainMesh.vertices[i].z - zmin) / (vScale * mainMesh.bounds.size.z);
                //v = ((mainMesh.vertices[i].z - ymin) - (meshHeight * scaleCenY * (1 - vScale))) / (meshHeight * vScale);
                //v -= vShift;

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
				return parametricObject.makeGameObjectsFromAXMeshes (ax_meshes);

			



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
