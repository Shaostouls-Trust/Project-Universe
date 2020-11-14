using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

using AXGeometry;

namespace AX.Generators 
{
	/*	CustomNode
	 *  This is a template for an AX Generator
	 *  To generate Mesh output, it is best to subclass Generator3D
	 */
	public class TaperDeformer : Deformer
	{

		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter P_Amount;
        public AXParameter P_modifierCurve;
        public AXParameter P_ZModifierCurve;
        public AXParameter P_ModifyX;
        public AXParameter P_ModifyZ;
        public AXParameter P_UnifyCurves;


        // WORKING FIELDS (Updated every Generate)
        // As a best practice, each parameter value could have a local variable
        // That is set before the generate funtion is called.
        // This will allow Handles to acces the parameter values more efficiently.
        public float 		amount;
        public AnimationCurve modifierCurve;
        public AnimationCurve zModifierCurve;
        private AnimationCurve zModifierCurveCached;

        bool modifyX = true;
        bool modifyZ = true;

        bool unifyTaperCurves = true;
           

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
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

			// GEOMETRY_CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Amount");


            P_UnifyCurves = parametricObject.addParameter(AXParameter.DataType.Bool, "UnifyTaperCurves", true);


            P_ModifyX = parametricObject.addParameter(AXParameter.DataType.Bool, "TaperX", true);
            P_modifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, "ModifierCurve");

            P_ModifyZ = parametricObject.addParameter(AXParameter.DataType.Bool, "TaperZ", true);
            P_ZModifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, "ZModifierCurve");

            Keyframe[] keys = new Keyframe[2];
			keys[0] = new Keyframe(0, 1);
			keys[1] = new Keyframe(3, .25f);

            P_modifierCurve.animationCurve = new AnimationCurve(keys);
            P_ZModifierCurve.animationCurve = new AnimationCurve(keys);
        }




        // POLL INPUTS (only on graph structure change())
        // This function is an optimization. It is costly to use getParameter(name) every frame,
        // so this function, which is called when scripts are loaded or when there has been a change in the
        // the graph structure. 
        // Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

        public override void pollInputParmetersAndSetUpLocalReferences()
		{



			base.pollInputParmetersAndSetUpLocalReferences();



			P_Amount 						= parametricObject.getParameter("Amount", "amount");
            P_modifierCurve = parametricObject.getParameter("ModifierCurve");
            P_ZModifierCurve = parametricObject.getParameter("ZModifierCurve");

            P_ModifyX = parametricObject.getParameter("TaperX");
            P_ModifyZ = parametricObject.getParameter("TaperZ");
            P_UnifyCurves = parametricObject.getParameter("UnifyTaperCurves");


        }


        // POLL CONTROLS (every model.generate())
        // It is helpful to set the values for parameter variables before generate().
        // These values will be available not only to generate() but also the Handle functions.
        public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			amount 			= (P_Amount  != null)  	? P_Amount.FloatVal	: 0.0f;


            modifyX = (P_ModifyX != null) ? P_ModifyX.boolval : true;
            modifyZ = (P_ModifyZ != null) ? P_ModifyZ.boolval : true;
            unifyTaperCurves = (P_UnifyCurves != null) ? P_UnifyCurves.boolval : true;


            if (P_modifierCurve != null)
            {
                if (P_modifierCurve.animationCurve == null)
                {
                    Keyframe[] keys = new Keyframe[2];
                    keys[0] = new Keyframe(0, 1);
                    keys[1] = new Keyframe(3, .25f);

                    P_modifierCurve.animationCurve = new AnimationCurve(keys);
                }
                modifierCurve = P_modifierCurve.animationCurve;
            }

            //Debug.Log("unifyTaperCurves="+ unifyTaperCurves);
            if (unifyTaperCurves)
            {
                if (P_modifierCurve != null && P_ZModifierCurve != null)
                {

                    zModifierCurve = P_modifierCurve.animationCurve;
                    P_ZModifierCurve.animationCurve = P_modifierCurve.animationCurve;
                }

               
            }
            else
            {
                if (P_ZModifierCurve != null)
                {
                    if (P_ZModifierCurve.animationCurve == null || P_ZModifierCurve.animationCurve == P_modifierCurve.animationCurve)
                    {
                        Keyframe[] keys = new Keyframe[2];
                        keys[0] = new Keyframe(0, 1);
                        keys[1] = new Keyframe(3, .25f);

                        P_ZModifierCurve.animationCurve = new AnimationCurve(keys);
                    }
                    zModifierCurve = P_ZModifierCurve.animationCurve;
                }
            }
           

 

        }
        public override void parameterWasModified(AXParameter p)
        {
            switch (p.Name)
            {
                case "UnifyTaperCurves":
                    if (P_ZModifierCurve != null)
                    {
                        if (P_UnifyCurves != null && P_UnifyCurves.boolval)
                        {
                            zModifierCurveCached = zModifierCurve;
                            P_ZModifierCurve.animationCurve = P_modifierCurve.animationCurve;
                        }
                        else
                        {
                            zModifierCurve = zModifierCurveCached;
                            P_ZModifierCurve.animationCurve = zModifierCurveCached;
                        }

                    }


                    break;
            }
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
				
			if (parametricObject == null  || ! parametricObject.hasInputMeshReady("Input Mesh"))
				return null;

			// At this point the amount variable has been set to the FloatValue of its amount parameter.
			preGenerate();


			//Vector3 center = Vector3.zero;

			// AXMeshes are the key product of a Generator3D
			List<AXMesh> 		ax_meshes 	= new List<AXMesh>();

			AXParameter 		inputSrc_p  = P_Input.DependsOn;
			AXParametricObject 	inputSrc_po = inputSrc_p.parametricObject;


            //			GameObject go 		= null;
            //			if (makeGameObjects && ! parametricObject.combineMeshes)
            //				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);


            if (makeGameObjects && !parametricObject.combineMeshes)
            {
                GameObject GOsToDeformPlug = inputSrc_po.generator.generate(true, initiator_po, true);

               


                if (GOsToDeformPlug == null)
                    return null;

                GOsToDeformPlug.name = parametricObject.Name;

                AXGameObject axgo = GOsToDeformPlug.GetComponent<AXGameObject>();

                if (axgo != null)
                    UnityEngine.Object.DestroyImmediate(axgo);

                //if (axgo == null)
                axgo = ArchimatixUtils.addAXGameObject(GOsToDeformPlug, parametricObject);



                Collider[] colliders = GOsToDeformPlug.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    // remove the collider
#if UNITY_EDITOR
                    UnityEngine.Object.DestroyImmediate(collider);
#else
                    UnityEngine.Object.Destroy(collider);
#endif
                }

                modifyGameObject(GOsToDeformPlug, Matrix4x4.identity);


                Matrix4x4 tmx = parametricObject.getLocalMatrix();

                GOsToDeformPlug.transform.rotation = AXUtilities.QuaternionFromMatrix(tmx);
                GOsToDeformPlug.transform.position = AXUtilities.GetPosition(tmx);
                GOsToDeformPlug.transform.localScale = parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

                return GOsToDeformPlug;
            }
            else
            {
                // ! makeGameObjects || COMBINE


                Bounds b = new Bounds();

                if (inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
                {

                    //Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
                    AXMesh amesh = null;
                    for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++)
                    {

                        // Instance does not inhereit and build on the transform of its source object.
                        amesh = P_Input.DependsOn.meshes[i].Clone();
                        Mesh m = amesh.mesh;
                        Vector3[] verts = m.vertices;


                        // ---------- EACH VERTEX ----------------------
                        for (int j = 0; j < verts.Length; j++)
                        {

                            Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

                            vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

                            float xscale = 1;
                            float zscale = 1;

                            if (modifyX && modifierCurve != null)
                                xscale = modifierCurve.Evaluate(vert.y) * (1 - (amount * vert.y));


                            if (modifyZ)
                                zscale = zModifierCurve.Evaluate(vert.y) * (1 - (amount * vert.y));

                            vert = new Vector3(vert.x * xscale, vert.y, vert.z * zscale);

                            //							Vector3 n = -vert.normalized;
                            //							n *= scale;
                            //							vert = vert + n;

                            verts[j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
                        }
                        // ---------- / EACH VERTEX ----------------------

                        m.vertices = verts;

                        amesh.mesh.RecalculateBounds();
                        b.Encapsulate(amesh.mesh.bounds);

                        ax_meshes.Add(amesh);
                    }


                    //Debug.Log("taper isReplica=" + isReplica);

                    parametricObject.bounds = b;






                    parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);

                    if (makeGameObjects)
                    {
                        GameObject retGO = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);

                       //Debug.Log("HERE");
                        retGO.name = parametricObject.Name;
                        //AXGameObject axgo = retGO.GetComponent<AXGameObject>();

                        //if (axgo != null)
                        //    UnityEngine.Object.DestroyImmediate(axgo);

                        ////if (axgo == null)
                            //axgo = ArchimatixUtils.addAXGameObject(retGO, parametricObject);

                        return retGO;
                    }
                     

                }

            }

            return null;
        
        
        
        }


        // --------------------------------------------------------------------------
        // Keep separate GameObjects and deform each.
        // --------------------------------------------------------------------------

        public Bounds modifyGameObject(GameObject go, Matrix4x4 m, int gener= 0 )
		{
            Bounds bounds = new Bounds();

            if (go == null || modifierCurve == null)
				return bounds;

//			Debug.Log(" - " + go.name );
//			Debug.Log(go.transform.localToWorldMatrix);

			m = go.transform.localToWorldMatrix;





			MeshFilter mf = go.GetComponent<MeshFilter>();



			if (mf != null)
			{


				Mesh mesh = AXMesh.cloneMesh(mf.sharedMesh);
				Vector3[] verts = mesh.vertices;

				// ---------- EACH VERTEX ----------------------
				for (int j = 0; j < verts.Length; j++) {

					Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

					vert = m.MultiplyPoint3x4(vert);

                    //Debug.Log(verts[j].y + " => " + vert.y);

                    float xscale = 1;
                    float zscale = 1;

                    if (modifyX && modifierCurve != null)
                        xscale = modifierCurve.Evaluate(vert.y) * (1 - (amount * vert.y));


                    if (modifyZ)
                    {
                        if (unifyTaperCurves)
                        {
                            zscale = xscale;
                        }
                        else
                        {
                            zscale = zModifierCurve.Evaluate(vert.y) * (1 - (amount * vert.y));
                        }
                    }



                    //Debug.Log("scale="+scale);

                    //float vertX = 


                    vert = new Vector3(vert.x*xscale, vert.y, vert.z*zscale);

					verts [j] = m.inverse.MultiplyPoint3x4(vert);
				}
				// ---------- \ EACH VERTEX ----------------------

				mesh.vertices = verts;
//				mesh.RecalculateNormals();
//				mesh.RecalculateTangents();
				mesh.RecalculateBounds();
				mf.sharedMesh = mesh;

                bounds = mesh.bounds;





					//MeshCollider mc = go.AddComponent<MeshCollider>();
					//mc.sharedMesh = mesh;
					//mc.convex = true;
				
			}


			for (int i=0; i<go.transform.childCount; i++)
			{
               bounds.Encapsulate( modifyGameObject( go.transform.GetChild(i).gameObject, m, gener++) );

			}

            if (gener == 0)
            {


                switch (parametricObject.colliderType)
                {

                    case ColliderType.Box:
                        BoxCollider bc = (BoxCollider)go.AddComponent(typeof(BoxCollider));

                        if (parametricObject.axMat.physMat != null)
                            bc.material = parametricObject.axMat.physMat;
                        break;

                    case ColliderType.Capsule:
                        CapsuleCollider cc = (CapsuleCollider)go.AddComponent(typeof(CapsuleCollider));
                        if (parametricObject.axMat.physMat != null)
                            cc.material = parametricObject.axMat.physMat;
                        break;

                    case ColliderType.Sphere:
                        SphereCollider sc = (SphereCollider)go.AddComponent(typeof(SphereCollider));
                        //sc.center = mesh.bounds.center;
                        //sc.radius = mesh.bounds.extents.x;
                        //if (parametricObject.axMat.physMat != null)
                            sc.material = parametricObject.axMat.physMat;
                        break;

                    case ColliderType.Mesh:


                        MeshCollider mc = (MeshCollider)go.AddComponent(typeof(MeshCollider));

                        if (parametricObject.axMat.physMat != null)
                            mc.material = parametricObject.axMat.physMat;
                        break;

                    case ColliderType.ConvexMesh:
                        MeshCollider mvc = (MeshCollider)go.AddComponent(typeof(MeshCollider));
                        mvc.convex = true;

                        if (parametricObject.axMat.physMat != null)
                            mvc.material = parametricObject.axMat.physMat;

                        //Debug.Log("axMat.physMat="+mvc.material  );
                        break;


                }

            }
            return bounds;

		}

	}

}
