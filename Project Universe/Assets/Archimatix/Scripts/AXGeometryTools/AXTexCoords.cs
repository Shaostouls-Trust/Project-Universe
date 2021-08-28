using UnityEngine;
using System;
using System.Collections;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

namespace AXGeometry
{
	[Serializable]
	public class AXTexCoords
	{
		public Vector2 scale;
		public bool scaleIsUnified;
		public Vector2 shift;
		public bool runningU;
		public bool rotateSidesTex;
		public bool rotateCapsTex;

		public bool useRectMapping = false;
		public Rect boundsRect;
		public Rect atlasRect;

		public float mU;
		public float mV;

		public float u1;
		public float v1;

		public float x1;
		public float y1;

		// Let's grow the AXTex to be more of a game material
		// in that properties such as density, hardness, etc. may be stored here.


		public AXTexCoords()
		{
			shift = Vector2.zero;
			scale = new Vector2(5f, 5f);
			scaleIsUnified = true;

			runningU = true;
			rotateSidesTex = false;
			rotateCapsTex = false;
		}
		public AXTexCoords(AXTexCoords tex)
		{
			shift = new Vector2(tex.shift.x, tex.shift.y);
			scale = new Vector2(tex.scale.x, tex.scale.y);
			scaleIsUnified = tex.scaleIsUnified;

			runningU = tex.runningU;
			rotateSidesTex = tex.rotateSidesTex;
			rotateCapsTex = tex.rotateCapsTex;

		}

		public AXTexCoords(Rect _atlasRect, Rect _boundsRect)
		{
			InitWithRects(_atlasRect, _boundsRect);
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


		public void InitWithRects(Rect _atlasRect, Rect _boundsRect)
		{
			useRectMapping = true;

			atlasRect = _atlasRect;
			boundsRect = _boundsRect;

			mU = atlasRect.width / boundsRect.width;
			mV = atlasRect.height / boundsRect.height;

			u1 = atlasRect.x;
			v1 = atlasRect.y;

			x1 = boundsRect.x;
			y1 = boundsRect.y;
		}


		public Vector2 uvFromRectMapping(Vector2 meshPoint)
		{
			float u = mU * (meshPoint.x - x1) + u1;
			float v = mV * (meshPoint.y - y1) + v1;
			Vector2 uv = new Vector2(u, v);

			return uv;
		}


		public void printOut()
		{
			Debug.Log("scale=" + scale + ", shift=" + shift);
		}
	}
}