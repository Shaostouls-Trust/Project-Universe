
#if !(UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
#define UNITY_5_1_PLUS
#endif


using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

using AX;
using AXEditor;


 

 
 
public class AXStartupWindow : EditorWindow
{
	
	public static string identifier = "TH_AX";
	//static string pathImages = ArchimatixEngine.ArchimatixAssetPath+"/src/Editor/Startup/Images/";
	static string pathImages = ArchimatixEngine.ArchimatixAssetPath+"/ui/StartupImages/";
	
	Texture2D headerPic;
	string changelogText = "";
	Vector2 changelogScroll = Vector2.zero;
	GUIStyle richLabelStyle;
	GUIStyle richButtonStyle;
	GUIStyle iconButtonStyle;
	GUIStyle textLabelStyle;
	
	Texture2D icon3DLibrary;
	Texture2D icon2DLibrary;
	Texture2D iconAXEditor;

	bool showAcknowledgements; 

	
	[MenuItem("Help/Archimatix/About", false, 0)]
	public static void MenuInit()
	{
		AXStartupWindow.open();
	}
	
	[MenuItem("Window/Archimatix/About", false, 0)]
	public static void MenuGetStartedInit()
	{
		AXStartupWindow.open();
	}
	
	[MenuItem("Help/Archimatix/User Guide (PDF)", false, 0)]
	public static void MenuGuide()
	{
		string file = "file:///"+ Application.dataPath +"/Archimatix/Documentation/UserGuide.pdf";
		Application.OpenURL(file);
	}
	[MenuItem("Help/Archimatix/Online User Manual", false, 0)]
	public static void MenuManual()
	{
		Application.OpenURL("http://archimatix.com/documentation");
	}

	
	public static void open()
	{
		
		AXStartupWindow window;
		window = EditorWindow.GetWindow<AXStartupWindow>(true, "About Archimatix", true);
		Vector2 size = new Vector2(530, 670);
		window.minSize = size;
		window.maxSize = size;
		window.ShowUtility();

		 

		EditorPrefs.SetString(identifier, ArchimatixEngine.version);


	}

	 
	public static T LoadAssetAt<T>(string path) where T : UnityEngine.Object
	{
		#if UNITY_5_1_PLUS
		return AssetDatabase.LoadAssetAtPath<T>(path);
		#else
		return Resources.LoadAssetAtPath<T>(path);
		#endif
	}
	



	public static string GetVersion()
	{
		DirectoryInfo info = new DirectoryInfo(Application.dataPath);
		FileInfo[] files = info.GetFiles("AXChangeLog.txt", SearchOption.AllDirectories);

		if (files != null && files.Length > 0)
		{
			StreamReader sr = files[0].OpenText();
			ArchimatixEngine.version = sr.ReadLine();
			sr.Close();

			return ArchimatixEngine.version;
		} 
		return "no version found";
	}
	


	 public static bool ArchimatixIsInstalled()
	 {
		if (string.IsNullOrEmpty(ArchimatixEngine.pathChangelog))
			return false;

		return true;
	 }

	void OnEnable()
	{		
		ArchimatixEngine.establishPaths();


		// Is all in order with the Archimatix install?
		if (string.IsNullOrEmpty(ArchimatixEngine.pathChangelog))
			return;

		pathImages = ArchimatixEngine.ArchimatixAssetPath+"/ui/StartupImages/";

		string StartupImagesPrefix = "zz_AXStartup_";

		//Debug.Log("AXStartupWindowProcessor SCREEN");

		
		string versionColor = EditorGUIUtility.isProSkin ? "#ffffffee" : "#000000ee";

		TextAsset changelogAsset = LoadAssetAt<TextAsset>(ArchimatixEngine.pathChangelog);


		if (changelogAsset == null)
			return;

		
		changelogText = changelogAsset.text;
		changelogText = Regex.Replace(changelogText, @"^[0-9].*", "<color=" + versionColor + "><size=13><b>Version $0</b></size></color>", RegexOptions.Multiline);
		changelogText = Regex.Replace(changelogText, @"^- (\w+:)", "  <color=" + versionColor + ">$0</color>", RegexOptions.Multiline);
		
		
		headerPic 		= AssetDatabase.LoadAssetAtPath<Texture2D>(pathImages + StartupImagesPrefix + "EditorWindowBanner.jpg");

		icon3DLibrary 	= LoadAssetAt<Texture2D>(pathImages + StartupImagesPrefix + "icon3DLibrary.jpg");
		icon2DLibrary 	= LoadAssetAt<Texture2D>(pathImages + StartupImagesPrefix + "icon2DLibrary.jpg");
		iconAXEditor 	= LoadAssetAt<Texture2D>(pathImages  + StartupImagesPrefix + "iconEditor.jpg");
		
	}











	
	void OnGUI()
	{

		// Is all in order with the Archimatix install?
		if (string.IsNullOrEmpty(ArchimatixEngine.pathChangelog) ||  string.IsNullOrEmpty(changelogText))
			return;

		if (richLabelStyle == null)
		{
			richLabelStyle 							= new GUIStyle(GUI.skin.label);
			richLabelStyle.richText 				= true;
			richLabelStyle.wordWrap 				= true;
			richButtonStyle 						= new GUIStyle(GUI.skin.button);
			richButtonStyle.richText 				= true;
			
			textLabelStyle 							= new GUIStyle(GUI.skin.label);
			textLabelStyle.richText 				= true;
			textLabelStyle.wordWrap 				= true;
			textLabelStyle.alignment 				= TextAnchor.MiddleLeft;
			textLabelStyle.fixedWidth 				= 510;
			
			iconButtonStyle 						= new GUIStyle(GUI.skin.button);
			iconButtonStyle.normal.background 		= null;
			iconButtonStyle.imagePosition 			= ImagePosition.ImageOnly;
			iconButtonStyle.fixedWidth 				= 128;
			iconButtonStyle.fixedHeight 			= 128;
			
			
			
			
		}
		
		Rect headerRect = new Rect(0, 0, 530, 200);
		EditorGUI.DrawTextureTransparent(headerRect, headerPic, ScaleMode.StretchToFill);
		

		GUILayout.Space(200);
		
		GUILayout.BeginVertical();
		{
			
			
			
			// What to do next....
			horizontalRule(0, 2);

			/*
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label ("Version " +ArchimatixEngine.version, richLabelStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
*/


			// TOP BUTTONS

			GUILayout.BeginHorizontal();

			GUILayout.Space(5);


			// User GUIDE
			if (GUILayout.Button("<b>User Guide </b>\n<size=12>The best way to get started</size>", richButtonStyle, GUILayout.MaxWidth(400), GUILayout.Height(36)))
			{
				string file = "file:///"+ Application.dataPath +"/Archimatix/Documentation/UserGuide.pdf";
				Application.OpenURL(file);
			}



			// TUTORIALS
			if (GUILayout.Button("<b>Tutorial Videos </b>\n<size=12>Watch a range of how-tos</size>", richButtonStyle, GUILayout.MaxWidth(400), GUILayout.Height(36)))
				Application.OpenURL("http://www.archimatix.com/tutorials");
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			GUILayout.Label("Or, click on one of these to jump right in!", textLabelStyle, new GUILayoutOption[] {GUILayout.MinWidth(450)}); 

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				
				if (GUILayout.Button(icon3DLibrary, iconButtonStyle))
					ArchimatixEngine.openLibrary3D();

				
				if (GUILayout.Button(icon2DLibrary, iconButtonStyle))
					ArchimatixEngine.openLibrary2D();
				
				
				//if (ArchimatixEngine.NodeGraphEditorIsInstalled)
				//{

				if (GUILayout.Button(iconAXEditor, iconButtonStyle))
				{
					

					AXNodeGraphEditorWindow.Init();


				}

					
				//}
				
				
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
			
		}






		horizontalRule(4, 1);


		 

		// CHANGE_LOG

		GUILayout.BeginHorizontal();
		changelogScroll = GUILayout.BeginScrollView(changelogScroll, GUIStyle.none, new GUIStyle(GUI.skin.verticalScrollbar));
		GUILayout.Label(changelogText, textLabelStyle);
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal(); 
		 
		  

		horizontalRule(0, 4);
		

		GUILayout.BeginHorizontal();
		{
			showAcknowledgements = EditorGUILayout.Foldout(showAcknowledgements, "Acknowledgments", true);
			 
			if (showAcknowledgements)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.TextArea("Special thanks to Unity community members in the Beta Team: @elbows, @S_Darkwell, @HeliosDoubleSix, @Teila, @wetcircuit, @radimoto @puzzlekings, @mangax, @sarum, @Sheriziya, @Voronoi, @steveR, @manpower13, @vrpostcard, @Whippets, @pixelstream, @Damien-Delmarle, @recursive, @sloopidoopi, @KWaldt, @pcg, @Hitch42, @Bitstream, @CognitiveCode, @ddutchie, @JacooGamer, @razmot, @angrypenguin, @Paylo, @AdamGoodrich. Also, for much needed feedback and support, many thanks to Eden Muir, Andrew Cortese, Lucas Meijer, Sanjay Mistry, Dominic Laflamme, Rune Skovbo, Alan Robles, Luke Noonan, Anton Hand, Lucas Miller, and Joachim Holmér");
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndHorizontal();
		// BOTTOM BUTTON - DOCUMENTATION

		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("<b>Documentation</b>\n<size=12>Manual, examples and tips</size>", richButtonStyle, GUILayout.MaxWidth(260), GUILayout.Height(36)))
				Application.OpenURL("http://archimatix.com/documentation");
			
			if (GUILayout.Button("<b>Rate it</b>\n<size=12>on the Asset Store</size>", richButtonStyle, GUILayout.Height(36)))
			{
				Application.OpenURL("http://u3d.as/qYW");
				//Application.OpenURL("com.unity3d.kharma:content/?");

			}
			if (GUILayout.Button("<b>Join the Community!</b>\n<size=12>We are building worlds.</size>", richButtonStyle, GUILayout.Height(36)))
			{
				Application.OpenURL("http://community.archimatix.com");
				//Application.OpenURL("com.unity3d.kharma:content/?");

			}
		}
		GUILayout.EndHorizontal();
		
		
		
		// Contact
		horizontalRule(4, 2);
		
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("<b>E-mail</b>\n<size=12>support@archimatix.com</size>", richButtonStyle, GUILayout.MaxWidth(172), GUILayout.Height(36)))
				Application.OpenURL("mailto:support@archimatix.com");
			
			if (GUILayout.Button("<b>Twitter</b>\n<size=12>@archimatix</size>", richButtonStyle, GUILayout.Height(36)))
				Application.OpenURL("http://twitter.com/archimatix");

			// UNITY FORUM
			if (GUILayout.Button("<b>Unity Forum</b>\n<size=12>The Original!</size>", richButtonStyle, GUILayout.MaxWidth(172), GUILayout.Height(36)))
				Application.OpenURL("http://bit.ly/2gj4oax");
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(5);
		
		GUILayout.EndVertical();
		
	}
	
	void horizontalRule(int prevSpace, int nextSpace)
	{
		GUILayout.Space(prevSpace);
		Rect r = GUILayoutUtility.GetRect(Screen.width, 2);
		Color og = GUI.backgroundColor;
		GUI.backgroundColor = new Color(.7f, .6f, .7f, .8f);
		GUI.Box(r, "");
		GUI.backgroundColor = og;
		GUILayout.Space(nextSpace);
	}
}



