using UnityEngine;

public class SpinningCube : MonoBehaviour
{
    public float speed = 10f;

    private void Update()
    {
        transform.Rotate(Vector3.up, speed*Time.deltaTime);
        transform.Rotate(Vector3.left, speed * Time.deltaTime);
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
