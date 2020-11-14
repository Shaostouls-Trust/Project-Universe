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

	public class GridRepeater2D : AX.Generators.Generator2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "GridRepeater2DHandler"; } }



		// INPUTS
		public AXParameter	P_Node;
		public AXParameter	P_Cell;

		//public AXParameter	P_JitterTranslation;
		//public AXParameter	P_JitterRotation;
		//public AXParameter	P_JitterScale;

		public AXParameter	P_RepeaterU;
		public AXParameter	P_RepeaterV;


		// WORKING FIELDS (Updated every Generate)

		public AXParameter nodeSrc_p;
		public AXParameter cellSrc_p;

		public AXParametricObject nodeSrc_po;
		public AXParametricObject cellSrc_po;

		public AXParameter progressiveRotation_p;
		public AXParameter zAxis_p;


		// JITTER (Generators are never serialized)
		//public JitterTool jitterTranslationTool;
		//public JitterTool jitterRotationTool;
		//public JitterTool jitterScaleTool;

		// REPEATER
		public RepeaterTool repeaterToolU;
		public RepeaterTool repeaterToolV;


		public Vector3 			translate;
		public Quaternion		rotation 		= Quaternion.identity;
		//public Vector3			jitterScale		= Vector3.zero;

		public float progressiveRotation;
		public bool zAxis;

		public float shiftU = 0;
		public float shiftV = 0;



		// INITIALIZE
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Node Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Cell Shape"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));

			initRepeaterTools();


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));

			parametricObject.addParameter(AXParameter.DataType.Float, "Progressive Rotation", 0f); 
		}

		public virtual void initRepeaterTools() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterU"));
			AXParametricObject repeaterToolU =  parametricObject.model.createNode("RepeaterTool");
			repeaterToolU.rect.x = parametricObject.rect.x-200;
			repeaterToolU.isOpen = false;
			repeaterToolU.intValue("Edge_Count", 50);
			parametricObject.getParameter("RepeaterU").makeDependentOn(repeaterToolU.getParameter("Output"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterV"));
			AXParametricObject repeaterToolV =  parametricObject.model.createNode("RepeaterTool");
			repeaterToolV.rect.x = parametricObject.rect.x-200;
			repeaterToolV.isOpen = false;
			repeaterToolV.intValue("Edge_Count", 50);
			parametricObject.getParameter("RepeaterV").makeDependentOn(repeaterToolV.getParameter("Output"));
		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Node 					= parametricObject.getParameter("Node Shape");
			P_Cell 					= parametricObject.getParameter("Cell Shape");


			// JITTER 
			P_JitterTranslation 	= parametricObject.getParameter("Jitter Translation");
			P_JitterRotation 		= parametricObject.getParameter("Jitter Rotation");
			P_JitterScale 			= parametricObject.getParameter("Jitter Scale");

			// REPEATERS
			P_RepeaterU 			=  parametricObject.getParameter("RepeaterU");
			P_RepeaterV 			=  parametricObject.getParameter("RepeaterV");


			zAxis_p					= parametricObject.getParameter("zAxis");
			progressiveRotation_p 	= parametricObject.getParameter("Progressive Rotation");



			setupRepeaters();

			pollControlValuesFromParmeters();
			 
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters() 
		{
			base.pollControlValuesFromParmeters();

			nodeSrc_p 		= (P_Node  		!= null && P_Node.DependsOn != null)  	? getUpstreamSourceParameter(P_Node)	: null;
			nodeSrc_po 		= (nodeSrc_p 	!= null)  								? nodeSrc_p.parametricObject			: null;

			cellSrc_p 	= (P_Cell  			!= null && P_Cell.DependsOn != null)  	? getUpstreamSourceParameter(P_Cell)	: null;
			cellSrc_po 	= (cellSrc_p  		!= null)								? cellSrc_p.parametricObject			: null;

			jitterTranslationTool 	= ( P_JitterTranslation != null && P_JitterTranslation.DependsOn != null) 	? getUpstreamSourceParameter(P_JitterTranslation).parametricObject.generator 	as JitterTool	: null;
			jitterRotationTool 		= ( P_JitterRotation != null 	&& P_JitterRotation.DependsOn != null) 		? getUpstreamSourceParameter(P_JitterRotation).parametricObject.generator 		as JitterTool	: null;
			jitterScaleTool 		= ( P_JitterScale != null 		&& P_JitterScale.DependsOn != null) 		? getUpstreamSourceParameter(P_JitterScale).parametricObject.generator 			as JitterTool	: null;

			if (zAxis_p != null)
				zAxis 				= zAxis_p.boolval;

			if (progressiveRotation_p != null)
				progressiveRotation = progressiveRotation_p.FloatVal;


		}



		public void setupRepeaters()
		{
			AXParameter input_pU = null;
			AXParameter input_pV = null;

			if (zAxis_p == null || ! zAxis_p.boolval)
			{
				input_pU = P_RepeaterU;
				input_pV = P_RepeaterV;
			}
			else
			{
				input_pV = P_RepeaterU;
				input_pU = P_RepeaterV;
			}

			if (input_pU != null)
				repeaterToolU = (input_pU != null && input_pU.DependsOn != null) ? input_pU.DependsOn.parametricObject.generator as RepeaterTool	: null;
			else 
				repeaterToolU = null;

			if (input_pV != null)
				repeaterToolV = (input_pV != null && input_pV.DependsOn != null) ? input_pV.DependsOn.parametricObject.generator as RepeaterTool	: null;
			else 
				repeaterToolV = null;

		}





		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			if (from_p == null)// || from_p.DependsOn == null)
				return;


			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;


			base.connectionMadeWith(to_p, from_p);



			this_p.shapeState 	= src_p.shapeState;
			this_p.breakGeom	= src_p.breakGeom;
			this_p.breakNorm	= src_p.breakNorm;


			if (P_Output == null)
			if (P_Output == null)
				P_Output = parametricObject.getParameter("Output Shape");

			//Debug.Log("OUT: " + P_Output);
			//P_Output.shapeState = src_p.shapeState;


			if (repeaterToolU == null || repeaterToolV == null)
				return;


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

			case "RepeaterU":
				repeaterToolU = src_p.parametricObject.generator as RepeaterTool;
				break;
			case "RepeaterV":
				repeaterToolV = from_p.parametricObject.generator as RepeaterTool;
				break;
			}
		}

		public virtual void initializeBays(string pName)
		{
			if (parametricObject.isInitialized)
				return;

			parametricObject.isInitialized = true;

			switch(pName)
			{
			case "Node Shape":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Abs(nodeSrc_p.parametricObject.bounds.size.x)*1.5f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Abs(nodeSrc_p.parametricObject.bounds.size.y)*1.5f);
				break;
			case "Cell Shape":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(  Mathf.Abs(cellSrc_p.parametricObject.bounds.size.x)*1.1f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(  Mathf.Abs(cellSrc_p.parametricObject.bounds.size.y)*1.1f);
				break;
			}
		}





		// SHAPE_REPEATER_2D :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if ( (P_Node == null ||  inputSrc_p == null) &&  (P_Cell == null ||  cellSrc_p == null))
			{
				if (P_Output != null)
				{
					P_Output.paths = null;
					P_Output.polyTree = null;
				}
				return null;
			}

			if (repeaterToolU == null || repeaterToolV == null)
				return null;

			// PRE_GENERATE
			preGenerate();

           //Debug.Log(parametricObject.Name + " :: " + parametricObject.code);
            if (parametricObject.code != null)
                parametricObject.executeCodeBloc(new List<string>(parametricObject.code.Split("\n"[0])), null);


            AXParameter P_nodeOutput = new AXParameter ();
			P_nodeOutput.parametricObject = parametricObject;


			AXParameter P_cellOutput = new AXParameter ();
			P_cellOutput.parametricObject = parametricObject;
				


			// PROCESS NODE INPUT

			if (inputSrc_p != null)
			{
				P_Node.polyTree = null;
				AXShape.thickenAndOffset(ref P_Node, inputSrc_p);

				//P_nodeOutput.polyTree = null;
				//AXShape.thickenAndOffset(ref P_nodeOutput, P_Node);
			}

			if (cellSrc_p != null)
			{
				P_Cell.polyTree = null;
				AXShape.thickenAndOffset(ref P_Cell, cellSrc_p);

				//P_cellOutput.polyTree = null;
				//AXShape.thickenAndOffset(ref P_cellOutput, P_Cell);

			}
			 



			bool doPolyTreeNodes = false;
			bool doPolyTreeCells = false;


			// NODE
			Matrix4x4 tm = Matrix4x4.identity;

			Paths tmpPaths = null;

			Clipper clipper = null;


			if (nodeSrc_p != null)
			{

				if (P_Node.polyTree != null)
				{
					tmpPaths = AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_Node.polyTree));
					doPolyTreeNodes = true;
					clipper =  new Clipper(Clipper.ioPreserveCollinear);
				}
				else
				{
					tmpPaths = P_Node.getClonePaths();
					P_nodeOutput.paths = new Paths();
				} 

				//Debug.Log("tmpPaths="+tmpPaths.Count);
				if (tmpPaths != null && tmpPaths.Count > 0)
				{

					for (int i=0; i<=repeaterToolU.cells; i++)
					{
						for (int j=0; j<=repeaterToolV.cells; j++) 
						{


							if ( (i<=repeaterToolU.edgeCount || i>=repeaterToolU.cells-repeaterToolU.edgeCount) || (j<=repeaterToolV.edgeCount || j>=repeaterToolV.cells-repeaterToolV.edgeCount) )
							{


								//tm = Matrix4x4.TRS(new Vector3(2*i-2, 2*j-2, 0), Quaternion.identity, Vector3.one);
								tm = localMatrixFromAddress(RepeaterItem.Node, i, j);


								if (doPolyTreeNodes)
								{
									clipper.AddPaths ( AX.Generators.Generator2D.transformPaths(tmpPaths, tm), PolyType.ptSubject, true);
					 			}
								else 
								{
									Paths tmp = AX.Generators.Generator2D.transformPaths(tmpPaths, tm);


									P_nodeOutput.paths.AddRange(tmp);
									
								} 
							}
						}
					}

					if (doPolyTreeNodes)
					{	
						P_nodeOutput.polyTree = new AXClipperLib.PolyTree();
						clipper.Execute(ClipType.ctDifference, 	P_nodeOutput.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

					}

				}


			}



			// CELL
			// PROCESS CELL INPUT

			if (cellSrc_p != null)
			{
				if (P_Cell.polyTree != null)
				{
					tmpPaths = AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_Cell.polyTree));
					doPolyTreeCells = true;
					clipper =  new Clipper(Clipper.ioPreserveCollinear);
				}
				else
				{
					tmpPaths = P_Cell.getClonePaths();
					P_cellOutput.paths = new Paths();
				}


				if (tmpPaths != null && tmpPaths.Count > 0)
				{

					for (int i=0; i<repeaterToolU.cells; i++)
					{
						for (int j=0; j<repeaterToolV.cells; j++) 
						{
							tm = localMatrixFromAddress(RepeaterItem.Cell, i, j);

							if (doPolyTreeCells)
								clipper.AddPaths ( AX.Generators.Generator2D.transformPaths(tmpPaths, tm), PolyType.ptSubject, true);
							else 
							{
								Paths tmp = AX.Generators.Generator2D.transformPaths(tmpPaths, tm);
								P_cellOutput.paths.AddRange(tmp);
							} 
						}
					}

					if (doPolyTreeCells)
					{	
						P_cellOutput.polyTree = new AXClipperLib.PolyTree();
						clipper.Execute(ClipType.ctDifference, 	P_cellOutput.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);
					}
				}

			}

			/*
			if ( P_Output.polyTree != null)
			{
				transformPolyTree(P_Output.polyTree, localMatrix);
			}
			else if (P_Output.paths != null)
			{
				P_Output.paths = transformPaths(P_Output.paths, localMatrix);
				P_Output.transformedControlPaths = P_Output.paths;
			}
			*/

			P_Output.polyTree = null;

			if (nodeSrc_p != null && cellSrc_p == null)
			{
				// JUST NODES
				AXShape.thickenAndOffset(ref P_Output, P_nodeOutput);

			}

			else if (nodeSrc_p == null && cellSrc_p != null)
			{
				// JUST CELLS
				AXShape.thickenAndOffset(ref P_Output, P_cellOutput);

			}
			else
			{
				// BOTH TO COMBINE
				clipper =  new Clipper(Clipper.ioPreserveCollinear);

				if (P_nodeOutput.polyTree == null && P_nodeOutput.paths != null)
					clipper.AddPaths(P_nodeOutput.paths, PolyType.ptSubject, true);
				else if (P_nodeOutput.polyTree != null)
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_nodeOutput.polyTree)), PolyType.ptSubject, true);
				
				if (P_cellOutput.polyTree == null && P_cellOutput.paths != null)
					clipper.AddPaths(P_cellOutput.paths, PolyType.ptSubject, true);
				else if (P_cellOutput.polyTree != null)
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_cellOutput.polyTree)), PolyType.ptSubject, true);


				P_Output.polyTree = new AXClipperLib.PolyTree();
				clipper.Execute(ClipType.ctUnion, 	P_Output.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

				AXShape.thickenAndOffset(ref P_Output, P_Output);
			}


			if ( P_Output.polyTree != null)
			{
				transformPolyTree(P_Output.polyTree, localMatrix);
			}
			else if (P_nodeOutput.paths != null)
			{
				P_Output.paths 						= transformPaths(P_nodeOutput.paths, localMatrix);
				P_Output.transformedControlPaths 	= P_nodeOutput.paths;
			}
			else if (P_cellOutput.paths != null)
			{
				P_Output.paths 						= transformPaths(P_cellOutput.paths, localMatrix);
				P_Output.transformedControlPaths 	= P_cellOutput.paths;
			}
			//base.generate(false, initiator_po, isReplica);
			calculateBounds();

			return null;


		} // \generate






		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input)
		{

			if (input == nodeSrc_po)
				return localMatrixFromAddress(RepeaterItem.Node, 0, 0);
			
			else if (input == cellSrc_po)
				return localMatrixFromAddress(RepeaterItem.Cell, 0, 0);

			return Matrix4x4.identity;

		}




		public virtual Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0)
		{

			if (repeaterToolU == null || repeaterToolV == null)
				return Matrix4x4.identity;
			
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;


			float actualBayU = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			float actualBayV = (repeaterToolV != null) ? repeaterToolV.actualBay : 0;


			shiftU = -repeaterToolU.cells * actualBayU / 2;
			shiftV = -repeaterToolV.cells * actualBayV / 2;


			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;

			//Debug.Log(shiftVV);

												   // NODE				   //CELL
			float x = (ri == RepeaterItem.Node) ? (i*actualBayU+shiftUU) : (i*actualBayU+shiftUU+actualBayU/2);
			float y = (ri == RepeaterItem.Node) ? (j*actualBayV+shiftVV) : ((j>-1) ? (j*actualBayV+shiftVV+actualBayV/2) : 0);




			if (jitterTranslationTool 	!= null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	y * jitterTranslationTool.perlinScale);

			if (jitterRotationTool 		!= null)
				perlinRot 			=  Mathf.PerlinNoise( x * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			
			if (jitterScaleTool 		!= null)
				perlinScale 		=  Mathf.PerlinNoise( (x+jitterScaleTool.offset) * jitterScaleTool.perlinScale,  		(y+jitterScaleTool.offset) * jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3(x, y, 0);
			if (jitterTranslationTool 	!= null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, 0);


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, 0, 0);
			if (jitterRotationTool 		!= null)
				rotation = Quaternion.Euler (0, 0, (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2));




			// SCALE		**********
			jitterScale = new Vector3 (scaleX, scaleY, 1);

			if (jitterScaleTool != null)
			{
				// X
				if (jitterScaleTool.x < scaleX)
					jitterScale.x = jitterScale.x + perlinScale * jitterScaleTool.x - jitterScaleTool.x / 2;
				else
					jitterScale.x = jitterScale.x/2 +perlinScale * jitterScaleTool.x;

				// Y
				if (jitterScaleTool.y < scaleY)
					jitterScale.y = jitterScale.y + perlinScale * jitterScaleTool.y - jitterScaleTool.y / 2;
				else
					jitterScale.y = jitterScale.y/2 +perlinScale * jitterScaleTool.y;

			}

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, (i + j) * progressiveRotation), Vector3.one);

		}

	}

}