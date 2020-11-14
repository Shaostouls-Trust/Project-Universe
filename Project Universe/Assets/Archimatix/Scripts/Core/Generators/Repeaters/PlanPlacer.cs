using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;


namespace AX.Generators
{




    // LINEAR_REPEATER

    public class PlanPlacer : RepeaterBase, IRepeater
    {
        public override string GeneratorHandlerTypeName { get { return "PlanPlacerHandler"; } }


        public AXParameter P_Plan;
        public AXParameter planSrc_p;
        public AXParametricObject planSrc_po;



        public AXParameter P_Distance;
        float distance = 0;

        public AXParameter P_AsPercentage;
        bool asPercentage;

        public Paths planPaths;


        List<AXMesh> ax_meshes;

        Curve2D planCurve;
        Matrix4x4 localPlacement_mx;


        public override void init_parametricObject()
        {
            base.init_parametricObject();

            P_Plan = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
            P_Node = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Mesh"));



            P_Distance = parametricObject.addParameter(AXParameter.DataType.Float, "Distance", 0, 0f, 100f);

            P_AsPercentage = parametricObject.addParameter(AXParameter.DataType.Bool, "As Percentage", false);

            P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));

        }


        // POLL INPUTS (only on graph change())
        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();

            P_Plan = parametricObject.getParameter("Plan");
            P_Node = parametricObject.getParameter("Mesh");
            P_Distance = parametricObject.getParameter("Distance");

            P_AsPercentage = parametricObject.getParameter("As Percentage");

        }


        public override void connectionBrokenWith(AXParameter p)
        {
            base.connectionBrokenWith(p);

            if (P_Plan != null)
            {
                P_Plan.polyTree = null;
                P_Plan.paths = null;
            }

            switch (p.Name)
            {

                case "Mesh":
                    nodeSrc_po = null;
                    P_Output.meshes = null;
                    break;

                case "Plan":
                    planSrc_po = null;
                    P_Output.meshes = null;
                    break;



            }

        }




        // POLL CONTROLS (every model.generate())
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

            planSrc_p = (P_Plan != null) ? getUpstreamSourceParameter(P_Plan) : null;
            planSrc_po = (planSrc_p != null) ? planSrc_p.parametricObject : null;


            nodeSrc_p = (P_Node != null) ? getUpstreamSourceParameter(P_Node) : null;
            nodeSrc_po = (nodeSrc_p != null) ? nodeSrc_p.parametricObject : null;

            distance = (P_Distance != null) ? P_Distance.FloatVal : 0;
            asPercentage = (P_AsPercentage != null) ? P_AsPercentage.boolval : false;

        }



        // GENERATE PLAN_REPEATER
        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
        {
            if (parametricObject == null || !parametricObject.isActive)
                return null;
           // Debug.Log("yo ******* makeGameObjects="+makeGameObjects);

            preGenerate();


            // RESULTING MESHES
            ax_meshes = new List<AXMesh>();



            planSrc_p = P_Plan.DependsOn;// getUpstreamSourceParameter(P_Plan);
            planSrc_po = (planSrc_p != null) ? planSrc_p.parametricObject : null;
            

            P_Plan.polyTree = null;
            AXShape.thickenAndOffset(ref P_Plan, planSrc_p);
            if (P_Plan.reverse)
                P_Plan.doReverse();

            planPaths = P_Plan.getPaths();

            if (planPaths == null || planPaths.Count == 0)
                return null;
           

            // ** CREATE PLAN_SPLINES **

            if (planPaths != null && planPaths.Count > 0)
            {
                Path planPath = planPaths[0];


                //Pather.printPath(planPath);

                //Debug.Log((P_Plan.shapeState == ShapeState.Closed));
                planCurve = new Curve2D(planPath, (P_Plan.shapeState == ShapeState.Closed));



                //Debug.Log(planCurve.uDistance);

                //planCurve.printSelf();

                float dist;

                
                if (asPercentage)
                {
                    dist = distance * planCurve.uDistances.Last();
                 
                }
                else
                {
                    dist = distance;
                   
                }
                dist = Mathf.Clamp(dist, 0, planCurve.uDistances.Last());

                localPlacement_mx = planCurve.getTransformationMatrixForLengthOnCurve(dist);

               



                GameObject go = null;
       
               // if (makeGameObjects && !parametricObject.combineMeshes)
                if (makeGameObjects )
                     go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);

                // BOUNDING

                List<AXMesh> boundingMeshes = new List<AXMesh>();



                // NODE_MESH


                GameObject nodePlugGO = null;
                if (base.nodeSrc_p != null)
                {
                    base.nodeSrc_po = base.nodeSrc_p.parametricObject;

                   
                    //Debug.Log("yo makeGameObjects="+makeGameObjects+", parametricObject.combineMeshes="+parametricObject.combineMeshes);


                    Matrix4x4 mx = localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;

                    // AX_MESHES
                    AXMesh tmpMesh = null;

                    for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++)
                    {
                        AXMesh dep_amesh = nodeSrc_p.meshes[mi];
                        tmpMesh = dep_amesh.Clone(localPlacement_mx * dep_amesh.transMatrix);
                        tmpMesh.subItemAddress = "_1";
                        ax_meshes.Add(tmpMesh);
                    }


                    if (makeGameObjects && !parametricObject.combineMeshes)
                    {
                        nodePlugGO = base.nodeSrc_po.generator.generate(true, initiator_po, renderToOutputParameter);

                    }


                    if (nodePlugGO != null && makeGameObjects && !parametricObject.combineMeshes)
                    {

                     

                        nodePlugGO.transform.SetParent(go.transform);
                        nodePlugGO.transform.rotation = AXUtilities.QuaternionFromMatrix(mx);
                        nodePlugGO.transform.position = AXUtilities.GetPosition(mx);


                    }
                }


                // BOUNDING
                if (nodeSrc_po == null)
                    return null;

                boundingMeshes.Add(new AXMesh(nodeSrc_po.boundsMesh, localPlacement_mx * nodeSrc_po.generator.localMatrix));

                //boundingMeshes.Add(new AXMesh(Pather.getBoundingBox(planPath)));

                // FINISH AX_MESHES
                //Debug.Log("finish " + ax_meshes.Count);
                parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);



                // FINISH BOUNDS

                CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
                for (int bb = 0; bb < boundsCombinator.Length; bb++)
                {
                    boundsCombinator[bb].mesh = boundingMeshes[bb].mesh;
                    boundsCombinator[bb].transform = boundingMeshes[bb].transMatrix;
                }
                setBoundsWithCombinator(boundsCombinator);




                // FINISH GAME_OBJECTS

                if (makeGameObjects)
                {
                   
                    Matrix4x4 tmx = parametricObject.getLocalMatrix();

                    go.transform.rotation = AXUtilities.QuaternionFromMatrix(tmx);
                    go.transform.position = AXUtilities.GetPosition(tmx);
                    go.transform.localScale = parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

                    return go;
                }



            }


            return null;

        }

        public override void parameterWasModified(AXParameter p)
        {
            switch (p.Name)
            {
                case "As Percentage":
                    Debug.Log("AsPercentage: " + asPercentage);
                    if (asPercentage)
                    {
                        // convert to percentage
                        //Debug.Log(distance + "/" + planCurve.uDistances.Last());
                        distance = distance * planCurve.uDistances.Last();

                    }
                    else
                    {
                        distance = distance / planCurve.uDistances.Last();
                        
                    }
                    P_Distance.FloatVal = distance;
                    break;
                default:
                    break;
            }

            

            
        }

        public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
        {
            // use shift too
            // use caller address
            if (input_po == null)
                return Matrix4x4.identity;

            //if (input_P != null)
            //{
            //    if (input_P.Name == "Plan")
            //        return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
            //}
            // PLAN
            if (input_po == planSrc_po)
                return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);




            return localPlacement_mx;
        }

     }
 }
