using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTESGXEditor.Entities
{
    public class SGXDEntry
    {
        public uint namePointer, dataOffset;
        public ushort fileSize, unknown;
        public NameChunk nameChunk;
        public WaveChunk waveChunk;
        public byte[] audioStream;
        public string audioStreamName;

        public SGXDEntry()
        {
            nameChunk = new NameChunk();
            waveChunk = new WaveChunk();
        }

        public void WriteVAG(string path)
        {
            File.WriteAllBytes(string.Format("{0}.vag", Path.Combine(path, nameChunk.fileName)), audioStream);
        }
    }
}
