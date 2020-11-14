using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace AX.Generators
{

	public interface IPrimitiveGenerator
	{
		
	}

	public class Primitive : AX.Generators.Generator3D, IPrimitiveGenerator
	{
		
		public override void initGUIColor ()
		{
			GUIColor 		= new Color(1f, .6f, .6f, .8f);
			GUIColorPro 	= new Color(1f, .8f, .8f, .8f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);

		}
		
		
	}
	

}


