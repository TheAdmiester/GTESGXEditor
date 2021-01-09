using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTESGXEditor.Entities
{
    public class NameChunk
    {
        public uint chunkSize;
        public byte[] unknown;
        public string fileName;

        public NameChunk()
        {
            fileName = "New Sound";
        }
    }
}
