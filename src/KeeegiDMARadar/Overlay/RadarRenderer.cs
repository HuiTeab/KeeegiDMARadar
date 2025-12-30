using KeeegiDMARadar.Models;

namespace KeeegiDMARadar.Overlay
{
    /// <summary>
    /// Renders a simple text-based radar overlay
    /// </summary>
    public class RadarRenderer
    {
        private readonly Configuration _config;

        public RadarRenderer(Configuration config)
        {
            _config = config;
        }

        /// <summary>
        /// Render entities to console as a simple radar
        /// </summary>
        public void Render(Entity? localPlayer, IReadOnlyList<Entity> entities)
        {
            Console.Clear();
            Console.WriteLine("=== KeeegiDMARadar ===");
            Console.WriteLine($"Process: {_config.TargetProcess}");
            Console.WriteLine($"Radar Range: {_config.Radar.Range}m");
            Console.WriteLine();

            if (localPlayer == null)
            {
                Console.WriteLine("Waiting for local player...");
                return;
            }

            Console.WriteLine($"Local Player: {localPlayer.Name}");
            Console.WriteLine($"Position: {localPlayer.Position}");
            Console.WriteLine($"Health: {localPlayer.Health}/{localPlayer.MaxHealth}");
            Console.WriteLine();

            Console.WriteLine("=== Nearby Entities ===");
            var nearbyEntities = entities
                .Where(e => e.Id != localPlayer.Id && e.IsValid)
                .OrderBy(e => e.DistanceTo(localPlayer))
                .ToList();

            if (nearbyEntities.Count == 0)
            {
                Console.WriteLine("No entities detected.");
            }
            else
            {
                foreach (var entity in nearbyEntities)
                {
                    float distance = entity.DistanceTo(localPlayer);
                    if (distance <= _config.Radar.Range)
                    {
                        string teamIndicator = entity.Team == localPlayer.Team ? "[ALLY]" : "[ENEMY]";
                        string healthBar = GenerateHealthBar(entity.Health, entity.MaxHealth);
                        Console.WriteLine($"{teamIndicator} {entity.Name,-15} | Dist: {distance,6:F1}m | {healthBar}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to exit...");
        }

        private string GenerateHealthBar(float health, float maxHealth)
        {
            if (maxHealth <= 0) return "[----------]";

            int barLength = 10;
            int filledLength = (int)((health / maxHealth) * barLength);
            filledLength = Math.Max(0, Math.Min(barLength, filledLength));

            string filled = new string('█', filledLength);
            string empty = new string('░', barLength - filledLength);
            
            return $"[{filled}{empty}] {health,3:F0}/{maxHealth,3:F0}";
        }
    }
}
