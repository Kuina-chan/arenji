using osu.Framework.Input.Bindings;
using osu.Framework.Graphics;
using osuTK.Input;
using System.Collections.Generic;

namespace arenji.Game
{
    // 1. Define the logical actions your visualizer can perform
    public enum ArenjiAction
    {
        TogglePlayback,
        SeekForward,
        SeekBackward,
        ToggleSetting,
    }

    // 2. Create the container that maps physical keys to those actions
    public partial class ArenjiKeyBindingContainer : KeyBindingContainer<ArenjiAction>
    {
        public override IEnumerable<IKeyBinding> DefaultKeyBindings => new[]
        {
            // Map the Spacebar to the Play/Pause action
            new KeyBinding(InputKey.Space, ArenjiAction.TogglePlayback),
            
            // Map the Arrow keys to seeking
            new KeyBinding(InputKey.Right, ArenjiAction.SeekForward),
            new KeyBinding(InputKey.Left, ArenjiAction.SeekBackward),
            new KeyBinding(new KeyCombination(InputKey.Control, InputKey.O), ArenjiAction.ToggleSetting),
        };
    }
}