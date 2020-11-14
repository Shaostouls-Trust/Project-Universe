

// Editor Window Scaling Utility

using UnityEngine;

using System;
 


/// A simple class providing static access to functions that will provide a 
/// zoomable area similar to Unity's built in BeginVertical and BeginArea
/// Systems.
// From:
// http://www.arsentuf.me/2015/01/27/unity-3d-editor-gui-and-scaling-matricies/
//https://github.com/Honeybunch/VNKit/blob/master/VNKit/Assets/VNKit/Editor/EditorZoomArea.cs

namespace AXEditor
{
	public static class RectExtensions
	{
		/// <summary>
		/// Scales a rect by a given amount around its center point
		/// </summary>
		/// <param name="rect">The given rect</param>
		/// <param name="scale">The scale factor</param>
		/// <returns>The given rect scaled around its center</returns>
		public static Rect ScaleSizeBy(this Rect rect, float scale) 
		{ 
			return rect.ScaleSizeBy(scale, rect.center); 
		}

		/// <summary>
		/// Scales a rect by a given amount and around a given point
		/// </summary>
		/// <param name="rect">The rect to size</param>
		/// <param name="scale">The scale factor</param>
		/// <param name="pivotPoint">The point to scale around</param>
		/// <returns>The rect, scaled around the given pivot point</returns>
		public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint) 
		{
			Rect result = rect;

			//"translate" the top left to something like an origin
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;

			//Scale the rect
			result.xMin *= scale;
			result.yMin *= scale;
			result.xMax *= scale;
			result.yMax *= scale;

			//"translate" the top left back to its original position
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;

			return result;


		}
	}






	public class EditorZoomArea
	{

		private static Matrix4x4 prevMatrix;


		// After you call this begin,
		// every GUI item called will be in this transform. 
		// To pan, you must move the gui tiems yourself.
		public static void Begin(float zoomScale, Rect screenRect, Vector2 zoomPivot) 
		{
			GUI.EndGroup(); //End the group that Unity began so we're not bound by the EditorWindow

			// The Rect of the new clipping group to- draw our scaled GUI in
			Rect clippedArea = screenRect.ScaleSizeBy( (1.0f/zoomScale), zoomPivot );

			clippedArea.y += 21 ; //Account for the window tab

			GUI.BeginGroup(clippedArea);


			prevMatrix = GUI.matrix;

            //Perform scaling		

           

            Matrix4x4 localTranslation = Matrix4x4.TRS(clippedArea.center, Quaternion.identity, Vector3.one);
			Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
			GUI.matrix = localTranslation * scale * localTranslation.inverse;


			return;
		}



		/// <summary>
		/// Ends the zoom area
		/// </summary>
		public static void End() 
		{
			GUI.matrix = prevMatrix;
			GUI.EndGroup();
			GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
		}






		/// <summary>
		/// Scales the rect around the pivot with scale
		/// </summary>
		public static Rect ScaleRect (Rect rect, Vector2 pivot, Vector2 scale) 
		{
			rect.position = Vector2.Scale (rect.position - pivot, scale) + pivot;
			rect.size = Vector2.Scale (rect.size, scale);
			return rect;
		}

	}
}

