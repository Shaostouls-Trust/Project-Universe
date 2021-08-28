using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using AX;

using AXGeometry;
using Axis = AXGeometry.Axis;

namespace AXEditor
{

	public class AXHandleGUI
	{



		public static int OnGUI(AXHandle han, Rect pRect, AXNodeGraphEditorWindow editor)
		{
			int cur_y = (int)pRect.y;
			int lineHgt = 16;
			int margin = 24;
			int wid = (int)pRect.width - margin;


			int gap = 5;

			int indent = (int)pRect.x + 16;
			int indent2 = indent + 12;
			int indentWid = wid - indent;

			if (han.Name == null || han.Name == "")
				han.Name = "Handle";



			if (han.isEditing)
			{



				if (han.expressions == null)
					han.expressions = new List<string>();
				GUI.Box(new Rect(pRect.x, cur_y - 4, (pRect.width - 20), ((12 + han.expressions.Count) * lineHgt)), "");

				// TITLE
				GUI.SetNextControlName("HanTitle_Text_" + han.parametricObject.Guid + "_" + han.param);
				han.Name = GUI.TextField(new Rect(pRect.x + 4, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt), han.Name);



				cur_y += lineHgt;

				han.color = EditorGUI.ColorField(new Rect(pRect.x + 4, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt),
						  "Color:",
						  han.color);

				cur_y += lineHgt;




				//if (expressions != null)
				//GUI.Box (new Rect(pRect.x, cur_y, wid, lineHgt*(8+expressions.Count)), GUIContent.none);

				// HANDLE_TYPE
				//EditorGUI.PropertyField( new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), hProperty.FindPropertyRelative("m_type"), GUIContent.none);

				AXModel model = han.parametricObject.model;

				//string[] options = Archimatix.getMenuOptions(p.Name);


				cur_y += gap * 2;



				// HANDLE TYPE
				string[] options = System.Enum.GetNames(typeof(AXHandle.HandleType));

				EditorGUIUtility.labelWidth = wid - 50;
				EditorGUI.BeginChangeCheck();
				han.Type = (AXHandle.HandleType)EditorGUI.Popup(
				new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt),
				"",
				(int)han.Type,
				options);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(model, "value change for handle type");
					model.autobuild();
				}


				cur_y += lineHgt + gap;

				GUIStyle labelstyle = GUI.skin.GetStyle("Label");
				labelstyle.alignment = TextAnchor.MiddleLeft;





				// 	RUNTIME HANDLE: expose in Model interface ------------

				Rect cntlRect = new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt);
				EditorGUI.BeginChangeCheck();
				han.exposeForRuntime = EditorGUI.Toggle(cntlRect, "Runtime", han.exposeForRuntime);
				if (EditorGUI.EndChangeCheck())
				{

					Undo.RegisterCompleteObjectUndo(model, "Runtime Handle");

					if (han.exposeForRuntime)
					{
						//Debug.Log(ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob);


						ArchimatixEngine.assertRuntimeHandlePrefab_PlanarKnob();

						//AXHandleRuntimeAlias rtHandleAlias =  model.addHandleRuntimeAlias(han);

						if (ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob != null)
						{
							// CREATE A GAME_OBJECT

							GameObject tmp = GameObject.Instantiate(ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob);
							//GameObject tmp = GameObject.Instantiate(model.RuntimeHandlePrefab_PlanarKnob);
							tmp.name = han.Name;
							//Debug.Log("han.Name="+han.Name);
							if (tmp != null)
							{
								tmp.transform.parent = model.runtimeHandlesGameObjects.transform;

								AXRuntimeHandleBehavior rtHandleBehavior = tmp.GetComponent<AXRuntimeHandleBehavior>();

								rtHandleBehavior.handleGuid = han.Guid;
								rtHandleBehavior.handle = han;
							}
						}

					}
					else
					{
						model.removeHandleRuntimeAlias(han);

						AXRuntimeHandleBehavior[] rhbs = model.runtimeHandlesGameObjects.GetComponentsInChildren<AXRuntimeHandleBehavior>();
						for (int j = 0; j < rhbs.Length; j++)
						{
							if (rhbs[j] != null && rhbs[j].gameObject != null && rhbs[j].handleGuid == han.Guid)
							{
								// This GameObject and all its children...

								if (Application.isPlaying)
									GameObject.Destroy(rhbs[j].gameObject);
								else
									GameObject.DestroyImmediate(rhbs[j].gameObject);

								break;
							}
						}
						Resources.UnloadUnusedAssets();
					}
				}


				cur_y += lineHgt + gap;


				int axisInt = (int)han.axis;

				if (GUI.Button(new Rect(indent, cur_y, 50, lineHgt), "" + han.axis))
				{
					int next = (axisInt == 6) ? 0 : axisInt + 1;

					han.axis = (Axis)next;


				}
				cur_y += lineHgt + gap;




				// POSITION HEADER -------
				GUI.Label(new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt), new GUIContent("Position", "Use a number, parameter name, or a mathematical expressions to define the handle position"));
				cur_y += lineHgt;

				// X_POS
				GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "X");
				GUI.SetNextControlName("x_Text_" + han.parametricObject.Guid + "_" + han.param);
				han.pos_x = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), han.pos_x);
				cur_y += lineHgt;

				// Y_POS
				GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "Y");
				GUI.SetNextControlName("y_Text_" + han.parametricObject.Guid + "_" + han.param);
				han.pos_y = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), han.pos_y);
				cur_y += lineHgt;

				// Z_POS
				GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "Z");
				GUI.SetNextControlName("z_Text_" + han.parametricObject.Guid + "_" + han.param);
				han.pos_z = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), han.pos_z);
				cur_y += lineHgt + gap;




				if (han.Type == AXHandle.HandleType.Circle)
				{



					// RADIUS
					GUI.Label(new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt), new GUIContent("Radius", "Use a number, parameter name, or a mathematical expressions to define the handle radius"));
					cur_y += lineHgt;

					GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "R");
					han.radius = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), han.radius);
					cur_y += lineHgt + gap;



					// TANGENT
					Color prevColor = Handles.color;
					Handles.color = Color.cyan;

					GUI.Label(new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt), new GUIContent("Tangent", "Use a number, parameter name, or a mathematical expressions to define the handle tangent"));
					cur_y += lineHgt;

					GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "T");
					han.tangent = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), han.tangent);
					cur_y += lineHgt + gap;

					/*
						// OFFSET ANGLE
						GUI.Label(new Rect(indent, cur_y, pRect.width-3*lineHgt-14, lineHgt), new GUIContent("Angle", "Use a number, parameter name, or a mathematical expressions to define the handle angle"));
						cur_y += lineHgt;

						GUI.Label(new Rect(indent, cur_y, 12, lineHgt), "Z");
						angle = EditorGUI.TextField(new Rect(indent2, cur_y, indentWid, lineHgt), angle);
						cur_y += lineHgt+gap;
						*/

					Handles.color = prevColor;
				}


				// EXPRESSIONS HEADER -------
				GUI.Label(new Rect(indent, cur_y, pRect.width - 3 * lineHgt - 14, lineHgt), new GUIContent("Expressions", "These mathematical expressions interpret the handle's values."));
				cur_y += lineHgt;

				if (han.expressions == null)
					han.expressions = new List<string>();

				if (han.expressions.Count == 0)
					han.expressions.Add("");


				for (int i = 0; i < han.expressions.Count; i++)
				{
					GUI.SetNextControlName("HandleExp_Text_" + han.parametricObject.Guid + "_" + han.param);

					han.expressions[i] = EditorGUI.TextField(new Rect(indent, cur_y, indentWid, 16), han.expressions[i]);

					if (GUI.Button(new Rect(pRect.width - lineHgt * 1.5f, cur_y - 1, lineHgt * 1.25f, lineHgt), "-"))
					{
						han.expressions.Remove(han.expressions[i]);
						//p.expressions.RemoveAt(i);
					}


					cur_y += lineHgt;
				}
				if (GUI.Button(new Rect(pRect.width - lineHgt * 1.5f, cur_y, lineHgt * 1.25f, lineHgt), new GUIContent("+", "Add an expression to this Handle")))
					han.expressions.Add("");

				cur_y += gap * 5;
				if (GUI.Button(new Rect(pRect.width - 3 * lineHgt - 5, cur_y, 3 * lineHgt, lineHgt), "Save"))
				{
					if (han.parametricObject != null)
						han.parametricObject.stopEditingAllHandles();

					han.calculatePosition();

					han.isEditing = false;
				}
				cur_y += lineHgt;


			}
			else
			{
				if (GUI.Button(new Rect(pRect.x, pRect.y, wid - 16, 16), new GUIContent(han.Name, "Click to edit this handle.")))
				{
					han.parametricObject.stopEditingAllHandlesExcept(han);
					han.isEditing = true;
				}

			}
			if (GUI.Button(new Rect(wid + 6, pRect.y, lineHgt, lineHgt), "-"))
			{
				//Debug.Log ("REMOVE HANDLE...");
				if (han.parametricObject != null)
				{
					han.parametricObject.removeHandleFromReplicantsControlsWithName(han.Name);
					han.parametricObject.handles.Remove(han);
				}

			}



			cur_y += lineHgt;


			return cur_y - (int)pRect.y;
		}




	}


} //namespace
