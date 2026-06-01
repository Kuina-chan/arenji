using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace arenji.Game.UI 
{
    // Inherit from Box so we can size it, and IHasTooltip to trigger the framework's engine!
    public partial class Tooltips : Box, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }

        public Tooltips(string tooltipMessage)
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            AlwaysPresent = true;
            
            TooltipText = tooltipMessage;
        }
    }
}