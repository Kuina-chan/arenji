using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
// 1. Add these two namespaces
using osu.Framework.Input.Events;
using osu.Framework.Input.Bindings;

namespace arenji.Game
{
    // 2. Add the IKeyBindingHandler interface here
    public partial class PlaybackControlPanel : FillFlowContainer, IKeyBindingHandler<ArenjiAction>
    {
        // ... (Keep all your existing variables, load(), LinkClock, Update, etc. exactly the same) ...

        // 3. Add the shortcut listener methods at the bottom of the class
        public bool OnPressed(KeyBindingPressEvent<ArenjiAction> e)
        {
            // Ignore input if no song is loaded
            if (linkedClock == null) return false; 

            switch (e.Action)
            {
                case ArenjiAction.TogglePlayback:
                    TogglePlayback();
                    return true; // Return true to tell the framework "I handled this input!"

                case ArenjiAction.SeekForward:
                    SeekBindable.Value += 5000; // Skip forward 5 seconds (5000ms)
                    return true;

                case ArenjiAction.SeekBackward:
                    SeekBindable.Value -= 5000; // Skip backward 5 seconds (5000ms)
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ArenjiAction> e)
        {
            // We don't need to do anything when the user lets go of the key, 
            // but the interface requires this method to exist!
        }
    }
}