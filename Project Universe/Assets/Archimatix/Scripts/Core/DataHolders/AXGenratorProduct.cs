using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

namespace AX
{
    /// <summary>
    /// Generator product.
    /// 
    /// A holder for the various objects that are the products of a nodeless generation function.
    /// 
    /// ax_meshes is usually for redering using Graphics.DrawMesh
    /// 
    /// </summary>
    public class GeneratorProduct
    {
        public List<AXMesh> ax_meshes;
        public GameObject   gameObject;
        public Paths        paths;
        public PolyTree     polytree;

		public float area;
		public float volume;

	}



}