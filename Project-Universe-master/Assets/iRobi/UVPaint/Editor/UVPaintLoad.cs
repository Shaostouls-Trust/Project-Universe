using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
class UVPaintLoad : EditorWindow {
	static Rect rect;
	static Rect rect2;
	static Rect rect3;
	static Rect rect4;
	static Rect rect5;
	static Rect rect6;
	static Rect rect7;
	static Rect rect8;
	static Rect rect9;
	static Rect rect10;
	static GUIStyle style = new GUIStyle();
	static GUIStyle styleLink = new GUIStyle();
	static GUIStyle styleBig = new GUIStyle();
	static Texture2D Afghan;
	static Texture2D facebook64;
	static Texture2D twitter64;
	[MenuItem ("Window/iRobi/UVPaint (Decal System)")]

	public static void  ShowWindow () {
		rect = new Rect (20, 20, 128, 128);
		rect2 = new Rect (170, 30, 200, 128);
		rect3 = new Rect (140, 220, 64, 64);
		rect4 = new Rect (220, 220, 64, 64);
		rect5 = new Rect (170, 130, 200, 30);
		rect6 = new Rect (200, 50, 200, 30);
		rect7 = new Rect (170, 70, 200, 30);
		rect8 = new Rect (170, 90, 200, 30);
		rect9 = new Rect (170, 110, 200, 30);
		rect10 = new Rect (40, 180, 200, 30);

		style.fontStyle = FontStyle.Bold;
		style.fontSize = 13;
		style.normal.textColor = Color.white;

		styleBig.fontStyle = FontStyle.Bold;
		styleBig.fontSize = 18;
		styleBig.normal.textColor = Color.white;

		styleLink.fontStyle = FontStyle.BoldAndItalic;
		styleLink.fontSize = 13;
		styleLink.normal.textColor = Color.red;
		styleLink.hover.textColor = Color.white;
		styleLink.hover.background = new Texture2D(8,8);

		Afghan = Resources.Load ("UVPaintIcon") as Texture2D;
		facebook64 = Resources.Load ("facebook64") as Texture2D;
		twitter64 = Resources.Load ("twitter64") as Texture2D;
		EditorWindow.GetWindowWithRect<UVPaintLoad> (new Rect (100, 100, 400, 300),true,"iRobi Dev.",true);
	}

	[InitializeOnLoadMethod]
	static void UVPaintLoadM()
	{ 
		string key = Application.dataPath + "UVPaintLoadFirstStart";
		if (!EditorPrefs.HasKey (key)) {
			ShowWindow ();
			Debug.Log ("UVPaint was successfully loaded.");
		}
		EditorPrefs.SetBool (key, true);
	}

	void OnGUI () {
		GUI.DrawTexture (rect, Afghan);
		if (GUI.Button (rect3, facebook64,style))
			Application.OpenURL ("https://www.facebook.com/groups/iRobiDev/");
		if (GUI.Button (rect4, twitter64,style))
			Application.OpenURL ("https://twitter.com/iR0biDev");
		GUI.Label (rect2, "Thank you for your support!",style);

		if (GUI.Button (rect5, "Try other Assets from iRobi>>",styleLink))
			Application.OpenURL ("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:7755");
		//GUI.Label (rect5, "Also try our other products>>>",styleLink);
		GUI.Label (rect6, "UVPaint:",style);
		GUI.Label (rect7, "Version 1.84e",style);
		GUI.Label (rect8, "See Readme.txt for ChangeLog",style);
		GUI.Label (rect9, "See UVPaint.pdf for learn",style);
		GUI.Label (rect10, "Also follow us in social networks:",styleBig);
	}
}
