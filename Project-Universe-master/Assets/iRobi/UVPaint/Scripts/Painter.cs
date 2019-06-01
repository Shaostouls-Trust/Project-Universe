using UnityEngine;
using System.Collections;
using iRobi;
using System.Diagnostics;

public class Painter : MonoBehaviour {
	public Texture2D Decal;
	public Texture2D Decal2;
	float DecalSize = 2f;
	public bool RandomColor=false;
	public Color[] colors;
	Color current;
	bool oneFrame;
	// Use this for initialization
	void Start () {
	
	}
	
    //Debug
    //Stopwatch SW = new Stopwatch();
    //long mil, frames;

    //Debug
	// Update is called once per frame
   
	void Update () {
		DecalSize = UIScaller.DecalSize;
		Decal = UIScaller.current;
		bool rightMouseButton = false;
		bool pressed = false;

		rightMouseButton = Input.GetMouseButton (1);
		if (rightMouseButton) {
			current = colors [Random.Range (0, colors.Length)];
			//			current.a = 0.05f;
		}
		else
			current = new Color(1,1,1,0.3f);
		
		if (UIScaller.selected == 0) {
			pressed = Input.GetMouseButton (0) || rightMouseButton;
//			DecalSize = 2f;
		}
		else
		{
			pressed = Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1);
//			DecalSize = 0.7f;
			current.a = 1f;
		}
		
		if (pressed) {
			RaycastHit[] ray = Physics.RaycastAll (transform.position, transform.forward);
			for (int i = 0; i < ray.Length; i++) {
				Transform T = ray [i].collider.transform;
				UVPaint.DecalColor(current);
				if(rightMouseButton)UVPaint.AngleInDegrees(Random.Range(0,360));

				if (T.name == "TeddyBearCustomShader") {
					UVPaint.Create (T.gameObject, transform.position, transform.rotation, Decal2, UVPaint.DecalSize (DecalSize));
					UVPaint.ShaderPropertyName ("_SliceGuide");
					UVPaint.DecalColor (current);
					if(rightMouseButton)UVPaint.AngleInDegrees(Random.Range(0,360));
					UVPaint.Create (T.gameObject, transform.position, transform.rotation, Decal2, UVPaint.DecalSize (DecalSize));
				} else if (T.name == "TeddyBearTransparent") {
					current.a = 1;
					UVPaint.DecalColor(current);
					UVPaint.Create (T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize (DecalSize));
                }
                else if (T.name == "TeddyBearTransparentCutout")
                {
                    UVPaint.Create(T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize(DecalSize));
                }
                else
                {
                    //frames += 1;
                    //SW.Start();
                    UVPaint.Create(T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize(DecalSize));
                    //mil = (mil+SW.ElapsedMilliseconds*10);
                    //SW.Stop();
                    //UnityEngine.Debug.Log(mil / frames + " : " + SW.ElapsedMilliseconds*10);
                    //SW.Reset();
                }
			}
		}
		else if (Input.GetMouseButton (2)) {
			RaycastHit[] ray = Physics.RaycastAll (transform.position, transform.forward);
			for (int i = 0; i < ray.Length; i++) {
				Transform T = ray [i].collider.transform;
				UVPaint.DecalColor(current);
				if (T.name == "TeddyBearCustomShader") {
					current.a /= 2;
					UVPaint.DecalColor(current);
					UVPaint.Restore (T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize (DecalSize));
					UVPaint.ShaderPropertyName ("_SliceGuide");
					UVPaint.DecalColor(current);
					UVPaint.Restore (T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize (DecalSize));
				}
				else if (T.name == "TeddyBearTransparentCutout") {
					UVPaint.Restore (T.gameObject, transform.position, transform.rotation, Decal2, UVPaint.DecalSize (DecalSize));
				}
				else
				UVPaint.Restore (T.gameObject, transform.position, transform.rotation, Decal, UVPaint.DecalSize (DecalSize));
			}
		}
	}
}
