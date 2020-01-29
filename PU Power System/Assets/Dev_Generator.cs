using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dev_Generator : IGenerator
{
    public Dev_Generator()
    {
        IGenerator generator = new IGenerator();
        base.totalVoltage = 120f;
        base.currentVoltage = 120f;
        base.legs = 1;
        
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //base.Update();
    }
}
