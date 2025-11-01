using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion
{
    public class rotateLightORION : MonoBehaviour
    {

        public float speed = 5.0f;

        private Vector3 lastMousePosition;

        void Update()
        {
            Vector3 DTT = (lastMousePosition - Input.mousePosition) * speed * Time.deltaTime ;

            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
            {
                transform.Rotate(new Vector3(-DTT.y, -DTT.x, 0));
            }

            lastMousePosition = Input.mousePosition;
        }
    }
}
