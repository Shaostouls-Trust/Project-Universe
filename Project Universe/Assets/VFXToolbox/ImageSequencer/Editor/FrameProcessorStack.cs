using System.Collections.Generic;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class FrameProcessorStack
    {
        public ProcessingFrameSequence inputSequence
        {
            get
            {
                return m_InputSequence;
            }
        }

        public ProcessingFrameSequence outputSequence
        {
            get
            {
                if (m_Processors.Count > 0)
                    return m_Processors[m_Processors.Count - 1].OutputSequence;
                else
                    return m_InputSequence;
            }
        }

        public ImageSequencer imageSequencer
        {
            get { return m_ImageSequencer; }
        }

        public List<FrameProcessor> processors
        {
            get
            {
                return m_Processors;
            }
        }

        private List<FrameProcessor> m_Processors;
        private ProcessingFrameSequence m_InputSequence;
        private ImageSequencer m_ImageSequencer;

        public FrameProcessorStack(ProcessingFrameSequence inputSequence, ImageSequencer imageSequencer)
        {
            m_InputSequence = inputSequence;
            m_Processors = new List<FrameProcessor>();
            m_ImageSequencer = imageSequencer;
        }

        public void Dispose()
        {
            foreach(FrameProcessor p in m_Processors)
            {
                p.Dispose();
            }
            m_Processors.Clear();
        }

        public ProcessingFrameSequence GetOutputSequence()
        {
            if(m_Processors.Count > 0)
            {
                return m_Processors[m_Processors.Count - 1].OutputSequence;
            }
            else
            {
                return inputSequence;
            }
        }

        public ProcessingFrameSequence GetInputSequence(FrameProcessor processor)
        {
            int index = m_Processors.IndexOf(processor);

            if (index > 0)
            {
                return m_Processors[index - 1].OutputSequence;
            }
            else
                return m_InputSequence;
        }

        public FrameProcessor GetNextProcessor(FrameProcessor processor)
        {
            int index = m_Processors.IndexOf(processor);
            if(index < m_Processors.Count-1)
            {
                return m_Processors[index + 1];
            }
            return null;
        }

        public void Invalidate(FrameProcessor processor)
        {
            int index = m_Processors.IndexOf(processor);
            if(index != -1)
                m_Processors[index].Invalidate();
        }

        public void InvalidateAll()
        {
            if (m_Processors.Count > 0)
                m_Processors[0].Invalidate();
        }

    }

}
