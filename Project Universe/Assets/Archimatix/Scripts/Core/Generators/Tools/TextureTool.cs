using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AXGeometry;



namespace AX.Generators
{



	public class TextureTool : AXTool
	{
		public override string GeneratorHandlerTypeName { get { return "MaterialToolHandler"; } }



        // INIT_PARAMETRIC_OBJECT
        public override void init_parametricObject()
        {
            base.init_parametricObject();

         
            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Texture2D, AXParameter.ParameterType.Output, "Output"));
        }







    }

}
