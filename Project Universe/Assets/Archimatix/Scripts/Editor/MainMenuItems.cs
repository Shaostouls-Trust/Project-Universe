using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;


using AX;
using AXEditor;

public static class MainMenuItems {


	// MENU ITEMS FOR LIBRARY

	[MenuItem("GameObject/3D Object/Archimatix/Wooden Floor Plane")]
	static void WoodenFloorPlane() {
		launch3DLibraryItem("Wooden Floor Plane");

	}
	[MenuItem("GameObject/3D Object/Archimatix/Box")]
	static void Box() {
		launch3DLibraryItem("Box");

	}

	// STAIRS
	[MenuItem("GameObject/3D Object/Archimatix/Stairs/Basic Stair")]
	static void Stairs_Item() {
		launch3DLibraryItem("BasicStair");
	}
	[MenuItem("GameObject/3D Object/Archimatix/Stairs/Stair With Landing")]
	static void StairWithLanding_Item() {
		launch3DLibraryItem("Stair With Landing");
	}
	[MenuItem("GameObject/3D Object/Archimatix/Stairs/Half Landing Staircase")]
	static void HalfLandingStaircase_Item() {
		launch3DLibraryItem("Half Landing Staircase");
	}
	[MenuItem("GameObject/3D Object/Archimatix/Stairs/Colonial Staircase")]
	static void ColonialStaircase_Item() {
		launch3DLibraryItem("ColonialStaircase");
	}
	[MenuItem("GameObject/3D Object/Archimatix/Stairs/Rickety Staircase")]
	static void RicketyStaircase() {
		launch3DLibraryItem("RicketyStaircase");
	}


	[MenuItem("GameObject/3D Object/Archimatix/Furniture/Arts and Crafts Chair (AX)")]
	static void ArtsAndCraftsChair() {
		launch3DLibraryItem("Arts And Crafts Chair");
	}

	[MenuItem("GameObject/3D Object/Archimatix/Props/Column Shaft")]
	static void ColumnShaft() {
		launch3DLibraryItem("Column Shaft");
	}


	[MenuItem("GameObject/2D Object/Archimatix/FreeCurveLathe")]
	static void FreeCurveLathe() {
		AXGrapher.rigUp_FreeCuverLathe();;

	}



	public static void launch3DLibraryItem(string itemName)
	{
		if (ArchimatixEngine.currentModel == null)
			AXEditorUtilities.createNewModel(itemName + " (AX)");
		ArchimatixEngine.establishPaths();




//		AXParametricObject  po = null;
//		po = ArchimatixEngine.library.getFirstItemByName(Regex.Replace(itemName, @"\s+", ""));
//		if (po != null)
//		{	
//			po = Library.instantiateParametricObject (po.readIntoLibraryFromRelativeAXOBJPath);
//			SceneView.FrameLastActiveSceneView();
//		}


		LibraryItem li = ArchimatixEngine.library.getLibraryItem(itemName);

		if (li != null)
		{
			Library.instantiateParametricObject (li.readIntoLibraryFromRelativeAXOBJPath);
		}
		
	}


	// CALLU LIBRARY WINDOW

	[MenuItem("GameObject/3D Object/Archimatix/3D Library")]
	static void Init3DLibrary() {
		LibraryEditorWindow libwin = (LibraryEditorWindow) EditorWindow.GetWindow(typeof(LibraryEditorWindow));
		libwin.titleContent = new GUIContent("AX Library");
		libwin.autoRepaintOnSceneChange = true;
	}




}
