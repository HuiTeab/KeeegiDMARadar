using KeeegiDMARadar.Core;
using KeeegiDMARadar.Models;
using KeeegiDMARadar.Overlay;
using KeeegiDMARadar.Utils;

namespace KeeegiDMARadar
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("    KeeegiDMARadar v1.0.0");
            Console.WriteLine("=================================");
            Console.WriteLine();

            // Load configuration
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "config", "config.json");
            configPath = Path.GetFullPath(configPath);
            var config = ConfigLoader.LoadConfiguration(configPath);

            Console.WriteLine($"Target Process: {config.TargetProcess}");
            Console.WriteLine($"Update Interval: {config.UpdateInterval}ms");
            Console.WriteLine();

            // Initialize components
            var memoryReader = new MemoryReader();
            var entityManager = new EntityManager(memoryReader, config);
            var radarRenderer = new RadarRenderer(config);

            // Attach to process
            Console.WriteLine("Attempting to attach to target process...");
            if (!memoryReader.Attach(config.TargetProcess))
            {
                Console.WriteLine("Failed to attach to process. Running in demo mode...");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Successfully attached to process!");
            }

            Console.WriteLine();
            Console.WriteLine("Starting radar...");
            Thread.Sleep(1000);

            // Main loop
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Update entities
                    entityManager.Update();

                    // Render radar
                    radarRenderer.Render(entityManager.LocalPlayer, entityManager.Entities);

                    // Wait for next update
                    await Task.Delay(config.UpdateInterval, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when user presses Ctrl+C
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Cleanup
                Console.WriteLine();
                Console.WriteLine("Shutting down...");
                memoryReader.Detach();
                Console.WriteLine("Goodbye!");
            }
        }
    }
}
