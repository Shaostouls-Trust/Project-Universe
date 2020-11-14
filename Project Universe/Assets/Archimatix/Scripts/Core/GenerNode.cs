#pragma warning disable

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AX
{
	

	public class GenerNode
	{
		// A GenerNode is a representation of an item created by the model.build().
		// It can persist after the AXGameObject has been destroy.
		// It is associated with a ParametricObject
		
		// A Generator generates multiple GenerNodes whenit generates, but especially when it builds() AXGameObjects.
		// It has an internal addressing system that is relevant to how it makes items.
		// This could be a straight index (i), or a grided (i, j) or a radial coordinates (alpha, beta, radius), etc.
		
		
		// A CycleList of GenerNodes becomes a memory of a selected slice through the generatedGameObjects heirarchy.
		AXParametricObject parametricObject;
		
		// Each ParametricObject knows its addressing system and how to parse a data string.
		// And the GenerObject knows how it was created by its Consumer.
		
		// Hierarchical Structure - follows same heirarchy as GameObjects from build...
		public GenerNode consumerGenerNode;
		
		// better still if an address is given, but not necessary
		public string 	 consumerGenerNodeLocalAddress;

		
	}

}

