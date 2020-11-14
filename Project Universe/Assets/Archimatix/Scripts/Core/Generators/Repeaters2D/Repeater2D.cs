using UnityEngine;

using AXGeometry;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;



namespace AX.Generators
{

	public class Repeater2D : AX.Generators.Generator2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "Generator2DHandler"; } }



		// INPUTS
		public AXParameter			P_Node;
		public AXParameter			P_Cell;



		// WORKING FIELDS (Updated every Generate)

		public AXParameter 			nodeSrc_p;
		public AXParametricObject 	nodeSrc_po;
		
		public AXParameter 			cellSrc_p;
		public AXParametricObject 	cellSrc_po;

		public AXParameter 			progressiveRotation_p;
		public AXParameter 			zAxis_p;



		public Vector3 				translate;
		public Quaternion			rotation 		= Quaternion.identity;
		//public Vector3				jitterScale		= Vector3.zero;

		public float 				progressiveRotation;
		public bool 				zAxis;

		public float 				shiftU = 0;
		public float 				shiftV = 0;

	

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Node 					= parametricObject.getParameter("Node Shape");
			P_Cell 					= parametricObject.getParameter("Cell Shape");



			zAxis_p					= parametricObject.getParameter("zAxis");
			progressiveRotation_p 	= parametricObject.getParameter("Progressive Rotation");
						 
		}


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters() 
		{
			base.pollControlValuesFromParmeters();

			nodeSrc_p 		= (P_Node  		!= null && P_Node.DependsOn != null)  	? getUpstreamSourceParameter(P_Node)	: null;
			nodeSrc_po 		= (nodeSrc_p 	!= null)  	? nodeSrc_p.parametricObject										: null;

			cellSrc_p 		= (P_Cell  		!= null && P_Cell.DependsOn != null)  	? getUpstreamSourceParameter(P_Cell)	: null;
			cellSrc_po 		= (cellSrc_p 	!= null)  	? cellSrc_p.parametricObject										: null;


			if (zAxis_p != null)
				zAxis 				= zAxis_p.boolval;

			if (progressiveRotation_p != null)
				progressiveRotation = progressiveRotation_p.FloatVal;


		}


		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{
			//Debug.Log("INIT: ");

			if (from_p == null)// || from_p.DependsOn == null)
				return;

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;


			this_p.shapeState 	= src_p.shapeState;
			this_p.breakGeom	= src_p.breakGeom;
			this_p.breakNorm	= src_p.breakNorm;


			if (P_Output == null)
				P_Output = getPreferredOutputParameter();

			P_Output.shapeState = src_p.shapeState;

			switch(this_p.Name) { 
			case "Node Shape":
				nodeSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;
			case "Cell Shape":
				cellSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;

			case "Jitter Translation":
				jitterTranslationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Rotation":
				jitterRotationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Scaling":
				jitterScaleTool = src_p.parametricObject.generator as JitterTool;
				break;

			}
		}


		public virtual void initializeBays(string pName)
		{

		}


	}


}
