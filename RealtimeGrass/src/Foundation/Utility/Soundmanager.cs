using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using SlimDX;
using SlimDX.XAudio2;
using SlimDX.Multimedia;


namespace RealtimeGrass.Utility
{
    class Soundmanager
    {
        private XAudio2                     m_device;
        private MasteringVoice              m_masteringVoice;
        private bool                        m_active;
        
        public Soundmanager()
        {
            m_device = new XAudio2();
            m_masteringVoice = new MasteringVoice(m_device);
        }

        public void playSingle(string fileName)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(playSingleThread), (object) fileName);            
        }

        private void playSingleThread(object o)
        {
            m_active = true;
            string fileName = o as string;
            var s = System.IO.File.OpenRead(fileName);
            WaveStream stream = new WaveStream(s);
           
            s.Close();

            AudioBuffer buffer = new AudioBuffer();
            buffer.AudioData = stream;
            buffer.AudioBytes = (int) stream.Length;
            buffer.Flags = BufferFlags.EndOfStream;

            SourceVoice sourceVoice = new SourceVoice(m_device, stream.Format);
            sourceVoice.SubmitSourceBuffer(buffer);
            sourceVoice.Start();

            while (m_active && sourceVoice.State.BuffersQueued > 0)
            {
                Thread.Sleep(10);
            }

            // cleanup the voice
            sourceVoice.Stop();
            sourceVoice.ExitLoop();
            sourceVoice.Dispose();
            stream.Dispose();
            buffer.Dispose();
            m_active = false;
        }

        public void playLoop(object o)
        {

        }

        public virtual void Dispose()
        {
            m_active = false;
            m_masteringVoice.Dispose();
            m_device.Dispose();
        }
    }
}
