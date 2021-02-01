#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections; 
using System.Collections.Generic;
using System.Globalization;

using AXClipperLib;

using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using AXGeometry;

using AX;
using AX.Generators;
using AXEditor;

using Parameters 				= System.Collections.Generic.List<AX.AXParameter>;



namespace AX.GeneratorHandlers
{

	
	public class BuildingExtruderHandler : GeneratorHandler2D 
	{
		
		[System.NonSerialized]
		public int movingMidhandle = 0;
		

		public List<Vector2> handlePoints;

		float lastclick;
	
		public override void drawTransformHandles(List<string> visited, Matrix4x4 consumerM, bool addLocalMatrix = false)
		{

			//Debug.Log (".............................................. drawHandles NO-D");

			// Implemented differently in 2D and 3D PO's 

			// See Generator2D and AX.Generators.Generator3D
			if (Event.current.command)
				base.drawTransformHandles(visited, consumerM, addLocalMatrix);
		}



		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			BuildingExtruder gener = (BuildingExtruder) parametricObject.generator;

			base.drawControlHandles(ref visited, consumerM,  true);

			if (alreadyVisited(ref visited, "FreeCurveHandler"))
				return;


		
			AXParameter p = parametricObject.getParameter("Output Shape");
			
			if (p == null || p.getPaths() == null)
				return;
			

			parametricObject.model.addActiveFreeCurve(parametricObject);

			Event e = Event.current;

			if (ArchimatixEngine.sceneViewState == ArchimatixEngine.SceneViewState.AddPoint && e.type == EventType.KeyDown && (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return))
			{
				

				ArchimatixEngine.setSceneViewState(ArchimatixEngine.SceneViewState.Default);

			
				e.Use();

			}


			handlePoints = new List<Vector2>();


			bool handleHasChanged = false;




			// HANDLES MATRIX FOR GRID

			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			/*
			Matrix4x4 context 		= parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;
			if (generator.hasOutputsConnected() || parametricObject.is2D())
				context *= generator.localMatrix.inverse;
			else
				context *= parametricObject.getAxisRotationMatrix().inverse  * generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix();

			Handles.matrix = context;
			*/


			// DRAW GRID
			Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix.inverse;


			float gridDim = parametricObject.model.snapSizeGrid * 100;

			// AXIS
			Handles.color = Color.red;
			Handles.DrawLine(new Vector3(-gridDim/2, 0, 0), new Vector3(gridDim/2, 0, 0));
			Handles.color = Color.green;
			Handles.DrawLine(new Vector3(0, -gridDim/2, 0), new Vector3(0, gridDim/2, 0));
			
			// GRID COLORS

			if (ArchimatixEngine.snappingOn())
				Handles.color = new Color(1, .5f, .65f, .4f);
			else
				Handles.color = new Color(1, .5f, .65f, .25f);



			
			AXEditorUtilities.DrawGrid3D(gridDim, parametricObject.model.snapSizeGrid);
			
			

			
			//Handles.matrix = Matrix4x4.identity;

			CurveControlPoint2D newCurvePoint = null;; 
			int newCurvePointIndex = -1;

			if (parametricObject.curve != null)
			{
				//if (Event.current.type == EventType.MouseDown)
				//	selectedIndex = -1;
				
				//Vector3 pos;


				
				for (int i=0; i<parametricObject.curve.Count; i++)
				{
					//Debug.Log (i + ": "+ parametricObject.curve[i].position);
					
					// Control points in Curve

					bool pointIsSelected = (generator.selectedIndices != null && generator.selectedIndices.Contains(i));



					Vector2 pos = new Vector2(parametricObject.curve[i].position.x, parametricObject.curve[i].position.y);




					Handles.color = (pointIsSelected) ? Color.white :  Color.magenta;

					float capSize = .13f*HandleUtility.GetHandleSize(pos);

					if (pointIsSelected)
					{
						capSize = .17f*HandleUtility.GetHandleSize(pos);
					}	



					 


					// POSITION
					//pos = new Vector3(parametricObject.curve[i].position.x, parametricObject.curve[i].position.y, 0);

//					pos = Handles.FreeMoveHandle(
//						pos, 
//						Quaternion.identity,
//						capSize,
//						Vector3.zero, 
//						(controlID, positione, rotation, size, type) =>
//					{
//						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
//						Debug.Log("YOP");
//						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
//					});

					
					pos = Handles.FreeMoveHandle(
						pos, 
						Quaternion.identity,
						capSize,
						Vector3.zero, 
						(controlID, position, rotation, size, type) =>
						{ 
							
							if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
							{

								//Debug.Log("*** " + e.type + " -" + e.keyCode + "-");

								// MOUSE DOWN ON HANDLE!

								Undo.RegisterCompleteObjectUndo (parametricObject.model, "FreeCurve");
								//Debug.Log(controlID + ": " + e.type);

								
								//Debug.Log("SELECT NODE " +i + " ci="+controlID);

								if (i == 0 && ArchimatixEngine.sceneViewState == ArchimatixEngine.SceneViewState.AddPoint && ! e.alt)
								{
									

									generator.P_Output.shapeState = ShapeState.Closed;

									ArchimatixEngine.setSceneViewState(ArchimatixEngine.SceneViewState.Default);

								}
								else if (e.shift && ! ArchimatixEngine.mouseIsDownOnHandle)
									generator.toggleItem(i);
								
								else if (gener.selectedIndices == null || gener.selectedIndices.Count < 2)
								{
									if (! generator.isSelected(i))
									{
										parametricObject.model.clearActiveFreeCurves();
										generator.selectOnlyItem(i);
									}
									
								}
								ArchimatixEngine.isPseudoDraggingSelectedPoint = i;

																// CONVERT TO BEZIER
								

								for (int j = 0; j < generator.P_Output.Dependents.Count; j++) 
									generator.P_Output.Dependents [j].parametricObject.generator.adjustWorldMatrices ();


								ArchimatixEngine.mouseDownOnSceneViewHandle();



							}


							Handles.SphereHandleCap(controlID, position, rotation, size, type);
						});










					
					// MID_SEGEMNET HANDLE

					if (i < parametricObject.curve.Count)
					{
						//Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.localMatrix.inverse;

						Handles.color = Color.cyan;

						//Debug.Log("mid handle "+i);
						 
						CurveControlPoint2D a = parametricObject.curve[i];

						int next_i = (i==parametricObject.curve.Count-1) ? 0 : i+1;
						CurveControlPoint2D b = parametricObject.curve[next_i];

						if (a.isPoint() && b.isPoint())						
							pos = Vector2.Lerp(a.position, b.position, .5f);
						else 
						{
							Vector2 pt =  FreeCurve.bezierValue(a, b, .5f);
							pos = (Vector3) pt;
						}

						EditorGUI.BeginChangeCheck();

						pos = Handles.FreeMoveHandle(
							pos, 
							Quaternion.identity, 
							.06f*HandleUtility.GetHandleSize(pos),
							Vector3.zero,
							(controlID, positione, rotation, size, eventType) =>
							{ 
								
								Handles.CubeHandleCap(controlID, positione, rotation, size, eventType);
							});
						
						if(EditorGUI.EndChangeCheck())
						{
							 
							// add point to spline at i using pos.x, pos.y
							Undo.RegisterCompleteObjectUndo (parametricObject.model, "New Midpoint");

							//Debug.Log(pos);
							//Debug.Log(ArchimatixEngine.isPseudoDraggingSelectedPoint + " ::: " + (i));

							// IF THIS IS THE FIRST CLICK, THEN CREAT A POINT AND CONTINUE AS THOUGH IT WERE A NORMAL POINT
							//if (ArchimatixEngine.isPseudoDraggingSelectedPoint != (i+1))
							if (ArchimatixEngine.isPseudoDraggingSelectedPoint == -1)
							{
								//Debug.Log("CREATE!!!!");
								newCurvePoint = new CurveControlPoint2D(pos.x, pos.y);

								if (parametricObject.curve.Count == 2)
									newCurvePointIndex= i;
								else
									newCurvePointIndex= i+1;

								parametricObject.curve.Insert(newCurvePointIndex, newCurvePoint);
								ArchimatixEngine.isPseudoDraggingSelectedPoint = newCurvePointIndex;
								generator.selectedIndex = newCurvePointIndex;

								parametricObject.model.clearActiveFreeCurves();
								generator.selectOnlyItem(newCurvePointIndex);
							}

							parametricObject.model.isAltered();
						}


					}
					  









					
				} // \loop	






				// BEZIER HANDLES LOOP
				for (int i=0; i<parametricObject.curve.Count; i++)
				{
					//Debug.Log (i + ": "+ parametricObject.curve[i].position);

					// Control points in Curve

					bool pointIsSelected = (generator.selectedIndices != null && generator.selectedIndices.Contains(i));



					Vector2 pos = parametricObject.curve[i].position; // new Vector3(parametricObject.curve[i].position.x, parametricObject.curve[i].position.y, 0);
					Vector3 posA = parametricObject.curve[i].position + parametricObject.curve[i].localHandleA; //new Vector3(parametricObject.curve[i].position.x+parametricObject.curve[i].localHandleA.x, parametricObject.curve[i].position.y+parametricObject.curve[i].localHandleA.y, 0);
					Vector2 posB = parametricObject.curve[i].position + parametricObject.curve[i].localHandleB; //new Vector3(parametricObject.curve[i].position.x+parametricObject.curve[i].localHandleB.x, parametricObject.curve[i].position.y+parametricObject.curve[i].localHandleB.y, 0);

					//posA = new Vector3(posA.z, posA.y);
					Handles.color = (pointIsSelected) ? Color.white :  Color.magenta;



					if (pointIsSelected)
					{

						
					
						Handles.color = Color.magenta;

						if ( parametricObject.curve[i].isBezierPoint())
						{
							Handles.color = Color.white;
							if (parametricObject.curve[i].curvePointType == CurvePointType.BezierBroken)
								Handles.color = Color.cyan;
							Handles.DrawLine(pos, posA);
							Handles.DrawLine(pos, posB);

							  

							EditorGUI.BeginChangeCheck();
							posA = Handles.FreeMoveHandle(
								posA, 
								Quaternion.identity,
								.1f*HandleUtility.GetHandleSize(pos),
								Vector3.zero, 
								Handles.SphereHandleCap
							);

							if(EditorGUI.EndChangeCheck())
							{
								//Debug.Log(" -- " + posA);
								Undo.RegisterCompleteObjectUndo (parametricObject.model, "FreeformShapee");
								handleHasChanged = true;

								parametricObject.curve[i].setHandleA(posA);




								//parametricObject.curve[i].localHandleA = new Vector2(pos.x, pos.y) - parametricObject.curve[i].position;
								//parametricObject.model.generate("Move FreeForm Shape Handle");
								parametricObject.model.isAltered();

							}





							// HANDLE_B


							EditorGUI.BeginChangeCheck();
							posB = Handles.FreeMoveHandle(
								posB, 
								Quaternion.identity,
								.1f*HandleUtility.GetHandleSize(pos),
								Vector3.zero, 
								Handles.SphereHandleCap
							);

							if(EditorGUI.EndChangeCheck())
							{
								Undo.RegisterCompleteObjectUndo (parametricObject.model, "FreeformShapee");
								handleHasChanged = true;
								//parametricObject.curve[i].localHandleB = new Vector2(pos.x, pos.y) - parametricObject.curve[i].position;
								parametricObject.curve[i].setHandleB(posB);//new Vector2(posB.x, posB.y));



								//parametricObject.model.generate("Move FreeForm Shape Handle");
								parametricObject.model.isAltered();

							}


						}
					} // selected



				} // \bezier handles loop




				if (handleHasChanged)
				{
					
				}
			}
			
			Handles.matrix = prevHandlesMatrix;
			
		}



		public void drawGUIControls()
		{
			//FreeCurve gener = (FreeCurve) generator;

			//AXModel model = ArchimatixEngine.currentModel;
			FreeCurve gener = (FreeCurve) parametricObject.generator;

			//Debug.Log(ArchimatixEngine.uiIcons["AddPoint"] );
			if (ArchimatixEngine.uiIcons == null || ArchimatixEngine.uiIcons.Count == 0 || ArchimatixEngine.uiIcons["AddPoint"] == null)
				AXEditorUtilities.loaduiIcons();



			// 2D GUI
			Handles.BeginGUI();

			GUIStyle buttonStyle = GUI.skin.GetStyle ("Button");
			buttonStyle.alignment = TextAnchor.MiddleCenter;

			float prevFixedWidth = buttonStyle.fixedWidth;
			float prevFixedHeight = buttonStyle.fixedHeight;


			buttonStyle.fixedHeight = 30;

			//GUIStyle areaStyle = new GUIStyle();
			//areaStyle. = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));

			float windowWidth 	= (SceneView.lastActiveSceneView != null) ? SceneView.lastActiveSceneView.position.width : Screen.width;
			float buttonWid = 40;
			float yHgt = 20;


			if (ArchimatixEngine.sceneViewState == ArchimatixEngine.SceneViewState.Default)
			{
				if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 - 3f*buttonWid),yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons["AddPoint"], "Add Point")))
						ArchimatixEngine.setSceneViewState(ArchimatixEngine.SceneViewState.AddPoint);

			}
			else
			{
				if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 - 3f*buttonWid),yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons["AddPointOff"], "Stop Adding Points")))
					ArchimatixEngine.setSceneViewState(ArchimatixEngine.SceneViewState.Default);

			}

			string gridSnapStr = (ArchimatixEngine.snappingOn()) ? "GridSnapOn" : "GridSnapOff";

			if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 + 3f*buttonWid), yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons[gridSnapStr], "Grid Snap")))
            {
                ArchimatixEngine.snappingIsOn = !ArchimatixEngine.snappingIsOn;
                EditorPrefs.SetBool("snappingIsOn", ArchimatixEngine.snappingIsOn);
            }


			  

			if (generator.selectedIndices != null && generator.selectedIndices.Count == 1)
			{
				int selInd = generator.selectedIndices[0];

				CurveControlPoint2D cp = parametricObject.curve[ selInd ];
				if (cp != null)
				{
					string pointStr = (cp.curvePointType == CurvePointType.Point) ? "PointOn" : "PointOff";
					string bezierStr = (cp.curvePointType == CurvePointType.BezierMirrored) ? "BezierPointOn" : "BezierPointOff";
					string brokenStr = (cp.curvePointType == CurvePointType.BezierBroken) ? "BrokenBezierPointOn" : "BrokenBezierPointOff";
	
					 

//					if(GUI.Button(new Rect(((windowWidth-buttonWid)/2),55, buttonWid, 20), ArchimatixEngine.nodeIcons["Point"]))
//							gener.convertToBezierBroken(selInd);


					if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 - 1.5f*buttonWid),yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons["DeletePoint"], "Delete Point")))
							gener.DeleteSelected();

					if(GUI.Button(new Rect(((windowWidth-buttonWid)/2  - .5f*buttonWid), yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons[pointStr], "Sharp Point")))
							gener.convertToSharpPoint(selInd);

					if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 + .5f*buttonWid), yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons[bezierStr], "Bezier")))
							gener.convertToBezier(selInd);

					if(GUI.Button(new Rect(((windowWidth-buttonWid)/2 + 1.5f*buttonWid), yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons[brokenStr], "Broken Bezier")))
							gener.convertToBezierBroken(selInd);


//					if (cp.curvePointType != CurvePointType.BezierBroken)
//						if(GUI.Button(new Rect(((windowWidth-buttonWid)/2),55, buttonWid, 20), "Break Handles"))
//							gener.convertToBezierBroken(selInd);
//					if (cp.curvePointType == CurvePointType.BezierBroken)
//						if(GUI.Button(new Rect(((windowWidth-buttonWid)/2),55, buttonWid, 20), "Unify Handles"))
//							gener.convertToBezier(selInd);
				}


			}


		


			EditorGUIUtility.labelWidth = 40;



			Handles.EndGUI();




			buttonStyle.fixedWidth = prevFixedWidth;
			buttonStyle.fixedHeight = prevFixedHeight;


		}





		public override void OnSceneGUI()
		{
			

			AXModel model = ArchimatixEngine.currentModel;


			FreeCurve gener = (FreeCurve) generator;

			Event e = Event.current;




			if (model != null)
			{
				Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix;



				drawGUIControls();

                float windowHeight = (SceneView.lastActiveSceneView != null) ? SceneView.lastActiveSceneView.position.height : Screen.height;

                bool notInFooterMenuZone = (e.mousePosition.y < (windowHeight - 40));

   				if(notInFooterMenuZone && (ArchimatixEngine.sceneViewState == ArchimatixEngine.SceneViewState.AddPoint || (e.type == EventType.MouseDrag && ArchimatixEngine.isPseudoDraggingSelectedPoint >= 0))   && ! e.alt) 
				{

                   

                    Vector3 v1 = context.MultiplyPoint3x4(Vector3.zero);
					Vector3 v2 = context.MultiplyPoint3x4(new Vector3( 100, 0, 0));
					Vector3 v3 = context.MultiplyPoint3x4(new Vector3(0, 100, 0));
					
					//Debug.Log ("-"+ArchimatixEngine.sceneViewState + "- plane points: " + v1 + ", " + v2+ ", " + v3 );


					Plane drawingSurface = new Plane( v1, v2, v3);


                    //Matrix4x4 = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix.inverse;

                   

					Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);		

					float rayDistance = 0;
					Vector3 hitPoint;




					// Point on Plane
					if (drawingSurface.Raycast(ray, out rayDistance)) 
					{
						hitPoint = ray.GetPoint(rayDistance);


						Vector3 hit_position3D = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z);

 					

						if ( ArchimatixEngine.snappingOn() )
							hit_position3D = AXGeometry.Utilities.SnapToGrid(hit_position3D, parametricObject.model.snapSizeGrid);

   						int nearId = HandleUtility.nearestControl;

						if (nearId == 0)
						{

							// CROSSHAIR

							Matrix4x4 prevHandlesMAtrix = Handles.matrix;

							//Handles.matrix =  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 90, 0), Vector3.one) * context;
							//Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix.inverse;

							Color cyan = Color.cyan;
							cyan.a = .1f;

							float hScale = .09f*HandleUtility.GetHandleSize(hit_position3D);
							float lScale = 5*hScale;

							Handles.color = cyan;
							Handles.DrawSolidDisc(hit_position3D, 
													drawingSurface.normal, 						                  
													hScale);

							cyan.a = .8f;
							Handles.color = cyan;
							Handles.DrawWireDisc(hit_position3D, 
													drawingSurface.normal, 						                  
													2*hScale);

							
							Handles.DrawLine(hit_position3D+lScale*v2, hit_position3D-lScale*v2);

							//Handles.color = Color.white;
							Handles.DrawLine(hit_position3D+lScale*v3, hit_position3D-lScale*v3);


							Handles.matrix = prevHandlesMAtrix;
																										
							// put visual cue under mouse....
							//Debug.Log (hitPoint);

						}



						hitPoint = context.inverse.MultiplyPoint3x4(hitPoint);

						Vector2 hit_position2D = new Vector2(hitPoint.x, hitPoint.y);

						if (ArchimatixEngine.snappingOn())
							hit_position2D = AXGeometry.Utilities.SnapToGrid(hit_position2D, parametricObject.model.snapSizeGrid);


						// EVENTS 

						if (e.type == EventType.MouseDown && ! e.alt && nearId == 0 && e.button == 0)
						{

							ArchimatixEngine.isPseudoDraggingSelectedPoint = -1;

							if (gener != null)
							{ 
								if (! e.control) // add to end of line
								{
									// ADD POINT AT END

									gener.parametricObject.curve.Add(new CurveControlPoint2D(hit_position2D.x, hit_position2D.y));
									gener.selectedIndex = gener.parametricObject.curve.Count-1;


								}
								else // ADD POINT TO BEGINNING
								{
									gener.parametricObject.curve.Insert(0, new CurveControlPoint2D(hit_position2D.x, hit_position2D.y));
									gener.selectedIndex = 0;
								}
								
								
								ArchimatixEngine.isPseudoDraggingSelectedPoint = gener.selectedIndex;
								model.autobuild();
								
							}
						}
						else if (e.type == EventType.MouseDrag)
						{
							//Debug.Log("Dragging "+ ArchimatixEngine.isPseudoDraggingSelectedPoint + " :: " +  generator.selectedIndices);

							if( gener == ArchimatixEngine.selectedFreeCurve &&  ArchimatixEngine.isPseudoDraggingSelectedPoint >= 0)
							{
								if (gener.parametricObject.curve.Count > ArchimatixEngine.isPseudoDraggingSelectedPoint && generator.selectedIndices != null)
								{

									// The actual point being dragged:
									Vector2 displ = hit_position2D - gener.parametricObject.curve[ArchimatixEngine.isPseudoDraggingSelectedPoint].position;
									//gener.parametricObject.curve[ArchimatixEngine.isPseudoDraggingSelectedPoint].position = hit_position2D;
									gener.setCurvePointPosition (ArchimatixEngine.isPseudoDraggingSelectedPoint, hit_position2D);


									for (int i=0; i<model.activeFreeCurves.Count; i++)
									{ 
										FreeCurve fc = (FreeCurve) model.activeFreeCurves[i].generator;

										if (fc != null && fc.selectedIndices != null)
										{
											for (int j=0; j<fc.selectedIndices.Count; j++)
											{
												if (! (fc == gener  &&  fc.selectedIndices[j] == ArchimatixEngine.isPseudoDraggingSelectedPoint) )
												{
													fc.parametricObject.curve[fc.selectedIndices[j]].position += displ;
												} 
											}
										}
									} 

									//Debug.Log ("DRAGGING");
									parametricObject.setAltered();
									model.isAltered();
									generator.adjustWorldMatrices();
								}
							}	
								
						 
							
							
						} 
						if (e.type == EventType.MouseUp)
						{
							//if (ArchimatixEngine.isPseudoDraggingSelectedPoint > -1)
								

							ArchimatixEngine.isPseudoDraggingSelectedPoint = -1;
							ArchimatixEngine.draggingNewPointAt = -1;
							//model.autobuild();

							//Debug.Log("mouse up");
						}


						if ((e.type == EventType.MouseDown) || (e.type == EventType.MouseDrag && ArchimatixEngine.isPseudoDraggingSelectedPoint >= 0)  || (e.type == EventType.MouseUp))
						{

							
							//e.Use();
						}

					

						SceneView sv = SceneView.lastActiveSceneView;
						if (sv != null)
							sv.Repaint();

					}

				}

			
			}
		} // \OnScenView




		public override int customNodeGUIZone_4(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po, float x=0, float w=150)
		{
			float cur_x = ArchimatixUtils.cur_x; 

			Rect pRect = new Rect(x,cur_y, w, 16);
			//float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
			float box_w = pRect.width - cur_x - 1*ArchimatixUtils.indent;


			int lineHgt = (int)pRect.height;
			//int gap = 5;
			
			//int margin = 24;
			
			//float wid = pRect.width-margin;

			// LABEL
			Rect boxRect = new Rect(cur_x + ArchimatixUtils.indent+3, cur_y, box_w, pRect.height);
			Rect lRect = boxRect;

			lRect.x += 3;
			lRect.width -= 10;
			lRect.y = cur_y;

			GUI.Label(lRect, "Curve Points: " + po.curve.Count);
			cur_y += 16;



			GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle ("Label"));
			labelStyle.fixedWidth = 150;

			float origLabelWidth = EditorGUIUtility.labelWidth;

			EditorGUIUtility.labelWidth = 10;

			for (int i=0; i<po.curve.Count; i++)
			{
				lRect.y = cur_y;

				GUILayout.BeginArea(lRect);
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label( "["+i+"] ", GUILayout.MaxWidth(15));

                //				EditorGUI.BeginChangeCheck();
                //				po.curve[i].position.x = EditorGUILayout.FloatField("X", po.curve[i].position.x,  GUILayout.MinWidth(50));
                //				if (EditorGUI.EndChangeCheck())
                //				{
                //					po.model.isAltered();
                //				}

               // Debug.Log(i + " -" + po.curve[i].xExpression + "- " + (po.curve[i].xExpression==" "));

				EditorGUI.BeginChangeCheck();
				po.curve[i].xExpression = EditorGUILayout.TextField("X", po.curve[i].xExpression,  GUILayout.MinWidth(50));
				if (EditorGUI.EndChangeCheck())
				{
					// evaluate expression and set po.curve[i].position.x
					float num;

					if (! string.IsNullOrEmpty(po.curve [i].xExpression)) {
						
						if (float.TryParse (po.curve [i].xExpression, NumberStyles.Any, CultureInfo.InvariantCulture, out num)) {
							// ONLY NUMERIC
							po.curve [i].position.x = num;
						} else {
							AXParameter p = parametricObject.getParameter (po.curve [i].xExpression);
							if (p != null) {
								
								po.curve [i].position.x = p.FloatVal;
							} else {
								//Debug.Log ("No parameter called " + po.curve [i].xExpression);
							}
						}
					}
					po.model.isAltered();
				}

				//GUILayout.Label( "Y ", GUILayout.MaxWidth(12));
				// Y POSITION
//				EditorGUI.BeginChangeCheck();
//				po.curve[i].position.y = EditorGUILayout.FloatField("Y ", po.curve[i].position.y,  GUILayout.MinWidth(50));
//				if (EditorGUI.EndChangeCheck())
//				{
//					po.model.isAltered();
//				}
				EditorGUI.BeginChangeCheck();
				po.curve[i].yExpression = EditorGUILayout.TextField("Y", po.curve[i].yExpression,  GUILayout.MinWidth(50));
				if (EditorGUI.EndChangeCheck())
				{
					// evaluate expression and set po.curve[i].position.x
					float num;

					if (!string.IsNullOrEmpty (po.curve [i].yExpression)) {

						if (float.TryParse (po.curve [i].yExpression, NumberStyles.Any, CultureInfo.InvariantCulture, out num)) {
							// ONLY NUMERIC
							po.curve [i].position.y = num;
						} else {
							AXParameter p = parametricObject.getParameter (po.curve [i].yExpression);
							if (p != null) {
								po.curve [i].position.y = p.FloatVal;
							} else {
								//Debug.Log ("No parameter called " + po.curve [i].xExpression);
							}
						}
					} else {
						// no string, init string
						po.curve [i].yExpression = ""+po.curve [i].position.y;
					}
					po.model.isAltered();
				}



				//GUI.Label(new Rect, "["+i+"] X:" );


				//+ po.curve[i].position

				EditorGUILayout.EndHorizontal();
				GUILayout.EndArea();


				cur_y += 16;

			}

			cur_y += 16;


			EditorGUIUtility.labelWidth = origLabelWidth;

			return cur_y;
		}





		public bool isNearHandle(Vector3 pt, float nearDist = .01f)
		{
			

			if (parametricObject.curve != null)
			{
				//if (Event.current.type == EventType.MouseDown)
				//	selectedIndex = -1;
				
				//Vector3 pos;
				
				for (int i=0; i<parametricObject.curve.Count; i++)
				{
					//Debug.Log (i + ": "+ parametricObject.spline.verts[i]);
					
					// Control points in Curve


					Vector3 pos  = new Vector3(parametricObject.curve[i].position.x, parametricObject.curve[i].position.y, 0);
					if (Vector3.Distance(pt, pos) < nearDist)
						return true;

					if (Vector3.Distance(pt, pos) < nearDist)
						return true;

					if (Vector3.Distance(pt, pos) < nearDist)
						return true;



				}
			}
			return false;

		}



	}






}
	
