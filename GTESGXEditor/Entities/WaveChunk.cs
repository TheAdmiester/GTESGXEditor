using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTESGXEditor.Entities
{
    public class WaveChunk
    {
        public uint chunkSize, soundAmount, flag2, nameOffset, soundSampleRate, bitRate, loopStartSample, loopEndSample, streamSize;
        public ushort volumeL, volumeR;

        public byte codecType, channels;
    }
}
