using UnityEngine;

namespace Koobando.UI.Console
{
    [CreateAssetMenu(fileName = "Untitled Key Config", menuName = "Console/Key Config")]
    public class KeyConfig : ScriptableObject
    {
        public ModifierKeyCombo HideConsoleKey = KeyCode.None;
        public ModifierKeyCombo ShowConsoleKey = KeyCode.None;
        public ModifierKeyCombo ToggleConsoleVisibilityKey = new ModifierKeyCombo() { Key = KeyCode.Escape };
        public ModifierKeyCombo SuggestNextCommandKey = KeyCode.Tab;
        public ModifierKeyCombo SuggestPreviousCommandKey = new ModifierKeyCombo() { Key = KeyCode.Tab, Shift = true };
        public KeyCode NextCommandKey = KeyCode.UpArrow;
        public KeyCode PreviousCommandKey = KeyCode.DownArrow;
        public KeyCode SubmitCommandKey = KeyCode.Return;
    }
}
