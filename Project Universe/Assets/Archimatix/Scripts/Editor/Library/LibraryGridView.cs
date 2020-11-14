using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using AX;


namespace AXEditor
{


	public class LibraryGridView 
	{

		public static int thumbnailSize = 100;



		public static int  columnCount = 0;
		public static int  rowCount	= 0;
		public static int  cellCount	= 0;


		 

		public static void OnGUI(Rect position)
		{

			int displayedCount = 0;

			for (int i=0; i< ArchimatixEngine.library.filteredResults.Count; i++)
			{
				LibraryItem li = ArchimatixEngine.library.filteredResults[i];

				if (LibraryEditorWindow.showClass == 1 || (LibraryEditorWindow.showClass == 2 && li.is2D) || (LibraryEditorWindow.showClass == 3 && !li.is2D))
				{
					displayedCount++;
				}
				
			}
			if (Event.current.type == EventType.Layout)
			{
				columnCount = (int) Math.Floor(position.width/(thumbnailSize));

				if (columnCount == 0)
					columnCount = 1;
				rowCount = displayedCount / columnCount;
				rowCount++;

				cellCount = rowCount * columnCount;
			}










			// Grid
			int cursor = 0;

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int i=0; i<ArchimatixEngine.library.filteredResults.Count; i++)
			{
				if (i >= ArchimatixEngine.library.filteredResults.Count)
					break;

				LibraryItem li = ArchimatixEngine.library.filteredResults[i];


				if (LibraryEditorWindow.showClass == 1 || (LibraryEditorWindow.showClass == 2 && li.is2D) || (LibraryEditorWindow.showClass == 3 && !li.is2D))
				{
					
					if (columnCount > 0 && cursor % columnCount == 0 && cursor > 0) {
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();


						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
					}


					// THE BIG CLICK
					if(GUILayout.Button( li.icon, GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize)))
					{
						if (Event.current.shift)
						{
							//Make a new Model For Sure
							AXEditorUtilities.createNewModel();

							AX.Library.instantiateParametricObject(li.readIntoLibraryFromRelativeAXOBJPath);
						}
						else
							AX.Library.instantiateParametricObject(li.readIntoLibraryFromRelativeAXOBJPath);

					}

					cursor++;
				}
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();





		}

	}

}