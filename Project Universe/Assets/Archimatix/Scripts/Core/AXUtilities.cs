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



using AXGeometry;
using AX;
using AX.Generators;
//using AX.GeneratorHandlers;

using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

// based on: http://forum.unity3d.com/threads/hiding-default-transform-handles.86760/


namespace AX
{


	public class ArchimatixUtils 
	{
		// Horizontal Layout

	



		public static Rect paletteRect;
		public static int PALETTE_TO_FOCUS_ID = 99999; // number larger than the likely number of GUI.Windows in the Editor
		
		public static string versionKey = "ARCHIMATIX_VERSION";
				
		public const  int indent 		= 10;
		public static int indentLevel 	=  0;
		public static int cur_x 		=  0;
		public static int paletteWidth 	=  0;
		
		// Vertical Layout
		public static int cur_y 		=  0;
		public const int lineHgt 		= 16;
		public const int lineHgtSmall 	= 16;
		public const int gap 			= 5;
		
		public static bool doDebug = false;
		
		public static Color[] generatorColorsPro;
		
		//public static int IntPointPrecision = 5000;
			
		public static AXParametricObject lastCopiedPO;
		public static AXParametricObject lastPastedPO;


		public static string guidToKey(string guid)
		{
	
			return "_"+guid.Replace("-", "_");
			
		}
		public static string keyToGuid(string key)
		{
			char[] delims = { '_' };
			return key.Trim(delims).Replace("_", "-");
			
		}


		public static Mesh meshClone(Mesh mesh)
		{

			if (mesh == null)
				return null;

			mesh.RecalculateNormals();

			Vector3[] vertices 		= (Vector3[]) 	mesh.vertices.Clone();
			Vector2[] uv 			= (Vector2[]) 	mesh.uv.Clone();
				int[] triangles 	= (int[]) 		mesh.triangles.Clone();
			Vector3[] normals 		= (Vector3[]) 	mesh.normals.Clone();

				
			Mesh tmpMesh = new Mesh();
			tmpMesh.vertices 	= vertices;
			tmpMesh.uv 			= uv;
			tmpMesh.triangles 	= triangles;
			tmpMesh.normals 	= normals;

			tmpMesh.RecalculateBounds();
			tmpMesh.RecalculateTangents();
			return tmpMesh;
		}

		
		public static Vector2 IntPoint2Float(IntPoint ip)
		{
			return new Vector2((float)ip.X/(float)AXGeometry.Utilities.IntPointPrecision, (float)ip.Y/(float)AXGeometry.Utilities.IntPointPrecision);
		}
		public static CycleList<Transform>   getTransformAncestry(GameObject go)
		{
			CycleList<Transform> _ancestry = new CycleList<Transform>();
			
			// the source GO is always item 0
			
			Transform t = go.transform;
			
			
			for (int i=0; i<25; i++)
			{
				_ancestry.Add(t);
				
				if (t.parent == null)
					break;
				t = t.parent;
			}	
			return _ancestry;
		}
		



		
		public static string[] getMenuOptions(string optionsName) 
		{
			string menu = "Option 1|Option 2|Option 3";
			
			switch (optionsName)
			{
			case "ShapeState":
				menu = "Closed|Open";
				break;
			
			case "CombineType":
				menu = "Difference|Difference Rail|Intersection|Intersection Rail|Union";
				break;
				
			case "ClipType":
				menu = "Intersection|Union|Difference|Xor";
				break;
				 
			case "JoinType":
				menu = "Square|Round|Miter";
				break;
				 
			case "EndType":
				menu =  "ClosedPolygon|ClosedLine|OpenButt|OpenSquare|OpenRound";
				break;
				
			case "OpenEndType":
				menu =  "Butt|Square|Round";
				break;

            case "Axis":
                menu = "None|X|Y|Z|-X|-Y|-Z";
                break;

            case "ProjectionAxis":
                menu = "None|X|Y|Z|-X|-Y|-Z";
                break;

                case "Align_X":
				menu = "None|Left|Center|Right";
				break;
				
			case "Align_Y":
				menu = "None|Top|Center|Bottom";
				break;
				
			case "Align_Z":
				menu = "None|Front|Center|Back";
				break;
			
				
			}
			
			return menu.Split('|');
		}



		// find all 3rd-party nodes

		private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
		{
		    return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
		}





		public static void generateModel(AXModel model)
		{
			model.generate ();
		
		}



        public static AXGameObject addAXGameObject(GameObject obj, AXParametricObject po, string subItemAddress = "")
        {
            AXGameObject axgo = obj.AddComponent<AXGameObject>();
            axgo.makerPO_GUID = po.Guid;

            // STATIC FLAGS
            #if UNITY_EDITOR
            // *** Could temporarily remove the Lightmap static flag here to allow for Auto... http://forum.unity3d.com/threads/how-to-set-use-staticeditorflags-cant-seem-to-set-them-from-script.137024/
            // flags = flags & ~(StaticEditorFlags.LightmapStatic);

            AXStaticEditorFlags axflags = po.axStaticEditorFlags;
            if (!po.model.staticFlagsEnabled)
            {
                //var mask = ~AXStaticEditorFlags.LightmapStatic;
                //var newValue = originalValue & mask;
                axflags = axflags & ~AXStaticEditorFlags.LightmapStatic;

            }
            StaticEditorFlags flags = (StaticEditorFlags)axflags;


            GameObjectUtility.SetStaticEditorFlags(obj, flags);


            #endif


            if (!string.IsNullOrEmpty(po.tag))
                obj.tag = po.tag;
            obj.layer = po.layer;

            //Debug.Log ("copying protos");
            po.copyComponentsFromPrototypeToGameObject(obj);

            return axgo;
        }



        public static GameObject createAXGameObject(string name, AXParametricObject po, string subItemAddress = "")
		{
			GameObject obj = new GameObject(name);
			AXGameObject axgo = obj.AddComponent<AXGameObject>();
			axgo.makerPO_GUID = po.Guid;
			 

			// STATIC FLAGS
			#if UNITY_EDITOR  
			// *** Could temporarily remove the Lightmap static flag here to allow for Auto... http://forum.unity3d.com/threads/how-to-set-use-staticeditorflags-cant-seem-to-set-them-from-script.137024/
			// flags = flags & ~(StaticEditorFlags.LightmapStatic);

			AXStaticEditorFlags axflags = po.axStaticEditorFlags;
			if (! po.model.staticFlagsEnabled)
			{	
				//var mask = ~AXStaticEditorFlags.LightmapStatic;
				//var newValue = originalValue & mask;
				axflags = axflags & ~AXStaticEditorFlags.LightmapStatic;

			}
			StaticEditorFlags flags = (StaticEditorFlags) axflags;


			GameObjectUtility.SetStaticEditorFlags(obj, flags);


			#endif
			 

			if (! string.IsNullOrEmpty(po.tag))
				obj.tag 	= po.tag;
			obj.layer 	= po.layer;

			//Debug.Log ("copying protos");
			po.copyComponentsFromPrototypeToGameObject(obj);


			return obj;
			 
		}

		public static GameObject AddAXGameObjectTo(AXParametricObject po, GameObject ngo)
		{
			//GameObject obj = new GameObject(prefab.name);

			//GameObject ngo = GameObject.Instantiate(go);


			AXGameObject axgo = ngo.AddComponent<AXGameObject>();
			axgo.makerPO_GUID = po.Guid;
			 

			// STATIC FLAGS
			#if UNITY_EDITOR  
			// *** Could temporarily remove the Lightmap static flag here to allow for Auto... http://forum.unity3d.com/threads/how-to-set-use-staticeditorflags-cant-seem-to-set-them-from-script.137024/
			// flags = flags & ~(StaticEditorFlags.LightmapStatic);

			AXStaticEditorFlags axflags = po.axStaticEditorFlags;
			if (! po.model.staticFlagsEnabled)
			{	
				axflags = axflags & ~AXStaticEditorFlags.LightmapStatic;
			}
			StaticEditorFlags flags = (StaticEditorFlags) axflags;


			GameObjectUtility.SetStaticEditorFlags(ngo, flags);
			
			#endif
			 

			if (! string.IsNullOrEmpty(po.tag))
				ngo.tag 	= po.tag;
			ngo.layer 	= po.layer;

			po.copyComponentsFromPrototypeToGameObject(ngo);


			return ngo;
			 
		}










		public static Type AXGetType( string TypeName )
		{
            // This should be called only when creating a node

            // http://answers.unity3d.com/questions/206665/typegettypestring-does-not-work-in-unity.html



            //if (TypeName.Contains("Handler"))
            //{
            //    if (! TypeName.Contains("AX.GeneratorHandlers"))
            //        TypeName = "AX.GeneratorHandlers." + TypeName;
            //}
            //else 

            if (!TypeName.Contains("AX.Generators") && !TypeName.Contains("Handler"))
                TypeName = "AX.Generators." + TypeName;

        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, in the same assembly as the caller, etc.
        var type = Type.GetType( TypeName );

		
		// If it worked, then we're done here
		//if( type != null && (typeof(Generator).IsAssignableFrom(type) ||  TypeName.Contains("Handler")))
		if( type != null)
            {

                return type;
            }
           


		// If we still haven't found the proper type, we can enumerate all of the 
		// loaded assemblies and see if any of them define the type

			//sDebug.Log("AXGetType: " + TypeName);
			//if (TypeName.Contains("AX.GeneratorHandlers"))
			if (TypeName.Contains("Handler"))
			{
				var currentAssembly = Assembly.GetExecutingAssembly();
				var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
				foreach( var assemblyName in referencedAssemblies )
				{
					
					// Load the referenced assembly
					var assembly = Assembly.Load( assemblyName );
					if( assembly != null )
					{
						// See if that assembly defines the named type
						type = assembly.GetType( TypeName );
						if( type != null )
							return type;
					}
				}

			}




		//foreach( var assemblyName in referencedAssemblies )
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{

            //if (assembly != null && assembly.GetName().ToString().Contains("Assembly-CSharp"))
            if (assembly != null && assembly.GetName().ToString().Contains("Assembly-CSharp") || assembly.GetName().ToString().Contains("Archimatix"))
            {
                foreach (Type t in assembly.GetExportedTypes())
   				{
					if (t.Name == TypeName)
					{
						//Debug.Log("***** ****** *****>>> " +TypeName + " -- " + assembly.GetName());
   				 		return t;
   				 	}
   				}
 			}
		}



            ////foreach( var assemblyName in referencedAssemblies )
            //foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            //{

            //    if (assembly != null && (assembly.GetName().ToString().Contains("Assembly-CSharp") || assembly.GetName().ToString().Contains("Archimatix")))
            //    {

            //        foreach (Type t in assembly.GetExportedTypes())
            //        {
            //            if (t.Name == TypeName)
            //            {
            //                //Debug.Log("***** ****** *****>>> " +TypeName + " -- " + assembly.GetName());
            //                return t;
            //            }
            //        }
            //    }
            //}

            // The type just couldn't be found...
            Debug.Log("TypeName Not found: " + TypeName);
		return null;
		
	}

		
		public static string getRelativeFilePathOLD(string filepath)
		{
			// if it is an absolute path (starts with Application dataPath
			// knock off the dataPath...
			
			
			
			
			Debug.Log ("Application.dataPath="+Application.dataPath+", filepath="+filepath);
			if (filepath.StartsWith(Application.dataPath)) {
				Debug.Log ("YES, IT DOES BEGIN WITH DATA_PATH: " +filepath.Substring(Application.dataPath.Length) );
				return "Assets" + filepath.Substring(Application.dataPath.Length);
			}
			else
			{
				Debug.Log ("HMMM, IT DOES NOT BEGIN WITH DATA_PATH " );
				
			}
			return filepath;
		
		}
		
		
		// As it turns out, it is not easy to extract a relative path since
		// the System.IO.Path class does not have cross platform substring functions
		// This solution using System.Uri is based on example at:
		// http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
		public static String getRelativeFilePath(String absolutePath)
		{
			if (String.IsNullOrEmpty(absolutePath))   throw new ArgumentNullException("toPath");
			
			Uri fromUri 	= new Uri(Application.dataPath);
			Uri toUri 		= new Uri(absolutePath);
			
			if (fromUri.Scheme != toUri.Scheme) { return absolutePath; } // path can't be made relative.
			
			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			String relativePath = Uri.UnescapeDataString(relativeUri.ToString());
			
			if (toUri.Scheme.ToUpperInvariant() == "FILE")
			{
				relativePath = relativePath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
			}
			// [not sure why this is needed here to correct the filepath, but not in the Library Save dalog...]
			relativePath = relativePath.Replace('\\', '/');

			return relativePath;
		}
		
		public static string getAbsoluteLibraryPath(string relativePath)
		{
			return Application.dataPath + relativePath.Replace("Assets", "");
		}
		
		
		
		
		


		//public static Paths transformPolytree(AXParameter p)
		//{

			//p.parametricObject.transMatrix = Matrix4x4.TRS(new Vector3(transX, transY, 0), Quaternion.Euler(0, 0, rotZ), new Vector3(1,1,1));



		//}

		

		
		public static Vector3[] loft(Vector3[] verts, float height)
		{
			Vector3[] nverts = verts;
			for (int i=0; i<nverts.Length; i++)
				nverts[i] = new Vector3(nverts[i].x, height,  nverts[i].z);
				
			return nverts;
		}
		
		
	
			
			
			
			
			
			
		
		
		
		
		void displayAnyAXModelInScene() 
		{
			//AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];
			
			//if (axModels != null && axModels.Length > 0)
			//	displayAXModel(axModels[0]);
			
		}
		
		public static AXModel[] getAllModels()
		{
			return GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];
		}
		
			
	}
	
		
		
	 

	public class AXUtilities {

		public static string swapOutGuids(string po_p_guid, ref Dictionary<string, string> guidMap)
		{
			// Replace any guids in the string with new ones from the guidmap
			// and return the new string
				
			//Debug.Log ("swapOutGuids for " + po_p_guid);
			string[] guids = po_p_guid.Split('%');
	
			if ( guids.Length>1 && guidMap.ContainsKey(guids[0]) && guidMap.ContainsKey(guids[1]) )
			{
				guids[0] = guidMap[guids[0]]; // replace po guid
				guids[1] = guidMap[guids[1]]; // replace p guid
				return guids[0]+"%"+guids[1];
			}
			else if (guidMap.ContainsKey(guids[0]))
			{
					//Debug.Log (" -- " + guidMap[po_p_guid]);
					return guidMap[po_p_guid];
			}
	
			// return original
			return po_p_guid;
		}
		
	
	
	
	
		public static Matrix4x4 Plane2Matrix(Plane p, Vector3 origin)
		{
			Quaternion q =  Quaternion.FromToRotation(Vector3.up, p.normal);
			
			return Matrix4x4.TRS(origin, q, new Vector3(1,1,1));
			
		
		}
	 
		
		public static Quaternion QuaternionFromMatrix(Matrix4x4 m) 
		{ 
			Quaternion rot = Quaternion.identity;
			// from Leto: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
			
			if (m == Matrix4x4.identity)
				return rot;

			if (float.IsNaN (m.m00))
				return rot;
			
			if (AXUtilities.GetScale(m) == Vector3.zero)
				return rot;


			try {
				rot =  Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); 
			} catch {
				return rot;
			}
			return rot;
		}
	
	 
		
		public Quaternion QuaternionFromMatrixAlternate(Matrix4x4 m) 
		{ 
			// from Leto: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
			return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); 
			
			/*
			Quaternion q = new Quaternion();
			q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
			q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
			q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
			q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
			q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
			q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
			q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
			return q;
			*/
			
		}
		
		
		
		
		public static Vector3 GetPosition(Matrix4x4 m)
		{
			// From runevision: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
			return m.GetColumn(3);
		}
		
		public static Vector3 GetScale(Matrix4x4 m)
		{
			var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
			var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
			var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
			
			return new Vector3(x, y, z);
		}
	
	
	






		public static Rect getBoundaryRectFromPOs(List<AXParametricObject> pos, float margin=50)
		{

			float minx = 10000;
			float miny = 10000;


			float maxx = -10000;
			float maxy = -10000;



			for (int i=0; i<pos.Count; i++)
			{
				if (! pos[i].isOpen)
					continue;

				if (float.IsNaN(pos[i].rect.x))
					pos[i].rect.x = 0;
				if (float.IsNaN(pos[i].rect.y))
					pos[i].rect.y = 0;

				Rect r = pos[i].rect;


				if (r.x < minx)
					minx = r.x;

				if (r.x+r.width > maxx)
					maxx = r.x+r.width;

				if (r.y < miny)
					miny = r.y;

				if (r.y+r.height > maxy)
					maxy = r.y+r.height;



			}
			return new Rect(minx-margin, miny-margin, maxx-minx+2*margin, maxy-miny+2*margin);
		}


		public static void movePOsToRightOfPO(AXParametricObject po, List<AXParametricObject> poList)
		{
			Rect r = getBoundaryRectFromPOs(poList);

			float target_x = po.rect.x + po.rect.width + 100;
			float target_y = po.rect.y - po.rect.height/2 + 200;

			// get the r to target_x

			float displ_x = r.x - target_x;

			float displ_y = r.y - target_y;

			if (displ_x != 0 || displ_y != 0)
			{
				for (int i = 0; i < poList.Count; i++) 
				{
					poList[i].rect.x -= displ_x;
					poList[i].rect.y -= displ_y;
				}

			}


		}

		public static float getMinXFromPOs(List<AXParametricObject> pos)
		{
			float minx = 10000;

			for (int i=0; i<pos.Count; i++)
			{
				if (! pos[i].isOpen)
					continue;

				Rect r = pos[i].rect;

				if (r.x < minx)
					minx = r.x;

			}

			return minx;

		}




		public static AXParametricObject clonePO(AXParametricObject po, bool sameGuid = false)
		{
			AXParametricObject npo = new AXParametricObject(po.GeneratorTypeString, po.Name);

			if (sameGuid)
				npo.Guid = po.Guid;


			npo.basedOnAssetWithGUID 	= po.basedOnAssetWithGUID;
			npo.author 					= po.author;
			npo.tags 					= po.tags;
			npo.Guid 					= po.Guid;
			npo.description 			= po.description;
			npo.isInitialized 			= po.isInitialized;
			npo.documentationURL 		= po.documentationURL;
			npo.includeInSidebarMenu 	= po.includeInSidebarMenu;
			npo.grouperKey 				= po.grouperKey;
			npo.rect 					= po.rect;
			npo.bounds 					= po.bounds;
			npo.transMatrix 			= po.transMatrix;
			npo.curve 					= po.curve;
			npo.sortval 				= po.sortval;
			npo.combineMeshes 			= po.combineMeshes;
			npo.isRigidbody 			= po.isRigidbody;
			npo.colliderType 			= po.colliderType;
			npo.axStaticEditorFlags 	= po.axStaticEditorFlags;
			npo.lightProbeUsage 		= po.lightProbeUsage;
			npo.reflectionProbeUsage 	= po.reflectionProbeUsage;
			npo.shadowCastingMode 		= po.shadowCastingMode;
			npo.receiveShadows 			= po.receiveShadows;
			npo.motionVectorGenerationMode 	= po.motionVectorGenerationMode;
			npo.axMat 					= po.axMat;
			npo.prefab 					= po.prefab;
			npo.code 					= po.code;
//			npo. 	= po.;
//			npo. 	= po.;
//			npo. 	= po.;

			npo.parameters = new List<AXParameter>();
			foreach(AXParameter p in po.parameters)
			{
				AXParameter cp = p.clone(sameGuid);
				cp.parametricObject = po;

				npo.parameters.Add(cp);
			}
			// shapes
//			if (po.shapes != null)
//			{
//				npo.shapes = new List<AXShape>();
//				foreach(AXShape shp in po.shapes)
//				{
//					npo.shapes.Add(shp.cl
//				}
//			}



			return npo;
		}


		public static AXParametricObject duplicatePO(AXParametricObject po)
		{

			List<AXParametricObject> poList = new List<AXParametricObject>();
			List<AXRelation> rels = new List<AXRelation>();


			Dictionary<string, AXParameter> clonedParameters = new Dictionary<string, AXParameter>();

			Dictionary<string, string> relationKeys = new Dictionary<string, string>();


			// first record all relation GUIDS
			foreach (AXParameter p in po.parameters)
				if (p.DependsOn != null)
				{
					relationKeys.Add(p.Guid, p.DependsOn.Guid);
					Debug.Log("Adding rel keys: "+p.Name+" : "+p.Guid + " ________  " + p.DependsOn.Name + " : " +p.DependsOn.Guid);
				}
			
			Debug.Log(relationKeys.Count);




			AXParametricObject npo  = clonePO(po, true);
			npo.Name = "Box2";
			poList.Add(npo);

			foreach (AXParameter p in npo.parameters)
				clonedParameters.Add(p.Guid, p);




			po.gatherSubnodes();

			foreach (AXParametricObject spo in po.subnodes)
			{
               // Debug.Log(spo.Name);
				AXParametricObject nnpo = clonePO(spo, true);

				poList.Add(nnpo);

				foreach(AXParameter p in spo.parameters)
				{
					clonedParameters.Add(p.Guid, p);

					if (p.DependsOn != null)
					{
						Debug.Log("Adding rel keys");
						relationKeys.Add(p.Guid, p.DependsOn.Guid);
					}
				}
//				// gather rels
					foreach(AXParameter p in spo.parameters)
					{
						foreach (AXRelation rr in p.relations)
							if (! rels.Contains(rr))
								rels.Add(rr);
					}
//				}
			}


			// cloned relations
			foreach(AXParametricObject _po in poList)
			{
                //Debug.Log("**** " + _po.Name);
				foreach(AXParameter p in _po.parameters)
				{
					//Debug.Log("contains? "+p.parametricObject.Name+"."+p.Name+" : " + p.Guid);
					if (relationKeys.ContainsKey( p.Guid))
					{
						Debug.Log("******* " + clonedParameters[relationKeys[p.Guid]]);
						p.DependsOn = clonedParameters[relationKeys[p.Guid]];
						p.dependsOnKey = relationKeys[p.Guid];
					}


					//Debug.Log(p.Name + " -- " + p.DependsOn + " :: " + p.relations.Count);


					foreach(AXRelation r in p.relations)
					{
						Debug.Log("RELATION");
						AXRelation nr = new AXRelation(clonedParameters[r.pA_guid], clonedParameters[r.pB_guid]);
						nr.expression_AB = r.expression_AB;
						nr.expression_BA = r.expression_BA;

						rels.Add(nr);
					}
				}
			}

			AXParametricObject mostRecentPO = po.model.recentlySelectedPO;

			po.model.addAndRigUp(poList, rels);

			npo.startStowInputs();

			float pos_x = (po.model.focusPointInGraphEditor.x)+100;// + UnityEngine.Random.Range(-100, 300);
			float pos_y = (po.model.focusPointInGraphEditor.y - 250) + UnityEngine.Random.Range(-10, 0);

			if(mostRecentPO != null)
			{
				pos_x =  mostRecentPO.rect.x + 200;
				pos_y =  mostRecentPO.rect.y;
			}

			//Debug.Log (new Rect(pos_x, pos_y, 150, 100));
			npo.rect = new Rect(pos_x, pos_y, 150, 500);



			return npo;

		}






	
	} // \AXUtilities
	 
	
	public struct MathParseResults
	{
		public List<string> symbolsFound;
		public float result;
	}

	
	
	
	
	public class CycleList <T> : System.Collections.Generic.List<T>
	{	
		private int currentIndex;
		
		public void reset()
		{
			currentIndex = 0;
		}
		
		public T current
		{
			get 
			{
				return this[currentIndex];
			}
		}
		
		
		public T next
		{
			get
			{
				//Debug.Log("currentIndex="+currentIndex);
				currentIndex++;
				if (currentIndex == Count) // because the last two are the model and the generatedGameObjects
					currentIndex = 0;
				
				return this[currentIndex];
			}
			
		}
		
		
	}		
	
	
	
	
	
	
	
	
	
	
	
	
	




















	public class AXJson
	{
		// VECTOR2
		public static string Vector2ToJSON(Vector2 v)
		{
			return "["+v.x+","+v.y+"]";
		}
		public static Vector2 Vector2FromJSON(AX.SimpleJSON.JSONNode jn)
		{
			return new Vector2(jn[0].AsFloat, jn[1].AsFloat);
		}
		
		// VECTOR3
		public static string Vector3ToJSON(Vector3 v)
		{
			return "["+v.x+","+v.y+","+v.z+"]";
		}
		public static Vector3 Vector3FromJSON(AX.SimpleJSON.JSONNode jn)
		{
			
			return new Vector3(jn[0].AsFloat, jn[1].AsFloat, jn[2].AsFloat);
		}
	
		// VECTOR4
		public static string Vector4ToJSON(Vector4 v)
		{
			return "["+v.x+","+v.y+","+v.z+","+v.w+"]";
		}
		public static Vector4 Vector4FromJSON(AX.SimpleJSON.JSONNode jn)
		{
			return new Vector4(jn[0].AsFloat, jn[1].AsFloat, jn[2].AsFloat, jn[3].AsFloat);
		}
	
		
		// RECT
		public static string RectToJSON(Rect rect)
		{
			
			return "["+rect.x+","+rect.y+","+rect.width+","+rect.height+"]";
		}
		public static Rect RectFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			
			return new Rect(jn[0].AsFloat, jn[1].AsFloat, jn[2].AsFloat, jn[3].AsFloat);
		}
	
	
		// BOUNDS
		public static string BoundsToJSON(Bounds b)
		{
			return "{\"center\":"+Vector3ToJSON(b.center)+", \"size\":"+Vector3ToJSON(b.size)+"}";
		}
		public static Bounds BoundsFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			Bounds b 	= new Bounds(); 
			b.center 	= new Vector3(jn["center"][0].AsFloat,   jn["center"][1].AsFloat,   jn["center"][2].AsFloat);
			b.size 		= new Vector3(  jn["size"][0].AsFloat,     jn["size"][1].AsFloat,     jn["size"][2].AsFloat);
			b.extents 	= b.size/2;
			return b;
		}

		// RECT
		public static string ColorToJSON(Color c)
		{
			return "["+c.r+","+c.g+","+c.b+","+c.a+"]";
		}
		public static Color ColorFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			
			return new Color(jn[0].AsFloat, jn[1].AsFloat, jn[2].AsFloat, jn[3].AsFloat);
		}

	
		// MATRIX4X4
		public static string Matrix4x4ToJSON(Matrix4x4 m)
		{
			return "["+Vector4ToJSON(m.GetColumn(0))+","+Vector4ToJSON(m.GetColumn(1))+","+Vector4ToJSON(m.GetColumn(2))+","+Vector4ToJSON(m.GetColumn(3))+"]";
		}
		public static Matrix4x4 Matrix4x4FromJSON(AX.SimpleJSON.JSONNode jn)
		{
			Matrix4x4 m = new Matrix4x4(); 
	
			m.SetColumn(0, Vector4FromJSON(jn[0]));
			m.SetColumn(1, Vector4FromJSON(jn[1]));
			m.SetColumn(2, Vector4FromJSON(jn[2]));
			m.SetColumn(3, Vector4FromJSON(jn[3]));
	
			return m;
		}
	
		// LIST<STRING>
		public static string StringListToJSON(List<string> list)
		{
			StringBuilder sb = new StringBuilder();
	
			sb.Append("[");
	
			string the_comma = "";
			foreach (string s in list)
			{
				sb.Append(the_comma + "\""+s+"\"");
				the_comma = ", ";
			}
			sb.Append("]");
	
			return sb.ToString();
		}
		public static List<string> StringListFromJSON(AX.SimpleJSON.JSONNode N)
		{
			List<string> list = new List<string>();
			foreach(AX.SimpleJSON.JSONNode jn in N.AsArray)
				list.Add(jn.Value);
	
			return list;
		}
		




		// LIST<FLOAT>
		public static string FloatListToJSON(List<float> list)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("[");

			string the_comma = "";
			foreach (float f in list)
			{
				sb.Append(the_comma +f);
				the_comma = ", ";
			}
			sb.Append("]");

			return sb.ToString();
		}
		public static List<float> FloatListFromJSON(AX.SimpleJSON.JSONNode N)
		{
			List<float> list = new List<float>();
			// Select(n => n.AsFloat).ToArray();;
			foreach(AX.SimpleJSON.JSONNode jn in N.AsArray)
				list.Add(float.Parse(jn.Value));

			return list;
		}







		
		// CURVE_POINT
		public static string CurvePointToJSON(CurveControlPoint2D cp)
		{

			StringBuilder sb = new StringBuilder();

			sb.Append( "{" );

			sb.Append("\"curvePointType\":" 			+  cp.curvePointType.GetHashCode());

			sb.Append(", \"position\":" 				+ AXJson.Vector2ToJSON(cp.position));
			sb.Append(", \"localHandleA\":" 			+ AXJson.Vector2ToJSON(cp.localHandleA));
			sb.Append(", \"localHandleB\":" 			+ AXJson.Vector2ToJSON(cp.localHandleB));

			// finish
			sb.Append("}");  

			return sb.ToString();
		}

		public static CurveControlPoint2D CurvePointFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			
			CurveControlPoint2D cp = new CurveControlPoint2D(Vector2FromJSON(jn["position"]));

			cp.curvePointType	= (CurvePointType) jn["curvePointType"].AsInt;
			  
			if (jn["localHandleA"] != null && string.IsNullOrEmpty(jn["localHandleA"]))
				cp.localHandleA 	= Vector2FromJSON(jn["localHandleA"]);
			if (jn["localHandleB"] != null && string.IsNullOrEmpty(jn["localHandleB"]))
				cp.localHandleB 	= Vector2FromJSON(jn["localHandleB"]);


			return cp;

		}
			



	
		public static string CurveToJson(Curve curve)
		{
				StringBuilder sb = new StringBuilder();
				
				sb.Append("[");
				
				string the_comma = "";
				foreach (CurveControlPoint2D cp in curve)
				{
					sb.Append(the_comma + CurvePointToJSON(cp));
					the_comma = ", ";
				}
				sb.Append("]");
				
				return sb.ToString();
				
		
		}
		public static Curve CurveFromJSON(AX.SimpleJSON.JSONNode N)
		{

			Curve curve = new Curve();
			foreach(AX.SimpleJSON.JSONNode jn in N.AsArray)
				curve.Add(CurvePointFromJSON(jn));
			
			return curve;
		}
			




		// ANIMATION_CURVE

		public static string AnimationCurveToJSON(AnimationCurve ac)
		{
			if (ac == null)
				return "";
			StringBuilder sb = new StringBuilder();
				
			sb.Append("[");

			string the_comma = "";
			foreach (Keyframe kf in ac.keys)
			{
				sb.Append(the_comma + KeyframeToJSON(kf));
				the_comma = ", ";
			}
			sb.Append("]");
			
			return sb.ToString();
		}

		public static AnimationCurve AnimationCurveFromJSON(AX.SimpleJSON.JSONNode N)
		{
			AnimationCurve curve = new AnimationCurve();
			curve.keys = new Keyframe[N.AsArray.Count];

			for (int i = 0; i < N.AsArray.Count; i++) {
				AX.SimpleJSON.JSONNode jn = N.AsArray [i];
				Keyframe k = KeyframeFromJSON (jn);
				curve.AddKey(k);
			}
			
			return curve;
		}

		public static string KeyframeToJSON(Keyframe kf)
		{
			StringBuilder sb = new StringBuilder();
				
			sb.Append("{");

			sb.Append("  \"inTangent\":" 		+ kf.inTangent);
			sb.Append(", \"outTangent\":" 		+ kf.outTangent);
			sb.Append(", \"time\":" 		    + kf.time);
			sb.Append(", \"value\":" 		    + kf.value);

			sb.Append("}");
			
			return sb.ToString();
		}

		public static Keyframe KeyframeFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			Keyframe kf = new Keyframe();
				
			kf.inTangent  		= jn["inTangent"].AsFloat;
			kf.outTangent  		= jn["outTangent"].AsFloat;
			kf.time  			= jn["time"].AsFloat;
			kf.value  			= jn["value"].AsFloat;


			return kf;
		}

	
	}
	










}