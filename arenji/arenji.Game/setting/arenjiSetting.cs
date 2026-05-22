using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
using osuTK;
using System;

namespace arenji.Game
{
    public partial class arenjiSettings : FocusedOverlayContainer
    {
        public readonly BindableFloat WhiteNoteWidth = new BindableFloat(1.0f) { MinValue = 0.2f, MaxValue = 1.0f };
        public readonly BindableFloat BlackNoteWidth = new BindableFloat(0.6f) { MinValue = 0.1f, MaxValue = 0.8f };
        public readonly BindableFloat NoteRoundness = new BindableFloat(0f) { MinValue = 0f, MaxValue = 25f };
        public Action<NoteColorMode> OnRequestAdvancedColors;
        public Action OnRequestImport;
        private FillFlowContainer solidSettingsGroup;
        private BasicButton advancedColorsButton;
        private BasicButton modeCycleButton;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            solidSettingsGroup = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical,
                Child = createLabeledTextBox("Global Solid Color:", "e.g., #FF0000", text => 
                {
                    ArenjiColorManager.SolidColor = ArenjiColorManager.ParseString(text, ArenjiColorManager.SolidColor);
                })
            };

            advancedColorsButton = new BasicButton
            {
                RelativeSizeAxes = Axes.X, Height = 40, BackgroundColour = Color4.SteelBlue,
                Action = () => OnRequestAdvancedColors?.Invoke(ArenjiColorManager.CurrentMode)
            };

            var importButton = new BasicButton
            {
                RelativeSizeAxes = Axes.X, Height = 40, 
                Text = "Import Existing Project...", 
                BackgroundColour = Color4.ForestGreen,
                Margin = new MarginPadding { Bottom = 20 },
                Action = () => OnRequestImport?.Invoke()
            };

            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.7f },
                
                new Container
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 400, AutoSizeAxes = Axes.Y,
                    Masking = true, CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(40, 40, 40, 255) },
                        
                        new BasicScrollContainer
                        {
                            RelativeSizeAxes = Axes.X, Height = 600, 
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding(30), Spacing = new Vector2(0, 20),
                                Children = new Drawable[]
                                {
                                    new SpriteText { Text = "Visualizer Settings", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan },
                                    importButton,
                                    createLabeledSlider("White Note Width", WhiteNoteWidth),
                                    createLabeledSlider("Black Note Width", BlackNoteWidth),
                                    createLabeledSlider("Note Roundness", NoteRoundness),

                                    new SpriteText 
                                    { 
                                        Text = "Color Settings", 
                                        Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan,
                                        Margin = new MarginPadding { Top = 20 } 
                                    },
                                    
                                    createModeCycleButton(), // <-- Called right here!
                                    solidSettingsGroup,
                                    advancedColorsButton
                                }
                            }
                        }
                    }
                }
            };

            updateGroupVisibility();
        }

        public void RefreshUIAfterLoad()
        {
            updateGroupVisibility();
        }

        // --- PLACE YOUR UPDATED METHODS HERE ---

        private void updateGroupVisibility()
        {
            if (modeCycleButton != null)
                modeCycleButton.Text = $"Current Mode: {ArenjiColorManager.CurrentMode}";

            if (ArenjiColorManager.CurrentMode == NoteColorMode.Solid)
            {
                solidSettingsGroup.Show();
                advancedColorsButton.Hide();
            }
            else
            {
                solidSettingsGroup.Hide();
                advancedColorsButton.Show();
                advancedColorsButton.Text = $"Configure {ArenjiColorManager.CurrentMode} Colors...";
            }
        }

        private Drawable createModeCycleButton()
        {
            modeCycleButton = new BasicButton
            {
                RelativeSizeAxes = Axes.X, Height = 40, 
                Text = $"Current Mode: {ArenjiColorManager.CurrentMode}",
                BackgroundColour = Color4.DarkCyan
            };

            modeCycleButton.Action = () =>
            {
                int nextMode = ((int)ArenjiColorManager.CurrentMode + 1) % 3;
                ArenjiColorManager.CurrentMode = (NoteColorMode)nextMode;
                updateGroupVisibility(); 
            };

            return modeCycleButton;
        }

        // --- YOUR EXISTING HELPERS ---

        private Drawable createLabeledSlider(string label, BindableFloat bindable)
        {
            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical, Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new SpriteText { Text = label, Font = FrameworkFont.Regular.With(size: 16) },
                    new BasicSliderBar<float>
                    {
                        RelativeSizeAxes = Axes.X, Height = 20, Current = bindable,
                        BackgroundColour = Color4.Black, SelectionColour = Color4.Cyan
                    }
                }
            };
        }

        private Drawable createLabeledTextBox(string label, string placeholder, Action<string> onTextChanged)
        {
            var textBox = new BasicTextBox { RelativeSizeAxes = Axes.X, Height = 30, PlaceholderText = placeholder };
            textBox.Current.ValueChanged += e => onTextChanged?.Invoke(e.NewValue);

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical, Spacing = new Vector2(0, 5),
                Children = new Drawable[] { new SpriteText { Text = label, Font = FrameworkFont.Regular.With(size: 16) }, textBox }
            };
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() 
        {
            this.FadeOut(200, Easing.OutQuint);
            arenjiProjectManager.SaveCurrentProject(this);
        }
    }
}