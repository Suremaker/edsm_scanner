using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisitedStarCacheMerger
{
    class Cache
    {
        private readonly Header _header;
        private readonly Dictionary<long, Record> _records;
        private readonly byte[] _footer;

        private Cache(Header header, Record[] records, byte[] footer)
        {
            _header = header;
            _records = records.ToDictionary(r => r.Id);
            _footer = footer;
        }

        public static Cache Read(BinaryReader input)
        {
            var header = Header.Read(input);
            var records = Enumerable.Range(0, header.Rows).Select(_ => Record.Read(input)).ToArray();
            var footer = input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position));
            return new Cache(header, records, footer);
        }

        public void MergeSystemIds(IEnumerable<long> systemIds)
        {
            var maxDate = _records.Max(r => r.Value.VisitedDate);

            foreach (var systemId in systemIds)
            {
                if (_records.TryGetValue(systemId, out var r))
                    r.VisitedDate = maxDate;
                else
                    _records[systemId] = new Record { Id = systemId, VisitedDate = maxDate, Visits = 1 };
            }

            _header.UpdateRows(_records.Count);
        }

        public void Write(BinaryWriter output)
        {
            _header.Write(output);
            foreach (var record in _records.Values.OrderByDescending(v => v.VisitedDate)) //latest to win
                record.Write(output);
            output.Write(_footer);
        }
    }
}