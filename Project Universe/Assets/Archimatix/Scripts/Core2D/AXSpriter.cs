using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using AXGeometry;

using AX.Generators;

namespace AX
{

	[System.Serializable]
	public class AXSpriter 
	{

		// each sprite has its own camera and RenderTexture

		public AXSpriteCamera cameraInfo;
		public float fov;


		public RenderTexture 	spriteRenderTexture;
		public Texture2D 		spriteTexture2D;

		public int 				tex_width 	= 1024;
		public int 				tex_height 	= 1024;



		public AXSpriter()
		{


		}

		public void init()
		{
			


//			renderTexture = new RenderTexture(tex_width, tex_height, 24, RenderTextureFormat.ARGB32);
//			renderTexture.Create();

			cameraInfo = new AXSpriteCamera();







		}


		public void renderPO2Sprite(AXParametricObject po, bool makeTexture = false)
		{
//			if (Event.current != null && Event.current.type == EventType.repaint)
//				return;

			if (po == null)
				return;

			AXModel model = po.model;
			
			if (model == null)
				return;

			

			Debug.Log("render Sprite texture! " + po.generator);

//			if (renderTexture == null)
//			{
//				//Debug.Log("making rentex");
//				//renderTexture = new RenderTexture(tex_width, tex_height, 24, RenderTextureFormat.ARGB32);
//				renderTexture = new RenderTexture(tex_width, tex_height, 24);
//				renderTexture.Create();//			
//			}
			//Debug.Log(renderTexture);





			Generator3D gener = (Generator3D) po.generator;
			//model.spriteCameraGO.transform.position = new Vector3(gener.transX, 0, -10);
			//model.spriteCameraGO.transform.LookAt(new Vector3(gener.transX, 0, 0));
			model.spriteCameraGO.transform.position = new Vector3(0, 0, -20);
			model.spriteCameraGO.transform.LookAt(new Vector3(0, 0, 0));

			//model.spriteCameraGO.orthographicSize = Mathf.Max(po.bounds.size.x, po.bounds.size.y);









			Material tmpMat = null;

			AXParameter op = po.getParameter("Output Mesh");

			//po.spriter.renderTexture = new RenderTexture(tex_width, tex_height, 24);
//			RenderTexture tmpRenTex = new RenderTexture (tex_width, tex_height, 24, RenderTextureFormat.ARGB32);
//			tmpRenTex.Create();


			int AX2D_Srite_Layer = LayerMask.NameToLayer("AX2D_Sprite");

				Debug.Log("AX2D_Srite_Layer="+AX2D_Srite_Layer);


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

						//Debug.Log("--- Draw mesh: "+axmesh.drawMesh.vertices.Length);		 
						Graphics.DrawMesh (axmesh.drawMesh, axmesh.transMatrix, tmpMat, AX2D_Srite_Layer, model.spriteCamera);
					}
				}


				model.spriteCamera.Render ();


				// write to texture2D
				//spriteTexture = new Texture2D(tex_width, tex_width, TextureFormat.ARGB32, false);

				//Graphics.CopyTexture(tmpRenTex, spriteTexture);

//				RenderTexture prevActive = RenderTexture.active;
//				RenderTexture.active = renderTexture; //tmpRenTex;//po.spriter.renderTexture;
//				spriteTexture.ReadPixels(new Rect(0, 0, tex_width, tex_width), 0, 0);
//				spriteTexture.Apply();
//				RenderTexture.active = prevActive;

			}







			model.spriteCameraGO.SetActive(false);

		}

	}
}
