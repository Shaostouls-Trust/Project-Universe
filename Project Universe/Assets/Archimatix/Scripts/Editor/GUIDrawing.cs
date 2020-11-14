using UnityEditor;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

  
using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using AXEditor;

using AXClipperLib;
using Path 	= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

 


public class GUIDrawing   {


    public static void DrawGrid(Vector2 offset, Color col, Color axiscol, int bay = 25)		
	{		
		

		Color origColor = Handles.color;
		
		Handles.BeginGUI();
		
		Handles.color = col;
		
		for(int i=-200; i<200; i++) 
		{		
			//Handles.DrawLine(new Vector3(i*bay+offset.x-shift,-5000,0), new Vector3(i*bay+offset.x-shift,5000,0));
			//Handles.DrawLine(new Vector3(-5000,i*bay+offset.y-shift,0), new Vector3(5000,i*bay+offset.y-shift,0));
			Handles.DrawLine(new Vector3(	i*bay+offset.x, 		-5000+offset.y, 0), new Vector3(i*bay+offset.x,	 5000+offset.y,	0));
			Handles.DrawLine(new Vector3(	-5000+offset.x,		i*bay+offset.y, 0), new Vector3( 5000+offset.x,	i*bay+offset.y,  0));
		}
		
		Handles.color = axiscol;
		Handles.DrawLine(new Vector3(	offset.x, 		-5000+offset.y, 0), new Vector3(offset.x,	 5000+offset.y,	0));
		Handles.DrawLine(new Vector3(	-5000+offset.x, 		offset.y, 0), new Vector3(5000+offset.x,	 offset.y,	0));
		
		Handles.color = origColor;
		Handles.EndGUI();
	}



	public static void DrawCurves(Vector2 p1, Vector2 p2, Color c, Color curveShadowColor, float thickness = 5f)		
	{		
		
		Vector3 startPos 		= new Vector3(p1.x,   p1.y, 0);
		Vector3 endPos 			= new Vector3(p2.x,   p2.y, 0);


		float 	dist 			= Vector3.Distance(p1,p2);
		Vector3 startTangent 	= startPos + Vector3.right * (dist / 3f) ;
		Vector3 endTangent 		= endPos + Vector3.left * (dist / 3f);

		float g = 2;

		Handles.BeginGUI();
		if (p2.x > p1.x-300)
		{

			Handles.DrawBezier(startPos+new Vector3(-g,8,0), endPos+new Vector3(g,8,0), startTangent, endTangent, curveShadowColor, null, 2*thickness);
			Handles.DrawBezier(startPos+new Vector3(-g,0,0), endPos+new Vector3(g,0,0), startTangent, endTangent, c, null, thickness);
		}
		else
		{

			Vector3 m1 		= startPos + new Vector3(  50, dist/10, 0);
			Vector3 m2 		= endPos   + new Vector3( -50, dist/10-(endPos.y-startPos.y), 0);
			float 	mdist 	= Vector3.Distance(m1, m2);

			Handles.DrawBezier(startPos+new Vector3(-g,8,0), m1, startPos+Vector3.right*50, m1+Vector3.down*50f, c, null, thickness);
			Handles.DrawBezier( m1, m2, m1+Vector3.up*mdist/6, m2+Vector3.up*mdist/6, c, null, thickness);
			Handles.DrawBezier( m2, endPos, m2+Vector3.down*20, endPos+Vector3.left*50, c, null, thickness);

		}
		Handles.EndGUI();
		
	}
	
	
	public static void DrawSplineFit(AXParameter p, Vector2 offset, float size, Color splineColor) 
	{
		
		AXSpline s = p.spline.clone ();
		s.rotate (p.Parent.floatValue("Rot_Z"));
		
		if (s == null) return;
		
		// scale the spline to fit in "size" in pixels
		AXSpline os = s.clone();
		
		os.calcStats();
		os.shift (-os.cenX, -os.cenY);
		
		float maxdim = (os.width > os.height) ? os.width : os.height;
		float scale = size / maxdim;
		os.scale(scale);

		GUIDrawing.DrawSpline(os, offset, splineColor);
	}
	


	public static void DrawSpline(AXSpline _spline, Vector2 offset, Color splineColor) 
	{
		Handles.BeginGUI();
		Handles.color = splineColor;


		if (_spline != null)
		{
			
			List<AXSpline> subs = _spline.getSubsplines();
			if (subs != null && subs.Count > 0)
			{

				foreach (AXSpline sub in subs)
				{
					if (sub.verts == null || sub.vertCount == 0)
						continue;

					List<AXSpline> parts = sub.getSolidAndHoles();


					AXSpline contour = parts[0];

					Color closeColor 	= splineColor;
					closeColor.a 		= splineColor.a/3;

					// origin
					Handles.color = new Color(0,1,0,.5f);
					GUIDrawing.drawSquare(new Vector2(contour.verts[0].x+offset.x, -contour.verts[0].y+offset.y), 2);

					for (int i=1; i<contour.vertCount; i++) 
					{
						Handles.color = splineColor;
						Handles.DrawLine(new Vector3(contour.verts[i-1].x+offset.x, -contour.verts[i-1].y+offset.y,0), new Vector3(contour.verts[i].x+offset.x, -contour.verts[i].y+offset.y,0));
					}
					Handles.color = closeColor;
					if(contour.isClosed)
						Handles.DrawLine(new Vector3(contour.verts[contour.vertCount-1].x+offset.x, -contour.verts[contour.vertCount-1].y+offset.y,0), new Vector3(contour.verts[0].x+offset.x, -contour.verts[0].y+offset.y,0));

					Handles.color = Color.magenta;
					for (int pc=1; pc<parts.Count; pc++)
					{
						// origin
						Handles.color = new Color(0,1,0,.5f);
						GUIDrawing.drawSquare(new Vector2(parts[pc].verts[0].x+offset.x, -parts[pc].verts[0].y+offset.y), 2);


						Handles.color = Color.cyan;
						for (int i=1; i<parts[pc].vertCount; i++) 
						{
							Handles.DrawLine(new Vector3(parts[pc].verts[i-1].x+offset.x, -parts[pc].verts[i-1].y+offset.y,0), new Vector3(parts[pc].verts[i].x+offset.x, -parts[pc].verts[i].y+offset.y,0));
						}
						Handles.DrawLine(new Vector3(parts[pc].verts[parts[pc].vertCount-1].x+offset.x, -parts[pc].verts[parts[pc].vertCount-1].y+offset.y,0), new Vector3(parts[pc].verts[0].x+offset.x, -parts[pc].verts[0].y+offset.y,0));



					}



					//Debug.Log ("close= "+(contour.vertCount-1));
				}
			}
		}

		
		Handles.EndGUI();
		 
	}









	public static void DrawPathsFit(AXParameter p, Vector2 offset, float size) 
	{
		DrawPathsFit(p, offset, size, Color.magenta);
	}
	public static void DrawPathsFit(AXParameter p, Vector2 offset, float size, Color color) 
	{

		if (p == null)
			return;
		
		bool isClosed = (p.shapeState == ShapeState.Closed || p.thickness > 0);
		
		float rot = 0;//p.Parent.floatValue("Rot_Z");
		
		
		Paths paths = null;
		if(p.polyTree != null)
			paths = Clipper.PolyTreeToPaths(p.polyTree);
		else if (p.paths != null)
			paths = p.paths;		
		

		
						
		Rect 	t_bounds = AXGeometry.Utilities.getClipperBounds(p.transformedControlPaths);
		float 	t_maxdim = (t_bounds.width > t_bounds.height) ? t_bounds.width : t_bounds.height;
		float 	t_scale = size / t_maxdim;
		
		Rect 	p_bounds = AXGeometry.Utilities.getClipperBounds(paths);
		float 	p_maxdim = (p_bounds.width > p_bounds.height) ? p_bounds.width : p_bounds.height;
		float 	p_scale = size / p_maxdim;
		
		float 	scale = t_scale;
		Rect 	bounds = t_bounds;
		
		if (p_scale < t_scale)
		{
			scale = p_scale;
			bounds = p_bounds;
		}
		offset += scale * new Vector2(-(bounds.x+bounds.width/2), bounds.y+bounds.height/2);
		
		// DRAW CONTROL_PATHS control paths 
		//if(p.transformedControlPaths != null)
		if(p.transformedButUnscaledOutputPaths != null)
		{
			foreach(Path cpath in p.transformedButUnscaledOutputPaths)
				drawPath(AXGeometry.Utilities.path2Vec2s(cpath, rot, scale), offset, isClosed, Color.gray);
		}
												
		if(p.transformedAndScaledButNotOffsetdOutputPaths != null)
		{
			foreach(Path cpath in p.transformedAndScaledButNotOffsetdOutputPaths)
				drawPath(AXGeometry.Utilities.path2Vec2s(cpath, rot, scale), offset, isClosed, Color.cyan);
		}
												
		if (paths != null)
		foreach(Path path in paths)
				drawPath(AXGeometry.Utilities.path2Vec2s(path, rot, scale), offset, isClosed, color);


		// axis
		Vector2[] xaxis = new Vector2[2];
			xaxis[0] = new Vector2(-5, 0);
			xaxis[1] = new Vector2(5, 0);

			Vector2[] yaxis = new Vector2[2];
			yaxis[0] = new Vector2(0, -5);
			yaxis[1] = new Vector2(0, 5);

		drawPath(xaxis, offset, false, Color.gray);
		drawPath(yaxis, offset, false, Color.gray);


	}

	public static void drawPath(Vector2[] verts, Vector2 offset, bool isClosed, Color color, int thickness=6)
	{
		if (verts == null || verts.Length == 0)
			return;


		Handles.BeginGUI();
		Handles.color = color;

		Color closeColor 	= color;
		closeColor.a 		= color.a * .7f;
		if (color == Color.yellow)
		{
			Handles.color = new Color(0,1,0,.9f);
			drawSquare(new Vector2(verts[0].x+offset.x, -verts[0].y+offset.y), 2);
		}
		Handles.color = color;


			int len =  verts.Length;

			if ( Vector2.Distance(verts[0], verts[verts.Length-1]) < .01f )
				len--;

			int size = (isClosed) ? len+1 : len;

			Vector3[] verts3D = new Vector3[size];

			for (int i=0; i<len; i++)
				verts3D[i] = new Vector3( (verts[i].x+offset.x), (-verts[i].y+offset.y), 0);

			if (isClosed)
				verts3D[len] = new Vector3( (verts[0].x+offset.x), (-verts[0].y+offset.y), 0);

			Handles.DrawAAPolyLine(thickness, verts3D);
		

		Handles.EndGUI();

	}

	public static void drawSquare(Vector2 cen, int rad)
	{
		Handles.DrawLine(new Vector3(cen.x-rad, cen.y-rad, 0), new Vector3(cen.x+rad, cen.y-rad, 0));
		Handles.DrawLine(new Vector3(cen.x-rad, cen.y+rad, 0), new Vector3(cen.x+rad, cen.y+rad, 0));
		Handles.DrawLine(new Vector3(cen.x-rad, cen.y-rad, 0), new Vector3(cen.x-rad, cen.y+rad, 0));
		Handles.DrawLine(new Vector3(cen.x+rad, cen.y-rad, 0), new Vector3(cen.x+rad, cen.y+rad, 0));
	}
	public static void drawCrosshair(Vector2 cen, int rad)
	{
		Handles.DrawLine(new Vector3(cen.x+rad, cen.y+rad, 0), new Vector3(cen.x-rad, cen.y-rad, 0));
		Handles.DrawLine(new Vector3(cen.x-rad, cen.y+rad, 0), new Vector3(cen.x+rad, cen.y-rad, 0));
	}


	 

}
