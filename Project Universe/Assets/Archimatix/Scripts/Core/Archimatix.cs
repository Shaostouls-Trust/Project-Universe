using UnityEngine;

#if UNITY_EDITOR  
using UnityEditor;
#endif


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using AX.SimpleJSON;

using AX.Expression;
using Parser = AX.Expression.AXParser;


using AXGeometry;
using AX;
using AX.Generators;

using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


namespace AX
{
	public class Archimatix 
	{
		public static float Version = 1.05f;


		// CUSTOM NODES
		// AX supports extensions in the form of custom nodes that are derived from
		// one of the Generator classes.
		// If a class is derived from a Generator and implements ICustomNode, then it will appear in the node sidebar at the bottom.
		// If an node icon image exists anywhere in the assets folder with the naming convention zz_AXNode-[nodeName], then that icon will be displayed
		// in the sidebar. Otherwise, a default CustomNode icon will be displayed.
		public static IEnumerable<Type> customNodeTypes;
		public static List<string> customNodeNames;


		public static string ArchimatixAssetPath;

		public static void discoverThirdPartyNodes()
		{
            if (Application.isPlaying)
                return;
           
           //Debug.Log("discoverThirdPartyNodes");

            customNodeNames = new List<string>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				customNodeTypes = assembly.GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(AX.Generators.ICustomNode)));
							
				foreach (Type type in customNodeTypes)
				{
					string longname = type.ToString();
					customNodeNames.Add(longname.Substring(longname.LastIndexOf(".") + 1)); 
				}
			}
		}



        // PARSER
        public static Parser parser;

        public static Parser GetMathParser()
        {
            if (parser == null)
                parser = new Parser();

            return parser;
        }

        public static AXScriptParser axScriptParser = new AXScriptParser();
 
    }




  


}