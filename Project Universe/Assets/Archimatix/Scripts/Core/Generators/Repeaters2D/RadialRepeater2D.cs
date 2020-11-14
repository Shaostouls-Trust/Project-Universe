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

	public class RadialRepeater2D : Repeater2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "RadialRepeater2DHandler"; } }



		// INPUTS
		public AXParameter	P_RepeaterU;


		// WORKING FIELDS (Updated every Generate)

		public RadialRepeaterTool repeaterToolU;



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
			AXParametricObject repeaterToolU =  parametricObject.model.createNode("RadialRepeaterTool");
			repeaterToolU.rect.x = parametricObject.rect.x-200;
			repeaterToolU.isOpen = false;
			parametricObject.getParameter("RepeaterU").makeDependentOn(repeaterToolU.getParameter("Output"));


		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();


			// REPEATERS
			P_RepeaterU 			=  parametricObject.getParameter("RepeaterU");

			setupRepeaters();

			pollControlValuesFromParmeters();
			 
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters() 
		{
			base.pollControlValuesFromParmeters();


		}



		public void setupRepeaters()
		{
			AXParameter input_pU = null;

			if (zAxis_p == null || ! zAxis_p.boolval)
			{
				input_pU = P_RepeaterU;
			}

			if (input_pU != null)
				repeaterToolU = (input_pU != null && input_pU.DependsOn != null) ? input_pU.DependsOn.parametricObject.generator as RadialRepeaterTool	: null;
			else 
				repeaterToolU = null;


		}





		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			if (from_p == null)// || from_p.DependsOn == null)
				return;

			if (repeaterToolU == null)
				return;
				
			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;




			base.connectionMadeWith(to_p, from_p);

//			this_p.shapeState 	= src_p.shapeState;
//			this_p.breakGeom	= src_p.breakGeom;
//			this_p.breakNorm	= src_p.breakNorm;
//
//			P_Output.shapeState = src_p.shapeState;


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
				repeaterToolU = src_p.parametricObject.generator as RadialRepeaterTool;
				break;
			
			}
		}

		public override void initializeBays(string pName)
		{
			if (parametricObject.isInitialized)
				return;

			parametricObject.isInitialized = true;

			RadialRepeaterTool gener = repeaterToolU as RadialRepeaterTool;

			switch(pName)
			{
			case "Node Shape":
				if (repeaterToolU != null)
					gener.radius = 3.5f * nodeSrc_p.parametricObject.bounds.size.x ;
				break;
			case "Cell Shape":
				if (repeaterToolU != null)
					gener.radius = 2.5f * cellSrc_p.parametricObject.bounds.size.x ;
				break;
			}
		}





		// SHAPE_REPEATER_2D :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if ( (P_Node == null ||  inputSrc_p == null) &&  (P_Cell == null ||  cellSrc_p == null))
			{
				if (P_Output != null)
				{
					P_Output.paths = null;
					P_Output.polyTree = null;
				}
				return null;
			}

			if (repeaterToolU == null)
				return null;

			// PRE_GENERATE
			preGenerate();



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
				

				if (P_Node.polyTree != null && P_Output.shapeState == ShapeState.Closed)
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



				if (tmpPaths != null && tmpPaths.Count > 0)
				{
					for (int i=0; i<=repeaterToolU.cells; i++)
					{

						//tm = Matrix4x4.TRS(new Vector3(2*i-2, 2*j-2, 0), Quaternion.identity, Vector3.one);
						tm = localMatrixFromAddress(RepeaterItem.Node, i);


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
						tm = localMatrixFromAddress(RepeaterItem.Cell, i);

						if (doPolyTreeCells)
							clipper.AddPaths ( AX.Generators.Generator2D.transformPaths(tmpPaths, tm), PolyType.ptSubject, true);
						else 
						{
							Paths tmp = AX.Generators.Generator2D.transformPaths(tmpPaths, tm);
							P_cellOutput.paths.AddRange(tmp);
						} 
						
					}

					if (doPolyTreeCells)
					{	
						P_cellOutput.polyTree = new AXClipperLib.PolyTree();
						clipper.Execute(ClipType.ctDifference, 	P_cellOutput.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);
					}
				}

			}



			P_Output.polyTree = null;

			if (nodeSrc_p != null && (cellSrc_p == null || (P_cellOutput.paths == null && P_cellOutput.polyTree == null)))
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

				if (P_nodeOutput.polyTree == null)
					clipper.AddPaths(P_nodeOutput.paths, PolyType.ptSubject, true);
				else
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_nodeOutput.polyTree)), PolyType.ptSubject, true);
				
				if (P_cellOutput.polyTree == null)
					clipper.AddPaths(P_cellOutput.paths, PolyType.ptSubject, true);
				else
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_cellOutput.polyTree)), PolyType.ptSubject, true);


				P_Output.polyTree = new AXClipperLib.PolyTree();
				clipper.Execute(ClipType.ctUnion, 	P_Output.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

				AXShape.thickenAndOffset(ref P_Output, P_Output);
			}


			if ( P_Output.polyTree != null)
				transformPolyTree(P_Output.polyTree, localMatrix);
			else if (P_nodeOutput.paths != null)
			{
				P_Output.paths 						= transformPaths(P_nodeOutput.paths, localMatrix);
				P_Output.transformedControlPaths 	= P_nodeOutput.paths;
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

			if (repeaterToolU == null)
				return Matrix4x4.identity;

			RadialRepeaterTool gener = repeaterToolU as RadialRepeaterTool;

			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;


			float actualSectorAngle = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			    

			float dang = (ri == RepeaterItem.Node) ? (i*actualSectorAngle) : (i*actualSectorAngle + actualSectorAngle/2);

			// VALIDATION
			if (float.IsNaN(dang) || float.IsInfinity(dang))
				return Matrix4x4.identity;
				 
			Matrix4x4 radialDispl = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler (0, 0, dang), Vector3.one) * Matrix4x4.TRS(new Vector3(gener.radius, 0, 0), Quaternion.Euler(0, 0, -90), Vector3.one);

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (i*actualSectorAngle) * jitterTranslationTool.perlinScale,  	0);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (i*actualSectorAngle) * jitterRotationTool.perlinScale,  		0);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (i*actualSectorAngle) * jitterScaleTool.perlinScale,  		0);

				 

			// TRANSLATION 	*********
			Vector3 translate = new Vector3(0, 0, 0);
			if (jitterTranslationTool 	!= null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, 0);


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, 0, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;




			// SCALE		**********
			jitterScale = Vector3.zero;

			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, perlinScale*jitterScaleTool.z-jitterScaleTool.z/2);

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			Matrix4x4 dm =  Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) *radialDispl * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, i * progressiveRotation), Vector3.one)  ;

			return dm;
		}

	}

}