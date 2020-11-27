using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VisitedStarCacheMerger
{
    class RecordComparer : IEqualityComparer<Record>
    {
        public bool Equals(Record? x, Record? y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode([DisallowNull] Record obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}