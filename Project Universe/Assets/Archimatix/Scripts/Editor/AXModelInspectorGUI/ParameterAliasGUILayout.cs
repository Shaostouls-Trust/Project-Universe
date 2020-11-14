using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;

namespace AXEditor
{

	public class ParameterAliasGUILayout  
	{

		
		
		// return the height of this gui area
		public static void OnGUI(AXParameterAlias pa) 
		{

			AXParameter p = pa.parameter;

			//Event e = Event.current;


	//		if (Event.current.type == EventType.KeyUp)
	//		{
	//			if (p != null && ! GUI.GetNameOfFocusedControl().Contains("logicTextArea_") )
	//			{
	//				p.parametricObject.model.autobuildDelayed(1000);
	//			}
	//		}

					


			//Color oldBackgroundColor = GUI.backgroundColor;


			if (p == null || p.parametricObject == null)
				return;
			
		
			
			//Color defcolor = GUI.color;
			

			GUILayout.BeginHorizontal();

			// FOLDOUT

			GUIStyle toggleStyle = new GUIStyle(EditorStyles.foldout);
			toggleStyle.fixedWidth =50;

			pa.isEditing = EditorGUILayout.Foldout(pa.isEditing, GUIContent.none );

			//Debug.Log(pa.isEditing);
			// SLIDER AND NUMBER FIELD 

			GUIStyle textfieldStyle = new GUIStyle(GUI.skin.textField); 
			textfieldStyle.fixedWidth =150;




			GUIStyle labelWrap = new GUIStyle(GUI.skin.label); 
			labelWrap.stretchWidth = true;
			labelWrap.wordWrap = true;
			labelWrap.fontSize = 9;
			labelWrap.fixedWidth = 150;
			if (pa.isEditing)
			{
				GUILayout.BeginVertical();

				pa.alias = GUILayout.TextField(pa.alias, textfieldStyle);

				GUILayout.Label("You can edit the alias without altering the source parameter.", labelWrap);

				GUILayout.Label("Actual: ", labelWrap);
				if (GUILayout.Button(pa.parameter.parametricObject.Name + "." + pa.parameter.Name))
				{
					pa.parameter.parametricObject.model.selectPO(pa.parameter.parametricObject);
					pa.parameter.ParentNode.isOpen = true;

					float framePadding = 200;

					Rect allRect = AXUtilities.getBoundaryRectFromPOs(pa.parameter.parametricObject.model.selectedPOs);
							allRect.x 		-= framePadding;
							allRect.y 		-= framePadding;
							allRect.width 	+= framePadding*2;
							allRect.height 	+= framePadding*2;

							AXNodeGraphEditorWindow.zoomToRectIfOpen(allRect);

				}
                if (GUILayout.Button("Delete"))
                {
                    if (p != null && p.parametricObject != null && p.parametricObject.model)
                    {
                        p.parametricObject.model.removeExposedParameter(p);

                    }



                }


                    GUILayout.Space(15);
				GUILayout.EndVertical();

			}
			else
			{
					
				
				string nameString = pa.alias;
					
				switch(p.Type)
				{
				case AXParameter.DataType.Float:
					
					// FLOAT SLIDER
					if(p.PType != AXParameter.ParameterType.DerivedValue)
					{

						// VALIDATE INPUT - IF INVALID >> LOSE FOCUS
						AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Text_" + p.Name);

						EditorGUI.BeginChangeCheck ();
						GUI.SetNextControlName("FloatField_Text_" + p.Guid + "_"  + p.Name);
						p.val = EditorGUILayout.FloatField(nameString ,  p.val);
						if (EditorGUI.EndChangeCheck ()) {
							//Debug.Log(val);

							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
							p.Parent.initiateRipple_setFloatValueFromGUIChange(p.Name, p.val);
							p.parametricObject.model.isAltered(27);
							p.parametricObject.generator.adjustWorldMatrices ();
						}
					} 
					else
					{
						GUILayout.Label(p.Name );
						GUILayout.Label(""+p.val );
					}
					break;
					
				case AXParameter.DataType.Int:
				{
					// INT SLIDER
					// VALIDATE INPUT - IF INVALID >> LOSE FOCUS
					/*
					if (Event.current.type == EventType.KeyDown)
					{
							if(Event.current.keyCode != KeyCode.None && !AXEditorUtilities.isValidIntFieldKeyCode(Event.current.keyCode) && GUI.GetNameOfFocusedControl() == ("IntField_" + p.Name))
						{
							Event.current.Use();
							GUI.FocusControl("dummy_label");
						}
					}
					*/

					AXEditorUtilities.assertIntFieldKeyCodeValidity("IntField_" + p.Name);

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("IntField_" + p.Guid + "_"  + p.Name);
					p.intval = EditorGUILayout.IntField(nameString,  p.intval);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
						p.initiateRipple_setIntValueFromGUIChange(p.intval);
						p.parametricObject.model.isAltered(28);
						p.parametricObject.generator.adjustWorldMatrices ();
					}
					
					break;
				}
				case AXParameter.DataType.Bool:
				{
					EditorGUI.BeginChangeCheck ();

					GUI.SetNextControlName("BoolToggle_" + p.Guid + "_"  + p.Name);
					p.boolval = EditorGUILayout.Toggle (nameString,  p.boolval);
					if (EditorGUI.EndChangeCheck ()) {

						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);

						p.parametricObject.initiateRipple_setBoolParameterValueByName(p.Name, p.boolval);
						p.parametricObject.model.autobuild();
						p.parametricObject.generator.adjustWorldMatrices();
					}
					
					break;
				}
				case AXParameter.DataType.String:
					if (p.Parent.isEditing)
						GUILayout.Label(nameString);
					else 
						GUILayout.Label(nameString);

					break;
				case AXParameter.DataType.Option:
				{
					// OPTION POPUP
					
					string[] options = ArchimatixUtils.getMenuOptions(p.Name);

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("OptionPopup_" + p.Guid + "_"  + p.Name);	
					p.intval = EditorGUILayout.Popup(
						nameString,
						p.intval, 
						options);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
						p.parametricObject.model.autobuild();

						if (p.PType == AXParameter.ParameterType.PositionControl)
							p.parametricObject.generator.adjustWorldMatrices();

					}
					
					break;
				}
				case AXParameter.DataType.CustomOption:
					{
						// OPTION POPUP
						
						string[] options = p.optionLabels.ToArray();

		

						EditorGUI.BeginChangeCheck ();
						GUI.SetNextControlName("CustomOptionPopup_" + p.Guid + "_"  + p.Name);	
						p.intval = EditorGUILayout.Popup(
							nameString,
							p.intval, 
							options);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);

							if (p.PType == AXParameter.ParameterType.PositionControl)
								p.parametricObject.generator.adjustWorldMatrices();

							p.parametricObject.model.autobuild();
						}
						
						break;
					}

					case AXParameter.DataType.Spline:
					{
						GUILayout.Label(pa.alias );
						break;
					}


					
				}// END switch (Type)
				
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

				

			
			

			//GUI.backgroundColor = oldBackgroundColor;
			
			
			
			
			//GUI.color = defcolor;


		}

	}
}
