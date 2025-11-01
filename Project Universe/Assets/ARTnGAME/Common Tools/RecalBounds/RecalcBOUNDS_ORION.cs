using UnityEngine;
using System.Collections;

namespace Artngame.SKYMASTER {
    [ExecuteInEditMode]
public class RecalcBOUNDS_ORION : MonoBehaviour {

	public void Start () {
            if (Application.isPlaying)
            {
                AA = this.GetComponent(typeof(MeshFilter)) as MeshFilter;
                this_transform = transform;
            }
	}
	MeshFilter AA;
	Transform this_transform;

        public bool calcBounds = false;
        public bool alwaysCalcBounds = false;
        public float boundsBigNumber = 1e6f;

        public bool realtiveBoundsCalc = false;


        public void Awake()
        {
            if (AA == null)
            {
                AA = this.GetComponent(typeof(MeshFilter)) as MeshFilter;
                this_transform = transform;
            }
            float bigNumber = boundsBigNumber;
            if (AA.sharedMesh != null)
            {
                AA.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));
            }
        }

            public void Update () {

            if ((calcBounds && Application.isPlaying) || alwaysCalcBounds)
            {
                if (AA == null)
                {
                    AA = this.GetComponent(typeof(MeshFilter)) as MeshFilter;
                    this_transform = transform;
                }

                calcBounds = false;
                //shiftMesh();
                //Debug.Log(AA.name);
                //AA.mesh = AA.sharedMesh;
                float bigNumber = boundsBigNumber;//1e6f;
                //AA.mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));
                AA.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));
                //AA.mesh.RecalculateBounds();
                //AA.sharedMesh.RecalculateBounds();
            }
            if (realtiveBoundsCalc)
            {
                if (Vector3.Distance(Camera.main.transform.position, this_transform.position) < 1000)
                {

                    Vector3 camPosition = Camera.main.transform.position;
                    Vector3 normCamForward = Vector3.Normalize(Camera.main.transform.forward);
                    float boundsDistance = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 2 + Camera.main.nearClipPlane;
                    Vector3 boundsTarget = camPosition + (normCamForward * boundsDistance);

                    Vector3 realtiveBoundsTarget = this.transform.InverseTransformPoint(boundsTarget);

                    if (Application.isPlaying)
                    {
                        Mesh mesh = AA.mesh;
                        mesh.bounds = new Bounds(realtiveBoundsTarget, Vector3.one);
                    }
                }
            }
	}

	private void shiftMesh(){ 
		Mesh mesh = AA.sharedMesh; 
		Vector3[] vertices = mesh.vertices; 
		mesh.vertices = vertices; 
		mesh.RecalculateBounds(); 
	} 
  }
}