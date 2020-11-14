using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;

using CurvePoint2 = AXGeometry.CurvePoint2;
using CurvePoint3 = AXGeometry.CurvePoint3;

namespace AX.Generators
{

    // CURVE SWEEPER GENERATOR
    public class CurveSweeper3 : MesherExtruded, IMeshGenerator, ICustomNode
    {
        public override string GeneratorHandlerTypeName { get { return "CurveSweeperHandler"; } }

        // INPUTS



        public AXParameter P_Curve;
        public AXParameter path3DSrc_p;
        public AXParametricObject path3DSrc_po;

        public AXParameter P_Section;
        public AXParameter sectionSrc_p;
        public AXParametricObject sectionSrc_po;





        // INIT
        public override void init_parametricObject()
        {

            P_Curve = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Curve3D, AXParameter.ParameterType.Input, "Curve3D"));

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Section"));

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "End Cap Mesh"));

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Top Cap Material"));
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "End Cap Material"));



            parametricObject.addParameter(AXParameter.DataType.Bool, "Top Cap", true);
            parametricObject.addParameter(AXParameter.DataType.Bool, "Bottom Cap", true);

            parametricObject.addParameter(AXParameter.DataType.Float, "Lip Top", 0f, 0f, 1000f);
            parametricObject.addParameter(AXParameter.DataType.Float, "Lip Bottom", 0f, 0f, 1000f);
            parametricObject.addParameter(AXParameter.DataType.Float, "Lip Edge", 0f, 0f, 1000f);

            parametricObject.addParameter(AXParameter.DataType.Bool, "End Cap A", false);
            parametricObject.addParameter(AXParameter.DataType.Bool, "End Cap B", false);

            base.init_parametricObject();

            // HANDLES
            parametricObject.addHandle("depth", AXHandle.HandleType.Point, "0", "depth", "0", "depth=han_y");

            // Code
        }

        // POLL INPUTS (only on graph change())
        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();

            P_Curve = parametricObject.getParameter("Curve3D");
            P_Section = parametricObject.getParameter("Section");

            if (P_Section != null)
                sectionSrc_p = P_Section.DependsOn;

            if (sectionSrc_p != null)
                sectionSrc_po = sectionSrc_p.parametricObject;


        }


        // POLL CONTROLS (every model.generate())
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

            path3DSrc_p = P_Curve.DependsOn;

            if (path3DSrc_p != null)
            {
                path3DSrc_po = path3DSrc_p.parametricObject;
            }
            //reduceValuesForDetailLevel();

        }


        public override void connectionBrokenWith(AXParameter p)
        {
            base.connectionBrokenWith(p);


            switch (p.Name)
            {

                case "Curve3D":
                    planSrc_po = null;
                    P_Output.meshes = null;
                    break;


                case "Section":
                    sectionSrc_po = null;
                    P_Output.meshes = null;
                    break;

            }



        }






        // GENERATE CURVE_SWEEP
        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
        {
            if (parametricObject == null || !parametricObject.isActive)
                return null;

            if (path3DSrc_po == null)
                return null;

            if (path3DSrc_po.curve3D == null || path3DSrc_po.curve3D.controlPoints.Count < 2)
                return null;

            if (P_Section == null)
                return null;

            Paths paths = null;
            Path path = null;

            Curve2D sectionCurve2D = null;

            if (sectionSrc_p != null)
            {
                paths = sectionSrc_p.getPaths();
                path = paths[0];

                sectionCurve2D = new Curve2D(path, (P_Section.shapeState == ShapeState.Closed));
            }






            //UnityEngine.Debug.Log("PlanSweep::generate()");

            // RESULTING MESHES
            ax_meshes = new List<AXMesh>();


            preGenerate();

            setSectionHandleTransform();


            Stopwatch TIMER = new Stopwatch();
            bool d = false;
            TIMER.Start();


			// ** GENERATE THE MESH **
            Mesh mesh = new Mesh();
            Extrude(mesh, path3DSrc_po.curve3D, sectionCurve2D);



            if (d) UnityEngine.Debug.Log("Extrude " + TIMER.ElapsedTicks);

            TIMER.Reset();  TIMER.Start();

            //Debug.Log(mesh.vertices.Length);
            //float[] verts
            mesh.RecalculateNormals();

            if (d) UnityEngine.Debug.Log("recalc " + TIMER.ElapsedTicks);
            TIMER.Reset(); TIMER.Start();

            AXMesh tmpAXMesh = new AXMesh(mesh);
            tmpAXMesh.makerPO = parametricObject;

            ax_meshes.Add(tmpAXMesh);

            if (d) UnityEngine.Debug.Log("add mesh " + TIMER.ElapsedTicks);
            TIMER.Reset(); TIMER.Start();



            // FINISH AX_MESHES

            parametricObject.finishMultiAXMeshAndOutput(ax_meshes, true);//renderToOutputParameter);

            if (d) UnityEngine.Debug.Log("fin and output " + TIMER.ElapsedTicks);
            TIMER.Reset(); TIMER.Start();

            if (makeGameObjects)
            {
                GameObject go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);
                if (d) UnityEngine.Debug.Log("******** MAKE GO  " + TIMER.ElapsedTicks);
                return go;
            }
             

            return null;

        }

        public void Extrude(Mesh mesh, Curve3D curve3D, Curve2D sectionCurve2D )
        {
            //Debug.Log("path3D="+ curve3D.controlPoints.Count);


            if (curve3D == null || sectionCurve2D == null)
                return;

            List<CurvePoint3> path3D = curve3D.derivedPathPoints;
            List<CurvePoint2> sectionPath = sectionCurve2D.derivedCurvePoints;

            int sectionVerts = sectionCurve2D.derivedCurvePoints.Count;
            int segments = path3D.Count-1;
            int edgeloops = path3D.Count;
            int vertCount = sectionVerts * edgeloops;
            int triCount = sectionCurve2D.lines.Count * segments;
            int triIndexCount = triCount * 3;

            Vector3[] vertices = new Vector3[vertCount];
            Vector3[] normals  = new Vector3[vertCount];
            Vector2[] uv = new Vector2[vertCount];

            int[] triangles = new int[triIndexCount];



            // generat mesh

			// Create vertices
            for (int i=0; i< path3D.Count; i++)
            {
                int offset = i * sectionVerts;
                for (int j = 0; j < sectionVerts; j++)
                {
                    int id = offset + j;
                    vertices[id] = path3D[i].LocalToWorld(sectionPath[j].position);
                    normals[id] = Vector3.one;
                    uv[id] = new Vector2(1*path3D[i].u, 1*sectionPath[j].u);
                }
            }


            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int offset = i * sectionVerts;
                List<int> lines = sectionCurve2D.lines;
                for (int l = 0; l < lines.Count; l += 2)
                {
                    int a = offset + lines[l] + sectionVerts;
                    int b = offset + lines[l];
                    int c = offset + lines[l+1] + sectionVerts;
                    int d = offset + lines[l+1];

                    triangles[ti] = a; ti++;
                    triangles[ti] = b; ti++;
                    triangles[ti] = d; ti++;

                    triangles[ti] = d; ti++;
                    triangles[ti] = c; ti++;
                    triangles[ti] = a; ti++;

                }
            }



            // finish
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }
         
         
        public void setSectionHandleTransform()
        {
            // BEG SECTION HANDLE TRANSFORM ===================================================================

            // Find middle point of path3D and use that

            if (path3DSrc_po.curve3D == null || path3DSrc_po.curve3D.controlPoints.Count < 2)
            {
                sectionHandleTransform = Matrix4x4.identity;
                return;
            }

            CurvePoint3 pp3 = path3DSrc_po.curve3D.derivedPathPoints[1];

            sectionHandleTransform = Matrix4x4.TRS(pp3.position, pp3.rotation * Quaternion.Euler(-90, 0, 0), Vector3.one);


        }
         

        // GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
        public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
        {
            //Debug.Log ("input_po.Name=" + input_po.Name + " :: planSrc_po.Name=" + planSrc_po.Name);// + " -- " + endCapHandleTransform);
           

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
                    //if (P_Plan.shapeState == ShapeState.Open)
                    //    return endCapHandleTransform;
                    //else
                        return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
                }
            }
            // ENDCAP
            //if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.Parent)
                //return endCapHandleTransform;

            return Matrix4x4.identity;
        }
    }

}