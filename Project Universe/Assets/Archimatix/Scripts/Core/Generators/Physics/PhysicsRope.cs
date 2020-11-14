#pragma warning disable

using System.Collections;

using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

namespace AX.Generators
{

	// PHYSICS_BALLOON

	// Creates a spherical cage of rigidbody colliders and a sphere mesh. 
	// Uses collider transforms as bones to add skinnedmeshrenderer.

	public class PhysicsRope : AX.Generators.Mesher, IReplica, ICustomNode
	{
		public override string GeneratorHandlerTypeName { get { return "PhysicsRopeHandler"; } }

		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter P_RigidbodyA;
		public AXParameter P_RigidbodyB;


		public AXParameter P_Point1;
		public AXParameter P_Point2;

		public AXParameter P_Segs;
		public AXParameter P_Slack;
		public AXParameter P_RopeRadius;
		public AXParameter P_SectionSegs;
		public AXParameter P_IsPhysical;

		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.

		public Rigidbody rigidbodyA;
		public Rigidbody rigidbodyB;

		public Vector3  point1;
		public Vector3  point2;
		public int      segs;
		public float    slack;

		public float ropeRadius = .1f;
		public int sectionSegs = 8;

		public bool isPhysical = true;

		// INIT_PARAMETRIC_OBJECT
		// This initialization function is called when this Generator's AXParametric object 
		// is first created; for exampe, when its icon is clicked in the sidebar node menu.
		// It creates the default parameters that will appear in the node. 
		// Often there is at least one input and one output parameter. 
		// Parameters of type Float, Int and Bool that are created here will be available
		// to your generate() function. If no AXParameterType is specified, the type will be GeometryControl.

		public Vector3[] catenaryPoints;
		public Vector3[] catenaryPointsEvenSpacing;


		Matrix4x4[]  evenRibMatrices;

		Mesh ropeMesh;

		List<Rigidbody> bonesRBs;
		BoneWeight[] boneWeights;

		public override void init_parametricObject()
		{
			// Init parameters for the node
			base.init_parametricObject();

			// INPUT AND OUTPUT


			P_RigidbodyA = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "RigidBody A"));
			P_RigidbodyA = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "RigidBody B"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Top Cap Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Bottom Cap Material"));

			// GEOMETRY_CONTROLS

			// RADIUS_BASE
			P_Point1    = parametricObject.addParameter(AXParameter.DataType.Vector3, "Point 1");
			P_Point2    = parametricObject.addParameter(AXParameter.DataType.Vector3, "Point 2");

			P_Point1.vector3 = new Vector3 (0, 5, 0);
			P_Point2.vector3 = new Vector3 (10, 10, 0);

			P_Segs      = parametricObject.addParameter(AXParameter.DataType.Int, "Segs", 12, 3, 12);
			P_Slack     = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "Slack", 2f, 0, 60);
			P_RopeRadius     = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "Rope Radius", .25f, 0.01f, 2);
			P_SectionSegs      = parametricObject.addParameter(AXParameter.DataType.Int, "Section Segs", 6, 3, 24);
			P_IsPhysical    = parametricObject.addParameter(AXParameter.DataType.Bool, "IsPhysical",true);
		}




		// POLL INPUTS (only on graph structure change())
		// This function is an optimization. It is costly to use getParameter(name) every frame,
		// so this function, which is called when scripts are loaded or when there has been a change in the
		// the graph structure. 
		// Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_RigidbodyA = parametricObject.getParameter("Rigidbody A");
			P_RigidbodyB = parametricObject.getParameter("Rigidbody B");
			P_Point1 = parametricObject.getParameter("Point 1");
			P_Point2 = parametricObject.getParameter("Point 2");
			P_Segs   = parametricObject.getParameter("Segs");
			P_Slack  = parametricObject.getParameter("Slack");
			P_RopeRadius  = parametricObject.getParameter("Rope Radius");
			P_SectionSegs  = parametricObject.getParameter("Section Segs");
			P_IsPhysical  = parametricObject.getParameter("IsPhysical");
		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			if (P_RigidbodyA != null && P_RigidbodyA.DependsOn != null) {
				AXParametricObject src_po = P_RigidbodyA.DependsOn.parametricObject;
				if (src_po.isRigidbody && src_po.lastGameObjectCreated != null) {
					rigidbodyA = src_po.lastGameObjectCreated.GetComponent<Rigidbody> ();

				}
			}
			 

			point1 = (P_Point1 != null) ? P_Point1.vector3 : Vector3.zero;
			point2 = (P_Point2 != null) ? P_Point2.vector3 : Vector3.zero;


			slack = (P_Slack != null) ? P_Slack.FloatVal 	: 1.0f;
			ropeRadius = (P_RopeRadius != null) ? P_RopeRadius.FloatVal 	: .1f;
			segs  = (P_Segs  != null) ? P_Segs.IntVal 		:    6;
			sectionSegs  = (P_SectionSegs  != null) ? P_SectionSegs.IntVal 		:    6;
			isPhysical  = (P_IsPhysical  != null) ? P_IsPhysical.boolval 		:    true;


			//ventRate = (P_VentRate != null) ? P_VentRate.FloatVal : 2.0f;


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

		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{

			//if (parametricObject == null || !parametricObject.hasInputMeshReady("Input Mesh"))
			//return null;

			// At this point the amount variable has been set to the FloatValue of its amount parameter.
			preGenerate ();

			// AXMeshes are the key product of a Generator3D
			List<AXMesh> ax_meshes = new List<AXMesh>();

			GameObject go = null;
			if (makeGameObjects && !parametricObject.combineMeshes) {
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);
				go.name = parametricObject.Name;


			}



			catenaryPoints = catenary(point1, point2, slack, segs+1);
			catenaryPointsEvenSpacing = evenSegs (catenaryPoints, 1);

			if (! isPhysical) {
				calculateEvenRibMatrices (catenaryPoints);
				skinSpline (catenaryPoints, sectionSegs);
			} else {
				calculateEvenRibMatrices (catenaryPointsEvenSpacing);
				skinSpline (catenaryPointsEvenSpacing, sectionSegs);
			}

			ax_meshes.Add(new AXMesh(ropeMesh, Matrix4x4.identity,parametricObject.axMat.mat));

			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);



			if (makeGameObjects) {

				MeshFilter mf = go.AddComponent<MeshFilter> ();
				mf.sharedMesh = ropeMesh;


				Rigidbody prevRigidBody = null;


				if (isPhysical && catenaryPointsEvenSpacing != null) {


					int boneCount = catenaryPointsEvenSpacing.Length - 1;

					Transform[] bones       = new Transform[boneCount];
					Matrix4x4[] bindPoses   = new Matrix4x4[boneCount];

					int bonesCursor = 0;

					SoftJointLimit sjl = new SoftJointLimit ();
					sjl.limit = 20;

					SoftJointLimitSpring sls = new SoftJointLimitSpring ();
					sls.spring = 12;
					sls.damper = 12;

					JointDrive jd = new JointDrive ();
					jd.positionSpring = 12;
					jd.positionDamper = 12;

					float linkLen = (catenaryPointsEvenSpacing [1] - catenaryPointsEvenSpacing [0]).magnitude;
					for (int i = 0; i < catenaryPointsEvenSpacing.Length - 1; i++) {
					
						//float len = Vector3.Distance (catenaryPointsEvenSpacing [i], catenaryPointsEvenSpacing [i + 1]);
				
						GameObject link = new GameObject ("seg_" + i);

						Rigidbody rb = link.AddComponent<Rigidbody> ();
						CapsuleCollider cc = link.AddComponent<CapsuleCollider> ();
						cc.radius = ropeRadius;
						cc.height = linkLen * .9f;
						cc.center = new Vector3 (0, linkLen / 2, 0);

					

						//CharacterJoint cj = link.AddComponent<CharacterJoint> ();
						if (i == 0 && rigidbodyA != null) {
							ConfigurableJoint cj = link.AddComponent<ConfigurableJoint> ();
							cj.connectedBody = rigidbodyA;

							cj.xMotion = ConfigurableJointMotion.Locked;
							cj.yMotion = ConfigurableJointMotion.Locked;
							cj.zMotion = ConfigurableJointMotion.Locked;

							cj.angularXMotion = ConfigurableJointMotion.Limited;
							cj.angularYMotion = ConfigurableJointMotion.Limited;
							cj.angularZMotion = ConfigurableJointMotion.Limited;

							cj.lowAngularXLimit 	= sjl;
							cj.highAngularXLimit 	= sjl;
							cj.angularYLimit 		= sjl;
							cj.angularZLimit 		= sjl;

							cj.angularXLimitSpring	= sls;
							cj.angularYZLimitSpring = sls;

							cj.angularXDrive 		= jd;
							cj.angularYZDrive 		= jd;

						}
						else {
							ConfigurableJoint cj = link.AddComponent<ConfigurableJoint> ();
							cj.connectedBody = prevRigidBody;

							cj.xMotion = ConfigurableJointMotion.Locked;
							cj.yMotion = ConfigurableJointMotion.Locked;
							cj.zMotion = ConfigurableJointMotion.Locked;

							cj.angularXMotion = ConfigurableJointMotion.Limited;
							cj.angularYMotion = ConfigurableJointMotion.Limited;
							cj.angularZMotion = ConfigurableJointMotion.Limited;

							cj.lowAngularXLimit = sjl;
							cj.highAngularXLimit = sjl;
							cj.angularYLimit = sjl;
							cj.angularZLimit = sjl;

							cj.angularXLimitSpring = sls;
							cj.angularYZLimitSpring = sls;

							cj.angularXDrive = jd;
							cj.angularYZDrive = jd;
						}
						link.transform.position = catenaryPointsEvenSpacing [i];
						link.transform.LookAt (catenaryPointsEvenSpacing [i + 1]);
						link.transform.Rotate( new Vector3(90,0,0) );

						if (i == 0)
							link.transform.parent = go.transform;
						else
							link.transform.parent = prevRigidBody.gameObject.transform;
						//link.transform.parent = go.transform;


						// ADD TO BONES
						bones[bonesCursor++] = link.transform;



						prevRigidBody = rb;
					  
					}

					//Debug.Log (catenaryPointsEvenSpacing.Length + " ..... " + bones.Length);


					// BONE POSES
					for (int i = 0; i < bones.Length; i++)
					{
						bindPoses[i] = bones[i].worldToLocalMatrix * go.transform.localToWorldMatrix;
					}


					SkinnedMeshRenderer rend = go.AddComponent<SkinnedMeshRenderer>();
					rend.sharedMaterial = parametricObject.axMat.mat;
					rend.updateWhenOffscreen = true;

//					for (int i = 0; i < ropeMesh.vertices.Length; i++) {
//						// each vert, search for closet colliders (bones)
//
//						boneWeights[i].boneIndex0 = i;
//						boneWeights[i].weight0 = 1;
//
//					}

					// A BoneWeights array (weights) was just created and the boneIndex and weight assigned.
					// The weights array will now be assigned to the boneWeights array in the Mesh.
					ropeMesh.boneWeights = boneWeights;


					// assign the bindPoses array to the bindposes array which is part of the mesh.
					ropeMesh.bindposes = bindPoses;

					// Assign bones and bind poses
					rend.bones = bones;
					rend.sharedMesh = ropeMesh;



				}




			





			}

			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);

			return go;
		}




		// CATENARY

		// BAsed on: https://gist.github.com/Farfarer/a765cd07920d48a8713a0c1924db6d70
		public static Vector3[] catenary(Vector3 pt1, Vector3 pt2, float slack, int segs)
		{
			float lineDist = Vector3.Distance (pt1, pt2);
			float lineDistH = Vector3.Distance (new Vector3(pt2.x, pt1.y, pt2.z), pt1);
			float l = lineDist + Mathf.Max(0.0001f, slack);
			float r = 0.0f;
			float s = pt1.y;
			float u = lineDistH;
			float v = pt2.y;

			if ((u-r) == 0.0f)
				return null;



			float ztarget = Mathf.Sqrt(Mathf.Pow(l, 2.0f) - Mathf.Pow(v-s, 2.0f)) / (u-r);

			int loops = 30;
			int iterationCount = 0;
			int maxIterations = loops * 10; // For safety.
			bool found = false;

			float z = 0.0f;
			float ztest = 0.0f;
			float zstep = 100.0f;
			float ztesttarget = 0.0f;
			for (int i = 0; i < loops; i++) {
				for (int j = 0; j < 10; j++) {
					iterationCount++;
					ztest = z + zstep;
					ztesttarget = (float)Math.Sinh(ztest)/ztest;

					if (float.IsInfinity (ztesttarget))
						continue;

					if (ztesttarget == ztarget) {
						found = true;
						z = ztest;
						break;
					} else if (ztesttarget > ztarget) {
						break;
					} else {
						z = ztest;
					}

					if (iterationCount > maxIterations) {
						found = true;
						break;
					}
				}

				if (found)
					break;

				zstep *= 0.1f;
			}

			float a = (u-r)/2.0f/z;
			float p = (r+u-a*Mathf.Log((l+v-s)/(l-v+s)))/2.0f;
			float q = (v+s-l*(float)Math.Cosh(z)/(float)Math.Sinh(z))/2.0f;



			Vector3[] points = new Vector3[segs];
			float stepsf = segs-1;
			float stepf;
			for (int i = 0; i < segs; i++) {
				stepf = i / stepsf;
				Vector3 pos = Vector3.zero;
				pos.x = Mathf.Lerp(pt1.x, pt2.x, stepf);
				pos.z = Mathf.Lerp(pt1.z, pt2.z, stepf);
				pos.y = a * (float)Math.Cosh(((stepf*lineDistH)-p)/a)+q;
				points[i] = pos;
			}







			return points;



		}








		public static Vector3[] evenSegs(Vector3[] points, float desiredSeglen) {

			float totalLen = 0;

			if (points == null || points.Length == 0)
				return null;
			
			for (int i = 0; i < points.Length-1; i++) {
				totalLen += Vector3.Distance (points[i], points[i+1]);
			}

			int segs = Mathf.CeilToInt (totalLen / desiredSeglen);

			float seglen = totalLen / segs;

			Vector3[] newPoints = new Vector3[segs + 1];

			float origLenCursor = 0;
			//float newLenCursor  = 0;

			float seglenTmp = seglen;

			float len_j = Vector3.Distance (points [0], points [1]);

			Vector3 lastnewPoint = points [0];
			newPoints [0] = points [0];
			newPoints [segs] = points [points.Length-1];
			int j = 0;
			int k = 1;
			while (j < points.Length-1) {



				if (seglenTmp > len_j) {
					origLenCursor += len_j;


					// advance j
					seglenTmp -= len_j;
					j++;

					if (j < points.Length-1)
						len_j = Vector3.Distance(points[j], points[j+1]);

					lastnewPoint = points [j]; 

					continue;
				} else {
					
					lastnewPoint = Vector3.Lerp(lastnewPoint,  points[j+1], seglenTmp/len_j);
					newPoints [k++] = lastnewPoint;
					len_j -= seglenTmp;
					seglenTmp = seglen;	
				}
			}

			return newPoints;

		}


		public void skinSpline (Vector3[] spline, int secSegs = 6)
		{

			if (ropeMesh == null) {
				ropeMesh = new Mesh ();
				ropeMesh.name = "ropeMesh";
			} else {
				ropeMesh.Clear ();
			}



			bool endCapA = true;
			bool endCapB = false;


			Vector3[] sectionSpline = Circle (ropeRadius, secSegs);



			int vertCount = spline.Length * sectionSpline.Length;
			if (endCapA)
				vertCount += sectionSpline.Length;
			if (endCapB)
				vertCount += sectionSpline.Length;

			// Assign bone weights to mesh
			boneWeights = new BoneWeight[vertCount];


			int capTriangleCount = 0;

			if (endCapA)
				capTriangleCount += sectionSpline.Length - 1;

			if (endCapB)
				capTriangleCount += sectionSpline.Length - 1;

			int quadCount = secSegs * (spline.Length - 1);

			int triangleCount = 3 * 2 * quadCount + 3 * capTriangleCount;

			Vector3[] 	vertices 	= new Vector3[vertCount];
			Vector2[] 	uvs 		= new Vector2[vertCount];
			int[] 		triangles 	= new int[triangleCount];



			int vertIndex = 0;
			int bid1 = 0;
			float infl = 1;

			// VERTS FOR EXTRUSION -- EACH RIB
			for (int i = 0; i < spline.Length; i++) {

				for (int j = 0; j < sectionSpline.Length; j++) {
					//vertIndex = ribVertIndex + j;
					vertices[vertIndex] = evenRibMatrices[i].MultiplyPoint3x4 (sectionSpline [j]);
					uvs [vertIndex] = new Vector2 (i * .8f, j * .3f);

					// assign all these to bone i

					if (i == 0) {
						bid1 = 0;
						infl = 1;
					} else if (i == spline.Length - 1) {
						bid1 = spline.Length - 2;
						infl = 1;
					}
					else  {
						bid1 = i ;

						infl = 1f;
					}



					boneWeights[vertIndex].boneIndex0 = bid1;
					boneWeights[vertIndex].weight0 = infl;

//					if (i > 0 && i < spline.Length - 1) {
//						boneWeights [vertIndex].boneIndex1 = bid2;
//						boneWeights [vertIndex].weight0 = infl;
//					}
					vertIndex++;
				}
			}

			// VERTS FOR CAPS
			int capAVertIndex = vertIndex;
			if (endCapA) {
				for (int j = 0; j < sectionSpline.Length; j++) {

					vertices[vertIndex] = evenRibMatrices[0].MultiplyPoint3x4 (sectionSpline [j]);
					uvs [vertIndex] = new Vector2 (sectionSpline [j].x * .8f, sectionSpline [j].y * .3f);

					// assign all these to bone 0
					boneWeights[vertIndex].boneIndex0 = 0;
					boneWeights[vertIndex].weight0 = 1;

					vertIndex++;
				}
			}
			// VERTS FOR CAPS
			int capBVertIndex = vertIndex;
			if (endCapB) {
				for (int j = 0; j < sectionSpline.Length; j++) {

					vertices[vertIndex] = evenRibMatrices[spline.Length-1].MultiplyPoint3x4 (sectionSpline [j]);
					uvs [vertIndex] = new Vector2 (sectionSpline [j].x * .8f, sectionSpline [j].y * .3f);

					// assign all these to bone 0
					boneWeights[vertIndex].boneIndex0 = spline.Length-2;
					boneWeights[vertIndex].weight0 = 1;

					vertIndex++;
				}
			}
				
//			foreach (BoneWeight bw in boneWeights) {
//				Debug.Log (bw.boneIndex0 + ", " + bw.boneIndex1);
//			}

			// TRIANGLES
			int triangleCursor = 0;
			for (int i = 1; i < spline.Length; i++) {

				int ribVertIndex 		=  i 	* sectionSpline.Length;
				int prevRibVertIndex 	= (i-1) * sectionSpline.Length;

				for (int j = 1; j <= sectionSpline.Length; j++) {

					int secj = j;
					int psecj = j - 1;

					if (j == sectionSpline.Length) {
						secj = 0;
						psecj = j-1;
					}
					
					int vi0 = prevRibVertIndex 	+ psecj;
					int vi1 = ribVertIndex 		+ psecj;
					int vi2 = ribVertIndex 		+ secj;
					int vi3 = prevRibVertIndex 	+ secj;

					triangles [triangleCursor++] = vi0;
					triangles [triangleCursor++] = vi1;
					triangles [triangleCursor++] = vi2;

					triangles [triangleCursor++] = vi0;
					triangles [triangleCursor++] = vi2;
					triangles [triangleCursor++] = vi3;
				}
			}
			int step = 1;
			if (endCapA) {
				for (int j = 0; j < sectionSpline.Length-2; j++) {
					triangles [triangleCursor++] = capAVertIndex;
					triangles [triangleCursor++] = capAVertIndex+step;
					triangles [triangleCursor++] = capAVertIndex+step+1;
					step++;
				}
			}
			step = 1;
			if (endCapB) {
				for (int j = 0; j < sectionSpline.Length-2; j++) {
					triangles [triangleCursor++] = capBVertIndex;
					triangles [triangleCursor++] = capBVertIndex+step+1;
					triangles [triangleCursor++] = capBVertIndex+step;
					step++;
				}
			}




			ropeMesh.vertices = vertices;
			ropeMesh.uv = uvs;
			ropeMesh.triangles = triangles;

			ropeMesh.RecalculateBounds ();
			ropeMesh.RecalculateNormals ();
			ropeMesh.RecalculateTangents ();



		}



		public void calculateEvenRibMatrices(Vector3[] spline)
		{
			if (spline == null || spline.Length == 0)
				return;
			
			evenRibMatrices = new Matrix4x4[spline.Length];

			Quaternion rot = Quaternion.identity;


			Vector3 v0, v1, tangent;

			Vector3 normal = Vector3.up;

			for (int i = 0; i < spline.Length; i++) {
				
				if (i == 0) {
					v0 = (spline [i] - spline [i + 1]);
					rot = Quaternion.LookRotation (v0, normal);// ((spline [i + 1] - spline [i]), Vector3.Cross (v0, v1));

				} else if (i == spline.Length - 1) {
					v0 = (spline [i - 1] - spline [i]);					
					rot = Quaternion.LookRotation (v0, normal);// ((spline [i + 1] - spline [i]), Vector3.Cross (v0, v1));

				} else {
					v0 = (spline [i] - spline [i - 1]);					
					v1 = (spline [i + 1] - spline [i]);
					tangent = v0.normalized + v1.normalized;
					rot = Quaternion.LookRotation (-tangent, normal);// ((spline [i + 1] - spline [i]), Vector3.Cross (v0, v1));

				}

				Matrix4x4 m = Matrix4x4.TRS (spline [i], rot, Vector3.one);
				evenRibMatrices [i] = m;
			}

		

		}


		public static Vector3[] Circle(float radius, int segs, bool rectify = false)
		{

			// VALIDATION: Assert at least 3 sides
			segs = Mathf.Max(segs, 3);

			Vector3[] circlePath = new Vector3[segs];

			float dang = 360f/((float)segs);
			// circle spline creation...

			for (int i = 0; i<segs; i++) {
				float rads = Mathf.Deg2Rad*(((float)i)*dang);

				//Debug.Log("rads = "+rads);

				circlePath[i] = new Vector3(radius*Mathf.Cos(rads), radius*Mathf.Sin(rads));;
			}
			return circlePath;

		}



	} // CLASS
}// NAMESPACE
