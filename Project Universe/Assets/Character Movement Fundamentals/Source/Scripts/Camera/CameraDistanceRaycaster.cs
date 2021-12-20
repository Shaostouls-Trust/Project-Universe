﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script uses raycasts (or spherecasts) to detect obstacles between this transform and the camera;
	//The camera transform is then moved closer to this transform based on the obstacle's proximity;
	//The main purpose of this script is to prevent the camera from clipping into level geometry or to prevent any obstacles from blocking the player's view;
	public class CameraDistanceRaycaster : MonoBehaviour {

        //Transform component of camera;
		public Transform cameraTransform;
        //Transform component of camera target;
		public Transform cameraTargetTransform;

		Transform tr;

		//Whether a raycast or spherecast is used to scan for obstacles;
		public CastType castType;
		public enum CastType
		{
			Raycast,
			Spherecast
		}

		//Layermask used for raycasting;
		public LayerMask layerMask = ~0;

		//Layer number for 'Ignore Raycast' layer;
		int ignoreRaycastLayer;

		//List of colliders to ignore when raycasting;
		public Collider[] ignoreList;

		//Array to store layers of colliders in ignore list;
		int[] ignoreListLayers;

		float currentDistance;

		//Additional distance which is added to the raycast's length to prevent the camera from clipping into level geometry;
		//For most situations, the default value of '0.1f' is sufficient;
		//You can try increasing this distance a bit if you notice a lot of clipping;
		//This value is only used if 'Raycast' is chosen as 'castType'; 
		public float minimumDistanceFromObstacles = 0.1f;

		//This value controls how smoothly the old camera distance will be interpolated toward the new distance;
		//Setting this value to '50f' (or above) will result in no (visible) smoothing at all;
		//Setting this value to '1f' (or below) will result in very noticable smoothing;
		//For most applications, a value of '25f' is recommended; 
		public float smoothingFactor = 25f;

		//Radius of spherecast, only used if 'Spherecast' is chosen as 'castType';
		public float spherecastRadius = 0.2f;

		void Awake () {
			tr = transform;

            //Setup array to store ignore list layers;
            ignoreListLayers = new int[ignoreList.Length];

            //Store ignore layer number for later;
            ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

            //Make sure that the selected layermask does not include the 'Ignore Raycast' layer;
			if (layerMask == (layerMask | (1 << ignoreRaycastLayer))) {
				layerMask ^= (1 << ignoreRaycastLayer);
			}
			
            if (cameraTransform == null)
                Debug.LogWarning("No camera transform has been assigned.", this);

            if (cameraTargetTransform == null)
                Debug.LogWarning("No camera target transform has been assigned.", this);

            //If the necessary transform references have not been assigned, disable this script;
            if (cameraTransform == null || cameraTargetTransform == null)
            {
                this.enabled = false;
                return;
            }
                
			//Set intial starting distance;
			currentDistance = (cameraTargetTransform.position - tr.position).magnitude; 
		}

		void LateUpdate () {

			//Check if ignore list length has been changed since last frame;
			if(ignoreListLayers.Length != ignoreList.Length)
			{
				//If so, setup ignore layer array to fit new length;
				ignoreListLayers = new int[ignoreList.Length]; 
			}

			//(Temporarily) move all objects in ignore list to 'Ignore Raycast' layer and store their layer value for later;
			for(int i = 0; i < ignoreList.Length; i++)
			{
				ignoreListLayers[i] = ignoreList[i].gameObject.layer;
				ignoreList[i].gameObject.layer = ignoreRaycastLayer;
			}

			//Calculate current distance by casting a raycast;
			float _distance = GetCameraDistance();

			//Reset layers;
			for(int i = 0; i < ignoreList.Length; i++)
			{
				ignoreList[i].gameObject.layer = ignoreListLayers[i];
			}

			//Lerp 'currentDistance' for a smoother transition;
			currentDistance = Mathf.Lerp(currentDistance, _distance, Time.deltaTime * smoothingFactor);

			//Set new position of 'cameraTransform';
			cameraTransform.position = tr.position + (cameraTargetTransform.position - tr.position).normalized * currentDistance;

		}

		//Calculate maximum distance by casting a ray (or sphere) from this transform to the camera target transform;
		float GetCameraDistance()
		{
			RaycastHit _hit;

			//Calculate cast direction;
			Vector3 _castDirection = cameraTargetTransform.position - tr.position;

			if(castType == CastType.Raycast)
			{
				//Cast ray;
				if(Physics.Raycast(new Ray(tr.position, _castDirection), out _hit, _castDirection.magnitude + minimumDistanceFromObstacles, layerMask, QueryTriggerInteraction.Ignore))
				{
					//Check if 'minimumDistanceFromObstacles' can be subtracted from '_hit.distance', then return distance;
					if(_hit.distance - minimumDistanceFromObstacles < 0f)
						return _hit.distance;
					else
						return _hit.distance - minimumDistanceFromObstacles;
				}
			}
			else
			{
				//Cast sphere;
				if(Physics.SphereCast(new Ray(tr.position, _castDirection), spherecastRadius, out _hit, _castDirection.magnitude, layerMask, QueryTriggerInteraction.Ignore))
				{
					//Return distance;
					return _hit.distance;
				}
			}
			//If no obstacle was hit, return full distance;
			return _castDirection.magnitude;
		}
	}
}
