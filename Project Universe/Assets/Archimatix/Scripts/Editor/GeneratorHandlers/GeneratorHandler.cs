#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;

using AX.Generators;
using AXEditor;




/* Generating Handles
		 * 
		 * On each SceneView OnSceneGUI, the Model iterates through each 
		 * selected ParametricObject and asks it to draw its handles.
		 * 
		 * A selected PO will determine where in space its handles should be located.
		 * 
		 * If the PO's output is consumed by another PO, the selected PO will ask that 
		 * consumer where it should draw itself. If that consumr is consumed, it will ask it, etc.
		 * 
		 * Once the selected PO has its matrix from its consumers, 
		 * it will draw its supart or consumed PO's by handing a matrix down to them.
		 * The selected PO may want, for example, a section profile shape to draw its handles roted up 90 degs, etc.
		 * 
		 * Thus, the matrix to be passed upstream along the input chain
		 * will be the [consumer transformation] * [where this PO would like the 
		 * subpart handles]. As it goes upstream in the chain of inputs, each stop adds its own transformation.
		 * 
		 * 
		 * 
		 * 
		 */

namespace AX.GeneratorHandlers
{
	public class GeneratorHandler
	{



		public AXParametricObject parametricObject;
		public AX.Generators.Generator generator;

		public static float handleScale = .15f;




		public static GeneratorHandler getGeneratorHandler(AXParametricObject po)
		{

			GeneratorHandler generatorHandler = null;

			//			if (po != null && po.generator != null)
			//			{
			//				if (! ArchimatixEngine.generatorHandlerCache.ContainsKey(po.Guid))
			//				{



			//Debug.Log(" ++++++++++++++++++++++++++++++++ " + po.Name);
			//Debug.Log(" ++++++++++++++++++++++++++++++++ " + po.generator);
			Type typeBasedOnString = po.generator.getGeneratorHandlerType(); //.generatorHandlerType;

			if (typeBasedOnString != null)
			{
				generatorHandler = (GeneratorHandler)Activator.CreateInstance(typeBasedOnString);
				generatorHandler.parametricObject = po;
				generatorHandler.generator = po.generator;
			}




			//					ArchimatixEngine.generatorHandlerCache.Add(po.Guid, generatorHandler);
			//				}
			//				else
			//				{
			//					generatorHandler = ArchimatixEngine.generatorHandlerCache[po.Guid];
			//				}
			//			}




			return generatorHandler;

		}





		// These function are called as the Node Graph window draws a node palette GUI.
		// They should return cur_y
		public virtual int customNodeGUIZone_1(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po)
		{

			return cur_y;
		}
		public virtual int customNodeGUIZone_2(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po)
		{


			return cur_y;
		}
		public virtual int customNodeGUIZone_3(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po)
		{


			return cur_y;
		}
		public virtual int customNodeGUIZone_4(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po, float x = 0, float w = 150)
		{


			return cur_y;
		}



		// DRAW_HANDLES
		// -------------------------------------------------------------------------
		public virtual void drawHandles()
		{


			AXModel model = parametricObject.model;

			if (model == null)
				return;


			Matrix4x4 consumerM = Matrix4x4.identity;// model.transform.localToWorldMatrix * parametricObject.generator.parametricObject.worldDisplayMatrix;		


			if (!parametricObject.is2D())
				consumerM *= parametricObject.getLocalMatrix().inverse;

			//drawTransformHandles(new List<string>(), consumerM);			
			drawTransformHandles(new List<string>(), consumerM);

			List<string> visited = new List<string>();
			drawControlHandles(ref visited, consumerM, false);
		}




		public virtual void drawHandlesUnselected()
		{
			// mostly for 2D shapes
		}





		public virtual void drawTransformHandles(List<string> visited, Matrix4x4 consumerM, bool addLocalMatrix = false)
		{

			//Debug.Log (".............................................. drawHandles NO-D");

			// Implemented differently in 2D and 3D PO's 

			// See Generator2D and AX.Generators.Generator3D

		}


		public virtual void OnSceneGUI()
		{
			// 
		}







		#region Control Handles
		// CONTROL HANDLES

		public virtual void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			drawControlHandlesofInputParametricObjects(ref visited, consumerM, true);
		}


		public virtual void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{

		}




		public virtual void drawControlHandles(ref List<string> visited)
		{
			drawControlHandles(ref visited, Matrix4x4.identity, false);
		}



		public bool alreadyVisited(ref List<string> visited, string callerClass)
		{
			string key = "c_" + parametricObject.Guid + "_" + callerClass;


			if (visited.Contains(key))
				return true;

			// mark as visited
			visited.Add(key);

			//Debug.Log("visit: "  + parametricObject.Name + " :: " + key );

			return false;
		}



		public virtual void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{


			if (parametricObject == null || !parametricObject.isActive)
				return;


			if (alreadyVisited(ref visited, "GeneratorHandler"))
				return;


			//Debug.Log(parametricObject.Name + " drawControlHandles");

			// CUSTOM HANDLES GENERALIZED IN THE DATA OF EACH ParmetricObject
			// Each PO has a List of AXHandles that define their own location and use of their 
			// reserved variable: han_x, han_y, han_z

			// The transformation matrix passed in as an argument has thisPO's local transform, 
			// as well as the model's and the counsmer's

			Matrix4x4 prevhandlesMatrix = Handles.matrix;
			Color prevColor = Handles.color;


			Matrix4x4 flipM = Matrix4x4.identity;
			if (generator.P_Output != null && generator.P_Output.flipX)
				flipM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));

			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;
			//if (parametricObject.is2D() && ! generator.hasOutputsConnected())
			//	context *=  parametricObject.getAxisRotationMatrix().inverse  * generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix()  * generator.localMatrix;

			Handles.matrix = context * generator.localMatrix.inverse * flipM * generator.localMatrix; //consumerM;



			if (parametricObject.is2D())
			{
				Handles.matrix = generator.getLocalAlignMatrix() * Handles.matrix;
			}


			float handlesMatrixScaleAdjuster = 1.442f / AXUtilities.GetScale(Handles.matrix).magnitude;


			if (!(this.generator is Replicant))
			{
				drawControlHandlesofInputParametricObjects(ref visited, context * generator.getLocalConsumerMatrixPerInputSocket(parametricObject));

			}





			if (parametricObject.handles != null)
			{


				foreach (AXHandle han in parametricObject.handles)
				{





					//// TANGENT
					float tangent = han.getTangent();


					//// RADIUS
					float radius = han.getRadius();// han.radiusCache;



					// SET POSITION OF HANDLE
					//
					//					float x = 0;
					//					float y = 0;
					//					float z = 0;
					//
					//					try {	
					//						x = (float) parametricObject.parseMath(han.pos_x);
					//					} catch(Exception e) {
					//						parametricObject.codeWarning = "3. Handle error: Please check syntax of: \"" + han.pos_x+"\" " + e ;
					//					}
					//
					//					try {	
					//						//Debug.Log(han.pos_y);
					//						y = (float) parametricObject.parseMath(han.pos_y);
					//					} catch(Exception e) {
					//						parametricObject.codeWarning = "4. Handle error: Please check syntax of: \"" + han.pos_y+"\" " + e ;
					//					}
					//
					//					try {	
					//						z = (float) parametricObject.parseMath(han.pos_z);
					//					} catch(Exception e) {
					//						parametricObject.codeWarning = "5. Handle error: Please check syntax of: \"" + han.pos_z+"\" "+e.Message;
					//					}
					//
					//
					//
					//
					//					Vector3 pos = new Vector3(x, y, z);
					//


					Vector3 pos = han.getPointPosition();


					bool handleHasChanged = false;
					string handleChangedVar = null;





					switch (han.Type)
					{




						// POINT HANDLE
						// -------------------------------------------------------------------------------------------

						case AXHandle.HandleType.Point:

							Handles.color = han.color;
							// get to world space
							//Debug.Log(pos);
							EditorGUI.BeginChangeCheck();
							pos = Handles.FreeMoveHandle(
								pos,
								Quaternion.identity,
								handleScale * HandleUtility.GetHandleSize(pos) * handlesMatrixScaleAdjuster,
								Vector3.zero,
								(controlID, positione, rotation, size, type) =>
								{
									if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
									{
										ArchimatixEngine.mouseDownOnSceneViewHandle();
										han.parametricObject.model.latestHandleClicked = han;
										//Debug.Log(han.Name);
									}

									Handles.SphereHandleCap(controlID, positione, rotation, size, type);
								});



							if (EditorGUI.EndChangeCheck())
							{

								if (!float.IsNaN(pos.x))
								{
									Undo.RegisterCompleteObjectUndo(parametricObject.model, "Point Handle");

									handleHasChanged = true;
									parametricObject.geometryControls.isOpen = true;

									//gener.parametricObject.model.isAltered(23);
									for (int i = 0; i < generator.P_Output.Dependents.Count; i++)
										generator.P_Output.Dependents[i].parametricObject.generator.adjustWorldMatrices();

								}


								// update these vars so math parsing has them as special variables...
								if (ArchimatixEngine.snappingOn())
									pos = AXGeometry.Utilities.SnapToGrid(pos, parametricObject.model.snapSizeGrid);

								parametricObject.setVar("han_x", pos.x);
								parametricObject.setVar("han_y", pos.y);
								parametricObject.setVar("han_z", pos.z);

								han.processExpressionsAfterHandleChange();
							}

							break;





						// CIRCLE HANDLE
						// -------------------------------------------------------------------------------------------
						case AXHandle.HandleType.Circle:

							// get to world space
							Matrix4x4 prevMatrix = Handles.matrix;

							Matrix4x4 rotang = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), Vector3.one);

							Handles.matrix *= rotang;

							parametricObject.setVar("han_r", radius);

							parametricObject.setVar("han_t", tangent);



							Vector3 radpos = pos + new Vector3(radius, 0, 0);
							Vector3 tanpos = pos + new Vector3(radius, tangent * (radius / 10), 0);

							//Color prevColor = Handles.color;





							// TANGENT
							Color preTanColor = Handles.color;
							Color tanColor = Color.cyan;
							tanColor.a *= .5f;
							Handles.color = tanColor;

							Vector3[] tanLine = new Vector3[2];
							tanLine[0] = radpos;
							tanLine[1] = tanpos;
							Handles.DrawAAPolyLine(5, tanLine);


							Handles.DrawWireDisc(pos, Vector3.forward, radius);

							EditorGUI.BeginChangeCheck();
							tanpos = Handles.FreeMoveHandle(
								tanpos,
								Quaternion.identity,
								handleScale * HandleUtility.GetHandleSize(tanpos) * handlesMatrixScaleAdjuster,
								Vector3.zero,
								(controlID, positione, rotation, size, type) =>
								{

									if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
									{
										ArchimatixEngine.mouseDownOnSceneViewHandle();
										han.parametricObject.model.latestHandleClicked = han;
										//ArchimatixEngine.selectedFreeCurve = gener;
									}
									Handles.SphereHandleCap(controlID, positione, rotation, size, type);
								});


							tangent = tanpos.y / (radius / 10);
							if (EditorGUI.EndChangeCheck())
							{
								Undo.RegisterCompleteObjectUndo(parametricObject.model, "Circle Handle");
								parametricObject.setVar("han_t", tangent);
								han.tangentCache = tangent;
								handleHasChanged = true;
								handleChangedVar = "han_t";
								parametricObject.geometryControls.isOpen = true;
							}
							Handles.color = preTanColor;

							// RADIUS
							EditorGUI.BeginChangeCheck();
							radpos = Handles.FreeMoveHandle(
								radpos,
								Quaternion.identity,
								handleScale * HandleUtility.GetHandleSize(radpos) * handlesMatrixScaleAdjuster,
								Vector3.zero,
								(controlID, positione, rotation, size, type) =>
								{

									if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
									{
										ArchimatixEngine.mouseDownOnSceneViewHandle();
										han.parametricObject.model.latestHandleClicked = han;
										//ArchimatixEngine.selectedFreeCurve = gener;
									}
									Handles.SphereHandleCap(controlID, positione, rotation, size, type);
								});

							radius = (radpos - pos).x;
							if (EditorGUI.EndChangeCheck())
							{
								Undo.RegisterCompleteObjectUndo(parametricObject.model, "Radius Handle");
								parametricObject.setVar("han_r", radius);
								han.radiusCache = radius;
								handleHasChanged = true;
								handleChangedVar = "han_r";
								parametricObject.geometryControls.isOpen = true;
							}








							// update these vars so math parsing has them as special variables...




							Handles.matrix = prevMatrix;
							break;










						// ANGLE HANDLE
						case AXHandle.HandleType.Angle:

							// Debug.Log("ANGLE HANDLE");
							Handles.color = Color.red;

							float len = 2;
							float ang = 10;
							try
							{
								ang = han.angleCache;  // (float)parametricObject.parseMath(han.angle );
							}
							catch (Exception e)
							{
								parametricObject.codeWarning = "Handle error: Please check syntax of: \"" + han.angle + "\" " + e.Message;
							}
							try
							{
								len = han.lenCache;  // (float) parametricObject.parseMath(han.len);
							}
							catch (Exception e)
							{
								parametricObject.codeWarning = "Handle error: Please check syntax of: \"" + han.len + "\" " + e.Message;
							}

							float lenx = len * Mathf.Cos(ang * Mathf.Deg2Rad);
							float leny = len * Mathf.Sin(ang * Mathf.Deg2Rad);


							Vector3 angpos = new Vector3(lenx, 0, leny);
							angpos += pos;



							Handles.color = Color.yellow;
							Handles.DrawWireArc(pos, Vector3.up, Vector3.right, -ang * 1.25f, len);


							Handles.color = Color.yellow;
							Handles.Label(angpos, ("" + ang));

							Handles.DrawLine(pos, angpos);

							Handles.color = Color.red;


							// ANGLE -- POSITION
							EditorGUI.BeginChangeCheck();
							angpos = Handles.FreeMoveHandle(
							angpos,
							Quaternion.identity,
							handleScale * HandleUtility.GetHandleSize(angpos) * handlesMatrixScaleAdjuster,
							Vector3.zero,
							(controlID, positione, rotation, size, type) =>
							{

								if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
								{
									ArchimatixEngine.mouseDownOnSceneViewHandle();
									han.parametricObject.model.latestHandleClicked = han;
									//ArchimatixEngine.selectedFreeCurve = gener;
								}
								Handles.SphereHandleCap(controlID, positione, rotation, size, type);
							});
							if (EditorGUI.EndChangeCheck())
							{

								angpos -= pos;


								parametricObject.setVar("han_x", angpos.x);
								parametricObject.setVar("han_y", angpos.y);
								parametricObject.setVar("han_z", angpos.z);

								han.processExpressionsAfterHandleChange();
							}
							break;










						case AXHandle.HandleType.Position:

							// get to world space

							EditorGUI.BeginChangeCheck();
							Handles.color = han.color;
							Vector3 posNew = Handles.PositionHandle(pos, AXUtilities.QuaternionFromMatrix(consumerM));

							//Debug.Log("posNew=" + posNew);

							if (posNew == pos)
							{
								ArchimatixEngine.mouseDownOnSceneViewHandle();
								han.parametricObject.model.latestHandleClicked = han;
							}
							pos = posNew;


							if (EditorGUI.EndChangeCheck())
							{
								Undo.RegisterCompleteObjectUndo(parametricObject.model, "Position Handle");

								handleHasChanged = true;
								parametricObject.geometryControls.isOpen = true;
							}
							else
							{
								ArchimatixEngine.mouseDownOnSceneViewHandle();
								han.parametricObject.model.latestHandleClicked = han;
							}

							if (ArchimatixEngine.snappingOn())
								pos = AXGeometry.Utilities.SnapToGrid(pos, parametricObject.model.snapSizeGrid);

							// update these vars so math parsing has them as special variables...
							parametricObject.setVar("han_x", pos.x);
							parametricObject.setVar("han_y", pos.y);
							parametricObject.setVar("han_z", pos.z);



							break;

					}

					// set values for han_x, han_y, han_z, han_t and han_r... then process expression and ripple result from parameter


					// RIPPLE CHANGES IN OTHER PARAMETERS BASED ON HANDLE'S EXPRESSIONS
					// now set all the param=descrip items stored in the handle
					if (handleHasChanged && han.expressions != null)
					{
						Undo.RegisterCompleteObjectUndo(parametricObject.model, "Parameter Changed");

						for (int i = 0; i < han.expressions.Count; i++)
						{
							if (han.expressions[i] == "")
								continue;

							if (!String.IsNullOrEmpty(handleChangedVar))
							{
								// only use expressions that have the changed variable in them.
								if (!han.expressions[i].Contains(handleChangedVar))
									continue;
							}

							string expression = Regex.Replace(han.expressions[i], @"\s+", "");

							string paramName = expression.Substring(0, expression.IndexOf("="));
							string definition = expression.Substring(expression.IndexOf("=") + 1);
							//Debug.Log (param + " --- " + definition);

							try
							{
								//setParameterValueByName(param, (float) parseMath(definition));
								if (parametricObject.getParameter(paramName).Type == AXParameter.DataType.Int)
									parametricObject.initiateRipple_setIntValueFromGUIChange(paramName, Mathf.RoundToInt((float)parametricObject.parseMath_ValueOnly(definition)));

								else
									parametricObject.initiateRipple_setFloatValueFromGUIChange(paramName, (float)parametricObject.parseMath_ValueOnly(definition));


								SceneView.RepaintAll();
								//parametricObject.model.sw.milestone("intiateRipple_setIntValueFromGUIChange ");

							}
							catch (Exception e)
							{
								parametricObject.codeWarning = "10. Handle error: Please check syntax of: \"" + definition + "\" " + e.Message;
							}
						}
					}

					if (handleHasChanged)
					{
						//parametricObject.model.generate("Generator[base]::drawControlHandles");

						parametricObject.model.isAltered();
					}


				}

			}



			Handles.matrix = prevhandlesMatrix;
			Handles.color = prevColor;



		}




		#endregion





		public virtual void drawLineCube(Vector3 v1, Vector3 v2)
		{
			Color yellow = Color.green;
			yellow.a = .3f;
			drawLineCube(v1, v2, yellow);
		}

		public virtual void drawLineCube(Vector3 v1, Vector3 v2, Color c)
		{

			Vector3[] b = new Vector3[5];
			Vector3[] m = new Vector3[5];
			Vector3[] e = new Vector3[5];

			b[0] = v1;
			b[1] = new Vector3(v2.x, v1.y, v1.z);
			b[2] = new Vector3(v2.x, v1.y, v2.z);
			b[3] = new Vector3(v1.x, v1.y, v2.z);
			b[4] = v1;

			m[0] = new Vector3(v1.x, v2.y / 2, v1.z);
			m[1] = new Vector3(v2.x, v2.y / 2, v1.z);
			m[2] = new Vector3(v2.x, v2.y / 2, v2.z);
			m[3] = new Vector3(v1.x, v2.y / 2, v2.z);
			m[4] = new Vector3(v1.x, v2.y / 2, v1.z);

			e[0] = new Vector3(v1.x, v2.y, v1.z);
			e[1] = new Vector3(v2.x, v2.y, v1.z);
			e[2] = v2;
			e[3] = new Vector3(v1.x, v2.y, v2.z);
			e[4] = new Vector3(v1.x, v2.y, v1.z);




			Handles.color = c;

			Handles.DrawPolyLine(b);
			//Handles.DrawPolyLine(m);
			Handles.DrawPolyLine(e);

			Handles.DrawDottedLine(b[0], e[0], 5);
			Handles.DrawDottedLine(b[1], e[1], 5);
			Handles.DrawDottedLine(b[2], e[2], 5);
			Handles.DrawDottedLine(b[3], e[3], 5);


			/*
			Handles.DrawLine(new Vector3(v1.x, v1.y, 0), new Vector3(v1.x, v2.y, 0));
			Handles.DrawLine(new Vector3(v2.x, v1.y, 0), new Vector3(v2.x, v2.y, 0));
			Handles.DrawLine(new Vector3(0, v1.y, v1.z), new Vector3(0, v2.y, v1.z));
			Handles.DrawLine(new Vector3(0, v1.y, v2.z), new Vector3(0, v2.y, v2.z));
			*/
		}



		public virtual void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw = false)
		{
			if (parametricObject == null || !parametricObject.isActive)
				return;


			if (parametricObject.model.isSelected(parametricObject))
				drawLineCube(-parametricObject.bounds.extents, parametricObject.bounds.extents);
		}

		public virtual void drawTextureHandles(Matrix4x4 consumerM)
		{

		}





		public virtual void addContextMenuItems(GenericMenu menu)
		{

		}



		public virtual Color getGUIColor()
		{

			return Color.white;
		}











		public void drawCircleHandle(AXParametricObject po, string radiusName, float radiusValue, string segsName, int segsValue, float segsHandleScale = 1)
		{
			// TANGENT


			// get to world space


			Vector3 radpos = new Vector3(radiusValue, 0, 0);

			segsHandleScale *= HandleUtility.GetHandleSize(radpos);

			Vector3 tanpos = new Vector3(radiusValue, segsHandleScale * segsValue, 0);




			Color prevColor = Handles.color;

			// TANGENT
			Handles.color = Color.cyan;

			//tmpColor.a /= 3;

			Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radiusValue);

			Handles.DrawLine(radpos, tanpos);





			EditorGUI.BeginChangeCheck();
			tanpos = Handles.FreeMoveHandle(
				tanpos,
				Quaternion.identity,
				handleScale * HandleUtility.GetHandleSize(tanpos),
				Vector3.zero,
				(controlID, positione, rotation, size, type) =>
				{

					if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
					{
						ArchimatixEngine.mouseDownOnSceneViewHandle();
						parametricObject.model.latestHandleClicked = null;
						//ArchimatixEngine.selectedFreeCurve = gener;
					}
					Handles.SphereHandleCap(controlID, positione, rotation, size, type);
				});


			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(parametricObject.model, "Circle Handle");

				segsValue = Mathf.FloorToInt(tanpos.y / segsHandleScale);
				parametricObject.intValue(segsName, segsValue);
				parametricObject.model.isAltered(30);

			}




			Handles.color = Color.magenta;


			// RADIUS
			EditorGUI.BeginChangeCheck();
			radpos = Handles.FreeMoveHandle(
				radpos,
				Quaternion.identity,
				handleScale * HandleUtility.GetHandleSize(radpos),
				Vector3.zero,
				(controlID, positione, rotation, size, type) =>
				{

					if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
					{
						ArchimatixEngine.mouseDownOnSceneViewHandle();
						parametricObject.model.latestHandleClicked = null;
						//ArchimatixEngine.selectedFreeCurve = gener;
					}
					Handles.SphereHandleCap(controlID, positione, rotation, size, type);
				});
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(parametricObject.model, "Circle Handle");

				parametricObject.floatValue(radiusName, radpos.x);
				parametricObject.model.isAltered(29);
			}
			Handles.Label(radpos + Vector3.right * segsHandleScale * .5f, radpos.x.ToString("F2"), ArchimatixEngine.sceneViewLabelStyle);




			Handles.Label(tanpos + Vector3.up * segsHandleScale * .5f, segsValue.ToString(), ArchimatixEngine.sceneViewLabelStyle);

			Handles.color = prevColor;
		}










		/// <summary>
		/// Draws the paths.
		/// </summary>
		/// <param name="p">P.</param>

		public void drawPaths(AXParameter p)
		{



			if (p == null)
				return;

			//Debug.Log("p=" + p);

			Handles.BeginGUI();

			// Actual shape verts:						
			Paths actualPaths = p.getPaths();
			if (actualPaths != null)
			{
				foreach (Path path in actualPaths)
				{


					Vector3[] verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, (p.drawClosed || (p.shapeState == ShapeState.Closed)));

					Handles.DrawAAPolyLine(5, verts3d);
				}
			}

			Handles.EndGUI();


		}


		/// <summary>
		/// Draws the paths.
		/// </summary>
		/// <param name="p">P.</param>

		public void drawPathsForEditor(AXParameter p)
		{



			if (p == null)
				return;



			Handles.BeginGUI();

			// Actual shape verts:                      
			Paths actualPaths = p.getPaths();
			if (actualPaths != null)
			{
				foreach (Path path in actualPaths)
				{


					Vector3[] verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, (p.drawClosed || (p.shapeState == ShapeState.Closed)));

					for (int i = 0; i < verts3d.Length; i++)
						verts3d[i].z *= -1;
					Handles.DrawAAPolyLine(5, verts3d);
				}
			}

			Handles.EndGUI();


		}






		public static float snapValue(float dec, float inc)
		{
			var mod = (dec % inc);
			return dec - Math.Abs(mod) + (inc < 0 && mod != 0 ? Math.Abs(inc) : 0) + inc;
		}



	}



	public class GeneratorHandler2D : GeneratorHandler
	{

		private Color m_shapeColor;
		public Color shapeColor
		{
			get
			{
				return m_shapeColor;
			}
			set { m_shapeColor = value; }
		}




		// DRAW_TRANSFORM_HANDLES for a 2D Shape

		public override void drawTransformHandles(List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer = false)
		{

			if (visited.Contains("t_" + parametricObject.Guid))
				return;
			visited.Add("t_" + parametricObject.Guid);



			//	if (p == nfdiscull)
			//		return;



			//Debug.Log(parametricObject.getAxisRotationMatrix());

			AX.Generators.Generator2D gener2D = generator as AX.Generators.Generator2D;


			Matrix4x4 flipM = Matrix4x4.identity;
			if (generator.P_Output != null && generator.P_Output.flipX)
				flipM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));



			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;

			if (generator.hasOutputsConnected() || parametricObject.is2D())
				context *= generator.localMatrix.inverse * flipM;
			else
				context *= parametricObject.getAxisRotationMatrix().inverse * generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix();

			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			Handles.matrix = context;

			//Handles.matrix = flipM * Handles.matrix;

			Vector3 cen = AXUtilities.GetPosition(generator.localMatrix);



			//Debug.Log(Handles.matrix);

			// DRAW 2D CENTROID																											

			// Scale adjuster so handles are not too large in scaled up PO
			float handlesMatrixScaleAdjuster = 1.442f / AXUtilities.GetScale(Handles.matrix).magnitude;

			float handleRadius = 1.6f * handleScale * HandleUtility.GetHandleSize(cen) * handlesMatrixScaleAdjuster;
			float handlesScale = 0.8f * handleRadius;

			float crosshairRadius = 1.3f * handleRadius;

			Vector3 v1 = generator.localMatrix.MultiplyPoint3x4(new Vector3(crosshairRadius, 0, 0));
			Vector3 v2 = generator.localMatrix.MultiplyPoint3x4(new Vector3(0, crosshairRadius, 0));
			Vector3 nv1 = generator.localMatrix.MultiplyPoint3x4(new Vector3(-crosshairRadius, 0, 0));
			Vector3 nv2 = generator.localMatrix.MultiplyPoint3x4(new Vector3(0, -crosshairRadius, 0));

			Vector3 n = Vector3.Cross(v1 - nv1, v2 - nv2);

			Handles.color = new Color(.5f, .7f, 1f, 0.2f);




			Handles.DrawSolidArc(cen,
				n,
				v1 - cen,
				90,
				handlesScale);

			Handles.DrawSolidArc(cen,
				n,
				nv1 - cen,
				90,
				handlesScale);
			Handles.color = new Color(.5f, .7f, 1f, 0.8f);

			Handles.DrawLine(nv1, v1);
			Handles.DrawLine(nv2, v2);





			// POSITION HANDLE

			Handles.color = Color.cyan;

			if (ArchimatixEngine.sceneViewState != ArchimatixEngine.SceneViewState.AddPoint)
			{
				EditorGUI.BeginChangeCheck();

				cen = Handles.FreeMoveHandle(
					cen,
					Quaternion.identity,
					.3f * handleRadius,
					Vector3.zero,
					(controlID, positione, rotation, size, type) =>
					{

						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
						{
							ArchimatixEngine.mouseDownOnSceneViewHandle();
							parametricObject.model.latestHandleClicked = null;
							//ArchimatixEngine.selectedFreeCurve = gener;
						}
						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});

				// convert to local space
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(parametricObject.model, "Move");

					// ONLY USE LOCAL AXIS ROTATION IF FREE FLOATING PALETTE WITH NO DEPENDENTS
					//if (p.Dependents == null || p.Dependents.Count == 0)	

					if (ArchimatixEngine.optionClick)
					{
						//Debug.Log("Make new SHAPE!");
						ArchimatixEngine.optionClick = false;


						AXEditorUtilities.instancePO(parametricObject);
						//adjustparametricObject.model.selectPO(npo);

					}


					// lock movement of the cent in one axis, that of the axis orientation 

					//parametricObject.setParameterValueByName("Trans_X", cen.x);
					//parametricObject.setParameterValueByName("Trans_Y", cen.y);
					if (ArchimatixEngine.snappingOn())
						cen = AXGeometry.Utilities.SnapToGrid(cen, parametricObject.model.snapSizeGrid);

					parametricObject.initiateRipple_setFloatValueFromGUIChange("Trans_X", cen.x);
					parametricObject.initiateRipple_setFloatValueFromGUIChange("Trans_Y", cen.y);

					//parametricObject.model.generate("Generator2D::drawTransformHandles.trans_x");
					parametricObject.model.isAltered();

					//generator.adjustWorldMatrices();
				}
			}







			Color tmp = Handles.color;



			// ROTATION HANDLE
			Handles.color = Color.cyan;

			Quaternion rot = AXUtilities.QuaternionFromMatrix(generator.localMatrix);
			EditorGUI.BeginChangeCheck();

			Quaternion rot2 =
				Handles.Disc(rot,
					cen,
					n,
					handleRadius,
					false,
					1);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(parametricObject.model, "Rotation");

				float rot2z = (gener2D.flipX) ? -rot2.eulerAngles.z : rot2.eulerAngles.z;



				if (ArchimatixEngine.snappingOn())
					rot2z = snapValue(rot2z, 10);

				parametricObject.initiateRipple_setFloatValueFromGUIChange("Rot_Z", rot2z);

				//parametricObject.model.generate("Generator2D::drawTransformHandles.rot_z");
				parametricObject.model.isAltered();


				generator.adjustWorldMatrices();
			}


			Handles.color = tmp;


			Handles.matrix = prevHandlesMatrix;



		}








		#region Control Handels 2D
		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "GeneratorHandler2D"))
				return;


			Matrix4x4 prevHandlesMatrix = Handles.matrix;


			AXParameter p = parametricObject.generator.getPreferredOutputParameter();//.P_Output;

			if (p == null)
				return;

			Paths paths = p.getPaths();


			if (paths == null)
				return;


			//if (! skip)
			//	consumerType = ConsumerType.None;

			consumerM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;

			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;

			if (generator.hasOutputsConnected() || parametricObject.is2D())
				context *= generator.localMatrix.inverse;

			else
				context *= parametricObject.getAxisRotationMatrix().inverse * generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix();



			Handles.matrix = context;




			//Debug.Log(generator.parametricObject.Name + " AAA: " + Handles.matrix);


			Color col;
			if (beingDrawnFromConsumer == true)
				col = new Color(.5f, .5f, .5f, .2f);
			else
				col = Color.magenta;


			col.a *= .5f;
			Handles.color = col;



			Vector3[] verts3d;

			// DRAW THE SHAPE
			if ((!(generator is ShapeMerger)) && !visited.Contains("c_" + parametricObject.Guid))
			{
				//Debug.Log(parametricObject.Name + " -- --- --- ");

				// Since the paths points are transformed, we have to adjust the handles matrix
				//Handles.matrix = context;

				//if (! generator.hasOutputsConnected())
				//	Handles.matrix *= parametricObject.getAxisRotationMatrix2D();


				// unscaled and unoffset
				Handles.color = Color.gray;
				if (p.transformedButUnscaledOutputPaths != null)
				{
					for (int i = 0; i < p.transformedButUnscaledOutputPaths.Count; i++)
					{

						verts3d = AXGeometry.Utilities.path2Vec3s(p.transformedButUnscaledOutputPaths[i], Axis.Z, (p.shapeState == ShapeState.Closed));

						//Handles.DrawPolyLine(verts3d);
						Handles.DrawAAPolyLine(5, verts3d);
					}
				}



				Handles.color = Color.cyan;
				//Debug.Log("YO: "+p.transformedAndScaledButNotOffsetdOutputPaths);
				if (p.offset != 0 && p.transformedAndScaledButNotOffsetdOutputPaths != null)
				{
					for (int i = 0; i < p.transformedAndScaledButNotOffsetdOutputPaths.Count; i++)
					{
						verts3d = AXGeometry.Utilities.path2Vec3s(p.transformedAndScaledButNotOffsetdOutputPaths[i], Axis.Z, (p.shapeState == ShapeState.Closed));

						//Handles.DrawPolyLine(verts3d);
						Handles.DrawAAPolyLine(5, verts3d);
					}
				}

				/*
				Handles.color = Color.yellow;
				if (p.transformedFullyAndOffsetdButNotThickenedOutputPaths != null)
				{

					for (int i=0; i<p.transformedFullyAndOffsetdButNotThickenedOutputPaths.Count; i++)
					{
						verts3d = Archimatix.path2Vec3s(p.transformedFullyAndOffsetdButNotThickenedOutputPaths[i], Axis.Z, (p.shapeState == ShapeState.Closed));

						Handles.DrawPolyLine(verts3d);
					} 
				}
				*/


				/*
				foreach(Path path in paths)
				{
					verts3d = Archimatix.path2Vec3s(path, Axis.Z, (p.shapeState == ShapeState.Closed));

					if (Clipper.Orientation(path))
						Handles.color = Color.yellow;
					else
						Handles.color = Color.blue;
					Handles.DrawPolyLine(verts3d);
				}
				*/



				Handles.color = Color.magenta;
				// Actual shape verts:		

				foreach (Path path in p.getPaths())
				{

					verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, (p.drawClosed || (p.shapeState == ShapeState.Closed)));
					Handles.DrawAAPolyLine(12, verts3d);
				}


			}




			//Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;
			//Handles.matrix = prevHandlesMatrix;
			//Handles.matrix = context;
			//handleMatrix *= parametricObject.getTransMatrix2D();	

			// NOW DRAW THE HANDLES




			base.drawControlHandles(ref visited, context, beingDrawnFromConsumer);




			//if (visited.Contains ("c_"+parametricObject.Guid))
			//	return;

			Matrix4x4 flipM = Matrix4x4.identity;

			if (generator.P_Output != null && generator.P_Output.flipX)
				flipM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));


			Handles.matrix = context * flipM;




			//Debug.Log("----=> " +parametricObject.Name+ " " +generator.P_Output.Name+ ".flipX="+ generator.P_Output.flipX);



			// draw consumer origin CENTROID...
			Handles.color = Color.red;



			Vector3 vo = new Vector3(0, 0, 0);

			Vector3 vx = new Vector3(.5f, 0, 0);
			Vector3 vy = new Vector3(0, .5f, 0);
			Vector3 vz = new Vector3(0, 0, .5f);

			Handles.color = Handles.xAxisColor;
			Handles.DrawLine(vo, vx);
			Handles.DrawLine(vo, vx);
			Handles.DrawLine(vo, vx);

			Handles.color = Handles.yAxisColor;
			Handles.DrawLine(vo, vy);
			Handles.color = Handles.zAxisColor;
			Handles.DrawLine(vo, vz);
			Handles.DrawLine(vo, vz);
			Handles.DrawLine(vo, vz);


			Handles.color = Color.yellow;
			Handles.DrawDottedLine(new Vector3(0, 0, 0), generator.localMatrix.MultiplyPoint3x4(new Vector3(0, 0, 0)), 3);

			Handles.matrix = prevHandlesMatrix;





		}

		// GENERATOR_2D
		//		public override void drawHandlesUnselected()
		//		{
		//		}
		public override void drawHandlesUnselected()
		{
			if (!ArchimatixEngine.drawGuides)
				return;

			//AXParameter p = parametricObject.getParameter("Output Spline");
			AXParameter p = parametricObject.generator.P_Output;


			if (p == null || p.getPaths() == null)
				return;


			// MATRIX
			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			//Debug.Log(model);
			//Debug.Log(model.transform.localToWorldMatrix);
			//Debug.Log(parametricObject.getConsumerMatrix());


			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;


			if (generator.hasOutputsConnected() || parametricObject.is2D())
				context *= generator.localMatrix.inverse;
			else
				context *= parametricObject.getAxisRotationMatrix().inverse * generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix();


			Handles.matrix = context;





			// COLOR

			Vector3[] verts3d;
			Color prevColor = Handles.color;

			Color col = Color.cyan;
			col.a = .5f;

			Handles.color = col;


			// control shape verts:
			if (!p.drawClosed)
			{

				foreach (Path path in p.getPaths())//p.getTransformedControlPaths())
				{


					verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, (p.shapeState == ShapeState.Closed));
					Handles.DrawAAPolyLine(2, verts3d);
				}
			}
			else
			{
				// actual shape verts:						
				foreach (Path path in p.getPaths())
				{
					verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, (p.drawClosed || (p.shapeState == ShapeState.Closed)));
					//Handles.DrawPolyLine(verts3d);
					Handles.DrawAAPolyLine(2, verts3d);
				}
			}


			Handles.color = prevColor;
			Handles.matrix = prevHandlesMatrix;
		}




		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{

			//if (visited.Contains ("c_"+parametricObject.Guid))
			//	return;
			//visited.Add ("c_"+parametricObject.Guid);

			// by default, draw all of them here
			// Override this to treat particular inputs individually

			//Debug.Log("DC: " + parametricObject.Name);
			//foreach (AXParameter input_p in parametricObject.parameters)
			foreach (AXParameter input_p in parametricObject.getAllInputs())
			{

				if (input_p.PType == AXParameter.ParameterType.Input && input_p.DependsOn != null)
				{
					//Debug.Log(" -- " + input_p.Name + " -> " + input_p.DependsOn.parametricObject.Name);


					AXParametricObject src_po = input_p.DependsOn.parametricObject;

					if (parametricObject.is3D() && src_po.is2D())
						consumerM *= parametricObject.getAxisRotationMatrix2D(Axis.Y);

					consumerM = Matrix4x4.identity; //*= generator.getLocalConsumerMatrixPerInputSocket(src_po);

					GeneratorHandler gh = GeneratorHandler.getGeneratorHandler(src_po);
					if (gh != null)
						gh.drawControlHandles(ref visited, consumerM, true);

				}

			}

		}


		#endregion






		public override Color getGUIColor()
		{
			if (EditorGUIUtility.isProSkin)
				return Color.magenta;

			return new Color(.7f, .5f, .65f, .7f);
		}


	}

	public class GeneratorHandler3D : GeneratorHandler
	{



		// TRANSFORM HANDLES
		public override void drawTransformHandles(List<string> visited, Matrix4x4 consumerM, bool addLocalMatrix = false)
		{
			if (parametricObject == null || !parametricObject.isActive)
				return;



			if (visited.Contains("t_" + parametricObject.Guid))
				return;
			visited.Add("t_" + parametricObject.Guid);

			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * (parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix()).inverse * generator.localMatrix.inverse;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;
			Handles.matrix = context;

			Vector3 cen = AXUtilities.GetPosition(generator.localMatrix);
			Quaternion rot = AXUtilities.QuaternionFromMatrix(generator.localMatrix);


			switch (UnityEditor.Tools.current)
			{
				case UnityEditor.Tool.Move:

					EditorGUI.BeginChangeCheck();

					cen = Handles.PositionHandle(cen, rot);

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RegisterCompleteObjectUndo(parametricObject.model, "Translate");

						if (ArchimatixEngine.optionClick)
						{
							//Debug.Log("Make new PO!");
							ArchimatixEngine.optionClick = false;


							AXParametricObject npo = AXEditorUtilities.instancePO(parametricObject);
							parametricObject.model.selectPO(npo);

						}
						// convert to local space
						//cen = (consumerM*parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix()).inverse.MultiplyPoint3x4(cen);

						if (ArchimatixEngine.snappingOn())
							cen = AXGeometry.Utilities.SnapToGrid(cen, parametricObject.model.snapSizeGrid);


						parametricObject.initiateRipple_setFloatValueFromGUIChange("Trans_X", cen.x);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Trans_Y", cen.y);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Trans_Z", cen.z);


						//parametricObject.model.generate("Generator3D::drawTransformHandles.trans");
						parametricObject.model.isAltered();


						generator.parametricObject.worldDisplayMatrix = context * Matrix4x4.TRS(cen, rot, Vector3.one) * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix();
						generator.adjustWorldMatrices();
					}

					break;

				case Tool.Rotate:

					//Quaternion rot = AXUtilities.QuaternionFromMatrix(centerMatrix);

					EditorGUI.BeginChangeCheck();
					Quaternion rot2 = Handles.RotationHandle(rot, cen);
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RegisterCompleteObjectUndo(parametricObject.model, "Rotate");

						Vector3 rotAngs = rot2.eulerAngles;

						if (ArchimatixEngine.snappingOn())
							rotAngs = AXGeometry.Utilities.SnapToGrid(rotAngs, 10);

						parametricObject.initiateRipple_setFloatValueFromGUIChange("Rot_X", rotAngs.x);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Rot_Y", rotAngs.y);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Rot_Z", rotAngs.z);

						//parametricObject.model.generate("Generator3D::drawTransformHandles.rot");
						parametricObject.model.isAltered();


						generator.parametricObject.worldDisplayMatrix = context * Matrix4x4.TRS(cen, rot2, Vector3.one) * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix();
						generator.adjustWorldMatrices();

					}
					break;

				case Tool.Scale:

					cen = AXUtilities.GetPosition(generator.localMatrix);

					Vector3 scale = parametricObject.getLocalScale();

					EditorGUI.BeginChangeCheck();
					scale = Handles.ScaleHandle(scale, cen, rot, HandleUtility.GetHandleSize(cen));
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RegisterCompleteObjectUndo(parametricObject.model, "Scale");

						parametricObject.initiateRipple_setFloatValueFromGUIChange("Scale_X", scale.x);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Scale_Y", scale.y);
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Scale_Z", scale.z);

						//parametricObject.model.generate("Generator3D::drawTransformHandles.scale");
						parametricObject.model.isAltered();


						generator.adjustWorldMatrices();

					}
					break;
			}










			// bounds handles

			drawBoundsHandles(generator.localMatrix);


			// Texture Handles
			//if (parametricObject.Mat != null)
			//	drawTextureHandles(consumerM);


			Handles.matrix = prevHandlesMatrix;

		}






		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "GeneratorHandler3D"))
				return;

			// getTransMatrix translates then rotates
			/*
			if (beingDrawnFromConsumer)
				thisM *= generator.parametricObject.getLocalMatrix();
			else	
			{	
				consumerM *= generator.parametricObject.getLocalMatrix().inverse;
			}
			*/
			if (parametricObject == null || !parametricObject.isActive)
				return;



			if (beingDrawnFromConsumer)
			{
				//Debug.Log("Here: " + generator.parametricObject.Name);	
				consumerM *= parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix();
			}
			else
			{
				//Debug.Log(parametricObject.Name+" add local");
				consumerM *= parametricObject.getLocalMatrix();

			}





			base.drawControlHandles(ref visited, consumerM, true);// beingDrawnFromConsumer);



		}







		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw = false)
		{

			//if (!ArchimatixEngine.drawGuides)
			//    return;


			if (parametricObject == null || !parametricObject.isActive)
				return;

			if (!forceDraw && !parametricObject.model.isSelected(parametricObject))
				return;




			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;//  consumerM * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix() ;

			Color prevc = Handles.color;
			Handles.color = new Color(.9f, .9f, 1f, .3f);
			Handles.color = new Color(.3f, .3f, 3f, .5f);







			//Bounds amalgamBounds = parametricObject.getBoundsAdjustedForAxis();
			Bounds amalgamBounds = parametricObject.bounds;
			Vector3 amalgamCenter = parametricObject.bounds.center;


			// BOUNDING BOX 
			if (ArchimatixEngine.drawGuides)
				drawLineCube(amalgamCenter - amalgamBounds.extents, amalgamCenter + amalgamBounds.extents);


			Handles.matrix *= Matrix4x4.TRS(new Vector3(amalgamCenter.x, 0, amalgamCenter.z), Quaternion.identity, Vector3.one);

			float handlesMatrixScaleAdjuster = AXEditorUtilities.getHandlesMatrixScaleAdjuster();


			// BINDING BOX

			// Is differnt from Bounding Box which is based on where the objects are.
			// BindingBox is a control volume which may not align with where the objects end up.


			Handles.color = Color.green;
			AXEditorUtilities.drawCrosshair(amalgamCenter);


			Handles.color = Color.cyan;


			float leverscale = 2 * handleScale;

			float lever;



			AXParameter P_X = generator.P_BoundsX;
			AXParameter P_Y = generator.P_BoundsY;
			AXParameter P_Z = generator.P_BoundsZ;

			bool doX = (P_X != null && (!(generator is Grouper) || P_X.hasRelations() || P_X.hasExpressions()));
			bool doY = (P_Y != null && (!(generator is Grouper) || P_Y.hasRelations() || P_Y.hasExpressions()));
			bool doZ = (P_Z != null && (!(generator is Grouper) || P_Z.hasRelations() || P_Z.hasExpressions()));



			float bx = doX ? P_X.FloatVal / 2 : generator.parametricObject.bounds.size.x / 2;
			float by = doY ? P_Y.FloatVal : generator.parametricObject.bounds.size.y;
			float bz = doZ ? P_Z.FloatVal / 2 : generator.parametricObject.bounds.size.z / 2;



			if (P_X != null && P_Y != null && P_Z != null)
			{
				Vector3 v1 = new Vector3(-bx, 0, -bz);
				Vector3 v2 = new Vector3(bx, by, bz);

				drawLineCube(v1, v2, Color.cyan);

			}

			// X_HANDLE


			if (doX)
			{
				// xAxisBinding

				//Vector3 posx = new Vector3(center.x+b.extents.x, center.y, center.z);
				Vector3 posx = new Vector3(P_X.FloatVal / 2, parametricObject.bounds.center.y, 0);

				lever = leverscale * HandleUtility.GetHandleSize(posx) * handlesMatrixScaleAdjuster;

				Handles.color = new Color(Handles.xAxisColor.r, Handles.xAxisColor.g, Handles.xAxisColor.b, .5f);
				Handles.DrawSolidDisc(posx, Vector3.right, lever / 4);
				Handles.DrawLine(posx, posx + new Vector3(lever, 0, 0));
				Handles.DrawLine(posx, posx + new Vector3(lever, 0, 0));
				Handles.DrawLine(posx, posx + new Vector3(lever, 0, 0));


				posx.x += lever;

				EditorGUI.BeginChangeCheck();
				posx = Handles.FreeMoveHandle(
					posx,
					Quaternion.identity,
					handleScale * HandleUtility.GetHandleSize(posx),
					Vector3.zero,
					(controlID, positione, rotation, size, type) =>
					{
						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
						{
							ArchimatixEngine.mouseDownOnSceneViewHandle();
							parametricObject.model.latestHandleClicked = null;
						}


						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});



				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(parametricObject.model, "Bounds Handles");
					//parametricObject.propagateParameterByBinding(Axis.X, 2*(posx.x-center.x-lever-parametricObject.margin.x/2));

					if (ArchimatixEngine.snappingOn())
						posx.x = AXGeometry.Utilities.SnapToGrid(posx.x, parametricObject.model.snapSizeGrid) + lever;


					P_X.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(2 * (posx.x - lever));

					//parametricObject.model.generate ("Generator3D::drawBoundsHandles.x");
					parametricObject.model.isAltered();

					generator.adjustWorldMatrices();
				}
			}



			// Y AXIS

			if (doY)
			{

				//Vector3 posy = new Vector3(center.x, center.y+b.extents.y, center.z);
				Vector3 posy = new Vector3(0, P_Y.FloatVal, 0);

				lever = leverscale * HandleUtility.GetHandleSize(posy) * handlesMatrixScaleAdjuster;

				Handles.color = new Color(Handles.yAxisColor.r, Handles.yAxisColor.g, Handles.yAxisColor.b, 1f);
				Handles.DrawSolidDisc(posy, Vector3.up, lever / 4);
				Handles.DrawLine(posy, posy + new Vector3(0, lever, 0));
				Handles.DrawLine(posy, posy + new Vector3(0, lever, 0));
				Handles.DrawLine(posy, posy + new Vector3(0, lever, 0));


				posy.y += lever;
				EditorGUI.BeginChangeCheck();
				posy = Handles.FreeMoveHandle(
					posy,
					Quaternion.identity,
					handleScale * HandleUtility.GetHandleSize(posy),
					Vector3.zero,
					(controlID, positione, rotation, size, type) =>
					{
						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
						{
							ArchimatixEngine.mouseDownOnSceneViewHandle();
							parametricObject.model.latestHandleClicked = null;
						}
						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(parametricObject.model, "Bounds Handles");

					//parametricObject.propagateParameterByBinding(Axis.Y, (posy.y-lever-2*parametricObject.margin.y/2));

					if (ArchimatixEngine.snappingOn())
						posy.y = AXGeometry.Utilities.SnapToGrid(posy.y, parametricObject.model.snapSizeGrid) + lever;

					//Debug.Log(posy.y + " ... " + lever);
					P_Y.initiatePARAMETER_Ripple_setFloatValueFromGUIChange((posy.y - lever));

					//parametricObject.model.generate ("Generator3D::drawBoundsHandles.y");
					parametricObject.model.isAltered();

					generator.adjustWorldMatrices();
				}

			}


			// Z-AXIS
			if (doZ)
			{
				//Vector3 posz = new Vector3(center.x, center.y, center.z+b.extents.z);
				Vector3 posz = new Vector3(0, parametricObject.bounds.center.y, P_Z.FloatVal / 2);

				lever = leverscale * HandleUtility.GetHandleSize(posz) * handlesMatrixScaleAdjuster;

				Handles.color = new Color(Handles.zAxisColor.r, Handles.zAxisColor.g, Handles.zAxisColor.b, 1f);
				Handles.DrawSolidDisc(posz, Vector3.forward, lever / 4);
				Handles.DrawLine(posz, posz + new Vector3(0, 0, lever));
				Handles.DrawLine(posz, posz + new Vector3(0, 0, lever));
				Handles.DrawLine(posz, posz + new Vector3(0, 0, lever));


				posz.z += lever;
				EditorGUI.BeginChangeCheck();
				posz = Handles.FreeMoveHandle(
					posz,
					Quaternion.identity,
					handleScale * HandleUtility.GetHandleSize(posz),
					Vector3.zero,
					(controlID, positione, rotation, size, type) =>
					{
						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
						{
							ArchimatixEngine.mouseDownOnSceneViewHandle();
							parametricObject.model.latestHandleClicked = null;
						}
						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(parametricObject.model, "Bounds Handles");
					//Debug.Log("===========================================================");

					//parametricObject.propagateParameterByBinding(Axis.Z, 2*(posz.z-center.z-lever-parametricObject.margin.z/2));

					if (ArchimatixEngine.snappingOn())
						posz.z = AXGeometry.Utilities.SnapToGrid(posz.z, parametricObject.model.snapSizeGrid) + lever;

					P_Z.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(2 * (posz.z - lever));
					parametricObject.model.isAltered();// generate ("Generator3D::drawBoundsHandles.z");

					generator.adjustWorldMatrices();

					HandleUtility.Repaint();
				}

			}


			Handles.Label(amalgamCenter + amalgamBounds.extents + Vector3.up * 3f, parametricObject.Name);




			Handles.color = prevc;
			Handles.matrix = prevHandlesMatrix;


		}





		// DRAW_TEXTURE_HANDLES
		public override void drawTextureHandles(Matrix4x4 consumerM)
		{
			if (parametricObject == null || !parametricObject.isActive)
				return;


			if (!parametricObject.model.isSelected(parametricObject))
				return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;


			Matrix4x4 centerMatrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * parametricObject.getLocalTransformMatrix();
			Handles.matrix = centerMatrix;


			//Handles.matrix = consumerM * generator.getLocalAlignMatrix() * parametricObject.getAxisRotationMatrix();




			float uShift = parametricObject.floatValue("uShift");
			float vShift = parametricObject.floatValue("vShift");

			float uScale = parametricObject.floatValue("uScale");
			float vScale = parametricObject.floatValue("vScale");

			float shiftScale = .5f;


			Vector3 texcen = new Vector3(1.7f * parametricObject.bounds.extents.x, -vShift / shiftScale, uShift / shiftScale);
			//Vector3 texcenZ = new Vector3(1.3f*parametricObject.bounds.extents.x, 2, 0);

			//Vector3 texcen = centerMatrix.MultiplyPoint3x4(texcenLocal);


			float side = handleScale * HandleUtility.GetHandleSize(texcen);
			Vector3[] verts = new Vector3[4];

			verts[0] = new Vector3(texcen.x, texcen.y - side, texcen.z - side);
			verts[1] = new Vector3(texcen.x, texcen.y + side, texcen.z - side);
			verts[2] = new Vector3(texcen.x, texcen.y + side, texcen.z + side);
			verts[3] = new Vector3(texcen.x, texcen.y - side, texcen.z + side);

			Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.2f), new Color(0, 0, 0, 1));

			Vector3[] verts_UR = new Vector3[4];
			verts_UR[0] = new Vector3(texcen.x, texcen.y, texcen.z);
			verts_UR[1] = new Vector3(texcen.x, texcen.y + side, texcen.z);
			verts_UR[2] = new Vector3(texcen.x, texcen.y + side, texcen.z + side);
			verts_UR[3] = new Vector3(texcen.x, texcen.y, texcen.z + side);

			//Handles.color = Color.black;
			Handles.DrawSolidRectangleWithOutline(verts_UR, new Color(0, 0, 0, 0.3f), new Color(0, 0, 0, 1));

			Vector3[] verts_LL = new Vector3[4];
			verts_LL[0] = new Vector3(texcen.x, texcen.y - side, texcen.z - side);
			verts_LL[1] = new Vector3(texcen.x, texcen.y, texcen.z - side);
			verts_LL[2] = new Vector3(texcen.x, texcen.y, texcen.z);
			verts_LL[3] = new Vector3(texcen.x, texcen.y - side, texcen.z);

			//Handles.color = Color.black;
			Handles.DrawSolidRectangleWithOutline(verts_LL, new Color(0, 0, 0, 0.3f), new Color(0, 0, 0, 1));










			Quaternion yrot = Quaternion.Euler(-90, 0, 0);

			// Shifting
			EditorGUI.BeginChangeCheck();

			if (UnityEditor.Tools.current == UnityEditor.Tool.Move)
			{



				// U
				Handles.color = Handles.xAxisColor;
				texcen = Handles.Slider(texcen, Vector3.forward);
				parametricObject.setParameterValueByName("uShift", shiftScale * texcen.z);

				// V
				Handles.color = Handles.yAxisColor;
				texcen = Handles.Slider(texcen, Vector3.up);
				parametricObject.setParameterValueByName("vShift", -shiftScale * texcen.y);
			}



			// Scaling
			if (UnityEditor.Tools.current == UnityEditor.Tool.Scale)
			{


				Handles.color = Handles.yAxisColor;

				uScale = Handles.ScaleSlider(uScale,
					texcen,
					Vector3.forward,
					Quaternion.identity,
					handleScale * HandleUtility.GetHandleSize(texcen),
					.005f);
				parametricObject.setParameterValueByName("uScale", uScale);


				Handles.color = Handles.xAxisColor;

				vScale = Handles.ScaleSlider(vScale,
					texcen,
					Vector3.up,
					yrot,
					handleScale * HandleUtility.GetHandleSize(texcen),
					.005f);
				parametricObject.setParameterValueByName("vScale", vScale);

			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(parametricObject.model, "Material");

				//parametricObject.model.generate("Generator3D::drawTextureHandles");
				parametricObject.model.isAltered();

			}



			Handles.matrix = prevHandlesMatrix;

		}


	}
}
