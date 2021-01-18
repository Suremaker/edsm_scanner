using System;
using System.Text.Json.Serialization;

namespace EdsmScanner
{
    internal struct CoordF
    {
        [JsonConstructor]
        public CoordF(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public override string ToString() => $"[{X},{Y},{Z}]";
        public static CoordF operator +(CoordF a, CoordF b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static CoordF operator -(CoordF a, CoordF b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public bool Equals(CoordF other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object? obj) => obj is CoordF other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public double Distance(CoordF b)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;
            float dz = Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}