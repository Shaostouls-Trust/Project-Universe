using UnityEngine;
using System;
using System.Collections;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using AX;

namespace AXGeometry
{
	[Serializable]
	public class AXTexCoords
	{
		public Vector2 	scale;
		public bool 	scaleIsUnified;
		public Vector2 	shift;
		public bool 	runningU;
		public bool     rotateSidesTex;
		public bool     rotateCapsTex;

        public bool useRectMapping = false;
        public Rect boundsRect;
        public Rect atlasRect;

		// Let's grow the AXTex to be more of a game material
		// in that properties such as density, hardness, etc. may be stored here.


		public AXTexCoords()
		{
			shift 			= Vector2.zero;
			scale 			= new Vector2(5f, 5f);
			scaleIsUnified 	= true;

			runningU 		= true;
			rotateSidesTex 	= false; 
			rotateCapsTex 	= false; 
		}   
		public AXTexCoords(AXTexCoords tex)
		{
			shift 			= new Vector2(tex.shift.x, tex.shift.y);
			scale 			= new Vector2(tex.scale.x, tex.scale.y);
			scaleIsUnified 	= tex.scaleIsUnified;

			runningU 		= tex.runningU;
			rotateSidesTex 	= tex.rotateSidesTex; 
			rotateCapsTex 	= tex.rotateCapsTex; 

		}   

        public void fitPathBoundsToAtlasRect(Path path, Rect atlasRect)
        {

            useRectMapping = true;

            boundsRect = Pather.IntRect2Rect(Pather.getBounds(path));

            scale.x = atlasRect.width / boundsRect.width;
            scale.y = atlasRect.height / boundsRect.height;

            shift.x = atlasRect.x;
            shift.y = atlasRect.y;



        }
        public Vector2 uvFromRectMapping(Vector2 meshPoint)
        {
            float u = (meshPoint.x - boundsRect.x) * scale.x + atlasRect.x;
            float v = (meshPoint.y - boundsRect.y) * scale.y + atlasRect.y;
            Vector2 uv =  new Vector2(u, v);
           


            return uv;
        }


        public void printOut()
		{
			Debug.Log("scale="+scale + ", shift="+shift);

		}

	}
}
