using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILinkOnCreation : MonoBehaviour
{
    private GameObject[] shipGenerators;
    private GameObject[] shipRouters;
    private GameObject[] shipSubstations;
    private GameObject[] shipMachines;
    private GameObject[] shipBreakers;
    private GameObject[] shipSubMachines;
    public ILinkOnCreation(GameObject shipRoot)
    {
        shipGenerators = GetAllChildrenWithComponent<IGenerator>(shipRoot);
        shipRouters = GetAllChildrenWithComponent<IRouter>(shipRoot);
        shipSubstations = GetAllChildrenWithComponent<IRoutingSubstation>(shipRoot);
        shipMachines = GetAllChildrenWithComponent<IMachine>(shipRoot);
        shipBreakers = GetAllChildrenWithComponent<IBreakerBox>(shipRoot);
        shipSubMachines = GetAllChildrenWithComponent<ISubMachine>(shipRoot);
        //StartCoroutine(SplittingLogic());
        //SplittingLogic();
    }

    public void LinkSystems(IGenerator generator, IRouter[] router)
    {
        generator.SetRouters(router);
    }
    public void LinkSystems(IRouter router, IRoutingSubstation[] subs)
    {
        router.SetSubstations(subs);
    }
    public void LinkSystems(IRoutingSubstation subs, IMachine[] machines)
    {
        subs.SetMachines(machines);
    }
    public void LinkSystems(IRoutingSubstation subs, IBreakerBox[] breakers)
    {
        subs.SetBreakers(breakers);
    }
    public void LinkSystems(IBreakerBox breaker, ISubMachine[] submachines)
    {
        breaker.SetMachines(submachines);
    }
    //IEnumerator SplittingLogic()
   
    /// <summary>
    /// Establish connections between all powersystem components in the ship.
    /// Starting from the ISubMachines and working in, so as to call all ProxyStart methods in the proper order.
    /// </summary>
    private void SplittingLogic()
    {
        List<List<GameObject>> masterList = new List<List<GameObject>>();
        float[] distances = new float[shipRouters.Length * shipSubstations.Length];
        int count = 0;
        ////ISubMachine-IBreakerBox linking
        distances = new float[shipSubMachines.Length * shipBreakers.Length];
        //Debug.Log(distances.Length);
        count = 0;
        foreach (GameObject breaker in shipBreakers)
        {
            masterList.Add(new List<GameObject>());
            DistanceCalculationHelper(breaker, shipSubMachines, ref count, ref distances);
        }
        if (shipBreakers.Length > 0)
        {
            SortByDistance(distances, shipSubMachines, shipBreakers, ref masterList);
            //assign the new links to each router
            for (int k = 0; k < masterList.Count; k++)
            {
                ISubMachine[] subMach = new ISubMachine[masterList[k].Count];
                for (int l = 0; l < masterList[k].Count; l++)
                {
                    subMach[l] = masterList[k][l].GetComponent<ISubMachine>();
                }
                LinkSystems(shipBreakers[k].GetComponent<IBreakerBox>(), subMach);
                Debug.Log("Linked " + shipBreakers[k].name + " to " + subMach.Length + " subMachines.");
                shipBreakers[k].GetComponent<IBreakerBox>().ProxyStart();
            }
        }

        ////IMachine-Substation Linking
        masterList.Clear();
        distances = new float[shipMachines.Length * shipSubstations.Length];
        //Debug.Log(distances.Length);
        count = 0;
        foreach (GameObject subst in shipSubstations)
        {
            masterList.Add(new List<GameObject>());
            DistanceCalculationHelper(subst, shipMachines, ref count, ref distances);
        }
        if (shipSubstations.Length > 0)
        {
            //THIS HERE NEEDS A CAPACITY CHECK!!!
            SortByDistance(distances, shipMachines, shipSubstations, ref masterList);
            //assign the new links to each router
            for (int k = 0; k < masterList.Count; k++)
            {
                IMachine[] mach = new IMachine[masterList[k].Count];
                for (int l = 0; l < masterList[k].Count; l++)
                {
                    mach[l] = masterList[k][l].GetComponent<IMachine>();
                }
                LinkSystems(shipSubstations[k].GetComponent<IRoutingSubstation>(), mach);
                Debug.Log("Linked " + shipSubstations[k].name + " to " + mach.Length + " machines.");
                shipSubstations[k].GetComponent<IRoutingSubstation>().ProxyStart(2);
            }
        }

        ////IRoutingSubstation-IBreakerBox linking
        masterList.Clear();
        distances = new float[shipBreakers.Length * shipSubstations.Length];
        count = 0;
        foreach (GameObject subst in shipSubstations)
        {
            masterList.Add(new List<GameObject>());
            DistanceCalculationHelper(subst, shipBreakers, ref count, ref distances);
        }
        //link breaker boxes to nearest substation
        if (shipBreakers.Length > 0)
        {
            SortByDistance(distances, shipBreakers, shipSubstations, ref masterList);
            //assign the new links to each router
            for (int k = 0; k < masterList.Count; k++)
            {
                IBreakerBox[] breaks = new IBreakerBox[masterList[k].Count];
                for (int l = 0; l < masterList[k].Count; l++)
                {
                    breaks[l] = masterList[k][l].GetComponent<IBreakerBox>();
                }
                LinkSystems(shipSubstations[k].GetComponent<IRoutingSubstation>(), breaks);
                Debug.Log("Linked " + shipSubstations[k].name + " to " + breaks.Length + " breaker.");
                shipSubstations[k].GetComponent<IRoutingSubstation>().ProxyStart(1);
            }
        }

        ////IRouter-IRoutingSubstation Linking
        foreach (GameObject route in shipRouters)
        {
            masterList.Add(new List<GameObject>());
            DistanceCalculationHelper(route, shipSubstations, ref count, ref distances); //float[] 
        }
        //yield return null;
        //determine which substation is closest to which router
        if (shipRouters.Length > 0)
        {
            SortByDistance(distances, shipSubstations, shipRouters, ref masterList);
            //assign the new links to each router
            for (int k = 0; k < masterList.Count; k++)
            {
                IRoutingSubstation[] substs = new IRoutingSubstation[masterList[k].Count];
                for (int l = 0; l < masterList[k].Count; l++)
                {
                    substs[l] = masterList[k][l].GetComponent<IRoutingSubstation>();
                }
                LinkSystems(shipRouters[k].GetComponent<IRouter>(), substs);
                Debug.Log("Linked " + shipRouters[k].name + " to " + substs.Length + " substations.");
                shipRouters[k].GetComponent<IRouter>().ProxyStart();
            }
        }

        ////generator-router linking
        masterList.Clear();
        //link routers to generator. For now, all generators will link to every router
        foreach (GameObject generator in shipGenerators)
        {
            IRouter[] routes = new IRouter[shipRouters.Length];
            for (int i = 0; i < shipRouters.Length; i++)
            {
                routes[i] = shipRouters[i].GetComponent<IRouter>();
            }
            Debug.Log("Linked "+generator.name+" to "+routes.Length+" routers");
            LinkSystems(generator.GetComponent<IGenerator>(), routes);
            generator.GetComponent<IGenerator>().ProxyStart();
        }
       
        ////router-substation linking
        //calculate the distance from each router to each substation
        
        ////breaker-machine linking
       
        //submachine to breker

        ////substation-machine linking
        
        //yield return null;

        //StopCoroutine("SplittingLogic");
    }

    public void SortByDistance(float[] distances, GameObject[] outerArray, GameObject[] innerArray, ref List<List<GameObject>> masterList)
    {
        float min;
        float current;
        int index = 0;
        for (int i = 0; i < outerArray.Length; i++)//subs
        {
            min = distances[i];
            for (int j = 0; j < innerArray.Length - 1; j++)//router
            {
                index = j;
                //compare distance of a and b until we reach the length of routers
                current = distances[j + outerArray.Length + i];//subs
                //Debug.Log("Distance: "+distances[j + outerArray.Length + i]);
                if (min >= current)
                {
                    min = current;
                    //if min is j+0, then the next router down the line is the one we are checking
                    index = j + 1;
                }
            }
            masterList[index].Add(outerArray[i]);//add to router's master list
        }
    }

    public void DistanceCalculationHelper(GameObject origin, GameObject[] targets, ref int startIndex, ref float[] distances)
    {
        //int count = 0;
        float x = origin.transform.localPosition.x;
        float y = origin.transform.localRotation.y;
        float z = origin.transform.localPosition.z;
        //float[] theseDistances = new float[targets.Length];
        double distance = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            //calculate the distance from this subst to the router
            distance = Math.Sqrt((
                Math.Pow(x - targets[i].transform.localPosition.x, 2)
                + Math.Pow(y - targets[i].transform.localPosition.y, 2)
                + Math.Pow(z - targets[i].transform.localPosition.z, 2)
                ));
            //Debug.Log(startIndex);
            distances[startIndex++] = (float)distance;
            //Debug.Log(distance);
        }
    }

    private GameObject[] GetAllChildrenWithComponent<T>(GameObject root)
    {
        List<GameObject> allGameobjects = new List<GameObject>();
        List<T> childComponents = new List<T>();
        root.GetComponentsInChildren<T>(false, childComponents);
        IList list = childComponents;
        //dynamic type3 = (T) Convert.ChangeType(type2, typeof(T));
        //for (int i = 0; i < list.Count; i++)
        // {
        //    type3 = (T) list[i];
        //    allGameobjects.Add(type3.gameObject);
        //}
        if(typeof(T) == typeof(ISubMachine))
        {
            for(int i = 0; i <list.Count; i++)
            {
                ISubMachine type = (ISubMachine)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        else if (typeof(T) == typeof(IMachine))
        {
            for (int i = 0; i < list.Count; i++)
            {
                IMachine type = (IMachine)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        else if (typeof(T) == typeof(IRoutingSubstation))
        {
            for (int i = 0; i < list.Count; i++)
            {
                IRoutingSubstation type = (IRoutingSubstation)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        else if (typeof(T)==typeof(IRouter)){
            for (int i = 0; i < list.Count; i++)
            {
                IRouter type = (IRouter)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        else if (typeof(T) == typeof(IBreakerBox))
        {
            for (int i = 0; i < list.Count; i++)
            {
                IBreakerBox type = (IBreakerBox)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        else if (typeof(T) == typeof(IGenerator))
        {
            for (int i = 0; i < list.Count; i++)
            {
                IGenerator type = (IGenerator)list[i];
                allGameobjects.Add(type.gameObject);
            }
        }
        return allGameobjects.ToArray();
    }
}
