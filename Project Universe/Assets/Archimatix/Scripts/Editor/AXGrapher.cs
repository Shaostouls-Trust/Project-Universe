using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using AXEditor;

public class AXGrapher {


	[MenuItem("Tools/Archimatix2D/New Sprite", false, 0)]
	public static void createShapeGroup()
	{

	}

	[MenuItem("Tools/Archimatix2D/Shapes/Circle")]
	public static void add_Circle_toSelectedShapeGroup()
	{

	}

	[MenuItem("Tools/Archimatix2D/Shapes/Square")]
	public static void add_Square_toSelectedShapeGroup()
	{

	}
	[MenuItem("Tools/Archimatix2D/Shapes/Rectangle")]
	public static void add_Rectangle_toSelectedShapeGroup()
	{

	}

	[MenuItem("Tools/Archimatix2D/Shapes/Rounded Rectangle")]
	public static void add_RoundedRectangle_toSelectedShapeGroup()
	{

	}

	[MenuItem("Tools/Archimatix2D/Shapes/Isoceles Triangle")]
	public static void add_IsocelesTriangle_toSelectedShapeGroup()
	{

	}

	[MenuItem("Tools/Archimatix2D/Shapes/Right Triangle")]
	public static void add_RightTriangle_toSelectedShapeGroup()
	{

	}




	public static void rigUp_FreeCuverLathe()
	{

		AXModel model = ArchimatixEngine.getOrMakeCurrentModel();



		// ADD NEW PO TO MODEL (only this new po is selected after this)
		AXParametricObject freeCurve_po = AXEditorUtilities.addNodeToCurrentModel("FreeCurve", false);
		AXParametricObject lathe_po = AXEditorUtilities.addNodeToCurrentModel("Lathe", false);

//		if (po == null || po.generator == null)
//			return;
		freeCurve_po.setAxis(AXEditorUtilities.getDefaultAxisBasedBasedOnSceneViewOrientation(SceneView.lastActiveSceneView));

		FreeCurve freeCurve_gener = (FreeCurve) freeCurve_po.generator;
		Lathe lathe_gener = (Lathe) lathe_po.generator;




		// connect
		lathe_gener.P_Section.makeDependentOn( freeCurve_gener.P_Output);


		model.cleanGraph();
		model.autobuild();
		EditorUtility.SetDirty( model.gameObject );

		ArchimatixEngine.setSceneViewState(ArchimatixEngine.SceneViewState.AddPoint);

	}



}
