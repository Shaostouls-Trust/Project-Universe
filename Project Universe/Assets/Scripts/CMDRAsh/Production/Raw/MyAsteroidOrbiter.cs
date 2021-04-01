using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAsteroidOrbiter : MonoBehaviour
{
    //size of the solar system
    [SerializeField] private long SolarRadius;
    [SerializeField] private float AngularVelocityAtRadius;
    //v = wr

    public long GetSolarRadius()
    {
        return SolarRadius;
    }
    public float GetAngularVelocity()
    {
        return AngularVelocityAtRadius;
    }
}
