using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is a test object for gameobject tiles. It should be able to use TileObjectBase to get the stuff it needs to save and instanciate. Or, should this
 * class have the constructor seperate from TOB (IE no TOB objects get created, rather the inheritters get instances.
 */
public class TestGameObject : DataHandling.TileObjectBase
{
    public UnityEngine.GameObject obj;//reference var

    //called when object dies for purpose of saving meta before destruction.
    private void OnDestroy()
    {
        
    }
    /*
     * Assign metadata to this object (get it from the fields)
     */
    protected override void Start()
    {
        
    }

    /*
     * Used to look for changes in meta and update accordingly (server-client relation).
     */
    protected override void Update()
    {
        
    }

    /*
     * TestGameObject constructor. Only used for loading (in cases where no object exists)
     */
    public TestGameObject()
    {

    }
}
