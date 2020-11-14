
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

namespace AXGeometry
{


	/// <summary>
	/// AXSegment encapsulates a Path and adds the ability to link to AXSegments before and after it.
	/// It has a compare points funtion to see if two IntPoints are equal.
	/// </summary>
	public class AXSegment
	{
		// points 0 and 1
		public Path path;

		public AXSegment prev;
		public AXSegment next;

		public AXSegment(Path p)
		{
			path = p;
		}

		public static bool pointsAreEqual(IntPoint a, IntPoint b)
		{
			if (a.X == b.X && a.Y == b.Y)
				return true;
			return 
				false;
		}

	}
}

