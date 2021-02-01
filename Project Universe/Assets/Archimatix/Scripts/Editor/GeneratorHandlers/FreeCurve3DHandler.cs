#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


using AX;
using AX.Generators;
using AXGeometry;
using AXEditor;

using Parameters = System.Collections.Generic.List<AX.AXParameter>;



namespace AX.GeneratorHandlers
{

    public class FreeCurve3DHandler : GeneratorHandler
    { 
        private const float handleSize = 0.04f;
        private const float pickSize = 0.06f;

        public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
        {
            FreeCurve3D gener = (FreeCurve3D)parametricObject.generator;

            base.drawControlHandles(ref visited, consumerM, true);

            if (alreadyVisited(ref visited, "FreeCurveHandler"))
                return;

            Color prevHandlesColor = Handles.color;

            List<CurveControlPoint3D> controlPoints = parametricObject.curve3D.controlPoints;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                Handles.color = new Color(1, 1, 1, .3f);  
                if (i>0)
                {
                    Handles.DrawLine(controlPoints[i - 1].position, controlPoints[i].position);
                }

                ShowControlPoint(i);
            }


           
            for (int i=1; i< parametricObject.curve3D.derivedPathPoints.Count; i++)
            {
                Handles.color = Color.magenta;
                Handles.DrawLine(parametricObject.curve3D.derivedPathPoints[i - 1].position, parametricObject.curve3D.derivedPathPoints[i].position);

                Handles.color = Color.green;
                Handles.DrawLine(parametricObject.curve3D.derivedPathPoints[i - 1].position, parametricObject.curve3D.derivedPathPoints[i - 1].position + parametricObject.curve3D.derivedPathPoints[i - 1].normal);

                Handles.color = Color.blue;
                Handles.DrawLine(parametricObject.curve3D.derivedPathPoints[i - 1].position, parametricObject.curve3D.derivedPathPoints[i - 1].position + parametricObject.curve3D.derivedPathPoints[i - 1].tangent.normalized);
            }

            Handles.color = prevHandlesColor;
        }


        private Vector3 ShowControlPoint(int index)
        {

            List<CurveControlPoint3D> points = parametricObject.curve3D.controlPoints;

            float size = HandleUtility.GetHandleSize(points[index].position);

            if (index == 0)
            {
                size *= 2f;
            }




            if (parametricObject.curve3D.IsSelected(index))
            {
                //Debug.Log("here");
                //ArchimatixEngine.snappingIsOn;

                EditorGUI.BeginChangeCheck();
                Vector3 pos = points[index].position;
                pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    if (ArchimatixEngine.snappingOn())
                        pos = AXGeometry.Utilities.SnapToGrid(pos, parametricObject.model.snapSizeGrid);

                    Vector3 DIFF = pos - points[index].position;

                    parametricObject.curve3D.MoveSelectedControlPoints(DIFF);

                    Undo.RegisterCompleteObjectUndo(parametricObject.model, "Move Point");
                    // Undo.RecordObject(spline, "Move Point");
                    // EditorUtility.SetDirty(spline);
                    //spline.points[index].position = point;

                    parametricObject.model.isAltered();
                }
            }


            //Handles.color = modeColors[(int)spline.GetControlPointMode(index)];

            if (parametricObject.curve3D.IsSelected(index))
                Handles.color = Color.red;
            else
                Handles.color = Color.cyan;

            if (Handles.Button(points[index].position, Quaternion.identity, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {

                if (parametricObject.curve3D.IsSelected(index))
                {
                    if (Event.current.command)
                        parametricObject.curve3D.DeselectPoint(index);
                    else
                    {
                        Debug.Log("deslel all");
                        parametricObject.curve3D.DeselectAllPoints();
                    }
                }
                else
                {
                    if (!Event.current.shift && !Event.current.command)
                    {
                        parametricObject.curve3D.DeselectAllPoints();
                    }

                    if (Event.current.shift)
                    {             
                        parametricObject.curve3D.SelectToPoint(index);
                    }
                    else
                        parametricObject.curve3D.SelectPoint(index);
                }
                //Repaint();
            }


          
            return points[index].position;
        }



        public override void OnSceneGUI()
        {
            drawGUIControls();

          


        } // \OnScenView



        public void drawGUIControls()
        {
            //FreeCurve gener = (FreeCurve) generator;

            //AXModel model = ArchimatixEngine.currentModel;
            FreeCurve3D gener = (FreeCurve3D)parametricObject.generator;

            //Debug.Log(ArchimatixEngine.uiIcons["AddPoint"] );
            if (ArchimatixEngine.uiIcons == null || ArchimatixEngine.uiIcons.Count == 0 || ArchimatixEngine.uiIcons["AddPoint"] == null)
                AXEditorUtilities.loaduiIcons();



            // 2D GUI
            Handles.BeginGUI();

            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            float prevFixedWidth = buttonStyle.fixedWidth;
            float prevFixedHeight = buttonStyle.fixedHeight;


            buttonStyle.fixedHeight = 30;

            //GUIStyle areaStyle = new GUIStyle();
            //areaStyle. = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));

            float windowWidth = (SceneView.lastActiveSceneView != null) ? SceneView.lastActiveSceneView.position.width : Screen.width;
            float buttonWid = 40;
            float yHgt = 20;


            if (GUI.Button(new Rect(((windowWidth - buttonWid) / 2 - 3f * buttonWid), yHgt, buttonWid, 30), new GUIContent(ArchimatixEngine.uiIcons["AddPoint"], "Add Point")))
            {
                Undo.RegisterCompleteObjectUndo(parametricObject.model, "Add Point");
                parametricObject.curve3D.addControlPoint();
            }


            


            buttonStyle.fixedWidth = prevFixedWidth;
            buttonStyle.fixedHeight = prevFixedHeight;
        }
    }
}