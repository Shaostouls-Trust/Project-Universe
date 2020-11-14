using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;







namespace AXGeometry
{
    /// <summary>
    /// Curve3D
    /// Manages a collection of CurvePoint3D's
    /// </summary>
    /// 

    [System.Serializable]
    public class Curve3D {

        public List<CurveControlPoint3D> controlPoints = new List<CurveControlPoint3D>();

        // DERIVED PATH POINTS
        // The creation of derivedPoints is a product of which interpolation method is used; Catmull-Rom, Quadratic Bezier, Cubic Bezier, Piping, Ramping, etc.
        public List<CurvePoint3> derivedPathPoints;

        public List<int> lines = new List<int>();

        //float geomBreakingAngle = 60f;

        public List<int> selectedControlPointIndices = new List<int>();

        public int segs = 25;

        public Curve3D()
        {


        }

        public Curve3D(Vector3 v1, Vector3 v2)
        {
            CurveControlPoint3D p1 = new CurveControlPoint3D(v1);
            CurveControlPoint3D p2 = new CurveControlPoint3D(v2);

            controlPoints.Add(p1);
            controlPoints.Add(p2);
        }

        public CurveControlPoint3D addControlPoint()
        {
            CurveControlPoint3D p = null;

            if (controlPoints.Count == 0)
                p = new CurveControlPoint3D();
            else if (controlPoints.Count == 1)
                p = new CurveControlPoint3D(controlPoints[0].position+new Vector3(3,3,3));

            else
            {
                CurveControlPoint3D p1 = controlPoints[controlPoints.Count - 2]; 
                CurveControlPoint3D p2 = controlPoints[controlPoints.Count - 1];
                Vector3 diff = p2.position - p1.position;

                p = new CurveControlPoint3D(p2.position + diff);
            }
            Debug.Log("Add point : " + p.position);
            controlPoints.Add(p);
            return p;
        }

        public CurveControlPoint3D addControlPoint(Vector3 v)
        {
            CurveControlPoint3D p = new CurveControlPoint3D(v);

            controlPoints.Add(p);
            return p;
        }


        // CONTROL POINT SELECTION
        public void SelectPoint(int index)
        {
            if (! selectedControlPointIndices.Contains(index))
            {
                selectedControlPointIndices.Add(Mathf.Clamp(index, 0, controlPoints.Count - 1));
            }

            selectedControlPointIndices.Sort();
        }

        public void SelectToPoint(int index)
        {
            // find the index where the new index would be.

            if (selectedControlPointIndices.Count == 0)
            {
                SelectPoint(index);
                return;
            }
            else 
            {
                int toSelectedIndicesIndex = -1;

                for (int i=0; i<selectedControlPointIndices.Count; i++)
                {
                    if (selectedControlPointIndices[i] == index - 1)
                    {
                        SelectPoint(index);
                        toSelectedIndicesIndex = -1;
                        break;
                    }
                    else if (selectedControlPointIndices[i] < index)
                        toSelectedIndicesIndex = i;
                }
                if (toSelectedIndicesIndex > -1)
                {
                    // we fonud a gap below
                    for (int i = selectedControlPointIndices[toSelectedIndicesIndex]; i <= index; i++)
                        SelectPoint(i);

                    return;
                }


               // Debug.Log("HERE: " + (selectedPointIndices.Count - 1)
                for (int i = selectedControlPointIndices.Count-1; i >= 0; i--)
                {
                    //Debug.Log(i + " " + selectedPointIndices[i] + " -- " + (selectedPointIndices[i] > index));
                    if (selectedControlPointIndices[i] == index + 1)
                    {
                        SelectPoint(index);
                        toSelectedIndicesIndex = -1;
                        break;
                    }
                    else if (selectedControlPointIndices[i] > index)
                        toSelectedIndicesIndex = i;
                }
                //Debug.Log("backer " + toSelectedIndicesIndex);
                if (toSelectedIndicesIndex > -1)
                {
                   
                    // we fonud a gap below
                    for (int i = selectedControlPointIndices[toSelectedIndicesIndex]; i >= index; i--)
                        SelectPoint(i);

                    return;
                }
            }
        }

        public bool IsSelected(int index)
        {
            if (selectedControlPointIndices.Contains(index))
                return true;

            return false;
        }

        public void DeselectPoint(int index)
        {
            if (selectedControlPointIndices.Contains(index))
            {
                selectedControlPointIndices.Remove(index);
            }
         }

        public void DeselectAllPoints()
        {
            selectedControlPointIndices.Clear();
        }



        public void MoveSelectedControlPoints(Vector3 v)
        {
            for (int i=0; i<selectedControlPointIndices.Count; i++)
            {
                controlPoints[selectedControlPointIndices[i]].position += v;
            }

        }


        public void calculateLengths()
        {
            float u = 0;
            for (int i = 1; i < derivedPathPoints.Count; i++)
            {
                int prev_i = i - 1;
                int this_i = (i == derivedPathPoints.Count) ? 0 : i;
                int next_i = (i == derivedPathPoints.Count - 1) ? 0 : i + 1;

                Vector3 v1 = derivedPathPoints[this_i].position - derivedPathPoints[prev_i].position;
                Vector3 v2 = derivedPathPoints[next_i].position - derivedPathPoints[this_i].position;

                u += v1.magnitude;
                derivedPathPoints[i].u = u;

 
                derivedPathPoints[this_i].angle = Vector3.Angle(v1, v2);
                //Debug.Log(derivedPathPoints[i].angle);
            }
        }

        // DERIVE PATH POINTS

        public void DerivePoints()
        {
            derivedPathPoints = new List<CurvePoint3>();


            DerivePiping(12, 3);
            //DeriveMidpointQuadraticBezier(5);

        }



        // PIPING
        public void DerivePiping(int segments, float rad)
        {

            if (controlPoints == null || controlPoints.Count == 0)
                return;
            else if (controlPoints.Count == 1)
                derivedPathPoints.Add(new CurvePoint3(controlPoints[0].position));

            derivedPathPoints.Add(new CurvePoint3(controlPoints[0].position, (controlPoints[1].position - controlPoints[0].position).normalized, Vector3.up));

            if (controlPoints.Count == 2)
                return;


            // Do quadratic beziers from midpoint to midpoint

            for (int i = 1; i < controlPoints.Count - 1; i++)
            {
                // POINT A
                float lenA = (controlPoints[i].position - controlPoints[i - 1].position).magnitude;
                float ta = 1 - (rad / lenA);
                Vector3 A = Vector3.Lerp(controlPoints[i - 1].position, controlPoints[i].position, ta);

                // POINT B
                Vector3 B = controlPoints[i].position;

                // POINT C
                float lenB = (controlPoints[i + 1].position - controlPoints[i].position).magnitude;
                float tb = rad / lenB;
                Vector3 C = Vector3.Lerp(controlPoints[i].position, controlPoints[i + 1].position, tb);



                // Add points from a Quadratic bezier from A, B, C

                for (int j = 0; j < segments; j++)
                {
                    float t = (1.0f * j) / segments;

                    Vector3 pos = AXBezier.GetPoint(A, B, C, t);
                    Vector3 tng = AXBezier.GetTangent(A, B, C, t);
                    derivedPathPoints.Add(new CurvePoint3(pos, tng, Vector3.up));
                }
            }

            derivedPathPoints.Add(new CurvePoint3(controlPoints[controlPoints.Count - 1].position, (controlPoints[controlPoints.Count - 1].position - controlPoints[controlPoints.Count - 2].position).normalized, Vector3.up));

            calculateLengths();
       }



        public void DeriveMidpointQuadraticBezier(int segments = 5)
        {
            derivedPathPoints.Add(new CurvePoint3(controlPoints[0].position));

            // Do quadratic beziers from midpoint to midpoint

            for (int i=2; i<controlPoints.Count; i++)
            {
                Vector3 midpoint1 = Vector3.Lerp(controlPoints[i - 2].position, controlPoints[i - 1].position, .5f);
                Vector3 midpoint2 = Vector3.Lerp(controlPoints[i-1].position, controlPoints[i].position, .5f);

                // Add points from a Quadratic bezier from midpoint1, controlPoints[i-1], midpoint2

                for (int j=0; j< segs; j++)
                {
                    float t = (1.0f * j) / segments;

                    Vector3 pos = AXBezier.GetPoint(midpoint1, controlPoints[i - 1].position, midpoint2, t);
                    Vector3 tng = AXBezier.GetTangent(midpoint1, controlPoints[i - 1].position, midpoint2, t);
                    derivedPathPoints.Add(new CurvePoint3(pos, tng, Vector3.up));
                }
            }
        }







        public CurveControlPoint3D prevControlPoint(int index)
        {
            if (controlPoints.Count > 0)
                return controlPoints[(((index - 1) < 0) ? (controlPoints.Count - 1) : index - 1)];
            return null;
        }
        public CurveControlPoint3D thisControlPoint(int index)
        {
            // useful when you want to be able to stray beyong the Count of the control vertices    
            if (controlPoints.Count > 0)
                return controlPoints[(index > (controlPoints.Count - 1)) ? 0 : index];
            return null;
        }
        public CurveControlPoint3D nextControlPoint(int index)
        {
            if (controlPoints.Count > 0)
                return controlPoints[(index + 1) % controlPoints.Count];
            return null;
        }

    }
}
