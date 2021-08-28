#pragma warning disable

using UnityEditor;
using UnityEngine;

using System;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using System.Threading;
using System.Globalization;

using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using Debug = UnityEngine.Debug;


namespace AXEditor
{




	[InitializeOnLoad]
	public class ArchimatixEngine
	{

		// This singleton manages SceneView, Commands-keys, etc. 
		// It also manages Selection, which is a layer on top of Unity GameObject selection. 
		// Individual AWGameObjects may be selected in the scene, but the AXEngine knows 
		// the "currentModel" and AXParametricObject that created these objects.

		public enum SceneViewState { Default, AddPoint };

		[System.NonSerialized]
		public static SceneViewState sceneViewState;

		[System.NonSerialized]
		public static bool sceneViewPainted;

		public static string doucumentationDomain = "archimatix.com";



		public static string version = "1.0.5";
		public static string identifier = "TH_AX";

		public static bool libraryInitCalled;


		public static int updateCounter = 0;


		public static Stopwatch buildStopwatch;


		static int windowPickerID;



		[System.NonSerialized]
		public static Dictionary<string, Texture2D> nodeIcons;

		[System.NonSerialized]
		public static Dictionary<string, Texture2D> uiIcons;


		//														"CSGCombiner",

		//public static string[] nodeStrings 					= {  "FreeCurve", "ShapeOffsetter", "ShapeMerger", "GridRepeater2D", "ShapeDistributer", "Curve2D", "Library2D", "PlanePrimitive", "BoxPrimitive", "Polygon", "Extrude", "Lathe", "PlanSweep",  "Grouper", "PairRepeater", "LinearRepeater", "GridRepeater", "FloorRepeater", "StepRepeater", "RadialRepeater",  "PlanRepeater", "RepeaterTool", "MaterialTool","JitterTool", "RandomTool"  };
		public static string[] nodeStrings = { "FreeCurve", "ShapeDistributor", "ShapeChanneler", "ShapeConnector", "ShapeMerger", "ShapeOffsetter", "PairRepeater2D", "LinearRepeater2D", "GridRepeater2D", "RadialRepeater2D", "PlanRepeater2D", "ImageShaper", "ShapeNoiser", "PlanePrimitive", "BoxPrimitive", "Polygon", "Extrude", "Lathe", "PlanSweep", "ContourExtruder", "Grouper", "Channeler", "PairRepeater", "LinearRepeater", "GridRepeater", "FloorRepeater", "StepRepeater", "RadialRepeater", "RadialStepRepeater", "PlanPlacer", "PlanRepeater", "NoiseDeformer", "TaperDeformer", "ShearDeformer", "TwistDeformer", "DomicalDeformer", "InflateDeformer", "PlanDeformer", "TerrainDeformer", "TerrainStamper", "PhysicsJoiner", "PrefabInstancer", "RepeaterTool", "RadialRepeaterTool", "UVProjector", "MaterialTool", "TextureTool", "JitterTool" };

		//"Replicator",  

		public static string[] nodeStringsFrom2DOutput = { "ShapeDistributor", "ShapeChanneler", "ShapeConnector", "ShapeMerger", "ShapeOffsetter", "PairRepeater2D", "LinearRepeater2D", "RadialRepeater2D_Node", "RadialRepeater2D_Cell", "GridRepeater2D_Node", "GridRepeater2D_Cell", "RadialRepeater2D", "PlanRepeater2D_Plan", "PlanRepeater2D_Corner", "ShapeNoiser", "Polygon_Plan", "Extrude_Plan", "Lathe_Section", "PlanSweep_Plan", "PlanSweep_Section", "ContourExtruder_Plan", "ContourExtruder_Section", "PlanPlacer_Plan", "PlanRepeater_Plan", "PlanRepeater_Section", "PlanDeformer_Plan" };

		public static string[] nodeStringsFrom3DOutput = { "Grouper", "Channeler", "PairRepeater", "LinearRepeater_Node", "LinearRepeater_Cell", "LinearRepeater_Span", "GridRepeater_Node", "GridRepeater_Cell", "GridRepeater_Span", "FloorRepeater", "StepRepeater", "RadialRepeater_Node", "RadialRepeater_Span", "RadialStepRepeater", "PlanPlacer_Mesh", "PlanRepeater_Node", "PlanRepeater_Cell", "PlanRepeater_Corner", "NoiseDeformer", "TaperDeformer", "ShearDeformer", "TwistDeformer", "DomicalDeformer", "InflateDeformer", "PlanDeformer", "RepeaterTool", "MaterialTool", "UVProjector" };

		public static string[] nodeStringsFromRepeaterTool = { "Grouper", "Channeler", "LinearRepeater", "GridRepeater", "FloorRepeater" };

		public static string[] nodeStringsFrom2DMultiSelect = { "ShapeMerger", "ShapeChanneler" };
		public static string[] nodeStringsFrom3DMultiSelect = { "Grouper", "Channeler" };
		public static string[] nodeStringsFromMultiSelect = { "ShapeMerger", "ShapeChanneler", "Grouper", "Channeler" };

		public static float buttonSize = 16;

		public static bool keyIsDown = false;

		// PROXY FOR NODE_GRAPH_EDITOR... if it is included in this installation
		public static bool NodeGraphEditorIsInstalled;
		public static Type NodeGraphEditorType;
		public static MethodInfo RepaintGraphEditorIfOpenMethodInfo;

		public delegate void OpenNodeGraphEditrWindow();

		public static bool meshBufferJustClearedOnUpdate;

		public static Texture2D menubarTexture;

		// SCEN_VIEW MEMBERS

		public static bool snappingIsOn = false;
		public static bool useKyle = true;

		public static bool shiftIsDown;

		public static bool justClickedOnHandle;
		public static long lastHandleClick;

		public static int isPseudoDraggingSelectedPoint = -1;
		[System.NonSerialized]
		public static int draggingNewPointAt = -1;

		// The FreeCurve of the handle being dragged
		public static FreeCurve selectedFreeCurve = null;




		// NEW MODEL
		/*
		[MenuItem("GameObject/3D Object/Archimatix Objects/New AX Model")]
		static void NewModel() {
			
			AXEditorUtilities.createNewModel();
			AXNodeGraphEditorWindow edwin = (AXNodeGraphEditorWindow) EditorWindow.GetWindow(typeof(AXNodeGraphEditorWindow));
		edwin.titleContent = new GUIContent("Archimatix");
		edwin.autoRepaintOnSceneChange = true;


		}
		*/



		public static bool mouseIsDownOnHandle;


		[System.NonSerialized]
		public static Library library;



		// PATHS

		[System.NonSerialized]
		public static string ArchimatixAssetPath;

		[System.NonSerialized]
		public static string pathChangelog;



		public static GUIStyle sceneViewLabelStyle;


		public static List<UserMessage> userMessages;

		public static Color ccc = new Color(1, 1, 1, 1);

		public static Vector3 sceneViewCameraPosition;
		public static GameObject ScalingFigureBillboardPrefab;
		public static GameObject ScalingFigureBillboard;
		public static Transform ScalingFigureBillboardTransform;

		public static bool lookForScalingBillboard = true;

		//public static Texture2D infoIconTexture;


		public static Dictionary<string, Color> AXGUIColors;
		public static Color defaultDataColor;

		public static bool drawGuides = false;


		[System.NonSerialized]
		public static Dictionary<string, GeneratorHandler> generatorHandlerCache;

		public static int plevel = 3;

		// Script Writing
		// Once a .cs script has been created, assign it as a component after an AssetDatabase refresh is done.
		//bool waitingToAddComponentAfterCompile
		public static AXModel createControllerForModel = null;

		// RUNTIME

		public static GameObject RuntimeHandlePrefab_PlanarKnob;





		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded()
		{


			// do something
			//Debug.Log("COMPILE DONE");
			//Debug.Log("LOAD DEFAULT");
			loadDefaultMaterial();



			AXModel[] axModels = GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];
			foreach (AXModel m in axModels)
			{
				//Debug.Log(m.name);

				m.assertDefaultMaterial();

				m.regenerateAXMeshes(true);

			}


			// DO WE NEED TO CONNECT A NEWLY CREATED SCRIPT TO AN A GAMEOBJECT?
			string AddComponentByGUIDS = EditorPrefs.GetString("AddComponentByGameObjectIDAndClassname");
			if (!string.IsNullOrEmpty(AddComponentByGUIDS))
			{
				string[] data = AddComponentByGUIDS.Split('_');

				if (data != null && data.Length == 2 && !string.IsNullOrEmpty(data[0]) && !string.IsNullOrEmpty(data[0]))
				{
					GameObject go = (GameObject)EditorUtility.InstanceIDToObject(int.Parse(data[0]));
					System.Type theType = ArchimatixUtils.AXGetType(data[1]);
					//Debug.Log(" ------------------------ " + theType);
					if (go != null && theType != null)
						go.AddComponent(theType);
				}
				EditorPrefs.DeleteKey("AddComponentByGameObjectIDAndClassname");
			}

		}




		public static string GetVersion()
		{


			//			if (fileInfo != null) {
			//				StreamReader sr = fileInfo.OpenText();
			//				version = sr.ReadLine();
			//				sr.Close();
			//
			//				return version;
			//
			//			}
			//			 


			TextAsset asset = (TextAsset)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/Scripts/AXChangeLog.txt", typeof(TextAsset));
			if (asset != null)
			{
				string[] fLines = Regex.Split(asset.text, "\n|\r|\r\n");
				if (fLines != null && fLines.Length > 0)
				{
					version = fLines[0];
					//Debug.Log (version);
				}
			}


			return "no version found";
		}




		public static void establishPaths()
		{
			//Debug.Log("establishPaths..."); 
			//			if (String.IsNullOrEmpty(Archimatix.ArchimatixAssetPath))
			//			{

			String priorArchimatixAssetPath = EditorPrefs.GetString("priorArchimatixAssetPath");


			if (AssetDatabase.IsValidFolder("Assets/Archimatix"))
			{
				// path is a directory.

				ArchimatixEngine.ArchimatixAssetPath = "Assets/Archimatix";
				Archimatix.ArchimatixAssetPath = "Assets/Archimatix";

			}
			else if (!string.IsNullOrEmpty(priorArchimatixAssetPath) && AssetDatabase.IsValidFolder(priorArchimatixAssetPath))
			{
				ArchimatixEngine.ArchimatixAssetPath = priorArchimatixAssetPath;
				Archimatix.ArchimatixAssetPath = priorArchimatixAssetPath;

			}
			else
			{
				// Find the Archimatix folder, whereever the user may have placed it in the Assets folder.

				string[] dirs = Directory.GetDirectories(Application.dataPath, "Archimatix", SearchOption.AllDirectories);


				if (dirs != null && dirs.Length > 0)
				{
					//Debug.Log (dirs[0].ToString());
					ArchimatixEngine.ArchimatixAssetPath = ArchimatixUtils.getRelativeFilePath(dirs[0].ToString());
					Archimatix.ArchimatixAssetPath = ArchimatixEngine.ArchimatixAssetPath;
				}
			}

			//Debug.Log(Archimatix.ArchimatixAssetPath);

			if (!string.IsNullOrEmpty(priorArchimatixAssetPath) && Archimatix.ArchimatixAssetPath != priorArchimatixAssetPath)
			{
				EditorPrefs.SetString("priorArchimatixAssetPath", Archimatix.ArchimatixAssetPath);
				//Debug.Log("folder moved");
				createLibraryFromJSONFiles();
			}


			//}
			// The pathChangelog by default is Assets/Archimatix/Scripts
			pathChangelog = ArchimatixEngine.ArchimatixAssetPath + "/Scripts/AXChangeLog.txt";
			GetVersion();



			sceneViewLabelStyle = new GUIStyle();
			sceneViewLabelStyle.alignment = TextAnchor.MiddleCenter;
			sceneViewLabelStyle.normal.textColor = Color.white;





			ArchimatixEngine.InitColors();


			generatorHandlerCache = new Dictionary<string, GeneratorHandler>();

		}

		public void onArchimaticFolderMoved()
		{

		}

		public static void InitColors()
		{




			//string filepath = ArchimatixEngine.ArchimatixAssetPath +"/

			// GUI COLORS
			AXGUIColors = new Dictionary<string, Color>();


			if (EditorGUIUtility.isProSkin)
			{
				AXGUIColors.Add("GridColor", new Color(0.6f, 0.6f, .6f, .65f));
				AXGUIColors.Add("AxisColor", new Color(0.8f, 0.33f, .33f, .85f));
				AXGUIColors.Add("ConnectorShadowColor", new Color(.1f, .1f, .1f, .6f));
				AXGUIColors.Add("ConnectorColor", new Color(0.9f, .6f, .65f, 1.0f));


				//AXGUIColors.Add("NodePaletteBG", 			new Color(.9f, .9f, .9f, 1));
				AXGUIColors.Add("NodePaletteBG", new Color(.99f, .99f, .99f, 1));
				AXGUIColors.Add("NodePaletteHighlight", new Color(1f, 1f, 1, 1f));
				AXGUIColors.Add("NodePaletteHighlightRect", new Color(.7f, .7f, 1, 1f));



				AXGUIColors.Add("Float", new Color(1.0f, .5f, .4f, 1));
				AXGUIColors.Add("Int", new Color(.8f, .9f, .9f, 1));
				AXGUIColors.Add("Bool", new Color(1f, 1f, .3f, 1));
				AXGUIColors.Add("Spline", new Color(.8f, .5f, .9f, 1));
				AXGUIColors.Add("Shape", new Color(.6f, .3f, .7f, 1));
				AXGUIColors.Add("Mesh", new Color(.5f, .9f, .5f, 1));
				AXGUIColors.Add("Plane", new Color(1.0f, .7f, .1f, 1));
				AXGUIColors.Add("Data", new Color(0.99f, .9f, .1f, 1));
				AXGUIColors.Add("MaterialTool", new Color(0.99f, .9f, .1f, 1));
				AXGUIColors.Add("RepeaterTool", new Color(0.99f, .9f, .1f, 1));
				AXGUIColors.Add("JitterTool", new Color(0.99f, .7f, .1f, 1));


				AXGUIColors.Add("NameColor", new Color(0.75f, .7f, 1f, 1));

				AXGUIColors.Add("GrayText", new Color(0.8f, 0.8f, .8f, .85f));

				defaultDataColor = new Color(.8f, .8f, .8f, 1);

				//AXGUIColors.Add("Default",  				new Color(.8f,.8f,.8f,1));	




			}
			else
			{
				//AXGUIColors.Add("GridColor", 				new Color(0.95f, 0.95f, 1f, .5f));
				//AXGUIColors.Add("AxisColor", 				new Color(0.5f, 0.33f, .33f, .15f));

				//AXGUIColors.Add("GridColor", 				new Color(0.65f, 0.65f, .75f, .1f));
				AXGUIColors.Add("GridColor", new Color(0.45f, 0.45f, .55f, .1f));

				// AXGUIColors.Add("AxisColor", 				new Color(1f, 0.53f, .53f, .995f));
				AXGUIColors.Add("AxisColor", new Color(.9f, 0.33f, .33f, .995f));

				AXGUIColors.Add("ConnectorShadowColor", new Color(.2f, .2f, .2f, .6f));
				AXGUIColors.Add("ConnectorColor", new Color(0.6f, .3f, .35f, 1.0f));

				//				AXGUIColors.Add("NodePaletteBG", 			new Color(.8f, .85f, .9f, 1));
				//				AXGUIColors.Add("NodePaletteHighlight", 	new Color( .93f,  .98f,  1f, 1f));
				//				AXGUIColors.Add("NodePaletteHighlightRect", new Color(.1f, .1f,  1f, 1f));
				AXGUIColors.Add("NodePaletteBG", new Color(.74f, .75f, .8f, 1));
				AXGUIColors.Add("NodePaletteHighlight", new Color(.9f, .9f, .9f, 1f));
				AXGUIColors.Add("NodePaletteHighlightRect", new Color(.5f, .5f, .7f, 1f));


				//				AXGUIColors.Add("Float", 					new Color(1.0f,.6f,.5f,1));	
				//				AXGUIColors.Add("Int",  					new Color(.8f,.9f,.9f,1));
				//				AXGUIColors.Add("Bool", 					new Color(1f,1f,.6f,1));
				//				AXGUIColors.Add("Spline",  					new Color(.8f,.65f,.9f,1));
				//				AXGUIColors.Add("Shape",  					new Color(.8f,.65f,.9f,1));
				//				AXGUIColors.Add("Mesh",  					new Color(.5f,.9f,.5f,1));
				//				AXGUIColors.Add("Plane",  					new Color(.9f,.9f,.5f,1));
				//				AXGUIColors.Add("Data",  					new Color(0.99f,.9f,.1f, 1));
				//				AXGUIColors.Add("MaterialTool",  			new Color(0.99f,.9f,.6f, 1));
				//				AXGUIColors.Add("RepeaterTool",  			new Color(0.99f,.9f,.6f, 1));
				//				AXGUIColors.Add("JitterTool",  				new Color(0.99f,.9f,.6f, 1));	

				AXGUIColors.Add("Float", new Color(1.0f, .6f, .5f, 1));
				AXGUIColors.Add("Int", new Color(.8f, .9f, .9f, 1));
				AXGUIColors.Add("Bool", new Color(1f, 1f, .6f, 1));
				AXGUIColors.Add("Spline", new Color(.7f, .55f, .8f, 1));
				AXGUIColors.Add("Shape", new Color(.6f, .45f, .7f, 1));
				AXGUIColors.Add("Mesh", new Color(.5f, .8f, .5f, 1));
				AXGUIColors.Add("Plane", new Color(.9f, .9f, .5f, 1));
				AXGUIColors.Add("Data", new Color(0.99f, .9f, .1f, 1));
				AXGUIColors.Add("MaterialTool", new Color(0.89f, .8f, .5f, 1));
				AXGUIColors.Add("RepeaterTool", new Color(0.89f, .8f, .5f, 1));
				AXGUIColors.Add("JitterTool", new Color(0.89f, .8f, .5f, 1));

				AXGUIColors.Add("NameColor", new Color(0.45f, .1f, .3f, 1));

				//AXGUIColors.Add("GrayText", 				new Color(0.8f, 0.8f, .8f, .85f));
				AXGUIColors.Add("GrayText", new Color(0.1f, 0.1f, .1f, .85f));

				defaultDataColor = new Color(0.89f, .8f, .5f, 1);
				//AXGUIColors.Add("Default",  				new Color(.9f,.7f,.2f,1));
			}
			AXGUIColors.Add("ShapeColor", new Color(.88f, .70f, 1f, 1f));

		}



		// Use this for initialization, check version, etc.
		static ArchimatixEngine()
		{
			version = "";

			// DELEGATES

			//SceneView.onSceneGUIDelegate				-= SceneGUI;
			//SceneView.onSceneGUIDelegate				+= SceneGUI;

#if UNITY_2019_2_OR_NEWER
            SceneView.duringSceneGui += SceneGUI;
#else
			SceneView.onSceneGUIDelegate += SceneGUI;
#endif


			EditorApplication.update -= Update;
			EditorApplication.update += Update;
			//EditorApplication.hierarchyWindowChanged 	+= hierarchyWindowChanged;	
			//EditorApplication.hierarchyWindowItemOnGUI 	+= hierarchyWindowChanged;	
			//EditorApplication.projectWindowChanged += projectWindowChanged;

			Undo.undoRedoPerformed -= OnUndoRedo; // subscribe to the event
			Undo.undoRedoPerformed += OnUndoRedo; // subscribe to the event

			//Selection.selectionChanged 				+= OnSceneSelectionChanged;


			//Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
			//EditorWindow hierarchyWindow = EditorWindow.GetWindow(type);


			// load ui images

			/*
			string[] guids = AssetDatabase.FindAssets ("AXReadMe");
			if(guids!= null && guids.Length >0)
				ArchimatixAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
			*/


			//Debug.Log("ArchimatixAssetPath == " + ArchimatixAssetPath);





		}

		public static void scheduleBuild()
		{
			if (currentModel != null && currentModel.automaticModelRegeneration)
			{
				if (buildStopwatch == null)
					buildStopwatch = new Stopwatch();

				buildStopwatch.Reset();
				buildStopwatch.Start();
			}

		}
		public static void doneBuild()
		{
			if (buildStopwatch == null)
				buildStopwatch = new Stopwatch();

			buildStopwatch.Reset();

		}


		public static void Init()
		{
			//Debug.Log("Init AX");

			buildStopwatch = new Stopwatch();

			// This was bad! Thought it would fix international issues.
			//Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if (currentModel != null)
			{
				currentModel.snapSizeGrid = EditorPrefs.GetFloat("snapSizeGrid");
				if (currentModel.snapSizeGrid == 0)
				{
					currentModel.snapSizeGrid = .25f;
					EditorPrefs.SetFloat("snapSizeGrid", .25f);
				}
			}


			updateCounter = 0;

			establishPaths();
			//Debug.Log(ArchimatixAssetPath+"/Prefabs/RobotKyleBillboard");
			ScalingFigureBillboardPrefab = AssetDatabase.LoadAssetAtPath(ArchimatixAssetPath + "/Prefabs/RobotKyleBillboard.prefab", (typeof(GameObject))) as GameObject;

			//Debug.Log(" ------- ScalingFigureBillboardPrefab="+ScalingFigureBillboardPrefab);

			//ScalingFigureBillboardTransform = ScalingFigureBillboard.GetComponentInChildren<Transform>();


			// ASSERT DEFAULT MATERIAL FOR MODELS IN SCENE
			loadDefaultMaterial();


			NodeGraphEditorType = Type.GetType("AXNodeGraphEditorWindow");
			if (NodeGraphEditorType != null)
			{
				NodeGraphEditorIsInstalled = true;
				RepaintGraphEditorIfOpenMethodInfo = NodeGraphEditorType.GetMethod("repaintIfOpen");
			}


			if (EditorGUIUtility.isProSkin)
			{
				menubarTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenubarDark.png", typeof(Texture2D));

			}
			else
			{
				menubarTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenubarLight.png", typeof(Texture2D));

			}


			// RUNTIME

			//string relativepath = ArchimatixEngine.ArchimatixAssetPath+"/Prefabs/Resources/RuntimeHandlePrefab_PlanarKnob.prefab";
			string relativepath = ArchimatixEngine.ArchimatixAssetPath + "/Scenes Runtime/Resources/RuntimeHandlePrefab_PlanarKnob.prefab";
			//string absolutpath  = ArchimatixUtils.getAbsoluteLibraryPath(relativepath);

			ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob = (GameObject)AssetDatabase.LoadAssetAtPath(relativepath, typeof(GameObject));


			//Debug.Log(" *********** " + ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob);

			if (!Application.isPlaying)
			{

				//createLibrary();
				if (library == null)
					library = Library.loadLibraryMetadata();



				if (library == null)
					createLibraryFromJSONFiles();

			}


			//Debug.Log("Library: " + library);

			if (userMessages == null)
				userMessages = new List<UserMessage>();









		}

		public static void assertRuntimeHandlePrefab_PlanarKnob()
		{
			if (ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob == null)
			{
				string relativepath = ArchimatixEngine.ArchimatixAssetPath + "/Scenes Runtime/Resources/RuntimeHandlePrefab_PlanarKnob.prefab";
				ArchimatixEngine.RuntimeHandlePrefab_PlanarKnob = (GameObject)AssetDatabase.LoadAssetAtPath(relativepath, typeof(GameObject));
			}




		}


		public static void loadDefaultMaterial()
		{
			if (string.IsNullOrEmpty(ArchimatixEngine.ArchimatixAssetPath))
				ArchimatixEngine.establishPaths();

			AXModel.defaultMaterial = (Material)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/Materials/AX_GridPurple.mat", typeof(Material));
			AXModel.defaultPhysicMaterial = (PhysicMaterial)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/Materials/AX_DefaultPhysicMaterial.mat", typeof(PhysicMaterial));

		}


		public static void assertDefaultMaterialForAllModels()
		{
			AXModel[] axModels = GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];
			foreach (AXModel m in axModels)
			{
				if (m != null)
					m.assertDefaultMaterial();
			}
		}


		public static void createLibraryFromJSONFiles()
		{
			//Debug.Log("CreateLibrary");
			library = new AX.Library();
			//if (library.parametricObjects == null)
			library.readLibraryFromFiles();

			if (library != null)
			{
				library.cacheLibraryItems();
				library.sortLibraryItems();
				saveLibrary();
			}

		}

		public static void loadLibrary()
		{

		}

		public static void saveLibrary()
		{

			library.saveLibraryMetadata();
		}




		public static void OnUndoRedo()
		{
			// some code here
			//Debug.Log("Undo currentModel=" + currentModel);

			AXEditorUtilities.clearFocus(); // GUI.FocusControl("dummy_label"); 
			GUIUtility.keyboardControl = 0;

			if (currentModel != null)
			{

				//				foreach (AXParametricObject po in currentModel.parametricObjects)
				//				{
				//					//string grouperName = (po.grouper != null) ? po.grouper.Name : " None";
				//
				//					//Debug.Log(po.Name + " :: " + grouperName + " " + po.grouperKey);
				//
				//				}

				/*
				if (currentModel.currentWorkingGroupPO != null)
				{
					// was a Grouper creation just reverted?

					if (! currentModel.parametricObjects.Contains(currentModel.currentWorkingGroupPO))
					{
						if (currentModel.currentWorkingGroupPO.grouper != null && currentModel.parametricObjects.Contains(currentModel.currentWorkingGroupPO.grouper))
						{
							currentModel.currentWorkingGroupPO = currentModel.currentWorkingGroupPO.grouper;
						}
						else
							currentModel.currentWorkingGroupPO = null;

					}

				}
				*/

				currentModel.cleanGraph();
				currentModel.autobuild();
				EditorUtility.SetDirty(currentModel.gameObject);
			}

		}

		[System.NonSerialized]
		public static AXModel _currentModel;
		public static AXModel currentModel
		{
			get { return _currentModel; }
			set
			{
				//Debug.Log ("SET CURRENT MODEL");
				_currentModel = value;
			}
		}

		public static AXModel getOrMakeCurrentModel()
		{
			if (currentModel == null)
				AXEditorUtilities.createNewModel("AXModel");
			establishPaths();

			return currentModel;
		}


		[System.NonSerialized]
		public static long lastAXGOselectionTime;


		[System.NonSerialized]
		public static bool optionClick;

		[System.NonSerialized]
		public static int workingAxis;



		public static void repaintGraphEditorIfExistsAndOpen()
		{

			if (NodeGraphEditorType != null && RepaintGraphEditorIfOpenMethodInfo != null)
				RepaintGraphEditorIfOpenMethodInfo.Invoke(null, null);


			//AXNodeGraphEditorWindow.repaintIfOpen();
		}

		public static bool graphEditorIsOpen()
		{
			return AXNodeGraphEditorWindow.IsOpen;
			//return false;
		}



		public static void hierarchyWindowChanged(int i, Rect r)
		{
			Debug.Log("hierachy");


		}
		public static void projectWindowChanged()
		{
			Debug.Log("project window changed");


		}

		public static void createScalingFigure()
		{



			if (ScalingFigureBillboardPrefab == null)
				ScalingFigureBillboardPrefab = AssetDatabase.LoadAssetAtPath(ArchimatixAssetPath + "/Prefabs/RobotKyleBillboard.prefab", (typeof(GameObject))) as GameObject;


			if (ScalingFigureBillboard == null && ScalingFigureBillboardPrefab != null)
				ScalingFigureBillboard = GameObject.Instantiate(ScalingFigureBillboardPrefab);

			if (ScalingFigureBillboard != null)
			{
				ScalingFigureBillboard.name = "RobotKyleBillboard";

				ScalingFigureBillboardTransform = ScalingFigureBillboard.GetComponentInChildren<Transform>();

				ScalingFigureBillboardTransform.Translate(-2, 0, 2);
			}
			//Debug.Log("ScalingFigureBillboardPrefab="+ScalingFigureBillboardTransform.parent.name);
		}

		static void checkScalingFigureBillboard()
		{
			ScalingFigureBillboard = GameObject.Find("RobotKyleBillboard");
			lookForScalingBillboard = false;
		}


		public static void setSceneViewState(SceneViewState svs)
		{
			if (svs == SceneViewState.Default && sceneViewState == SceneViewState.AddPoint)
			{
				// JUST FINISHED ADDING FIRST POINTS TO A CURVE
				if (currentModel != null && currentModel.activeFreeCurves.Count == 1 && currentModel.activeFreeCurves[0] != null)
				{
					//FreeCurve gener = (FreeCurve) currentModel.activeFreeCurves[0].generator;
					//gener.assertCCW();
				}

			}

			sceneViewState = svs;
		}


		public static bool isModel(GameObject go)
		{

			if (go != null && go.GetComponent<AXModel>())
				return true;

			return false;

		}



		// Update is called once per frame
		// There is not much computation going on here, 
		// just a check to see if the currently selected GameObject has an AXGameObject attached.
		// If it does, then this is a special selection event.
		static void Update()
		{


			if (updateCounter++ > 500)
				updateCounter = 0;

			if (buildStopwatch == null)
				buildStopwatch = new Stopwatch();

			if (buildStopwatch.ElapsedMilliseconds > 800)
			{
				if (currentModel != null)
					currentModel.autobuild();

				doneBuild();

				if (SceneView.lastActiveSceneView != null)
					SceneView.lastActiveSceneView.Repaint();

			}


			if (library == null && !Application.isPlaying)
			{
				ArchimatixEngine.Init();
			}

			//Debug.Log(library.allLibraryThumbnailsLoaded);

			// Make sure library thumbnails (icons) are loaded. 
			if (!Application.isPlaying && library != null && !library.allLibraryThumbnailsLoaded && updateCounter == 499)
			{
				//Debug.Log("Loading Library thumbnails...");
				library.loadThumnails();
			}

			//Debug.Log("------------------------------ Update");

			if (currentModel != null && !Application.isPlaying)
			{
				currentModel.canRegenerate = true;

				//if (currentModel.isDirty)
				//currentModel.generateIfDirty();

			}




			//Debug.Log(Selection.activeGameObject);
			// DETECT IF THERE HAS BEEN A VERSION CHANGE
			//if (string.IsNullOrEmpty(EditorPrefs.GetString(ArchimatixEngine.identifier)) ||  EditorPrefs.GetString(ArchimatixEngine.identifier) != AXStartupWindow.GetVersion())

			if (!string.IsNullOrEmpty(version))
			{
				if (string.IsNullOrEmpty(EditorPrefs.GetString(ArchimatixEngine.identifier)) || EditorPrefs.GetString(ArchimatixEngine.identifier) != version)
				{
					AXStartupWindow.open();
					EditorPrefs.SetString(ArchimatixEngine.identifier, version);
					ArchimatixEngine.createLibraryFromJSONFiles();
				}
			}



			GameObject currentGO = Selection.activeGameObject;

			if (currentGO != null)
			{
				AXModel currentGOModel = currentGO.GetComponent<AXModel>();

				// Ok, the currently selected GO has a model. 
				if (currentGOModel != null && (currentModel == null || currentGO != currentModel.gameObject))
				{
					// model change!!!! Probably from heirarchy window...
					currentModel = currentGOModel;

				}
			}
			else
			{
				// no AXModel selected
				currentModel = null;

			}



			if (lookForScalingBillboard)
				checkScalingFigureBillboard();

			//Debug.Log("currentModel.robotKyleBillboardGO="+currentModel.robotKyleBillboardGO);


			//Debug.Log("="+RenderSettings.ambientIntensity + ", " + RenderSettings.ambientLight + ", " + RenderSettings.ambientMode);
			workingAxis = (int)Axis.NONE;

			// if an axGameObject is selected, hide tools 
			bool axGameObjectIsSelected = false;





			// CUSTOM SELECTION SYSTEM -- for hierarchy selection
			// Since we can't get events from the Hiearchy View, 
			// Try to detect if a new AWGameObject was select in the Hiearchy
			// by seeing if the last and previous AWGO's selected are different.

			if (Selection.activeGameObject != null)
			{



				AXGameObject axgo = Selection.activeGameObject.GetComponent<AXGameObject>();



				// Was it a GameObject with an AXGameObject attached?
				if (axgo != null && Selection.activeGameObject.transform.parent != null)
				{
					// Yes, an AXGameObject was just selected a moment ago.

					// This is an "event" because
					// the ax model GameObject does not have an AXGameObject attached.

					// Remember that, when an GameObject is selected in SceneView,
					// the Selection.activeGameObject is always the model.
					// If we got here and the model is not the Active object, and the active Object is an AXGO, 
					// then it must have been selected in the SceneView
					//Debug.Log("CARCAR "+axgo);

					axGameObjectIsSelected = true;

					// Make sure the parent model of this AXGO becomes the currentModel for graph display, etc.
					if (axgo.model != null)
						currentModel = axgo.model;

					if (currentModel != null)
						currentModel.selectedPOs.Clear();


					axgo.OnSelect();

					if (currentModel != null)
					{
						Selection.activeGameObject = currentModel.gameObject;

						EditorApplication.RepaintHierarchyWindow();
						SceneView.RepaintAll();
						EditorUtility.SetDirty(currentModel);
					}

				}


			}


			if ((axGameObjectIsSelected || isModel(Selection.activeGameObject)) && (currentModel != null && currentModel.selectedPOs != null && currentModel.selectedPOs.Count > 0))
			{
				// SOMETHING OTHER GAME_OBJECT WAS SELECTED (i.e., not AXModel and note AXGameObject

				UnityEditor.Tools.hidden = true;
			}
			else
				UnityEditor.Tools.hidden = false;



			/*
			if (sceneViewClickRegisteredAt > 0)
			{
				if ( (StopWatch.now() - sceneViewClickRegisteredAt) > 50 )
					executeSceneViewClick();

			}
			*/
			if (currentModel != null && currentModel.renderMode == AX.AXModel.RenderMode.DrawMesh)
				EditorUtility.SetDirty(currentModel.gameObject);
			meshBufferJustClearedOnUpdate = true;




			if (createControllerForModel != null)
			{
				AXModel m = createControllerForModel;
				createControllerForModel = null;
				AXRuntimeEditor.createControllerButtonAction(m);

			}

		}








		// SCENE_GUI
		//
		//----------------------------------------------------------------------------

		public static void SceneGUI(SceneView sceneView)
		{


			Event e = Event.current;

			//			if (e.type != EventType.Layout && e.type != EventType.Repaint && e.type != EventType.MouseMove)
			//				Debug.Log("sceneGUI: " + e.type + " " + e.keyCode);

			shiftIsDown = e.shift;

			if (currentModel != null && e.type == EventType.Layout && meshBufferJustClearedOnUpdate)
			{
				//Debug.Log("SceneGUI: " + currentModel.axRenderMeshCount);

				if (!Application.isPlaying)
					currentModel.drawMeshesInScene();

				meshBufferJustClearedOnUpdate = false;
			}

			if (e.type == EventType.Repaint)
			{


				//Debug.Log("OnSceneView Repaint ==================================");
				if (currentModel != null && !Application.isPlaying)
				{
					currentModel.canRegenerate = true;

					//if (currentModel.isDirty)
					//currentModel.generateIfDirty();
				}

			}
			// !!! 
			// Take control of events in SceneView - VOODOO?
			//Debug.Log("selectedFreeCurve.hasSelectedPoints()="+selectedFreeCurve.hasSelectedPoints());
			//if (selectedFreeCurve != null && (selectedFreeCurve.hasSelectedPoints() || sceneViewState == SceneViewState.AddPoint) )


			//if (currentModel != null && currentModel.activeFreeCurves.Count > 0 || sceneViewState == SceneViewState.AddPoint) 
			//{

			if (e.type == EventType.Layout && currentModel != null)
			{

				HandleUtility.AddDefaultControl(0);
			}
			//}




			// CUSTOM CLICK-SELECTION LOGIC
			// Since each click on an AX GameObject selects the model (as far as the scene is concerned)
			// and then internally selects an GameObject instance stored via its AWGameObject,
			// we can't use the unity engine selection event.


			//Debug.Log(e.type);
			//int controlIDD = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive); // Save editor click data first thing

			// If there is an active AWGameObject (after mouse up on an object in the scene, switch to model

			if (e.type != EventType.MouseDown && currentModel != null && currentModel.selectedPOs != null && currentModel.selectedPOs.Count > 0 && Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<AXGameObject>() != null)
				Selection.activeGameObject = currentModel.gameObject;

			bool mouseJustDown = false;



			//Debug.Log("GUIUtility.hotControl="+GUIUtility.hotControl);

			//Debug.Log("Here " + currentModel) ;
			// DO BUTTONS FIRST
			if (currentModel != null)
			{
				//AXParametricObject currentPO = null;

				//if (currentModel.selectedPOs != null && currentModel.selectedPOs.Count == 1 &&  currentModel.selectedPOs[0] != null)
				//	currentPO = currentModel.selectedPOs[0];





				// FREE_CURVES: Process OnSceneGUI to support controlPoint editing
				if (currentModel.activeFreeCurves != null && currentModel.activeFreeCurves.Count == 1)
				{

					if (currentModel.activeFreeCurves[0].generator is FreeCurve3D)
					{
						FreeCurve3DHandler gh = (FreeCurve3DHandler)GeneratorHandler.getGeneratorHandler(currentModel.activeFreeCurves[0]);
						gh.OnSceneGUI();
					}
					else
					{
						if (e.keyCode == KeyCode.Backspace)
						{
							FreeCurve gener = (FreeCurve)currentModel.activeFreeCurves[0].generator;

							//Debug.Log("gener.hasSelectedPoints()="+gener.hasSelectedPoints());
							if (gener.hasSelectedPoints())
							{
								//Debug.Log("delete");
								gener.DeleteSelected();
								gener.generate(false, null, false);
								// SCENEVIEW
								if (SceneView.lastActiveSceneView != null)
									SceneView.lastActiveSceneView.Repaint();
							}
						}
						FreeCurveHandler gh = (FreeCurveHandler)GeneratorHandler.getGeneratorHandler(currentModel.activeFreeCurves[0]);
						gh.OnSceneGUI();
					}

				}






				Handles.BeginGUI();

				/*
			if (! AXNodeGraphEditorWindow.IsOpen)
			{
				if (GUI.Button(new Rect(10, 10, 80, 20), "Graph Editor"))
				{
					Debug.Log("Pressed");
					EditorWindow.GetWindow(typeof(AXNodeGraphEditorWindow));
					currentModel.isAltered(5);
					e.Use ();
				}
			}
			*/








				// 2D GUI

				GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
				RectOffset prevButtonMargin = buttonStyle.margin;

				buttonStyle.alignment = TextAnchor.MiddleLeft;
				buttonStyle.margin = new RectOffset(0, 0, -1, 0);
				buttonStyle.fontSize = 12;

				float prevFixedWidth = buttonStyle.fixedWidth;
				float prevFixedHeight = buttonStyle.fixedHeight;

				buttonStyle.fixedWidth = 60;
				//buttonStyle.fixedHeight = 18;

				//GUIStyle areaStyle = new GUIStyle();
				//areaStyle. = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));

				GUIStyle labelStyle = GUI.skin.GetStyle("Label");
				labelStyle.alignment = TextAnchor.MiddleLeft;
				labelStyle.fontSize = 20;

				GUIStyle paramLabelStyle = GUI.skin.GetStyle("Label");
				paramLabelStyle.alignment = TextAnchor.MiddleLeft;
				paramLabelStyle.fontSize = 18;

				GUIStyle boxStyle = GUI.skin.GetStyle("Box");
				boxStyle.alignment = TextAnchor.UpperLeft;
				boxStyle.normal.textColor = Color.white;



				// FOOTER GUI
				//SceneView.lastActiveSceneView.position

				//float windowWidth 	= (Application.platform == RuntimePlatform.OSXEditor) ? Screen.width/2 : Screen.width;
				//float windowHeight 	= (Application.platform == RuntimePlatform.OSXEditor) ? Screen.height/2 : Screen.height;

				float windowWidth = (SceneView.lastActiveSceneView != null) ? SceneView.lastActiveSceneView.position.width : Screen.width;
				float windowHeight = (SceneView.lastActiveSceneView != null) ? SceneView.lastActiveSceneView.position.height : Screen.height;



				float panelWidth = Mathf.Min(windowWidth, 530);
				float panelHeight = 43;

				Rect footerBoxRect = new Rect(windowWidth / 2 - (panelWidth + 10) / 2, windowHeight - panelHeight, panelWidth + 20, panelHeight);
				Rect footerRect = new Rect(windowWidth / 2 - panelWidth / 2, windowHeight - panelHeight + 2, panelWidth, 22);




				if (mouseIsDownOnHandle)
				{
					if (currentModel.latestHandleClicked != null)
					{

						AXHandle han = currentModel.latestHandleClicked;

						if (han.expressions.Count == 1)
						{
							string exp = han.expressions[0];
							string pName = exp.Substring(0, exp.IndexOf("="));
							AXParameter p = han.parametricObject.getParameter(pName);

							if (p != null)
								GUI.Label(new Rect(windowWidth / 2 - 100, windowHeight - 100, 220, 20), han.Name + ": " + p.FloatVal, labelStyle);
						}
						else
						{
							float y = windowHeight - 200;
							GUI.Label(new Rect(windowWidth / 2 - 100, y, 220, 20), han.Name, labelStyle);

							y += 5;
							foreach (string exp in han.expressions)
							{
								y += 20;
								string pName = exp.Substring(0, exp.IndexOf("="));
								AXParameter p = han.parametricObject.getParameter(pName);
								GUI.Label(new Rect(windowWidth / 2 - 100, y, 220, 20), p.Name + ": " + p.FloatVal, paramLabelStyle);

							}
						}
					}

					else if (currentModel.latestEditedParameter != null)
					{
						string val = "" + currentModel.latestEditedParameter.FloatVal.ToString("0.00");
						if (currentModel.latestEditedParameter.Type == AXParameter.DataType.Int)
							val = "" + currentModel.latestEditedParameter.IntVal;
						GUI.Label(new Rect(windowWidth / 2 - 100, windowHeight - 100, 220, 20), currentModel.latestEditedParameter.parametricObject.Name + "." + currentModel.latestEditedParameter.Name + ": " + val, labelStyle);

					}
					//GUI.Label(new Rect(windowWidth / 2 - 100, windowHeight - 100, 220, 20), currentModel.latestEditedParameter.parametricObject.Name + "." + currentModel.latestEditedParameter.Name + ": " + val, labelStyle);
					// GUI.Label(new Rect(windowWidth / 2 - 100, windowHeight - 150, 220, 20), currentModel.latestHandleClicked. + ": " + val, labelStyle);



				}


				labelStyle.fontSize = 12;



				GUI.Box(footerBoxRect, "");
				GUILayout.BeginArea(footerRect);


				buttonStyle.margin = prevButtonMargin;



				EditorGUILayout.BeginHorizontal();





				// SNAPPING TOGGLE
				EditorGUIUtility.labelWidth = 33;
				ArchimatixEngine.snappingIsOn = EditorPrefs.GetBool("snappingIsOn");

				EditorGUI.BeginChangeCheck();
				ArchimatixEngine.snappingIsOn = EditorGUILayout.Toggle("Snap", ArchimatixEngine.snappingIsOn);
				if (EditorGUI.EndChangeCheck())
				{
					EditorPrefs.SetBool("snappingIsOn", ArchimatixEngine.snappingIsOn);
				}


				GUILayout.Space(8);





				// GRID_SIZE 


				GUIStyle style = new GUIStyle(GUI.skin.textField);
				style.active.textColor = Color.white;

				//GUI.skin.settings.selectionColor = new Color(.2f,.2f,.3f);
				//GUI.skin.settings.cursorColor = Color.white;

				EditorGUIUtility.labelWidth = 31;
				float snapSizeGrid = EditorPrefs.GetFloat("snapSizeGrid");
				if (snapSizeGrid < .01f)
					snapSizeGrid = .01f;
				//currentModel.snapSizeGrid = EditorGUILayout.FloatField("Grid", currentModel.snapSizeGrid, style, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(15) });
				EditorGUI.BeginChangeCheck();
				snapSizeGrid = EditorGUILayout.FloatField("Grid", snapSizeGrid, style, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(15) });
				if (EditorGUI.EndChangeCheck())
				{
					if (snapSizeGrid < .01f)
						snapSizeGrid = .01f;
					EditorPrefs.SetFloat("snapSizeGrid", snapSizeGrid);
					currentModel.snapSizeGrid = snapSizeGrid;

				}

				GUILayout.Space(8);
				// GUIDES TOGGLE
				EditorGUIUtility.labelWidth = 63;
				EditorGUI.BeginChangeCheck();
				ArchimatixEngine.drawGuides = EditorGUILayout.Toggle("Unselected", ArchimatixEngine.drawGuides);
				if (EditorGUI.EndChangeCheck())
				{
					//currentModel.autobuild();
				}


				GUILayout.Space(10);


				// KYLE
				EditorGUIUtility.labelWidth = 33;



				EditorGUI.BeginChangeCheck();
				ArchimatixEngine.useKyle = EditorGUILayout.Toggle("Kyle", ArchimatixEngine.useKyle);

				if (EditorGUI.EndChangeCheck())
				{
					if (ScalingFigureBillboard != null)
					{
						ScalingFigureBillboard.SetActive(useKyle);

					}
					else if (useKyle)
					{
						createScalingFigure();
					}

				}


				if (currentModel != null)
				{
					if (GUILayout.Button("Material"))
					{
						windowPickerID = EditorGUIUtility.GetControlID(FocusType.Passive);
						EditorGUIUtility.ShowObjectPicker<Material>(null, false, "", windowPickerID);
					}
				}

				if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == windowPickerID)
				{

					currentModel.axMat.mat = (Material)EditorGUIUtility.GetObjectPickerObject();
					windowPickerID = -1;

					currentModel.remapMaterialTools();
					currentModel.autobuild();

				}



				GUILayout.FlexibleSpace();

				// STAMP BUTTON
				if (GUILayout.Button("Stamp"))
				{
					currentModel.stamp();
					e.Use();
				}
				if (GUILayout.Button("Prefab"))
				{
					string startDir = Application.dataPath;

					string path = EditorUtility.SaveFilePanel(
						"Save Prefab",
						startDir,
						("" + currentModel.name),
						"prefab");

					if (!string.IsNullOrEmpty(path))
						AXPrefabWindow.makePrefab(currentModel, path, null);
				}

				// Material


				//				if (currentModel != null)// &&  e.type == EventType.Layout)
				//				{ 
				//					GUILayout.FlexibleSpace();
				//
				//					Debug.Log("currentModel="+currentModel);
				//
				//
				//					if (GUILayout.Button("Material"))
				//					{
				//					int controlId = EditorGUIUtility.GetControlID(FocusType.Passive);
				//
				//						EditorGUIUtility.ShowObjectPicker<Material>(null, false, "", controlId);
				//
				//					}
				//
				//
				////
				////					EditorGUI.BeginChangeCheck ();
				////
				////
				////					currentModel.axMat.mat = (Material)EditorGUILayout.ObjectField (currentModel.axMat.mat, typeof(Material), true);
				////					if (EditorGUI.EndChangeCheck ()) {
				////						Undo.RegisterCompleteObjectUndo (currentModel, "Default material for " + currentModel.name);
				////						currentModel.remapMaterialTools ();
				////						currentModel.autobuild ();
				////
				////					}
				//				}




				EditorGUILayout.EndHorizontal();

				GUILayout.EndArea();

				buttonStyle.fixedWidth = prevFixedWidth;
				buttonStyle.fixedHeight = prevFixedHeight;





				Handles.EndGUI();



				// Capture click in SceneView footer
				if (e.type == EventType.MouseDown && footerBoxRect.Contains(e.mousePosition))
				{

					e.Use();
				}

			}



			switch (e.type)
			{

				case EventType.ValidateCommand:

					e.Use();
					break;



				case EventType.ExecuteCommand:

					if (currentModel != null)
						AXEditorUtilities.processEventCommand(e, currentModel);

					break;


				case EventType.KeyDown:

					//if ( (currentModel != null && currentModel.selectedPOs != null && currentModel.selectedPOs.Count > 0) && (e.command || e.control))
					if ((currentModel != null && currentModel.selectedPOs != null && currentModel.selectedPOs.Count > 0) && (e.command || e.control))
						AXEditorUtilities.processEventCommandKeyDown(e, currentModel);
					break;


				case EventType.MouseUp:

					ArchimatixEngine.mouseIsDownOnHandle = false;


					ArchimatixEngine.isPseudoDraggingSelectedPoint = -1;
					ArchimatixEngine.draggingNewPointAt = -1;


					break;

				case EventType.MouseDown:
					//bDebug.Log ("MOUSE DOWN -- GUIUtility.hotControl="+GUIUtility.hotControl + " ---> " + GUI.GetNameOfFocusedControl());

					mouseJustDown = true;


					if (e.alt)
						optionClick = true;

					//Debug.Log("hc="+GUIUtility.hotControl + " ... " + mouseIsDownOnHandle);


					//if (GUIUtility.hotControl != 0 || (currentModel != null && currentModel.activeFreeCurves != null && currentModel.activeFreeCurves.Count > 0))
					//	{
					// We are not on a handle!
					// This is a click that did not land on a SceneView Handle.
					// See if it hit an object and if that object is an AXGameObject
					//Debug.Log("hotcontrol != 0..... " + GUI.GetNameOfFocusedControl());
					clickedObject = null;

					RaycastHit hit;
					if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hit))
						clickedObject = hit.collider.gameObject;


					int nearId = HandleUtility.nearestControl;

					//Debug.Log("Ya Ya A clickedObject="+clickedObject + "; nearId="+nearId);


					if (nearId == 0)
						if (!optionClick && sceneViewState != SceneViewState.AddPoint && Event.current.button == 0)
						{
							//Debug.Log("Ya Ya B");
							//registerScenViewClick(clickedObject);
							executeSceneViewClick();

						}


					/*
					if(clickedObject == null)
					{	
						// A click into the void!

						registerVoidClick();

						clickedInVoid = true;
					}
					else
					{
						OnSceneView_ColliderClicked(clickedObject);
						e.Use ();
					}
					*/

					SceneView sv = SceneView.lastActiveSceneView;
					if (sv != null)
						sv.Repaint();


					//}



					break;  // mouseDown



			} // end switch

			switch (e.rawType)
			{
				case EventType.MouseUp:
					// mouse up is only called when mousing up from a handle...not sure why.
					//Debug.Log ("MOUSE UP -- GUIUtility.hotControl="+GUIUtility.hotControl);
					mouseIsDownOnHandle = false;
					isPseudoDraggingSelectedPoint = -1;




					optionClick = false;

					if (currentModel != null && currentModel.buildStatus == AXModel.BuildStatus.Generated)
					{
						currentModel.renderMode = AXModel.RenderMode.GameObjects;

						if (currentModel.buildStatus == AXModel.BuildStatus.Generated)
							currentModel.build();

					}

					//Debug.Log("MOUSE UP");

					break; // end OnMouseUp

			}





			// DRAW HANDLES
			// --------------------------------------------------------------------------------------------------
			// Draw any 2d shapes that are not selected
			//if (currentModel != null && currentModel.selectedPOs != null && currentModel.selectedPOs.Count > 0)



			if (currentModel != null)
			{
				// Unselected


				foreach (AXParametricObject po in currentModel.parametricObjects)
				{

					if (po.is2D())
					{

						//if (po.getConsumer() == null)
						//{ 
						GeneratorHandler gh = GeneratorHandler.getGeneratorHandler(po);
						if (gh != null)
							gh.drawHandlesUnselected();
						//}
					}
				}



				Handles.color = Color.red;

				// DRAW PO HANDLES
				//Debug.Log("currentModel.selectedPOs.Count="+currentModel.selectedPOs.Count);
				//Debug.Log("============================");
				for (int i = 0; i < currentModel.selectedPOs.Count; i++)
				{
					AXParametricObject po = currentModel.selectedPOs[i];
					if (po.GeneratorTypeString == "Turtle" || po.GeneratorTypeString == "Shape")
					{
						Handles.color = Color.magenta;
					}
					po.codeWarning = "";

					//Debug.Log("* Draw " + po.Name);
					GeneratorHandler gh = GeneratorHandler.getGeneratorHandler(po);
					if (gh != null)
						gh.drawHandles();
				}




				//if (e.type == EventType.MouseDown)


				// use the selected PO's of this model and 
				// make handles for any of its parameters that have been designated as handlable 



				if (GUI.changed)
				{
					if (mouseJustDown)
					{
						//Debug.Log ("ModelInspector::mouseJustDown Register Undo");
						mouseJustDown = false;

						//Undo.RegisterCompleteObjectUndo (currentModel, "Scene Handle Drag");

						currentModel.setRenderMode(AXModel.RenderMode.DrawMesh);

					}
					//model.generate(); <-- NOW DONE BEFORE ANY CALL TO Parameter::intiateRipple_setFloatValueFromGUIChange

					//EditorUtility.SetDirty (target);
				}

			}



		}



		public static long sceneViewClickRegisteredAt = 0;
		public static GameObject clickedObject;

		public static void registerScenViewClick(GameObject go = null)
		{
			sceneViewClickRegisteredAt = StopWatch.now();

			if (go != null)
				clickedObject = go;

		}


		public static void mouseDownOnSceneViewHandle()
		{
			//haltSceneViewClick();
			ArchimatixEngine.mouseIsDownOnHandle = true;
		}

		public static void haltSceneViewClick()
		{
			sceneViewClickRegisteredAt = 0;
		}

		public static void executeSceneViewClick()
		{
			sceneViewClickRegisteredAt = 0;





			if (clickedObject == null)
			{
				// DE-SELECT SOMETHING..

				//FreeCurve selectedFreeCurve = null;

				// Later... go through list of displayed FreeCurves (either selected or upstream of a selected po).
				if (currentModel != null)
				{
					//if ( currentModel.selectedPOs != null && currentModel.selectedPOs.Count == 1 &&  currentModel.selectedPOs[0] != null && currentModel.selectedPOs[0].generator != null &&  currentModel.selectedPOs[0].generator is FreeCurve)
					//selectedFreeCurve = (FreeCurve) currentModel.selectedPOs[0].generator;
				}


				//if (selectedFreeCurve != null && selectedFreeCurve.hasSelectedPoints())
				if (currentModel != null && currentModel.activeFreeCurves.Count > 0)
				{
					//Debug.Log("DESEL ALL");

					bool foundSelectedPoints = false;

					for (int i = 0; i < currentModel.activeFreeCurves.Count; i++)
					{
						Generator gener = currentModel.activeFreeCurves[i].generator;
						if (gener.hasSelectedPoints())
						{
							foundSelectedPoints = true;
							gener.deslectAllItems();
						}

					}

					if (!foundSelectedPoints)
					{
						if (currentModel != null)
							currentModel.deselectAll();
						currentModel = null;
						Selection.activeGameObject = null;
						mouseIsDownOnHandle = false;

					}
				}
				else
				{
					if (currentModel != null)
						currentModel.deselectAll();
					currentModel = null;
					Selection.activeGameObject = null;
					mouseIsDownOnHandle = false;
				}
			}
			else
			{

				OnSceneView_ColliderClicked(clickedObject);

			}
			SceneView sv = SceneView.lastActiveSceneView;
			if (sv != null)
				sv.Repaint();

		}



		// ** CUSTOM SELECTION SYSTEM ** SCENE_VIEW SELECTION
		// 		In SceneView, support Cycle-Select when clicking on a single object.
		// ---------------------------------------------------------------------------------------------
		public static void OnSceneView_ColliderClicked(GameObject clickedObject)
		{
			// we did hit a collider. Is this a AXGameObject? Or some other scene object?
			AXGameObject axgo = clickedObject.GetComponent<AXGameObject>();


			if (axgo != null)
			{
				// Ahoy there, we hit an AWGameObject!
				// This may be a new AXGO, or a repeat(cycle) click on on already selected

				//Debug.Log(axgo.parametricObject.Name);

				// Boiler plate
				currentModel = axgo.model;



				if (currentModel != null && currentModel.selectedPOs != null)
				{
					Selection.activeGameObject = currentModel.gameObject;
					currentModel.selectedPOs.Clear();
				}



				// Is there a contectual click for a menu?


				//********************************************************************************************************
				// CYCLE-SELECT
				//********************************************************************************************************
				//Debug.Log("Cycle select A " + currentModel.clickSelectedAXGO + " == "  + axgo);

				// is this a repeat click on the same AWGameOnject?
				if (currentModel != null && currentModel.clickSelectedAXGO != null && currentModel.clickSelectedAXGO == axgo)
				{
					//Debug.Log("Cycle select B " + currentModel.clickSelectedAXGO.parametricObject.Name);
					// repeat click on this object - cycle select...
					if (Event.current == null || !Event.current.control)
					{
						Transform t = currentModel.nextAncestor();
						//Debug.Log("t="+t);
						if (t != null)
						{
							//Debug.Log("t="+t.gameObject.name);
							currentModel.cycleSelectedAXGO = t.gameObject.GetComponent<AXGameObject>();

							// SKIP OVER NULL GAME_OBJECTS
							// If the parent was made by the same PO as the current, then 
							// then it is probably an intermediate "Null" GameObject generated as a group parent
							// such sa with the parts of an extrude from a AXClipperLib.PolyTree plan.
							// Keep going up the transforms (downstream in PO's) until you hit a new PO...
							int govenor = 0;
							while (currentModel.cycleSelectedAXGO.parametricObject == currentModel.cycleSelectedPO)
							{
								if (govenor++ > 10)
									break;
								t = currentModel.nextAncestor();

								if (t != null)
									currentModel.cycleSelectedAXGO = t.gameObject.GetComponent<AXGameObject>();
							}



							currentModel.cycleSelectedPO = currentModel.cycleSelectedAXGO.parametricObject;

							//currentModel.selectAndPanToPO(currentModel.cycleSelectedAXGO.parametricObject);
							currentModel.selectPO(currentModel.cycleSelectedAXGO.parametricObject);

						}
						else
						{
							//Debug.Log("yo");
							currentModel.clickSelectedAXGO = null;
							currentModel.cycleSelectedPO = null;



							// ADDED 2021_04_11
							// New Behavior: first handles already showing when object is first selected
							axgo.OnSelect();
						}

						lastAXGOselectionTime = StopWatch.now();


					}

				}
				else
				{
					//Debug.Log("ak");
					axgo.OnSelect();

					//Debug.Log("First click: " + currentModel.clickSelectedAXGO);
					lastAXGOselectionTime = StopWatch.now();
				}

				/*
				if (Event.current.control && currentModel != null)
				{
					AXParameter p = currentModel.cycleSelectedAXGO.parametricObject.generator.P_Output;//getPreferredOutputMeshParameter();
					//MeshOuputMenu.contextMenu(p, Event.current.mousePosition);
				}
				*/

				EditorApplication.RepaintHierarchyWindow();

				if (currentModel != null)
					EditorUtility.SetDirty(currentModel);
			}
			else
			{
				//Debug.Log(" OUCH!!!!");
				currentModel = null;
				Selection.activeGameObject = clickedObject;
			}

		}




		public static void newUserMessage(string message)
		{
			userMessages.Add(new UserMessage(message));

		}


		public static EditorWindow NodeGraphEditorOpenWindow()
		{

			if (NodeGraphEditorIsInstalled && NodeGraphEditorType != null)
			{
				//return (AXNodeGraphEditorWindow) EditorWindow.GetWindow(typeof(AXNodeGraphEditorWindow));

				return EditorWindow.GetWindow(NodeGraphEditorType, false, "Archimatix");

				//return Convert.ChangeType(EditorWindow.GetWindow(NodeGraphEditorType), NodeGraphEditorType);



			}

			return null;
		}

		public static bool snappingOn()
		{

			if ((ArchimatixEngine.snappingIsOn && !Event.current.control) || (!ArchimatixEngine.snappingIsOn && Event.current.control))
				return true;

			return false;
		}

		public static void openLibrary3D()
		{
			openLibrary(3);
		}
		public static void openLibrary2D()
		{
			openLibrary(2);
		}

		public static void openLibrary(int showClass = 3)
		{

			LibraryEditorWindow libwin3D = (LibraryEditorWindow)EditorWindow.GetWindow(typeof(LibraryEditorWindow), false, "AX Library", true);
			LibraryEditorWindow.showClass = showClass;
			libwin3D.autoRepaintOnSceneChange = true;
			libwin3D.setPositionIfNotDocked();

		}

		// FREE FORM SPLINE DRAWING....
		/** calculate the point in 3D where the mouse clicked,
		2
		    using the plane of the "floor" of your building (assuming your 3D arrow
		3
		    is on the floor */
		/*
			Ray rayCameraThroughMouse = editorCamera.ScreenPointToRay( new Vector2(e.mousePosition.x, (editorCamera.pixelHeight - e.mousePosition.y)) );
		
			Plane planeOfFloor = new Plane( myObject.transform.up, myObject.transform.position );          
		
			float distanceAlongRay;
		
			planeOfFloor.Raycast( rayCameraThroughMouse, out distanceAlongRay );
		
			Vector3 worldPointForMousePositon = rayCameraThroughMouse.GetPoint( distanceAlongRay );
		*/




	}


	public class AXAssetPostprocessor : AssetPostprocessor
	{
		void OnPostprocessTexture(Texture2D texture)
		{

		}

		private static bool libraryLoadStarted;

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			//Debug.Log("OnPostprocessAllAssets " + ArchimatixEngine.library);


			if (Application.isPlaying)
				return;

			// in case the Archimatix folder was moved
			ArchimatixEngine.establishPaths();
			ArchimatixEngine.InitColors();



			// UPDATE LIBRARY 
			// OnPostprocessAllAssets may be called multiple types per asst change.

			bool recreateLibrary = false;


			if (ArchimatixEngine.library == null)
			{
				//ArchimatixEngine.library = Library.loadLibraryMetadata();
				ArchimatixEngine.createLibraryFromJSONFiles();
				return;
			}

			//			if (ArchimatixEngine.library == null)
			//				ArchimatixEngine.library = Library.loadLibrary();

			//else // && ! libraryLoadStarted)
			//{


			foreach (string str in importedAssets)
			{
				if (str.Contains(".axobj"))
				{
					string nodeName = System.IO.Path.GetFileNameWithoutExtension(str);

					if (!ArchimatixEngine.library.hasItem(nodeName))
					{
						// some new item was added to Assets, not saved from Editor
						recreateLibrary = true;
					}

				}


			}
			foreach (string str in deletedAssets)
			{
				//Debug.Log("Deleted Asset: " + str);
				if (str.Contains(".axobj"))
				{
					string nodeName = System.IO.Path.GetFileNameWithoutExtension(str);

					if (ArchimatixEngine.library.hasItem(nodeName))
					{
						// Since the library still thinks its there, this must have been 
						// a deleteion of a file from the assts folder.
						recreateLibrary = true;
					}
				}
			}

			foreach (string str in movedAssets)
			{
				//Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
				if (str.Contains(".axobj"))
				{
					recreateLibrary = true;
				}

			}



			if (!recreateLibrary)
			{

				// Check if the number of 
			}


			if (recreateLibrary)
			{
				//ArchimatixEngine.library.readLibraryFromFiles();
				ArchimatixEngine.createLibraryFromJSONFiles();
			}

			//}





			// UPDATE TEXEL DENSITIES IN ALL MATERIAL_TOOLS
			AXModel model = AXEditorUtilities.getSelectedModel();

			if (model != null && model.parametricObjects != null)
			{
				for (int i = 0; i < model.parametricObjects.Count; i++)
				{
					if (model.parametricObjects[i].generator is MaterialTool)
						(model.parametricObjects[i].generator as MaterialTool).setTexelsPerUnit();
				}
			}


			// NODE_GRAPH_EDITOR_WINDOE REPAINT
			ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();




		}

	}


	public class UserMessage
	{
		public DateTime createdate;
		public string message;

		public UserMessage(string m)
		{
			createdate = DateTime.Now;
			message = m;
		}


	}

}
