using System;
using System.IO;

namespace VisitedStarCacheMerger
{
    class Header
    {
        private readonly byte[] _bytes;
        public int Rows { get; private set; }


        private Header(byte[] bytes)
        {
            _bytes = bytes;
            Rows = BitConverter.ToInt32(_bytes.AsSpan(24, 4));
        }

        public void UpdateRows(int count)
        {
            Rows = count;
            if (!BitConverter.TryWriteBytes(_bytes.AsSpan(24, 4), count))
                throw new InvalidOperationException();
        }

        public static Header Read(BinaryReader input) => new Header(input.ReadBytes(48));

        public void Write(BinaryWriter output) => output.Write(_bytes);
    }
}