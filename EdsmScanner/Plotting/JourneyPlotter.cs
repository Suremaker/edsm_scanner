using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EdsmScanner.Plotting
{
    internal class JourneyPlotter
    {
        private readonly JourneySystem[] _systems;
        private List<SystemBucket> _buckets;
        private readonly int _bucketSize;
        private int journeyJumps = 0;

        public JourneyPlotter(IEnumerable<SystemDetails> systems)
        {
            _systems = systems.Select(s => new JourneySystem(s)).ToArray();
            _bucketSize = 30;
        }

        public IEnumerable<SystemDetails> Plot()
        {
            Console.WriteLine($"Plotting journey for {_systems.Length} systems...");
            _buckets = CreateBuckets();
            InitializeDistances();
            PlotJourney();
            var jumps = MaterializeJumps(_systems.OrderBy(s => s.Details.Ref.Distance).First());
            
            if (jumps.Length < _systems.Length)
            {
                Console.Error.WriteLine($"Something went wrong in plotting - generated journey for {jumps.Length} systems out of {_systems.Length}");
                File.WriteAllText("error.txt",string.Join("\n",_systems.Select(s=>s.ToString())));
            }
            return jumps;
        }

        private SystemDetails[] MaterializeJumps(JourneySystem start)
        {
            return Enumerable.Repeat(start.Details, 1).Concat(start.Traverse().Select(x => x.Details)).ToArray();
        }

        private void PlotJourney()
        {
            Console.WriteLine("  Plotting journeys ...");
            SystemDistance? distance;
            while ((distance = GetShortestDistance()) != null) 
                EstablishConnection(distance);

            if (journeyJumps < _systems.Length)
            {
                Console.WriteLine("  Calculating distances for final bucket ...");
                var finalBucket = new SystemBucket(_systems.Where(s => s.Connections.Count < 2));
                finalBucket.InitializeDistances();
                _buckets = new List<SystemBucket> {finalBucket};
            }

            while ((distance = GetShortestDistance()) != null)
                EstablishConnection(distance);
        }

        private void EstablishConnection(SystemDistance distance)
        {
            distance.First.Connections.Add(distance.Second);
            distance.Second.Connections.Add(distance.First);
            journeyJumps++;
        }

        private SystemDistance? GetShortestDistance()
        {
            SystemBucket? bucket = null;
            SystemDistance? minDistance = null;
            var canCloseTheLoop = journeyJumps == _systems.Length - 1;
            for (int i = 0; i < _buckets.Count; ++i)
            {
                var b = _buckets[i];
                var distance = b.GetCurrentDistance(canCloseTheLoop);
                if (distance == null)
                {
                    _buckets.RemoveAt(i--);
                    continue;
                }

                if (minDistance == null || distance.CompareTo(minDistance) < 1)
                {
                    bucket = b;
                    minDistance = distance;
                }
            }
            bucket?.MoveToNextDistance();
            return minDistance;
        }

        private void InitializeDistances()
        {
            Console.WriteLine($"  Calculating distances for {_buckets.Count} buckets ...");
            foreach (var bucket in _buckets)
                bucket.InitializeDistances();
        }

        private List<SystemBucket> CreateBuckets()
        {
            var buckets = new Dictionary<CoordF, SystemBucket>();
            var bucketShift = new CoordF(_bucketSize / 2, _bucketSize / 2, _bucketSize / 2);
            foreach (var system in _systems)
            {
                AddToBucket(system, system.Coords, buckets);
                AddToBucket(system, system.Coords - bucketShift, buckets);
            }

            return buckets.Values.ToList();
        }

        private void AddToBucket(JourneySystem system, CoordF sysCoord, Dictionary<CoordF, SystemBucket> buckets)
        {
            var bucketCoord = new CoordF(
                (int)(sysCoord.X / _bucketSize),
                (int)(sysCoord.Y / _bucketSize),
                (int)(sysCoord.Z / _bucketSize)
                );
            var bucket = buckets.TryGetValue(bucketCoord, out var b)
                ? b
                : buckets[bucketCoord] = new SystemBucket();
            bucket.Add(system);
        }
    }
}