using KeeegiDMARadar.Models;

namespace KeeegiDMARadar.Core
{
    /// <summary>
    /// Manages entity scanning and tracking
    /// </summary>
    public class EntityManager
    {
        private readonly IMemoryReader _memoryReader;
        private readonly Configuration _config;
        private readonly List<Entity> _entities;
        private Entity? _localPlayer;

        public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
        public Entity? LocalPlayer => _localPlayer;

        public EntityManager(IMemoryReader memoryReader, Configuration config)
        {
            _memoryReader = memoryReader;
            _config = config;
            _entities = new List<Entity>();
        }

        /// <summary>
        /// Update entity list from game memory
        /// </summary>
        public void Update()
        {
            if (!_memoryReader.IsAttached)
            {
                return;
            }

            try
            {
                _entities.Clear();

                // In a real implementation, this would:
                // 1. Read the entity list pointer from base address + offset
                // 2. Iterate through entity list
                // 3. Read entity data (position, health, team, etc.)
                // 4. Update local player reference

                // Stub: Create some dummy entities for demonstration
                for (int i = 0; i < 5; i++)
                {
                    var entity = new Entity
                    {
                        Id = i,
                        Name = $"Entity_{i}",
                        Position = new Vector3(i * 10, i * 5, 0),
                        Health = 100,
                        MaxHealth = 100,
                        Team = i % 2,
                        IsValid = true
                    };
                    _entities.Add(entity);
                }

                // Set first entity as local player
                if (_entities.Count > 0)
                {
                    _localPlayer = _entities[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EntityManager] Error updating entities: {ex.Message}");
            }
        }

        /// <summary>
        /// Get entities within a certain distance from the local player
        /// </summary>
        public List<Entity> GetNearbyEntities(float maxDistance)
        {
            if (_localPlayer == null)
            {
                return new List<Entity>();
            }

            return _entities
                .Where(e => e.Id != _localPlayer.Id && e.IsAlive && e.IsValid)
                .Where(e => e.DistanceTo(_localPlayer) <= maxDistance)
                .ToList();
        }
    }
}
