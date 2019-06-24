using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStuff : MonoBehaviour {

    public Transform[] snaps;
    public Transform mainCamera;
    bool buildStuff = false;

    Transform snap = null;
    bool switchSnap = false;
    int snapI = 0;
    public float previewDist = 0.5f;
    public float previewScale = 0.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        debugOut = "";
        if (mainCamera == null || snaps == null) return;
        if (Input.GetMouseButtonDown(2)) buildStuff = !buildStuff;
        switchSnap = false;
        if (buildStuff)
        {
            if (Input.mouseScrollDelta.y < 0f)
            {
                snapI--;
                if (snapI < 0) snapI = snaps.Length - 1;
                switchSnap = true;
            }
            if (Input.mouseScrollDelta.y > 0f)
            {
                snapI++;
                if (snapI == snaps.Length) snapI = 0;
                switchSnap = true;
            }

            RaycastHit hit;

            if(switchSnap)
            {
                if (snap != null) Destroy(snap.gameObject);
                snap = Instantiate(snaps[snapI], Vector3.zero, mainCamera.rotation) as Transform;
                snap.name = snaps[snapI].name;
                switchSnap = false;
            }

            if (snap != null)
            {
                snap.position = mainCamera.position + (mainCamera.forward * previewDist);
                snap.rotation = mainCamera.rotation;
                snap.localScale = new Vector3(previewScale, previewScale, previewScale);
            }
            // Does the ray intersect layer 9 (Snaps)
            int layerMask = 1 << 9;



            layerMask = ~layerMask;

            Transform snapper = null;
            if (snap!=null && Physics.Raycast(mainCamera.position, mainCamera.TransformDirection(Vector3.forward), out hit, 5f, ~layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                debugOut += hit.transform.name;

                if (IsFreeToSnap(hit.transform) && HasMyTag(hit.transform.name, snap.name))
                {
                    snapper = hit.transform;
                    snap.position = snapper.position;
                    snap.rotation = snapper.rotation;
                    snap.localScale = new Vector3(1f, 1f, 1f);
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                debugOut+="Did not Hit";
            }

            if(Input.GetMouseButtonDown(0))
            {
                if (snapper != null)
                {
                    snap.parent = snapper;
                }
                snap = null;
            }
        }
    }
    string debugOut = "";
    void OnGUI()
    {
        if (!buildStuff || snaps == null) return;
        GUI.Label(new Rect(10f, 10f, 1000, 1000), snaps[snapI].name + "\n" + debugOut);
    }

    bool IsFreeToSnap(Transform trans)
    {
        if (trans.childCount == 0) return true;
        return false;
    }

    bool HasMyTag(string snapName, string myName)
    {
        if (snapName.Contains(myName)) return true;
        return false;
    }
}
