using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;
using AXEditor;

using Axis = AXGeometry.Axis;

public class ParameterGUI  {

	
	
	// return the height of this gui area
	public static int OnGUI(Rect pRect, AXNodeGraphEditorWindow editor, AXParameter p) 
	{
        //if (p.Name == "BodyChannel1" || p.Name == "BodyChannel")
        //{
        //    Debug.Log(p.Name + ": " + p.Type + " :: " + p.PType);
        //}

        if (p == null)
            return 0;

        Event e = Event.current;


		if (Event.current.type == EventType.KeyUp)
		{

			if (p != null && ! GUI.GetNameOfFocusedControl().Contains("logicTextArea_") )
			{
				p.parametricObject.model.autobuildDelayed(1000);
			}
		}
		int hgt = (int) pRect.height;
		
		float foldoutWidth = 20;
		float boxWidth = pRect.width - foldoutWidth - ArchimatixUtils.indent;
		
		float cur_x = ArchimatixUtils.cur_x; 
		float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
		
		
		int cur_y = (int)pRect.y;



		Color inactiveColor = new Color(.7f,.7f,.7f);

		Color oldBackgroundColor = GUI.backgroundColor;
		Color dataColor =  editor.getDataColor(p.Type);

		// PERSONAL
		if (! EditorGUIUtility.isProSkin)
			dataColor = new Color(dataColor.r, dataColor.b, dataColor.g, .3f);
		
		GUI.color = dataColor;
		Rect boxRect = new Rect(cur_x + ArchimatixUtils.indent, cur_y, box_w, ArchimatixUtils.lineHgt);
		GUI.Box(boxRect, " ");
		GUI.Box(boxRect, " ");
		GUI.color = Color.white;
		
		int margin = 24;
		float x0 = pRect.x + 14;
		float xTF = pRect.x + 12;
		float wid = pRect.width-margin;

		
		int indent = (int) x0 + 2;
		int lineHgt = 16;
		int gap = 2;
		
		if (p.isEditing)
		{
			hgt *= 5;
			hgt += 8;
			GUI.Box (new Rect(pRect.x-6, pRect.y-3, boxWidth, lineHgt*(8+p.expressions.Count)), GUIContent.none);
			
		}
		
		
		if (p.Parent == null)
			return 0;
		
		if (p.Parent != null && p.to_delete == true) {
			p.Parent.removeParameter(p);
			return 0;
		}
		
		Color defcolor = GUI.color;
		
		
		
		
		
		// input/ouput sockets
		
		GUI.backgroundColor = dataColor;
		
		
		
		// INPUT SOCKET
		
		string buttonLabel = null;
		Rect buttonRect;
		
		if (p.isEditing || p.hasInputSocket) { 
			
			if (p.isEditing &&  ! p.hasInputSocket)
				GUI.color = new Color(defcolor.r, defcolor.g, defcolor.b, .3f);
			
			
			buttonLabel 	= (editor.InputParameterBeingDragged == p) ? "-" : "";
			buttonRect 		= new Rect(-3, pRect.y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize);
			
			// button color
			if (editor.OutputParameterBeingDragged != null)
			{

               
				if (editor.OutputParameterBeingDragged.Type != p.Type) 
					GUI.backgroundColor = inactiveColor;
				else if (buttonRect.Contains (Event.current.mousePosition)) 
					GUI.backgroundColor = Color.white;
			} 
			else if (editor.InputParameterBeingDragged != null)
			{
				if (editor.InputParameterBeingDragged == p)
					GUI.backgroundColor = Color.white;
				else
					GUI.backgroundColor = inactiveColor;
			}
			
			// Input Button

			if (editor.OutputParameterBeingDragged != null && (editor.OutputParameterBeingDragged.parametricObject == p.parametricObject || editor.OutputParameterBeingDragged.Type != p.Type))
				GUI.enabled = false;

			if (GUI.Button (buttonRect, buttonLabel))
			{
				if (p.isEditing)
					p.hasOutputSocket = (! p.hasOutputSocket);
				else 
				{

					if (Event.current.command)
						Debug.Log ("CONTEXT");
					else if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type) 
						editor.OutputParameterBeingDragged = null; 
					else
					{
                       

                        editor.inputSocketClicked(p);
					}
				}		
			}
			GUI.enabled = true;
		}
		GUI.backgroundColor = editor.getDataColor(p.Type);
		GUI.color = defcolor;
		
		// INPUT SOCKET
		
		
		
		
		
		// OUTPUT SOCKET
		
		if (p.isEditing || p.hasOutputSocket) { 
			
			
			if (p.isEditing &&  ! p.hasOutputSocket)
				GUI.color = new Color(defcolor.r, defcolor.g, defcolor.b, .3f);
			
			buttonLabel = (editor.OutputParameterBeingDragged == p) ? "-" : "";
			buttonRect = new Rect(p.Parent.rect.width-pRect.height+3, pRect.y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize);
			
			// button color
			if (editor.InputParameterBeingDragged != null)
			{
				if (editor.InputParameterBeingDragged.Type != p.Type) 
					GUI.backgroundColor = inactiveColor;
				else if (buttonRect.Contains (Event.current.mousePosition)) 
					GUI.backgroundColor = Color.white;
			} 
			else if (editor.OutputParameterBeingDragged != null)
			{
				if (editor.OutputParameterBeingDragged == p)
					GUI.backgroundColor = Color.white;
				else
					GUI.backgroundColor = inactiveColor;
			}
			
			// Output Button
			if (editor.InputParameterBeingDragged != null && (editor.InputParameterBeingDragged.parametricObject == p.parametricObject || editor.InputParameterBeingDragged.Type != p.Type))
				GUI.enabled = false;

			if (GUI.Button (buttonRect, buttonLabel))
			{
				
				
				if (p.isEditing)
					p.hasOutputSocket = (! p.hasOutputSocket);
				else 
				{
					if (Event.current.control)
					{
						
						// DISPLAY CONTEXT MENU FOR THINGS YOU CAN USE THIS OUTPUT FOR
						Debug.Log ("context");
						//model.selectPO(parametricObject);
						
						MeshOuputMenu.contextMenu(p, e.mousePosition); 
						
						
						e.Use();
					}					
					else if (editor.InputParameterBeingDragged != null && editor.InputParameterBeingDragged.Type != p.Type) 
						editor.InputParameterBeingDragged = null;
					else
						editor.outputSocketClicked(p);
				}
			}
			GUI.enabled = true;
			
		}
		
		GUI.color = defcolor;
		
		// OUTPUT SOCKET
		
		
		
		
		
		

		
		// SLIDER AND NUMBER FIELD 
		

		Rect nameLabelRect = new Rect(x0, pRect.y, wid-30, 16);
		Rect cntlRect = new Rect(x0, pRect.y, wid, 16);
		
		//Rect boxRect = new Rect(xb, pRect.y+vMargin, boxWidth, pRect.height-vMargin*2);
		
		
		/*
		if (hasInputSocket && ! hasOutputSocket)
			boxRect = new Rect(xb, pRect.y+vMargin, inputwid, pRect.height-vMargin*2); 
		else if(! hasInputSocket && hasOutputSocket)
			boxRect = new Rect(xb+x_output, pRect.y+vMargin, inputwid+4, pRect.height-vMargin*2);
		*/
		
		Rect textFieldRect = new Rect(xTF+3, pRect.y, wid-60, pRect.height+2);
		
		
		
		// -- DON'T USE PROPERTY DRAWER TO HANDLE UNDO WITH SLIDER
		// we need o get the updated value atomically, which the propertydrawer does not seem to do (slower update)
		 
		
		
		// BINDING HIGHLIGHT BACKGROUND BOX
		//Handles.yAxisColor;


		//GUI.color = Handles.yAxisColor;

		/*
		if (p.sizeBindingAxis > 0)
		{
			switch(p.sizeBindingAxis)
			{
			case Axis.X:
				GUI.color = Handles.xAxisColor; break;
			case Axis.Y:
				GUI.color = Handles.yAxisColor; break;
			case Axis.Z:
				GUI.color = Handles.zAxisColor; break;
				
			}
			
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
			GUI.Box (new Rect(boxRect.x-m, boxRect.y-m, boxRect.width+4*m, boxRect.height+2*m), GUIContent.none);
		}
		*/
		/*
		if (EditorGUIUtility.isProSkin)
			GUI.color = new Color(defcolor.r, defcolor.b, defcolor.g, 1f);
		else
			GUI.color = new Color(defcolor.r, defcolor.b, defcolor.g, .4f);
		
		GUI.Box (boxRect, GUIContent.none);
		GUI.color = defcolor;
		
		if (EditorGUIUtility.isProSkin)
		{
			GUI.Box (boxRect, GUIContent.none);
			GUI.Box (boxRect, GUIContent.none);
			if (p.Parent.model.isSelected(p.Parent))
				GUI.Box (boxRect, GUIContent.none);
		}
		*/
		// BINDING HIGHLIGHT BACKGROUND BOX

		
		
		
		
		// PROPERTYDRAWER
		//EditorGUI.PropertyField(pRect, pProperty);
		
		//OR...
		GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
		labelstyle.alignment = TextAnchor.MiddleLeft;
		
		GUIStyle buttonstyle = GUI.skin.GetStyle ("Button");
		buttonstyle.alignment = TextAnchor.MiddleLeft;
		
		// NAME 

		string nameString = p.Name;// + "_" + Guid;

		if (p.Type == AXParameter.DataType.Mesh || p.Type ==  AXParameter.DataType.Spline ||  p.Type ==  AXParameter.DataType.Generic)
		{
			labelstyle.alignment = TextAnchor.MiddleLeft;
			labelstyle.fixedWidth = boxRect.width-10;
			GUI.Label(nameLabelRect, nameString);
		} 

		else if (p.PType == AXParameter.ParameterType.Output)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			labelstyle.fixedWidth = boxRect.width-10;
			GUI.Label(nameLabelRect, nameString);
		} 

		else if ((p.hasInputSocket && ! p.hasOutputSocket) )
		{
			labelstyle.alignment = TextAnchor.MiddleLeft;
			if (p.Parent.isEditing)
				GUI.Label(nameLabelRect, nameString);
			else 
				GUI.Label(nameLabelRect, nameString);
		}
		else if (p.PType == AXParameter.ParameterType.Output)//(!hasInputSocket && hasOutputSocket)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			if (p.Parent.isEditing)
				GUI.Label(nameLabelRect, nameString + " .... "  );
			else 
				GUI.Label(cntlRect, nameString);
			
		}
		else if (p.Type == AXParameter.DataType.Plane)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			if (p.Parent.isEditing)
				GUI.Label(nameLabelRect, nameString);
			else 
				GUI.Label(cntlRect, nameString);
		}
		else if (p.Type == AXParameter.DataType.MaterialTool)
		{
			labelstyle.alignment = TextAnchor.MiddleLeft;
			if (p.Parent.isEditing)
				GUI.Label(nameLabelRect, nameString);
			else 
				GUI.Label(cntlRect, nameString);
		}
	
		
		else if ((p.hasInputSocket && p.hasOutputSocket) || (p.Type == AXParameter.DataType.Float || p.Type ==  AXParameter.DataType.Int || p.Type ==  AXParameter.DataType.FloatList) )
		{


			
			EditorGUIUtility.fieldWidth = wid/3.2f;//36;
			EditorGUIUtility.labelWidth = wid-EditorGUIUtility.fieldWidth;
		
			GUI.backgroundColor =Color.white;

			EditorGUI.BeginChangeCheck ();
			p.isOpen = EditorGUI.Foldout (new Rect (pRect.x , cur_y, 20, lineHgt), p.isOpen, "");
			if(EditorGUI.EndChangeCheck())
			{
				if (p.isOpen )
				{
					foreach(AXParameter pop in p.parametricObject.parameters)
						if (pop != p)
							pop.isOpen = false;
				}
			}







			if (p.isOpen)
			{

				// NAME

				GUI.SetNextControlName("ParameterName_Text_NameField" + p.Guid + "_"  + p.Name);

				p.Name = EditorGUI.TextField(textFieldRect, p.Name);

				if (p.shouldFocus) 
				{
					GUI.FocusControl("ParameterName_Text_NameField" + p.Guid + "_"  + p.Name);
					p.shouldFocus = false;

				}


				// DELETE PARAMETER
				if (GUI.Button(new Rect(pRect.width-2*lineHgt*1.5f, cur_y-1, lineHgt*1.25f, lineHgt), "-"))
				{
					//Debug.Log("remove...");
					p.parametricObject.removeParameter(p);
					//p.expressions.RemoveAt(i);
				}

				// MOVE PARAMETER UP
				if (GUI.Button(new Rect(pRect.width-lineHgt*1.5f, cur_y-1, lineHgt*1.25f, lineHgt), "^"))
				{
					//Debug.Log("remove...");
					p.parametricObject.moveParameterUp(p);
					//p.expressions.RemoveAt(i);
				}




				cur_y += lineHgt +gap*3;

				GUI.Box(new Rect((x0-4), cur_y-4, (pRect.width-20), ((6+p.expressions.Count)*lineHgt)), " ");

				
				// DATA_TYPE
				//EditorGUI.PropertyField( new Rect((textFieldRect.x+textFieldRect.width)+6, textFieldRect.y, 60, 22), pProperty.FindPropertyRelative("m_type"), GUIContent.none);
				
				
				//EditorGUI.PropertyField( new Rect((textFieldRect.x+textFieldRect.width)+6, textFieldRect.y, 60, 22), m_type, GUIContent.none);
				//Rect rec = new Rect((textFieldRect.x+textFieldRect.width)+6, textFieldRect.y, 60, 22);
				Rect rec = new Rect(x0, cur_y, 60, 22);
				//m_type = (DataType) EditorGUI.EnumPopup(rec, m_type, "YUP");
				string dataType_menu = "Float|Int|Bool|String|Color|FloatList|CustomOption";
				string[] dataType_options = dataType_menu.Split('|');

				EditorGUI.BeginChangeCheck ();

                // get current setting
				int cur_typeIndex = (int) p.Type;

                switch(p.Type)
                {
                    case AXParameter.DataType.Float:
                        cur_typeIndex = 0;
                        break;
                    case AXParameter.DataType.Int:
                        cur_typeIndex = 1;
                        break;
                    case AXParameter.DataType.Bool:
                        cur_typeIndex = 2;
                        break;
                    case AXParameter.DataType.String:
                        cur_typeIndex = 3;
                        break;
                    case AXParameter.DataType.Color:
                        cur_typeIndex = 4;
                        break;
                    case AXParameter.DataType.FloatList:
                        cur_typeIndex = 5;
                        break;
                    case AXParameter.DataType.CustomOption:
                        cur_typeIndex = 6;
                        break;
                }

                
                cur_typeIndex = (int) EditorGUI.Popup(
					rec,
					"",
                    cur_typeIndex, 
					dataType_options);
				if (EditorGUI.EndChangeCheck ()) 
				{
					Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Parameter Type");

                   
                    switch (cur_typeIndex)
                    {
                        case 0:
                            p.Type = AXParameter.DataType.Float;
                            break;
                        case 1:
                            p.Type = AXParameter.DataType.Int;
                            break;
                        case 2:
                            p.Type = AXParameter.DataType.Bool;
                            break;
                        case 3:
                            p.Type = AXParameter.DataType.String;
                            break;
                        case 4:
                            p.Type = AXParameter.DataType.Color;
                            break;
                        case 5:
                            p.Type = AXParameter.DataType.FloatList;
                            break;
                        case 6:
                            p.Type = AXParameter.DataType.CustomOption;
                            break;

                    }

                 
					if (p.Type == AXParameter.DataType.Spline)
						p.Type = AXParameter.DataType.String;
				}
				
				
				cur_y += lineHgt +gap*3;
	
				
				// 	EXPOSE AS RUNTIME Interface ------------
				//EditorGUIUtility.labelWidth = wid-36;
				cntlRect = new Rect(x0, cur_y, wid, 16);
				EditorGUI.BeginChangeCheck ();
				p.exposeAsInterface = EditorGUI.Toggle (cntlRect, "Expose",  p.exposeAsInterface);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Expose Parameter");

					if (p.exposeAsInterface)
						p.parametricObject.model.addExposedParameter(p);
					else
						p.parametricObject.model.removeExposedParameter(p); 
				}
		
				cur_y += lineHgt +gap;
			
				
				
				/*
				if (p.Type == AXParameter.DataType.Float)
				{
					EditorGUI.BeginChangeCheck ();
					
					//EditorGUIUtility.labelWidth = 20;
					GUI.backgroundColor = Color.white;
					GUI.Label(new Rect(indent, cur_y, 200,16), "Bind externally in: ");
					p.sizeBindingAxis = EditorGUI.Popup(
						new Rect((textFieldRect.x+textFieldRect.width)+6, cur_y, 60,16),
						"",
						p.sizeBindingAxis, 
						new string[] {
						"None",
						"X",
						"Y",
						"Z"
					});
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Size bind Axis");
						Debug.Log ("sizeBindingAxis changed to "+p.sizeBindingAxis );
						
					}
					cur_y += lineHgt;
				}
				*/

				// STRING
				if (p.Type == AXParameter.DataType.String)
				{
					p.StringVal =  EditorGUI.TextField(new Rect(indent, cur_y, wid-10, lineHgt), p.StringVal);
					cur_y += lineHgt;
				}



				// NUMBER
				else if (p.Type == AXParameter.DataType.Float || (p.Type == AXParameter.DataType.Int))
				{

					// MIN/MAX

					cntlRect = new Rect(x0, cur_y, wid, 16);

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("FloatField_Text_MIN" + p.Guid + "_"  + p.Name);

					if (p.Type == AXParameter.DataType.Float)
						p.min = EditorGUI.FloatField(cntlRect,"Min",  p.min);
					else
						p.intmin = EditorGUI.IntField(cntlRect,"Min",  p.intmin);
					  
					if (EditorGUI.EndChangeCheck ()) {
						//Debug.Log(val);

						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);

						if (p.Type == AXParameter.DataType.Float)
							p.Parent.initiateRipple_setFloatValueFromGUIChange(p.Name, p.min);
						else
							p.Parent.initiateRipple_setIntValueFromGUIChange(p.Name, p.intmin);

						p.parametricObject.model.isAltered(27);
						p.parametricObject.generator.adjustWorldMatrices ();
						ArchimatixEngine.scheduleBuild();
					}

					cur_y += lineHgt;

					cntlRect = new Rect(x0, cur_y, wid, 16);

					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("FloatField_Text_MAX" + p.Guid + "_"  + p.Name);
					if (p.Type == AXParameter.DataType.Float)
						p.max = EditorGUI.FloatField(cntlRect,"Max",  p.max);
					else
						p.intmax = EditorGUI.IntField(cntlRect,"Max",  p.intmax);

					if (EditorGUI.EndChangeCheck ()) {
						//Debug.Log(val);

						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
						if (p.Type == AXParameter.DataType.Float)
							p.Parent.initiateRipple_setFloatValueFromGUIChange(p.Name, p.max);
						else 
							p.Parent.initiateRipple_setIntValueFromGUIChange(p.Name, p.intmax);

						p.parametricObject.model.isAltered(27);
						p.parametricObject.generator.adjustWorldMatrices ();
					}

					cur_y += lineHgt;

				

					GUI.Label(new Rect(indent, cur_y, 200,16), new GUIContent("Expressions","When the value of this parameter changes, define its effect on other paramters with mathematical descriptions using this parameter."));
					cur_y += lineHgt;
					




					
					// EXPRESSIONS

					if (p.expressions == null)
					{
						p.expressions = new List<string>();
					}
					if(p.expressions.Count == 0)
						p.expressions.Add ("");
					
					for(int i=0; i<p.expressions.Count; i++)
					{
						GUI.SetNextControlName("ParameterExpression_"+i+"_Text_" + p.Guid + "_" + p.Name);
						p.expressions[i] = EditorGUI.TextField(new Rect(indent, cur_y, wid-30, lineHgt), p.expressions[i]);


						if (GUI.Button(new Rect(pRect.width-lineHgt*1.5f, cur_y-1, lineHgt*1.25f, lineHgt), "-"))
						{
							p.expressions.RemoveAt(i);
						}

						cur_y += lineHgt+gap*2;
					}

					if(GUI.Button(new Rect(pRect.width-lineHgt*1.5f, cur_y, lineHgt*1.25f, lineHgt), new GUIContent("+", "Add an expression to this Parameter")))
						p.expressions.Add ("");
					cur_y += lineHgt;
				}

                else if (p.Type == AXParameter.DataType.CustomOption)
                {
                    string[] options = p.optionLabels.ToArray();

                    for(int i=0; i< p.optionLabels.Count; i++)
                    {
                        p.optionLabels[i] = EditorGUI.TextField(new Rect(indent, cur_y, wid - 30, lineHgt), p.optionLabels[i]);

                        if (GUI.Button(new Rect(pRect.width - lineHgt * 1.5f, cur_y - 1, lineHgt * 1.25f, lineHgt), "-"))
                        {
                            p.optionLabels.RemoveAt(i);
                        }
                        cur_y += lineHgt + gap * 2;

                    }

                    if (GUI.Button(new Rect(pRect.width - lineHgt * 1.5f, cur_y, lineHgt * 1.25f, lineHgt), new GUIContent("+", "Add an option item to this Parameter")))
                        p.optionLabels.Add("");
                    cur_y += lineHgt;
                }





                cur_y += lineHgt;



				// DONE
				if (GUI.Button(new Rect((wid-30-lineHgt*1.5f), cur_y-1, 50, pRect.height), "Done"))
				{
					
					p.isOpen = false;
					p.shouldFocus = false;
					AXEditorUtilities.clearFocus();
				}

				// MOVE PARAMETER UP
				if (GUI.Button(new Rect(pRect.width-lineHgt*1.5f, cur_y-1, lineHgt*1.25f, lineHgt), "v"))
				{
					//Debug.Log("remove...");
					p.parametricObject.moveParameterDown(p);
					//p.expressions.RemoveAt(i);
				}



				cur_y += lineHgt;
				 
				
			}
			else // (not open)
			{

				// NOT EDITING, RATHER USING
				string bindingLabel = "";
				switch(p.sizeBindingAxis)
				{
				case Axis.X:
					bindingLabel = " [X]"; break;
				case Axis.Y:
					bindingLabel = " [Y]"; break;
				case Axis.Z:
					bindingLabel = " [Z]"; break;
					
				}
				
				
				switch(p.Type)
				{

				case AXParameter.DataType.AnimationCurve:
					GUILayout.BeginArea(new Rect(indent, cur_y, wid-10, 2*lineHgt));

					EditorGUI.BeginChangeCheck ();
						EditorGUILayout.CurveField(p.animationCurve);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "ModifierCurve");
						p.parametricObject.model.isAltered(28);
						ArchimatixEngine.scheduleBuild();
					}

					GUILayout.EndArea();

					break;

					// COLOR
				case AXParameter.DataType.Color:

					p.colorVal = EditorGUI.ColorField(cntlRect, p.colorVal);

					break;
				

								

				case AXParameter.DataType.Float:
					
					// FLOAT SLIDER
					if(p.PType != AXParameter.ParameterType.DerivedValue)
					{

						// VALIDATE INPUT - IF INVALID >> LOSE FOCUS
						AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Text_FloatField_" + p.Name);

						EditorGUI.BeginChangeCheck ();
						GUI.SetNextControlName("FloatField_Text_" + p.Guid + "_"  + p.Name);
						p.val = EditorGUI.FloatField(cntlRect, nameString + bindingLabel,  p.val);
						if (EditorGUI.EndChangeCheck ()) {
							//Debug.Log(p.val);

							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
							p.parametricObject.initiateRipple_setFloatValueFromGUIChange(p.Name, p.val);
							p.parametricObject.model.isAltered(27);
							p.parametricObject.generator.adjustWorldMatrices ();
							ArchimatixEngine.scheduleBuild();
						}
					} 
					else
					{
						Rect labelRect = cntlRect;
						labelRect.width = cntlRect.width-25;
						GUI.Label(labelRect, p.Name );
						GUI.Label(new Rect(cntlRect.width-10, cntlRect.y, 18, cntlRect.height), ""+p.val );
					}

					if (p.shouldFocus)
					{
						GUI.FocusControl("FloatField_Text_" + p.Guid + "_"  + p.Name);
						p.shouldFocus = false;
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
					p.intval = EditorGUI.IntField(cntlRect, nameString,  p.intval);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
						p.initiateRipple_setIntValueFromGUIChange(p.intval);
						p.parametricObject.model.isAltered(28);
						p.parametricObject.generator.adjustWorldMatrices ();
						ArchimatixEngine.scheduleBuild();
					}
					
					break;
				}
				case AXParameter.DataType.Bool:
					{
						EditorGUIUtility.labelWidth = wid-16;
						EditorGUI.BeginChangeCheck ();

						GUI.SetNextControlName("BoolToggle_" + p.Guid + "_"  + p.Name);
						p.boolval = EditorGUI.Toggle (cntlRect, nameString,  p.boolval);
						if (EditorGUI.EndChangeCheck ()) {

							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);

							p.parametricObject.initiateRipple_setBoolParameterValueByName(p.Name, p.boolval);
							//p.parametricObject.model.autobuild();
							p.parametricObject.model.isAltered();
							p.parametricObject.generator.adjustWorldMatrices();

							ArchimatixEngine.scheduleBuild();
						}

						break;
					}

				case AXParameter.DataType.Vector3:

					// Vector3
					if(p.PType != AXParameter.ParameterType.DerivedValue)
					{

						// VALIDATE INPUT - IF INVALID >> LOSE FOCUS
						AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Text_FloatField_" + p.Name);

						EditorGUI.BeginChangeCheck ();
						GUI.SetNextControlName("Vector3_Text_" + p.Guid + "_"  + p.Name);
						p.vector3 = EditorGUI.Vector3Field( cntlRect, nameString,  p.vector3);
						if (EditorGUI.EndChangeCheck ()) {
							//Debug.Log(val);

							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
							//p.Parent.initiateRipple_setFloatValueFromGUIChange(p.Name, p.val);


							p.parametricObject.model.isAltered(27);
							p.parametricObject.generator.adjustWorldMatrices ();
							ArchimatixEngine.scheduleBuild();
						}


					} 

					if (p.shouldFocus)
					{
						GUI.FocusControl("Vector3_Text_" + p.Guid + "_"  + p.Name);
						p.shouldFocus = false;
					}

					cur_y += lineHgt;
					break;

				case AXParameter.DataType.String:
					labelstyle.alignment = TextAnchor.MiddleLeft;
					if (p.Parent.isEditing)
						GUI.Label(nameLabelRect, nameString);
					else 
						GUI.Label(nameLabelRect, nameString);

					break;
				case AXParameter.DataType.FloatList:
					labelstyle.alignment = TextAnchor.MiddleLeft;
					if (p.Parent.isEditing)
						GUI.Label(nameLabelRect, nameString);
					else 
						GUI.Label(nameLabelRect, nameString +"   ("+ p.floats.Count+ ")");

					break;
				case AXParameter.DataType.Option:
				{
					// OPTION POPUP
					
					string[] options = ArchimatixUtils.getMenuOptions(p.Name);
					EditorGUIUtility.labelWidth = wid-50;


					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("OptionPopup_" + p.Guid + "_"  + p.Name);	
					p.intval = EditorGUI.Popup(
						cntlRect,
						p.Name,
						p.intval, 
						options);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);


                                p.initiateRipple_setIntValueFromGUIChange(p.intval);
                                
                                p.parametricObject.generator.parameterWasModified(p);
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

					EditorGUIUtility.labelWidth = wid*.5f;


					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName("CustomOptionPopup_" + p.Guid + "_"  + p.Name);	
					p.intval = EditorGUI.Popup(
						cntlRect,
						p.Name,
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

	
					
				}// END switch (Type)
				



			}

			
		} 
		
		cur_y += lineHgt;
		
		
		GUI.backgroundColor = oldBackgroundColor;
		
		
		
		
		/*
		if(GUI.changed && ! editor.codeChanged) 
		{
			Debug.Log ("generate " + Parent.Name + " :: " + Name);
			//Parent.generateOutput("guid01");

			
		}
		*/
		GUI.color = defcolor;


		return cur_y-(int)pRect.y;
	}
	
	


}
