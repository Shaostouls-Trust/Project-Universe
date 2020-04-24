using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class ImageSequencer : EditorWindow
    {
        private void ShowAddProcessorMenu(ReorderableList list)
        {
            // Create Menu to add...
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Texture Sheet/Break Flipbook"),false, MenuAddProcessor, "Break Flipbook");
            menu.AddItem(new GUIContent("Texture Sheet/Assemble Flipbook"),false, MenuAddProcessor, "Assemble Flipbook");
            menu.AddItem(new GUIContent("Common/Crop Image"),false, MenuAddProcessor, "Crop");
            menu.AddItem(new GUIContent("Common/Rotate"),false, MenuAddProcessor, "Rotate");
            menu.AddItem(new GUIContent("Common/Resize"),false, MenuAddProcessor, "Resize");
            menu.AddItem(new GUIContent("Color/Remove Background Color"),false, MenuAddProcessor, "Remove Background");
            menu.AddItem(new GUIContent("Color/Fix Borders"),false, MenuAddProcessor, "Fix Borders");
            menu.AddItem(new GUIContent("Color/Premultiply Alpha"),false, MenuAddProcessor, "Premultiply Alpha");
            menu.AddItem(new GUIContent("Color/Color Correction"),false, MenuAddProcessor, "Color Correction");
            menu.AddItem(new GUIContent("Color/Alpha From RGB"),false, MenuAddProcessor, "Alpha From RGB");
            menu.AddItem(new GUIContent("Color/Remap Color"),false, MenuAddProcessor, "Remap Color");
            menu.AddItem(new GUIContent("Temporal/Retime"),false, MenuAddProcessor, "Retime");
            menu.AddItem(new GUIContent("Temporal/Decimate"),false, MenuAddProcessor, "Decimate");
            menu.AddItem(new GUIContent("Temporal/Fade"),false, MenuAddProcessor, "Fade");
            menu.AddItem(new GUIContent("Temporal/Loop Sequence"),false, MenuAddProcessor, "Looping");
            menu.AddItem(new GUIContent("Custom Material"),false, MenuAddProcessor, "Custom Material");
            menu.ShowAsContext();
        }

        private void MenuAddProcessor(object processorName)
        {
            Undo.RecordObject(m_CurrentAsset, "Add Processor : " + processorName);
            string name = (string)processorName;
            FrameProcessor processor = null;
            switch(name)
            {
                case "Break Flipbook":
                    processor = new BreakFlipbookProcessor(m_processorStack, ProcessorInfo.CreateDefault<BreakFilpbookProcessorSettings>(name, true));
                    break;
                case "Crop":
                    processor = new CropProcessor(m_processorStack, ProcessorInfo.CreateDefault<CropProcessorSettings>(name, true));
                    break;
                case "Rotate":
                    processor = new RotateProcessor(m_processorStack, ProcessorInfo.CreateDefault<RotateProcessorSettings>(name, true));
                    break;
                case "Retime":
                    processor = new RetimeProcessor(m_processorStack, ProcessorInfo.CreateDefault<RetimeProcessorSettings>(name, true));
                    break;
                case "Looping":
                    processor = new LoopingProcessor(m_processorStack, ProcessorInfo.CreateDefault<LoopingProcessorSettings>(name, true));
                    break;
                case "Remove Background":
                    processor = new RemoveBackgroundBlendingProcessor(m_processorStack, ProcessorInfo.CreateDefault<RemoveBackgroundSettings>(name, true));
                    break;
                case "Decimate":
                    processor = new DecimateProcessor(m_processorStack, ProcessorInfo.CreateDefault<DecimateProcessorSettings>(name, true));
                    break;
                case "Fix Borders":
                    processor = new FixBordersProcessor(m_processorStack, ProcessorInfo.CreateDefault<FixBordersProcessorSettings>(name, true));
                    break;
                case "Fade":
                    processor = new FadeProcessor(m_processorStack, ProcessorInfo.CreateDefault<FadeProcessorSettings>(name, true));
                    break;
                case "Assemble Flipbook":
                    processor = new AssembleProcessor(m_processorStack, ProcessorInfo.CreateDefault<AssembleProcessorSettings>(name, true));
                    break;
                case "Premultiply Alpha":
                    processor = new PremultiplyAlphaProcessor(m_processorStack, ProcessorInfo.CreateDefault<PremultiplyAlphaProcessorSettings>(name, true));
                    break;
                case "Color Correction":
                    processor = new ColorCorrectionProcessor(m_processorStack, ProcessorInfo.CreateDefault<ColorCorrectionProcessorSettings>(name, true));
                    break;
                case "Alpha From RGB":
                    processor = new AlphaFromRGBProcessor(m_processorStack, ProcessorInfo.CreateDefault<AlphaFromRGBProcessorSettings>(name, true));
                    break;
                case "Remap Color":
                    processor = new RemapColorProcessor(m_processorStack, ProcessorInfo.CreateDefault<RemapColorProcessorSettings>(name, true));
                    break;
                case "Resize":
                    processor = new ResizeProcessor(m_processorStack, ProcessorInfo.CreateDefault<ResizeProcessorSettings>(name, true));
                    break;
                case "Custom Material":
                    processor = new CustomMaterialProcessor(m_processorStack, ProcessorInfo.CreateDefault<CustomMaterialProcessorSettings>(name, true));
                    break;
                default: break;
            }

            if(processor != null)
            {
                m_processorStack.AddProcessor(processor, m_CurrentAsset);
                m_processorStack.InvalidateAll();
                UpdateViewport();
            }
        }

        private void MenuSelectProcessor(ReorderableList list)
        {
            if (m_CurrentAsset.editSettings.selectedProcessor == list.index)
                return;

            if (list.count > 0  && list.index != -1)
            {
                SetCurrentFrameProcessor(m_processorStack.processors[list.index], false);
            }
            else
                SetCurrentFrameProcessor(null, false);
        }

        private void ReorderProcessor(ReorderableList list)
        {
            Undo.RecordObject(m_CurrentAsset, "Reorder Processors");
            m_processorStack.ReorderProcessors(m_CurrentAsset);
            m_processorStack.InvalidateAll();
            UpdateViewport();

            // If locked processor is present, update its index
            if(m_LockedPreviewProcessor != null)
            {
                m_CurrentAsset.editSettings.lockedProcessor = m_processorStack.processors.IndexOf(m_LockedPreviewProcessor);
                EditorUtility.SetDirty(m_CurrentAsset);
                AssetDatabase.SaveAssets();
            }

        }

        private void MenuRemoveProcessor(ReorderableList list)
        {
            int idx = list.index;

            Undo.RecordObject(m_CurrentAsset, "Remove Processor : " + m_processorStack.processors[idx].GetName());
            m_processorStack.RemoveProcessor(idx,m_CurrentAsset);

            // If was locked, unlock beforehand
            if (idx == m_CurrentAsset.editSettings.lockedProcessor)
                SetCurrentFrameProcessor(null, true);
            else if (idx < m_CurrentAsset.editSettings.lockedProcessor)
                m_CurrentAsset.editSettings.lockedProcessor--;

            if(m_processorStack.processors.Count > 0)
            {
                int newIdx = Mathf.Clamp(idx - 1, 0, m_processorStack.processors.Count - 1);

                SetCurrentFrameProcessor(m_processorStack.processors[newIdx], false);
                list.index = newIdx;
            }
            else
            {
                SetCurrentFrameProcessor(null, false);
                list.index = -1;
            }

            previewCanvas.currentFrameIndex = 0;
            m_processorStack.InvalidateAll();
            UpdateViewport();
        }

        public void RefreshCanvas()
        {
            if(m_CurrentProcessor != null)
                previewCanvas.sequence = m_CurrentProcessor.OutputSequence;
            else
                previewCanvas.sequence = m_processorStack.inputSequence;

            previewCanvas.currentFrameIndex = Mathf.Clamp(previewCanvas.currentFrameIndex, 0, previewCanvas.sequence.length - 1);

            UpdateViewport();
            Invalidate();
        }

        public void SetCurrentFrameProcessor(FrameProcessor processor, bool wantLock)
        {
            if(wantLock)
            {
                m_LockedPreviewProcessor = processor;
                if(processor != null)
                {
                    Undo.RecordObject(m_CurrentAsset, "Lock Processor");
                    m_CurrentProcessor = processor;
                    m_CurrentAsset.editSettings.lockedProcessor = m_processorStack.processors.IndexOf(processor);
                }
                else
                {
                    Undo.RecordObject(m_CurrentAsset, "Unlock Processor");
                    if(m_ProcessorsReorderableList.index != -1)
                        m_CurrentProcessor = m_processorStack.processors[Mathf.Min(m_ProcessorsReorderableList.index, m_processorStack.processors.Count-1)];
                    m_CurrentAsset.editSettings.lockedProcessor = -1;
                }
            }
            else
            {
                bool needChange = (m_CurrentProcessor != processor);

                if(needChange)
                    Undo.RecordObject(m_CurrentAsset, "Select Processor");

                if(m_LockedPreviewProcessor == null)
                    m_CurrentProcessor = processor;
                else
                    m_CurrentProcessor = m_LockedPreviewProcessor;
            }

            m_CurrentAsset.editSettings.selectedProcessor = m_processorStack.processors.IndexOf(processor);
            RefreshCanvas();
            EditorUtility.SetDirty(m_CurrentAsset);
        }

        public void DrawRListPreviewProcessorElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect toggle_rect = new Rect(rect.x + 4, rect.y, 16, rect.height);
            Rect label_rect = new Rect(rect.x + 24, rect.y, rect.width - 24, rect.height);

            using (new EditorGUI.DisabledScope(true))
            {
                GUI.Toggle(toggle_rect, m_processorStack.processors[index].Enabled, "");
            }

            GUI.Label( label_rect, string.Format("#{0} - {1} ",index+1, m_processorStack.processors[index].ToString()), VFXToolboxStyles.RListLabel);
        }


        public void DrawRListProcessorElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect toggle_rect = new Rect(rect.x + 4, rect.y, 16, rect.height);
            Rect label_rect = new Rect(rect.x + 24, rect.y, rect.width - 24, rect.height);
            Rect view_rect = new Rect(rect.x + rect.width - 37, rect.y, 19, 18);
            Rect lock_rect = new Rect(rect.x + rect.width - 16, rect.y+1, 16, 14);

            bool enabled = GUI.Toggle(toggle_rect, m_processorStack.processors[index].Enabled,"");
            if(enabled != m_processorStack.processors[index].Enabled)
            {
                m_processorStack.processors[index].Enabled = enabled;
                m_processorStack.processors[index].Invalidate();
                RefreshCanvas();
            }

            GUI.Label( label_rect, string.Format("#{0} - {1} ",index+1, m_processorStack.processors[index].ToString()), VFXToolboxStyles.RListLabel);

            if((m_LockedPreviewProcessor == null && isActive) || m_processorStack.processors.IndexOf(m_LockedPreviewProcessor) == index)
                GUI.DrawTexture(view_rect, (Texture2D)EditorGUIUtility.LoadRequired("ViewToolOrbit On.png"));

            bool locked = (m_LockedPreviewProcessor != null) && index == m_processorStack.processors.IndexOf(m_LockedPreviewProcessor);

            if(isActive || locked)
            {
                bool b = GUI.Toggle(lock_rect, locked,"", styles.LockToggle);
                if(b != locked)
                {
                    if(b)
                        SetCurrentFrameProcessor(m_processorStack.processors[index],true);
                    else
                        SetCurrentFrameProcessor(null, true);
                }
            }
        }
    }
}
