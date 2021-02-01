using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class IHierarchyMapper : MonoBehaviour
{
    //public Dictionary<string,T> thisDict = new Dictionary<string,T>();
    // Start is called before the first frame update
    void Start()
    {
        Apple();
    }

    public void Apple()
    {
        ArrayList master = new ArrayList();
        Dictionary<string, string> dict1 = new Dictionary<string, string>();
        dict1.Add("Blueberry", "250_57");
        dict1.Add("Caramel", "250_1");
        master.Add(dict1);
        master.Add("Dandy lions");
        master.Add(new string[] { "Elon", "Musk" });
        string[] aaaa = new string[2];
        for(int i = 0; i < master.Count; i++)
        {
            //Debug.Log(i+": "+master[i].GetType());
            if (master[i].GetType() == typeof(Dictionary<string, string>))
            {
                //Debug.Log("Dict:");
                Dictionary<string, string> dictinner = master[i] as Dictionary<string,string>;
                dictinner.TryGetValue("Blueberry", out string vaaa);
                dictinner.TryGetValue("Caramel", out string baaa);
                Debug.Log("0: "+vaaa+" 1: "+baaa);
                //Debug.Log(master[0].GetType().GetMethod("ToString"));
            }
            else if(master[i].GetType() == typeof(string))
            {
                Debug.Log(master[i]);
            }
            else if(master[i].GetType() == typeof(string[]))
            {
                Debug.Log("String[]");
                string[] stringinner = master[i] as string[];
                Debug.Log(stringinner[0]);
                Debug.Log(stringinner[1]);
            }
        }
    }

    // Update is called once per frame
   // void Update()
    //{
        
   // }
}
