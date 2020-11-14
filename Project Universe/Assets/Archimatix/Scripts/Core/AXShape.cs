using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;



using AXGeometry;


using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;
 

namespace AX {

	[System.Serializable]
	public class AXShape : AXNode 
	{
	
		/* AN AXShape has a List of parameters of type spline and 
		 * has methods for combining these splines' Paths
		 * 
		 * It is like a ParametricObject, but does not serve as its own node in the graph
		 * and it does not need all the additionall parameters and handles.
		 */
		
		
		
		// CLIPPER PARAMETERS
		public enum CombineType {Difference, DifferenceRail, Intersection, IntersectionRail, Union, Grouped};
		public CombineType combineType = CombineType.Difference;
		
		
		// INPUTS
		public List<AXParameter> inputs;
	
		// OUTPUTS
		// These need to be serialized because 
		// of the subparameter values and relations stored in each.
		// However, the Paths and PolyTrees will not be serialized
		public AXParameter difference;
		public AXParameter differenceRail;
		public AXParameter intersection;
		public AXParameter intersectionRail;
		public AXParameter union;
		public AXParameter grouped; // no clipper
		
		
		public float geometryBreakAngle = 60;
		public float normalBreakAngle 	= 60;
	
		// similar to Parameter - a kind of eta parameter with a ParametricObject as a parent
		[System.NonSerialized]
		private AXParametricObject		parent				= null;
		public 	AXParametricObject		Parent   
		{
			get  { return parent; }
			set  { parent = value; }
		}


		public bool outputParametersOpen = true;

		//public static double cleanPolygonPrecision = 2;
		public static int cleanPolygonPrecision = 2;


	
		public AXShape(AXParametricObject po, string n) : base(n)
		{
			
			parametricObject = po;
			ParentNode 		= po;
			Parent 			= po;
			
			
			if (inputs == null)
				inputs = new List<AXParameter>();
	
			
			isOpen = false;
			
			if (difference == null)
				initOutputs ();
		}


		public AXShape(string n) : base(n)
		{
			// done!
			
		}
	
	
		// A shape can have any number of input shapes.
		// These shapes will be combined during the generation cycle
		// According to 
		public AXParameter addInput()
		{
			if (inputs == null)
				inputs = new List<AXParameter>();

			foreach (AXParameter input_p in inputs)
			{
				if (input_p.DependsOn == null)
					return input_p;
			}
			// NEW SPLINE PARAMETER 
			AXParameter input = createSplineParameter(AXParameter.ParameterType.Input, "Input Shape");
			
			inputs.Add(input);


			//parametricObject.assertInputControls();
			//parametricObject.inputControls.addChild(input);

			input.ParentNode = this;

			return input;
		} 
		
		public void initOutputs()
		{
			difference 			= createSplineParameter(AXParameter.ParameterType.Output, "Difference");
						
			differenceRail 		= createSplineParameter(AXParameter.ParameterType.Output, "DifferenceRail");
			differenceRail.shapeState = ShapeState.Open;
			
			intersection		= createSplineParameter(AXParameter.ParameterType.Output, "Intersection");
			
			intersectionRail	= createSplineParameter(AXParameter.ParameterType.Output, "IntersectionRail");
			intersectionRail.shapeState = ShapeState.Open;
			
			union 				= createSplineParameter(AXParameter.ParameterType.Output, "Union");
			
			grouped 			= createSplineParameter(AXParameter.ParameterType.Output, "Grouped");
			
		}
	
		public AXParameter createSplineParameter(AXParameter.ParameterType ptype, string name)
		{
			AXParameter p 	= new AXParameter();
			
			p.Type 			= AXParameter.DataType.Spline;
			p.PType 		= ptype;
			p.Name 			= name;
			
			p.Parent 			= Parent;
			p.parametricObject 	= Parent;
			p.ParentNode 		= this;
			

			if (inputs != null && inputs.Count > 0f)
				p.polyType = PolyType.ptClip;
			
			return p;
		
		}
	
		public bool hasInput(AXParameter p)
		{
			for (int i = 0; i < inputs.Count; i++) 
				if (inputs[i].Guid == p.Guid)
					return true;
			
			return false;
		}
		public bool hasInput(AXParametricObject po)
		{
			for (int i = 0; i < inputs.Count; i++) 
				if (inputs[i].DependsOn != null && inputs[i].DependsOn.Parent.Guid == po.Guid)
					return true;
			
			return false;
		}


		public bool hasInputsConnected()
		{
			for (int i = 0; i < inputs.Count; i++) 
				if (inputs[i].DependsOn != null )
					return true;
			
			return false;
		}




		public bool hasOutputReady()
		{
			if (		difference 	!= null 	&& ((difference.paths != null && difference.paths.Count > 0) || (difference.polyTree != null)  ) )
				return true;
			else if (	differenceRail 	!= null 	&& ((differenceRail.paths != null && differenceRail.paths.Count > 0) || (differenceRail.polyTree != null)  ) )
				return true;
			else if (	intersection 	!= null 	&& ((intersection.paths != null && intersection.paths.Count > 0) || (intersection.polyTree != null)  ) )
				return true;
			else if (	intersectionRail 	!= null 	&& ((intersectionRail.paths != null && intersectionRail.paths.Count > 0) || (intersectionRail.polyTree != null)  ) )
				return true;
			else if (	union 	!= null 	&& ((union.paths != null && union.paths.Count > 0) || (union.polyTree != null)  ) )
				return true;
			else if (	grouped 	!= null 	&& ((grouped.paths != null && grouped.paths.Count > 0) || (grouped.polyTree != null)  ) )
				return true;

			return false;
		}
		
		public bool hasOutputConnected()
		{
			if (		difference 			!= null 	&& difference.Dependents 		!= null 	&& difference.Dependents.Count 			> 0)
				return true;
			else if (	differenceRail 		!= null 	&& differenceRail.Dependents 	!= null 	&& differenceRail.Dependents.Count 		> 0)
				return true;
			else if (	intersection 		!= null 	&& intersection.Dependents 		!= null 	&& intersection.Dependents.Count 		> 0)
				return true;
			else if (	intersectionRail 	!= null 	&& intersectionRail.Dependents 	!= null 	&& intersectionRail.Dependents.Count 	> 0)
				return true;
			else if (	union 				!= null 	&& union.Dependents 			!= null 	&& union.Dependents.Count 				> 0)
				return true;
			else if (	grouped 			!= null 	&& grouped.Dependents 			!= null 	&& grouped.Dependents.Count 			> 0) 
				return true;

			return false;
		}

		public void clearAllOutputs()
		{
			difference.paths 			= null;
			difference.polyTree 		= null;

			differenceRail.paths 		= null;
			differenceRail.polyTree 	= null;

			intersection.paths 			= null;
			intersection.polyTree 		= null;

			intersectionRail.paths 		= null;
			intersectionRail.polyTree 	= null;

			union.paths 				= null;
			union.polyTree 				= null;

			grouped.paths 				= null;
			grouped.polyTree 			= null;




		}


	
		public AXParameter getSelectedOutputParameter()
		{
			AXParameter return_P = difference;
			
			switch(combineType)
			{
			case CombineType.Difference:
				return_P = difference;
				break;
				
			case CombineType.DifferenceRail:
				return_P = differenceRail;
				break;
				
			case CombineType.Intersection:
				return_P = intersection;
				break;
				
			case CombineType.IntersectionRail:
				return_P = intersectionRail;
				break;
				
			case CombineType.Union:
				return_P = union;
				break;
				
			case CombineType.Grouped:
				return_P = grouped;
				break;
				
				
			}
			return return_P;
			
			
		}
		
		
		
		
		
		// JSON SERIALIZATION
		public string asJSON()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append("{");
						
			sb.Append("\"name\":\"" 				+ Name +"\"");
			
			// inputs
			sb.Append(", \"inputs\": [");		// begin inputs
			string the_comma = "";
			foreach(AXParameter p in inputs)
			{
				sb.Append(the_comma + p.asJSON());
				the_comma = ", ";
			}
			sb.Append("]"); 			    // end inputs			
			
			// outputs
			sb.Append(", \"difference\": "+difference.asJSON());
			sb.Append(", \"differenceRail\": "+differenceRail.asJSON());
			sb.Append(", \"intersection\": "+intersection.asJSON());
			sb.Append(", \"intersectionRail\": "+intersectionRail.asJSON());
			sb.Append(", \"union\": "+union.asJSON());
			sb.Append(", \"grouped\": "+grouped.asJSON());
			
			
			// finish
			sb.Append("}"); // end parametri_object
			
			return sb.ToString();
		}
		
		
		
		public static AXShape fromJSON(AXParametricObject po, AX.SimpleJSON.JSONNode jn)
		{
			AXShape shape = new AXShape(jn["name"]);
			
			shape.parametricObject = po;
			shape.ParentNode 		= po;
			shape.Parent 			= po;
			
			// parameters
			
			if (jn["inputs"] != null)
			{
				shape.inputs = new List<AXParameter>();
				foreach(AX.SimpleJSON.JSONNode jn_p in jn["inputs"].AsArray)
					shape.inputs.Add (AXParameter.fromJSON(jn_p));
			}
			
			
			if (jn["difference"] != null)
				shape.difference = AXParameter.fromJSON(jn["difference"]);

			if (jn["differenceRail"] != null)
				shape.differenceRail = AXParameter.fromJSON(jn["differenceRail"]);
			
			if (jn["intersection"] != null)
				shape.intersection = AXParameter.fromJSON(jn["intersection"]);
			
			if (jn["intersectionRail"] != null)
				shape.intersectionRail = AXParameter.fromJSON(jn["intersectionRail"]);
			
			if (jn["union"] != null)
				shape.union = AXParameter.fromJSON(jn["union"]);
			
			if (jn["grouped"] != null)
				shape.grouped = AXParameter.fromJSON(jn["grouped"]);
			
			
			
			return shape;
		}
		
		
		
		
		
		
		
		
		
		public static PolyNode mergeSubjPathAndClipPath(Path subj_p, Path clip_p)
		{

			Clipper c 		= new Clipper(Clipper.ioPreserveCollinear);

			c.AddPath (subj_p,  	PolyType.ptSubject, true);
			c.AddPath (clip_p,  	PolyType.ptClip, 	true);

			PolyTree polyTree 	= new AXClipperLib.PolyTree();
			c.Execute(ClipType.ctDifference, 	polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

			return polyTree;
		}
		
		
		
		
		
		
	
		/* GENERATE */
		
		public void generate()
		{
			// Use the inputs to create a new shape for the ouput
	
			if (difference == null)
				initOutputs();
	
			
	
			// fill the five outputs
			generateLine();
			genrateRail();
		}
	
		public float getShift_x()
		{
		
			if (inputs.Count == 1 && inputs[0].DependsOn != null)
			{
				float shift_x = inputs[0].DependsOn.Parent.floatValue("Trans_X");
				return shift_x;
			}
			return 0;
		
		}
	
	
		/* GENERATE LINE */
	
		public void generateLine()
		{
			if (inputs.Count == 0) 
				return;
			
			Clipper c 		= new Clipper(Clipper.ioPreserveCollinear);
			c.PreserveCollinear = true;
			AXParameter input 	= null;
			AXParameter src 	= null;
		
			//Debug.Log ("GENERATE LINE");


//			// DON'T ALTER AN "OPEN" SHAPE OR RAIL IF IT IS NOT BEING COMBINED WITH OTHER INPUTS AND IS NOT THICKENED OR OFFSET
			if (inputs.Count == 1 && inputs[0].offset == 0 && inputs[0].thickness == 0)
			{
				if (inputs[0].DependsOn != null)
				{
					difference.polyTree 	= inputs[0].DependsOn.polyTree;
					difference.paths 		= inputs[0].DependsOn.paths;
				}
				
						
				//Archimatix.printPath(output.paths[0]);
				return;
			}
			
			// GROUPED COMBINE
			grouped.paths = new Paths();
			foreach (AXParameter inp in inputs)
			{
				if (inp.paths == null)
					continue;
					
				foreach(Path _path in inp.paths)
				{
					grouped.paths.Add (_path);
				}
			}
			
			
			// EACH INPUT (PREPROCESSING)
			
			for (int i = 0; i < inputs.Count; i++) {


				input 	= inputs [i];
				src 	= input.DependsOn;

				if (src != null && src.parametricObject != null && ! src.parametricObject.isActive)
					continue;
				
				if (src == null)
						continue;
				input.polyTree = null;
				
				thickenAndOffset(ref input, src); 	
	
				// Add as a Solid or Void (subj or clip) to Clipper c
				//bool isClosed = (input.shapeState == ShapeState.Closed || input.polyType == PolyType.ptSubject) ? true : false;
				bool isClosed = true;
				if (input.polyTree != null) {
					c.AddPaths (AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(input.polyTree)), 	input.polyType, isClosed);
				} else if (input.paths != null) {
					c.AddPaths (AXGeometry.Utilities.cleanPaths(input.paths),  	input.polyType, isClosed);
				}
			} // end inputs preprocessing






	 
			// COMBINE INPUTS
	
			// Output

			AX.Generators.Generator2D gener2D = parametricObject.generator as AX.Generators.Generator2D;
			
			// DIFFERENCE
			
			if((difference.Dependents != null && difference.Dependents.Count > 0) || combineType == CombineType.Difference)
			{ 
				difference.polyTree 	= new AXClipperLib.PolyTree();
				c.Execute(ClipType.ctDifference, 	difference.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);


				thickenAndOffset(ref difference, difference);


				AXGeometry.Utilities.transformPolyTree(difference.polyTree, gener2D.localMatrix);

				if (difference.reverse)
					AXGeometry.Utilities.reversePolyTree(difference.polyTree);

				AX.Generators.Generator2D.deriveStatsETC(difference);
			}
			
			// INTERSECTION	
			//if((intersection.Dependents != null && intersection.Dependents.Count > 0) || combineType == CombineType.Intersection)
			//{ 
				intersection.polyTree 	= new AXClipperLib.PolyTree();
				c.Execute(ClipType.ctIntersection, 	intersection.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);


				thickenAndOffset(ref intersection, intersection);

				AXGeometry.Utilities.transformPolyTree(intersection.polyTree, gener2D.localMatrix);

			if (intersection.reverse)
				AXGeometry.Utilities.reversePolyTree(intersection.polyTree);

			AX.Generators.Generator2D.deriveStatsETC(intersection);

			//}

			// UNION
			if((union.Dependents != null && union.Dependents.Count > 0) || combineType == CombineType.Union)
			{ 
				union.polyTree 			= new AXClipperLib.PolyTree();
				c.Execute(ClipType.ctUnion, 		union.polyTree, 		PolyFillType.pftNonZero, PolyFillType.pftNonZero);


				thickenAndOffset(ref union, union);

				AXGeometry.Utilities.transformPolyTree(intersection.polyTree, gener2D.localMatrix);

				if (union.reverse)
					AXGeometry.Utilities.reversePolyTree(union.polyTree);

				AX.Generators.Generator2D.deriveStatsETC(union);

			}
		}
	
		public bool doOutput(AXParameter o)
		{
			if(o.Dependents != null && o.Dependents.Count > 0)
				return true;
				
			return false;
		}
	
	
	
	
		public static Paths segment (Paths paths, float lowerY, float upperY, int segs)
		{
								
			Paths resPaths = new Paths();
			
			

			for (int i = 0; i < paths.Count; i++) {
				Path path = paths [i];
				
				
				
				for(int j=0; j<path.Count-1; j++)
				{
					for (int seg=0; seg<segs; seg++)
					{
					
					}
				
				
				}
				
				
				
			}
			
			
			return resPaths;
			
			
		
		 
		}




		public static void thickenAndOffset(ref AXParameter sp, AXParameter src)
		{
			if (sp == null || src == null)
				return;
			//sp.polyTree = null;



			float thickness = sp.thickness;
			float roundness = sp.roundness;
			float offset 	= sp.offset;
			bool  flipX		= sp.flipX;

			//Debug.Log(sp.parametricObject.Name + "." + sp.Name +"."+ sp.offset);

			//bool srcIsCC = src.isCCW();




			Paths subjPaths = src.getClonePaths();
			if (subjPaths == null) 
				return;
			

			// FLIP_X
			if (flipX)
			{
				//Debug.Log(subjPaths.Count);
				//AXGeometry.Utilities.printPaths(subjPaths);

				subjPaths = AXGeometry.Utilities.transformPaths(subjPaths, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1,1,1)));

				sp.transformedControlPaths = AXGeometry.Utilities.transformPaths(sp.transformedControlPaths, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1,1,1)));
			}




			// SYMMETRY
			if (sp.symmetry)
			{
				 if (subjPaths != null && subjPaths.Count > 0)
				{
					for(int i=0; i<subjPaths.Count; i++)
					{
						Path orig = subjPaths[i];

						Path sym = new Path();

						float seperation = sp.symSeperation * AXGeometry.Utilities.IntPointPrecision;
						float apex = .1f*seperation;


						for(int j=0; j<orig.Count; j++)
							sym.Add(new IntPoint(orig[j].X+seperation/2, orig[j].Y));

						// midpoint. slightly raised
						sym.Add(new IntPoint(0, orig[orig.Count-1].Y+apex));

						for(int j=orig.Count-1; j>=0; j--)
							sym.Add(new IntPoint(-orig[j].X-seperation/2, orig[j].Y));

						subjPaths[i] = sym;
					}
				}
			}









			AX.Generators.Generator2D gener2D = sp.parametricObject.generator as AX.Generators.Generator2D;


			if (subjPaths != null && gener2D != null && (gener2D.scaleX != 1 || gener2D.scaleY != 1))
				sp.transformedButUnscaledOutputPaths = AX.Generators.Generator2D.transformPaths(subjPaths, gener2D.localUnscaledMatrix);
			else 
				sp.transformedButUnscaledOutputPaths = null;




			//cleanPolygonPrecision
			IntRect brect = Clipper.GetBounds(subjPaths);

			if ( (brect.right-brect.left) < 100000)
				cleanPolygonPrecision = 2;
			else
				cleanPolygonPrecision = 30;

			//Debug.Log("cleanPolygonPrecision="+cleanPolygonPrecision);
			/*
			if (offset_p.FloatVal == 0 && wallthick_p.FloatVal == 0)
			{
				sp.polyTree = src.polyTree;
				sp.paths = src.paths; 
				return;
			}
			
			*/
			//sp.polyTree = null;
			



			//Debug.Log("new count a = " + subjPaths[0].Count);


			bool hasOffset = false;
			sp.hasThickness = false;
			
			
			Paths 		resPaths 	= new Paths ();
			AXClipperLib.PolyTree 	resPolytree = null;


			// OFFSETTER
			ClipperOffset 	co  = new ClipperOffset ();


			//co.ArcTolerance = sp.arcTolerance;
			float smooth = (float) (120 / (sp.arcTolerance * sp.arcTolerance));
			float smoothLOD =((smooth-.048f) * sp.parametricObject.model.segmentReductionFactor)+.048f;
			co.ArcTolerance = (float) (Mathf.Sqrt(120/smoothLOD));


			co.MiterLimit = 2.0f;
			

			//if (offset != 0)
					

			// 1. Offset? Can't offset an open shape
			if (sp.shapeState == ShapeState.Closed && (sp.endType == AXClipperLib.EndType.etClosedLine || sp.endType == AXClipperLib.EndType.etClosedPolygon))
			{
				
				//AXClipperLib.JoinType jt = (sp.endType == AXClipperLib.EndType.etClosedLine) ? JoinType.jtMiter : sp.joinType;//output.joinType;
				AXClipperLib.JoinType jt = (sp.parametricObject.model.segmentReductionFactor < .15f) ? AXClipperLib.JoinType.jtSquare  : sp.joinType;//output.joinType;
				//Debug.Log ("sp.endType="+sp.endType+", jt="+jt);

				if (roundness != 0) { 
					// reduce
					co.Clear ();

					if (subjPaths != null) 
						co.AddPaths (AXGeometry.Utilities.cleanPaths (subjPaths, cleanPolygonPrecision), jt, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);

					co.Execute (ref subjPaths, (double)(-roundness * AXGeometry.Utilities.IntPointPrecision));
				}

				offset += roundness;

				if (subjPaths != null) 
					subjPaths = Clipper.SimplifyPolygons(subjPaths, PolyFillType.pftNonZero);

				co.Clear();

				hasOffset = true;

//				if (offset != 0 || thickness == 0) { // --! PUC ** Removed because the pass thru was causing a problem with Instance2D of a ShapeMerger
					// Do Offset

					// = true;

					if (subjPaths != null)
					{

						// After changes made mid April to allow for FlipX, this started doubling the localMatrix and thus became redundent, though not sure why.
						//if (gener2D != null)
						//	sp.transformedAndScaledButNotOffsetdOutputPaths = AX.Generators.Generator2D.transformPaths(subjPaths, gener2D.localMatrix);

						co.AddPaths (AXGeometry.Utilities.cleanPaths(subjPaths, cleanPolygonPrecision), jt, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);

						// this resPolytree has transformed curves in it
						resPolytree = new AXClipperLib.PolyTree();
						co.Execute (ref resPolytree, (double)(offset * AXGeometry.Utilities.IntPointPrecision));


					}
//				}
				if (thickness > 0) 
				{	// No offset, but is closed
					sp.transformedAndScaledButNotOffsetdOutputPaths 	= null;
					if (src.polyTree != null) {
						if (thickness > 0)
							resPaths = subjPaths; // Clipper.PolyTreeToPaths(src.polyTree);
						else
						{
							resPolytree = src.polyTree;
						}
					} else {
						resPaths = subjPaths;
					}
				}



				
			} 
			else 
			{
				//resPolytree = src.polyTree;
				if (src.polyTree != null) {
					if (thickness > 0)
						resPaths = subjPaths; // Clipper.PolyTreeToPaths(src.polyTree);
					else
						resPolytree = src.polyTree;
				} else {
					resPaths = subjPaths;
				}

			}


			// 2. Thickness?
			//subjPaths = sp.getPaths();
			 

			if ( (sp.endType != AXClipperLib.EndType.etClosedPolygon) && thickness > 0) //input.endType != AXClipperLib.EndType.etClosedPolygon) {
			{	
				
				// this is a wall
				if (resPaths != null && gener2D != null)
					sp.transformedFullyAndOffsetdButNotThickenedOutputPaths = AX.Generators.Generator2D.transformPaths(resPaths, gener2D.localMatrix);

				sp.hasThickness = true;


				co.Clear();
				if (resPolytree != null) // closed block has happened
				{
					
					co.AddPaths (AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(resPolytree), cleanPolygonPrecision), sp.joinType, sp.endType);//input.endType);
				}
				else if (resPaths != null)
				{
					
					co.AddPaths (AXGeometry.Utilities.cleanPaths(resPaths, cleanPolygonPrecision), sp.joinType, sp.endType);//input.endType);
				}

				resPolytree = new AXClipperLib.PolyTree ();
				co.Execute (ref resPolytree, (double)(thickness * AXGeometry.Utilities.IntPointPrecision));


			}  
			else
				sp.transformedFullyAndOffsetdButNotThickenedOutputPaths = null;



			 
			// 3. Update input data
			//Debug.Log(sp.parametricObject.Name  + "." + sp.Name + " here ["+hasOffset+"] " + (! sp.symmetry) + " " +  (! flipX)  + " " + (! hasOffset)  + " " +  (! hasThicken)  + " " +  (roundness == 0));


			// SIMPLE PASSTHRU?
			if (! sp.symmetry && ! flipX && ! hasOffset && ! sp.hasThickness && roundness == 0)
			{
				// SIMPLE PASS THROUGH
				sp.polyTree =  src.polyTree;
				sp.paths = src.paths;



			}

			else
			{
				if (resPolytree == null)
				{
					//sp.paths 		= resPaths; //Generator2D.transformPaths(resPaths, gener2D.localMatrix);
					//if (Clipper.Orientation(resPaths[0]) != srcIsCC)
					//	AXGeometry.Utilities.reversePaths(ref resPaths);

					sp.paths 		= AXGeometry.Utilities.cleanPaths(resPaths, cleanPolygonPrecision); 
				}
				else
				{
					//Generator2D.transformPolyTree(resPolytree, gener2D.localMatrix);

					//if (resPolytree != null && resPolytree.Childs.Count > 0 &&  Clipper.Orientation(resPolytree.Childs[0].Contour) != srcIsCC)
					//	AXGeometry.Utilities.reversePolyTree(resPolytree);


					sp.polyTree 	= resPolytree;

				}
			}




			// REVERSE
			if (sp.reverse)
			{

				if (sp.polyTree != null)
				{
					AXGeometry.Utilities.reversePolyTree(sp.polyTree);
				}
				else if (sp.paths != null && sp.paths.Count > 0)
				{
					for(int i=0; i<sp.paths.Count; i++)
					{

						sp.paths[i].Reverse();


					}
				}
			}








//			if (sp.paths != null && sp.paths.Count > 0)
//			{
//				// SUBDIVISION
//				Debug.Log("sp.paths.Count="+sp.paths.Count);
//
//				for(int i=0; i<sp.paths.Count; i++)
//				{
//					
//
//					Path path = sp.paths[i];
//					Path subdivPath = new Path();
//
//					for (int j=0; j<path.Count-1; j++)
//					{
//						subdivPath.Add(path[j]);
//						Vector2 v0 = new Vector2(path[j].X, path[j].Y);
//						Vector2 v1 = new Vector2(path[j+1].X, path[j+1].Y);
//
//						Debug.Log("["+i+"]["+j+"] " + Vector2.Distance(v0, v1)/10000);
//						 Vector2 newp = Vector2.Lerp(v0, v1, .5f);
//
//						subdivPath.Add(new IntPoint(newp.x, newp.y));
//					}
//					subdivPath.Add(path[path.Count-1]);
//
//
//					sp.paths[i] = subdivPath;
//
//					Debug.Log("------------");
//					AXGeometry.Utilities.printPath(sp.paths[i]);
//				}
//					// SUBDIVISION ---
//			}
//




		}
	
	
	
	
	
	
		// RAIL
	
		public void genrateRail()
		{
			// generate a rail for all three: difference, intersection, union
			
			
			if (inputs.Count == 0) 
				return;
			
	
			PolyFillType subjPolyFillType = PolyFillType.pftNonZero;
			//if (hasHoles)
				//subjPolyFillType = PolyFillType.pftPositive;
	
	
			//* organize by solids, holes (from SOLID designation) and clippers (from VOID designation)
			Paths subjPaths = new Paths();
			Paths clipPaths = new Paths();
	
			AXParameter inp   = null;
			AXParameter src = null;


			Paths segments = new Paths();
			Path tmp = null;


			for (int i = 0; i < inputs.Count; i++) {
				inp 	= inputs [i];
				src = inp.DependsOn;
				
				if (src == null)
					continue;
	
				Paths srcPaths = src.getPaths();
				
				if (srcPaths == null)
						continue;
						
				if (inp.polyType == PolyType.ptSubject)
				{
					subjPaths.AddRange (srcPaths);


					foreach(Path sp in srcPaths)
					{
						// When clipping open shapes, don't add a closingsegment:
						int ender = (inp.shapeState == ShapeState.Open) ? sp.Count-1 : sp.Count;

						for(int j=0; j<ender; j++)
						{
							int next_j = (j == sp.Count-1) ? 0 : j+1;
							tmp = new Path();
							tmp.Add (sp[j]);
							tmp.Add (sp[next_j]);
							segments.Add(tmp);
						}
					}

					//subjPaths.AddRange (src.getTransformedHolePaths());
	
				}
				else if (inp.polyType == PolyType.ptClip)
				{
					clipPaths.AddRange (srcPaths);
					//clipPaths.AddRange (src.getTransformedHolePaths());
				}
				else 
					continue;
			}
	
			
			// turn subjs and holes into segments to be clipped
//			foreach(Path sp in subjPaths)
//			{
//				// each path
//				//int ender = 
//				for(int i=0; i<sp.Count-1; i++)
//				{
//					int next_i = (i == sp.Count-1) ? 0 : i+1;
//					tmp = new Path();
//					tmp.Add (sp[i]);
//					tmp.Add (sp[next_i]);
//					segments.Add(tmp);
//				}
//			}
			
			//Debug.Log ("segments");
			//Archimatix.printPaths(segments);
			
			Clipper railc = new Clipper(Clipper.ioPreserveCollinear);
			
			if (segments != null)
				railc.AddPaths(segments, PolyType.ptSubject, false);
			
			if (clipPaths != null)
				railc.AddPaths(clipPaths, PolyType.ptClip, true);
			
			AXClipperLib.PolyTree solutionRail = new AXClipperLib.PolyTree();
			
			
			
			// DIFFERENCE_RAIL
			if((differenceRail.Dependents != null && differenceRail.Dependents.Count > 0) || combineType == CombineType.DifferenceRail)
			{ 

				// Execute Difference
				railc.Execute(ClipType.ctDifference, solutionRail, subjPolyFillType, PolyFillType.pftNonZero);


				differenceRail.polyTree 	= null;

					
				differenceRail.paths = assembleRailPathsFromClippedSegments(solutionRail);



//				Debug.Log("******** " + differenceRail.paths.Count);
//				Pather.printPaths(differenceRail.paths);


				if (differenceRail.paths.Count == 0)
					differenceRail.paths = subjPaths;

				if (differenceRail.paths.Count > 1)
					differenceRail.paths = Pather.cleanPaths(differenceRail.paths);

//				Debug.Log("-- " + differenceRail.paths.Count);
//				Pather.printPaths(differenceRail.paths);


				alignPathsDirectionsWithSource(ref differenceRail.paths, ref segments);



				if (differenceRail.paths.Count > 1)
					joinPathsIfEndpointsMatch(ref differenceRail.paths);




				thickenAndOffset(ref differenceRail, differenceRail);


			}	
			
			// INTERSECTION RAIL
			if((intersectionRail.Dependents != null && intersectionRail.Dependents.Count > 0) || combineType == CombineType.IntersectionRail)
			{ 
				//railc.Execute(ClipType.ctIntersection, solutionRail, subjPolyFillType, PolyFillType.pftNonZero);
				railc.Execute(ClipType.ctIntersection, solutionRail, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

				intersectionRail.polyTree 	= null;

				intersectionRail.paths = assembleRailPathsFromClippedSegments(solutionRail);		
				if (intersectionRail.paths.Count == 0)
					AXGeometry.Utilities.reversePaths(ref intersectionRail.paths);

				alignPathsDirectionsWithSource(ref intersectionRail.paths, ref segments);

				thickenAndOffset(ref intersectionRail, intersectionRail);

			}
			
			
		}
		
		public void alignPathsDirectionsWithSource(ref Paths segs, ref Paths sourceSegs)
		{
			// The assumption is that a seg in segs will be on only one of the sourceSegs.

			foreach(Path seg in segs)
			{
				if (seg == null || seg.Count < 2)
					continue;
				IntPoint first  = seg[0];
				IntPoint second = seg[1];

				bool foundSource = false;

				foreach (Path src in sourceSegs)
				{
					// setp through each src line to see if first falls on it.
					for (int k=0; k< src.Count-1; k++)
					{
						if(PointOnLineSegment(first, src[k], src[k+1], true))
						{
							//Debug.Log(first.X + ", "+ first.Y + " on line " + src[k].X + ", " +src[k].Y + "->" +src[k+1].X + ", " + src[k+1].Y);

							if (AXSegment.pointsAreEqual(second, src[k])  ||   (Pather.Distance(src[k], second) < Pather.Distance(src[k], first))  )
							{
								foundSource = true;
								seg.Reverse();
							}

							break;
						} 

					}

					if (foundSource)
						break;


				}
			}


		}



		public void joinPathsIfEndpointsMatch(ref Paths paths)
		{

			// For now, assume only one occurence of endpoint matching a begin point of another path
			//Debug.Log(paths.Count);
			for (int i = 0; i < paths.Count; i++) {
				
				for (int j = 0; j < paths.Count; j++) 
				{
					//if (i==j || )
					//	continue;

					if ( Pather.Equals(paths[i][paths[i].Count-1], paths[j][0]))
					{
						// dont' add first point
						for (int k=1; k<paths[j].Count; k++)
							paths[i].Add(paths[j][k]);

						paths.RemoveAt(j);
						return;

					}
				}

			}

		}








		public static bool PointOnLineSegment(IntPoint pt, 
        IntPoint linePt1, IntPoint linePt2, bool UseFullRange)
	    {
	      if (UseFullRange)
	        return ((pt.X == linePt1.X) && (pt.Y == linePt1.Y)) ||
	          ((pt.X == linePt2.X) && (pt.Y == linePt2.Y)) ||
	          (((pt.X > linePt1.X) == (pt.X < linePt2.X)) &&
	          ((pt.Y > linePt1.Y) == (pt.Y < linePt2.Y)) &&
	          ((Int128.Int128Mul((pt.X - linePt1.X), (linePt2.Y - linePt1.Y)) ==
	          Int128.Int128Mul((linePt2.X - linePt1.X), (pt.Y - linePt1.Y)))));
	      else
	        return ((pt.X == linePt1.X) && (pt.Y == linePt1.Y)) ||
	          ((pt.X == linePt2.X) && (pt.Y == linePt2.Y)) ||
	          (((pt.X > linePt1.X) == (pt.X < linePt2.X)) &&
	          ((pt.Y > linePt1.Y) == (pt.Y < linePt2.Y)) &&
	          ((pt.X - linePt1.X) * (linePt2.Y - linePt1.Y) ==
	            (linePt2.X - linePt1.X) * (pt.Y - linePt1.Y)));
	    }
		
		// This is used with rails since the Clipper Lib does not support 
		// difference with the lines of the clip object removed.
		// In other words, to open a door in a closed polygon, 
		// we use this to remove the edges that were originally on the clip object
		// leaving only the line segments of the subject that were not inside the clip shape.
		public Paths assembleRailPathsFromClippedSegments(AXClipperLib.PolyTree solutionRail)
		{
			
			// Take all the clipped segments and connect
			Paths clippedSegments = Clipper.OpenPathsFromPolyTree(solutionRail);
			
			//Debug.Log ("clipped segments " + clippedSegments.Count);
			//Archimatix.printPaths(clippedSegments);
			
			Dictionary<Path, AXSegment> axsegs = new Dictionary<Path, AXSegment>();
			
			foreach (Path p in clippedSegments)
				axsegs.Add(p, new AXSegment(p));
			
			int cc = 0;
			foreach(Path clipseg in clippedSegments)
			{
				//Debug.Log ("PASS["+cc+"] " + AXGeometry.Utilities.pathToString(clipseg));
				
				if (axsegs[clipseg].prev != null && axsegs[clipseg].next != null)
				{   

					//Debug.Log("Already done");
					cc++;
					continue;
				}
				int ccc = 0;
				foreach(Path compseg in clippedSegments)
				{
					if (compseg == clipseg)
					{ 	
						//Debug.Log("COMP["+cc+"]["+ccc+"] skip same");
						ccc++;
						continue;
					}
					//Debug.Log ("COMP["+cc+"]["+ccc+"]"+AXGeometry.Utilities.pathToString(clipseg)+ " with "+AXGeometry.Utilities.pathToString(compseg));
					
					if (axsegs[clipseg].prev == null && axsegs[compseg].next == null )
					{
						if (AXSegment.pointsAreEqual (clipseg[0], compseg[1]))
						{
							axsegs[clipseg].prev = axsegs[compseg];
							axsegs[compseg].next = axsegs[clipseg];
							//Debug.Log ("prev");
						}
					}
					if (axsegs[clipseg].prev == null && axsegs[compseg].prev == null )
					{
						if (AXSegment.pointsAreEqual (clipseg[0], compseg[0]))
						{
							compseg.Reverse();
							axsegs[clipseg].prev = axsegs[compseg];
							axsegs[compseg].next = axsegs[clipseg];
						}
					}
					if (axsegs[clipseg].next == null && axsegs[compseg].prev == null )
					{
						if (AXSegment.pointsAreEqual (clipseg[1], compseg[0]))
						{
							axsegs[clipseg].next = axsegs[compseg];
							axsegs[compseg].prev = axsegs[clipseg];
						}
					}
					if (axsegs[clipseg].next == null && axsegs[compseg].next == null )
					{
						if (AXSegment.pointsAreEqual (clipseg[1], compseg[1]))
						{
							compseg.Reverse();
							axsegs[clipseg].next = axsegs[compseg];
							axsegs[compseg].prev = axsegs[clipseg];
						}
					}
					ccc++;
				}
				cc++;
			}
			
			// now all segs should be linked.
			// go through each that has no prev and create new paths...
			Paths retPaths = new Paths();
			
			foreach(KeyValuePair<Path, AXSegment> item in axsegs)
			{			
				Path path = item.Key;
				if (item.Value.prev == null)
				{   // now crawl up the nexts adding pt[1]s
					AXSegment next = item.Value;
					while( (next= next.next) != null)
						path.Add (next.path[1]);
					
					retPaths.Add (path);
				}
			}
			
			return retPaths;
			
	
		}
		
		
		
		
		
	
	} // Shape

} // namespace AX
