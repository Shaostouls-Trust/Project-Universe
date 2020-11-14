#pragma warning disable

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using AX.SimpleJSON;



using AXGeometry;

namespace AX
{
	
	[System.Serializable]
	public class AXRelation {
	
		/// This class is meant mostly for 2-way relationships between floats.
		/// It allows for an equation and its inverse that transform the float 
		/// during propagation of a value throughout the graph.
		/// 
		/// A link is established between two Parameters, A and B. 
		/// Each Parameter has a list of Relations. This is a doubly linked list.
		/// 
		/// pA <--> relation <--> pB
		///   
	
		public string pA_guid;
		public string pB_guid;
	
		///  These expressions are used when propagateing values.
		///  One is often the mathematical inverse of the other.
		
		public string expression_AB; 
		public string expression_BA;
		
		public string expression_AB_prev; 
		public string expression_BA_prev;
		
	
		// These are live values only - for convenience
		// * don't serialize these, it will only make copies on deserialize
		[System.NonSerialized]
		private AXParameter		m_pA			= null;
		public 	AXParameter		  pA   
		{
			get { return m_pA; }
			set { m_pA = value; }
		} 
		
		[System.NonSerialized]
		private AXParameter		m_pB			= null;
		public 	AXParameter		  pB   
		{
			get { return m_pB; }
			set { m_pB = value; }
		}


		public bool isVisibleInGraphEditor;

		Vector2 startPosition;
		Vector2 endPosition;
		Vector2 startTangent;
		Vector2 endTangent;


		public string toString()
		{
	 		if (pA == null || pB == null)
	 		{


	 			return "null values in Relation=";
	 		}
			return ("relation: "+pA.Parent.Name+"."+pA.Name+"---"+pB.Parent.Name+"."+pB.Name+" ... pA_guid=" + pA_guid + ", pB_guid=" + pB_guid + ", expression_AB=" + expression_AB + ", expression_BA=" + expression_BA);
		}
	 
		// JSON SERIALIZATION
		public string asJSON()
		{
			/* At some point, we could use a more generalized serializer,
			 * but it would have to recognize the NonSerialized members and not pursue 
			 * their graph links
			 * 
			 */
			
			StringBuilder sb = new StringBuilder();
			
			sb.Append("{");
			
			sb.Append(    "\"pA_guid\":\"" 			+ pA_guid 		+"\"");
			sb.Append(  ", \"pB_guid\":\"" 			+ pB_guid 		+"\"");
			
			sb.Append(  ", \"expression_AB\":\"" 			+ expression_AB 		+"\"");
			sb.Append(  ", \"expression_BA\":\"" 			+ expression_BA 		+"\"");
			
			sb.Append(  ", \"expression_AB_prev\":\"" 			+ expression_AB_prev 		+"\"");
			sb.Append(  ", \"expression_BA_prev\":\"" 			+ expression_BA_prev 		+"\"");
			
	
			sb.Append("}");
			
			return sb.ToString();
		}
		
		public static AXRelation fromJSON(AX.SimpleJSON.JSONNode jn)
		{
			AXRelation r = new AXRelation();
	
			r.pA_guid 				= jn["pA_guid"];
			r.pB_guid 				= jn["pB_guid"];
			
			r.expression_AB 		= jn["expression_AB"];
			r.expression_BA 		= jn["expression_BA"];
	
			r.expression_AB_prev 	= jn["expression_AB_prev"];
			r.expression_BA_prev 	= jn["expression_BA_prev"];
			
			
			return r;
		}
		
	
	
		public AXRelation()
		{
	
		}
	
		public AXRelation(AXParameter _pA, AXParameter _pB)
		{
			if (_pA != null && _pB != null)
			{
			
			//Debug.Log ("Adding relation :: "+_pA.Parent.Name+"."+_pA.Name+" "+_pB.Parent.Name+"."+_pB.Name);
			pA = _pA;
			pB = _pB;
	
			pA_guid = pA.Guid;
			pB_guid = pB.Guid;
	
			// one expression would normally want to be the inverse of the other:
			//expression_AB = "IN";
			//expression_BA = "IN";
			expression_AB = ArchimatixUtils.guidToKey(pA.Guid);
			expression_BA = ArchimatixUtils.guidToKey(pB.Guid);
			}
	
		}
	
	
	
		public void remapGuids(ref Dictionary<string, string> guidMap)
		{

			if ( ! guidMap.ContainsKey(pA_guid) || ! guidMap.ContainsKey(pB_guid))
				return;

			string old_pA_guid_key = ArchimatixUtils.guidToKey(pA_guid);
			string old_pB_guid_key = ArchimatixUtils.guidToKey(pB_guid);
			
			string new_pA_guid_key = ArchimatixUtils.guidToKey(guidMap[pA_guid]);
			string new_pB_guid_key = ArchimatixUtils.guidToKey(guidMap[pB_guid]);
			
			//Debug.Log("+++ remapGuids pA_guid: " + old_pA_guid_key + "==="+new_pA_guid_key);
			
			//Debug.Log("=====> expression_AB (old): " + expression_AB);
			// EXPRESSIONS

			if (expression_AB != null)
				expression_AB = expression_AB.Replace(old_pA_guid_key, new_pA_guid_key);
			if (expression_BA != null)
				expression_BA = expression_BA.Replace(old_pB_guid_key, new_pB_guid_key);
			
			// ACTUAL PARAMETER GUIDS
			// replace old guids in the strings with the new guids
			pA_guid = AXUtilities.swapOutGuids(pA_guid, ref guidMap);
			pB_guid = AXUtilities.swapOutGuids(pB_guid, ref guidMap);
	
			// later, swap guids in pA_expression && pB
	
		}
	
	
		 
		 
		public void setupReferencesInModel(AXModel model) 
		{
			//Debug.Log ("SETTING UP REFERENCES IN MODEL pA_guid="+pA_guid);
			// USE THIS TO INITIALIZE REFERENCES AFTER DESERIALIZATION
			
			if (pA_guid != null && pB_guid != "") 
			{
				// - pA
				pA = model.getParameterByGUID(pA_guid);
				//Debug.Log ("AXRelation::setupReferencesInModel - relations.add pA="+pA+" : " + pA.Guid);
				//if (pA != null) 
				//	pA.relations.Add (this);
				
				// - pA
				pB = model.getParameterByGUID(pB_guid);
				//Debug.Log ("AXRelation::setupReferencesInModel - relations.add pB="+pB+" : " + pB.Guid);
				//if (pB != null) 
				//	pB.relations.Add (this);
				
				
			}
			//Debug.Log (toString ());
		}
		
		public void setupFullReferencesInModel(AXModel model) 
		{
			//Debug.Log ("SETTING UP REFERENCES IN MODEL pA_guid="+pA_guid);
			// USE THIS TO INITIALIZE REFERENCES AFTER DESERIALIZATION
			
			if (pA_guid != null && pB_guid != "") 
			{
				// - pA
				//Debug.Log ("pA_guid="+pA_guid);
				pA = model.getParameterByGUID(pA_guid);
				if (pA != null && ! pA.relations.Contains(this)) 
				{
					//Debug.Log ("AXRelation::setupReferencesInModel - relations.add pA="+pA.Name+" : " + pA.Guid);
					pA.relations.Add (this);
				}
				// - pA
				pB = model.getParameterByGUID(pB_guid);

				if (pB != null && ! pB.relations.Contains(this)) 
				{
					//Debug.Log ("AXRelation::setupReferencesInModel - relations.add pB="+pB.Name+" : " + pB.Guid);
					pB.relations.Add (this);
				}
 				
			}
			//Debug.Log (toString ()); 
		}
		
		
	
		public AXParameter getRelatedTo(AXParameter p)
		{
			if (pA != p)
				return pA;
	
			return pB;
	
		}
	
	
		public void memorize()
		{
			expression_AB_prev = expression_AB;
			expression_BA_prev = expression_BA;
		}
		public void revert()
		{
			expression_AB = expression_AB_prev;
			expression_BA = expression_BA_prev;
		}
	}
}
