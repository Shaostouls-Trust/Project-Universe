using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;


namespace AXEditor
{

	public class AXRuntimeHandlesGUI{

		// Maintain a AXHandle.GUID
		// and any other runtime preferences 



		public static void OnGUI(AXHandleRuntimeAlias rth) 
		{

			AXHandle handle = rth.handle;

			if (handle == null)
				return;

			//Event e = Event.current;

			GUILayout.BeginHorizontal();




			// FOLDOUT

			GUIStyle toggleStyle = new GUIStyle(EditorStyles.foldout);
			toggleStyle.fixedWidth =50;

			rth.isEditing = EditorGUILayout.Foldout(rth.isEditing, GUIContent.none );

			//Debug.Log(pa.isEditing);
			// SLIDER AND NUMBER FIELD 

			GUIStyle textfieldStyle = new GUIStyle(GUI.skin.textField); 
			textfieldStyle.fixedWidth =150;




			GUIStyle labelWrap = new GUIStyle(GUI.skin.label); 
			labelWrap.stretchWidth = true;
			labelWrap.wordWrap = true;
			labelWrap.fontSize = 9;
			labelWrap.fixedWidth = 150;
			if (rth.isEditing)
			{
				GUILayout.BeginVertical();

				rth.alias = GUILayout.TextField(rth.alias, textfieldStyle);

				GUILayout.Label("You can edit the alias without altering the source handle.", labelWrap);

				GUILayout.Label("Actual: ", labelWrap);
				if (GUILayout.Button(handle.parametricObject.Name + "." + handle.Name))
				{
					handle.parametricObject.model.selectPO(handle.parametricObject);
					//handle.ParentNode.isOpen = true;

					float framePadding = 200;

					Rect 	allRect = AXUtilities.getBoundaryRectFromPOs(handle.parametricObject.model.selectedPOs);
							allRect.x 		-= framePadding;
							allRect.y 		-= framePadding;
							allRect.width 	+= framePadding*2;
							allRect.height 	+= framePadding*2;

							AXNodeGraphEditorWindow.zoomToRectIfOpen(allRect);

				}


				GUILayout.Space(15);
				GUILayout.EndVertical();

			}
			else
			{
				GUILayout.Label(rth.alias);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();




		} // OnGUI()
	}

}
