using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

namespace AX.Generators
{
    /*	CustomNode
	 *  This is a template for an AX Generator
	 *  To generate Mesh output, it is best to subclass Generator3D
	 */
    public class PillowDeformer : Deformer, ICustomNode, ILogic
    {
        public override string GeneratorHandlerTypeName { get { return "GeneratorHandler3D"; } }


        public AXParameter P_ZModifierCurve;


        // INPUTS
        // It is nice to have a local variable for parameters set at initialization
        // so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.

        
        //public AXParameter	P_ScaleZ;

        public AXParameter P_Width;
        public AXParameter P_Depth;

        public AXParameter P_CenX;

        public AXParameter P_ScaleL;
        public AXParameter P_DepthC;
        public AXParameter P_ScaleR;

        public AXParameter P_CenY;

        public AXParameter P_ScaleT;
        public AXParameter P_ScaleB;


        
        Vector3 center = Vector3.zero;

        public AXParameter P_HorizontalCurve;

        public AXParameter P_VerticalCurve;


        // WORKING FIELDS (Updated every Generate)
        // As a best practice, each parameter value could have a local variable
        // That is set before the generate funtion is called.
        // This will allow Handles to acces the parameter values more efficiently.
        public float width;
        public float depth;

        public float cenX;
        public float cenY;

        public float scaleT;

        public float scaleB;

        public float scaleL;
        public float scaleR;

        public float depthC;

        public AnimationCurve modifierCurveH;
        public AnimationCurve modifierCurveV;

        // INIT_PARAMETRIC_OBJECT
        // This initialization function is called when this Generator's AXParametric object 
        // is first created; for exampe, when its icon is clicked in the sidebar node menu.
        // It creates the default parameters that will appear in the node. 
        // Often there is at least one input and one output parameter. 
        // Parameters of type Float, Int and Bool that are created here will be available
        // to your generate() function. If no AXParameterType is specified, the type will be GeometryControl.

        public override void init_parametricObject()
        {
            // Init parameters for the node
            base.init_parametricObject();

            // INPUT AND OUTPUT
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));

            // GEOMETRY_CONTROLS
            P_CenX = parametricObject.addParameter(AXParameter.DataType.Float, "CenX", 0, -1f, 1f);
            P_CenY = parametricObject.addParameter(AXParameter.DataType.Float, "CenY", 0, -1f, 1f);

            // P_ScaleZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"ScaleZ", .01f, 1, 1);
            //P_ScaleZ.expressions.Add("ClampZ=Scale_Z*Width");
            //P_Scale_Z.expressions.Add("ClampZ=Scale_Z*ParentWidth");

            P_Width = parametricObject.addParameter(AXParameter.DataType.Float, "Width", 1f, .005f, 2f);
            P_Width = parametricObject.addParameter(AXParameter.DataType.Float, "Depth", 1f, .005f, 2f);

            //parametricObject.addParameter(AXParameter.DataType.Float, 	"ClampY", 1);
            P_ScaleL = parametricObject.addParameter(AXParameter.DataType.Float, "ScaleLeft", .5f, .005f, 1f);
            //P_DepthC = parametricObject.addParameter(AXParameter.DataType.Float, "DepthCenter", 1, .005f, 1f);
            P_ScaleR = parametricObject.addParameter(AXParameter.DataType.Float, "ScaleRight", .5f, .005f, 1f);
            P_ScaleT = parametricObject.addParameter(AXParameter.DataType.Float, "ScaleTop", .5f, .005f, 1f);
            P_ScaleB = parametricObject.addParameter(AXParameter.DataType.Float, "ScaleBottom", .5f, .005f, 1f);

            //parametricObject.addHandle("depth", AXHandle.HandleType.Point, "0", "0", "-Depth/2", "Depth=-han_z*2");


            //P_Width.expressions.Add("Scale_Z=ZDim/Width");



            P_HorizontalCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, "H Modifier Curve");
            P_VerticalCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, "V Modifier Curve");


            Keyframe[] ks = new Keyframe[3];


            ks[0] = new Keyframe(0, -.1f);
            ks[0].outTangent = 0f;    // straight

            ks[1] = new Keyframe(1, 0);
            ks[1].inTangent = .5f;    // -1 units on the y axis for 1 unit on the x axis.
            ks[1].outTangent = .5f;    // -1 units on the y axis for 1 unit on the x axis.

            ks[2] = new Keyframe(2 * 1, .5f);
            ks[2].inTangent = 0f;    // straight

            P_HorizontalCurve.animationCurve = new AnimationCurve(ks);
            P_VerticalCurve.animationCurve = new AnimationCurve(ks);


            //			Keyframe[] keys = new Keyframe[2];
            //			keys[0] = new Keyframe( 0f, 1f);
            //			keys[1] = new Keyframe(10f, 0f);
            //
            //			P_ModifierCurve.animationCurve = new AnimationCurve(keys);
        }




        // POLL INPUTS (only on graph structure change())
        // This function is an optimization. It is costly to use getParameter(name) every frame,
        // so this function, which is called when scripts are loaded or when there has been a change in the
        // the graph structure. 
        // Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();

            P_CenX = parametricObject.getParameter("CenX");
            P_CenY = parametricObject.getParameter("CenY");
            // P_Depth = parametricObject.getParameter("Flatness");
            P_Width = parametricObject.getParameter("Width");
            P_Depth = parametricObject.getParameter("Depth");

            P_ScaleL = parametricObject.getParameter("ScaleLeft");
            P_ScaleR = parametricObject.getParameter("ScaleRight");

            P_ScaleT = parametricObject.getParameter("ScaleTop");
            P_ScaleB = parametricObject.getParameter("ScaleBottom");

            P_HorizontalCurve = parametricObject.getParameter("H Modifier Curve");
            P_VerticalCurve = parametricObject.getParameter("V Modifier Curve");

        }




        // POLL CONTROLS (every model.generate())
        // It is helpful to set the values for parameter variables before generate().
        // These values will be available not only to generate() but also the Handle functions.
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

            //width 			= (P_Width  != null)  	? P_Width.FloatVal	: 0.0f;
            cenX = (P_CenX != null) ? P_CenX.FloatVal : 0.0f;
            cenY = (P_CenY != null) ? P_CenY.FloatVal : 0.0f;

            width = (P_Width != null) ? P_Width.FloatVal : 0.0f;
            depth = (P_Depth != null) ? P_Depth.FloatVal : 0.0f;

            scaleL = (P_ScaleL != null) ? P_ScaleL.FloatVal : 0.0f;
            scaleR = (P_ScaleR != null) ? P_ScaleR.FloatVal : 0.0f;

            scaleT = (P_ScaleT != null) ? P_ScaleT.FloatVal : 0.0f;
            scaleB = (P_ScaleB != null) ? P_ScaleB.FloatVal : 0.0f;

            AXParameter inputSrc_p = P_Input.DependsOn;




            if (P_HorizontalCurve != null)
                modifierCurveH = P_HorizontalCurve.animationCurve;

            if (P_VerticalCurve != null)
                modifierCurveV = P_VerticalCurve.animationCurve;
        }


        // GENERATE
        // This is the main function for generating a Shape, Mesh, or any other output that a node may be tasked with generating.
        // You can do pre processing of Inputs here, but that might best be done in an override of pollControlValuesFromParmeters().
        // 
        // Often, you will be creating a list of AXMesh objects. AXMesh has a Mesh, a Matrix4x4 and a Material 
        // to be used when drawing the ouput to the scene before creating GameObjects. Your list of AXMeshes generated 
        // is pased to the model via parametricObject.finishMultiAXMeshAndOutput().
        //
        // When generate() is called by AX, it will be either with makeGameObjects set to true or false. 
        // makeGameObjects=False is passed when the user is dragging a parameter and the model is regenerateing multiple times per second. 
        // makeGameObjects=True is passed when the user has stopped editing (for example, OnMouseup from a Handle) and it is time to 
        // build a GameObject hierarchy.

        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
        {

            if (parametricObject == null || !parametricObject.hasInputMeshReady("Input Mesh"))
                return null;

            // At this point the amount variable has been set to the FloatValue of its amount parameter.
            preGenerate();


            // AXMeshes are the key product of a Generator3D
            List<AXMesh> ax_meshes = new List<AXMesh>();

            AXParameter inputSrc_p = P_Input.DependsOn;
            AXParametricObject inputSrc_po = inputSrc_p.parametricObject;



            if (inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
            {

                //Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
                AXMesh amesh = null;

                // EACH MESH
                for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++)
                {



                    // Instance does not inhereit and build on the transform of its source object.
                    amesh = P_Input.DependsOn.meshes[i].Clone();
                    Mesh m = amesh.mesh;
                    Vector3[] verts = m.vertices;


                   
                    SetModifierCurve(P_Input.DependsOn.parametricObject.bounds);


                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < verts.Length; j++)
                    {


                        Vector3 fvert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

                        fvert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(fvert);

                        // float nz = zModifierCurve.Evaluate(fvert.z + W); // W is the unscaled Z from original round - the curve starts at z

                        float sxz = modifierCurveH.Evaluate(fvert.x); // W is the unscaled Z from original round - the curve starts at z
                        float syz = modifierCurveV.Evaluate(fvert.y); // W is the unscaled Z from original round - the curve starts at z


                        //float diff = D - Mathf.Abs(nz);

                        //float dimdiff = diff * Mathf.Clamp((.1f - Math.Abs(x)),0,1);

                        //if (nz < 0)
                        //    fvert.z = nz-diff;
                        //else
                        //    fvert.z = nz+diff;

                        fvert.z *= (sxz * syz);
                        verts[j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(fvert);
                    }
                    // ---------- EACH VERTEX ----------------------

                    m.vertices = verts;

                    ax_meshes.Add(amesh);
                }



                // BOUNDING

                parametricObject.boundsMesh = ArchimatixUtils.meshClone(inputSrc_po.boundsMesh);




                if (parametricObject.boundsMesh != null)
                {
                    Vector3[] boundsVerts = parametricObject.boundsMesh.vertices;

                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < boundsVerts.Length; j++)
                    {

                        Vector3 vert = new Vector3(boundsVerts[j].x, boundsVerts[j].y, boundsVerts[j].z);

                        //vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

                        //float scale = modifierCurve.Evaluate( vert.x);

                        boundsVerts[j] = new Vector3(vert.x, (vert.y + width * vert.x), vert.z);

                        //boundsVerts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
                    }
                    // ---------- EACH VERTEX ----------------------


                    parametricObject.boundsMesh.vertices = boundsVerts;
                    parametricObject.boundsMesh.RecalculateBounds();

                    //parametricObject.bounds = inputSrc_po.bounds;
                    parametricObject.bounds = parametricObject.boundsMesh.bounds;

                }

                //Debug.Log("inflate isReplica=" + isReplica);
                parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


                if (makeGameObjects)
                    return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);
            }

            return null;

        }


        public override void parameterWasModified(AXParameter p)
        {
            switch (p.Name)
            {
                case "ZDIM":

                    Debug.Log("ZDim modified");

                    scaleZ = scaleL / width;
                    P_Scale_Z.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(scaleZ);
                    break;

                    //case "Scale_Z":

                    //    //Debug.Log("Scale_Z modified");

                    //    zDim = scaleZ / width;
                    //    P_ZDim.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(zDim);
                    //    break;
            }
        }





        public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
        {

            if (from_p == null)// || from_p.DependsOn == null)
                return;

            AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p : from_p;
            AXParameter src_p = (to_p.parametricObject == parametricObject) ? from_p : to_p;
            //Debug.Log ("connectionMadeWith AAA "+ this_p.Name);

            base.connectionMadeWith(to_p, from_p);

            if (parametricObject.is2D())
                return;

            //Debug.Log ("connectionMadeWith BBB "+ this_p.Name);

            switch (this_p.Name)
            {
                case "Input Mesh":

                    Bounds b = src_p.parametricObject.bounds;

                    cenX = b.center.x - b.size.x / 2;
                    cenY = b.center.y - b.size.y / 2;

                    P_CenX.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(cenX);
                    P_CenY.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(cenY);

                    SetModifierCurve(src_p.parametricObject.bounds);


                    break;
            }
        }



        public void SetModifierCurve( Bounds b)
        {
            
            float minX = b.center.x - b.size.x / 2;
            float maxX = b.center.x + b.size.x / 2;

            
            
            float begslope = 1.75f * (1- scaleL) / (cenX - minX);
            float midslope = 0;
            float endslope = 1.75f * (1 - scaleR) / (cenX - maxX);

            Keyframe[] ks = new Keyframe[3];


            ks[0] = new Keyframe(minX, scaleL);
            ks[0].outTangent = begslope;    // straight

            ks[1] = new Keyframe(cenX, 1);
            ks[1].inTangent = midslope;    // -1 units on the y axis for 1 unit on the x axis.
            ks[1].outTangent = midslope;    // -1 units on the y axis for 1 unit on the x axis.

            ks[2] = new Keyframe(maxX, scaleR);
            ks[2].inTangent = endslope;    // straight


            modifierCurveH.keys = ks;





            float minY = b.center.y - b.size.y / 2;
            float maxY = b.center.y + b.size.y / 2;


            
            float begslopeV = 1.75f * (1 - scaleB) / (cenY - minY);
            float midslopeV = 0;
            float endslopeV = 1.75f * (1 - scaleT) / (cenY - maxY);

            Keyframe[] ksV = new Keyframe[3];


            ksV[0] = new Keyframe(minY, scaleB);
            ksV[0].outTangent = begslopeV;    // straight

            ksV[1] = new Keyframe(cenY, 1);
            ksV[1].inTangent = midslopeV;    // -1 units on the y axis for 1 unit on the x axis.
            ksV[1].outTangent = midslopeV;    // -1 units on the y axis for 1 unit on the x axis.

            ksV[2] = new Keyframe(maxY, scaleT);
            ksV[2].inTangent = endslopeV;    // straight


            modifierCurveV.keys = ksV;






        }


    }

}
