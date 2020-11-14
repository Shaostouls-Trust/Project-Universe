using UnityEditor;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using AX.SimpleJSON;




using AXEditor;

using AX.Generators;
using AXGeometry;

using AX.GeneratorHandlers;
using AX;

[CustomEditor(typeof(AXPrototype))] 
public class AXPrototypeInspector : Editor 
{
	public override void OnInspectorGUI() 
	{
		AXPrototype proto = (AXPrototype) target;

		if (proto.parametricObjects != null && proto.parametricObjects.Count > 0)
		{
			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.fixedWidth = 350;

			String nodesWord  = (proto.parametricObjects.Count == 1) ? "node" : "nodes";
			GUILayout.Label(proto.parametricObjects.Count + " " + nodesWord + " using this as a prototype:");
			for (int i=0; i<proto.parametricObjects.Count; i++)
			{
				//GUILayout.Label(proto.parametricObjects[i].Name);
				if (GUILayout.Button(proto.parametricObjects[i].Name))
				{
					
					proto.parametricObjects[i].model.selectAndPanToPO(proto.parametricObjects[i]);
					Selection.activeGameObject = proto.parametricObjects[i].model.gameObject;
				}

			}


		}
	}


}
