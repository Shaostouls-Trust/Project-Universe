using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using AXGeometry;

using AX.Generators;

namespace AX
{


	public class Thumbnail 
	{
	
		public static bool _prevFog;
		public static Color _prevAmbientLight;
		public static float _prevAmbientIntensity;
		public static Material _prevSkybox;
		public static UnityEngine.Rendering.DefaultReflectionMode _prevReflectionMode;

		[System.NonSerialized]
		private static List<Light> allLightsInTheScene;
		
		[System.NonSerialized]
		private static List<Light> lightsToTurnOn;

		public static Cubemap defaultCubeMap;

		public static Color backgroundColor = new Color(.318f,.31f,.376f);

		// Begin
		// Save previous render settings and lights to restore later
		public static void BeginRender()
		{
			/* NOTE: don't turn the Skybox off. It seems to need to kill the default RefectionProbe.
			 * 
			 * 
			 */
			_prevFog 					= RenderSettings.fog;
			_prevAmbientLight 			= RenderSettings.ambientLight;
			_prevAmbientIntensity		= RenderSettings.ambientIntensity;

			RenderSettings.fog 					= false;
			RenderSettings.ambientLight 		= new Color(1f, .9f, .95f);
			RenderSettings.ambientIntensity 	= .5f;

			// Turn other active directional lights off, turn them back on at the end
			lightsToTurnOn = new List<Light>();
			
						
			foreach (Light l in getAllActiveLightsInTheScene())
			{
				if (l.type == LightType.Directional && l.isActiveAndEnabled)
				{
					if (l.isActiveAndEnabled)
						lightsToTurnOn.Add(l);
					l.gameObject.SetActive(false);
				}
			}
			
		}
		
		public static void renderThumbnail(AXParametricObject po, bool makeTexture = false)
		{
			
			AXModel model = po.model;
			
			if (model == null)
				return;

			

		
			model.assertThumnailSupport();
			

			// THUMBNAIL BACKGROUND COLOR
			if (po.generator != null && makeTexture)
			{
				
				model.thumbnailCamera.clearFlags 			= CameraClearFlags.Color;
				model.thumbnailCamera.backgroundColor = po.generator.ThumbnailColor;

			}
			// WHY ARE THE THUMNAIL AND RenTex null?
			
			if (po.renTex == null)
			{
				//po.renTex = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				po.renTex = new RenderTexture(256, 256, 24);

			}
			if (! po.renTex.IsCreated())
			{
				po.renTex.antiAliasing = 8;
				po.renTex.Create();
				
			}

			//Debug.Log(po.renTex.sRGB);
			// This started giving an error in 5.3 and did not seem to be needed anyway!

			if (makeTexture)
				RenderTexture.active = po.renTex;

			model.thumbnailCamera.targetTexture = po.renTex;
			
			
			
			model.thumbnailCameraGO.transform.position = po.getThumbnailCameraPosition();
			model.thumbnailCameraGO.gameObject.transform.LookAt(po.getThumbnailCameraTarget());
			
			 
			
			
			// USE THIS MATRIX TO DRAW THE PO's MESHES SOMEWHERE FAR, FAR AWAY
			

			//Debug.Log("RENDER: " + po.Name);

			Material tmpMat = null;

			if (po.generator is MaterialTool)
			{

				
				if (po.axMat != null && po.axMat.mat != null)
					tmpMat = po.axMat.mat;
				else if (po.grouper != null && po.grouper.axMat != null&& po.grouper.axMat.mat != null)
					tmpMat = po.grouper.axMat.mat;
				else
					tmpMat = model.axMat.mat;

				//Debug.Log("RENDER TEXTURETOOL " + model.thumbnailCameraGO.transform.position + " mesh-> " + model.thumbnailMaterialMesh.vertices.Length + " -- " + tmpMat);

				Graphics.DrawMesh(model.thumbnailMaterialMesh, model.remoteThumbnailLocation, tmpMat, 0, model.thumbnailCamera);
						
				model.thumbnailCamera.Render ();
			}
			else 
			{
				AXParameter op = po.getParameter("Output Mesh");

				if (op != null)
				{
					
					// RE-RENDER
					// After a save or reload, the RenterTextures are empty
					if (op.meshes != null)
					{
						
						for (int mi = 0; mi < op.meshes.Count; mi++) {
							AXMesh axmesh = op.meshes [mi];
							if (axmesh.mat != null)
								tmpMat = axmesh.mat;
							else
								if (po.axMat != null && po.axMat.mat != null)
									tmpMat = po.axMat.mat;
								else if (po.grouper != null && po.grouper.axMat != null&& po.grouper.axMat.mat != null)
									tmpMat = po.grouper.axMat.mat;

								else
									tmpMat = model.axMat.mat;

							
							Graphics.DrawMesh (axmesh.drawMesh, (model.remoteThumbnailLocation * axmesh.transMatrix), tmpMat, 0, model.thumbnailCamera);
						}
					}

					model.thumbnailCamera.Render ();
				}
			}
			
			// write to texture
			if (makeTexture)
			{
				if (po.thumbnail == null)
					po.thumbnail = new Texture2D(256, 256);
				
				po.thumbnail.ReadPixels(new Rect(0, 0, po.renTex.width, po.renTex.height), 0, 0);
				po.thumbnail.Apply();
			}
			if (makeTexture)
				RenderTexture.active = null;

			// 3. Set helper objects to inactive
			model.thumbnailLightGO.SetActive(false);
			model.thumbnailCameraGO.SetActive(false);
			
		
		}
		
		
		public static void EndRender()
		{
		
			// CLEAN UP
			
			// 1. Revert environment

			RenderSettings.fog 					= _prevFog;
			RenderSettings.ambientLight 		= _prevAmbientLight;
			RenderSettings.ambientIntensity 	= _prevAmbientIntensity;

			// 2. Turn lights back on
			foreach(Light l in lightsToTurnOn)
				l.gameObject.SetActive(true);
			
			
		
		
		}
		
		
		
		public static List<Light> getAllActiveLightsInTheScene()
		{
			
			if (allLightsInTheScene == null || allLightsInTheScene.Count == 0 || allLightsInTheScene[0] == null)
				allLightsInTheScene = getAllLightsInTheScene();
				
			
			List<Light>	allActiveLights = new List<Light>();
			
			for (int i = 0; i < allLightsInTheScene.Count; i++) {
				
				Light l = allLightsInTheScene [i];
				
				if (l != null && l.type == LightType.Directional && l.isActiveAndEnabled) {
					if (l.isActiveAndEnabled)
						allActiveLights.Add (l);
				}
			}
			return allActiveLights;
		}
		
		public static List<Light> getAllLightsInTheScene()
		{
			Light[] lights = GameObject.FindObjectsOfType(typeof(Light)) as Light[];
			
			allLightsInTheScene = new List<Light>();
			foreach (Light l in lights)
			{
				if (l.type == LightType.Directional && l.isActiveAndEnabled)
				{
					if (l.isActiveAndEnabled)
						allLightsInTheScene.Add(l);
				}
			}
			
			return allLightsInTheScene;
		}
		
		
	}



}
