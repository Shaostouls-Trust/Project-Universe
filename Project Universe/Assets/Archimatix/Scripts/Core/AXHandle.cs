using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


using AX.SimpleJSON;




using AXGeometry;
using Axis = AXGeometry.Axis;


namespace AX
{

	[System.Serializable]
	public class AXHandle
	{

		/* 
		 * A Handle is used to generate a unity Handle in scene view
		 * and control the AXHandle's ParametricObject parent.
		 * This class has a position for the handle. The coordinates of the position are math expressions 
		 * that are interpreted from the parent PO's parameters
		 * It should also define inverse math expressions to interpret what the 
		 * new postion of the handle in the scene.
		 * 
		 * Thue, the handle description in logic would always have 7 args
		 * handleType pos_x pos_y pos_z inv_x inv_y inv_z
		 * 
		 */

		[SerializeField]
		private string m_guid = System.Guid.NewGuid().ToString();
		public string Guid
		{
			get
			{
				if (string.IsNullOrEmpty(m_guid))
					m_guid = System.Guid.NewGuid().ToString();
				return m_guid;
			}
			set { m_guid = value; }
		}



		[System.NonSerialized]
		public AXParametricObject parametricObject;



		public string handleType;

		public Axis axis = Axis.Y;


		public AXHandleRuntimeAlias runtimeHandle;
		public AXRuntimeHandleBehavior runtimeHandleBehavior;

		public enum HandleType { Point, Angle, Circle, Position };

		[SerializeField]
		private HandleType m_type;
		public HandleType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}

		public string Name = "Handle";

		public bool exposeForRuntime;

		// EXPRESSIONS <string>
		// These are strings because each can be a mathematical expression
		public Color color = Color.red;

		public string cen_x = "0";
		public string cen_y = "0";
		public string cen_z = "0";

		public string pos_x = "0";
		public string pos_y = "0";
		public string pos_z = "0";

		public Vector3 positionCache = new Vector3(-999999, -999999, -999999);

		public string radius = "1";
		public float radiusCache = -999999;

		public string tangent = "16";
		public float tangentCache = -999999;

		public string angle = "0";
		public float angleCache = -999999;

		public string len;
		public float lenCache = -999999;

		// VALUES
		float X = 0;
		float Y = 0;
		float Z = 0;

		[NonSerialized]
		public bool positionHasBeenReset = false;


		// EXPRESSIONS PROCESSING
		public List<string> expressions;

		public string[] param;
		public string[] descrip;


		public bool isEditing = false;



		public AXHandle()
		{
			Name = "Handle";

		}

		public AXHandle(AXParametricObject po)
		{
			parametricObject = po;

		}
		public AXHandle(AXParametricObject po, string[] args)
		{

			if (args.Length > 4)
			{

				parametricObject = po;

				//AXParameter p;

				int argc = 1;

				handleType = args[argc++];

				pos_x = args[argc++];
				pos_y = args[argc++];
				pos_z = args[argc++];

				// split remaining args by = sign
				// into params and descripts "param[i]=descrip[i]"
				// this is cludgy - trying to not do this parsing on every handle frame redraw

				if (handleType == "angle")
				{
					angle = args[argc++];
					len = args[argc++];
				}


				param = new string[args.Length - argc];
				descrip = new string[args.Length - argc];

				int index = 0;
				while (argc < args.Length)
				{

					int char_index = args[argc].IndexOf('=');
					if (char_index > 0)
					{
						param[index] = args[argc].Substring(0, char_index);
						descrip[index] = args[argc].Substring(char_index + 1);
					}
					argc++;
					index++;
				}

			}

		}

		public float getRadius(bool assert = false)
		{
			if (assert || radiusCache < -999998)
			{
				try
				{
					radiusCache = (float)parametricObject.parseMath_ValueOnly(radius);
				}
				catch (Exception e)
				{
					if (parametricObject != null)
						parametricObject.codeWarning = "3. Handle error: Please check syntax of: \"" + radius + "\" " + e;
				}
			}

			return radiusCache;

		}

		public float getTangent(bool assert = false)
		{
			if (assert || tangentCache < -999998)
			{
				//Debug.Log(tangentCache);
				try
				{
					tangentCache = (float)parametricObject.parseMath_ValueOnly(tangent);
				}
				catch (Exception e)
				{
					if (parametricObject != null)
						parametricObject.codeWarning = "3. Handle error: Please check syntax of: \"" + radius + "\" " + e;
				}
			}
			return radiusCache;

		}

		public Vector3 getPointPosition()
		{
			//Debug.Log("getPointPosition");
			//if (! )
			//{	
			//	calculatePosition();
			//	 = true;
			//}

			//			Debug.Log(pos_x);
			//
			//			Debug.Log(pos_y);
			//
			//			Debug.Log(pos_z);
			//
			//			Debug.Log("="+ parametricObject);


			//return new Vector3(X, Y, Z);

			if (positionCache.x < -999998)
				calculatePosition();

			return positionCache;
		}


		/// <summary>
		/// Calculates the position.
		/// 
		/// This should be called whenever a value is changed
		/// </summary>
		/// <returns>The position.</returns>
		public Vector3 calculatePosition()
		{
			//Debug.Log("calculatePosition");
			if (pos_x == "0")
				X = 0;
			else
			{
				try {
					X = (float)parametricObject.parseMath_ValueOnly(pos_x);
				} catch (Exception e) {
					if (parametricObject != null)
						parametricObject.codeWarning = "3. Handle error: Please check syntax of: \"" + pos_x + "\" " + e;
				}
			}

			if (pos_y == "0")
				Y = 0;
			else
			{
				try
				{
					//Debug.Log(han.pos_y);
					Y = (float)parametricObject.parseMath_ValueOnly(pos_y);
				}
				catch (Exception e)
				{
					if (parametricObject != null)
						parametricObject.codeWarning = "4. Handle error: Please check syntax of: \"" + pos_y + "\" " + e;
				}
			}

			if (pos_z == "0")
				Z = 0;
			else
			{
				try
				{
					Z = (float)parametricObject.parseMath_ValueOnly(pos_z);
				}
				catch (Exception e)
				{
					if (parametricObject != null)
						parametricObject.codeWarning = "5. Handle error: Please check syntax of: \"" + pos_z + "\" " + e.Message;
				}
			}

			positionCache = new Vector3(X, Y, Z);

			if (Type == HandleType.Circle)
				calculateRadiusAndTangent();

			return positionCache;
		}
		public void calculateRadiusAndTangent()
		{
			getRadius(true);
			getTangent(true);
		}



		public void processExpressionsAfterHandleChange()
		{
			//Debug.Log("processExpressionsAfterHandleChange");
			// ASSUME THAT han_x, han_y, or han_z have just been reset in the vars.
			// Ripple to set any parameters based on these new handle values.
			for (int i = 0; i < expressions.Count; i++)
			{
				if (expressions[i] == "")
					continue;

				string expression = Regex.Replace(expressions[i], @"\s+", "");

				string paramName = expression.Substring(0, expression.IndexOf("="));
				string definition = expression.Substring(expression.IndexOf("=") + 1);
				//Debug.Log (param + " --- " + definition);

				try {
					if (parametricObject.getParameter(paramName).Type == AXParameter.DataType.Int)
						parametricObject.initiateRipple_setIntValueFromGUIChange(paramName, Mathf.RoundToInt((float)parametricObject.parseMath_ValueOnly(definition)));

					else
						parametricObject.initiateRipple_setFloatValueFromGUIChange(paramName, (float)parametricObject.parseMath_ValueOnly(definition));

				} catch (System.Exception e) {
					parametricObject.codeWarning = "10. Handle error: Please check syntax of: \"" + definition + "\" " + e.Message;
				}
			}

			// Now that the parmaseters are all updated, reset the 
			// X, Y, and Z values...

			calculatePosition();
		}



		public AXHandle clone()
		{
			AXHandle h = new AXHandle();

			h.Name = Name;
			h.Type = Type;

			h.pos_x = pos_x;
			h.pos_y = pos_y;
			h.pos_z = pos_z;

			h.radius = radius;
			h.tangent = tangent;

			h.angle = angle;
			h.len = len;

			h.expressions = new List<string>(expressions);

			return h;
		}

		public string asJSON()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{");

			sb.Append("  \"name\":\"" + Name + "\"");
			sb.Append(", \"type\":\"" + Type.GetHashCode() + "\"");

			sb.Append(", \"pos_x\":\"" + pos_x + "\"");
			sb.Append(", \"pos_y\":\"" + pos_y + "\"");
			sb.Append(", \"pos_z\":\"" + pos_z + "\"");
			sb.Append(", \"radius\":\"" + radius + "\"");
			sb.Append(", \"tangent\":\"" + tangent + "\"");

			sb.Append(", \"angle\":\"" + angle + "\"");
			sb.Append(", \"len\":\"" + len + "\"");

			if (expressions != null && expressions.Count > 0)
				sb.Append(", \"expressions\":" + AXJson.StringListToJSON(expressions));

			sb.Append("}");

			return sb.ToString();
		}

		public static AXHandle fromJSON(AX.SimpleJSON.JSONNode jn)
		{
			AXHandle h = new AXHandle();

			h.Name = jn["name"].Value;

			h.Type = (AXHandle.HandleType)jn["type"].AsInt;

			h.pos_x = jn["pos_x"].Value;
			h.pos_y = jn["pos_y"].Value;
			h.pos_z = jn["pos_z"].Value;
			h.radius = jn["radius"].Value;
			h.tangent = jn["tangent"].Value;

			h.angle = jn["angle"].Value;
			h.len = jn["len"].Value;

			if (jn["expressions"] != null)
				h.expressions = AXJson.StringListFromJSON(jn["expressions"]);

			return h;
		}


		/*
			public int OnGUI(SerializedProperty hProperty, Rect pRect, AXModelEditorWindow editor)
			{
				int cur_y = (int) pRect.y;
				int lineHgt = 16;
				int margin = 24;
				int wid = (int) pRect.width-margin;


				int gap = 5;

				int indent = (int)pRect.x+16;
				int indent2 = indent+12;
				int indentWid = wid-indent;

				if (Name == null || Name == "")
					Name = "Handle";



				if ( isEditing)
				{
					Name = EditorGUI.TextField(new Rect(pRect.x+4, cur_y, pRect.width-3*lineHgt-14, lineHgt), Name);

					if(GUI.Button(new Rect(pRect.width-3*lineHgt, pRect.y, 3*lineHgt, lineHgt), "Done"))
					{
						if (Parent != null)
							Parent.stopEditingAllHandles();

						isEditing = false;
					}

					cur_y += lineHgt;


					//if (expressions != null)
					//GUI.Box (new Rect(pRect.x, cur_y, wid, lineHgt*(8+expressions.Count)), GUIContent.none);

					// HANDLE_TYPE
					EditorGUI.PropertyField( new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), hProperty.FindPropertyRelative("m_type"), GUIContent.none);
					cur_y += lineHgt+gap;

					GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
					labelstyle.alignment = TextAnchor.MiddleLeft;






					// POSITION HEADER -------
					GUI.Label(new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), new GUIContent("Position", "Use a number, parameter name, or a mathematical expressions to define the handle position"));
					cur_y += lineHgt;

					// X_POS
					GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "X");
					pos_x = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), pos_x);
					cur_y += lineHgt;

					// Y_POS
					GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "Y");
					pos_y = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), pos_y);
					cur_y += lineHgt;

					// Z_POS
					GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "Z");
					pos_z = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), pos_z);
					cur_y += lineHgt+gap;




					if (Type == HandleType.Circle)
					{



						// RADIUS
						GUI.Label(new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), new GUIContent("Radius", "Use a number, parameter name, or a mathematical expressions to define the handle radius"));
						cur_y += lineHgt;

						GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "R");
						radius = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), radius);
						cur_y += lineHgt+gap;


						// TANGENT
						GUI.Label(new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), new GUIContent("Tangent", "Use a number, parameter name, or a mathematical expressions to define the handle tangent"));
						cur_y += lineHgt;

						GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "T");
						tangent = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), tangent);
						cur_y += lineHgt+gap;

					}


					// EXPRESSIONS HEADER -------
					GUI.Label(new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), new GUIContent("Expressions", "These mathematical expressions interpret the handle's values."));
					cur_y += lineHgt;

					if (expressions == null)
						expressions = new List<string>();

					if(expressions.Count == 0)
						expressions.Add ("");


					for(int i=0; i<expressions.Count; i++)
					{
						expressions[i] = EditorGUI.TextField(new Rect(indent, cur_y, indentWid,16), expressions[i]);
						cur_y += lineHgt;
					}
					if(GUI.Button(new Rect(indent, cur_y, lineHgt*1.25f, lineHgt), new GUIContent("+", "Add an expression to this Handle")))
						expressions.Add ("");

					cur_y += lineHgt;

				} 
				else
				{
					if(GUI.Button(new Rect(pRect.x, pRect.y, 80, 16), new GUIContent(Name,"Click to edit this handle.")))
					{
						isEditing = true;
					}
					if(GUI.Button(new Rect(wid+6, pRect.y, lineHgt, lineHgt), "-"))
					{
						Debug.Log ("REMOVE HANDLE...");
						if (Parent != null)
						{
							Parent.removeHandleFromReplicantsControlsWithName(Name);
							Parent.handles.Remove(this);
						}

					}

				}

				cur_y += lineHgt;



				return cur_y-(int)pRect.y;
			}
		*/
	}





	// Serves and a alias to an AXHandle.
	[Serializable]
	public class AXHandleRuntimeAlias
	{
		public string alias;

		public bool isEditing;



		// the GUID of the aliased AXHandle
		public string handleGuid;


		// live AXHandle reference
		[System.NonSerialized]
		public AXHandle handle;




	}
}
