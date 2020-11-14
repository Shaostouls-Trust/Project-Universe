using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor.AnimatedValues;

using AX.SimpleJSON;




using AXGeometry;

using AX.Generators;
using AX;

/* This class manages the stored parametric objects.
 * 
 * In addition to the serialized as JSON for POs and their input subparts,
 * the library also manages thumbnails.
 * 
 * JSON Data files
 * These files have the Parmetric Assembly serialized as JSON
 * and also meta data for filtering the library browser including
 * – Type [Environment|Vehicle|Tool, etc.]
 * – Genre
 * – Period
 * – Description (real-world function)
 * – Author (Important for sharing or selling PO's]
 * 
 * Thumbnails can be generated automatically for Unity Pro users 
 * or uploaded from the desktop. Thumbnails are cued by PO name.
 * 
 * 
 * Rules:
 * Objects in the library mus:
 * 1. Have a unique name
 * 
 */

namespace AXEditor
{


	public class LibraryEditorWindow : EditorWindow 
	{

		[MenuItem("Window/Archimatix/Library")]
		static void InitLibrary() 
		{
			ArchimatixEngine.openLibrary3D();
		}

		/*
		[MenuItem("Window/Archimatix/2D Library")]
		static void InitLibrary2D() {
			ArchimatixEngine.openLibrary2D();
		}
		[MenuItem("Window/Archimatix/3D Library")]
		static void InitLibrary3D() {
			ArchimatixEngine.openLibrary3D();
		}

		*/




		public int thumbnailSize = 128;

		public string lastTextField = "";

		public static Rect rect;


		static public int showClass = 3; // 1: ALL, 2: 2D, 3: 3D

		public enum   ViewLayout {Grid, EditList};
		static public ViewLayout viewLayout;

		private Vector2 filtersScrollPosition;

		private int filtersHeight = 500;
		private AnimBool showFilters;


		// The library item currently being edited.
		public static LibraryItem editingLibraryItem;



		private Vector2 libraryScrollPosition;
		private Vector2 libraryScrollTargetPosition;
		private bool scrollingToTarget = false;
		public int currentRow = -999999;

		private bool editingLibrary = false;
		private bool editingDetailItem 	= false;


		public bool periodGroupEnabled = true;
		public bool[] period = new bool[4] { true, true, true, true };

		public bool genreGroupEnabled = true;
		public bool[] genre = new bool[3] { true, true, true };

		private List<string> queryTags;

		LibraryItem detailItem = null;

		AnimBool showDetailItem;

		Texture2D bgTex;
		Texture2D bgTexUp;

		[NonSerialized]
		private string searchString;


		int columnCount;
		int rowCount;
		int cellCount;



		public static GUIStyle nameLabelStyle;
		public static GUIStyle descripLabelStyle;

		public static GUIStyle richRightLabelStyle;
		public static GUIStyle richLeftLabelStyle;

		public static GUIStyle textfieldStyle;
		public static GUIStyle textareaStyle;





		void OnEnable()
		{
			if (ArchimatixEngine.library == null)
				ArchimatixEngine.library = Library.loadLibraryMetadata();

			 
			showFilters 	= new AnimBool(false, Repaint);
			showDetailItem 	= new AnimBool(false, Repaint);

			libraryScrollPosition = Vector2.zero;


			 
			if (ArchimatixEngine.ArchimatixAssetPath == null)
				ArchimatixEngine.establishPaths();

			bgTex 	= (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/zz_AXIcons-Shadow.png", typeof(Texture2D));
			bgTexUp = (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/zz_AXIcons-ShadowUp.png", typeof(Texture2D));




			//richLeftLabelStyle.wordWrap 				= true;

		}

		void initStyles()
		{
			
			nameLabelStyle 					= new GUIStyle(GUI.skin.label);
			nameLabelStyle.richText 		= true;
			nameLabelStyle.fixedHeight 		= 17;
			nameLabelStyle.fontSize 		= 14;
			nameLabelStyle.normal.textColor = Color.cyan;
			nameLabelStyle.fontStyle		= FontStyle.Bold;


			descripLabelStyle 					= new GUIStyle(GUI.skin.label);
			descripLabelStyle.richText 		= true;
			descripLabelStyle.wordWrap		= true;
			descripLabelStyle.alignment		= TextAnchor.UpperLeft;



			richRightLabelStyle 			= new GUIStyle(GUI.skin.label);
			richRightLabelStyle.richText 	= true;
			richRightLabelStyle.alignment 	= TextAnchor.UpperRight;
			richRightLabelStyle.fixedWidth 	= 100;
			 

			textfieldStyle = new GUIStyle(GUI.skin.textField);
			textfieldStyle.fontStyle = FontStyle.Bold;
			textfieldStyle.fixedHeight = 18;
			textfieldStyle.fontSize = 12;

			textareaStyle = new GUIStyle(GUI.skin.textArea);
			textareaStyle.wordWrap = true;

		}





		// ON_GUI
		void OnGUI()
		{

			if(richRightLabelStyle == null)
				initStyles();

			nameLabelStyle.normal.textColor = ArchimatixEngine.AXGUIColors["NameColor"];
			
			rect = position;

			HeaderOnGui();


			EditorGUILayout.Space();
			EditorGUILayout.Space();

			filtersScrollPosition = EditorGUILayout.BeginScrollView(filtersScrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height-100));


			if (viewLayout == ViewLayout.EditList)
				LibraryEditView.OnGUI();
			else {
				LibraryGridView.OnGUI(position);
			}
			EditorGUILayout.EndScrollView();

		}





		// HEADER GUI
		void HeaderOnGui()
		{

			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.UpperLeft;
			labelstyle.fixedWidth = 300;

			// WINDOW TITLE
			string libtype = (showClass == 3 ? "3D":"2D");



			EditorGUILayout.BeginHorizontal();

			labelstyle.fontSize  = 24;
			GUILayout.Label("Archimatix "+libtype+" Library");
			labelstyle.fontSize = 12;


			EditorGUILayout.Space();

			if (GUILayout.Button("Refresh"))
			{
				ArchimatixEngine.createLibraryFromJSONFiles();
			}
			EditorGUILayout.Space();

			if (showClass == 3)
			{
				if (GUILayout.Button("Switch to 2D"))
					showClass = 2;
			}
			else
			{
				if (GUILayout.Button("Switch to 3D"))
					showClass = 3;
			}

			EditorGUILayout.EndHorizontal();




			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();





			// SEARCH FIELD

			GUILayout.Label("Search", richRightLabelStyle);

			if (string.IsNullOrEmpty(searchString))
				searchString = "";

			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName("TF_SEARCH");
			searchString = GUILayout.TextField(searchString, GUILayout.Width(130));
			if (EditorGUI.EndChangeCheck()) {
				
				string[] words = searchString.Split(' ');
				ArchimatixEngine.library.filterResultsUsingSearch(new List<string>(words));

				lastTextField = "TF_SEARCH";
			}


			if(GUILayout.Button("x",  GUILayout.Width(20)))
			{
				GUI.FocusControl ("dummy");
				searchString = "";
				ArchimatixEngine.library.filteredResults = null;

			}


			GUILayout.FlexibleSpace();

			if (viewLayout == ViewLayout.EditList)
			{
				if(GUILayout.Button("Grid",  GUILayout.Width(100)))
				{
					viewLayout = ViewLayout.Grid;
				}
			}
			else
			{
				if(GUILayout.Button("Edit",  GUILayout.Width(100)))
				{
					viewLayout = ViewLayout.EditList;
				}

			}




			EditorGUILayout.EndHorizontal();


		}



		void OnGUIOld() 
		{

			Library library = ArchimatixEngine.library;

			Event e = Event.current;
			switch (e.type) {
			case EventType.MouseDown:
				//Debug.Log("Down");
				break;

			case EventType.DragUpdated:
				//Debug.Log("Drag and Drop sorting not supported... yet");
				break;
			case EventType.DragPerform:
				Debug.Log("Drag and Drop sorting not supported... yet");
			
			
				break;
			}
			//showDetailItem.target = EditorGUILayout.ToggleLeft("Show extra fields", showDetailItem.target);


			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.UpperLeft;
			labelstyle.fixedWidth = 300;




			//GUILayout.BeginHorizontal();

			// WINDOW TITLE
			string libtype = (showClass == 3 ? "3D":"2D");


			EditorGUILayout.BeginHorizontal();

			labelstyle.fontSize  = 24;
			GUILayout.Label("Archimatix "+libtype+" Library");
			labelstyle.fontSize = 12;


			EditorGUILayout.Space();

			if (GUILayout.Button("Refresh"))
			{
				ArchimatixEngine.createLibraryFromJSONFiles();
			}
			EditorGUILayout.Space();

			if (showClass == 3)
			{
				if (GUILayout.Button("Switch to 2D"))
					showClass = 2;
			}
			else
			{
				if (GUILayout.Button("Switch to 3D"))
					showClass = 3;
			}

			EditorGUILayout.EndHorizontal();

			//GUILayout.FlexibleSpace();



			//GUILayout.Space(10);


			//GUILayout.EndHorizontal();








			//labelstyle.fontSize  = 9;


			//GUILayout.Space(16);



			// SCROLL_VIEW

			GUIStyle shadowDown = new GUIStyle();
			shadowDown.normal.background = bgTex;

			GUIStyle shadowUp = new GUIStyle();
			shadowUp.normal.background = bgTexUp;


			 
			// BEGIN FILTER BAR
			EditorGUILayout.BeginHorizontal();


			GUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(25));


			GUILayout.EndVertical();

			GUILayout.Space(20);
			// FILTERS
			EditorGUI.BeginChangeCheck();
			showFilters.target = EditorGUILayout.ToggleLeft("Show Filters", showFilters.target);
			if (EditorGUI.EndChangeCheck()) {

				ArchimatixEngine.library.isFiltered = showFilters.target;
			}

			GUILayout.FlexibleSpace();

			// SEARCH
			GUILayout.Label("Search");
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName("TF_SEARCH");
			searchString = GUILayout.TextField(searchString, GUILayout.Width(130));
			if (EditorGUI.EndChangeCheck()) {
				string[] words = searchString.Split(' ');
				ArchimatixEngine.library.filterResultsUsingSearch(new List<string>(words));

				lastTextField = "TF_SEARCH";
			}
			if(GUILayout.Button("x",  GUILayout.Width(20)))
			{
				GUI.FocusControl ("dummy");
				searchString = "";
				ArchimatixEngine.library.filteredResults = null;

			}

			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return))
			{
				GUI.FocusControl(lastTextField);

				e.Use();
			}	

			GUILayout.Space(20);


			EditorGUILayout.EndHorizontal();
			// \ END FILTER BAR




			if (EditorGUILayout.BeginFadeGroup(showFilters.faded) && ArchimatixEngine.library.categories != null)
			{	



				// COLUMN LABELS
				GUILayout.BeginHorizontal();
				foreach(KeyValuePair<string, List<string>> kv in ArchimatixEngine.library.categories)
					GUILayout.Label (kv.Key, GUILayout.Width(130));
				GUILayout.EndHorizontal();

				GUILayout.Label(" ", shadowDown);

				filtersScrollPosition = EditorGUILayout.BeginScrollView(filtersScrollPosition, GUILayout.Width(position.width), GUILayout.Height(filtersHeight));

				// TAG CATEGORIES
				GUILayout.BeginHorizontal();

				if (queryTags == null)
					queryTags = new List<string>();

				foreach(KeyValuePair< string, List<string> > kv in library.categories)
				{
					GUILayout.BeginVertical(GUILayout.Width(130));

					//GUILayout.Label (kv.Key);

					foreach (string tag in kv.Value)
					{
						//GUILayout.Label ("- "+tag);
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.ToggleLeft(tag, queryTags.Contains(tag), GUILayout.Width(130));
						if(EditorGUI.EndChangeCheck()) {
							if (queryTags.Contains(tag))
								queryTags.Remove(tag);
							else
								queryTags.Add (tag);

							library.filterResultsUsing(queryTags);
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				// TAG CATEGORIES

				EditorGUILayout.EndScrollView();

				GUILayout.Label(" ", shadowUp);

				GUILayout.Space(16);

			} 

			EditorGUILayout.EndFadeGroup();






			// CATALOG




			List<LibraryItem> libraryItems = null;
			//if (showClass == 1)

			/*
			if (ArchimatixEngine.library.filteredResults.Count > 0)
			{


				libraryItems = ArchimatixEngine.library.filteredResults;
			}
			*/





			GUILayout.Label(" ", shadowDown);

			int currentFiltersHeight = (showFilters.target ? filtersHeight+36 : 0);
			Vector2 scrollPos = EditorGUILayout.BeginScrollView(libraryScrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height-120-currentFiltersHeight));


			if(Vector2.Distance(scrollPos,libraryScrollPosition)>1)
			{
				//Debug.Log ("NOT SCROLLING TO TARGET");
				scrollingToTarget = false;
				libraryScrollPosition = scrollPos;
			}
			else  if (scrollingToTarget )
			{
				libraryScrollPosition = Vector2.Lerp(libraryScrollPosition, libraryScrollTargetPosition, .05f);
				//Debug.Log ("SCROLLING TO TARGET "+Vector2.Distance(libraryScrollPosition, libraryScrollTargetPosition));
				if (Vector2.Distance(libraryScrollPosition, libraryScrollTargetPosition)<50)
				{
					scrollingToTarget = false;
				}
			}


			if (Event.current.type == EventType.Layout)
			{
				columnCount = (int) Math.Floor(position.width/(thumbnailSize+20));

				if (columnCount == 0)
					columnCount = 1;
				rowCount = libraryItems.Count / columnCount;
				rowCount++;

				cellCount = rowCount * columnCount;
			}





			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			bool showDetailRow = false;

			LibraryItem libraryItem;

			for (int i=0; i<cellCount; i++)
			{
				if (i >= libraryItems.Count)
					break;

				libraryItem = libraryItems[i];


				if (i % columnCount == 0 && i > 0) {
					GUILayout.EndHorizontal();


					if (showDetailRow)
					{
						showDetailRow = false;
						doDetailView();
					}

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
				}



				// MENU BUTTON 





				EditorGUILayout.BeginVertical();
				// ----------------------------

				//Debug.Log("LIBRARY: " + poList.Count);
				if (i < libraryItems.Count)
				{



					if (detailItem == libraryItem)
						showDetailRow = true;




					// THE BIG CLICK
					if(GUILayout.Button( libraryItems[i].icon, GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize)))
					{



						if (Event.current.shift)
						{
							//Make a new Model For Sure
							AXEditorUtilities.createNewModel();

							Library.instantiateParametricObject(libraryItem.readIntoLibraryFromRelativeAXOBJPath);
						}
						else
							Library.instantiateParametricObject(libraryItem.readIntoLibraryFromRelativeAXOBJPath);




					}

				}


				 

				// LABEL AND DELETE BUTTON
				EditorGUILayout.BeginHorizontal();

				string name = "";
				if (i < libraryItems.Count)
					name = libraryItem.Name;

				GUILayout.Label(name, GUILayout.Width(thumbnailSize-20), GUILayout.Height(16));

				if (i < libraryItems.Count)
				{
					if (editingLibrary )
					{
						if(GUILayout.Button("-",  GUILayout.Width(16)))
							Library.removeLibraryItem(libraryItem);
					}
					else
					{

						if(GUILayout.Button("i",  GUILayout.Width(16)))
						{

							if (detailItem == libraryItem && showDetailItem.faded == 1)
							{
								//detailItem = "";
								showDetailItem.target = false;
								libraryScrollTargetPosition = libraryScrollPosition + new Vector2(0, -thumbnailSize);
								scrollingToTarget = true;
							}
							else 
							{
								detailScreen(libraryItem);

								showDetailItem.target = true;

								int row = (int) Mathf.Floor (i/columnCount);

								//if (row != currentRow)
								//{
								libraryScrollTargetPosition = libraryScrollPosition + new Vector2(0, +thumbnailSize*2);
								scrollingToTarget = true;
								//}
								//else
								//scrollingToTarget = false;
								currentRow = row;

							}


						}
					}
				}


				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(8);
				// ------------------------------
				EditorGUILayout.EndVertical();





			}


			EditorGUILayout.EndHorizontal();


			if (showDetailRow)
			{
				showDetailRow = false;
				doDetailView();
			}

			GUILayout.Space(256);


			EditorGUILayout.EndScrollView();



			//GUILayout.FlexibleSpace();

			EditorGUILayout.BeginHorizontal();

			if (editingLibrary) 
			{
				if(GUILayout.Button("Done",  GUILayout.Width(32)))
					editingLibrary = false;
			}
			else
			{
				if(GUILayout.Button("Edit",  GUILayout.Width(32)))
					editingLibrary = true;
			}	
			GUILayout.FlexibleSpace();

			EditorGUI.BeginChangeCheck();
			thumbnailSize = EditorGUILayout.IntSlider(thumbnailSize, 32, 256, GUILayout.Width(300));
			if (EditorGUI.EndChangeCheck())
			{
				showDetailRow = false;
				detailItem = null;
			}

			EditorGUILayout.EndHorizontal();


			labelstyle.fixedWidth = 0;

		}


		public void setPositionIfNotDocked()
		{
			// Based on solution here: http://answers.unity3d.com/questions/62594/is-there-an-editorwindow-is-docked-value.html
			BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

			MethodInfo isDockedMethod = typeof( EditorWindow ).GetProperty( "docked", fullBinding ).GetGetMethod( true );

			if ( ( bool ) isDockedMethod.Invoke(this, null) == false ) // if not docked
				position = new Rect( 100, 100, Screen.currentResolution.width / 2, Screen.currentResolution.height * 3 / 4 );

		}



		public void detailScreen(LibraryItem li)
		{
			detailItem = li;
			GUI.FocusControl ("dummy");  


		}
















		// DETAIL VIEW

		public void doDetailView() 
		{

			GUIStyle shadowDown = new GUIStyle();
			Texture2D bgTex = (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/Shadow.png", typeof(Texture2D));
			shadowDown.normal.background = bgTex;



			if (EditorGUILayout.BeginFadeGroup(showDetailItem.faded))
			{					

				// DETAIL VIEW

				GUILayout.Space(15);


				GUILayout.BeginHorizontal(shadowDown,  GUILayout.Height(18));
				GUILayout.Label(" ");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

				// LARGE THUMBNAIL


				string thumbnailPath =  Path.ChangeExtension(detailItem.readIntoLibraryFromRelativeAXOBJPath, ".jpg");

				string thumbnailRelativePath = ArchimatixUtils.getRelativeFilePath(thumbnailPath); ;

				Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(thumbnailRelativePath, typeof(Texture2D));
				GUILayout.Label(tex, GUILayout.Width(256));

				GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
				GUIStyle buttonstyle = GUI.skin.GetStyle ("Button");

				GUILayout.Space(30);

				// DATA SHEET


				GUILayout.BeginVertical();






				// NAME
				labelstyle.fontSize  = 24;
				GUILayout.Label(detailItem.Name);
				labelstyle.fontSize = 10;


				if (editingDetailItem)
				{
					detailItem.author =  GUILayout.TextField(detailItem.author);

					EditorGUILayout.LabelField("Tags");
					detailItem.tags =  GUILayout.TextField(detailItem.tags);

					// TAG CATEGORIES
					GUILayout.BeginHorizontal();

					string tags = ","+detailItem.tags+",";

					foreach(KeyValuePair<string, List<string>> kv in ArchimatixEngine.library.categories)
					{
						GUILayout.BeginVertical();

						GUILayout.Label (kv.Key);

						foreach (string tag in kv.Value)
						{
							tags = ","+detailItem.tags+",";
							//GUILayout.Label ("- "+tag);
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.ToggleLeft(tag, tags.Contains(","+tag+","), GUILayout.Width(150));
							if(EditorGUI.EndChangeCheck()) {
								if (tags.Contains(tag))
									tags = tags.Replace( (","+tag+","), ",");
								else
									tags += tag;

								detailItem.tags = tags.Trim( new Char[] { ' ', ',' } );
							}
						}
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
					// TAG CATEGORIES



					GUILayout.FlexibleSpace();
					buttonstyle.alignment = TextAnchor.MiddleCenter;
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Cancel", GUILayout.Width(50)))
					{
						editingDetailItem = false;
					}
					if(GUILayout.Button("Save", GUILayout.Width(50)))
					{
						Debug.Log ("Saving author as: " + detailItem.author);
						editingDetailItem = false;


						//library.saveParametricObjectMetadata(detailItem);
					}

					GUILayout.EndHorizontal();
					buttonstyle.alignment = TextAnchor.UpperLeft;

				}
				else
				{

					// AUTHOR
					string author =  (string.IsNullOrEmpty(detailItem.author)) ? "anonymous" : detailItem.author;

					GUILayout.BeginHorizontal();
					labelstyle.alignment = TextAnchor.MiddleRight;
					GUILayout.Label("By: ", GUILayout.Width(50));
					labelstyle.alignment = TextAnchor.MiddleLeft;
					GUILayout.Label(author);
					GUILayout.EndHorizontal();

					if (string.IsNullOrEmpty(detailItem.tags)) detailItem.tags = "";

					string tmpTags = detailItem.tags.Replace (",", ", ");

					GUILayout.BeginHorizontal();
					labelstyle.alignment = TextAnchor.MiddleRight;
					GUILayout.Label("Tags: ", GUILayout.Width(50));
					labelstyle.alignment = TextAnchor.MiddleLeft;
					if (tmpTags == null || tmpTags == "")
						tmpTags = "none";
					GUILayout.Label(tmpTags);
					GUILayout.EndHorizontal();

					GUILayout.FlexibleSpace();
					buttonstyle.alignment = TextAnchor.MiddleCenter;
					if(GUILayout.Button("Edit", GUILayout.Width(50)))
					{
						editingDetailItem = true;

					}
					buttonstyle.alignment = TextAnchor.UpperLeft;


				}				


				GUILayout.EndVertical();

				GUILayout.EndHorizontal();




				// bottom shadow
				Texture2D bgTexUp = (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/ShadowUp.png", typeof(Texture2D));

				shadowDown.normal.background = bgTexUp;



				GUILayout.BeginHorizontal(shadowDown,  GUILayout.Height(18));
				GUILayout.Label(" ");
				GUILayout.EndHorizontal();



				GUILayout.Space(15);
			}
			EditorGUILayout.EndFadeGroup();


		}







		 

		





	}






}