// http://www.archimatix.com

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



namespace AX.Generators
{
	


	// POLYGON GENERATOR
	public class Polygon : Mesher, IMeshGenerator
	{
		// This is really an extrude with no sides or depth.
		// later mak it individual and have an Extrude use it to differentiate the caps!
		public override string GeneratorHandlerTypeName { get { return "PolygonHandler"; } }

		// INPUTS

		// INIT
		public override void init_parametricObject() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Top Cap", 		true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Bottom Cap", 	true);

            base.init_parametricObject();

            parametricObject.boolValue("Bottom Cap", false);


		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan = parametricObject.getParameter("Input Shape");
		}

		public override AXParameter getPreferredInputParameter()
		{			
			return P_Plan;
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

		}




		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{
				
			case "Input Shape":
				planSrc_po = null;
				P_Output.paths = null; 
				P_Output.polyTree = null;
				P_Output.meshes = null;
				break;

			}

		}

		





		// GENERATE POLYGON
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (parametricObject == null || ! parametricObject.isActive || planSrc_po == null)
				return null;

			//Debug.Log ("===> [" + parametricObject.Name + "] EXTRUDE generate ... MAKE_GAME_OBJECTS="+makeGameObjects);

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();



			preGenerate();

			P_Plan.polyTree = null;
			
			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);



			GameObject retGO =   generateFirstPass(initiator_po, makeGameObjects, P_Plan, null, Matrix4x4.identity, renderToOutputParameter);


			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);


			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			return retGO;
		}
		
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input, AXParameter input_p=null)
		{
			if (planSrc_po != null)
			{
				if (input == planSrc_po)
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	}
}