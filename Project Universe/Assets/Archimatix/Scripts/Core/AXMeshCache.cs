using UnityEngine;
using System.Collections;

using AXGeometry;

namespace AX
{

	public class AXMeshCache  {

		[System.NonSerialized]
		private int Cursor = 0;
		
		[System.NonSerialized]
		private AXMesh[] 		axMeshes;

		public AXMeshCache(int size=5000)
		{

			axMeshes = new AXMesh[size];
			Cursor = 0;
		}

		public void reset()
		{
			Cursor = 0;

		}


		public AXMesh next()
		{
			
			if ( axMeshes[Cursor] == null)
				axMeshes[Cursor] = new AXMesh();

			Cursor++;

			return axMeshes[Cursor-1];
		}

	}

}