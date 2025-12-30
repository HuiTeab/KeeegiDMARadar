using ImGuiNET;
using KeeegiDMARadar.UI.Skia;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;
using System.Numerics;

namespace KeeegiDMARadar.UI
{
    internal static partial class RadarWindow
    {
        #region Fields

        private static IWindow _window = null!;
        private static GL _gl = null!;
        private static IInputContext _input = null!;

        private static ImGuiController _imgui = null!;
        private static SKSurface _skSurface = null!;
        private static GRContext _grContext = null!;
        private static GRBackendRenderTarget _skBackendRenderTarget = null!;
        private static readonly PeriodicTimer _fpsTimer = new(TimeSpan.FromSeconds(1));
        private static int _fpsCounter;
        private static int _statusOrder = 1;

        // Mouse tracking skeleton
        private static bool _mouseDown;
        private static Vector2 _lastMousePosition;

        private static int _fps;

        #endregion

        #region Static Properties

        public static IntPtr Handle => _window?.Native?.Win32?.Hwnd ?? IntPtr.Zero;

        #endregion

        #region Initialization

        internal static void Initialize()
        {
            var options = WindowOptions.Default;

            // TODO: replace with Keeegi config once available
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = Program.Name;
            options.VSync = false;
            options.FramesPerSecond = 144;
            options.PreferredStencilBufferBits = 8;
            options.PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8);

            GlfwWindowing.Use();
            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
            _window.StateChanged += OnStateChanged;

            // Start FPS timer
            _ = RunFpsTimerAsync();

            _window.Run();
        }

        private static void OnLoad()
        {
            _gl = GL.GetApi(_window);

            // Apply dark mode and window icon (Windows only)
            if (_window.Native?.Win32 is { } win32)
            {
                EnableDarkMode(win32.Hwnd);
                SetWindowIcon(win32.Hwnd);
            }

            // Create input context FIRST (before ImGuiController to share it)
            _input = _window.CreateInput();

            // --- Skia GPU context ---
            var glInterface = GRGlInterface.Create(name =>
                _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0);

            _grContext = GRContext.CreateGl(glInterface);
            _grContext.SetResourceCacheLimit(512 * 1024 * 1024); // 512 MB

            CreateSkiaSurface();

            // --- ImGui ---
            ImGui.CreateContext();

            _imgui = new ImGuiController(
                gl: _gl,
                view: _window,
                input: _input
            );

            // Basic dark theme
            ImGui.StyleColorsDark();

            // Mouse events
            foreach (var mouse in _input.Mice)
            {
                mouse.MouseDown += OnMouseDown;
                mouse.MouseUp += OnMouseUp;
                mouse.MouseMove += OnMouseMove;
                mouse.Scroll += OnMouseScroll;
            }

            // Keyboard skeleton
            foreach (var keyboard in _input.Keyboards)
            {
                keyboard.KeyDown += OnKeyDown;
            }
        }

        private static void CreateSkiaSurface()
        {
            _skSurface?.Dispose();
            _skSurface = null!;
            _skBackendRenderTarget?.Dispose();
            _skBackendRenderTarget = null!;

            var size = _window.FramebufferSize;
            if (size.X <= 0 || size.Y <= 0 || _grContext is null)
            {
                _skSurface = null!;
                _skBackendRenderTarget = null!;
                return;
            }

            const GetPName SampleBuffersPName = (GetPName)0x80A8; // GL_SAMPLE_BUFFERS
            const GetPName SamplesPName = (GetPName)0x80A9;       // GL_SAMPLES
            const GetPName StencilBitsPName = (GetPName)0x0D57;   // GL_STENCIL_BITS

            _gl.GetInteger(SampleBuffersPName, out int sampleBuffers);
            _gl.GetInteger(SamplesPName, out int samples);
            if (sampleBuffers == 0)
                samples = 0;
            _gl.GetInteger(StencilBitsPName, out int stencilBits);

            var fbInfo = new GRGlFramebufferInfo(
                0, // default framebuffer
                (uint)InternalFormat.Rgba8
            );

            _skBackendRenderTarget = new GRBackendRenderTarget(
                size.X,
                size.Y,
                samples,
                stencilBits,
                fbInfo
            );

            _skSurface = SKSurface.Create(
                _grContext,
                _skBackendRenderTarget,
                GRSurfaceOrigin.BottomLeft,
                SKColorType.Rgba8888
            );
        }

        private static void OnResize(Vector2D<int> size)
        {
            _gl.Viewport(size);
            CreateSkiaSurface();
        }

        private static void OnStateChanged(WindowState state)
        {
            // TODO: persist Keeegi window state once you have a config type
        }

        private static void OnClosing()
        {
            // TODO: save window size/state to Keeegi config if needed
        }

        #endregion

        #region Render Loop

        private static readonly KeeegiDMARadar.Misc.RateLimiter _purgeRL =
            new(TimeSpan.FromSeconds(1));

        private static void OnRender(double delta)
        {
            if (_grContext is null || _skSurface is null)
                return;

            try
            {
                Interlocked.Increment(ref _fpsCounter);
                _grContext.ResetContext();
                if (_purgeRL.TryEnter())
                {
                    _grContext.PurgeUnlockedResources(false);
                }

                var fbSize = _window.FramebufferSize;

                // Scene Render (Skia)
                DrawScene(ref fbSize);

                // UI Render (ImGui)
                DrawImGuiUI(ref fbSize, delta);
            }
            catch (Exception ex)
            {
                // TODO: replace with your logging once available
                Console.Error.WriteLine($"***** CRITICAL RENDER ERROR: {ex}");
            }
        }

        private static void DrawScene(ref Vector2D<int> fbSize)
        {
            _gl.Viewport(0, 0, (uint)fbSize.X, (uint)fbSize.Y);
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _gl.ClearColor(0f, 0f, 0f, 1f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);

            var canvas = _skSurface.Canvas;
            try
            {
                // TODO: draw Keeegi radar content here with Skia
                canvas.Clear(SKColors.Black);
            }
            finally
            {
                canvas.Flush();
                _grContext.Flush();
            }
        }

        private static void DrawImGuiUI(ref Vector2D<int> fbSize, double delta)
        {
            _gl.Viewport(0, 0, (uint)fbSize.X, (uint)fbSize.Y);
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _imgui.Update((float)delta);
            try
            {
                if (ImGui.BeginMainMenuBar())
                {
                    ImGui.Text($"Keeegi DMA Radar | {_fps} FPS");
                    ImGui.EndMainMenuBar();
                }

                // TODO: add Keeegi panels/windows here
            }
            finally
            {
                _imgui.Render();
            }
        }

        private static async Task RunFpsTimerAsync()
        {
            while (await _fpsTimer.WaitForNextTickAsync())
            {
                _statusOrder = (_statusOrder >= 3) ? 1 : _statusOrder + 1;
                _fps = Interlocked.Exchange(ref _fpsCounter, 0);
            }
        }

        #endregion

        #region Input Handling

        private static void OnMouseDown(IMouse mouse, MouseButton button)
        {
            if (ImGui.GetIO().WantCaptureMouse)
                return;

            var pos = mouse.Position;
            var mousePos = new Vector2(pos.X, pos.Y);

            if (button == MouseButton.Left)
            {
                _lastMousePosition = mousePos;
                _mouseDown = true;
            }
        }

        private static void OnMouseUp(IMouse mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                _mouseDown = false;
            }
        }

        private static void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (ImGui.GetIO().WantCaptureMouse)
            {
                _mouseDown = false;
                return;
            }

            var mousePos = new Vector2(position.X, position.Y);

            if (_mouseDown)
            {
                // TODO: implement Keeegi map panning here later
                _lastMousePosition = mousePos;
            }
            else
            {
                // TODO: implement Keeegi mouseover logic here later
            }
        }

        private static void OnMouseScroll(IMouse mouse, ScrollWheel wheel)
        {
            if (!ImGui.GetIO().WantCaptureMouse)
            {
                // TODO: implement Keeegi zoom handling here later
            }
        }

        private static void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
        {
            if (ImGui.GetIO().WantCaptureKeyboard)
                return;

            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;
            }
        }

        #endregion

        #region Win32 Interop

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [LibraryImport("dwmapi.dll")]
        private static partial int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

        [LibraryImport("user32.dll")]
        private static partial nint SendMessageW(nint hWnd, uint Msg, nint wParam, nint lParam);

        [LibraryImport("user32.dll")]
        private static partial nint LoadImageW(nint hInst, nint lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint GetModuleHandleW(string lpModuleName);

        private const uint IMAGE_ICON = 1;
        private const uint LR_DEFAULTCOLOR = 0x00000000;
        private const uint WM_SETICON = 0x0080;
        private const nint ICON_SMALL = 0;
        private const nint ICON_BIG = 1;

        private const int IDI_APPLICATION = 32512;

        private static void EnableDarkMode(nint hwnd)
        {
            int useImmersiveDarkMode = 1;
            _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
        }

        private static void SetWindowIcon(nint hwnd)
        {
            var hModule = GetModuleHandleW(null);
            if (hModule == nint.Zero)
                return;

            var hIconSmall = LoadImageW(hModule, IDI_APPLICATION, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR);
            var hIconBig = LoadImageW(hModule, IDI_APPLICATION, IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR);

            if (hIconSmall != nint.Zero)
                SendMessageW(hwnd, WM_SETICON, ICON_SMALL, hIconSmall);
            if (hIconBig != nint.Zero)
                SendMessageW(hwnd, WM_SETICON, ICON_BIG, hIconBig);
        }

        #endregion
    }
}