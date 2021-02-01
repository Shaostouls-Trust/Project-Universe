#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AX;
using AX.Generators;

namespace AX.GeneratorHandlers
{


	public class PhysicsRopeHandler: GeneratorHandler3D 
	{


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			

		}
		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		{
			base.drawBoundsHandles (consumerM, forceDraw);

			PhysicsRope physicRope = (generator as PhysicsRope);

			Matrix4x4 prevHandleMatrix = Handles.matrix;
			Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;
			Handles.matrix = context;

			Color prevColor = Handles.color;
			Handles.color 	= Color.cyan;



			//Handles.DrawLine(physicRope.P_Point1.vector3, physicRope.P_Point2.vector3);

			EditorGUI.BeginChangeCheck();

			physicRope.P_Point1.vector3 = Handles.PositionHandle (physicRope.P_Point1.vector3, Quaternion.identity);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Translate");

				physicRope.point1 = physicRope.P_Point1.vector3;
				parametricObject.model.isAltered();
				generator.adjustWorldMatrices();

			}


			EditorGUI.BeginChangeCheck();

			physicRope.P_Point2.vector3 = Handles.PositionHandle (physicRope.P_Point2.vector3, Quaternion.identity);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Translate");

				physicRope.point2 = physicRope.P_Point2.vector3;

				parametricObject.model.isAltered();
				generator.adjustWorldMatrices();

			}



			// SLACK HANDLE
			Vector3 slackV = physicRope.P_Point2.vector3+((physicRope.slack+1)*Vector3.down);
			Handles.DrawLine (physicRope.P_Point2.vector3, slackV);
			EditorGUI.BeginChangeCheck();


			

			slackV = Handles.FreeMoveHandle(
				slackV, 
				Quaternion.identity,
				.1f*HandleUtility.GetHandleSize(slackV),
				Vector3.zero, 
				Handles.SphereHandleCap
			);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Slack");

				physicRope.slack = (physicRope.P_Point2.vector3 - slackV).y - 1;
			
				physicRope.P_Slack.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(physicRope.slack);
				parametricObject.model.isAltered();
				generator.adjustWorldMatrices();

			}







			// OPTIMIZED CATENARY SPLINE

			if (!physicRope.isPhysical && physicRope.catenaryPoints != null) {
				for (int i = 0; i < physicRope.catenaryPoints.Length; i++) {
					if (i == 0)
						continue;


					Handles.DrawLine (physicRope.catenaryPoints [i - 1], physicRope.catenaryPoints [i]);

					Handles.FreeMoveHandle(
						physicRope.catenaryPoints [i], 
						Quaternion.identity,
						.1f*HandleUtility.GetHandleSize(physicRope.catenaryPoints [i]),
						Vector3.zero, 
						Handles.SphereHandleCap
					);
				}
			}


			// EVENLY-SPACED CATENARY SPLINE
			if (physicRope.isPhysical && physicRope.catenaryPointsEvenSpacing != null) {
				
				Handles.color = Color.red;

				for (int i = 0; i < physicRope.catenaryPointsEvenSpacing.Length; i++) {
					if (i == 0)
						continue;


					Handles.DrawLine (physicRope.catenaryPointsEvenSpacing [i - 1], physicRope.catenaryPointsEvenSpacing [i]);


					// Draw tangents
//					Vector3[] spline = physicRope.catenaryPointsEvenSpacing;
//
//					if (i > 0 && i < spline.Length - 1) {
//
//						Vector3 v0 = (spline [i] - spline [i-1]);					
//						Vector3 v1 = (spline [i+1] - spline [i]);
//						Vector3 tangent = v1.normalized + v0.normalized;
//						Handles.DrawLine (spline [i], spline [i] + 2*tangent);
//					}



					Handles.FreeMoveHandle(
						physicRope.catenaryPointsEvenSpacing [i], 
						Quaternion.identity,
						.1f*HandleUtility.GetHandleSize(physicRope.catenaryPointsEvenSpacing [i]),
						Vector3.zero, 
						Handles.SphereHandleCap
					);
				}
			}

			Handles.matrix = prevHandleMatrix;
			Handles.color 	= prevColor;
		}

	}
}
