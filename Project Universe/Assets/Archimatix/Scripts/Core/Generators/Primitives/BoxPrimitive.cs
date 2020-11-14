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
	public class BoxPrimitive : Primitive, IPrimitiveGenerator
	{
		
		//public override string GeneratorHandlerTypeName { get { return "BoxHandler"; } }
		public override string GeneratorHandlerTypeName { get { return "GeneratorHandler3D"; } }

		// INPUTS
		public AXParameter P_Front;
		public AXParameter P_Back;
		public AXParameter P_Left;
		public AXParameter P_Right;
		public AXParameter P_Top;
		public AXParameter P_Bottom;




		public bool 	front 		= true;
		public bool 	back 		= true;
		public bool 	right 		= true;
		public bool 	left 		= true;
		public bool 	top 		= true;
		public bool 	bottom 		= true;



		// INIT EXTRUDE GENERATOR
		public override void init_parametricObject() 
		{
			
			// Parameters
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			AXParameter p = null;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeX", 1f, 0f, 5000f);
			p.sizeBindingAxis = Axis.X;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeY", 1f, 0f, 5000f);
			p.sizeBindingAxis = Axis.Y;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeZ", 1f, 0f, 5000f);
			p.sizeBindingAxis = Axis.Z;

			
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Front", 	true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Back", 	true);
			
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Left", 	true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Right", 	true);
			
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Top", 		true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Bottom", 	true);
			
			
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
			 

		}
		

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Front 	= parametricObject.getParameter("Front");
			P_Back 		= parametricObject.getParameter("Back");
			P_Right 	= parametricObject.getParameter("Right");
			P_Left 		= parametricObject.getParameter("Left");
			P_Top 		= parametricObject.getParameter("Top");
			P_Bottom 	= parametricObject.getParameter("Bottom");
		}


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			front 	= P_Front.boolval;
			back 	= P_Back.boolval;
			right 	= P_Right.boolval;
			left 	= P_Left.boolval;
			top 	= P_Top.boolval;
			bottom 	= P_Bottom.boolval;
		}


		// GENERATE BOX
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			preGenerate();
			

			AXBoxGenerator boxer = new AXBoxGenerator(sizeX, sizeY, sizeZ);

			boxer.front 	= front;
			boxer.back 		= back;
			boxer.left 		= left;
			boxer.right 	= right;
			boxer.top 		= top;
			boxer.bottom 	= bottom;

			Mesh mesh = new Mesh();

			boxer.generate(ref mesh, parametricObject.axTex);


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

			/*
				GameObject go =	parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, false);
				//Matrix4x4 tmx = parametricObject.getLocalMatrix();
				//go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				//go.transform.position 		= AXUtilities.GetPosition(tmx);
				//go.transform.localScale 	= parametricObject.getLocalScale();

				*/
				GameObject go =	parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);

				Matrix4x4 tmx = parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScale(); 



				return go;
			}
	
			
			//Debug.Log ("gen time="+sw.stop ());
			
			return null;
		}
	



	}

}
