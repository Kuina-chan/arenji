using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace arenji.Game
{
    public partial class arenjiLoadingOverlay : OverlayContainer
    {
        private SpriteIcon spinner;

        protected override bool BlockPositionalInput => true;
        protected override bool BlockNonPositionalInput => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box 
                { 
                    RelativeSizeAxes = Axes.Both, 
                    Colour = Color4.Black, 
                    Alpha = 0.85f 
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new osuTK.Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        spinner = new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Icon = FontAwesome.Solid.CircleNotch,
                            Size = new osuTK.Vector2(60),
                            Colour = Color4.Cyan
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Processing Audio Engine...",
                            Font = FrameworkFont.Regular.With(size: 24)
                        }
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            
            // Spin the loading icon constantly while the overlay is visible
            if (State.Value == Visibility.Visible)
            {
                spinner.Rotation += (float)(Clock.ElapsedFrameTime * 0.3);
            }
        }

        protected override void PopIn() => this.FadeIn(250, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(250, Easing.OutQuint);
    }
}