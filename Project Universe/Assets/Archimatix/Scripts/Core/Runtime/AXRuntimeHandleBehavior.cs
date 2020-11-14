#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using Axis = AXGeometry.Axis;

namespace AX
{

    /// <summary>
    /// AXR untime handle behavior.
    /// 
    /// if the axis of the handle Z or NZ, then diffx and diffy control x and y respectively
    /// 
    /// X, NX maps to (z : y) expressions
    /// Y, NY maps to (x : z) expressions
    /// Z, NZ maps to (x : y) expressions
    /// 
    /// 
    /// 
    /// 
    /// 1. choose axis
    /// 2. local matrix for grouper
    /// 
    /// </summary>
	public class AXRuntimeHandleBehavior : MonoBehaviour
	{

		public Color normalColor = Color.gray;
		public Color highlightColor = Color.white;
		public Color selectedColor = Color.red;


		public AXParameter P_H;
		public AXParameter P_V;

		public Plane drawingSurface;

		bool mouseIsDown;
		Vector3 mouseDownDiff;

		Vector3 mouseDownPosition;
		Vector3 lookV;

		Matrix4x4 context = Matrix4x4.identity;

        private bool altFunc = false;

		// Handle
		public string handleGuid;

		[System.NonSerialized]
		public AXHandle handle;

		float han_x;
		float han_y;
		float han_z;

        public float offPlaneGap = .5f;

        //public Axis axis = Axis.Y;

        public Vector3 scaleV = Vector3.one;


        

        // UPDATE is called once per frame
        // ---------------------------------------------------------------------

        void Update ()
		{
            if (handle == null)
                return;

            if (!Application.isPlaying)
				return;



            AXParametricObject parametricObject = handle.parametricObject;


            // --- GET POSITION MATRIX
            if (parametricObject.is2D())
            {
                context = parametricObject.model.transform.localToWorldMatrix * parametricObject.worldDisplayMatrix;

                if (parametricObject.generator.hasOutputsConnected() || parametricObject.is2D())
                    context *= parametricObject.generator.localMatrix.inverse;
                else
                    context *= parametricObject.getAxisRotationMatrix().inverse * parametricObject.generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix();
            }
            else
            {
                // GROUPER MATRIX NOT WORKING....
                context = parametricObject.model.transform.localToWorldMatrix * parametricObject.generator.parametricObject.worldDisplayMatrix * (parametricObject.getAxisRotationMatrix() * parametricObject.generator.getLocalAlignMatrix()).inverse * parametricObject.generator.localMatrix.inverse;
            }
            
           


			// --- POSITION HANDLE BY PARMETERS.
            positionHandleGameObjectByParameters ();


			if (mouseIsDown) {

                //if (Input.touchCount == 1) {
                //	// touch input - works better with deltaPosition
                //	var touch = Input.GetTouch (0);
                //	//var dx = touch.deltaPosition.x;
                //	h_diff = (100.0 / Screen.width) * touch.deltaPosition.x;
                //	v_diff = (100.0 / Screen.width) * touch.deltaPosition.y;		
                //} else {
                //	// 0 touches: must be mouse input
                //	h_diff = (5000 / Screen.width) * Input.GetAxis ("Mouse X");
                //	v_diff = (5000 / Screen.width) * Input.GetAxis ("Mouse Y");		
                //}
                //h_diff /= 5;
                //v_diff /= 5;
                //Debug.Log(h_diff +", " + v_diff);



                // 1. - BASED ON PLANE AS HIT POINT COLLIDER BASED ON HANDLE AXIS AND OPTIONAL INPUT KEYS
                establishHitPointPlane();
						
                // 2. - SET THE WORLD POSITION OF HANDLE	
                Vector3 world_pos     = sampleHitPoint() - mouseDownDiff;

                // 3. - POSITION THE HANDLE IN WORLD SPACE
				transform.position    = world_pos;
				
                // 4. - NOW ESTABLISH THE LOCAL POSITION OF THE HANDLE OBJECT
				Vector3 localPosition = context.inverse.MultiplyPoint3x4 (world_pos);


                if (parametricObject != null)
                {
                    parametricObject.setVar("han_x", localPosition.x);
                    parametricObject.setVar("han_y", localPosition.y);
                    parametricObject.setVar("han_z", localPosition.z);

                    // EACH EXPRESSION
                    for (int i = 0; i < handle.expressions.Count; i++)
                    {
                        if (handle.expressions[i] == "")
                            continue;

                        string expression = Regex.Replace(handle.expressions[i], @"\s+", "");

                        string paramName = expression.Substring(0, expression.IndexOf("=", StringComparison.InvariantCulture));
                        string definition = expression.Substring(expression.IndexOf("=", StringComparison.InvariantCulture) + 1);

                        try
                        {
                            if (parametricObject.getParameter(paramName).Type == AXParameter.DataType.Int)
                                parametricObject.initiateRipple_setIntValueFromGUIChange(paramName, Mathf.RoundToInt((float)parametricObject.parseMath_ValueOnly(definition)));
                            else
                                parametricObject.initiateRipple_setFloatValueFromGUIChange(paramName, (float)parametricObject.parseMath_ValueOnly(definition));

                        }
                        catch (System.Exception e)
                        {
                            parametricObject.codeWarning = "10. Handle error: Please check syntax of: \"" + definition + "\" " + e.Message;
                        }
                    }

                    parametricObject.model.isAltered();
                }
            }
        }

			


		
        // POSITION_HANDLE_GAMEOBJECT_BY_PARAMETERS
        // ---------------------------------------------------------------------
		public void positionHandleGameObjectByParameters ()
		{
            if (handle != null && handle.parametricObject != null)
            {



                Vector3 pos = handle.getPointPosition();
                //Debug.Log(pos);

                transform.position = context.MultiplyPoint3x4(pos);

                switch (handle.axis)
                {
                    case Axis.NZ:

                        transform.rotation = AXUtilities.QuaternionFromMatrix(context) * Quaternion.Euler(-90, 0, 0);
                        break;

                    default:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                }

                transform.localScale = scaleV;
            }
            else
            {
                transform.position = new Vector3(han_x, han_y, han_z);
            }
        }





        // SAMPLE_HIT_POINT
        // ---------------------------------------------------------------------
        public Vector3 sampleHitPoint ()
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			float rayDistance = 0;

			// Point on Plane
			if (drawingSurface.Raycast (ray, out rayDistance))
				return ray.GetPoint (rayDistance);

			return Vector3.zero;


		}



        // ESTABLISH_HIT_POINT_PLANE
        // ---------------------------------------------------------------------

        public void establishHitPointPlane ()
		{
			Matrix4x4 m = context;

            Vector3 v1, v2, v3;

            switch (handle.axis)
            {
                case Axis.NZ:
                    if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.Z))
                    {
                        // ALLOWS HNDLE MOVEMENT ON ITS AXIS
                        offPlaneGap = transform.localPosition.x;
                        v1 = new Vector3(offPlaneGap, 0, 0);
                        v2 = new Vector3(offPlaneGap, 0, 1);
                        v3 = new Vector3(offPlaneGap, 1, 0);
                    }
                    else
                    {
                        offPlaneGap = transform.localPosition.z;
                        v1 = new Vector3(0, 0, -offPlaneGap);
                        v2 = new Vector3(1, 0, -offPlaneGap);
                        v3 = new Vector3(0, 1, -offPlaneGap);
                    }
                    break;


                default:
                    v1 = new Vector3(0, offPlaneGap, 0);
                    v2 = new Vector3(1, offPlaneGap, 0);
                    v3 = new Vector3(0, offPlaneGap, 1);
                    break;

            }
            v1 = m.MultiplyPoint3x4 (v1);
			v2 = m.MultiplyPoint3x4 (v2);
			v3 = m.MultiplyPoint3x4 (v3);
			
			drawingSurface = new Plane (v1, v2, v3);
			//Matrix4x4 = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix.inverse;

		}





		void OnMouseDown ()
		{
			AXRuntimeControllerBase.runtimeHandleIsDown = true;

			establishHitPointPlane ();
			
			mouseDownDiff = sampleHitPoint () - transform.position;

			GetComponent<Renderer> ().material.color = selectedColor;

			mouseIsDown = true;

            if (handle != null && handle.parametricObject != null && handle.parametricObject.is3D())
            {

                handle.parametricObject.model.OnRuntimeHandleDown();
            }
		}

		void OnMouseUp ()
		{

           
			AXRuntimeControllerBase.runtimeHandleIsDown = false;

			mouseIsDown = false;
			GetComponent<Renderer> ().material.color = normalColor;
            if (handle != null && handle.parametricObject != null && handle.parametricObject.is3D())
            {
                handle.parametricObject.model.isAltered();
                handle.parametricObject.model.autobuild();
                handle.parametricObject.model.OnRuntimeHandleUp();
            }

		}

		void OnMouseEnter ()
		{
			if (!mouseIsDown)
            {
                GetComponent<Renderer>().material.color = highlightColor;
                if (handle != null && handle.parametricObject != null)
                    handle.parametricObject.model.OnRuntimeHandleEnter();
            }
				
		}

		void OnMouseExit ()
		{
			if (!mouseIsDown)
            {
                GetComponent<Renderer>().material.color = normalColor;
                if (handle != null && handle.parametricObject != null)
                    handle.parametricObject.model.OnRuntimeHandleExit();
            }
				
		}



	}

}
