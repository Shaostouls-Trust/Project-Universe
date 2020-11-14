using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering;


using System;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using AX.SimpleJSON;




using AXClipperLib;

using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using Parameters 				= System.Collections.Generic.List<AX.AXParameter>;
using SerializableParameters 	= System.Collections.Generic.List<AX.SerializableParameter>;

using AXGeometry;


using AX.Generators;

namespace AX
{
	public class JSONSerializersAX  {
	
		// It would be preferable to have these serializers in 
		// the classes to keep there private parts private. 
		// Unfortunately, since there are cases where the AssetDatabase needs to be accessed, 
		// these must be in an editor script :-(
		
		
		#region Model JSON
		
		// MODEL JSON SERIALIZATION
		public static string Model_asJSON(AXModel m)
		{
			
			StringBuilder sb = new StringBuilder();
			
			sb.Append( "{" );
			
			sb.Append( "\"guid\":\"" 			+ m.Guid +"\"");
			sb.Append(", \"name\":\"" 			+ m.name +"\"");
			
			string thecomma = "";
			
			// PARAMETRIC_OBJECTS
			sb.Append(", \"parametric_objects\": [");		// begin parametric_objects
			
			thecomma = "";
			foreach(AXParametricObject po in m.parametricObjects)
			{
				sb.Append(thecomma + JSONSerializersAX.ParametricObjectAsJSON(po));
				thecomma = ", ";
			}
			
			sb.Append("]"); 						// end parametric_objects
			
			
			 
			// RELATIONS
			if (m.relations != null && m.relations.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects

				thecomma = "";
				foreach(AXRelation r in m.relations)
				{
					sb.Append(thecomma + r.asJSON());
					thecomma = ", ";
				}
			}
			
			sb.Append("]"); 						// end parametric_objects
			
			
			// end model
			sb.Append("}"); 
			
			return sb.ToString();
		}
		
		
		// COPY OPERATION
		public static string allSelectedPOsAsJson(AXModel m)
		{
			Debug.Log ("allSelectedPOsAsJson");
			//List<string> registry = new List<string>();
			
			// gather relations as you go...
			List<AXRelation> rels = new List<AXRelation>();
			
			StringBuilder sb = new StringBuilder();
			sb.Append( "{" );
			sb.Append(	"\"model_guid\": \""+m.Guid+"\"");
			sb.Append(", \"parametric_objects\": [");		// begin parametric_objects
			
			for(int i=0; i<m.selectedPOs.Count; i++)
			{
				AXParametricObject po = m.selectedPOs[i];
				
				// if this po is an instance, get its source as json to store in the clipboard
				if (po.GeneratorTypeString == "Instance")
				{
					AXParametricObject src_po = po.getParameter("Input Mesh").DependsOn.Parent;
					if (! m.isSelected(src_po))
						po = src_po;
				}
				sb.Append( ((i>0)?",":"") + JSONSerializersAX.ParametricObjectAsJSON(po));
				
				foreach(AXParameter p in po.parameters)
				{
					foreach (AXRelation rr in p.relations)
						if (! rels.Contains(rr))
							rels.Add(rr);
				}







				// SUB_NODES

				if (po.inputsStowed)
				{
					// copy them too!
					po.gatherSubnodes();
					foreach (AXParametricObject spo in po.subnodes)
					{
						sb.Append(',' + JSONSerializersAX.ParametricObjectAsJSON(spo));
						
						// gather relations
						foreach(AXParameter p in spo.parameters)
						{
							foreach (AXRelation rr in p.relations)
								if (! rels.Contains(rr))
									rels.Add(rr);
						}
						
						
					}
				}
			}
			
			
			sb.Append("]"); 						// end parametric_objects
			
			// add relations to json
			string thecomma;
			// RELATIONS
			if (m.relations != null && m.relations.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects

				thecomma = "";
				foreach(AXRelation rr in m.relations)
				{
					sb.Append(thecomma + rr.asJSON());
					thecomma = ", ";
				}
			}
			sb.Append("]");
			
			
			/* to retrieve later...
		if (jn["relations"] != null)
		{
			foreach(AX.SimpleJSON.JSONNode jn_r in jn["relations"].AsArray)
				model.addRelationJSON(jn_r);
		}
		*/
			
			
			sb.Append("}"); // end collection
			
			return sb.ToString();
			
		}
		
		public static string allSelectedPOsAndInputsAsJson(AXModel m)
		{
			Debug.Log ("allSelectedPOsAndInputsAsJson");
			/* We don't know if some of the supart PO's are already selected
		 * so maintain a registry and exclude double entries into the json
		 * 
		 */
			List<string> registry = new List<string>();
			
			// gather relations as you go...
			List<AXRelation> rels = new List<AXRelation>();
			
			StringBuilder sb = new StringBuilder();
			sb.Append( "{" );
			
			Debug.Log("ASIGN:: model_guid="+m.Guid);
			sb.Append(	"\"model_guid\": \""+m.Guid+"\"");


			// ALL PARAMETRIC_OBJECTS IN THIS TREE

			sb.Append(", \"parametric_objects\": [");		// begin parametric_objects
			
			for(int i=0; i<m.selectedPOs.Count; i++)
			{
				AXParametricObject po = m.selectedPOs[i];
				
				if (! registry.Contains(po.Guid)) 
				{
					sb.Append( ((i>0)?",":"") + JSONSerializersAX.ParametricObjectAsJSON(po));
					registry.Add(po.Guid);
				}
				foreach(AXParameter p in po.parameters)
				{
					foreach (AXRelation rr in p.relations)
						if (! rels.Contains(rr))
							rels.Add(rr);
				}


				// SUB_NODES

				po.gatherSubnodes();
				foreach (AXParametricObject spo in po.subnodes)
				{
					if (registry.Contains(spo.Guid)) // don't copy subparts that were selected and already added
						continue;
					sb.Append(',' + JSONSerializersAX.ParametricObjectAsJSON(spo));
					registry.Add(spo.Guid);
					foreach(AXParameter p in spo.parameters)
					{
						foreach (AXRelation rr in p.relations)
							if (! rels.Contains(rr))
								rels.Add(rr);
					}
					
				}
				
			}
			
			sb.Append("]"); 						// end parametric_objects
			
			// add relations to json
			string thecomma;

			// RELATIONS
			if (m.relations != null && m.relations.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects
				
				thecomma = "";
				foreach(AXRelation rr in m.relations)
				{
					sb.Append(thecomma + rr.asJSON());
					thecomma = ", ";
				}
			}
			sb.Append("]");
			
			sb.Append("}"); // end collection
			
			return sb.ToString();
			
		}
		
		public static void pasteFromJSONString(AXModel m, string json_string)
		{
			
			List<AXParametricObject> poList = new List<AXParametricObject>();
			List<AXRelation> relationList = new List<AXRelation>();
			
			
			AXParametricObject po = null;
			Debug.Log (json_string);
			
			AX.SimpleJSON.JSONNode jn = AX.SimpleJSON.JSON.Parse(json_string);
			
			if (jn == null || jn["parametric_objects"] == null)
				return;
			
			foreach(AX.SimpleJSON.JSONNode pn in jn["parametric_objects"].AsArray)
			{
				po = JSONSerializersAX.ParametricObjectFromJSON(pn);
				po.rect = new Rect(po.rect.x+125, po.rect.y+125, po.rect.width, po.rect.height ) ;
				poList.Add (po);
			}
			
			//Debug.Log ("TEST: " + poList.Count + " - " + poList[0].isHead() + " - " + poList[0].generator + " .... " + jn["model_guid"].Value + " - " + m.Guid);
			if (poList.Count == 1 && poList[0].isHead() && jn["model_guid"].Value == m.Guid)
			{
				// common case of copying and pasting a single palette which is a head into the same model
				// as the source po
				// Find the source and instance it.
				AXParametricObject src_po = m.getParametricObjectByGUID(poList[0].Guid);
				List<AXParametricObject> src_List = new List<AXParametricObject>();
				src_List.Add (src_po);
				
				m.instancePOsFromList(src_List);
			}
			
			else
			{
				foreach(AX.SimpleJSON.JSONNode rNode in jn["relations"].AsArray)
					relationList.Add (AXRelation.fromJSON(rNode));
				Debug.Log ("COPYINGING: "+poList.Count +", " + relationList.Count);
				// really duplicate
				m.addAndRigUp(poList, relationList);
			}
			
			
			
			
		}
		
		#endregion
		
		
		
		
		
		#region ParametricObject JSON Serialization
		
		// JSON SERIALIZATION
		public static string ParametricObjectAsJSON(AXParametricObject po)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append("{");
			
			sb.Append(" \"basedOnAssetWithGUID\":\""+ po.basedOnAssetWithGUID +"\"");
			sb.Append(",\"author\":\"" 				+ po.author +"\"");
            sb.Append(",\"tags\":\"" + po.tags + "\""); 
            sb.Append(",\"layer\":\"" + po.layer + "\"");

            sb.Append(",\"guid\":\"" 				+ po.Guid +"\"");
			sb.Append(", \"name\":\"" 				+ po.Name +"\"");
			sb.Append(", \"description\":\"" 		+ po.description +"\"");
			sb.Append(", \"type\":\"" 				+ po.GeneratorTypeString +"\"");
			sb.Append(", \"isInitialized\":\"" 		+ po.isInitialized +"\"");
			sb.Append(", \"documentationURL\":\"" 	+ po.documentationURL +"\"");


			sb.Append(", \"includeInSidebarMenu\":\"" + po.includeInSidebarMenu +"\"");


			sb.Append(",\"grouperKey\":\"" 			+ po.grouperKey +"\"");



			sb.Append(", \"rect\":" 				+ AXJson.RectToJSON				   (po.rect));
			sb.Append(", \"bounds\":" 				+ AXJson.BoundsToJSON			 (po.bounds));
			sb.Append(", \"transMatrix\":" 			+ AXJson.Matrix4x4ToJSON	(po.transMatrix));

			if (po.curve != null && po.curve.Count > 0)
				sb.Append(", \"curve\":" 				+ AXJson.CurveToJson		(po.curve));

			sb.Append(", \"sortval\":\"" 			+ po.sortval +"\"");
			
			sb.Append(", \"combineMeshes\":\"" 			+ po.combineMeshes +"\"");
			sb.Append(", \"isRigidbody\":\"" 			+ po.isRigidbody +"\"");
			sb.Append(", \"colliderType\":\"" 			+ po.colliderType.GetHashCode() +"\"");
            sb.Append(", \"isTrigger\":\"" + po.isTrigger + "\"");

            sb.Append(", \"axStaticEditorFlags\":\"" 	+ po.axStaticEditorFlags.GetHashCode() +"\"");


            // MESH RENDERER OPTIONS

            sb.Append(", \"noMeshRenderer\":\"" + po.noMeshRenderer + "\"");
            sb.Append(", \"lightProbeUsage\":\"" + po.lightProbeUsage + "\"");
            sb.Append(", \"reflectionProbeUsage\":\"" 		+ po.reflectionProbeUsage +"\"");
			sb.Append(", \"shadowCastingMode\":\"" 			+ po.shadowCastingMode +"\"");
			sb.Append(", \"receiveShadows\":\"" 			+ po.receiveShadows +"\"");
			sb.Append(", \"motionVectorGenerationMode\":\"" + po.motionVectorGenerationMode +"\"");


			// material
			//Debug.Log ("ENCODE MATERIAL::::");
			#if UNITY_EDITOR
			if (po.axMat != null && po.axMat.mat != null)
			{  
				string matPath = AssetDatabase.GetAssetOrScenePath(po.axMat.mat);
				if (! String.IsNullOrEmpty(matPath))
					sb.Append(", \"material_assetGUID\":\"" + AssetDatabase.AssetPathToGUID(matPath) +"\"");
			}

			if (po.prefab != null)
			{
				string prefabPath = AssetDatabase.GetAssetOrScenePath(po.prefab);
				if (! String.IsNullOrEmpty(prefabPath))
					sb.Append(", \"prefab_assetGUID\":\"" + AssetDatabase.AssetPathToGUID(prefabPath) +"\"");

			}

			#endif
			
			// code
			if (po.code != null)
				sb.Append(", \"code\":\"" 				+ po.code.Replace("\n",";") +"\"");
			
			
			
			// parameters
			sb.Append(", \"parameters\": [");		// begin parameters
			string the_comma = "";
			foreach(AXParameter p in po.parameters)
			{
				sb.Append(the_comma + p.asJSON());
				the_comma = ", ";
			}
			sb.Append("]"); 						// end parameters
			
			// shapes
			if (po.shapes != null)
			{
				sb.Append(", \"shapes\": [");		// begin parameters
				the_comma = "";
				foreach(AXShape shp in po.shapes)
				{
					sb.Append(the_comma + shp.asJSON());
					the_comma = ", ";
				}
				sb.Append("]"); 						// end parameters
			}
			
			// handles
			
			if (po.handles != null && po.handles.Count > 0)
			{
				sb.Append(", \"handles\": [");		// begin parameters
				the_comma = "";
				foreach(AXHandle h in po.handles)
				{
					sb.Append(the_comma + h.asJSON());
					the_comma = ", ";
				}
				sb.Append("]"); 
			}// end parameters
			
			
			// cameraSettings
			if (po.cameraSettings != null)
			{
				sb.Append( ",\"cameraSettings\":" + JSONSerializersAX.AXCameraAsJSON(po.cameraSettings));
			}
			
			
			
			// finish
			sb.Append("}"); // end parametri_object
			//Debug.Log (sb.ToString());
			return sb.ToString();
		}


		 

		public static AXParametricObject ParametricObjectFromJSON(AX.SimpleJSON.JSONNode jn)
		{

			AXParametricObject po = new AXParametricObject(jn["type"], jn["name"]);


            po.Guid 					= jn["guid"].Value;

			po.basedOnAssetWithGUID		= jn["basedOnAssetWithGUID"].Value;

			po.description 				= jn["description"].Value;
			po.author					= jn["author"].Value;
			po.tags						= jn["tags"].Value;
            po.layer                    = jn["layer"].AsInt;

            po.documentationURL			= jn["documentationURL"].Value;
             
			if (jn["includeInSidebarMenu"] != null)
				po.includeInSidebarMenu		= jn["includeInSidebarMenu"].AsBool;
			else 
				po.includeInSidebarMenu	 = true;
			
			po.isInitialized			= jn["isInitialized"].AsBool;


			po.grouperKey				= jn["grouperKey"].Value;

			if (jn["sortval"] != null)
			po.sortval					= jn["sortval"].AsFloat;

			po.curve  					= AXJson.CurveFromJSON(jn["curve"]);
			
			po.rect  					= AXJson.RectFromJSON(jn["rect"]);
			po.bounds  					= AXJson.BoundsFromJSON(jn["bounds"]);
			po.transMatrix  			= AXJson.Matrix4x4FromJSON(jn["transMatrix"]);

			po.combineMeshes  			= jn["combineMeshes"].AsBool;
			po.isRigidbody  			= jn["isRigidbody"].AsBool;
			po.colliderType  			= (AXGeometry.ColliderType) jn["colliderType"].AsInt ;
            po.isTrigger                = jn["isTrigger"].AsBool;

            po.axStaticEditorFlags		= (AXStaticEditorFlags) jn["axStaticEditorFlags"].AsInt ;



            // MESH RENDERER OPTIONS
            if (jn["noMeshRenderer"] != null)
                po.noMeshRenderer = jn["noMeshRenderer"].AsBool;

            if (jn["lightProbeUsage"] != null)
				po.lightProbeUsage		= (LightProbeUsage) jn["lightProbeUsage"].AsInt ;

			if (jn["reflectionProbeUsage"] != null)
				po.reflectionProbeUsage	= (ReflectionProbeUsage) jn["reflectionProbeUsage"].AsInt ;

			if (jn["shadowCastingMode"] != null)
				po.shadowCastingMode		= (ShadowCastingMode) jn["shadowCastingMode"].AsInt ;
			
			if (jn["receiveShadows"] != null)
				po.receiveShadows			= jn["receiveShadows"].AsBool;

			if (jn["motionVectorGenerationMode"] != null)
				po.motionVectorGenerationMode		= (MotionVectorGenerationMode) jn["motionVectorGenerationMode"].AsInt ;



			//Debug.Log(po.Name + " " + po.grouperKey);

			// material
			// look to see if there is a material matching this name....
			#if UNITY_EDITOR
			if (jn["material_assetGUID"] != null)
			{
				string path = AssetDatabase.GUIDToAssetPath(jn["material_assetGUID"].Value);

				//Debug.Log("AssetDatabase.GUIDToAssetPath("+jn["material_assetGUID"].Value+") has path: "  + path);
				// REDO
				if (path != null)
				{
					//Debug.Log(jn["Name"] + ": path="+path);
					Material matter =  (Material) AssetDatabase.LoadAssetAtPath(path, typeof(Material)) ;

					//Debug.Log("..."+matter);
					//po.axMat.mat = (Material) AssetDatabase.LoadAssetAtPath(path, typeof(Material)) ;
					if (po.axMat == null)
						po.axMat = new AXMaterial();
					po.axMat.mat = matter;
				}
			}
			if (jn["prefab_assetGUID"] != null)
			{
				string path = AssetDatabase.GUIDToAssetPath(jn["prefab_assetGUID"].Value);

				//Debug.Log("AssetDatabase.GUIDToAssetPath("+jn["prefab_assetGUID"].Value+") has path: "  + path);
				// REDO
				if (path != null)
				{
					po.prefab =  (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) ;

				}
			}
			#endif



			// code
			if (jn["code"] != null)
				po.code 			= jn["code"].Value.Replace(";", "\n");
			// parameters

			if (jn["parameters"] != null)
			{
				po.parameters = new List<AXParameter>();

				foreach(AX.SimpleJSON.JSONNode jn_p in jn["parameters"].AsArray)
					po.addParameter(AXParameter.fromJSON(jn_p));
			} 
			// shapes
			if (jn["shapes"] != null)
			{
				po.shapes = new List<AXShape>();
				foreach(AX.SimpleJSON.JSONNode jn_p in jn["shapes"].AsArray)
					po.shapes.Add(AXShape.fromJSON(po, jn_p));
			}
			
			

			// handles
			if (jn["handles"] != null)
			{
				po.handles = new List<AXHandle>();
				foreach(AX.SimpleJSON.JSONNode jn_h in jn["handles"].AsArray)
					po.handles.Add(AXHandle.fromJSON(jn_h));
			}
			
			// cameraSettings
			if (jn["cameraSettings"] != null)
			{
				po.cameraSettings = JSONSerializersAX.AXCameraFromJSON(jn["cameraSettings"]);
				po.cameraSettings.setPosition();
			}
		
			
			// make the generator for this po
			po.instantiateGenerator();
			
			return po;
		}
		
		
		
		
		
		
		#endregion
		
		

		#region AXCamera JSON Serialization
		
		// JSON SERIALIZATION
		public static string AXCameraAsJSON(AXCamera axcam)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append("{");
			
			sb.Append(" \"alpha\":\""	+ axcam.alpha +"\"");
			sb.Append(",\"beta\":\"" 	+ axcam.beta +"\"");
			sb.Append(",\"radius\":\"" 	+ axcam.radius +"\"");
			sb.Append(",\"radiusAdjuster\":\"" 	+ axcam.radiusAdjuster +"\"");
			
			
			// finish
			sb.Append("}"); // end parametri_object
			
			return sb.ToString();
			
		}
				
		public static AXCamera AXCameraFromJSON(AX.SimpleJSON.JSONNode jn)
		{
			AXCamera axcam = new AXCamera();
			
	
			axcam.alpha 				= jn["alpha"].AsFloat;
			axcam.beta 					= jn["beta"].AsFloat;
			axcam.radius 				= jn["radius"].AsFloat;
			axcam.radiusAdjuster 		= jn["radiusAdjuster"].AsFloat;
								
			return axcam;
		}
		
		#endregion
			
	}
}

