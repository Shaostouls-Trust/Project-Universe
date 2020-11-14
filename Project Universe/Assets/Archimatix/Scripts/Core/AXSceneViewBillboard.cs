using UnityEngine;

#if UNITY_EDITOR  
using UnityEditor;
#endif

using System.Collections;

[ExecuteInEditMode]
public class AXSceneViewBillboard : MonoBehaviour {



    [Range(0f, 1f)]
    public float humanSize = 1f;

    [Range(1.610f, 1.740f)]
    private float humanHeight = 1.740f;
    [Range(0.395f, 0.465f)]
    private float humanWidth = 0.465f;

    private float originalHeight = 1.8f;//1.967f;
    private float originalWidth = .68f;//.58f; //0.63f;  

    void OnEnable()
    {
        if (Application.isEditor)
        {
            if (Application.isPlaying) gameObject.SetActive(false);
        }
        else
        {
            if (Application.isPlaying) Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        humanHeight = Mathf.Lerp(1.610f, 1.740f, humanSize);
        humanWidth = Mathf.Lerp(0.395f, 0.465f, humanSize);
        transform.localScale = new Vector3(humanWidth / originalWidth, 1f, humanHeight / originalHeight);
        //transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(0f, humanHeight * 0.5f, 0f);
    }






    void OnRenderObject () {
		#if UNITY_EDITOR  

		if (SceneView.lastActiveSceneView != null)
		{
			Vector3 targetPos = Camera.current.transform.position;

			#if UNITY_EDITOR  
			targetPos = SceneView.lastActiveSceneView.camera.gameObject.transform.position; //sceneCameras[0].transform.position;
			#endif

			if (Application.isPlaying)
				targetPos = Camera.current.transform.position;
                
			// From http://answers.unity3d.com/questions/36255/lookat-to-only-rotate-on-y-axis-how.html
			float distanceToPlane = Vector3.Dot(Vector3.up, targetPos - transform.position);
			Vector3 pointOnPlane = targetPos - (Vector3.up * distanceToPlane);
 			transform.LookAt(pointOnPlane, Vector3.up);


            // from atomicjoe: "The LookAt method doesn't work great for billboards because computer graphics frustum are linear instead of hemispherical, so the best way to orient a billboard towards a camera is just to use the same rotation for the billboard and the camera."
            // https://forum.unity.com/conversations/billboard-kyle-fix.518572/#message-859672
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, SceneView.lastActiveSceneView.camera.gameObject.transform.eulerAngles.y, transform.eulerAngles.z);        // Fix object to camera orientation


            transform.Rotate(new Vector3(-90, 90, -90));

		}
		#endif
	}

}
