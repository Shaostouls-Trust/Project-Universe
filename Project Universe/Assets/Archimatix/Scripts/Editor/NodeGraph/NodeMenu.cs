using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

using AXEditor;
namespace AX

{
    public class NodeMenu
    {
        public static Vector2 scrollPosition;


        public static void display(float imagesize = 64, AXNodeGraphEditorWindow editor = null)
        {
            //Debug.Log("imagesise="+imagesize);
            // called from an OnGUI
            //imagesize = 64;




            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUIStyle.none);

            EditorGUILayout.BeginVertical();

            string[] itemStrings = null;

            // Select menu item list
            if (editor.OutputParameterBeingDragged == null)
                if (editor.model != null && editor.model.selectedPOs.Count > 1)
                {

                    //if (editor.model.selectedPOs[0].is2D())
                    //	itemStrings = ArchimatixEngine.nodeStringsFrom2DMultiSelect;

                    //else
                    itemStrings = ArchimatixEngine.nodeStringsFromMultiSelect;
                }
                else
                    itemStrings = ArchimatixEngine.nodeStrings;
            else if (editor.OutputParameterBeingDragged.parametricObject.is2D())

                itemStrings = ArchimatixEngine.nodeStringsFrom2DOutput;

            else if (editor.OutputParameterBeingDragged.parametricObject.is3D())
            {
                if (editor.OutputParameterBeingDragged.Type == AXParameter.DataType.Spline)
                    itemStrings = ArchimatixEngine.nodeStringsFrom2DOutput;
                else
                    itemStrings = ArchimatixEngine.nodeStringsFrom3DOutput;
            }
            else if (editor.OutputParameterBeingDragged.parametricObject.generator is RepeaterTool)
                itemStrings = ArchimatixEngine.nodeStringsFromRepeaterTool;


            /*
            if (Library.last2DItem != null)
            {
                if (GUILayout.Button(new GUIContent(Library.last2DItem.icon, Library.last2DItem.po.Name), new GUILayoutOption[] {GUILayout.Width(imagesize), GUILayout.Height(imagesize)}))
                {

                }
            } 
            */


            List<string> stringList = null;

            if (itemStrings != null)
                stringList = itemStrings.ToList();

            if (stringList != null)
                stringList.AddRange(Archimatix.customNodeNames);

            // Build Menu
            string poName;

            if (stringList != null)
            {
                for (int i = 0; i < stringList.Count; i++)
                {
                    string nodeName = stringList[i];

                    Texture2D nodeIcon = null;


                    if (ArchimatixEngine.nodeIcons.ContainsKey(nodeName))
                        nodeIcon = ArchimatixEngine.nodeIcons[nodeName];
                    else
                    {
                        if (ArchimatixEngine.nodeIcons.ContainsKey("CustomNode"))
                            nodeIcon = ArchimatixEngine.nodeIcons["CustomNode"];
                        else
                            continue;
                    }






                    if (nodeIcon != null)
                    {
                        if (GUILayout.Button(new GUIContent(nodeIcon, nodeName), new GUILayoutOption[] { GUILayout.Width(imagesize), GUILayout.Height(imagesize) }))
                        {
                            //if (editor.DraggingOutputParameter != null)
                            //{
                            AXModel model = AXEditorUtilities.getOrMakeSelectedModel();

                            Undo.RegisterCompleteObjectUndo(model, "Node Menu Selection");

                            AXParametricObject mostRecentPO = model.mostRecentlyInstantiatedPO;

                            if (mostRecentPO == null)
                                mostRecentPO = model.recentlySelectedPO;


                            int index = nodeName.IndexOf("_", System.StringComparison.CurrentCulture);

                            poName = (index > 0) ? nodeName.Substring(0, index) : nodeName;

                            // Support multi-select operation
                            List<AXParametricObject> selectedPOs = new List<AXParametricObject>();
                            if (model.selectedPOs.Count > 0)
                                selectedPOs.AddRange(model.selectedPOs);




                            // ADD NEW PO TO MODEL (only this new po is selected after this)
                            AXParametricObject po = AXEditorUtilities.addNodeToCurrentModel(poName, false);

                            // COLLIDER


                            if (po == null || po.generator == null)
                                return;

                            if (po.is3D())
                                po.colliderType = model.defaultColliderType;







                            // 2D ORIENT TO DEFAULT AXIS

                            if (po.is2D() && SceneView.lastActiveSceneView != null)
                            {
                                po.setAxis(AXEditorUtilities.getDefaultAxisBasedBasedOnSceneViewOrientation(SceneView.lastActiveSceneView));
                            }






                            float max_x = -AXGeometry.Utilities.IntPointPrecision;


                            if (poName == "FreeCurve")
                            {
                                ArchimatixEngine.sceneViewState = ArchimatixEngine.SceneViewState.AddPoint;


                                //ArchimatixEngine.snappingIsOn = true;

                                //							if (SceneView.lastActiveSceneView.camera.orthographicSize < 1)
                                //								ArchimatixEngine.currentModel.snapSizeGrid = .1f;
                                //							else if (SceneView.lastActiveSceneView.camera.orthographicSize < 10)
                                //								ArchimatixEngine.currentModel.snapSizeGrid = .25f;
                                //							else if (SceneView.lastActiveSceneView.camera.orthographicSize < 100)
                                //								ArchimatixEngine.currentModel.snapSizeGrid = 1f;
                                //							else 
                                //								ArchimatixEngine.currentModel.snapSizeGrid = 5f;

                            }



                            // DRAGGING A PARAMETER? THEN RIG'R UP!
                            if (editor.OutputParameterBeingDragged != null)
                            {
                                AXParametricObject draggingPO = editor.OutputParameterBeingDragged.parametricObject;
                                AXParameter new_input_p = null;

                                switch (nodeName)
                                {
                                    case "Instance2D":
                                    case "ShapeOffsetter":
                                        po.getParameter("Input Shape").makeDependentOn(editor.OutputParameterBeingDragged);
                                        po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));

                                        if (po.geometryControls != null)
                                            po.geometryControls.isOpen = true;
                                        break;
                                          
                                    case "ShapeDistributor":
                                        List<AXParameter> deps = new List<AXParameter>();

                                        for (int dd = 0; dd < editor.OutputParameterBeingDragged.Dependents.Count; dd++)
                                            deps.Add(editor.OutputParameterBeingDragged.Dependents[dd]);

                                        for (int dd = 0; dd < deps.Count; dd++)
                                            deps[dd].makeDependentOn(po.getParameter("Output Shape"));

                                        po.getParameter("Input Shape").makeDependentOn(editor.OutputParameterBeingDragged);
                                        po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));

                                        if (po.geometryControls != null)
                                            po.geometryControls.isOpen = true;
                                        break;

                                    case "ShapeMerger":
                                        po.generator.getInputShape().addInput().makeDependentOn(editor.OutputParameterBeingDragged);
                                        if (editor.OutputParameterBeingDragged.axis != Axis.NONE)
                                            po.intValue("Axis", (int)editor.OutputParameterBeingDragged.axis);
                                        else
                                            po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));

                                        break;

                                    case "PlanRepeater2D":
                                    case "PlanRepeater2D_Corner":
                                        po.getParameter("Corner Shape").makeDependentOn(editor.OutputParameterBeingDragged);
                                        po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));
                                        break;

                                    case "PlanRepeater_Corner":
                                        po.getParameter("Corner Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;


                                    case "PairRepeater2D":
                                    case "RadialRepeater2D":
                                    case "RadialRepeater2D_Node":
                                    case "LinearRepeater2D":
                                    case "LinearRepeater2D_Node":
                                    case "GridRepeater2D":
                                    case "GridRepeater2D_Node":
                                        po.getParameter("Node Shape").makeDependentOn(editor.OutputParameterBeingDragged);
                                        po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));
                                        break;

                                    case "RadialRepeater2D_Cell":
                                    case "LinearRepeater2D_Cell":
                                    case "GridRepeater2D_Cell":
                                        po.getParameter("Cell Shape").makeDependentOn(editor.OutputParameterBeingDragged);
                                        po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));
                                        break;
                                    case "Channeler":
                                        AXParameter inputer = po.getOrAddInputMesh();

                                        inputer.makeDependentOn(editor.OutputParameterBeingDragged);

                                        break;

                                    case "Grouper":
                                        //po.addInputMesh().makeDependentOn(editor.OutputParameterBeingDragged);

                                       

                                       po.addGroupee(editor.OutputParameterBeingDragged.parametricObject);

                                        break;

                                    case "PlanRepeater2D_Plan":
                                    case "Polygon_Plan":
                                    case "Extrude_Plan":
                                    case "PlanSweep_Plan":
                                    case "PlanRepeater_Plan":
                                    case "PlanDeformer_Plan":
                                    case "PlanPlacer_Plan":
                                    case "ContourExtruder_Plan":

                                        // SYNC AXES
                                        if (editor.OutputParameterBeingDragged.axis != Axis.NONE)
                                            po.intValue("Axis", (int)editor.OutputParameterBeingDragged.axis);
                                        else
                                            po.intValue("Axis", editor.OutputParameterBeingDragged.parametricObject.intValue("Axis"));


                                        if (nodeName == "Extrude_Plan" && po.intValue("Axis") != (int)Axis.Y)
                                            po.floatValue("Bevel", 0);




                                        // INSERT SHAPE_DISTRIBUTOR?
                                        new_input_p = po.getParameter("Plan", "Input Shape");
                                        //if (draggingPO.is2D() && !(draggingPO.generator is ShapeDistributor) && editor.OutputParameterBeingDragged.Dependents != null && editor.OutputParameterBeingDragged.Dependents.Count > 0)
                                        //	model.insertShapeDistributor(editor.OutputParameterBeingDragged, new_input_p);
                                        //else
                                        new_input_p.makeDependentOn(editor.OutputParameterBeingDragged);

                                        // the output of the new node should match the shapestate of the input
                                        if (po.generator.P_Output != null)
                                            po.generator.P_Output.shapeState = new_input_p.shapeState;

                                        AXNodeGraphEditorWindow.repaintIfOpen();

                                        break;

                                    case "Lathe_Section":
                                    case "PlanSweep_Section":
                                    case "PlanRepeater_Section":
                                    case "ContourExtruder_Section":

                                        //po.getParameter("Section").makeDependentOn(editor.OutputParameterBeingDragged);

                                        // INSERT SHAPE_DISTRIBUTOR?
                                        new_input_p = po.getParameter("Section");
                                        if (draggingPO.is2D() && !(draggingPO.generator is ShapeDistributor) && draggingPO.hasDependents())
                                            model.insertShapeDistributor(editor.OutputParameterBeingDragged, new_input_p);
                                        else
                                            new_input_p.makeDependentOn(editor.OutputParameterBeingDragged);

                                        // the output of the new node should match the shapestate of the input
                                        //if (po.generator.P_Output != null)
                                        //Debug.Log(new_input_p.Name+" "+new_input_p.shapeState + " :=: " +editor.OutputParameterBeingDragged.Name + " " + editor.OutputParameterBeingDragged.shapeState);




                                        AXNodeGraphEditorWindow.repaintIfOpen();


                                        break;


                                    case "NoiseDeformer":
                                    case "ShearDeformer":
                                    case "TwistDeformer":
                                    case "DomicalDeformer":
                                    case "TaperDeformer":
                                    case "InflateDeformer":
                                    case "PlanDeformer":
                                    case "UVProjector":

                                        po.getParameter("Input Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    //case "PlanDeformer_Plan":



                                    case "PairRepeater":
                                    case "StepRepeater":
                                    case "RadialStepRepeater":


                                        po.getParameter("Node Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;


                                    case "LinearRepeater_Node":
                                        AXParameter nodeMesh_p = po.getParameter("Node Mesh");
                                        nodeMesh_p.makeDependentOn(editor.OutputParameterBeingDragged);

                                        // if the src is very long in x, assume you want to repeat in Z
                                        if (editor.OutputParameterBeingDragged.parametricObject.bounds.size.x > (6 * editor.OutputParameterBeingDragged.parametricObject.bounds.size.z))
                                            po.initiateRipple_setBoolParameterValueByName("zAxis", true);

                                        break;

                                    case "LinearRepeater_Cell":
                                    case "PlanRepeater_Cell":
                                        po.getParameter("Cell Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    case "LinearRepeater_Span":
                                        po.getParameter("Bay SpanU").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;


                                    case "LinearRepeater":
                                        po.getParameter("RepeaterU").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    case "FloorRepeater":
                                        po.getParameter("Floor Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;
                                    case "RadialRepeater":
                                    case "RadialRepeater_Node":
                                    case "GridRepeater_Node":
                                    case "PlanRepeater_Node":
                                        po.getParameter("Node Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    case "RadialRepeater_Span":
                                    case "GridRepeater_Span":
                                        po.getParameter("Bay SpanU", "SpanU Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    case "GridRepeater_Cell":
                                        po.getParameter("Cell Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;

                                    case "PlanPlacer_Mesh":
                                        po.getParameter("Mesh").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;


                                    case "ShapeRepeater_Plan":
                                        AXEditorUtilities.addNodeToCurrentModel("ShapeRepeater").getParameter("Plan").makeDependentOn(editor.OutputParameterBeingDragged);
                                        break;


                                    default:
                                        AXEditorUtilities.addNodeToCurrentModel(nodeName);
                                        break;
                                }


                                if (editor.OutputParameterBeingDragged.parametricObject != null)
                                {
                                    mostRecentPO = editor.OutputParameterBeingDragged.parametricObject;
                                    //po.rect = editor.OutputParameterBeingDragged.parametricObject.rect;
                                    //po.rect.x += 325;
                                }
                                /*
                                else	
                                {
                                    po.rect.x = (model.focusPointInGraphEditor.x)+100;// + UnityEngine.Random.Range(-100, 300);
                                    po.rect.y = (model.focusPointInGraphEditor.y - 250) + UnityEngine.Random.Range(-10, 0);
                                }
                                */


                            }



                            // NO DRAGGING - CONNECT ALL MULTI_SELECTED
                            else if (selectedPOs != null && selectedPOs.Count > 0)
                            {
                                switch (nodeName)
                                {
                                    case "ShapeMerger":
                                        AXShape shp = po.generator.getInputShape();
                                        for (int j = 0; j < selectedPOs.Count; j++)
                                        {
                                            AXParametricObject poo = selectedPOs[j];
                                            if (j == 0)
                                                po.intValue("Axis", selectedPOs[j].intValue("Axis"));
                                            max_x = Mathf.Max(max_x, poo.rect.x);
                                            if (poo.is2D())
                                            {
                                                AXParameter out_p = poo.generator.getPreferredOutputParameter();
                                                if (out_p != null)
                                                    shp.addInput().makeDependentOn(out_p);
                                            }
                                        }

                                        po.rect.x = max_x + 250;
                                        break;

                                    case "Grouper":
                                        //Debug.Log("selectedPOs="+selectedPOs.Count);

                                        //if (model.currentWorkingGroupPO != null && ! selectedPOs.Contains(model.currentWorkingGroupPO))
                                        //{
                                       
                                            if (EditorUtility.DisplayDialog("Creating Grouper. Do you want to add the " + selectedPOs.Count + " currently selected node"+ (selectedPOs.Count==1 ? "" : "s") +" to it?",
            "Are you sure you want to add nodes to this Grouper? ", "Yes", "No thanks, just create the Grouper."))
                                            {
                                               
                                                po.addGroupees(selectedPOs);

                                                Rect r = AXUtilities.getBoundaryRectFromPOs(selectedPOs);
                                                po.rect.x = r.center.x - po.rect.width / 2;
                                                po.rect.y = r.center.y - po.rect.height / 2;
                                            }
                                           
                                        


                                        //



                                        break;

                                    case "ShapeChanneler":
                                        foreach (AXParametricObject selpo in selectedPOs)
                                        {
                                            AXParameter inputer = po.addInputSpline();

                                            inputer.makeDependentOn(selpo.generator.P_Output);
                                            po.generator.P_Output.shapeState = inputer.shapeState;
                                        }


                                        Rect sr = AXUtilities.getBoundaryRectFromPOs(selectedPOs);
                                        po.rect.x = sr.center.x - po.rect.width / 2;
                                        po.rect.y = sr.center.y - po.rect.height / 2;
                                        //}
                                        //po.rect.x = max_x+250;
                                        break;

                                    case "Channeler":
                                        //Debug.Log("selectedPOs="+selectedPOs.Count);

                                        //if (model.currentWorkingGroupPO != null && ! selectedPOs.Contains(model.currentWorkingGroupPO))
                                        //{
                                        foreach (AXParametricObject selpo in selectedPOs)
                                        {
                                            AXParameter inputer = po.addInputMesh();

                                            inputer.makeDependentOn(selpo.generator.P_Output);

                                        }

                                        Rect cr = AXUtilities.getBoundaryRectFromPOs(selectedPOs);
                                        po.rect.x = cr.center.x - po.rect.width / 2;
                                        po.rect.y = cr.center.y - po.rect.height / 2;
                                        //}
                                        //po.rect.x = max_x+250;
                                        break;
                                }
                            }

                            else
                            {

                                switch (nodeName)
                                {
                                    case "ShapeMerger":
                                        po.assertInputControls();
                                        //po.generator.getInputShape().addInput();
                                        break;

                                }

                            }



                            editor.OutputParameterBeingDragged = null;
                            model.autobuild();

                            po.generator.adjustWorldMatrices();

                            if (mostRecentPO != null)
                            {
                                po.rect = mostRecentPO.rect;
                                po.rect.x += (mostRecentPO.rect.width + 50);
                            }
                            else
                            {
                                po.rect.x = (model.focusPointInGraphEditor.x) + 100;// + UnityEngine.Random.Range(-100, 300);
                                po.rect.y = (model.focusPointInGraphEditor.y - 250) + UnityEngine.Random.Range(-10, 0);
                            }

                            po.rect.height = 700;


                            model.mostRecentlyInstantiatedPO = po;
                            //AXNodeGraphEditorWindow.zoomToRectIfOpen(po.rect);


                            //model.beginPanningToRect(po.rect);

                        }
                    }
                }
            }
            //GUILayout.Label (GUI.tooltip);


            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            /* Not sure why I was doing this - it took up a huge amount of CPU!
             * 

            editor.Repaint();
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv != null)
                sv.Repaint();
            *
            */
        }

    }

}

