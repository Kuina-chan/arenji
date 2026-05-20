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

        // 1. We create references to our grouped containers so we can hide/show them later!
        private FillFlowContainer solidSettingsGroup;
        private FillFlowContainer trackSettingsGroup;
        private FillFlowContainer noteSettingsGroup;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            // 2. Generate the dynamic groups before we add them to the screen
            createSettingsGroups();

            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.7f },
                
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 400,
                    AutoSizeAxes = Axes.Y,
                    Masking = true, 
                    CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(40, 40, 40, 255) },
                        
                        new BasicScrollContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 600, 
                            Child = new FillFlowContainer
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
                                    createLabeledSlider("Note Roundness", NoteRoundness),

                                    new SpriteText 
                                    { 
                                        Text = "Color Settings", 
                                        Font = FrameworkFont.Regular.With(size: 24), 
                                        Colour = Color4.Cyan,
                                        Margin = new MarginPadding { Top = 20 } 
                                    },
                                    
                                    createModeCycleButton(),
                                    
                                    // 3. Drop the generated groups right into the layout!
                                    solidSettingsGroup,
                                    trackSettingsGroup,
                                    noteSettingsGroup
                                }
                            }
                        }
                    }
                }
            };

            // Force the layout to hide the inactive groups right as the panel loads
            updateGroupVisibility();
        }

        // --- GROUP GENERATION & VISIBILITY ---

        private void createSettingsGroups()
        {
            // A. Create the Solid group (Just one box)
            solidSettingsGroup = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical,
                Child = createLabeledTextBox("Global Solid Color:", "e.g., #FF0000 or 255, 0, 0", text => 
                {
                    ArenjiColorManager.SolidColor = ArenjiColorManager.ParseString(text, ArenjiColorManager.SolidColor);
                })
            };

            // B. Create the Track group (Loop to generate 16 boxes)
            trackSettingsGroup = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical, Spacing = new Vector2(0, 10)
            };
            for (int i = 0; i < 16; i++)
            {
                int trackIndex = i; // CRITICAL: We must capture the loop variable for the lambda!
                trackSettingsGroup.Add(createLabeledTextBox($"Track {trackIndex + 1} Color:", "Hex or RGB", text => 
                {
                    ArenjiColorManager.TrackColors[trackIndex] = ArenjiColorManager.ParseString(text, Color4.White);
                }));
            }

            // C. Create the Note group (Loop over the 12 musical pitches)
            noteSettingsGroup = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y, Direction = FillDirection.Vertical, Spacing = new Vector2(0, 10)
            };
            string[] pitchNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            for (int i = 0; i < pitchNames.Length; i++)
            {
                int pitchIndex = i; // CRITICAL: Capture variable again!
                noteSettingsGroup.Add(createLabeledTextBox($"Note {pitchNames[pitchIndex]} Color:", "Hex or RGB", text => 
                {
                    ArenjiColorManager.NoteColors[pitchIndex] = ArenjiColorManager.ParseString(text, Color4.White);
                }));
            }
        }

        private void updateGroupVisibility()
        {
            // In osu-framework, Hide() collapses the element entirely so it takes up zero space!
            if (ArenjiColorManager.CurrentMode == NoteColorMode.Solid) solidSettingsGroup.Show();
            else solidSettingsGroup.Hide();

            if (ArenjiColorManager.CurrentMode == NoteColorMode.ByTrack) trackSettingsGroup.Show();
            else trackSettingsGroup.Hide();

            if (ArenjiColorManager.CurrentMode == NoteColorMode.ByNote) noteSettingsGroup.Show();
            else noteSettingsGroup.Hide();
        }

        // --- UI HELPER METHODS ---

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

        private Drawable createModeCycleButton()
        {
            var button = new BasicButton
            {
                RelativeSizeAxes = Axes.X, Height = 40, Text = $"Current Mode: {ArenjiColorManager.CurrentMode}",
                BackgroundColour = Color4.DarkCyan
            };

            button.Action = () =>
            {
                int nextMode = ((int)ArenjiColorManager.CurrentMode + 1) % 3;
                ArenjiColorManager.CurrentMode = (NoteColorMode)nextMode;
                button.Text = $"Current Mode: {ArenjiColorManager.CurrentMode}";
                
                // Trigger the visibility swap instantly!
                updateGroupVisibility(); 
            };

            return button;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}