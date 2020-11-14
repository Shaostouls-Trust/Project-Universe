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
	public class Lofter : Mesher, IMeshGenerator, ICustomNode
	{
		public override string GeneratorHandlerTypeName { get { return "LofterHandler"; } }


		public List<AXParameter> inputs;

		List <int> shiftIndices = new List<int>();

		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.useSplineInputs = true;
			parametricObject.splineInputs = new List<AXParameter>();
			P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			P_Output.hasInputSocket = false;
			P_Output.shapeState = ShapeState.Open;
		}




		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			inputs = parametricObject.getAllInputSplineParameters();

		}

		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (inputs != null && inputs.Count > 0)
			{



			
				float bayw = 2f;

				List <Vector3> verts = new List<Vector3>();
				List <Vector2>   uvs = new List<Vector2>();

				List <int>		tris = new List<int>();

				shiftIndices.Add(0);


				Paths paths = new Paths();
				Path path = null;

				int prevPathCount = 0;


				for (int c=0; c<inputs.Count; c++)
				{
					if (inputs[c].DependsOn == null)
						continue;

					path = inputs[c].DependsOn.getPaths()[0];

					paths.Add(path);
					
					verts.AddRange(AXGeometry.Utilities.path2Vec3s(path, AXGeometry.Axis.Y, (c*bayw)));


					if (c>0)
						shiftIndices.Add(shiftIndices[shiftIndices.Count-1] + prevPathCount);

					prevPathCount = path.Count;

				}

				for (int u=0; u<verts.Count; u++)
				{
					uvs.Add(Vector2.zero);
				}




				for (int a=0; a<inputs.Count-1; a++)
				{
					int b = a+1;

	//				foreach (Vector3 vert in verts)
	//					Debug.Log(vert);
					if (inputs[a].DependsOn == null)
					{
						a++;
						continue;
					}
					if (inputs[b].DependsOn == null)
						return null;
					Path pathA = inputs[a].DependsOn.getPaths()[0];
					Path pathB = inputs[b].DependsOn.getPaths()[0];

					int av = 0;
					int bv = 0;


					// Step through...
					// each loop is a triangle

					int gov = 0;
					while (av < pathA.Count-1)
					{
						if (gov > 25)
							break;

						// add the first two points of the triangle
						tris.Add(getIndex(a, av));
						tris.Add(getIndex(b, bv));

						IntPoint aa1 = pathA[av];
						IntPoint aa2 = pathA[av+1];

						IntPoint bb1 = pathB[bv];
						IntPoint bb2 = pathB[bv+1];



						//Debug.Log(aa1.X + ", " + aa1.Y + "  --- " + bb1.X + ", " + bb1.Y);

						// if dist(aa1, bb1) < dist(bb1, aa2), uadd the former to the triabgle.
						if (Pather.DistanceSquared(aa1, bb2) < Pather.DistanceSquared(bb1, aa2))
						{
							//Debug.Log("use bb2");

							bv++;
							tris.Add(getIndex(b, bv));
						}
						else
						{
							//Debug.Log("use aa2");
							av++;
							tris.Add(getIndex(a, av));
						}

						gov++;
					}

				}

//				foreach (int t in tris)
//					Debug.Log(t);


				// create mesh and GameObject
				Mesh mesh = new Mesh();

				mesh.vertices = verts.ToArray();
				mesh.triangles = tris.ToArray();
				mesh.uv 		= uvs.ToArray();


				mesh.RecalculateNormals();
				mesh.RecalculateTangents();


				AXMesh axmesh = new AXMesh(mesh);
				ax_meshes = new List<AXMesh>();
				ax_meshes.Add(axmesh);

				// FINISH AX_MESHES

				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, true);//renderToOutputParameter);

				// FINISH BOUNDING

				setBoundaryFromAXMeshes(ax_meshes);


				if (makeGameObjects)
					return parametricObject.makeGameObjectsFromAXMeshes (ax_meshes);

			}



			return null;
		}

		public int getIndex(int pathid, int vertid)
		{
			return shiftIndices[pathid] + vertid;
		}






		// GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
		{
			return Matrix4x4.TRS(new Vector3(0, -20, 0), Quaternion.Euler(90, 0, 0), Vector3.one);

		}
//			Debug.Log ("input_po.Name=" + input_po.Name);// + " -- " + endCapHandleTransform);
//


			// PLAN
//			if (input_po == planSrc_po)
//			{
//				if (P_Plan.flipX)
//						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1,1,1));
//					else
//						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
//
//				
//			}
//			// SECTION
//			if (input_po == sectionSrc_po)
//			{
//				//Debug.Log("??? " + sectionHandleTransform);
//				if (P_Section.flipX)
//				{
//					return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1,1,1));
//				}
//				else
//				{
//					if (P_Plan.shapeState == ShapeState.Open)
//						return endCapHandleTransform;
//					else
//						return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
//				}
//			}
//			// ENDCAP
//			if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.Parent)
//				return endCapHandleTransform;
//							
//			return Matrix4x4.identity ;
//		}
//
	}
}
