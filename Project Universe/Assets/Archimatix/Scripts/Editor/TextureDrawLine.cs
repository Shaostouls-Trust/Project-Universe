using UnityEditor;
using UnityEngine;

/* Author: Eric Haines (Eric5h5) 
 * http://wiki.unity3d.com/index.php?title=TextureDrawLine 
 */
public class TextureDrawLine {



	public static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
	{
		Color col1 = col;
		col1.a *= .85f;
		
		Color col2 = col;
		col2.a *= .7f;
		
		
		int dy = (int)(y1-y0);
		int dx = (int)(x1-x0);
		int stepx, stepy;
		
		if (dy < 0) {dy = -dy; stepy = -1;}
		else {stepy = 1;}
		if (dx < 0) {dx = -dx; stepx = -1;}
		else {stepx = 1;}
		dy <<= 1;
		dx <<= 1;
		
		float fraction = 0;
		
		tex.SetPixel(x0, y0, col);
		if (dx > dy) {
			fraction = dy - (dx >> 1);
			while (Mathf.Abs(x0 - x1) > 1) {
				if (fraction >= 0) {
					y0 += stepy;
					fraction -= dx;
				}
				x0 += stepx;
				fraction += dy;
				tex.SetPixel(x0, y0, col);
				
				// ring 1
				tex.SetPixel(x0+1, y0, col1);
				tex.SetPixel(x0-1, y0, col1);
				tex.SetPixel(x0, y0+1, col1);
				tex.SetPixel(x0, y0-1, col1);
				
				tex.SetPixel(x0+1, y0+1, col2);
				tex.SetPixel(x0-1, y0-1, col2);
				tex.SetPixel(x0-1, y0+1, col2);
				tex.SetPixel(x0-1, y0-1, col2);
				
				
			
				// ring 2
				tex.SetPixel(x0+2, y0, col2);
				tex.SetPixel(x0-2, y0, col2);
				tex.SetPixel(x0, y0+2, col2);
				tex.SetPixel(x0, y0-2, col2);
				
				
				
								
				/*
				tex.SetPixel(x0+3, y0, col);
				tex.SetPixel(x0-3, y0, col);
				tex.SetPixel(x0, y0+3, col);
				tex.SetPixel(x0, y0-3, col);
				*/
			}
		}
		else {
			fraction = dx - (dy >> 1);
			while (Mathf.Abs(y0 - y1) > 1) {
				if (fraction >= 0) {
					x0 += stepx;
					fraction -= dy;
				}
				y0 += stepy;
				fraction += dx;
				tex.SetPixel(x0, y0, col);
				
				// ring 1
				tex.SetPixel(x0+1, y0, col1);
				tex.SetPixel(x0-1, y0, col1);
				tex.SetPixel(x0, y0+1, col1);
				tex.SetPixel(x0, y0-1, col1);
				
				tex.SetPixel(x0+1, y0+1, col2);
				tex.SetPixel(x0-1, y0-1, col2);
				tex.SetPixel(x0-1, y0+1, col2);
				tex.SetPixel(x0-1, y0-1, col2);
				
				
				tex.SetPixel(x0+2, y0, col2);
				tex.SetPixel(x0-2, y0, col2);
				tex.SetPixel(x0, y0+2, col2);
				tex.SetPixel(x0, y0-2, col2);
				
				/*
				tex.SetPixel(x0+3, y0, col);
				tex.SetPixel(x0-3, y0, col);
				tex.SetPixel(x0, y0+3, col);
				tex.SetPixel(x0, y0-3, col);
				*/
			}
		}
	}

}