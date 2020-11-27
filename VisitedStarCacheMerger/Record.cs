using System.IO;

namespace VisitedStarCacheMerger
{
    class Record
    {
        public long Id { get; set; }
        public int Visits { get; set; }
        public int VisitedDate { get; set; }

        public void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Visits);
            output.Write(VisitedDate);
        }

        public static Record Read(BinaryReader input)
        {
            return new Record
            {
                Id = input.ReadInt64(),
                Visits = input.ReadInt32(),
                VisitedDate = input.ReadInt32()
            };
        }
    }
}