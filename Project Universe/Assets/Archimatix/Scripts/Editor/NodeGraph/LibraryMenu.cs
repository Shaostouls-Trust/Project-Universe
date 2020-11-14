#pragma warning disable

using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using AXEditor;

public class LibraryMenu 
{
	public Vector2 scrollPosition;

	[System.NonSerialized]
	public AX.Library library;

	[System.NonSerialized]
	List<AXParametricObject> po2DList;

	public LibraryMenu()
	{
		
		scrollPosition = Vector2.zero;
		
	}

	public  void display(float imagesize = 64, AXNodeGraphEditorWindow editor = null, string mode = "2D") // mode 
	{
		//Debug.Log("imagesise="+imagesize);
		// called from an OnGUI
		//imagesize = 64;

		Event e = Event.current;

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUIStyle.none);

		//StopWatch sw = new StopWatch();

		EditorGUILayout.BeginVertical();

			 
		if (mode == "2D")
		{


			if (ArchimatixEngine.library != null && ArchimatixEngine.library.filteredResults != null)
			{

				// EACH 2D ITEM
				for (int i = 0; i < ArchimatixEngine.library.filteredResults.Count; i++) {


					

					// ** THE BIG CLICK 2D

					LibraryItem li = ArchimatixEngine.library.filteredResults [i];

					if (! li.includeInSidebarMenu)
						continue;
					if (! li.is2D)
						continue;

					if (GUILayout.Button (new GUIContent (li.icon, li.Name), GUILayout.Width (imagesize), GUILayout.Height (imagesize))) {
						e.Use();


						AXParametricObject prevSelectedPO = null;

						if (ArchimatixEngine.currentModel != null && ArchimatixEngine.currentModel.selectedPOs != null && ArchimatixEngine.currentModel.selectedPOs.Count > 0) {
							//foreach(AXParametricObject poo in ArchimatixEngine.currentModel.selectedPOs)
							//	Debug.Log(" --- " + poo.Name);
							prevSelectedPO = ArchimatixEngine.currentModel.selectedPOs [ArchimatixEngine.currentModel.selectedPOs.Count - 1];
							ArchimatixEngine.currentModel.deselectAll();

						}

						AXParametricObject npo = Library.instantiateParametricObject (li.readIntoLibraryFromRelativeAXOBJPath);

						npo.generator.pollInputParmetersAndSetUpLocalReferences();



						if (prevSelectedPO != null) {
							// AUTO-CONNECT TO A SELECTED SHAPE_MERGER
							ShapeMerger shapeMerger = null;


							Rect rect = new Rect ();
							if (prevSelectedPO != null && prevSelectedPO.generator is ShapeMerger) {
								shapeMerger = prevSelectedPO.generator as ShapeMerger;
								rect = new Rect (prevSelectedPO.rect.x - 300, prevSelectedPO.rect.y, prevSelectedPO.rect.width, prevSelectedPO.rect.height);
							}

							if (shapeMerger == null && prevSelectedPO.generator.P_Output != null) {
								foreach (AXParameter d in prevSelectedPO.generator.P_Output.Dependents) {
									if (d.parametricObject.generator is ShapeMerger) {
										shapeMerger = d.parametricObject.generator as ShapeMerger;
										rect = new Rect (prevSelectedPO.rect.x + 50, prevSelectedPO.rect.y + 50, prevSelectedPO.rect.width, prevSelectedPO.rect.height);
										break;
									}
								}
							}
							if (shapeMerger != null) {
								shapeMerger.connect (npo);
								npo.rect = rect;
								npo.generator.pollInputParmetersAndSetUpLocalReferences ();
								shapeMerger.pollInputParmetersAndSetUpLocalReferences ();
								ArchimatixEngine.currentModel.autobuild ();
								shapeMerger.adjustWorldMatrices ();
								npo.generator.adjustWorldMatrices ();

								//AXNodeGraphEditorWindow.zoomToRectIfOpen(npo.rect);

							}
						}

						if (npo != null)
						{
							if (npo.rect.width < npo.generator.minNodePaletteWidth)
								npo.rect.width = npo.generator.minNodePaletteWidth;
						}
						//Selection.activeGameObject = null;
					}
				}
			}
		}
		else
		{


			// 3D Library

			if (ArchimatixEngine.library != null && ArchimatixEngine.library.filteredResults != null)
			{
				for (int i = 0; i < ArchimatixEngine.library.filteredResults.Count; i++) {

					// THE BIG CLICK 3D !!!!

					LibraryItem li = ArchimatixEngine.library.filteredResults [i];

					if (! li.includeInSidebarMenu)
						continue;
					if (li.is2D)
						continue;


					if (GUILayout.Button (new GUIContent (li.icon, li.Name), GUILayout.Width (imagesize), GUILayout.Height (imagesize))) {
                        AXParametricObject npo = Library.instantiateParametricObject (li.readIntoLibraryFromRelativeAXOBJPath);

                        if (npo.is3D() && npo.model.overrideColliderOnLibraryItem)
                        {
                           
                            npo.colliderType = npo.model.defaultColliderType;
                        }



                        //Selection.activeGameObject = null;
                    }
                }
			}

		}


		EditorGUILayout.Space();


		EditorGUILayout.EndVertical();
		//Debug.Log(sw.stop());
		EditorGUILayout.EndScrollView();

		/* Not sure why I was doing this - it took up a huge amount of CPU!
		 * 
		editor.Repaint();
		SceneView sv = SceneView.lastActiveSceneView;
		if (sv != null)
			sv.Repaint();
		*
		*/
	}

}