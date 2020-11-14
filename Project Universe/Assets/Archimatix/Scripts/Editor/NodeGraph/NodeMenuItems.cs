using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;


using AX;
using AXEditor;

public static class NodeMenuItems {


	// MENU ITEMS FOR LIBRARY

	[MenuItem("Tools/Archimatix Nodes/Shape Merger")]
	static void createShapeMerger() {
		createNode("ShapeMerger");

	}


	public static void createNode(string nodeName)
	{

		AXModel model = ArchimatixEngine.getOrMakeCurrentModel();

		// ADD NEW PO TO MODEL (only this new po is selected after this)
	    AXEditorUtilities.addNodeToCurrentModel(nodeName, false);



        model.cleanGraph();
		model.autobuild();
		EditorUtility.SetDirty( model.gameObject );

	}

}
