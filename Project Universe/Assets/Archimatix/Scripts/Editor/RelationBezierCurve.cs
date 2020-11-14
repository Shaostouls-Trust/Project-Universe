using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AXClipperLib;
using Path 			= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 		= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


using AX;

namespace AXEditor
{

	/// <summary>
	/// Represents a AX.AXRelation connection curve. This curve is a Bezier curve dran in the Node Graph Editor.
	/// Use tis for selectable curves.
	/// </summary>
	public struct RelationBezierCurve
	{
		public AX.AXRelation relation;

		public Vector2 startPosition;
		public Vector2 endPosition;
		public Vector2 startTangent;
		public Vector2 endTangent;

		public RelationBezierCurve(AXRelation r, Vector2 startP, Vector2 endP)
		{
			relation 		=r;

			startPosition 	= startP;
			endPosition 	= endP;


			float 	dist 	= Vector3.Distance(startPosition, endPosition);
			startTangent 	= startPosition + Vector2.right * (dist / 3f) ;
			endTangent 		= endPosition + Vector2.left * (dist / 3f);


		}


	}



}