using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AX;

public class RelationEditorGUI  {

	// Use this for initialization
	public static void OnGUI (AXRelation r) {

		
		GUIStyle richLabelStyle 		= new GUIStyle(GUI.skin.label);
		richLabelStyle.richText 		= true;




		string labelA = r.pA.Parent.Name + "." + r.pA.Name;
		string labelB = r.pB.Parent.Name + "." + r.pB.Name;



		GUILayout.Label("<color="+((EditorGUIUtility.isProSkin) ? "#bbbbff" : "#660022")+"> <size=13>Relation Expressions</size></color>", richLabelStyle);


		GUILayout.Space(5);


		// SLIDING A SETS B
		EditorGUILayout.BeginHorizontal();

		// B = f(A)
		GUILayout.Label(labelB +"=", GUILayout.Width(150));

		// swap the internal variable name to let the user always use current name, 
		// even if that name has changed since the last time the expresion was edited.
		if (r.expression_AB != null)
		{	
//			if (
//				Debug.Log();
			GUI.SetNextControlName("expression_Text_AB_"+r.pA_guid + "_"+r.pB_guid);

			r.expression_AB = GUILayout.TextField(r.expression_AB.Replace(r.pA.guidAsEpressionKey, labelA));
			r.expression_AB = r.expression_AB.Replace(labelA, r.pA.guidAsEpressionKey);

		}
		EditorGUILayout.EndHorizontal();




		// SLIDING B SETS A
		EditorGUILayout.BeginHorizontal();

		// A = f(B)
		GUILayout.Label(labelA +"=", GUILayout.Width(150));

		// swap the internal variable name to let the user always use current name, 
		// even if that name has changed since the last time the expresion was edited.
		string tmp_expression_BA = "";

		if (r.expression_BA != null)
		{	
			GUI.SetNextControlName("expressionBA_Text_BA_"+r.pA_guid + "_"+r.pB_guid);
			tmp_expression_BA = GUILayout.TextField(r.expression_BA.Replace(r.pB.guidAsEpressionKey, labelB));
			r.expression_BA = tmp_expression_BA.Replace(labelB, r.pB.guidAsEpressionKey);
		}
		EditorGUILayout.EndHorizontal();


		GUILayout.Space(10);


		// FOOTER MENU

		AXModel model = r.pA.parametricObject.model;



		// HORIZONTALMENU
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Revert"))
		{
			r.revert ();
			AXNodeGraphEditorWindow.repaintIfOpen();
		}
		if(GUILayout.Button("Save"))
		{
			model.selectedRelationInGraph 	= null;

			//relationEditWindowIsDisplayed = false;
		}
		if(GUILayout.Button("Delete"))
		{
			//Debug.Log("model "+ model);
			model.unrelate(r);
			model.selectedRelation 			= null;
			model.selectedRelationInGraph 	= null;
		}
		if(GUILayout.Button("Cancel"))
		{
			r.revert ();
			model.selectedRelationInGraph 	= null;
			model.selectedRelation 			= null;
		}
		EditorGUILayout.EndHorizontal();

//		labelStyle.alignment = textAlignment;
//		labelStyle.fixedWidth = fixedWidth;

	}
	

}
