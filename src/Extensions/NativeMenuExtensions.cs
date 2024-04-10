using GTA;
using LemonUI.Menus;
using System.Windows.Forms;

namespace EasyScript.Extensions
{
    internal static class NativeMenuExtensions
    {
        public static void SelectItemWithNumKeys(this NativeMenu menu, KeyEventArgs e)
        {
            if (!menu.Visible) return;
            int key = e.KeyValue;
            // Key 0 is 47 and 9 is 57.
            bool isNum = key >= 48 && key <= 57;
            if (isNum)
            {
                // Get value between -1 and 8 (keys 0-9).
                int keyNum = (key - 48) - 1;
                // Set zero key to be item 10.
                if (keyNum == -1)
                    keyNum = 9;
                if (keyNum >= menu.Items.Count) return;
                menu.SelectedIndex = keyNum;

                NativeItem item = menu.SelectedItem;
                if (item is NativeListItem) return;
                Game.SetControlValueNormalized(GTA.Control.PhoneSelect, 1);
            }
        }
    }
}
