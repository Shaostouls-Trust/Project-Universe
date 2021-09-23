using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Demo
{
    public class UncapAngularVelocity : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Infinity;
        }
    }
}

