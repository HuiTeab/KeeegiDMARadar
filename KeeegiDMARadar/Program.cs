global using System.Diagnostics;
global using System.Reflection;
global using System.Runtime.InteropServices;
using KeeegiDMARadar.UI.Misc;
using KeeegiDMARadar.UI.Skia;
using KeeegiDMARadar.UI;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Input.Glfw;
using Silk.NET.Windowing.Glfw;
using System;
using Velopack;
using Velopack.Sources;

namespace KeeegiDMARadar
{
    internal partial class Program
    {
        private const string BaseName = "Keeegi DMA Radar";
        private const string MUTEX_ID = "0f908ff7-e614-6a93-60a3-cee36c9cea91";
        private static readonly Mutex _mutex;

        internal static string Name { get; } = $"{BaseName} v{GetSemVer2OrDefault()}";

        public static DirectoryInfo ConfigPath { get; } =
            new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keeegi-DMA"));

        public static ARCDMAConfig Config { get; }
        public static bool IsInstalled { get; }
        public static IServiceProvider ServiceProvider { get; }
        public static IHttpClientFactory HttpClientFactory { get; }

        static Program()
        {
            GlfwWindowing.RegisterPlatform();
            GlfwInput.RegisterPlatform();
            VelopackApp.Build().Run();

            try
            {
                IsInstalled = new UpdateManager(".").IsInstalled;

                _mutex = new Mutex(true, MUTEX_ID, out bool singleton);
                if (!singleton)
                    throw new InvalidOperationException("The application is already running.");

                ServiceProvider = BuildServiceProvider();
                HttpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();

                Config = ARCDMAConfig.Load();

                SetHighPerformanceMode();

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            }
            catch (Exception ex)
            {
                // TODO: Replace with your own UI/messagebox once available.
                Console.Error.WriteLine(ex);
                throw;
            }
        }

        private static void Main()
        {
            try
            {
                // Show loading window during initialization
                using var loadingWindow = new LoadingWindow();
                loadingWindow.Show();

                // Run initialization on a background thread while loading window pumps messages on main thread
                var initTask = Task.Run(() => ConfigureProgramAsync(loadingWindow));

                // Keep the loading window responsive until initialization completes
                while (!initTask.IsCompleted)
                {
                    loadingWindow.DoEvents();
                    Thread.Sleep(16); // ~60fps
                }

                // Close loading window
                loadingWindow.Close();

                // Check if initialization failed
                if (initTask.IsFaulted)
                {
                    throw initTask.Exception!.InnerException ?? initTask.Exception;
                }

                // Now start the radar window (this blocks until window closes)
                RadarWindow.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Name, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxOptions.DefaultDesktopOnly);
                throw;
            }
        }

        #region Boilerplate

        private static async Task ConfigureProgramAsync(LoadingWindow loadingWindow)
        {
            loadingWindow.UpdateProgress(10, "Loading, Please Wait...");

            //_ = Task.Run(CheckForUpdatesAsync); // Run continuations on the thread pool

            //var tarkovDataManager = TarkovDataManager.ModuleInitAsync();
            //var eftMapManager = EftMapManager.ModuleInitAsync();
            var memoryInterface = Memory.ModuleInitAsync();

            var misc = Task.Run(() =>
            {
                SKPaints.PaintBitmap.ColorFilter = SKPaints.GetDarkModeColorFilter(0.7f);
                SKPaints.PaintBitmapAlpha.ColorFilter = SKPaints.GetDarkModeColorFilter(0.7f);
            });

            //// Wait for all tasks
            await Task.WhenAll(misc);

            loadingWindow.UpdateProgress(100, "Loading Completed!");
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e) => OnShutdown();

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                // TODO: Replace with proper logging.
                Console.Error.WriteLine($"*** UNHANDLED EXCEPTION (Terminating: {e.IsTerminating}): {ex}");
            }

            if (e.IsTerminating)
            {
                OnShutdown();
            }
        }

        private static void OnShutdown()
        {
            // TODO: close Keeegi DMA / save config if needed.
            Console.WriteLine("Exiting...");
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();

            // TODO: register Keeegi-specific services/APIs here.

            return services.BuildServiceProvider();
        }

        private static void SetHighPerformanceMode()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            if (SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED) == 0)
                Console.Error.WriteLine($"WARNING: Unable to set Thread Execution State. ERROR {Marshal.GetLastWin32Error()}");

            Guid highPerformanceGuid = new("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            if (PowerSetActiveScheme(IntPtr.Zero, ref highPerformanceGuid) != 0)
                Console.Error.WriteLine($"WARNING: Unable to set High Performance Power Plan. ERROR {Marshal.GetLastWin32Error()}");

            if (TimeBeginPeriod(5) != 0)
                Console.Error.WriteLine($"WARNING: Unable to set timer resolution to 5ms. ERROR {Marshal.GetLastWin32Error()}");

            if (AvSetMmThreadCharacteristicsW("Games", out _) == 0)
                Console.Error.WriteLine($"WARNING: Unable to set Multimedia thread characteristics to 'Games'. ERROR {Marshal.GetLastWin32Error()}");
        }

        private static async Task CheckForUpdatesAsync()
        {
            try
            {
                if (!IsInstalled)
                    return;

                var updater = new UpdateManager(
                    source: new GithubSource(
                        repoUrl: "https://github.com/your-org/Keeegi-DMA-Radar",
                        accessToken: null,
                        prerelease: false));

                var newVersion = await updater.CheckForUpdatesAsync();
                if (newVersion is not null)
                {
                    // TODO: hook into your future UI layer instead of Console.
                    Console.WriteLine($"New version available: {newVersion.TargetFullRelease.Version}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Update check failed: {ex}");
            }
        }

        private static string GetSemVer2OrDefault()
        {
            try
            {
                string? strV = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyFileVersionAttribute>()
                    ?.Version;

                if (string.IsNullOrWhiteSpace(strV))
                    return "0.0.0";

                var v = new Version(strV);
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
            catch
            {
                return "0.0.0";
            }
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [LibraryImport("avrt.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial IntPtr AvSetMmThreadCharacteristicsW(string taskName, out uint taskIndex);

        [LibraryImport("powrprof.dll", SetLastError = true)]
        private static partial uint PowerSetActiveScheme(IntPtr userRootPowerKey, ref Guid schemeGuid);

        [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static partial uint TimeBeginPeriod(uint uMilliseconds);

        #endregion
    }
}