using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

using AX;

public class ReplaceTurtleScript : MonoBehaviour {

    // A reference to the AXModel component
    public AXModel model;

    // The name of the node
    public string shapeName = "Rectangle_01";

    // the node (or AXParametricObject) that will get the modified TurtleScript.
    AXParametricObject m_shape;

	// Use this for initialization
	void Start () {
       
        if (model != null && ! string.IsNullOrEmpty(shapeName))
            m_shape = model.parametricObjects.Find(x => x.Name.Equals(shapeName));

    }


    public void setNewTurtleScript()
    {
        if (m_shape != null)
        {
            // We could grab turtle script from in-game TextField 
            // but for this example, let's create a new script here...

            StringBuilder sb = new StringBuilder();

            sb.Append("mov 0 0");
            sb.Append("dir 0");
            sb.Append("fwd len");
            sb.Append("left hgt");
            sb.Append("back len");

            // TurtleScripts are stored in the "code" field of an AXParametric Object (which is a node)
            m_shape.code = sb.ToString();

            // Be sure to let AX know the model has been altered...
            model.setAltered(m_shape);

            // Re-generate
            model.autobuild();

        }
    }
}
