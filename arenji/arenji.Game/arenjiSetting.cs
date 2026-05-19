using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
using osuTK;

namespace arenji.Game
{
    public partial class arenjiSettings : FocusedOverlayContainer
    {
        // 1. The Bindables. These are the "global" variables that the notes will listen to!
        public readonly BindableFloat WhiteNoteWidth = new BindableFloat(1.0f) { MinValue = 0.2f, MaxValue = 1.0f };
        public readonly BindableFloat BlackNoteWidth = new BindableFloat(0.6f) { MinValue = 0.1f, MaxValue = 0.8f };
        public readonly BindableFloat NoteRoundness = new BindableFloat(0f) { MinValue = 0f, MaxValue = 25f };

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                // The dark background that dims the visualizer
                new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.7f },
                
                // The actual settings box
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 400,
                    AutoSizeAxes = Axes.Y,
                    Masking = true, // Clips the background to the corner radius
                    CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(40, 40, 40, 255) },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding(30),
                            Spacing = new Vector2(0, 20),
                            Children = new Drawable[]
                            {
                                new SpriteText { Text = "Visualizer Settings", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan },
                                createLabeledSlider("White Note Width", WhiteNoteWidth),
                                createLabeledSlider("Black Note Width", BlackNoteWidth),
                                createLabeledSlider("Note Roundness", NoteRoundness)
                            }
                        }
                    }
                }
            };
        }

        // Helper method to draw a text label and a slider bar together
        private Drawable createLabeledSlider(string label, BindableFloat bindable)
        {
            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new SpriteText { Text = label, Font = FrameworkFont.Regular.With(size: 16) },
                    new BasicSliderBar<float>
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 20,
                        Current = bindable,
                        BackgroundColour = Color4.Black,
                        SelectionColour = Color4.Cyan
                    }
                }
            };
        }

        // Framework animations for when the panel opens and closes
        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}