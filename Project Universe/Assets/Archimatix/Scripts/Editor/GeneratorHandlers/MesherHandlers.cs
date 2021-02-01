#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;
using AXEditor;
using AX.Generators;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;



namespace AX.GeneratorHandlers
{


	public class PolygonHandler : GeneratorHandler3D {



		 
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			Polygon gener = (generator as Polygon);

			// Draw the plan spline.

			if (gener != null && gener.planSrc_po != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);
				if (gh != null)
				{

					Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
				
					localSecM *= parametricObject.getAxisRotationMatrix2D(Axis.Y);

					gh.drawControlHandles(ref visited, localSecM, true);


					if (gener.planSrc_po.is2D())
						gh.drawTransformHandles(visited, localSecM, true);

				}
			} 
		}
	}
	 




	// EXTRUDE_HANDLER
	
	public class ExtrudeHandler : GeneratorHandler3D {

		


//		public override int customNodeGUIZone_2(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po)
//		{
//
//			int     gap         = ArchimatixUtils.gap;
//			int     lineHgt     = ArchimatixUtils.lineHgt;
//			float     winMargin     = ArchimatixUtils.indent;
//			float     innerWidth     = po.rect.width - 2*winMargin;
//
//			Rect pRect = new Rect(winMargin, cur_y, innerWidth, lineHgt);
//			EditorGUI.TextField(pRect, "YUBBA");
//			//GUI.Button(pRect, "HALLO");
//
//			cur_y += lineHgt;
//
//			return cur_y;
//		}



		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
		 

			Extrude gener = (generator as Extrude);



			if (gener.P_Plan == null || gener.planSrc_p == null)
				return;

			if (gener.parametricObject == null || ! gener.parametricObject.isActive)
				return;




			GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);
			if (gh != null)
			{
				

				Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);

				 
				gh.drawControlHandles(ref visited, localSecM, true);




				if (gener.planSrc_po != null && gener.planSrc_po.is2D() || gener.planSrc_po.generator is Grouper)
					gh.drawTransformHandles(visited, localSecM, true);


	 

				// BEVEL HANDLE
				Matrix4x4 prevHM = Handles.matrix;
				Color prevColor = Handles.color;

				Handles.matrix =  parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * AXGeometry.Utilities.getFirstSegmentHandleMatrix(gener.P_Plan.getPaths());

				float handlesMatrixScaleAdjuster = AXEditorUtilities.getHandlesMatrixScaleAdjuster();


				if (! gener.bevelOut)
				{
					//float bevelMax = (gener.bevelTop > gener.bevelBottom) ? gener.bevelTop : gener.bevelBottom;
					//Handles.matrix *= Matrix4x4.TRS(new Vector3(-bevelMax, 0, 0), Quaternion.identity, Vector3.one);
				}
				float x = 0; //(gener.bevelOut) ? gener.bevelTop : 0;

				x += gener.P_Plan.thickness + gener.P_Plan.offset;




				Handles.matrix *= Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), Vector3.one); 





				Vector3 posR1 = new Vector3( (gener.bevelBottom > gener.bevelTop )? 0 :gener.bevelTop-gener.bevelBottom,     gener.bevelBottom,     0);

				posR1.x += gener.P_Plan.offset;



				float r2X = (gener.bevelBottom > gener.bevelTop) ? (gener.bevelBottom-gener.bevelTop-gener.taper) : -gener.taper;

				Vector3 posR2 = new Vector3( r2X, (gener.extrude - gener.bevelTop), 0);
				posR2.x += gener.P_Plan.offset;


				// WIRE DISCS
				Handles.color = Color.red;
				Handles.DrawWireDisc(posR1, Vector3.forward,  gener.bevelBottom);
				Handles.DrawWireDisc(posR2, Vector3.forward,  gener.bevelTop);
				Handles.color = Color.green;



				// BEVEL RADIUS 1

				// R1
				EditorGUI.BeginChangeCheck();
				posR1 = Handles.FreeMoveHandle(
					posR1, 
					Quaternion.identity,
					.1f*HandleUtility.GetHandleSize(posR1),
					Vector3.zero, 
					(controlID, positione, rotation, size, type) =>
					{
						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
							ArchimatixEngine.mouseDownOnSceneViewHandle();
						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});


				if(EditorGUI.EndChangeCheck())
				{
					

					parametricObject.initiateRipple_setFloatValueFromGUIChange("Bevel Bottom", posR1.y);

					if (Event.current.alt && gener.bevelsUnified)
						gener.P_Bevels_Unified.boolval = false;


					// OFFSET
					float ox = posR1.x;

					if (gener.bevelBottom < gener.bevelTop)
						ox -= (gener.bevelTop-gener.bevelBottom);

					gener.P_Plan.offset = (Math.Abs(ox) < (.08f)*HandleUtility.GetHandleSize(posR1)) ? 0 : ox;


					parametricObject.model.isAltered(32);
				}




				// R2 & TAPER

				EditorGUI.BeginChangeCheck();

				posR2 = Handles.FreeMoveHandle(
					posR2, 
					Quaternion.identity,
					.1f*HandleUtility.GetHandleSize(posR2) ,
					Vector3.zero,							
					(controlID, positione, rotation, size, type) =>
					{
						if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
							ArchimatixEngine.mouseDownOnSceneViewHandle();
						Handles.SphereHandleCap(controlID, positione, rotation, size, type);
					});
				
				if(EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo (parametricObject.model, "Bevel Radius Changed");

					if (Event.current.alt)
					{
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Extrude", "Height", (gener.bevelTop + posR2.y) );
						gener.taper = (gener.bevelBottom > gener.bevelTop) ?  gener.bevelBottom - gener.bevelTop -posR2.x : -posR2.x;

						gener.taper = (Math.Abs(gener.taper) < (.08f)*HandleUtility.GetHandleSize(posR2) * handlesMatrixScaleAdjuster) ? 0 : gener.taper;

						parametricObject.initiateRipple_setFloatValueFromGUIChange("Taper", gener.taper+gener.P_Plan.offset );
					}
					else
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Bevel Top", (gener.extrude - posR2.y) );



					//parametricObject.model.latestEditedParameter = gener.P_Taper;
					parametricObject.model.isAltered(32);
				}







				/*
				EditorGUI.BeginChangeCheck();
				pos1 = Handles.Slider(pos1, Vector3.down,  .6f*HandleUtility.GetHandleSize(pos1), Handles.ArrowCap, 0);

				if(EditorGUI.EndChangeCheck())
				{
					parametricObject.intiateRipple_setFloatValueFromGUIChange("Bevel Radius Bottom", pos1.y);
					parametricObject.model.isAltered(32);
				}
				*/
				Handles.color = Color.cyan;
 


 /*
				// TAPER
				EditorGUI.BeginChangeCheck();
				//Vector3 taperV =  Handles.Slider(new Vector3(-gener.taper, gener.extrude-gener.bevelTop, 0), Vector3.right,  .6f*HandleUtility.GetHandleSize(pos1), Handles.ArrowCap, 0);

				Vector3 taperV =  Handles.Slider(new Vector3(gener.P_Plan.offset, gener.extrude-gener.bevelTop, 0), Vector3.right,  .6f*HandleUtility.GetHandleSize(pos1), Handles.ArrowCap, 0);
				if(EditorGUI.EndChangeCheck())
				{


					// TAPER
					//float px = (Math.Abs(taperV.x) < (.08f)*HandleUtility.GetHandleSize(posR2)) ? 0 : -taperV.x;

					Undo.RegisterCompleteObjectUndo (parametricObject.model, "Offset parameter changed");

					float px = (Math.Abs(taperV.x) < (.08f)*HandleUtility.GetHandleSize(posR2)) ? 0 : taperV.x;
					//parametricObject.intiateRipple_setFloatValueFromGUIChange("Taper", px);
					//parametricObject.intiateRipple_setFloatValueFromGUIChange("Taper", px);
					gener.P_Plan.offset = px;

					parametricObject.model.isAltered(32);


				}
			*/

			/*
				// BEVEL_RADIUS_1 ANGLED
				float c45 = .70710678f;
				EditorGUI.BeginChangeCheck();
				Vector3 bevel2V =  Handles.Slider(new Vector3(-gener.taper - (gener.bevelTop*c45),    gener.extrude-gener.bevelTop - (gener.bevelTop*c45),   0), new Vector3(-1, -1, 0),  .6f*HandleUtility.GetHandleSize(pos1), Handles.ArrowCap, 0);
				if(EditorGUI.EndChangeCheck())
				{
					
					float px = (-bevel2V.x - gener.taper)/c45;
					parametricObject.intiateRipple_setFloatValueFromGUIChange("Bevel Radius Top", px);
					parametricObject.model.isAltered(32);
				}
				*/

				/*

				// BEVEL TOP RADIUS
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("Bevel Top Radius");
				Vector3 r2V =  Handles.Slider(new Vector3(-gener.taper, gener.extrude-gener.bevelTop, 0), Vector3.up,  .6f*HandleUtility.GetHandleSize(pos1), Handles.ArrowCap, 0);
				//Vector3 r2V =  Handles.Slider(new Vector3(-gener.taper, gener.extrude-gener.bevelTop, 0), Vector3.up);
				if(EditorGUI.EndChangeCheck())
				{
					// TAPER
					parametricObject.intiateRipple_setFloatValueFromGUIChange("Bevel Radius Top", gener.extrude-r2V.y);
					parametricObject.model.isAltered(32);


				}

				*/

				// BEVEL SEGS

				if (gener.bevelTop > 0)
				{
					float handleSize = .55f*HandleUtility.GetHandleSize(posR1)  * handlesMatrixScaleAdjuster;

					float adjustedSegs = (gener.bevelHardEdge && gener.bevelSegs == 1) ? 0 : gener.bevelSegs;


					//Vector3 posSegs = new Vector3(-gener.taper,      gener.extrude - gener.bevelTop +handleSize + (.025f*HandleUtility.GetHandleSize(posR1)* gener.bevelSegs), 0);
					Vector3 posSegs = new Vector3(-gener.taper + handleSize + (.04f*HandleUtility.GetHandleSize(posR1) * adjustedSegs  * handlesMatrixScaleAdjuster),      gener.extrude - gener.bevelTop , 0);

					Handles.DrawLine(posR2, posSegs);

					EditorGUI.BeginChangeCheck();
					posSegs = Handles.FreeMoveHandle(
						posSegs, 
						Quaternion.identity,
						.1f*HandleUtility.GetHandleSize(posSegs) ,
						Vector3.zero, 
						(controlID, positione, rotation, size, type) =>
						{
							if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
								ArchimatixEngine.mouseDownOnSceneViewHandle();
							Handles.SphereHandleCap(controlID, positione, rotation, size, type);
						});



					if(EditorGUI.EndChangeCheck())
					{
						// BEVEL_TOP_RADIUS
						//int conv =  Mathf.CeilToInt(   (posSegs.y + gener.bevelTop - gener.extrude - handleSize) / (.025f*HandleUtility.GetHandleSize(posR1)));
						Undo.RegisterCompleteObjectUndo (parametricObject.model, "Bevel Segs Changed");



						int conv =  Mathf.CeilToInt(   (posSegs.x + gener.taper - handleSize) / (.04f*HandleUtility.GetHandleSize(posR2)));

						if (conv <= 0)
						{
							conv = 1;

							gener.bevelHardEdge = true;
							gener.P_BevelHardEdge.boolval =  true;
						}
						else 
						{
							gener.bevelHardEdge = false;
							gener.P_BevelHardEdge.boolval =  false;

						}


						if (gener.bevelTop == 0)
						{
						parametricObject.initiateRipple_setFloatValueFromGUIChange("Bevel Segs", .02f);

						}
						parametricObject.initiateRipple_setIntValueFromGUIChange("Bevel Segs", conv);
						parametricObject.model.isAltered(32);
					} 
				}

			
				

	

				/*

				// LIP_TOP
				Handles.color = Color.magenta;

				EditorGUI.BeginChangeCheck();
				Vector3 lipTopV =  Handles.Slider(new Vector3(-gener.taper-gener.lipTop, gener.extrude, 0), Vector3.left,  .6f*HandleUtility.GetHandleSize(posR2), Handles.ArrowCap, 0);
				if(EditorGUI.EndChangeCheck())
				{
					parametricObject.intiateRipple_setFloatValueFromGUIChange("Lip Top", -lipTopV.x-gener.taper);
					parametricObject.model.isAltered(32);
				}

				// LIP_EDGE
				EditorGUI.BeginChangeCheck();
				lipTopV =  Handles.Slider(new Vector3(-gener.taper-gener.lipTop, gener.extrude-gener.lipEdge, 0), Vector3.down,  .6f*HandleUtility.GetHandleSize(posR2), Handles.ArrowCap, 0);
				if(EditorGUI.EndChangeCheck())
				{
					parametricObject.intiateRipple_setFloatValueFromGUIChange("Lip Edge", -lipTopV.y+gener.extrude);
					parametricObject.model.isAltered(32);
				}

				*/


				
				//drawBevelHandle(parametricObject, "Bevel Radius Top", gener.bevelTop, "Taper", gener.taper, .06f);

				Handles.matrix = prevHM;
				Handles.color = prevColor;
				
			} 
		}
	}






	// LATHE_HANDLER
	
	public class LatheHandler : GeneratorHandler3D 
	{
			
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			Lathe gener = (generator as Lathe);

			//if (visited.Contains ("c_"+parametricObject.Guid))
			//	return;
			//visited.Add ("c_"+parametricObject.Guid);


			// SECTION
			if (  gener.sectionSrc_po != null && gener.sectionSrc_po.generator != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener.sectionSrc_po);
				
				if (gh != null)
				{


					Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.getLocalConsumerMatrixPerInputSocket(gener.sectionSrc_po);
					if (gener.sectionSrc_po.is2D())
						gh.drawTransformHandles(visited, localSecM, true);
					gh.drawControlHandles(ref visited, localSecM, true);
					
				}
			}



			// PLAN

			Matrix4x4 prevHandlesMatrix = Handles.matrix;
		


			float y = .5f*HandleUtility.GetHandleSize(Vector3.zero);
			float yd = 0.5f*HandleUtility.GetHandleSize(Vector3.zero) + .2f*parametricObject.bounds.extents.y;
			float handleSizer = .15f*HandleUtility.GetHandleSize(Vector3.zero);

			Handles.matrix *= Matrix4x4.TRS (new Vector3 (0, -yd, 0), Quaternion.identity, Vector3.one);

			Vector3 pos;

			Color lightLineColor = new Color (1, .8f, .6f, .7f);
			Color brightOrange 	 = new Color(1, .8f, 0, .9f);


			// RADIUS //

			Handles.color = brightOrange;

			//Handles.DrawLine(new Vector3(gener.radius, y, 0), new Vector3(gener.radius, -y, 0));
			Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(0, -y, 0));

			//Handles.DrawLine(new Vector3(0, -y*.66f, 0), new Vector3(gener.radius, -y*.66f, 0));

			// RADIUS LABEL
			GUIStyle labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;

			Handles.Label(new Vector3(gener.radius+handleSizer, -y/4, 0), "rad="+System.Math.Round(gener.radius, 2), labelStyle);


			// RADIUS HANDLE
			pos = new Vector3(gener.radius, 0, 0);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
				Vector3.zero, 
				(controlID, positione, rotation, size, type) =>
				{
					if (GUIUtility.hotControl > 0 && controlID == GUIUtility.hotControl)
						ArchimatixEngine.mouseDownOnSceneViewHandle();
					Handles.SphereHandleCap(controlID, positione, rotation, size, type);
				});


			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Radius");
				//.parametricObject.setAltered();
				gener.P_Radius.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( pos.x );
				gener.radius = pos.x;

				gener.parametricObject.model.isAltered(23);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();

				generator.adjustWorldMatrices();
			}



			Handles.color = new Color (1, .8f, .6f, .05f);

			Handles.DrawSolidArc(Vector3.zero, 
				Vector3.up, 
				Vector2.right, 
				-gener.sweepAngle, 
				gener.radius + .2f);

			


			Handles.color = lightLineColor;
			Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(gener.radius, 0, 0));



















			// TOTAL ANGLE SWEEP
			Handles.color = new Color (1, .5f, 0f, .3f);

			Quaternion rot = Quaternion.Euler (0, 360-gener.sweepAngle, 0);
			EditorGUI.BeginChangeCheck();
			rot = Handles.Disc(rot, Vector3.zero, Vector3.up, gener.radius, false, 1);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "RadialRepeat Total Angle");


				gener.P_SweepAngle.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( 360-rot.eulerAngles.y );

				gener.parametricObject.model.isAltered(23);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();

			}
			Handles.color = new Color (1, .5f, 0f, 1f);

			Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.right, (-gener.sweepAngle), gener.radius);


			Handles.matrix *= Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 360 - gener.sweepAngle, 0), Vector3.one);





			// SEGMENT CLICKERS

			// (-)

			/*
			Handles.color = new Color(.7f, .7f, 1, .9f);
			pos = new Vector3(gener.radius, 0, -handleSizer*2);
			if(Handles.Button(pos, Quaternion.Euler(0,180,0), handleSizer, handleSizer, Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Segs");

				gener.P_Segs.intiateRipple_setIntValueFromGUIChange(gener.segs-1);

				gener.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			// [+]
			Handles.color = new Color(.7f, 1f, .7f, .9f);
			pos = new Vector3(gener.radius, 0, handleSizer*2);
			if(Handles.Button(pos, Quaternion.Euler(0,0,0), handleSizer, handleSizer, Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Segs");

				gener.P_Segs.intiateRipple_setIntValueFromGUIChange(gener.segs+1);

				gener.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			Handles.Label(new Vector3(gener.radius,  -y/4, -handleSizer*4), ""+(gener.segs) + " segs", labelStyle);
			*/


			// ANGLESWEEP KNOB

			/*
			Handles.color = brightOrange;
			Handles.SphereHandleCap(0,
				new Vector3(gener.radius, 0, 0),
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos));
			*/

			Handles.Label(new Vector3(0, -y, 0), ""+System.Math.Round(gener.snappedSweepAngle, 0)+" degs", labelStyle);








			Handles.matrix = prevHandlesMatrix;

		}
		
		
		
	}





    public class WinWallHandler : GeneratorHandler3D
    {

        // WIN_WALLP :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
        public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
        {

            WinWall gener = (generator as WinWall);
            // Draw the plan AND the section splines.

            // PLAN first, since the section needs plan information to position itself.
            if (gener.planSrc_po != null && gener.planSrc_po.generator != null)
            {
                // Get the input PO and draw
                GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
                    Matrix4x4 localPlanM = Matrix4x4.identity;

                    if (gener.planSrc_po.is2D())
                        gh.drawTransformHandles(visited, localPlanM, true);
                    gh.drawControlHandles(ref visited, localPlanM, true);
                }
            }

            // SECTION

            //if (gener.sectionSrc_po != null && gener.sectionSrc_po.generator != null)
            //{

            //    GeneratorHandler gh = getGeneratorHandler(gener.sectionSrc_po);

            //    if (gh != null)
            //    {
            //        //Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix.inverse * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.sectionSrc_po);
            //        Matrix4x4 localSecM = Matrix4x4.identity;


            //        //Debug.Log("localSecM");
            //        //Debug.Log(localSecM);

            //        gh.drawTransformHandles(visited, localSecM, true);
            //        gh.drawControlHandles(ref visited, localSecM, true);

            //    }
            //}
        }

    }






    public class PlanSweepHandler : GeneratorHandler3D
    {

        // PLAN_SWEEP :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
        public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
        {

            PlanSweep gener = (generator as PlanSweep);
            // Draw the plan AND the section splines.

            // PLAN first, since the section needsplan information to position itself.
            if (gener.planSrc_po != null && gener.planSrc_po.generator != null)
            {
                // Get the input PO and draw
                GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
                    Matrix4x4 localPlanM = Matrix4x4.identity;
                    if (gener.planSrc_po.is2D())
                        gh.drawTransformHandles(visited, localPlanM, true);
                    gh.drawControlHandles(ref visited, localPlanM, true);
                }
            }

            // SECTION
            if (gener.sectionSrc_po != null && gener.sectionSrc_po.generator != null)
            {

                GeneratorHandler gh = getGeneratorHandler(gener.sectionSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix.inverse * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.sectionSrc_po);
                    Matrix4x4 localSecM = Matrix4x4.identity;


                    //Debug.Log("localSecM");
                    //Debug.Log(localSecM);

                    gh.drawTransformHandles(visited, localSecM, true);
                    gh.drawControlHandles(ref visited, localSecM, true);

                }
            }
        }

    }


    public class ContourExtruderHandler : GeneratorHandler3D
    {

        // PLAN_SWEEP :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
        public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
        {

            ContourExtruder gener = (generator as ContourExtruder);
            // Draw the plan AND the section splines.

            // PLAN first, since the section needsplan information to position itself.
            if (gener.planSrc_po != null && gener.planSrc_po.generator != null)
            {
                // Get the input PO and draw
                GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
                    Matrix4x4 localPlanM = Matrix4x4.identity;
                    if (gener.planSrc_po.is2D())
                        gh.drawTransformHandles(visited, localPlanM, true);
                    gh.drawControlHandles(ref visited, localPlanM, true);
                }
            }

            // SECTION
            if (gener.sectionSrc_po != null && gener.sectionSrc_po.generator != null)
            {

                GeneratorHandler gh = getGeneratorHandler(gener.sectionSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix.inverse * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.sectionSrc_po);
                    Matrix4x4 localSecM = Matrix4x4.identity;


                    //Debug.Log("localSecM");
                    //Debug.Log(localSecM);

                    gh.drawTransformHandles(visited, localSecM, true);
                    gh.drawControlHandles(ref visited, localSecM, true);

                }
            }



            //if (gener.contourPaths != null && gener.contourPaths.Count > 0)
            //{
            //    Vector3[] verts3d;

            //    Matrix4x4 prevM = Handles.matrix;
            //    Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
            //    foreach (Path path in gener.contourPaths)//p.getTransformedControlPaths())
            //    {

            //        //Handles.matrix = localplanm
            //        verts3d = AXGeometry.Utilities.path2Vec3s(path, Axis.Z, true);
            //        Handles.DrawAAPolyLine(2, verts3d);


            //    }
            //    Handles.matrix = prevM;
            //}
           

        }

    }




    public class CurveSweeperHandler : GeneratorHandler3D
    {

        // PLAN_SWEEP :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
        public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
        {
           
            CurveSweeper3 gener = (generator as CurveSweeper3);
            // Draw the plan AND the section splines.

            // PLAN first, since the section needsplan information to position itself.
            if (gener.path3DSrc_po != null && gener.path3DSrc_po.generator != null)
            {
                // Get the input PO and draw
                GeneratorHandler gh = getGeneratorHandler(gener.path3DSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
                    Matrix4x4 localPlanM = Matrix4x4.identity;
                    //if (gener.planSrc_po.is2D())
                        //gh.drawTransformHandles(visited, localPlanM, true);
                    gh.drawControlHandles(ref visited, localPlanM, true);
                }
            }

            // SECTION
            if (gener.sectionSrc_po != null && gener.sectionSrc_po.generator != null)
            {

                GeneratorHandler gh = getGeneratorHandler(gener.sectionSrc_po);

                if (gh != null)
                {
                    //Matrix4x4 localSecM = parametricObject.model.transform.localToWorldMatrix.inverse * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.sectionSrc_po);
                    Matrix4x4 localSecM = Matrix4x4.identity;


                    //Debug.Log("localSecM");
                    //Debug.Log(localSecM);

                    gh.drawTransformHandles(visited, localSecM, true);
                    gh.drawControlHandles(ref visited, localSecM, true);

                }
            }
        }

    }

}
