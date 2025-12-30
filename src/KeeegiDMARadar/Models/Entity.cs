namespace KeeegiDMARadar.Models
{
    /// <summary>
    /// Represents an entity in the game world
    /// </summary>
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public int Team { get; set; }
        public bool IsAlive => Health > 0;
        public bool IsValid { get; set; }

        public Entity()
        {
            Name = string.Empty;
            Position = new Vector3();
            IsValid = false;
        }

        public float DistanceTo(Entity other)
        {
            return Position.DistanceTo(other.Position);
        }

        public override string ToString()
        {
            return $"Entity {Id}: {Name} at {Position} (HP: {Health}/{MaxHealth})";
        }
    }
}
