using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using AX;


namespace AXEditor
{


	public class LibraryEditView 
	{

		public static int thumbnailSize = 148;




		public static void OnGUI()
		{

			List<LibraryItem> libraryItems = ArchimatixEngine.library.filteredResults;


			if (libraryItems != null && libraryItems.Count > 0)
			{
				for (int i = 0; i < libraryItems.Count; i++) {
					LibraryItem li = libraryItems [i];
					if (LibraryEditorWindow.showClass == 1 || (LibraryEditorWindow.showClass == 2 && li.is2D) || (LibraryEditorWindow.showClass == 3 && !li.is2D))
						LibraryItemEdit_OnGUI (li);
				}
			}
			else
			{
				EditorGUILayout.Space();
				 
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				GUI.color = Color.cyan;
				GUILayout.Label("No results found");
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

		}





		// INDIVIDUAL LIBRARY ITEM

		public static void LibraryItemEdit_OnGUI(LibraryItem li)
		{
			// CLICKABLE THUMBNAIL

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			float rightMargin = 300;

			if(GUILayout.Button( li.icon, GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize)))
			{
				if (Event.current.shift)
				{ 	//Make a new Model For Sure
					AXEditorUtilities.createNewModel();
				}
				AX.Library.instantiateParametricObject(li.readIntoLibraryFromRelativeAXOBJPath);
			}






			EditorGUILayout.BeginVertical();

			// NAME
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Name", LibraryEditorWindow.richRightLabelStyle);


			if (LibraryEditorWindow.editingLibraryItem != li)
				GUILayout.Label(li.Name, LibraryEditorWindow.nameLabelStyle);
			else
			{
				GUI.SetNextControlName("LibraryItemEdit_Text_" + li.guid + "_name");
				li.Name = GUILayout.TextField(li.Name, LibraryEditorWindow.textfieldStyle, new GUILayoutOption[] { GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin), GUILayout.MinWidth(200)});
			}
			GUILayout.FlexibleSpace();


			EditorGUILayout.EndHorizontal();




			// AUTHOR
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Author", LibraryEditorWindow.richRightLabelStyle);
			if (string.IsNullOrEmpty(li.author)) 
				li.author = "";
			if (LibraryEditorWindow.editingLibraryItem != li)
				GUILayout.Label(li.author);
			else
			{	GUI.SetNextControlName("LibraryItemEdit_Text_" + li.guid + "_author");
				li.author = GUILayout.TextField(li.author, new GUILayoutOption[] { GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin), GUILayout.MinWidth(200)});
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();



			// DESCRIPTION
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Description", LibraryEditorWindow.richRightLabelStyle);
			if (string.IsNullOrEmpty(li.description)) 
				li.description = "";

			if (LibraryEditorWindow.editingLibraryItem != li)
				GUILayout.Label(li.description, LibraryEditorWindow.descripLabelStyle, new GUILayoutOption[] {GUILayout.Height(50), GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin), GUILayout.MinWidth(200)});
			else
			{
				GUI.SetNextControlName("LibraryItemEdit_Text_" + li.guid + "_description");
				li.description = GUILayout.TextArea(li.description, LibraryEditorWindow.textareaStyle, new GUILayoutOption[] {GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin), GUILayout.MinWidth(200), GUILayout.Height(50)});
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();


			// TAGS
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Tags", LibraryEditorWindow.richRightLabelStyle);

			if (string.IsNullOrEmpty(li.tags)) 
				li.tags = "";

			if (LibraryEditorWindow.editingLibraryItem != li)
				GUILayout.Label(li.tags);
			else
			{
				GUI.SetNextControlName("LibraryItemEdit_Text_" + li.guid + "_tags");
				li.tags = GUILayout.TextField(li.tags, new GUILayoutOption[] {GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin), GUILayout.MinWidth(200)});
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();


			// DOCUMENTATION_URL
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Documentation", LibraryEditorWindow.richRightLabelStyle);

			if (string.IsNullOrEmpty(li.documentationURL)) 
				li.documentationURL = "";

			if (LibraryEditorWindow.editingLibraryItem != li)
				GUILayout.Label(li.documentationURL);
			else
			{
				GUI.SetNextControlName("LibraryItemEdit_Text_" + li.guid + "_documentationURL");
				li.documentationURL = GUILayout.TextField(li.documentationURL, new GUILayoutOption[] {GUILayout.Width(LibraryEditorWindow.rect.width-rightMargin)});
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();



			// VISIBLE IN SIDE_BAR
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("In Sidebar Menu", LibraryEditorWindow.richRightLabelStyle);
			EditorGUI.BeginChangeCheck();
			li.includeInSidebarMenu = EditorGUILayout.Toggle("", li.includeInSidebarMenu);
			if (EditorGUI.EndChangeCheck()) {
				ArchimatixEngine.library.saveLibraryMetadata();
				li.saveToFile();
			}
			EditorGUILayout.EndHorizontal();




			// FOOTER MENU
			EditorGUILayout.BeginHorizontal();

			if (LibraryEditorWindow.editingLibraryItem != li)
			{
				GUILayout.Label("", LibraryEditorWindow.richRightLabelStyle);


				// EDIT
				if (GUILayout.Button("Edit"))
				{
					li.cacheSelf();
					LibraryEditorWindow.editingLibraryItem = li;
				}

			}
			else
			{	
				// DELETE_ITEM
				GUILayout.Label("", LibraryEditorWindow.richRightLabelStyle, new GUILayoutOption[] {GUILayout.MaxWidth(16)});
				if (GUILayout.Button("Delete Item"))
				{
					Library.removeLibraryItem(li);
				}
				EditorGUILayout.Space();


				// CANCEL
				if (GUILayout.Button("Cancel"))
				{
					li.revertFromCache();
					LibraryEditorWindow.editingLibraryItem = null;
				}

				EditorGUILayout.Space();
				 

				// REVERT
				if (GUILayout.Button("Revert"))
				{
					li.revertFromCache();
				}

				EditorGUILayout.Space();

				// SAVE
				if (GUILayout.Button("Save"))
				{
					li.saveToFile();
					li.cacheSelf();

					LibraryEditorWindow.editingLibraryItem = null;


				}				

				EditorGUILayout.Space();

				GUILayout.FlexibleSpace();




			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();



			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

		}



	}

}