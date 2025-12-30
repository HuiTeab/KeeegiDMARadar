
using VmmSharpEx.Extensions.Input;

namespace KeeegiDMARadar.UI.Hotkeys.Internal
{
    /// <summary>
    /// Combo Box Wrapper for WindowsVirtualKeyCode Enums for Hotkey Manager.
    /// </summary>
    public sealed class ComboHotkeyValue
    {
        /// <summary>
        /// Full name of the Key.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Key enum value.
        /// </summary>
        public Win32VirtualKey Code { get; }

        public ComboHotkeyValue(Win32VirtualKey keyCode)
        {
            Key = keyCode.ToString();
            Code = keyCode;
        }

        public override string ToString() => Key;
    }
}
