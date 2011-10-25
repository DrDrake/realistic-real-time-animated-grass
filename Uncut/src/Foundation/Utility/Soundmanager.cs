using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using SlimDX;
using SlimDX.XAudio2;
using SlimDX.Multimedia;


namespace Uncut.Utility
{
    class Soundmanager
    {
        XAudio2             m_device;
        MasteringVoice      m_masteringVoice;
        Thread              m_playThread;

        public Soundmanager()
        {
            m_device = new XAudio2();
            m_masteringVoice = new MasteringVoice(m_device);
            //m_playThread = new Thread(this.playSingle);
        }

        ~Soundmanager()
        {
            m_masteringVoice.Dispose();
            m_device.Dispose();
        }

        public void playSingle(string fileName)
        {
            //WaveStream stream = new WaveStream(fileName);
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

            while (sourceVoice.State.BuffersQueued > 0)
            {
                Thread.Sleep(10);
            }

            // cleanup the voice
            buffer.Dispose();
            sourceVoice.Dispose();
            stream.Dispose();
        }

        public void playLoop(string filename)
        {

        }

    }
}
