using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using AX;

public class MeshOuputMenu 
{
 
	public static void contextMenu(AXParameter p, Vector2 position)
	{
	
		AXModel model = p.parametricObject.model;

        Debug.Log("yo");

		GenericMenu menu = new GenericMenu ();
			
			//parametricObject.generator.addContextMenuItems(menu);
			menu.AddSeparator("Organizers ...");
			menu.AddItem(new GUIContent("Grouper"), false,  () => {
				AXParametricObject npo = AXEditorUtilities.addNodeToCurrentModel("Grouper");
				AXParameter new_p = npo.addInputMesh();
				new_p.makeDependentOn(p);
				
				model.isAltered(21);
			});
			

			menu.AddSeparator(" ");
			menu.AddSeparator("Repeaters ...");
			menu.AddItem(new GUIContent("Instance"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("Instance").getParameter("Input Mesh").makeDependentOn(p);  model.autobuild();
			});
			menu.AddItem(new GUIContent("Replicant"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("Replicant").getParameter("Input Mesh").makeDependentOn(p);  model.autobuild();
			});
			
			menu.AddItem(new GUIContent("PairRepeater"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("PairRepeater").getPreferredInputParameter().makeDependentOn(p); model.autobuild();
			});
			
			menu.AddItem(new GUIContent("RadialRepeater"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("RadialRepeater").getPreferredInputParameter().makeDependentOn(p); model.autobuild();
			});
			
			
			menu.AddItem(new GUIContent("FloorRepeater (Node)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("FloorRepeater").getParameter("Node Mesh").makeDependentOn(p); model.autobuild();
			});
			menu.AddItem(new GUIContent("FloorRepeater (Node)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("FloorRepeater").getParameter("Story Mesh").makeDependentOn(p); model.autobuild();
			});
			
			// GRID_REPEATER
			menu.AddItem(new GUIContent("GridRepeater (Node)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("GridRepeater").getParameter("Node Mesh").makeDependentOn(p); model.autobuild();
			});
			menu.AddItem(new GUIContent("GridRepeater (Cell)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("GridRepeater").getParameter("Cell Mesh").makeDependentOn(p); model.autobuild();
			});
			menu.AddItem(new GUIContent("GridRepeater (Span)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("GridRepeater").getParameter("Bay Span").makeDependentOn(p); model.autobuild();
			});
			
			menu.AddItem(new GUIContent("ShapeRepeater (Node Mesh)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("ShapeRepeater").getParameter("Node Mesh").makeDependentOn(p); model.autobuild();
			});
			menu.AddItem(new GUIContent("ShapeRepeater (Bay Span Mesh)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("ShapeRepeater").getParameter("Bay Span Mesh").makeDependentOn(p); model.autobuild();
			});
			
			menu.AddItem(new GUIContent("ShapeRepeater (Corner Mesh)"), false,  () => {
				AXEditorUtilities.addNodeToCurrentModel("ShapeRepeater").getParameter("Corner Mesh").makeDependentOn(p); model.autobuild();
			});
						
			
			menu.ShowAsContext ();
			
		}

		
	

}
