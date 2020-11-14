using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;

namespace AXEditor
{

	public class InspectorParameterEditGUI {



		public static void OnGUI (AXParameter p)
		{


						EditorGUILayout.BeginVertical ("Box");

						// NAME
						p.Name = EditorGUILayout.TextField ("Name: ", p.Name);



						// EXPOSE AS RUNTIME
						EditorGUI.BeginChangeCheck ();
						p.exposeAsInterface = EditorGUILayout.Toggle ("Runtime", p.exposeAsInterface);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Expose Runtime Parameter");

							if (p.exposeAsInterface)
								p.parametricObject.model.addExposedParameter(p);
							else
								p.parametricObject.model.removeExposedParameter(p); 
						}


						// MIN/MAX
						p.min = EditorGUILayout.FloatField ("Min: ", p.min);
						p.max = EditorGUILayout.FloatField ("Max: ", p.max);

						// EXPRESSIONS

						EditorGUILayout.BeginHorizontal ();
							GUILayout.Space(45);
							GUILayout.Label ("Expressions");
						EditorGUILayout.EndHorizontal ();

						if (p.expressions == null)
							p.expressions = new List<string> ();
							
						if (p.expressions.Count == 0) {		
							p.expressions.Add ("");
						}



						for (int j = 0; j < p.expressions.Count; j++) {
							EditorGUILayout.BeginHorizontal ();

							GUI.SetNextControlName ("ParameterExpressionInsp_" + j + "_Text_" + p.Guid + "_" + p.Name);
							p.expressions [0] = EditorGUILayout.DelayedTextField (p.expressions [0]);


							if (GUILayout.Button ("-"))
								p.expressions.RemoveAt (j);
								
							EditorGUILayout.EndHorizontal ();

						}
						EditorGUILayout.BeginHorizontal ();
							GUILayout.Space(45);
							if(GUILayout.Button(new GUIContent("+", "Add an expression to this Parameter"),GUILayout.MaxWidth(20)))
							{
								p.expressions.Add ("");
							}
						EditorGUILayout.EndHorizontal ();


						EditorGUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Close")) {
							p.isOpen = false;
						}
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.EndVertical ();
		}


	}
}
