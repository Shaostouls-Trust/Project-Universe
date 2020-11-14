using UnityEngine;



using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

using AXGeometry;

namespace AX.Generators
{

    public class FreeCurve3D : AX.Generators.Generator, ICustomNode
    {

        public override string GeneratorHandlerTypeName { get { return "FreeCurve3DHandler"; } }

        public override void init_parametricObject()
        {
            //Debug.Log ("INIT " + parametricObject.Name); 

            base.init_parametricObject();

            // parameters


            P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Curve3D, AXParameter.ParameterType.Output, "Output Curve3D"));
            P_Output.hasInputSocket = false;
            P_Output.shapeState = ShapeState.Open;


        }

        // POLL INPUTS (only on graph change())
        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            base.pollInputParmetersAndSetUpLocalReferences();

            P_Output = parametricObject.getParameter("Output Curve3D");

        }

        // GENERATE FREE_CURVE
        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
        {
            //parametricObject.transMatrix = Matrix4x4.TRS(new Vector3(transX, transY, 0), Quaternion.Euler(0, 0, rotZ), new Vector3(1, 1, 1));

           //Debug.Log("gen freecurve3D");

            parametricObject.curve3D.segs = 20;

            //Debug.Log(" controlPoints="+ parametricObject.curve3D.controlPoints.Count);
            parametricObject.curve3D.DerivePoints();

           



            return null;
        }
    }
}