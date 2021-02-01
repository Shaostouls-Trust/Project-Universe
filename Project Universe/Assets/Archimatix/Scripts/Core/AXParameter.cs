using UnityEngine;

using System;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using AX.SimpleJSON;



using AXClipperLib;

using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXGeometry;
using AX.Generators;


//using AX.Generators;

using Parameters = System.Collections.Generic.List<AX.AXParameter>;
using SerializableParameters = System.Collections.Generic.List<AX.SerializableParameter>;

public enum LineType { Line, Rail, Opening };

namespace AX
{

	[System.Serializable]
	public class AXParameter : AXNode
	{

		[SerializeField]
		private string m_guid = System.Guid.NewGuid().ToString();
		public string Guid
		{
			get { return m_guid; }
			set { m_guid = value; }
		}
		public string guidAsEpressionKey
		{
			get
			{
				return ArchimatixUtils.guidToKey(m_guid);
			}
		}

		public delegate void ParameterAltered();
		// Define an Event based on the above Delegate
		public event ParameterAltered WasAltered;


		public enum DataType { Float, Int, Bool, Spline, Mesh, Vector3, Option, Shape, Plane, Data, MaterialTool, RepeaterTool, JitterTool, Outlet, String, CustomOption, AnimationCurve, Generic, Color, FloatList, Curve3D, Texture2D };

		[SerializeField]
		private DataType m_type;
		public DataType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		public string dataTypeAsString()
		{
			string retStr = "";

			switch (Type)
			{
				case DataType.Float: retStr = "float"; break;
				case DataType.Int: retStr = "int"; break;
				case DataType.Bool: retStr = "bool"; break;
				case DataType.Mesh: retStr = "Mesh"; break;
				case DataType.Option: retStr = "int"; break;
				case DataType.Color: retStr = "color"; break;
				case DataType.FloatList: retStr = "floatlist"; break;
			}
			return retStr;
		}
		public string valueTypeString()
		{
			string retStr = "";

			switch (Type)
			{
				case DataType.Float: retStr = "FloatVal"; break;
				case DataType.Int: retStr = "IntVal"; break;
				case DataType.Bool: retStr = "boolval"; break;
				case DataType.Mesh: retStr = "Mesh"; break;
				case DataType.Option: retStr = "Intval"; break;
				case DataType.Color: retStr = "Color"; break;
				case DataType.FloatList: retStr = "FloatList"; break;
				case DataType.Vector3: retStr = "Vector3"; break;
			}
			return retStr;
		}
		public string returnString()
		{
			string retStr = "";

			switch (Type)
			{
				case DataType.Float: retStr = "0"; break;
				case DataType.Int: retStr = "0"; break;
				case DataType.Bool: retStr = "true"; break;
				case DataType.Mesh: retStr = "null"; break;
				case DataType.Option: retStr = "0"; break;
			}
			return retStr;
		}




		public bool isEditable = true;

		// parameter type
		// Used as a convenience to group lists of parameters by their roll
		public enum ParameterType { None, Input, TextureControl, BoundsControl, GeometryControl, Output, PositionControl, DerivedValue, Subparameter, Hidden, Base };




		[SerializeField]
		private ParameterType m_ptype = ParameterType.GeometryControl;
		public ParameterType PType
		{
			get { return m_ptype; }
			set { m_ptype = value; }
		}

		public Axis axis = Axis.NONE;


		[SerializeField]
		public LineType lineType;



		public bool exposeAsInterface = false;

		public float perlinScale = 1;
		public float perlinVariance = 3;
		/*
		 *  expressions
		 * A Ripple is simply an instruction for what other variables should
		 * change when the value for this parameter has been changed
		 * 
		 * The change event is issued when a value is set, and the change 
		 * requests are propegated with a GUID to avoid cycles.
		 * 
		 * and example of a relation for the radius parameter is radius.relations[0]="diam=radius*2"
		 * while the diam parameter might have diam.relations[0]="radius=diam/2"
		 * 
		 * each expression is parsed only when the parameter value is change.
		 * 
		 * Note: expressions need not be inverse. For example, wh parameter a is cahnged:
		 * a.relations[0]="b=a*4"
		 * 
		 * and:
		 * b.relations[0]="c=b+2
		 * 
		 * In other words, the designer may deside that when b is altered, a is not.
		 */
		[SerializeField]
		public List<string> expressions = new List<string>();



		// CHeange - get rid of this
		// and implement a thread-safe ripple GUID with a count govenor ta boot
		private string ripple_guid;





		[System.NonSerialized]
		public bool isDisplayed;


		// The above is used to create the following runtime List
		// on deserialization
		[System.NonSerialized]
		public Parameters _parameters;
		public Parameters parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new Parameters();
				return _parameters;
			}
			set
			{
				_parameters = value;
			}
		}
		public AXParameter getParameter(string n)
		{
			return parameters.Find(x => x.Name.Equals(n));
		}






		// Data holders.
		// This parameter will use only one of the following, but since it encapsulates various
		// data types, it needs to have internal storage of all of the possible types it can be.

		// FLOAT

		public float val;
		public float FloatVal
		{
			get { return val; }
			set
			{
				//Debug.Log ("SET " + Name + "="+value);
				val = value;
			}
		}

		// INT

		public int intval = 1;
		public int IntVal
		{
			get { return intval; }
			set
			{
				//Debug.Log ("SET " + Name + "="+value);
				intval = value;
			}
		}

		// COLOR
		public Color colorVal;

		// VECTOR_3
		public Vector3 vector3;

		// PLANE

		public Plane m_plane;
		public Plane plane
		{
			get { return m_plane; }
			set { m_plane = value; }
		}

		public List<float> floats;

		// STRING_VAL
		public string StringVal;

		public List<string> optionLabels;


		public AnimationCurve animationCurve;

		public Rigidbody rigidbody;




		/* CHECK FOR OUTPUT CYCLE
		 *
		 */
		public static bool checkForCycle(AXParameter p1, AXParameter p2)
		{
			if (!p1.parametricObject.hasInputsConnected)
				return false;
			// p1 is being linked to an input of p2
			// Is p1 already downstream of p2?
			List<string> visitedGuids = new List<string>();
			//Debug.Log("Start "+p1.Name +" to "+ p2.Name);
			visitedGuids.Add(p1.parametricObject.Guid);

			return checkForCycle(p1, p2, ref visitedGuids);
		}

		public static bool checkForCycle(AXParameter p1, AXParameter p2, ref List<string> visitedGuids)
		{
			if (visitedGuids.Contains(p2.parametricObject.Guid))
			{
				//Debug.Log("Contains! " + p2.parametricObject.Name + "." + p2.Name);
				return true;
			}
			// Is p1 a dependent of p2?

			// check all outputs

			foreach (AXParameter output_p in p2.parametricObject.getAllSplineAndMeshParameters())
			{
				if (output_p.Dependents != null && output_p.Dependents.Count > 0)
				{
					foreach (AXParameter d in output_p.Dependents)
					{
						if (d.parametricObject.Guid == p1.parametricObject.Guid)
							return true;

						bool isDownstream = checkForCycle(p1, d, ref visitedGuids);

						if (isDownstream)
							return true;
					}
				}
			}



			visitedGuids.Add(p2.parametricObject.Guid);

			return false;
		}


		public void sendWasAlteredEvent()
		{
			if (WasAltered != null)
				WasAltered();

		}



		/* RECURSION
		 * This will propagate either along the straight dependency chain or
		 * if a relation exists, then through the relation via an expression
		 * 
		 * 
		 */
		public void setValue(float value)
		{
			//Debug.Log("SET_VALUE");
			initiatePARAMETER_Ripple_setFloatValueFromGUIChange(value);
			//parametricObject.model.isAltered();
		}

		public void initiatePARAMETER_Ripple_setFloatValueFromGUIChange(float value)
		{
			//Debug.Log ("PARAMETER::INITIATE RIPPLE _FLOAT: " +parametricObject.Name+" || "+Name + " || - " + value);

			// This starts a ripple around the graph. 
			// it travels first up until there are no dependsOn,
			// it changes than,
			// and then the ripple goes down the tree of dependents.
			// each dependent may have local PO relations that it must set first as new ripples.
			// The guid is the governeor ThreadSafeAttribute makes sure cycles dependsOn't form in the graph.
			//Debug.Log ("intiateRipple_setFloatValueFromGUIChange from " + Parent.Name + "." + Name + "::" + Guid);

			if (parametricObject == null)
				return;


			value = (value < min) ? min : value;
			value = (value > max) ? max : value;

			//Debug.Log("ripple");

			value = parametricObject.generator.validateFloatValue(this, value);
			FloatVal = value;

			//parametricObject.updateAllHandles();

			List<string> visitedGuids = new List<string>();


			parametricObject.model.latestEditedParameter = this;


			propagateFloatValue(ref visitedGuids, value, null, 0);


			//if (PType == AXParameter.ParameterType.PositionControl)
			//	parametricObject.generator.adjustWorldMatrices();


			//parametricObject.model.drawMeshes();
			//parametricObject.model.sw.milestone("ripple ");

		}

		public void setValue(int value)
		{
			initiateRipple_setIntValueFromGUIChange(value);
			parametricObject.model.isAltered();
		}

		public void initiateRipple_setIntValueFromGUIChange(int value)
		{

			if (parametricObject == null)
				return;



			List<string> visitedGuids = new List<string>();


			value = (value < intmin) ? intmin : value;
			value = (value > intmax) ? intmax : value;

			if (Name == "segs" && value < 1)
				value = 1;

			parametricObject.model.latestEditedParameter = this;

			propagateIntValue(ref visitedGuids, value, null, 0);


		}

		public void propagateFloatValue(ref List<string> visitedGuids, float value, List<string> symbolsUsedAlready, int governor)
		{

			// this parameter is connected via Relations to other parameters
			// which are connected in turn to other parameters. 
			// Follow the graph of Relation connections.
			// Relations are stored cannonically in the model
			// and non-serialized live links are generated onEnable/onDeserialize.

			//Debug.Log(Name+":propagateFloatValue: " +FloatVal +", "+ value);
			//if (NearlyEqual(FloatVal, value, .0000000001f))


			//Debug.Log("******* ****** propagateFloatValue "+parametricObject.Name+"__"+Name+"__ - " + governor);
			if (governor++ > 20)
			{
				Debug.Log("governor hit)");
				return;
			}

			governor++;

			//Debug.Log ("SETTING FloatVal " + Parent.Name + "." +Name + " to " + value + " -- " + visitedGuids.Contains (Guid));
			if (visitedGuids.Contains(Guid))
				return;

			//if (governor > 1)
			visitedGuids.Add(Guid);

			//Debug.Log ("OK, GO AHEAD AND  SET IT! " + Parent.Name + "." +Name + " ["+Guid+"] =" + value);

			// ACTUALLY SET THIS PARAMETER'S VALUE
			FloatVal = value;
			parametricObject.updateAllHandles();




			//parametricObject.generator.parameterWasModified(this);





			if (WasAltered != null)
				WasAltered();

			// For re-generating and for re-adjustWorldMatricesadjustWorldMatrices
			//Debug.Log("set altered: " + parametricObject.Name + " from " + Name);
			parametricObject.setAltered();

			//if (!shouldPropagate)
			//return;










			//Debug.Log(" BEG INTERNAL EXPRESSIONS -------------------------------------------------------------");
			for (int i = 0; i < expressions.Count; i++)
			{
				if (string.IsNullOrEmpty(expressions[i]))
					continue;

				//Debug.Log (expressions[i]);
				string expression = Regex.Replace(expressions[i], @"\s+", "");  // remove spaces
																				//Debug.Log("internal expression["+i+"] " + expression);

				// FIND THE PARAM BEING SET BY THIS EXPRESSION
				string param = expression.Substring(0, expression.IndexOf("=", StringComparison.InvariantCulture));



				// HAS THIS PARAM ALREADY BEEN SET DURING THIS CYCLE?
				// prevent cycle of internal expressions
				if (symbolsUsedAlready != null && symbolsUsedAlready.Contains(param))
				{
					//Debug.Log ("FLOAT: " + param + " ALREADY USED");
					continue;
				}


				//Debug.Log ("FLOAT: NOW DO " + expressions[i] );

				string definition = expression.Substring(expression.IndexOf("=", StringComparison.InvariantCulture) + 1);

				//Debug.Log(definition);

				// PARSE THE DEFINITION: RESULT WILL HAVE A LIST OF SYMBOLS FOUND IN THE DEFINITION
				MathParseResults mpr = parametricObject.parseMathWithResults(definition);

				//Debug.Log("mpr.result = "+mpr.result);
				//foreach (string s in mpr.symbolsFound)
				//Debug.Log("############ > " + s);

				try
				{
					AXParameter ep = Parent.getParameter(param);
					if (ep.Type == DataType.Float)
						ep.propagateFloatValue(ref visitedGuids, mpr.result, mpr.symbolsFound, governor);
					else if (ep.Type == DataType.Int)
						ep.propagateIntValue(ref visitedGuids, Mathf.CeilToInt(mpr.result), mpr.symbolsFound, governor);
				}
				catch
				{
					//Debug.Log("Handle error: Please check syntax of: \"" + expressions[i]+"\"");
				}

			}
			//Debug.Log("// END INTERNAL EXPRESSIONS -------------------------------------------------------------");



			//Debug.Log ("NOW FOR RELATIONS.... "+relations.Count);
			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------
			foreach (AXRelation r in relations)
			{

				//Debug.Log ("(-) (-) (-) -- (-)--() >> Fire relation: " + r.toString());
				//string expr = (r.pA == this) ? r.expression_AB.Replace("IN", r.pA.Name) : r.expression_BA.Replace("IN", r.pB.Name);

				if (r.pA == null || r.pB == null)
					continue;

				string expr = (r.pA == this) ? r.expression_AB : r.expression_BA;
				// Debug.Log("expr="+expr);
				if (!string.IsNullOrEmpty(expr))
				{
					AXParameter client = (r.pA == this) ? r.pB : r.pA;
					try
					{

						//Debug.Log (" ..... propagateFloatValue using: " + client.parametricObject.Name + "."+ client.Name + " = " + expr  + ", regExpr="+ regExpr + ", value="+ valToUse);


						client.propagateFloatValue(ref visitedGuids, (float)parametricObject.parseMath_ValueOnly(Regex.Replace(expr, @"\s+", "")), null, governor);
						//Debug.Log(" ..... DONE propagateFloatValue using: " + client.Name + " :: ==" + expr + "==");

					}
					catch
					{
						//Debug.Log ("bad expresion: " + client.Name  + ": " + expr);
					}
				}
			}
			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------


			// LULU

			//if (PType == AXParameter.ParameterType.PositionControl)
			parametricObject.generator.adjustWorldMatrices();



		}







		public void propagateIntValue(ref List<string> visitedGuids, int value, List<string> symbolsUsedAlready, int governor)
		{
			//			if (value < 0)
			//				value = 0;

			// this parameter is connected via Relations to other parameters
			// which are connected in turn to other parameters. 
			// Follow the graph of Relation connections.
			// Relations are stored cannonically in the model
			// and non-serialized live links are generated onEnable.

			if (governor++ > 20)
			{
				Debug.Log("governor hit)");
				return;
			}

			if (visitedGuids.Contains(Guid))
				return;

			visitedGuids.Add(Guid);



			//Debug.Log("Propagate int values");


			IntVal = Math.Max(intmin, value);

			if (WasAltered != null)
				WasAltered();

			parametricObject.setAltered();


			// BEG INTERNAL EXPRESSIONS -------------------------------------------------------------
			for (int i = 0; i < expressions.Count; i++)
			{
				if (string.IsNullOrEmpty(expressions[i]))
					continue;

				string expression = Regex.Replace(expressions[i], @"\s+", "");  // remove spaces

				string param = expression.Substring(0, expression.IndexOf("=", StringComparison.InvariantCulture));

				// prevent cycle of internal expressions
				if (symbolsUsedAlready != null && symbolsUsedAlready.Contains(param))
				{
					//Debug.Log ("INT: " + param + " ALREADY USED");
					continue;
				}
				//Debug.Log ("INT: NOW DO " + expressions[i] );

				string definition = expression.Substring(expression.IndexOf("=", StringComparison.InvariantCulture) + 1);
				MathParseResults mpr = parametricObject.parseMathWithResults(definition);
				try
				{
					AXParameter ep = Parent.getParameter(param);
					if (ep.Type == DataType.Int)
						ep.propagateIntValue(ref visitedGuids, Mathf.CeilToInt(mpr.result), mpr.symbolsFound, governor);
					else if (ep.Type == DataType.Float)
						ep.propagateFloatValue(ref visitedGuids, mpr.result, mpr.symbolsFound, governor);
				}
				catch
				{
					Debug.Log("Handle error: Please check syntax of: \"" + expressions[i] + "\"");
				}

			}
			// END INTERNAL EXPRESSIONS -------------------------------------------------------------


			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------
			foreach (AXRelation r in relations)
			{
				//string expr = (r.pA == this) ? r.expression_AB.Replace("IN", r.pA.Name) : r.expression_BA.Replace("IN", r.pB.Name);
				string expr = (r.pA == this) ? r.expression_AB : r.expression_BA;
				if (!string.IsNullOrEmpty(expr))
				{
					AXParameter client = (r.pA == this) ? r.pB : r.pA;
					try
					{
						client.propagateIntValue(ref visitedGuids, Mathf.CeilToInt((float)Parent.parseMath_ValueOnly(expr)), null, governor);
					}
					catch
					{
						Debug.Log("bad expresion: " + expr);
					}
				}
			}
			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------
			parametricObject.generator.adjustWorldMatrices();
		}










		public void showAllRelated(bool show)
		{
			foreach (AXRelation r in relations)
			{
				AXParameter pR = (r.pA == this) ? r.pB : r.pA;

				//Debug.Log (pR.Parent.Name + " show " + pR.PType);

				//Debug.Log ("open to root");
				pR.openToRoot(0);
			}
		}

		public void foldAllRelated()
		{
			foreach (AXRelation r in relations)
			{
				AXParameter pR = (r.pA == this) ? r.pB : r.pA;

				//Debug.Log (pR.Parent.Name + " show " + pR.PType);

				//Debug.Log ("fold to root");
				pR.foldToRoot(0);
			}
		}




















		// BOOL

		// RECURSION

		public bool boolval = false;


		public void setValue(bool value)
		{
			initiateRipple_setBoolValueFromGUIChange(value);
			parametricObject.model.isAltered();
		}

		public void initiateRipple_setBoolValueFromGUIChange(bool value)
		{
			// This starts a ripple around the graph. 
			// it travels first up until there are no dependsOn,
			// it changes than,
			// and then the ripple goes down the tree of dependents.
			// each dependent may have local PO relations that it must set first as new ripples.
			// The guid is the governeor ThreadSafeAttribute makes sure cycles dependsOn't form in the graph.

			//List<string> visitedGuids = new List<string>();
			//setBoolValueUpwards(newRippleGUID, value);

			if (parametricObject == null)
				return;


			List<string> visitedGuids = new List<string>();
			propagateBoolValue(ref visitedGuids, value, null, 0);

			if (WasAltered != null)
				WasAltered();

		}






		public void propagateBoolValue(ref List<string> visitedGuids, bool value, List<string> symbolsUsedAlready, int governor)
		{

			// this parameter is connected via Relations to other parameters
			// which are connected in turn to other parameters. 
			// Follow the graph of Relation connections.
			// Relations are stored cannonically in the model
			// and non-serialized live links are generated onEnable.

			if (governor++ > 20)
			{
				Debug.Log("governor hit)");
				return;
			}

			if (visitedGuids.Contains(Guid))
				return;

			visitedGuids.Add(Guid);


			boolval = value;

			parametricObject.setAltered();



			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------
			foreach (AXRelation r in relations)
			{
				//string expr = (r.pA == this) ? r.expression_AB.Replace("IN", r.pA.Name) : r.expression_BA.Replace("IN", r.pB.Name);
				string expr = (r.pA == this) ? r.expression_AB : r.expression_BA;
				if (!string.IsNullOrEmpty(expr))
				{
					AXParameter client = (r.pA == this) ? r.pB : r.pA;
					try
					{
						client.propagateBoolValue(ref visitedGuids, value, null, governor);
					}
					catch
					{
						Debug.Log("bad expresion: " + expr);
					}
				}
			}
			// BEG EXTERNAL RELATIONS ---------------------------------------------------------------
			parametricObject.generator.adjustWorldMatrices();
		}






















		public void setBoolValueUpwards(string guid, bool value)
		{
			if (ripple_guid == guid)
				return;

			// This parameter has not yet been set in this ripple.

			if (DependsOn != null && DependsOn.ripple_guid != guid)
				// call up to the top before acting.
				DependsOn.setBoolValueUpwards(guid, value);
			else
			{
				// if the ripple made it here, above has been taken care of.
				// Do this, then go down...
				setBoolValueForThisAndDownwards(guid, value);
			}

			parametricObject.setAltered();


		}

		public void setBoolValueForThisAndDownwards(string guid, bool value)
		{
			// This descends only downward...

			if (ripple_guid == guid)
				return;

			ripple_guid = guid;

			boolval = value;

			// internal expression:
			// -- for now, don't support internal expressions for bool

			if (dependents != null)
			{
				foreach (AXParameter d in dependents)
					d.setBoolValueForThisAndDownwards(guid, value);
			}

			parametricObject.setAltered();

		}














		public float m_margin = 0f;
		public float Margin
		{
			get { return m_margin; }
			set { m_margin = value; }
		}





		// domain of the parameter (float)
		public float max = 10000;
		public float min = -10000;

		// domain of the parameter (int)
		public int intmax = 10000;
		public int intmin = -10000;

		// If bound to an size axis, then 
		// this parameter will be controlled
		// by an output consumer without the user
		// having to make the link specifically
		//  "None|X|Y|Z"
		public Axis sizeBindingAxis = 0;


		public AXSpline _spline;
		public AXSpline spline
		{
			get
			{
				//future: call function that passes this guid and stops if it encounters it again.
				if (DependsOn != null)
				{
					if (negativeOfDependsValue)
						_spline = DependsOn._spline;
					else
						_spline = DependsOn._spline;
				}
				return _spline;
			}
			set { _spline = value; }
		}
		public AXSpline getParentShiftedSpline()
		{
			if (spline != null)
				return spline.clone(Parent.floatValue("Trans_X"), Parent.floatValue("Trans_Y"), Parent.floatValue("Rot_Z"));
			return null;
		}




		// FLIP_X
		public bool flipX;

		// REVERSE
		public bool reverse;

		// SYMMETRY
		public bool symmetry;
		public float symSeperation;

		public bool isCCW()
		{
			if (polyTree != null && polyTree.Childs != null && polyTree.Childs.Count > 0)
				return Clipper.Orientation(polyTree.Childs[0].Contour);
			else if (paths != null && paths.Count > 0)
				return Clipper.Orientation(paths[0]);


			return true;

		}

		public float breakGeom = 0;
		public float breakNorm = 60;

		// ROUNDNESS 
		public float roundness;
		[System.NonSerialized]
		public AXParameter roundness_p;

		// OFFSET
		public float offset;
		[System.NonSerialized]
		public AXParameter offset_p;

		// THICKNESS
		public bool hasThickness;
		public float thickness;
		[System.NonSerialized]
		public AXParameter wallthick_p;

		// SEGLEN
		// The maximum segment length in a SHape - this is the key to subdivision
		public float seglen = 10;
		//		public float seglen 
		//		{
		//			get {
		//				if (_seglen == 0)
		//					_seglen = 10;
		//				return _seglen;
		//			}
		//			set {
		//				_seglen = value;
		//			}
		//
		//		}

		// This sets the seglen based on the current bouds of the form
		public float subdivision = 0;




		// CLIPPER PARAMETERS 
		public ShapeState shapeState = ShapeState.Closed;

		public bool drawClosed = false;


		// modifiers to operate before out paths are created.
		public enum MergeType { Solid, Void, Line };
		public MergeType mergeType; // renaming of subj/clip in clipper


		// to dictate the role it plays in various clip operation types.
		// ptSubject or ptClip (in ctDifference, this could be translated as solid and void).
		public AXClipperLib.PolyType polyType;

		// how the line segments are connected.
		// jtSquare, jtRounded, etc.
		public AXClipperLib.JoinType joinType = AXClipperLib.JoinType.jtMiter;

		// closed: etClosedLine, etClosedPolygon
		// opens: etOpenButt, etOpenRound, etOpenSquare
		public AXClipperLib.EndType endType = AXClipperLib.EndType.etClosedPolygon;

		public enum OpenEndType { Butt, Square, Round };
		public OpenEndType openEndType; // renaming of subj/clip in clipper

		// This will get translated to ClipperOffset.ArcTolerance by segsPer90
		public float arcTolerance = 50f;


		// clipType determines how the generation will combine the inputs
		// i.e., ctDifference, ctUnion, ctIntersection
		// However, usage has shifted. Now all three types are made,
		// but this could be for how the shapes are combined with local logic?
		public AXClipperLib.ClipType clipType;


		// Using Clipper -- these are outputs!
		public Paths controlPaths;
		public Paths transformedControlPaths;
		public Paths paths;
		public AXClipperLib.PolyTree polyTree;
		public AXClipperLib.Clipper clipper;


		[System.NonSerialized]
		public Paths transformedButUnscaledOutputPaths;

		[System.NonSerialized]
		public Paths transformedAndScaledButNotOffsetdOutputPaths;

		[System.NonSerialized]
		public Paths transformedFullyAndOffsetdButNotThickenedOutputPaths;



		public float area;
		public float volume;



		public void clearAllPaths()
		{
			controlPaths = null;
			transformedControlPaths = null;
			paths = null;
			polyTree = null;
			transformedButUnscaledOutputPaths = null;
			transformedAndScaledButNotOffsetdOutputPaths = null;
			transformedFullyAndOffsetdButNotThickenedOutputPaths = null;
		}

		public bool pathsAreNull()
		{
			if (controlPaths == null
				&& transformedControlPaths == null
				&& paths == null
				&& polyTree == null
				&& transformedButUnscaledOutputPaths == null
				&& transformedFullyAndOffsetdButNotThickenedOutputPaths == null
			)
				return true;
			return false;
		}

		public bool hasPolytreeItems
		{
			get
			{
				return (polyTree != null && polyTree.Childs != null && polyTree.Childs.Count > 0);
			}
		}

		public bool hasPaths
		{
			get
			{
				return (paths != null && paths.Count > 0);
			}
		}



		/* Control Paths are generated or clicked in FreeForm. 
		 * They they are pre-transforms.
		 * And they are never the product of a clipper operation.
		 * 
		 * The following function gets transformed versions that are tehn stored in this.paths
		 */
		public Paths getControlPaths()
		{
			if (Parent == null)
				return null;
			return controlPaths;
		}
		public Paths getTransformedControlPaths()
		{
			if (parametricObject == null)
				return null;

			//if (polyTree != null)
			//	return Archimatix.transformPaths(Clipper.PolyTreeToPaths(polyTree),  Parent.floatValue("Trans_X"), Parent.floatValue("Trans_Y"), Parent.floatValue("Rot_Z"));
			//else if (paths != null)
			AX.Generators.Generator2D gener2D = parametricObject.generator as AX.Generators.Generator2D;
			Paths tmpPaths = AXGeometry.Utilities.transformPaths(controlPaths, gener2D.transX, gener2D.transY, -gener2D.rotZ);

			if (flipX)
				tmpPaths = AXGeometry.Utilities.transformPaths(tmpPaths, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1)));

			return tmpPaths;
			//return null;
		}


		public bool hasPathsReady()
		{
			if (Parent == null)
				return false;

			if (polyTree != null)
			{
				if ((polyTree.Contour != null && polyTree.Contour.Count > 0) || (polyTree.Childs != null && polyTree.Childs.Count > 0))

					return true;
			}
			else if (paths != null && paths.Count > 0)
			{
				//Debug.Log("returning paths!");

				return true;
			}

			return false;
		}






		public Paths getPaths()
		{

			if (parametricObject == null)
				return null;

			//Debug.Log("******** paths="+paths + " :: " + paths.Count);
			if (polyTree != null)
			{

				//Paths retPaths =  (shapeState == ShapeState.Open) ? Clipper.OpenPathsFromPolyTree(polyTree) : Clipper.PolyTreeToPaths(polyTree);
				Paths retPaths = Clipper.PolyTreeToPaths(polyTree);
				return retPaths;
			}
			else if (paths != null && paths.Count > 0)
			{
				//Debug.Log("returning paths!");

				return paths;
			}


			return null;
		}
		public Paths getClonePaths()
		{

			if (parametricObject == null)
				return null;

			//Debug.Log("paths="+paths + " :: " + paths.Count);

			Paths tmpPaths;

			if (polyTree != null)
			{

				tmpPaths = AX.Generators.Generator2D.clonePaths(Clipper.PolyTreeToPaths(polyTree));

				//if (reverse)
				//	for(int i=0; i<tmpPaths.Count; i++)
				//		tmpPaths[i].Reverse();

				return tmpPaths;
			}
			else if (paths != null && paths.Count > 0)
			{
				//Debug.Log("returning paths!");

				tmpPaths = AX.Generators.Generator2D.clonePaths(paths);
				//if (reverse)
				//	for(int i=0; i<tmpPaths.Count; i++)
				//		tmpPaths[i].Reverse();

				return tmpPaths;

			}


			return null;
		}






		public void doFlipX()
		{

			if (polyTree != null)
			{


			}
			else if (paths != null && paths.Count > 0)
				AXGeometry.Utilities.transformPaths(paths, Matrix4x4.TRS(Vector2.zero, Quaternion.identity, new Vector3(-1, 1, 1)));

		}

		public void doReverse()
		{
			if (polyTree != null)
				AXGeometry.Utilities.reversePolyTree(polyTree);
			else if (paths != null && paths.Count > 0)
				AXGeometry.Utilities.reversePaths(ref paths);


			/*
		if (transformedControlPaths != null)
			transformedControlPaths.Reverse();

		if (transformedButUnscaledOutputPaths != null)
			transformedButUnscaledOutputPaths.Reverse();

		if (transformedAndScaledButNotOffsetdOutputPaths != null)
			transformedAndScaledButNotOffsetdOutputPaths.Reverse();

		if (transformedFullyAndOffsetdButNotThickenedOutputPaths != null)
			transformedFullyAndOffsetdButNotThickenedOutputPaths.Reverse();
		*/
		}

		public Paths getTransformedSubjPaths()
		{
			if (polyTree != null)
				return Clipper.PolyTreeToPaths(polyTree);
			else if (paths != null)
				return paths;

			return null;
		}


		public Paths getTransformedHolePaths()
		{
			if (polyTree != null)
			{

				Paths retPaths = new Paths();
				foreach (PolyNode node in polyTree.Childs)
				{
					foreach (PolyNode subnode in node.Childs)
					{
						if (subnode.IsHole)
							retPaths.Add(subnode.Contour);
					}
				}
				//return Archimatix.transformPaths(retPaths,  Parent.floatValue("Trans_X"), Parent.floatValue("Trans_Y"), Parent.floatValue("Rot_Z"));
				return retPaths;
			}


			return null;
		}




		// Wow - the mesh of a parameter was being serialized 
		// which led to unecessary saving of transient data.

		//public Mesh		mesh;

		// Now the Mesh is ephemeral only the AXModel itself
		// will give permannce to the meshs by creating GameObjects to be 
		// children of the "RootModel" GameObject when the time comes.
		[System.NonSerialized]
		private Mesh m_mesh;
		public Mesh mesh
		{
			get { return m_mesh; }
			set { m_mesh = value; }
		}

		// Allow the transport and passthrough of mesh sets
		// these are also not serialized
		[System.NonSerialized]
		private List<AXMesh> _meshes = new List<AXMesh>();
		public List<AXMesh> meshes
		{
			get { return _meshes; }
			set { _meshes = value; }
		}


		public bool hasInputSocket = true;
		public bool hasOutputSocket = true;









		[System.NonSerialized]
		public bool isEditing = false;

		[System.NonSerialized]
		public bool shouldFocus = false;

		[System.NonSerialized]
		public bool to_delete = false;

		// If you bind this parameter to one from a different ParametricObjecct
		// you may use the negative of the master's value, which is commonly needed.
		public bool negativeOfDependsValue = false;







		// ** GRAPH RELATIONSHIPS ** //

		// These are the persistent linkages through serialization
		// of the form model_key%po_key%p_key
		// * NOTE: when a parametricobject or a paramter name is changed, these values in its dependents must be updated
		public string dependsOnKey = null;




		// These are live values only - for convenience
		// * don't serialize these, it will only make copies on deserialize
		// The parent is set when the PO that owns this Parameter (in its serialized List)
		// instantiates it or is creating graph links
		[System.NonSerialized]
		private AXParametricObject parent = null;
		public AXParametricObject Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		[System.NonSerialized]
		private AXParameter dependsOn = null;
		public AXParameter DependsOn
		{
			get { return dependsOn; }
			set
			{
				dependsOn = value;
				if (dependsOn == null)
					isDependent = false;
				else
					isDependent = true;
			}
		}

		public bool dependsOnCurveIsVisible;
		public Vector2 inputGUIPosition;
		public Vector2 dependsOnSrcGUIPosition;

		public bool isDependent;






		[System.NonSerialized]
		private List<AXParameter> dependents = new List<AXParameter>();
		public List<AXParameter> Dependents
		{
			get { return dependents; }
			set { dependents = value; }
		}


		// RELATIONS GRAPH
		[System.NonSerialized]
		private List<AXRelation> m_relations = new List<AXRelation>();
		public List<AXRelation> relations
		{
			get { return m_relations; }
			set { m_relations = value; }
		}












		public AXParameter clone(bool sameGuid = false)
		{
			AXParameter c = new AXParameter(Type, Name, hasInputSocket, hasOutputSocket);

			c.copyValues(this);

			// copy expressions
			c.expressions = new List<string>(expressions);

			if (sameGuid)
				c.Guid = Guid;

			return c;
		}

		public void copyValues(AXParameter p)
		{

			Type = p.Type;
			PType = p.PType;

			val = p.val;
			min = p.min;
			max = p.max;

			intval = p.intval;
			intmin = p.intmin;
			intmax = p.intmax;

			boolval = p.boolval;

			sizeBindingAxis = p.sizeBindingAxis;

		}

		// JSON SERIALIZATION
		public string asJSON()
		{
			/* At some point, we could use a more generalized serializer,
			 * but it would have to recognize the NonSerialized members and not pursue 
			 * their graph links
			 * 
			 */

			StringBuilder sb = new StringBuilder();

			sb.Append("{");

			sb.Append("\"guid\":\"" + Guid + "\"");
			sb.Append(", \"name\":\"" + Name + "\"");

			sb.Append(", \"data_type\":" + m_type.GetHashCode());
			sb.Append(", \"parameter_type\":" + m_ptype.GetHashCode());

			sb.Append(", \"shape_state\":" + shapeState.GetHashCode());
			sb.Append(", \"merge_type\":" + mergeType.GetHashCode());
			sb.Append(", \"poly_type\":" + polyType.GetHashCode());
			sb.Append(", \"end_type\":" + endType.GetHashCode());
			sb.Append(", \"join_type\":" + joinType.GetHashCode());
			sb.Append(", \"wallthick\":" + thickness);
			sb.Append(", \"offset\":" + offset);
			sb.Append(", \"roundness\":" + roundness);

			sb.Append(", \"seglen\":" + seglen);


			sb.Append(", \"breakGeom\":" + breakGeom);
			sb.Append(", \"breakNorm\":" + breakNorm);


			sb.Append(", \"reverse\":\"" + reverse + "\"");
			sb.Append(", \"symmetry\":\"" + symmetry + "\"");
			sb.Append(", \"symSeperation\":" + symSeperation);

			sb.Append(", \"flipX\":\"" + flipX + "\"");

			// graph support
			if (dependsOnKey != null && dependsOnKey != "")
				sb.Append(", \"dependsOnKey\":\"" + dependsOnKey + "\"");

			sb.Append(", \"hasInputSocket\":\"" + hasInputSocket + "\"");
			sb.Append(", \"hasOutputSocket\":\"" + hasOutputSocket + "\"");

			sb.Append(", \"val\":" + val);
			sb.Append(", \"intval\":" + intval);
			sb.Append(", \"boolval\":\"" + boolval + "\"");
			sb.Append(", \"colorVal\":" + AXJson.ColorToJSON(colorVal));

			sb.Append(", \"min\":" + min);
			sb.Append(", \"max\":" + max);
			sb.Append(", \"intmin\":" + intmin);
			sb.Append(", \"intmax\":" + intmax);

			sb.Append(", \"sizeBindingAxis\":" + (int)sizeBindingAxis);

			sb.Append(", \"animationCurve\":" + AXJson.AnimationCurveToJSON(animationCurve));



			sb.Append(", \"exposeAsInterface\":" + exposeAsInterface);



			if (floats != null && floats.Count > 0)
				sb.Append(", \"floats\":" + AXJson.FloatListToJSON(floats));


			if (expressions != null && expressions.Count > 0)
				sb.Append(", \"expressions\":" + AXJson.StringListToJSON(expressions));

			// end parameters

			sb.Append("}");

			return sb.ToString();


		}

		public static AXParameter fromJSON(AX.SimpleJSON.JSONNode jn)
		{

			AXParameter p = new AXParameter();
			p.Guid = jn["guid"];
			p.Name = jn["name"];


			p.Type = (AXParameter.DataType)jn["data_type"].AsInt;
			p.PType = (AXParameter.ParameterType)jn["parameter_type"].AsInt;

			p.shapeState = (ShapeState)jn["shape_state"].AsInt;
			p.mergeType = (MergeType)jn["merge_type"].AsInt;
			p.polyType = (PolyType)jn["poly_type"].AsInt;
			p.endType = (AXClipperLib.EndType)jn["end_type"].AsInt;
			p.joinType = (AXClipperLib.JoinType)jn["join_type"].AsInt;
			p.thickness = jn["wallthick"].AsFloat;
			p.offset = jn["offset"].AsFloat;
			p.roundness = jn["roundness"].AsFloat;

			p.seglen = jn["seglen"].AsFloat;


			p.breakGeom = jn["breakGeom"].AsFloat;
			p.breakNorm = jn["breakNorm"].AsFloat;


			p.reverse = jn["reverse"].AsBool;
			p.symmetry = jn["symmetry"].AsBool;
			p.symSeperation = jn["symSeperation"].AsFloat;

			p.flipX = jn["flipX"].AsBool;

			// graph support
			if (jn["dependsOnKey"] != null)
				p.dependsOnKey = jn["dependsOnKey"];

			p.hasInputSocket = jn["hasInputSocket"].AsBool;
			p.hasOutputSocket = jn["hasOutputSocket"].AsBool;


			p.val = jn["val"].AsFloat;
			p.intval = jn["intval"].AsInt;
			p.boolval = jn["boolval"].AsBool;
			p.sizeBindingAxis = (Axis)jn["sizeBindingAxis"].AsInt;

			if (jn["min"] != null)
				p.min = jn["min"].AsFloat;

			if (jn["max"] != null)
				p.max = jn["max"].AsFloat;

			if (jn["intmin"] != null)
				p.intmin = jn["intmin"].AsInt;

			if (jn["intmax"] != null)
				p.intmax = jn["intmax"].AsInt;

			if (jn["exposeAsInterface"] != null)
				p.exposeAsInterface = jn["exposeAsInterface"].AsBool;

			if (jn["colorVal"] != null)
			{

				p.colorVal = AXJson.ColorFromJSON(jn["colorVal"]);
			}
			if (jn["animationCurve"] != null)
				p.animationCurve = AXJson.AnimationCurveFromJSON(jn["animationCurve"]);


			if (jn["floats"] != null)
				p.floats = AXJson.FloatListFromJSON(jn["floats"]);


			if (jn["expressions"] != null)
				p.expressions = AXJson.StringListFromJSON(jn["expressions"]);



			return p;
		}


		// CONSTRUCTORS //

		public AXParameter(string name, float v, float mn, float mx) : base(name)
		{
			Type = DataType.Float;
			val = v;
			min = mn;
			max = mx;
		}

		public AXParameter(string name, int i) : base(name)
		{
			Type = DataType.Int;

			intval = i;
			intmin = 3;
			intmax = 32;

		}
		public AXParameter(string name, int i, int mn, int mx) : base(name)
		{
			Type = DataType.Int;
			m_name = name;
			intval = i;
			intmin = mn;
			intmax = mx;

		}

		public AXParameter(string name, bool b) : base(name)
		{
			Type = DataType.Bool;
			boolval = b;
		}

		public AXParameter(DataType type, string name) : base(name)
		{
			Type = type;
		}

		public AXParameter(DataType type, ParameterType p_type, string name) : base(name)
		{
			Type = type;
			PType = p_type;

			if (p_type == ParameterType.Output && (type == DataType.Mesh || type == DataType.Plane))
				hasInputSocket = false;
			else if (p_type == ParameterType.Input && type == DataType.Mesh)
				hasOutputSocket = true;
			//init();
		}

		public AXParameter(DataType type, string name, bool isock, bool osock) : base(name)
		{
			Type = type;
			hasInputSocket = isock;
			hasOutputSocket = osock;
			//init();
		}

		public AXParameter() : base("new")
		{
			Type = DataType.Float;
		}

		public AXParameter(string name) : base(name)
		{
			Type = DataType.Float;
		}

		public AXParameter(SerializableParameter sp) : base(sp.Name)
		{
			m_guid = sp.guid;
			Type = sp.Type;

			val = sp.val;
			intval = sp.intval;
			boolval = sp.boolval;

		}



		public void setupReferencesBasedOnDependsOnKey()
		{
			// USE THIS TO INITIALIZE REFERENCES AFTER DESERIALIZATION
			if (dependsOnKey != null && dependsOnKey != "")
			{
				string mo_guid = null;
				string po_guid = null;
				string p_guid = null;

				AXModel mo = null;
				AXParametricObject po = null;
				AXParameter p = null;

				string[] guids = dependsOnKey.Split('%');
				if (guids.Length == 3)
				{
					// has model key
					mo_guid = guids[0];
					po_guid = guids[1];
					p_guid = guids[2];
				}
				else
				{
					// use this model
					po_guid = guids[0];
					p_guid = guids[1];
				}

				if (mo_guid == null)
				{
					mo = Parent.model;
				}
				else
				{
					mo = AXModel.getModelWithGUID(mo_guid);
				}
				if (mo == null)
					return;

				po = mo.getParametricObjectByGUID(po_guid);

				if (po != null)
					p = po.getParameterForGUID(p_guid);

				if (p != null)
				{
					DependsOn = p;
					p.addDependent(this);

				}

			}
		}


		public bool hasRelations()
		{

			if (m_relations != null && m_relations.Count > 0)
				return true;

			return false;

		}
		public bool hasExpressions()
		{


			if (expressions != null && expressions.Count > 0 && !string.IsNullOrEmpty(expressions[0]))
				return true;

			return false;

		}

		public void addDependent(AXParameter p)
		{
			if (p.Parent.Guid == Parent.Guid || AXParameter.checkForCycle(this, p))
				return;

			if (!Dependents.Contains(p))
			{
				Dependents.Add(p);
				if (Parent.generator != null)
					Parent.generator.connectionMadeWith(this, p);

			}

			//Debug.Log("*********************************** Add Dependent");

			//parametricObject.model.remapMaterialTools();


		}
		public void removeDependent(AXParameter p)
		{

			if (Dependents.Contains(p))
			{
				if (p.parametricObject.generator != null)
					p.parametricObject.generator.connectionBrokenWith(p);


				Dependents.Remove(p);
				if (parametricObject.selectedConsumer == p.parametricObject)
					parametricObject.selectedConsumer = null;
			}


			parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();

		}

		public void makeDependentOn(AXParameter p)
		{

			if (p == null)
				return;


			if (p.Parent.Guid == Parent.Guid || AXParameter.checkForCycle(p, this))
				return;

			//Debug.Log ("Make " + Name + " dependent on " + p.Name);



			//Debug.Log("yo 00");
			makeIndependent();
			//Debug.Log("yo 0");
			dependsOnKey = p.Guid;

			DependsOn = p;
			p.addDependent(this);

			//Debug.Log("YO " + this.DependsOn);

			// Debug.Log("yo 1");

			if (parametricObject != null && p.parametricObject != null)
			{
				if (parametricObject.generator != null)
					parametricObject.generator.connectionMadeWith(this, p);

				if (PType == ParameterType.Input && Type == DataType.Mesh && Parent.generator != null)
					parametricObject.generator.onInputMeshAttachedWithBounds(p.Parent);
			}
			// Debug.Log("yo 2");
			parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();
			parametricObject.generator.pollControlValuesFromParmeters();
			parametricObject.getAllInputParameters(true);
			// Debug.Log("yo 3");
			if (p.parametricObject.is2D())
				p.parametricObject.setLocalMatrix();

			parametricObject.generator.adjustWorldMatrices();


			//p.parametricObject.generator.adjustWorldMatrices();
			//Debug.Log (Name + " dependsOn " + dependsOn.Name + " --count="+dependsOn.Dependents.Count);
			// disconnect from former
		}

		public void makeIndependent()
		{
			//Debug.Log (Name+": makeIndependent " + parametricObject.generator);

			if (parametricObject.generator != null)
			{
				parametricObject.generator.connectionBrokenWith(this);
			}

			// Debug.Log("oop 1");
			if (dependsOn != null)
			{
				if (dependsOn.parametricObject.selectedConsumer == parametricObject)
					dependsOn.parametricObject.selectedConsumer = null;

				AXParameter output_p = parametricObject.getParameter("Output Mesh", "Output");
				if (output_p != null)
					output_p.meshes = null;

				dependsOn.removeDependent(this);

				if (dependsOn.parametricObject.selectedConsumer == parametricObject)
					dependsOn.parametricObject.selectedConsumer = null;

				dependsOn.parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();
				dependsOn.parametricObject.generator.pollControlValuesFromParmeters();
				//dependsOn.parametricObject.generator.adjustWorldMatrices();
			}

			DependsOn = null;
			dependsOnKey = null;


			parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();
			parametricObject.generator.pollControlValuesFromParmeters();
			//parametricObject.generator.adjustWorldMatrices();





		}



		public void freeDependents()
		{
			//Debug.Log("free depens " + Dependents.Count);
			foreach (AXParameter d in Dependents)
			{

				if (d.parametricObject.generator != null)
					d.parametricObject.generator.connectionBrokenWith(d);


				d.dependsOnKey = null;
				d.DependsOn = null;

				if (d.parametricObject.generator != null)
					d.parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();
			}
			Dependents.Clear();



			parametricObject.selectedConsumer = null;



			parametricObject.generator.pollInputParmetersAndSetUpLocalReferences();
			parametricObject.generator.pollControlValuesFromParmeters();
			//parametricObject.generator.adjustWorldMatrices();

		}









		public void generateWithThumbnails()
		{
			Debug.Log("Parameter: generateWithThumbnails()");
			//Parent.generateWithThumbnails();
			//if(currentEditor != null)
			//	currentEditor.Repaint();

		}



		public void valueChangedByPropertyDrawer()
		{
			// I think that the value change in a property drawer happens with a delay.
			// So this is called after the immediate gui change, but before the drawer updates the object!!!!!
			Debug.Log("BOOL CHANGECHECK PARAM: " + boolval);



			//Parent.Parent.scheduleRegenerate(this.Guid);

		}

		public void floatValueChangedByPropertyDrawer()
		{
			Debug.Log("FLOAT CHANGECHECK PARAM: " + val);



			//Parent.Parent.scheduleRegenerate(this.Guid);
			/*
			if (dependsOn != null)
			{
				dependsOn.FloatVal = val;
			}
			*/

			// do dependents as well? or let that ripple back down from dependsOn?
			// ...

			/*
			 * else if(dependents != null)
			{
				Debug.Log("setting dependent");
				foreach(AXParameter d in dependents)
				{
					d.val = val;
				}
			}
			*/


		}









		public void editClicked()
		{
			Debug.Log("EDIT CLICKED: " + Name);
			Parent.parameterEditClicked(this);
		}









	}




	[Serializable]
	public class SerializableParameter
	{
		public string guid;
		public string Name;

		public AXParameter.DataType Type;

		public float val;
		public int intval;
		public bool boolval;




		// JSON SERIALIZATION
		public string asJSON()
		{
			/* At some point, we could use a more generalized serializer,
			 * but it would have to recognize the NonSerialized members and not pursue 
			 * their graph links
			 * 
			 */

			StringBuilder sb = new StringBuilder();

			sb.Append("{");

			sb.Append("\"guid\":\"" + guid + "\"");
			sb.Append(", \"name\":\"" + Name + "\"");

			sb.Append(", \"data_type\":" + Type.GetHashCode());

			sb.Append(", \"val\":" + val);
			sb.Append(", \"intval\":" + intval);
			sb.Append(", \"boolval\":\"" + boolval + "\"");



			sb.Append("}");

			return sb.ToString();
		}

		public static SerializableParameter fromJSON(AX.SimpleJSON.JSONNode jn)
		{
			SerializableParameter sp = new SerializableParameter();

			sp.guid = jn["guid"];
			sp.Name = jn["name"];

			sp.Type = (AXParameter.DataType)jn["data_type"].AsInt;

			sp.val = jn["val"].AsFloat;
			sp.intval = jn["intval"].AsInt;
			sp.boolval = jn["boolval"].AsBool;

			return sp;
		}
	}


	[Serializable]
	public class AXParameterAlias
	{
		// An alternatic name for a parameter 
		public string alias;

		// the GUID of the aliased parameter
		public string Guid;

		public bool isEditing;

		// live parameter reference
		[System.NonSerialized]
		public AXParameter parameter;
	}


}


