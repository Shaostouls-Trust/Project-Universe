using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
	public class PairRepeater2D : Repeater2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "PairRepeater2DHandler"; } }

		// INPUTS

		public AXParameter	P_Separation;
		public AXParameter	P_Symmetrical;


		// WORKING FIELDS (Updated every Generate)


		public float 				separation = 3;
		public bool 				symmetrical = true;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Node Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Cell Shape"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));


			P_Separation = parametricObject.addParameter(AXParameter.DataType.Float , 	"Separation", 		.5f, 0f, 2000f);


			parametricObject.addParameter(AXParameter.DataType.Bool, 	"zAxis", false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Symmetrical", true);

			parametricObject.addParameter(AXParameter.DataType.Float, "Progressive Rotation", 0f); 

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Cell 					= parametricObject.getParameter("Cell Shape");

			P_Separation 			= parametricObject.getParameter("Separation", "separation");
			P_Symmetrical 			= parametricObject.getParameter("Symmetrical", "symetrical");

			pollControlValuesFromParmeters();			 
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters() 
		{
			base.pollControlValuesFromParmeters();

			cellSrc_p 		= (P_Cell  		!= null && P_Cell.DependsOn != null)  	? getUpstreamSourceParameter(P_Cell)		: null;
			cellSrc_po 		= (cellSrc_p 	!= null)  								? cellSrc_p.parametricObject				: null;

			separation	= (P_Separation != null) 	? P_Separation.FloatVal : 3;
			symmetrical = (P_Symmetrical != null) 	? P_Symmetrical.boolval : true;

		}



		public override void initializeBays(string pName)
		{
			if (parametricObject.isInitialized)
				return;

			parametricObject.isInitialized = true;

			switch(pName)
			{
			case "Node Shape":
				if (nodeSrc_p != null)
				{
					separation = nodeSrc_p.parametricObject.bounds.size.x * 1.5f;
					P_Separation.FloatVal = separation;
				}
				break;

			}
		}





		// PAIR_REPEATER_2D :: GENERATE
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


				if (tmpPaths != null && tmpPaths.Count > 0)
				{

					for (int i=0; i<=1; i++)
					{

						//tm = Matrix4x4.TRS(new Vector3(2*i-2, 2*j-2, 0), Quaternion.identity, Vector3.one);
						tm = localMatrixFromAddress(RepeaterItem.Node, i);


						if (doPolyTreeNodes)
						{
							Paths ps = AX.Generators.Generator2D.transformPaths(tmpPaths, tm);
							if (i == 1)
								ps.Reverse();
							clipper.AddPaths ( ps, PolyType.ptSubject, true);
						}
						else 
						{

							Paths tmp = AX.Generators.Generator2D.transformPaths(tmpPaths, tm);
							if (i == 1)
								tmp.Reverse();

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



				for (int i=0; i<=1; i++)
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


			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;





			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			// NODE				   //CELL
			float x = (ri == RepeaterItem.Node) ? -separation/2 + (i+j)*separation : 0;
			float y = 0;

			if (zAxis)
			{
				y = x;
				x = 0;
			}


			if (jitterTranslationTool 	!= null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	y * jitterTranslationTool.perlinScale);

			if (jitterRotationTool 		!= null)
				perlinRot 			=  Mathf.PerlinNoise( x * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			if (jitterScaleTool 		!= null)
				perlinScale 		=  Mathf.PerlinNoise( x * jitterScaleTool.perlinScale,  		y * jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3(x, y, 0);
			if (jitterTranslationTool 	!= null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, 0);


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, 0, 0);
			if (jitterRotationTool 		!= null)
				rotation = Quaternion.Euler (0, 0, (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2));




			// SCALE		**********

			jitterScale = new Vector3 (((i==1 && symmetrical)? -scaleX : scaleX), ((j==1 && symmetrical)? -scaleY : scaleY), 1);

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