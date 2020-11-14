using UnityEngine;



using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

using AXGeometry;
using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;



namespace AX.Generators
{


	/// <summary>
	/// Free curve.
	/// Manages a pParametricObject's curve member
	/// </summary>
	public class FreeCurve : AX.Generators.Generator2D, IShape, ICurve
	{

		public override string GeneratorHandlerTypeName { get { return "FreeCurveHandler"; } }
		
		public AXParameter P_Segs;

		public int 		segs 		= 16;

		public float 	timetick;

		public override void init_parametricObject() 
		{
			//Debug.Log ("INIT " + parametricObject.Name); 

			base.init_parametricObject();

			// parameters
			
			
			P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			P_Output.hasInputSocket = false;
			P_Output.shapeState = ShapeState.Open;


			/*
			Vector2 pt0 = new Vector2(0, 0);
			Vector2 pt1 = new Vector2(0, 5);
			Vector2 pt2 = new Vector2(5, -5);
			Vector2 pt3 = new Vector2(5, 0);
			*/
			/*
			for (float i=0; i<=1; i=i+.05f)
			{
				Vector2 pt = AXTurtle.bezierValue(pt0, pt1, pt2, pt3, i);
				parametricObject.curve.Add (new CurvePoint(pt.x,pt.y)); 
			}
			*/

			//parametricObject.curve.Add ( new CurvePoint(Vector2.zero, new Vector2(0, -5)) ); 
			//parametricObject.curve.Add ( new CurvePoint(new Vector2(5, 0), new Vector2(5, -5)) ); 
			//parametricObject.curve.Add ( new CurvePoint(new Vector2(10, 0), new Vector2(10, -5)) ); 

			//parametricObject.curve.Add ( new CurvePoint(new Vector2(0, 5)) ); 
			//parametricObject.curve.Add ( new CurvePoint(new Vector2(5, 5)) ); 
			//parametricObject.curve.Add ( new CurvePoint(new Vector2(12, 5)) ); 
			//parametricObject.curve.Add (new CurvePoint(10,0)); 
			//parametricObject.curve.Add (new CurvePoint(10,10)); 
				
			parametricObject.addParameter(AXParameter.DataType.Int , 	"Segs",  		16, 		3, 64);

			
			// handles
			
		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Segs 		= parametricObject.getParameter("Segs");


		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			
			base.pollControlValuesFromParmeters();

			segs 	= (P_Segs != null) 		? P_Segs.IntVal		: 5;


			if (segs < 3)
			{ 	
				segs = 3;
				P_Segs.intval = 3;
			}

			if (segs > 50)
			{ 	
				segs = 50;
				P_Segs.intval = 50;
			}

			int msegs = Mathf.Max( 1, Mathf.FloorToInt( ( (float)segs * parametricObject.model.segmentReductionFactor ) ) );
			timetick =  1/(1f*msegs);

		}



		public Path getPathFromCurve()
		{

			Path path = new Path();

			for (int i=0; i<=parametricObject.curve.Count-1; i++)
			{
				if (i==parametricObject.curve.Count-1 && P_Output.shapeState == ShapeState.Open) 
					break;

				CurveControlPoint2D a = parametricObject.curve[i];

				int next_i = (i==parametricObject.curve.Count-1) ? 0 : i+1;

				CurveControlPoint2D b = parametricObject.curve[next_i];

				if (a.isPoint() && b.isPoint())
				{
					if (i==0)
						path.Add(AXGeometry.Utilities.Vec2_2_IntPt(a.position));

					path.Add(AXGeometry.Utilities.Vec2_2_IntPt(b.position));
				}
				else 
				{

					int governor = 0;
					for (float t=0; t<=(1+.9f*timetick); t=t+timetick)
					{
						if (governor++ > 50)
						{
							Debug.Log("governor hit)");
							break;
						}	

						if (i == 0 || t > 0)
						{
							
							Vector2 pt = bezierValue(parametricObject.curve[i], parametricObject.curve[next_i], t);
							path.Add(AXGeometry.Utilities.Vec2_2_IntPt(pt));
						}
					}
				}
			}
			return path;

		}


		public void assertCCW()
		{
			Path path = getPathFromCurve();

			if (! Clipper.Orientation(path))
				parametricObject.curve.Reverse();

		}


		public void setCurvePointPosition(int i, Vector2 pos)
		{
			// This may be related to a parameter

			parametricObject.curve[i].position = pos;

			float num;
			if (float.TryParse (parametricObject.curve [i].xExpression, NumberStyles.Any, CultureInfo.InvariantCulture, out num)) {
				// ONLY NUMERIC --- SIMPLE REPLACE
				parametricObject.curve [i].xExpression = ""+pos.x;
			} else {
				// PARAMETER ------ RIPPLE IT, IF THE PARAMETER EXISTS
				AXParameter p = parametricObject.getParameter (parametricObject.curve [i].xExpression);
				if (p != null) {
					parametricObject.initiateRipple_setFloatValueFromGUIChange (parametricObject.curve [i].xExpression, pos.x);
				}
			}
			if (float.TryParse (parametricObject.curve [i].yExpression, NumberStyles.Any, CultureInfo.InvariantCulture, out num)) {
				// ONLY NUMERIC --- SIMPLE REPLACE
				parametricObject.curve [i].yExpression = ""+pos.y;
			} else {
				// PARAMETER ------ RIPPLE IT, IF THE PARAMETER EXISTS
				AXParameter p = parametricObject.getParameter (parametricObject.curve [i].yExpression);
				if (p != null) {
					parametricObject.initiateRipple_setFloatValueFromGUIChange (parametricObject.curve [i].yExpression, pos.y);
				}
			}
		}


		// GENERATE FREE_CURVE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			parametricObject.transMatrix = Matrix4x4.TRS(new Vector3(transX, transY, 0), Quaternion.Euler(0, 0, rotZ), new Vector3(1,1,1));


			//Debug.Log ("IntPoint = " + parametricObject.ip.X + " --- " + parametricObject.TestInt);
			
			if (P_Output != null && parametricObject.curve != null)
			{
			
				//Curve transformedCurve = parametricObject.getTransformedCurve();
				
				//Output.spline = Archimatix.curve2Spline (transformedCurve);
				
				// ControlPaths
				P_Output.controlPaths	= new Paths();  // not to be altered in base postprocessing for offset and wallthick
				
				// use the "curve" to generate the source path

				// bezier experiment

				// EACH SEGMENT
				Path path = new Path();

			

				for (int i=0; i<=parametricObject.curve.Count-1; i++)
				{
                   //Debug.Log(i + " -" + string.IsNullOrEmpty(a.xExpression) + "- ");

        
					CurveControlPoint2D a = parametricObject.curve[i];

                   
					// ASSERT EXPRESSION STRINGS
					if (string.IsNullOrEmpty (a.xExpression))
						a.xExpression = ""+a.position.x;

					if (string.IsNullOrEmpty (a.yExpression))
						a.yExpression = ""+a.position.y;

                    if (i == parametricObject.curve.Count-1 && P_Output.shapeState == ShapeState.Open)
                        break;

                    int next_i = (i==parametricObject.curve.Count-1) ? 0 : i+1;

					CurveControlPoint2D b = parametricObject.curve[next_i];

					if (a.isPoint() && b.isPoint())
					{
						if (i==0)
							path.Add(AXGeometry.Utilities.Vec2_2_IntPt(a.position));

						path.Add(AXGeometry.Utilities.Vec2_2_IntPt(b.position));
					}
					else 
					{

						int governor = 0;
						for (float t=0; t<=(1+.9f*timetick); t=t+timetick)
						{
							if (governor++ > 50)
							{
								Debug.Log("governor hit)");
								break;
							}	

							if (i == 0 || t > 0)
							{
								
								Vector2 pt = bezierValue(parametricObject.curve[i], parametricObject.curve[next_i], t);
								path.Add(AXGeometry.Utilities.Vec2_2_IntPt(pt));
							}
						}
					}
				}

				/*
				if (P_Output.shapeState == ShapeState.Closed && (parametricObject.curve[0] || parametricObject.curve[parametricObject.curve.Count-1].isBezierPoint()))
				{
					// draw last Bezier curve


				}
				*/

			//	path = Clipper.CleanPolygon(path, .01f);
				
//				if (path != null)
//				{
//					if (! Clipper.Orientation(path))
//						path.Reverse();
//
//				}



				P_Output.controlPaths.Add(path);
				
				P_Output.transformedControlPaths 	= P_Output.getTransformedControlPaths();
				P_Output.paths						= P_Output.getTransformedControlPaths(); // may be altered before generate is over...
				P_Output.polyTree					= null;
				

				base.generate(false, initiator_po, isReplica);
				
			}
			else
				Debug.Log ("no path");
				


			base.generate(false, initiator_po, isReplica);

			calculateBounds();



			return null;

			
		}


		public override void parameterWasModified (AXParameter p)
		{
			base.parameterWasModified (p);

            // BRUTE FORCE: CHECK ALL THE POINTS IN THE CURVE TO SEE IF ANY OF THEM USE THIS PARAMETER
            //Debug.Log("param modified: " + p.Name);
			for (int i = 0; i < parametricObject.curve.Count ; i++) {
//				if (i == parametricObject.curve.Count - 1 )
//					break;


				CurveControlPoint2D cp = parametricObject.curve [i];

				if (cp.xExpression == p.Name)
					cp.position.x = p.FloatVal;

				else if (cp.yExpression == p.Name)
					cp.position.y = p.FloatVal;


			}


		}

		public static Vector2  bezierValue(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3, float t)
		{
			return    Mathf.Pow((1-t), 3)*pt0    +    3*Mathf.Pow((1-t), 2)*t*pt1    +    3*(1-t)*t*t*pt2   +   Mathf.Pow(t, 3)*pt3;

		}
		public static Vector2  bezierValue(CurveControlPoint2D a, CurveControlPoint2D b, float t)
		{
			return    Mathf.Pow((1-t), 3)*a.position    +    3*Mathf.Pow((1-t), 2)*t*(a.position+a.localHandleB)    +    3*(1-t)*t*t*(b.position+b.localHandleA)   +   Mathf.Pow(t, 3)*b.position;

		}


		public CurveControlPoint2D prev(int i)
		{
			if (i == 0)
				return  parametricObject.curve[parametricObject.curve.Count-1];

			return parametricObject.curve[i-1];
		}

		public CurveControlPoint2D next(int i)
		{
			if (i == (parametricObject.curve.Count-1))
				return  parametricObject.curve[0];

			return parametricObject.curve[i+1];

		}

		public void cycleConvertCurvePoint(int i)
		{
			CurveControlPoint2D cp = parametricObject.curve[ i ];
			cp.cycleConvertPoint(prev(i), next(i));
			parametricObject.isAltered = true;
			parametricObject.model.isAltered();

		}

		public void convertToSharpPoint(int i)
		{
			if (selectedIndices == null || parametricObject.curve.Count < i)
				return;
			CurveControlPoint2D cp = parametricObject.curve[ i ];
			cp.convertToPoint();
			parametricObject.isAltered = true;
			parametricObject.model.isAltered();

		}
		public void convertToBezier(int i)
		{
			if (selectedIndices == null || parametricObject.curve.Count < i)
				return;

			CurveControlPoint2D cp = parametricObject.curve[ i ];
			cp.convertToBezier(prev(i), next(i));
			parametricObject.isAltered = true;
			parametricObject.model.isAltered();

		}

		public void convertToBezierBroken(int i)
		{
			if (selectedIndices == null || parametricObject.curve.Count < i)
				return;

			CurveControlPoint2D cp = parametricObject.curve[ i ];
			cp.convertToBezierBroken(prev(i), next(i));
			parametricObject.isAltered = true;
			parametricObject.model.isAltered();

		}




		public void DeleteSelected()
		{


			if (selectedIndices == null)
				return;

			if (selectedIndices.Count == 1)
			{

				parametricObject.curve.RemoveAt(selectedIndex);

			}

			else 
			{
				selectedIndices.Sort();
				selectedIndices.Reverse();

				foreach (int index in selectedIndices)
					parametricObject.curve.RemoveAt(index);
				
			}

			selectedIndex = -1;
			selectedIndices.Clear();

			parametricObject.isAltered = true;
			parametricObject.model.autobuild();

		}

		
	}







}