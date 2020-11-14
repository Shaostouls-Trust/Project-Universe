using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AXGeometry;


namespace AX
{

	/// <summary>
	/// AX material.
	/// This class not only holds a Unity Material, but also 
	/// the data required for a physical material.
	/// This might be like havning collectible items 
	/// for game play such as granite, diamonf, gold, etc.
	/// Thus, an AXMaterial has a PhysicMaterial and a density used to calculate mass based on size of the object.
	/// A ParametricObject can have a reference to the model's AXMaterial or store its own locally.
	///
	/// AXMaterials are distributed through the graph through a function whenever there has been a change in the 
	/// connection or deletion of a MaterialTool.
	/// </summary>
	[System.Serializable] 
	public class AXMaterial 
	{
		public Material	 		mat;
		public PhysicMaterial 	physMat;
		public float			density = 0; // When zero, the goruper or model density will be used instead.
		public float 			price;
		public bool 			showPhysicsDefaults;
	}

}