namespace KeeegiDMARadar.Models
{
    /// <summary>
    /// Represents a 2D position in the game world
    /// </summary>
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2() : this(0, 0) { }

        /// <summary>
        /// Calculate distance to another position
        /// </summary>
        public float DistanceTo(Vector2 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }
    }

    /// <summary>
    /// Represents a 3D position in the game world
    /// </summary>
    public class Vector3 : Vector2
    {
        public float Z { get; set; }

        public Vector3(float x, float y, float z) : base(x, y)
        {
            Z = z;
        }

        public Vector3() : this(0, 0, 0) { }

        /// <summary>
        /// Calculate 3D distance to another position
        /// </summary>
        public float DistanceTo(Vector3 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
    }
}
