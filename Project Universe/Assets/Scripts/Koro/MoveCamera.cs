using UnityEditor;
using UnityEngine;
//piity bopity your code is now my propety
public class MoveCamera : MonoBehaviour {

    public Transform player;

    public void Start()
    {

        
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update() {
        transform.position = player.transform.position;
    }
}
