using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using AXEditor;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


[System.Serializable]
public class AXShapeEditorWindow : EditorWindow
{


    //private static AXShapeEditorWindow _instance = null;
 

    //[MenuItem("Window/Archimatix/Shape Editor")]
    public static void Init()
    {
        AXShapeEditorWindow edwin = (AXShapeEditorWindow)EditorWindow.GetWindow(typeof(AXShapeEditorWindow));
        edwin.titleContent = new GUIContent("AX ShapeEditor");
        edwin.autoRepaintOnSceneChange = true;
        // edwin.setPositionIfNotDocked();
    }





    public static AXModel model = null;
    public static AXParametricObject currentParametricObject;
    public static AXParametricObject currentShapePO;


    float headerHgt = 18;
    float footerHgt = 125;
    float rightSidebarWidth = 68;
    float leftSidebarWidth = 68;
   // float leftSidebar2DHeightPercentage = .5f;
    Rect leftSidebarRect;
    Rect rightSidebarRect;

    Vector2 origin;


    Rect graphRect;
    Vector2 focusCenter;

    public static float maxZoom = 10.0f;

    public static float zoomScale = 3f;
    public float zoomScalePrev = 3f;
    public static Vector2 zoomPosAdjust;


    public float targetZoomScale;
    public bool isZoomingScale;

    public Vector2 panTo;
    public float panToSpeed = 1.5f;




    public enum ShapeEditorState { Default, DraggingWindowSpace, ZoomingWindowSpace, AddPoint };

    public ShapeEditorState editorState;

    public Vector2 mouseDownPoint;
    public Vector2 dragStart;

    public bool mouseJustDown = false;
    public bool mouseHasBeenDragged = false;
    public long mouseDownTime;



    public Texture2D menubarTexture;



    public Color gridColor;
    public Color axisColor;
    public Color curveShadowColor;
    public Color splineColor;


    Vector3 pos;



    //int currentWindowOffset = 0;


    public void OnInspectorUpdate()
    {

        //if(Event.current.type == EventType.Repaint)
        Repaint();
    }


    public void OnEnable()
    {
        ArchimatixEngine.establishPaths();

        if (EditorGUIUtility.isProSkin)
        {
            menubarTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenubarDark.png", typeof(Texture2D));
            //resizeCornerTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-ResizeCornerDark.png", typeof(Texture2D));
            //menuIconTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenuIconDark.png", typeof(Texture2D));
              
        }
        else
        {
            menubarTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenubarLight.png", typeof(Texture2D));
            // resizeCornerTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-ResizeCornerLight.png", typeof(Texture2D));
            // menuIconTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXIcons-MenuIconLight.png", typeof(Texture2D));

        }

    }




    void Update()
    {







        if (isZoomingScale)
        {

            //zoomScale = model.zoomScale;

            zoomScale = Mathf.Lerp(zoomScale, targetZoomScale, panToSpeed * Time.deltaTime);

            if (Mathf.Abs(zoomScale - targetZoomScale) < .1f)
                isZoomingScale = false;
            //model.zoomScale = zoomScale;
        }

        //focusCenter = Vector2.Lerp(focusCenter, model.panToPoint, .18f * Time.deltaTime);

        //if ((focusCenter - model.panToPoint).magnitude < 150f)
        //    model.endPanningToPoint();

        //model.focusPointInGraphEditor = focusCenter;

        zoomPosAdjust = (graphRect.center - origin) / zoomScale - focusCenter;

        //  if (Event.current.type == EventType.Repaint)
        //  Repaint();







    }


    // ON_GUI 
    void OnGUI()
    {
        if (ArchimatixEngine.currentModel != null)
            model = ArchimatixEngine.currentModel;

        if (model != null)
        {
            currentParametricObject = model.recentlySelectedPO;
            if (currentParametricObject != null && currentParametricObject.is2D())
            {
                currentShapePO = currentParametricObject;
            }
           
        }





        headerHgt = 24;
        footerHgt = 32;


        float sidebarHeight = position.height - headerHgt - footerHgt - 2;

        //leftSidebarWidth = 68;
        //rightSidebarWidth = 68;
        leftSidebarWidth = 20;
        rightSidebarWidth = 0;

        leftSidebarRect = new Rect(0, headerHgt, leftSidebarWidth + 5, sidebarHeight);
        rightSidebarRect = new Rect(position.width - rightSidebarWidth, headerHgt, rightSidebarWidth, sidebarHeight);


        graphRect = new Rect(leftSidebarWidth, headerHgt, position.width - rightSidebarWidth - leftSidebarWidth, position.height - headerHgt - footerHgt);


        //relationEditorRect = new Rect(leftSidebarWidth, position.height + footerHgt / 2 - 170, graphRect.width - 1, 170);


        origin = new Vector2(leftSidebarWidth, headerHgt);





        Event e = Event.current;

        switch (e.rawType)
        {
            case EventType.MouseDown:
                {
                    editorState = ShapeEditorState.DraggingWindowSpace;
                    e.Use();

                    break;
                }
            case EventType.MouseUp:
                {

                    mouseJustDown = false;


                    //onMouseUp();
                    break;
                }
            case EventType.MouseDrag:
                {
                    if (editorState == ShapeEditorState.DraggingWindowSpace)
                    {
                        // drag entire view
                        //if (model != null)
                        //{
                        //    zoomScale = model.zoomScale;
                        //    focusCenter = model.focusPointInGraphEditor;
                        //}
                        if (zoomScale < .01f && zoomScale > -.01f) zoomScale = 1;



                        //
                        if (e.alt && (e.button == 1 || e.control))
                        {
                            // ZOOM - Windows: alt-RMB, OS X control-alt

                            Vector2 mouse_displ_px = mouseDownPoint - graphRect.center;
                            focusCenter += mouse_displ_px / zoomScale;
                            float deltaZoom = -e.delta.y / 100;
                            zoomScale -= deltaZoom;
                            zoomScale = Mathf.Clamp(zoomScale, .1f, maxZoom);
                            focusCenter -= mouse_displ_px / zoomScale;

                        }
                        else
                        {
                            // PAN
                            focusCenter -= e.delta / zoomScale;
                        }

                        //if (model != null)
                        //{
                        //    model.zoomScale = zoomScale;
                        //    model.focusPointInGraphEditor = focusCenter;
                        //}

                        zoomPosAdjust = (graphRect.center - origin) / zoomScale - focusCenter;

                        //doRepaint = true;
                    }

                    break;

                }
            case EventType.ScrollWheel:
                {

                    // ZOOM - logicwheel Zooming

                    if (leftSidebarRect.Contains(Event.current.mousePosition) || rightSidebarRect.Contains(Event.current.mousePosition) || GUI.GetNameOfFocusedControl().Contains("logicTextArea_"))
                        break;

                    //if (model != null)
                    //{
                    //    zoomScale = model.zoomScale;
                    //    focusCenter = model.focusPointInGraphEditor;
                    //}
                    Vector2 mouse_displ_px = Event.current.mousePosition - graphRect.center;

                    // shift focus temporarily based on mouse point
                    focusCenter += mouse_displ_px / zoomScale;

                    // alter zoom based on scoll delta
                    float deltaZoom = e.delta.y / 100;
                    zoomScale -= deltaZoom;

                    zoomScale = Mathf.Clamp(zoomScale, .1f, maxZoom);

                    // resift now that zoom is done by subtracting out the mouse distance in the new zoom
                    focusCenter -= mouse_displ_px / zoomScale;


                    //if (model != null)
                    //{
                    //    model.zoomScale = zoomScale;
                    //    model.focusPointInGraphEditor = focusCenter;
                    //    model.endPanningToPoint(); ;
                    //}

                    zoomPosAdjust = (graphRect.center - origin) / zoomScale - focusCenter;

                    //doRepaint = true;

                    break;
                }

        }


        // BEGIN SCALED SPACE ** 
        EditorZoomArea.Begin(zoomScale, graphRect, graphRect.center);

        drawGraph();


        Debug.Log(Camera.current);
        Handles.BeginGUI();

        Handles.SetCamera(Camera.current);
        //EditorGUI.BeginChangeCheck();
        //pos = Handles.FreeMoveHandle(pos, Quaternion.identity, 1, Vector3.one, Handles.SphereHandleCap);
        //if (EditorGUI.EndChangeCheck())
        //{
            
        //}
        Handles.EndGUI();




        EditorZoomArea.End();




        // HEADER

        //Rect headerRect = new Rect(0, 2, position.width, headerHgt);
        //Rect headerMenuGraphicWithShadowRect = new Rect (0, 0, position.width, 29);
        Rect headerMenuGraphicWithShadowRect = new Rect(0, 0, position.width, 36);



        Texture2D oldBackgroundTexture = GUI.skin.box.normal.background;
        GUI.skin.box.normal.background = menubarTexture;

        GUI.Box(new Rect(0, 0, position.width, headerHgt), GUIContent.none);

        GUI.DrawTexture(headerMenuGraphicWithShadowRect, menubarTexture);

        GUI.skin.box.normal.background = oldBackgroundTexture;
         
         
        //GUI.backgroundColor = oldBackgroundColor;
        //GUI.backgroundColor = Color.white;
        //GUI.color = defaultColor;




    }






    public void drawGraph()
    {


        GUIDrawing.DrawGrid(zoomPosAdjust, Color.gray, Color.red, 10);

       // GUIDrawing.drawSquare(Vector2.zero, 30);


        if (currentShapePO != null && currentShapePO.is2D())
        {
            AXParameter outShape_p = currentShapePO.Output;

            //Debug.Log("outShape_p="+ outShape_p);
            if (outShape_p != null)
            {
                Paths outpaths = outShape_p.getPaths();

                if (outpaths != null && outpaths.Count > 0)
                {
                    //Vector3[] verts3d = AXGeometry.Utilities.path2Vec3s(outpaths[0], Axis.Z, (outShape_p.shapeState == ShapeState.Closed));
                    Vector2[] verts2D = AXGeometry.Utilities.path2Vec2s(outpaths[0]);


                    //for (int i = 0; i < verts2D.Length; i++)
                    //verts2D[i].y *= -1;


                    GUIDrawing.drawPath(verts2D, zoomPosAdjust, (outShape_p.shapeState == ShapeState.Closed), Color.cyan, 1);





                    //Debug.Log(currentShapePO.Name);

                    //Generator2D gener = (Generator2D) currentShapePO.generator;
                    //GeneratorHandler2D generHandler = (GeneratorHandler2D) GeneratorHandler.getGeneratorHandler(currentShapePO);



                    // Handles.color = Color.blue;
                    // generHandler.drawPathsForEditor(outShape);


                    //Handles.BeginGUI();

                    //Vector3 snap = Vector3.one * 0.5f;



                    //for (int i = 0; i < currentShapePO.curve.Count; i++)
                    //{ 
                    //    //Vector3 pos = verts2D[i] + zoomPosAdjust;
                    //    Vector3 pos = new Vector3(currentShapePO.curve[i].position.x, currentShapePO.curve[i].position.y, 0);

                    //    //pos = Vector3.one;
                    //    //Debug.Log(pos);

                    //    pos = Handles.FreeMoveHandle(
                    //      pos, 
                    //      Quaternion.identity,
                    //      1,
                    //      snap,
                    //      Handles.RectangleHandleCap
                    //        );

                    //}



                    //Handles.EndGUI();
                }


            }


        }

       

    }
}