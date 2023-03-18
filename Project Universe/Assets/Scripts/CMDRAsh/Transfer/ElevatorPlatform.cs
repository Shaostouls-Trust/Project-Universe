using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 shaftCenterForLevel;

    public Vector3 ShaftCenterForLevel
    {
        get { return shaftCenterForLevel; }//transform.TransformPoint(shaftCenterForLevel); }
    }

}
