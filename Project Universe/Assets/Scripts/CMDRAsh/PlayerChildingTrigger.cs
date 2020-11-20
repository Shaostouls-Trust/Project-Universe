using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChildingTrigger : MonoBehaviour
{
    [SerializeField]
    public Transform nextLevelRoot;

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.SetParent(nextLevelRoot);
    }
}
