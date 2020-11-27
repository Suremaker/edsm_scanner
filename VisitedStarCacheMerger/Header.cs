using System;
using System.IO;

namespace VisitedStarCacheMerger
{
    class Header
    {
        private const int StaticRows = 4;
        private readonly byte[] _bytes;
        public int Rows { get; private set; }


        private Header(byte[] bytes)
        {
            _bytes = bytes;
            Rows = BitConverter.ToInt32(_bytes.AsSpan(24, 4)) - StaticRows;
        }

        public void UpdateRows(int count)
        {
            Rows = count;
            if (!BitConverter.TryWriteBytes(_bytes.AsSpan(24, 4), count + StaticRows))
                throw new InvalidOperationException();
        }

        public static Header Read(BinaryReader input) => new Header(input.ReadBytes(48));

        public void Write(BinaryWriter output) => output.Write(_bytes);
    }
}