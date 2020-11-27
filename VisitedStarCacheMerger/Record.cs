using System;
using System.IO;

namespace VisitedStarCacheMerger
{
    class Record
    {
        public long Id { get; set; }
        public int Visits { get; set; }
        public int Reminder { get; set; }

        public void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Visits);
            output.Write(Reminder);
        }

        public static Record Read(BinaryReader input)
        {
            return new Record
            {
                Id = input.ReadInt64(),
                Visits = input.ReadInt32(),
                Reminder = input.ReadInt32()
            };
        }
    }
}