using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Bindables; 

namespace arenji.Game.Tooltips
{
    public partial class SmartSliderTooltip : Container, IHasTooltip
    {
        // This is what the global TooltipContainer reads
        public LocalisableString TooltipText { get; private set; }

        private readonly string tooltipMessage;
        private ScheduledDelegate showTimer;
        private readonly BindableFloat sliderBindable;

        public SmartSliderTooltip(string message, BindableFloat bindable)
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true; 
            
            tooltipMessage = message;
            sliderBindable = bindable;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            sliderBindable.ValueChanged += onSliderMoved;
        }

        private void onSliderMoved(ValueChangedEvent<float> e) => hideTooltip();

        protected override bool OnHover(HoverEvent e)
        {
            showTimer?.Cancel();
            
            showTimer = Scheduler.AddDelayed(() => TooltipText = tooltipMessage, 500);
            
            return false; 
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hideTooltip();
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            hideTooltip();
            return false;
        }

        private void hideTooltip()
        {
            showTimer?.Cancel();
            TooltipText = "";
        }

        protected override void Dispose(bool isDisposing)
        {
            if (sliderBindable != null)
                sliderBindable.ValueChanged -= onSliderMoved;
                
            base.Dispose(isDisposing);
        }
    }
}