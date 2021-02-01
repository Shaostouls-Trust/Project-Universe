using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

namespace AX.Generators
{
    /*  CustomNode
     *  This is a template for an AX Generator
     *  To generate Mesh output, it is best to subclass Generator3D
     */
    public class UVProjector : AX.Generators.Generator3D 
    { 

        // INPUTS
        // It is nice to have a local variable for parameters set at initialization
        // so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
        public AXParameter P_Width;
        public AXParameter P_TotalWidth;
        public AXParameter P_Height;

        public AXParameter P_ShiftU;
        public AXParameter P_ShiftV;

        public AXParameter P_ScaleU;
        public AXParameter P_ScaleV;

        public AXParameter P_Reverse;

        public AXParameter P_RecalcNormals;

        public AXParameter P_ModifierCurve;


        // WORKING FIELDS (Updated every Generate)
        // As a best practice, each parameter value could have a local variable
        // That is set before the generate funtion is called.
        // This will allow Handles to acces the parameter values more efficiently.
        public float width;
        public float totalWidth;
        public float height;

        public float shiftU;
        public float shiftV;

        public float scaleU;
        public float scaleV;

        float scaleCenY = 0;

        public bool recalcNormals = false;

        public bool reverse;

        public AnimationCurve modifierCurve;

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
            parametricObject.addParameter(AXParameter.DataType.Float, "width", .15f);
           
            parametricObject.addParameter(AXParameter.DataType.Float, "height", .25f);

            parametricObject.addParameter(AXParameter.DataType.Float, "shiftU", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "shiftV", 0f);

            parametricObject.addParameter(AXParameter.DataType.Float, "scaleU", 1f);
            parametricObject.addParameter(AXParameter.DataType.Float, "scaleV", 1f);

            parametricObject.addParameter(AXParameter.DataType.Bool, "Reverse");
            parametricObject.addParameter(AXParameter.DataType.Bool, "RecalcNormals");

            P_ModifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, "ModifierCurve");

            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(10f, 0f);

            P_ModifierCurve.animationCurve = new AnimationCurve(keys);
        }




        // POLL INPUTS (only on graph structure change())
        // This function is an optimization. It is costly to use getParameter(name) every frame,
        // so this function, which is called when scripts are loaded or when there has been a change in the
        // the graph structure. 
        // Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();



            P_Width = parametricObject.getParameter("width");
            P_Height = parametricObject.getParameter("height");

            P_ShiftU = parametricObject.getParameter("shiftU");
            P_ShiftV = parametricObject.getParameter("shiftV");

            P_ScaleU = parametricObject.getParameter("scaleU");
            P_ScaleV = parametricObject.getParameter("scaleV");

            P_Reverse = parametricObject.getParameter("Reverse");


            P_RecalcNormals = parametricObject.getParameter("RecalcNormals");


            P_ModifierCurve = parametricObject.getParameter("ModifierCurve");

            P_Reverse = parametricObject.getParameter("Reverse");



        }


        // POLL CONTROLS (every model.generate())
        // It is helpful to set the values for parameter variables before generate().
        // These values will be available not only to generate() but also the Handle functions.
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

            width = (P_Width != null) ? P_Width.FloatVal : 0.0f;
            height = (P_Height != null) ? P_Height.FloatVal : 0.0f;

            shiftU = (P_ShiftU != null) ? P_ShiftU.FloatVal : 0f;
            shiftV = (P_ShiftV != null) ? P_ShiftV.FloatVal : 0f;

            scaleU = (P_ScaleU != null) ? P_ScaleU.FloatVal : 1.0f;
            scaleV = (P_ScaleV != null) ? P_ScaleV.FloatVal : 1.0f;

            reverse = (P_Reverse != null) ? P_Reverse.boolval : false;

            recalcNormals = (P_RecalcNormals != null) ? P_RecalcNormals.boolval : false;

            if (P_ModifierCurve != null)
                modifierCurve = P_ModifierCurve.animationCurve;
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


            //Vector3 center = Vector3.zero;

            // AXMeshes are the key product of a Generator3D
            List<AXMesh> ax_meshes = new List<AXMesh>();

            AXParameter inputSrc_p = P_Input.DependsOn;
            AXParametricObject inputSrc_po = inputSrc_p.parametricObject;


            //Debug.Log(inputSrc_po.bounds.center.x + " " + inputSrc_po.bounds.size.x/2);

            //shiftU -=  (inputSrc_po.bounds.center.x - inputSrc_po.bounds.size.x / 2);

            if (inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
            {

                //Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
                AXMesh amesh = null;


                // BOUNDING

                //CombineInstance[] boundsCombinator = new CombineInstance[ P_Input.DependsOn.meshes.Count];


                parametricObject.bounds = P_Input.DependsOn.parametricObject.bounds;
                Bounds b = parametricObject.bounds;


                float meshWidth = inputSrc_po.bounds.size.x;
                float meshHeight = inputSrc_po.bounds.size.y;

                // EACH MESH
                for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++)
                {

                    // Instance does not inhereit and build on the transform of its source object.
                    amesh = P_Input.DependsOn.meshes[i].CloneTransformed(P_Input.DependsOn.meshes[i].transMatrix);
                    Mesh                  m = amesh.mesh;
                    Vector3[]   verts   = m.vertices;
                    Vector2[]   uv      = m.uv;

                    float u, v;

                   
                    float xmin = inputSrc_po.bounds.center.x- inputSrc_po.bounds.size.x/2; //b.min.x;
                    float ymin = 0; // b.min.y;

                   

                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < verts.Length; j++)
                    {
           
                        u = ((verts[j].x - xmin) - (meshWidth/2 * (1-scaleU))) / (meshWidth * scaleU);
                        u -= shiftU;

                        u += .02f;


                        v = ((verts[j].y - ymin) - (meshHeight * scaleCenY * (1 - scaleV))) / (meshHeight * scaleV);
                        v -= shiftV;

                        
                        uv[j] = reverse ? new Vector2(1 - u, v) : new Vector2(u, v);


                        
                    }
                    // ---------- EACH VERTEX ----------------------

                    m.uv = uv;




                    if (recalcNormals)
                    {
                        //Debug.Log("RecalcNormals");
                        amesh.mesh.RecalculateNormals();
                    }
                       




                    ax_meshes.Add(amesh);


                }


            

                parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


                if (makeGameObjects)
                    return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);


                // BOUNDING

                parametricObject.boundsMesh = ArchimatixUtils.meshClone(inputSrc_po.boundsMesh);


            }

            return null;

        }


    }

}
