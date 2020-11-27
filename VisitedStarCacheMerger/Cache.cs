using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisitedStarCacheMerger
{
    class Cache
    {
        private readonly Header _header;
        private Record[] _records;
        private readonly byte[] _footer;

        private Cache(Header header, Record[] records, byte[] footer)
        {
            _header = header;
            _records = records;
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
            var reminder = _records.Max(r => r.Reminder);

            var newRecords = systemIds
                .Select(id => new Record { Id = id, Reminder = reminder, Visits = 1 });

            UpdateRecords(_records.Union(newRecords, new RecordComparer()).ToArray());
        }

        private void UpdateRecords(Record[] records)
        {
            _records = records;
            _header.UpdateRows(_records.Length);
        }

        public void Write(BinaryWriter output)
        {
            _header.Write(output);
            foreach (var record in _records)
                record.Write(output);
            output.Write(_footer);
        }
    }
}