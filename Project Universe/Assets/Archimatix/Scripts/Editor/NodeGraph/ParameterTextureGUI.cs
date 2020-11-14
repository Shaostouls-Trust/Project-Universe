#pragma warning disable

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Text.RegularExpressions;

using AXClipperLib;

using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXGeometry;


using AX;
using AX.Generators;
using AXEditor;



public class ParameterToolGUI  {

 
	AXTexCoords tex;

	
	public static int display(Rect pRect, AXNodeGraphEditorWindow editor, AXParameter p)
	{
		//Debug.Log("ParameterTextureGUI.DISPLAY "+p.Name);
		float cur_x = ArchimatixUtils.cur_x; 
		//float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
		float box_w = pRect.width - cur_x - 1*ArchimatixUtils.indent;

		int cur_y = (int)pRect.y;
		int lineHgt = (int)pRect.height;
		int gap = 5;
		


		
		
		//Color shapeColor = editor.getDataColor (AXParameter.DataType.Spline);
		
		Color dataColor =  editor.getDataColor(p.Type);

		Color oldBackgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = dataColor;
		
		
		// INPUT
		if (editor.OutputParameterBeingDragged == null|| editor.OutputParameterBeingDragged.Type == p.Type)
		{
			if (p.PType != AXParameter.ParameterType.Output)
			{
				if (GUI.Button (new Rect (-3, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) {
					if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type)
						editor.OutputParameterBeingDragged = null;
					else
						editor.inputSocketClicked (p);
				}
			}
		}
		

		// OUTPUT

		if (editor.InputParameterBeingDragged == null || editor.InputParameterBeingDragged.Type == AXParameter.DataType.MaterialTool)
		{
			if (GUI.Button (new Rect (pRect.width+6, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) 
			{
				if (editor.InputParameterBeingDragged != null && editor.InputParameterBeingDragged.Type != p.Type)
					editor.InputParameterBeingDragged = null;
				else
					editor.outputSocketClicked (p);
			}
		}



		// LABEL BOX
		Rect boxRect = new Rect(cur_x + ArchimatixUtils.indent, cur_y, box_w, pRect.height);

		GUI.Box(boxRect, " "); GUI.Box(boxRect, " "); GUI.Box(boxRect, " ");GUI.Box(boxRect, " ");


		// LABEL
		Rect lRect = boxRect;
		
		lRect.x += 3;
		lRect.width -= 10;
		GUI.Box(boxRect, " "); GUI.Box(boxRect, " "); GUI.Box(boxRect, " ");GUI.Box(boxRect, " ");

		GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
		labelstyle.alignment = TextAnchor.MiddleLeft;
		if (p.PType == AXParameter.ParameterType.Output)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			labelstyle.fixedWidth = lRect.width+5;
		}

		string label = p.Name;
		if (p.ParentNode != null && p.ParentNode is AXShape)
		{
			if (p.DependsOn != null)
				if (p.DependsOn.Parent != null )
					label = p.DependsOn.Parent.Name;
		}

		GUI.Label(lRect, label);





		 

		// Texture Thumbnail
		AXParameter src_p 			= p.DependsOn;
		AXParametricObject src_po 	= null;

		if (src_p != null)
			src_po = src_p.parametricObject;

		Rect texThumbRect;

		if(src_po != null)
		{
			if(src_po.generator is MaterialTool && src_po.axMat.mat != null && src_po.axMat.mat.HasProperty("_MainTex") && src_po.axMat.mat.mainTexture != null)
			{
				texThumbRect = new Rect ((boxRect.x + boxRect.width - lineHgt+1), cur_y+1, lineHgt-2, lineHgt-2);
				EditorGUI.DrawTextureTransparent(texThumbRect,   src_po.axMat.mat.mainTexture, ScaleMode.ScaleToFit, 1.0F);
			}
			else
			{
			 	texThumbRect = new Rect ((boxRect.x + boxRect.width - lineHgt+1), cur_y+1, lineHgt-2, lineHgt-2);
				EditorGUI.DrawTextureTransparent(texThumbRect,    ArchimatixEngine.nodeIcons[p.Type.ToString()], ScaleMode.ScaleToFit, 1.0F);
			} 

		}
		else if (src_po == null)
		{
			if (src_p != null)
			{
			 	texThumbRect = new Rect ((boxRect.x + boxRect.width - lineHgt+1), cur_y+1, lineHgt-2, lineHgt-2);
				EditorGUI.DrawTextureTransparent(texThumbRect,    ArchimatixEngine.nodeIcons[p.Type.ToString()], ScaleMode.ScaleToFit, 1.0F);
			} 
			else
			{
				// NEW TOOL

				texThumbRect = new Rect ((boxRect.x + boxRect.width - lineHgt), cur_y-1, lineHgt, lineHgt);

				if (GUI.Button( texThumbRect, ArchimatixEngine.nodeIcons[p.Type.ToString()] )) //"+"))
				{



					src_po = AXEditorUtilities.addNodeToCurrentModel(p.Type.ToString());
					src_po.Name = Regex.Replace(p.Name, @"\s+", "");	// remove spaces
					src_po.rect.x = p.parametricObject.rect.x - 220;
					src_po.rect.y = p.parametricObject.rect.y + 50;
					src_po.rect.height = 500;


					if (p.parametricObject.model.currentWorkingGroupPO != null)
					{
						p.parametricObject.model.currentWorkingGroupPO.addGroupee(src_po);
					}
					else
					{
						src_po.grouper = null;
						src_po.grouperKey = null;

					}

						

								

					//AXNodeGraphEditorWindow.zoomToRectIfOpen(src_po.rect);
					//src_po.inputControls.isOpen = true;

					src_po.geometryControls.isOpen = true;

					src_po.generator.pollInputParmetersAndSetUpLocalReferences();

					//Debug.Log("here " + src_po.getParameter("Output"));

					p.makeDependentOn(src_po.getParameter("Output"));

					p.parametricObject.model.remapMaterialTools();

					p.parametricObject.isAltered = true;
					p.parametricObject.model.autobuild();




				}
			}

		}





		/*
		// FOLDOUT (isOpen)
		GUI.backgroundColor = new Color(1,1,1,1f);

		EditorGUI.BeginChangeCheck ();
		p.isOpen = EditorGUI.Foldout (new Rect (cur_x, cur_y, 55, lineHgt), p.isOpen, "");
		if (EditorGUI.EndChangeCheck ()) 
		{
			if (src_p == null)
			{
				src_po = AXEditorUtilities.addNodeToCurrentModel(p.Type.ToString(), false);
				src_po.Name = Regex.Replace(p.Name, @"\s+", "");	// remove spaces
				src_po.rect.x = p.parametricObject.rect.x - 200;
				src_po.rect.y = p.parametricObject.rect.y + 50;
				src_po.isOpen = false;
				p.makeDependentOn(src_po.generator.P_Output);
				p.parametricObject.model.autobuild();
			}
		}
		GUI.backgroundColor = shapeColor;

		if (p.DependsOn == null)
			p.isOpen = false;

		cur_y += lineHgt+gap;
		
		if (p.isOpen)
		{


			
			//Archimatix.cur_x += Archimatix.indent;
			p.drawClosed = false;

			Rect tRect = pRect;
			tRect.x = 20;//30;
			tRect.width = pRect.width;
			tRect.x += 2;
			tRect.width -= 11;

			if (! src_po.isOpen)
			{
			foreach (AXParameter sp in src_po.getAllParametersOfPType(AXParameter.ParameterType.GeometryControl))
			{
				


				

				tRect.y = cur_y;
				Rect cntlRect = tRect; // new Rect(x0, cur_y, wid, 16);


				int hgt = ParameterGUI.OnGUI(cntlRect, editor, sp);

				cur_y += hgt + gap;
			}
			}









			//Archimatix.cur_x -= Archimatix.indent;


			
		}
		*/
		cur_y += lineHgt + gap;
																
		GUI.backgroundColor = oldBackgroundColor;

		return cur_y;

	}

}