using UnityEngine;

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
	
	
	
	
	// EXTRUDE GENERATOR
	public class PlanePrimitive : Primitive, IPrimitiveGenerator
	{
		
		//public override string GeneratorHandlerTypeName { get { return "BoxHandler"; } }
		public override string GeneratorHandlerTypeName { get { return "GeneratorHandler3D"; } }

		// INPUTS
		public AXParameter P_Top;
		public AXParameter P_Bottom;



		public bool 	top 		= true;
		public bool 	bottom 		= true;



		// INIT EXTRUDE GENERATOR
		public override void init_parametricObject() 
		{
			
			// Parameters
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			AXParameter p = null;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeX", 10f, 0f, 5000f);
			p.sizeBindingAxis = Axis.X;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeZ", 10f, 0f, 5000f);
			p.sizeBindingAxis = Axis.Z;

			
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Top", 		true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Bottom", 	false);

			
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
			 

		}
		

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Top 		= parametricObject.getParameter("Top");
			P_Bottom 	= parametricObject.getParameter("Bottom");
		}


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			top 	= P_Top.boolval;
			bottom 	= P_Bottom.boolval;
		}


		// GENERATE BOX
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			preGenerate();
			

			AXPlaneGenerator e = new AXPlaneGenerator();

			e.side_x 	= sizeX;
			e.side_y 	= sizeY;
			e.side_z 	= sizeZ;

			e.top 		= top;
			e.bottom 	= bottom;
			



			Mesh mesh = new Mesh();

			e.generate(ref mesh, parametricObject.axTex);
			
			
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();

			AXMesh tmpAXmesh;
			
			tmpAXmesh = new AXMesh( mesh, Matrix4x4.identity, parametricObject.axMat.mat);
			tmpAXmesh.name 				= parametricObject.Name;
			tmpAXmesh.hasContinuousUV 	= parametricObject.boolValue("hasContinuousUV");
			tmpAXmesh.makerPO 			= parametricObject;
			

			List<AXMesh> ax_meshes = new List<AXMesh>();
			ax_meshes.Add (tmpAXmesh);


			//parametricObject.finishSingleAXMeshAndOutput(tmpAXmesh, isReplica);
			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);

			setBoundaryFromAXMeshes(ax_meshes);

			if (makeGameObjects)
			{
				GameObject go =	parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, false);
				//Matrix4x4 tmx = parametricObject.getLocalMatrix();
				//go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				//go.transform.position 		= AXUtilities.GetPosition(tmx);
				//go.transform.localScale 	= parametricObject.getLocalScale();

				return go;
			}
	
			
			//Debug.Log ("gen time="+sw.stop ());
			
			return null;
		}
	



	}

}
