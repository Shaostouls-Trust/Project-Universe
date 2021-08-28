using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

using Axis = AXGeometry.Axis;

namespace AX.Generators
{
    /*  CustomNode
     *  This is a template for an AX Generator
     *  To generate Mesh output, it is best to subclass Generator3D
     */
    public class UVProjector : AX.Generators.Generator3D, IHandles
    {
        public override string GeneratorHandlerTypeName { get { return "UVProjectorHandler"; } }



        // INPUTS
        // It is nice to have a local variable for parameters set at initialization
        // so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
        public AXParameter P_ProjectionAxis;

        public AXParameter P_Width;
        public AXParameter P_TotalWidth;
        public AXParameter P_Height;

        public AXParameter P_ShiftU;
        public AXParameter P_ShiftV;

        public AXParameter P_ScaleU;
        public AXParameter P_ScaleV;



        public AXParameter P_UseRectMapping;

        public AXParameter P_X1;
        public AXParameter P_X2;
        public AXParameter P_Y1;
        public AXParameter P_Y2;

        public AXParameter P_U1;
        public AXParameter P_U2;
        public AXParameter P_V1;
        public AXParameter P_V2;


        public AXParameter P_DrawInputHandles;




        public AXParameter P_Reverse;

        public AXParameter P_RecalcNormals;

        public AXParameter P_ModifierCurve;


        // WORKING FIELDS (Updated every Generate)
        // As a best practice, each parameter value could have a local variable
        // That is set before the generate funtion is called.
        // This will allow Handles to acces the parameter values more efficiently.
        public Axis projectionAxis;

        public float width;
        public float totalWidth;
        public float height;

        public float shiftU;
        public float shiftV;

        public float scaleU;
        public float scaleV;


        // RECT MAPPING
        public bool useRectMapping = false;
        public Rect boundsRect;
        public Rect atlasRect;

        public float mU;
        public float mV;

        public float x1;
        public float x2;
        public float y1;
        public float y2;

        public float u1;
        public float u2;
        public float v1;
        public float v2;

        public bool drawInputHandles = true;

        public Color handleRectColor = Color.yellow;
        public Color handleOutlineColor = Color.red;


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
            parametricObject.addParameter(AXParameter.DataType.Option, "ProjectionAxis", (int)Axis.Y);

            parametricObject.addParameter(AXParameter.DataType.Float, "width", .15f);

            parametricObject.addParameter(AXParameter.DataType.Float, "height", .25f);

            parametricObject.addParameter(AXParameter.DataType.Float, "shiftU", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "shiftV", 0f);

            parametricObject.addParameter(AXParameter.DataType.Float, "scaleU", 1f);
            parametricObject.addParameter(AXParameter.DataType.Float, "scaleV", 1f);


            P_UseRectMapping = parametricObject.addParameter(AXParameter.DataType.Bool, "UseRectMapping", true);
            parametricObject.addParameter(AXParameter.DataType.Float, "X1", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "X2", 1f);
            parametricObject.addParameter(AXParameter.DataType.Float, "Y1", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "Y2", 1f);

            parametricObject.addParameter(AXParameter.DataType.Float, "U1", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "U2", 1f);
            parametricObject.addParameter(AXParameter.DataType.Float, "V1", 0f);
            parametricObject.addParameter(AXParameter.DataType.Float, "V2", 1f);

            parametricObject.addParameter(AXParameter.DataType.Bool, "Draw Input Handles", true);



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

            P_ProjectionAxis = parametricObject.getParameter("ProjectionAxis");

            P_Width = parametricObject.getParameter("width");
            P_Height = parametricObject.getParameter("height");

            P_ShiftU = parametricObject.getParameter("shiftU");
            P_ShiftV = parametricObject.getParameter("shiftV");

            P_ScaleU = parametricObject.getParameter("scaleU");
            P_ScaleV = parametricObject.getParameter("scaleV");



            P_UseRectMapping = parametricObject.getParameter("UseRectMapping");

            P_X1 = parametricObject.getParameter("X1");
            P_X2 = parametricObject.getParameter("X2");
            P_Y1 = parametricObject.getParameter("Y1");
            P_Y2 = parametricObject.getParameter("Y2");

            P_U1 = parametricObject.getParameter("U1");
            P_U2 = parametricObject.getParameter("U2");
            P_V1 = parametricObject.getParameter("V1");
            P_V2 = parametricObject.getParameter("V2");

            P_DrawInputHandles = parametricObject.getParameter("Draw Input Handles");



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

            projectionAxis = (P_ProjectionAxis != null) ? (Axis)P_ProjectionAxis.IntVal : Axis.Y;


            width = (P_Width != null) ? P_Width.FloatVal : 0.0f;
            height = (P_Height != null) ? P_Height.FloatVal : 0.0f;

            shiftU = (P_ShiftU != null) ? P_ShiftU.FloatVal : 0f;
            shiftV = (P_ShiftV != null) ? P_ShiftV.FloatVal : 0f;

            scaleU = (P_ScaleU != null) ? P_ScaleU.FloatVal : 1.0f;
            scaleV = (P_ScaleV != null) ? P_ScaleV.FloatVal : 1.0f;


            useRectMapping = (P_UseRectMapping != null) ? P_UseRectMapping.boolval : true;

            x1 = (P_X1 != null) ? P_X1.FloatVal : 0.0f;
            x2 = (P_X2 != null) ? P_X2.FloatVal : 1.0f;
            y1 = (P_Y1 != null) ? P_Y1.FloatVal : 0.0f;
            y2 = (P_Y2 != null) ? P_Y2.FloatVal : 1.0f;

            u1 = (P_U1 != null) ? P_U1.FloatVal : 0.0f;
            u2 = (P_U2 != null) ? P_U2.FloatVal : 1.0f;
            v1 = (P_V1 != null) ? P_V1.FloatVal : 0.0f;
            v2 = (P_V2 != null) ? P_V2.FloatVal : 1.0f;

            drawInputHandles = (P_DrawInputHandles != null) ? P_DrawInputHandles.boolval : true;


            mU = (u2 - u1) / (x2 - x1);
            mV = (v2 - v1) / (y2 - y1);



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



                if (inputSrc_p != null)
                {
                    if (inputSrc_p.mesh != null)
                    {
                        inputSrc_p.mesh.RecalculateBounds();
                        b = inputSrc_p.mesh.bounds;
                    }
                    else if (inputSrc_p.meshes != null && inputSrc_p.meshes.Count > 0)
                    {

                        // combine meshes
                        CombineInstance[] combinator = new CombineInstance[inputSrc_p.meshes.Count];
                        for (int i = 0; i < inputSrc_p.meshes.Count; i++)
                        {
                            AXMesh ax_mesh = inputSrc_p.meshes[i];
                            combinator[i].mesh = ax_mesh.mesh;
                            combinator[i].transform = ax_mesh.transMatrix;
                        }


                        Mesh cmesh = new Mesh();

                        cmesh.CombineMeshes(combinator);
                        //Debug.Log("inputSrc_p.meshes.Count="+ inputSrc_p.meshes.Count);
                        cmesh.RecalculateBounds();

                        b = cmesh.bounds;
                    }



                }
                parametricObject.bounds = b;


                float umin = 0, vmin = 0;


                float meshWidth = 0;
                float meshHeight = 0;

                switch (projectionAxis)
                {
                    case Axis.X:
                    case Axis.NX:
                        meshWidth = b.size.z;
                        meshHeight = b.size.y;
                        umin = b.min.z;
                        vmin = b.min.y;
                        break;

                    case Axis.Y:
                    case Axis.NY:
                        meshWidth = b.size.x;
                        meshHeight = b.size.z;
                        umin = b.min.x;
                        vmin = b.min.z;
                        break;

                    case Axis.Z:
                    case Axis.NZ:
                        meshWidth = b.size.x;
                        meshHeight = b.size.y;
                        umin = b.min.x;
                        vmin = b.min.y;
                        break;
                }

                // EACH MESH
                for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++)
                {
                    // Instance does not inhereit and build on the transform of its source object.
                    amesh = P_Input.DependsOn.meshes[i].CloneTransformed(P_Input.DependsOn.meshes[i].transMatrix);
                    Mesh m = amesh.mesh;
                    Vector3[] verts = m.vertices;
                    Vector2[] uv = m.uv;

                    float u = 0, v = 0, vertu = 0, vertv = 0; ;

                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < verts.Length; j++)
                    {
                        switch (projectionAxis)
                        {
                            case Axis.X:
                                vertu = verts[j].z;
                                vertv = verts[j].y;
                                break;
                            case Axis.NX:
                                vertu = -verts[j].z;
                                vertv = verts[j].y;
                                break;

                            case Axis.Y:
                                vertu = verts[j].x;
                                vertv = verts[j].z;
                                break;
                            case Axis.NY:
                                vertu = verts[j].x;
                                vertv = -verts[j].z;
                                break;

                            case Axis.Z:
                                vertu = verts[j].x;
                                vertv = verts[j].y;
                                break;
                            case Axis.NZ:
                                vertu = verts[j].x;
                                vertv = verts[j].y;
                                break;
                        }


                        Vector2 uv2;
                        if (useRectMapping)
                        {

                            uv2 = uvFromRectMapping(vertu, vertv);
                        }
                        else
                        {
                            u = ((vertu - umin) - (meshWidth / 2 * (1 - scaleU))) / (meshWidth * scaleU);
                            u -= shiftU;

                            u += .02f;

                            v = ((vertv - vmin) - (meshHeight * scaleCenY * (1 - scaleV))) / (meshHeight * scaleV);
                            v -= shiftV;

                            uv2 = new Vector2(u, v);
                        }


                        uv[j] = reverse ? new Vector2(1 - uv2.x, uv2.y) : uv2;
                    }
                    // ---------- EACH VERTEX ----------------------

                    m.uv = uv;

                    // if (recalcNormals)
                    amesh.mesh.RecalculateNormals();

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



        public Vector2 uvFromRectMapping(float x, float y)
        {
            float u = mU * (x - x1) + u1;
            float v = mV * (y - y1) + v1;
            Vector2 uv = new Vector2(u, v);

            return uv;
        }



        public void AutoSetBoundsRect()
        {

            inputSrc_p = P_Input.DependsOn;

            if (inputSrc_p == null)
                return;

            inputSrc_po = inputSrc_p.parametricObject;


            Axis pAxis = (Axis)P_ProjectionAxis.IntVal;

            Bounds b = inputSrc_po.bounds;
            // combine meshes
            CombineInstance[] combinator = new CombineInstance[inputSrc_p.meshes.Count];
            for (int i = 0; i < inputSrc_p.meshes.Count; i++)
            {
                AXMesh ax_mesh = inputSrc_p.meshes[i];
                combinator[i].mesh = ax_mesh.mesh;
                combinator[i].transform = ax_mesh.transMatrix;
            }


            Mesh cmesh = new Mesh();

            cmesh.CombineMeshes(combinator);
            //Debug.Log("inputSrc_p.meshes.Count="+ inputSrc_p.meshes.Count);
            cmesh.RecalculateBounds();
            b = cmesh.bounds;

            inputSrc_po.bounds = b;


            float toler = .001f;

            switch (pAxis)
            {

                case Axis.X:
                    width = b.size.z;
                    height = b.size.y;
                    x1 = b.min.z;
                    x2 = b.max.z;
                    y1 = b.min.y;
                    y2 = b.max.y;
                    break;

                case Axis.Y:
                    width = b.size.x;
                    height = b.size.z;
                    x1 = b.min.x;
                    x2 = b.max.x;
                    y1 = b.min.z;
                    y2 = b.max.z;
                    break;

                case Axis.Z:
                    width = b.size.x;
                    height = b.size.y;
                    x1 = b.max.x + toler;
                    x2 = b.min.x - toler;
                    y1 = b.min.y;
                    y2 = b.max.y;
                    break;

                case Axis.NX:
                    width = b.size.z;
                    height = b.size.y;
                    x1 = b.min.z;
                    x2 = b.max.z;
                    y1 = b.min.y;
                    y2 = b.max.y;
                    break;

                case Axis.NY:
                    width = b.size.x;
                    height = b.size.z;
                    x1 = b.min.x;
                    x2 = b.max.x;
                    y1 = b.min.z;
                    y2 = b.max.z;
                    break;

                case Axis.NZ:
                    width = b.size.x;
                    height = b.size.y;
                    x1 = b.min.x - toler;
                    x2 = b.max.x + toler;
                    y1 = b.min.y;
                    y2 = b.max.y + toler;
                    break;

            }

            //boundsRect = new Rect(x1, y1, , height);





            P_X1.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(x1);
            P_X2.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(x2);
            P_Y1.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(y1);
            P_Y2.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(y2);

        }



        public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
        {

            base.connectionMadeWith(to_p, from_p);


            if (P_Input != null)
            {
                P_Input.polyTree = null;
                P_Input.paths = null;
                AutoSetBoundsRect();
            }
            else
            {
                // set boundsRect
                AutoSetBoundsRect();
            }

        }




        public override void parameterWasModified(AXParameter p)
        {


            switch (p.Name)
            {
                case "ProjectionAxis":

                    AutoSetBoundsRect();


                    break;
            }
        }

    }
}
