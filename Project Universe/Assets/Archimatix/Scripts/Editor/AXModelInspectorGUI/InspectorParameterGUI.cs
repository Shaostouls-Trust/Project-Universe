using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;

namespace AXEditor
{

	public class InspectorParameterGUI
	{

		
		
		public static void OnGUI (List<AXNode>  parameters)
		{



			for (int i = 0; i < parameters.Count; i++) {

				AXParameter p = parameters [i] as AXParameter;

				EditorGUIUtility.labelWidth = 150;


				AXModel model = p.parametricObject.model;


				// PARAMETERS
				switch (p.Type) {



				// ANIMATION_CURVE

				case AXParameter.DataType.AnimationCurve:

					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.CurveField (p.animationCurve);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, "ModifierCurve");
						model.isAltered (28);
					}

					break;

				case AXParameter.DataType.Color:

					EditorGUI.BeginChangeCheck ();
					p.colorVal = EditorGUILayout.ColorField(p.colorVal);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, "Color");
						model.isAltered (28);
					}
					break;

				// FLOAT
				case AXParameter.DataType.Float:

					AXEditorUtilities.assertFloatFieldKeyCodeValidity ("FloatField_" + p.Name);

//						GUI.color = Color.white;
//						GUI.contentColor = Color.white;
//

					if (p.isOpen)
						EditorGUILayout.BeginHorizontal ("Box");
					else
						EditorGUILayout.BeginHorizontal ();

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName ("FloatFieldInsp_" + p.Name);
					p.FloatVal = EditorGUILayout.FloatField (p.Name, p.FloatVal);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, "value change for " + p.Name);
						p.parametricObject.initiateRipple_setFloatValueFromGUIChange (p.Name, p.FloatVal);
						model.isAltered (27);
						p.parametricObject.generator.adjustWorldMatrices ();
						ArchimatixEngine.scheduleBuild();
					}

					p.isOpen = EditorGUILayout.Foldout (p.isOpen, GUIContent.none);

					EditorGUILayout.EndHorizontal ();





					if (p.isOpen) {
						EditorGUI.indentLevel++;

						InspectorParameterEditGUI.OnGUI(p);

						EditorGUI.indentLevel--;
					}





					break;

				// INT
				case AXParameter.DataType.Int:

						//GUI.backgroundColor = new Color(.6f,.6f,.9f,.1f) ;
//					if (p.isOpen)
//						EditorGUILayout.BeginHorizontal ("Box");
//					else

					EditorGUILayout.BeginHorizontal ();

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName ("IntFieldFieldInsp_" + p.Name);
					p.IntVal = EditorGUILayout.IntField (p.Name, p.IntVal);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, "value change for " + p.Name);
						p.parametricObject.initiateRipple_setIntValueFromGUIChange (p.Name, p.IntVal);
						model.isAltered (27);
						p.parametricObject.generator.adjustWorldMatrices();
						ArchimatixEngine.scheduleBuild();
					}

					p.isOpen = EditorGUILayout.Foldout (p.isOpen, GUIContent.none);

					EditorGUILayout.EndHorizontal ();


					if (p.isOpen) {
						EditorGUI.indentLevel++;

						InspectorParameterEditGUI.OnGUI(p);

						EditorGUI.indentLevel--;
					}




						
					break;

				// BOOL
				case AXParameter.DataType.Bool:
						//EditorGUIUtility.currentViewWidth-16;

					GUILayout.BeginHorizontal ();

						//EditorGUIUtility.labelWidth = 150;
					EditorGUI.BeginChangeCheck ();
					p.boolval = EditorGUILayout.Toggle (p.Name, p.boolval);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, " value change for " + p.Name);
						p.parametricObject.initiateRipple_setBoolParameterValueByName (p.Name, p.boolval);
						//p.parametricObject.model.autobuild();
						p.parametricObject.model.isAltered (27);
						//p.parametricObject.generator.adjustWorldMatrices();
						ArchimatixEngine.scheduleBuild();
					}

					GUILayout.FlexibleSpace ();


					p.isOpen = EditorGUILayout.Foldout (p.isOpen, GUIContent.none);




						// Expose
					EditorGUI.BeginChangeCheck ();
					p.exposeAsInterface = EditorGUILayout.Toggle (p.exposeAsInterface, GUILayout.MaxWidth (20));
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Expose Parameter");

						if (p.exposeAsInterface)
							p.parametricObject.model.addExposedParameter (p);
						else
							p.parametricObject.model.removeExposedParameter (p); 
					}


					GUILayout.EndHorizontal ();
					break; 

					
				case AXParameter.DataType.CustomOption:
					{
						// OPTION POPUP
						
						string[] options = p.optionLabels.ToArray ();

						EditorGUI.BeginChangeCheck ();
						GUI.SetNextControlName ("CustomOptionPopup_" + p.Guid + "_" + p.Name);	
						p.intval = EditorGUILayout.Popup (
							p.Name,
							p.intval, 
							options);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
							p.parametricObject.model.autobuild ();

							if (p.PType == AXParameter.ParameterType.PositionControl)
								p.parametricObject.generator.adjustWorldMatrices ();
							ArchimatixEngine.scheduleBuild();
						}
						
						break;
					}
				}

				//if (p.PType != AXParameter.ParameterType.None && p.PType != AXParameter.ParameterType.GeometryControl)
				//	continue;
								

			}
		}
	}
}
