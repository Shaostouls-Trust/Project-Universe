using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class FrameProcessorStack
    {
        public void AddSettingsObjectToAsset(ImageSequence asset, ScriptableObject settings)
        {
                AssetDatabase.AddObjectToAsset(settings,asset);
                settings.hideFlags = HideFlags.HideInHierarchy;
        }

        public void AddProcessorInfoObjectToAsset(ImageSequence asset, ProcessorInfo info)
        {
                AssetDatabase.AddObjectToAsset(info,asset);
                info.hideFlags = HideFlags.HideInHierarchy;
        }

        public void RemoveAllInputFrames(ImageSequence asset)
        {
            Undo.RecordObject(asset, "Remove All Frames");
            asset.inputFrameGUIDs.Clear();
            m_InputSequence.frames.Clear();

            EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void LoadFramesFromAsset(ImageSequence asset)
        {
            inputSequence.frames.Clear();
            if (asset.inputFrameGUIDs != null && asset.inputFrameGUIDs.Count > 0)
            {
                int count = asset.inputFrameGUIDs.Count;
                int i = 1;
                foreach (string guid in asset.inputFrameGUIDs)
                {
                    VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Loading Textures (" + i + "/" + count + ")", (float)i/count, 0.1f);
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (t != null)
                    {
                        inputSequence.frames.Add(new ProcessingFrame(t));
                    }
                    else
                    {
                        inputSequence.frames.Add(ProcessingFrame.Missing);
                    }
                    i++;
                }
                VFXToolboxGUIUtility.ClearProgressBar();
            }
        }

        public void SyncFramesToAsset(ImageSequence asset)
        {
            asset.inputFrameGUIDs.Clear();
            foreach(ProcessingFrame f in inputSequence.frames)
            {
                asset.inputFrameGUIDs.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(f.texture)));
            }
            EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void AddProcessor(FrameProcessor processor, ImageSequence asset)
        {
            AddProcessorInfoObjectToAsset(asset, processor.ProcessorInfo);
            asset.processorInfos.Add(processor.ProcessorInfo);

            ProcessorSettingsBase settings = processor.GetSettingsAbstract();
            if (settings != null)
            {
                AddSettingsObjectToAsset(asset, settings);
                processor.ProcessorInfo.Settings = settings;
            }
            m_Processors.Add(processor);

            EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void RemoveAllProcessors(ImageSequence asset)
        {
            asset.processorInfos.Clear();
            m_Processors.Clear();

            EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void RemoveProcessor(int index, ImageSequence asset)
        {
            asset.processorInfos.RemoveAt(index);
            m_Processors.RemoveAt(index);

            EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void ReorderProcessors(ImageSequence asset)
        {
            if(m_Processors.Count > 0)
            {
                List<FrameProcessor> old = new List<FrameProcessor>();
                foreach(FrameProcessor p in m_Processors)
                {
                    old.Add(p);
                }

                m_Processors.Clear();
                foreach(ProcessorInfo info in asset.processorInfos)
                {
                    foreach(FrameProcessor p in old)
                    {
                        if(p.ProcessorInfo.Equals(info))
                        {
                            m_Processors.Add(p);
                            break;
                        }
                    }
                }
                EditorUtility.SetDirty(asset);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

        public void LoadProcessorsFromAsset(ImageSequence asset)
        {
            m_Processors.Clear();

            var infos = asset.processorInfos;

            // Creating Runtime
            foreach(ProcessorInfo procInfo in infos)
            {
                switch(procInfo.ProcessorName)
                {
                    case "Crop":
                        m_Processors.Add(new CropProcessor(this, procInfo));
                        break;
                    case "Rotate":
                        m_Processors.Add(new RotateProcessor(this, procInfo));
                        break;
                    case "Retime":
                        m_Processors.Add(new RetimeProcessor(this, procInfo));
                        break;
                    case "Looping":
                        m_Processors.Add(new LoopingProcessor(this, procInfo));
                        break;
                    case "Decimate":
                        m_Processors.Add(new DecimateProcessor(this, procInfo));
                        break;
                    case "Remove Background":
                        m_Processors.Add(new RemoveBackgroundBlendingProcessor(this, procInfo));
                        break;
                    case "Fade":
                        m_Processors.Add(new FadeProcessor(this, procInfo));
                        break;
                     case "Assemble Flipbook":
                        m_Processors.Add(new AssembleProcessor(this, procInfo));
                        break;
                     case "Premultiply Alpha":
                        m_Processors.Add(new PremultiplyAlphaProcessor(this, procInfo));
                        break;
                     case "Color Correction":
                        m_Processors.Add(new ColorCorrectionProcessor(this, procInfo));
                        break;
                     case "Alpha From RGB":
                        m_Processors.Add(new AlphaFromRGBProcessor(this, procInfo));
                        break;
                     case "Remap Color":
                        m_Processors.Add(new RemapColorProcessor(this, procInfo));
                        break;
                     case "Fix Borders":
                        m_Processors.Add(new FixBordersProcessor(this, procInfo));
                        break;
                     case "Resize":
                        m_Processors.Add(new ResizeProcessor(this, procInfo));
                        break;
                      case "Break Flipbook":
                        m_Processors.Add(new BreakFlipbookProcessor(this, procInfo));
                        break;
                      case "Custom Material":
                        m_Processors.Add(new CustomMaterialProcessor(this, procInfo));
                        break;
                    default:
                        Debug.LogError("Could Not deserizlize " + procInfo + " , Unrecognized processor;");
                        break;
                }
            }
        }


    }
}
