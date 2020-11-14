using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AXGeometry
{
 

	// Subpart of a larger Spline as noted by indices.
	// An important value is the subspline length in units.
	public class SubsplineIndices 
	{

		public Spline spline;

		public int beg_i = 0;
		public int end_i = 1;

		int cursor = 0;

		// the splineIndices provides a list of the verts in the Spline that comprise the vers to be used in this sub-spline.
		public List<int> splineIndices;

		public float length;

		public float inset;
		public float bay;



		// this cannot be a user controlled variable
		int 	nbays = 0;
		float 	actual_bay; 


		public SubsplineIndices(Spline s)
		{
			spline = s;

			splineIndices = new List<int>();


		} 

		public void setIndicesToAllSplinePoints()
		{
			for(int i=0; i<spline.controlVertices.Count; i++)
			{
				splineIndices.Add(i);
				beg_i=0;
				end_i=spline.controlVertices.Count-1;
			}

		}


		// NEXT_I
		public int nextIndex(int i)
		{
			i++;
			if (i > spline.controlVertices.Count-1)
				i = 0;
			return i;
		}

		// PREV_I
		public int prevIndex(int i)
		{
			i--;
			if (i < 0)
				i = spline.controlVertices.Count-1;
			return i;
		}

		public void reset()
		{
			cursor = beg_i;
		}

		public void next()
		{
			cursor = nextIndex(cursor);
		}

		public SubsplineIndices ()
		{
			

			splineIndices = new List<int>();

		}

		public void calcLength()
		{
			// length
			reset();
			next();
			length = 0;


			while(cursor != end_i)
			{
				length += spline.segment_lengths[cursor];
				next();
			}
			length += spline.segment_lengths[cursor];
		}



		public void calcRepeaterMemberValues()
		{

			float inset_length = length - 2*inset;

			if (length < inset*2)
				nbays = 0;

			else
			{
				nbays 		= Mathf.FloorToInt( inset_length / bay );
				actual_bay 	= inset_length / nbays;
			}

			//Debug.Log(">>>>>>>>>>>>>>>>>   LENGTH="+length+", inset_length="+inset_length+", nbays="+nbays+", actual_bay="+actual_bay);

		}



		// Crawl along subspline checking if the current running distance is greater then the current node distance.
		// if it is, lerp the position with the previous vert and increment the node distance.

		public Matrix4x4[] getNodeTransforms(float _inset, float _bay)
		{
			//Debug.Log("-----> GET TRANSFORMS nbays="+nbays);
			inset 	= _inset;
			bay 	= _bay;

			calcRepeaterMemberValues();

			if (nbays == 0)
				return null;

			
			List<Matrix4x4> matrices = new List<Matrix4x4>();


				

			reset();
			//next();

			float cur_node_distance 		= 0;
			if (inset > 0)
				cur_node_distance 			= inset;

			float prev_running_distance 	= 0;
			float next_running_distance 	= spline.segment_lengths[nextIndex(cursor)];


			//int prevCursor = 0;
			int nextCursor = 0;

			float percentage = 0;
			Vector2 position;

			int nodesCreated = 0;

			while(cursor != end_i)
			{
				//Debug.Log("****************** CURSOR: "+cursor + ", length="+length + ", running_distance="+prev_running_distance+", cur_node_distance="+cur_node_distance);
				// The cursor is a vertex index.

				// don't advance the cursor until the  cur_node_distance is greater than the reunning distance at that cursor.
				float next_running_distanceTmp = (nextIndex(cursor) == end_i) ? (next_running_distance-inset) : next_running_distance;

				int governor = 0;
				while ((next_running_distanceTmp)*1.01f >= cur_node_distance && nodesCreated <= nbays)
				{
					
					if (governor++ > 1000)
					{
						Debug.Log("governor hit)");
						return null;
					}	

					//prevCursor = prevIndex(cursor);
					nextCursor = nextIndex(cursor);

					// ** Position **

					// Get percentage for Lerp between this vert and the previous one.
					percentage = (cur_node_distance - prev_running_distance) / spline.segment_lengths[nextCursor];

					// do Lerp between points
					position = Vector2.Lerp(spline.controlVertices[cursor], spline.controlVertices[nextCursor], percentage);

					// ** Rotation **
					// Normal to line segment 
					float rot = spline.edgeRotations[cursor];

					// Interpolate between bevel angles
					if (Mathf.Abs(spline.bevelAngles[cursor]) < 22 && Mathf.Abs(spline.bevelAngles[nextCursor]) < 22)
						rot = Mathf.LerpAngle(spline.nodeRotations[cursor]-90, spline.nodeRotations[nextCursor]-90, percentage);
				
					matrices.Add(Matrix4x4.TRS(new Vector3(position.x, 0, position.y), Quaternion.Euler(0, rot, 0), Vector3.one));
					nodesCreated++;

					cur_node_distance += actual_bay;
				}

				next();
				prev_running_distance += spline.segment_lengths[cursor];
				next_running_distance += spline.segment_lengths[nextIndex(cursor)];
			}


			//Debug.Log(">>>> >>>> >>>> matrices.Count="+matrices.Count);
			Matrix4x4[] returnMatrices = new Matrix4x4[matrices.Count];

			for(int i=0; i<matrices.Count; i++)
				returnMatrices[i] = matrices[i];

			return returnMatrices;

		} // \getNodeTransforms()









		public void print()
		{
			string indices = "";
			reset();
			while(cursor != end_i)
			{
				indices += " "+cursor;
				next();
			}
			indices += " "+cursor;


			Debug.Log("Subspline :: "+indices +" :: LENGTH="+length+", nbays="+nbays+", actual_bay="+actual_bay);
		}

	}
}

