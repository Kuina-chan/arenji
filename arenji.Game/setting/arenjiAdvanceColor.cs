using osu.Framework.Allocation;
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
    public partial class arenjiAdvancedColorOverlay : FocusedOverlayContainer
    {
        private FillFlowContainer colorRowsContainer;
        private SpriteText titleText;
        private NoteColorMode activeMode;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                // Full screen dim so the user focuses on the popup
                new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.8f },
                
                // The main popup window
                new Container
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 450, Height = 500, // Fixed size for the popup
                    Masking = true, CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) },
                        
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both, Direction = FillDirection.Vertical, Padding = new MarginPadding(20),
                            Children = new Drawable[]
                            {
                                // The Title Header
                                titleText = new SpriteText 
                                { 
                                    Font = FrameworkFont.Regular.With(size: 28), 
                                    Colour = Color4.Cyan,
                                    Margin = new MarginPadding { Bottom = 15 }
                                },

                                // The Scrollable list of colors
                                new BasicScrollContainer
                                {
                                    RelativeSizeAxes = Axes.X, Height = 360,
                                    Child = colorRowsContainer = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical, Spacing = new Vector2(0, 10)
                                    }
                                },
                                
                                // The Close Button
                                new BasicButton
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40, Text = "Done",
                                    BackgroundColour = Color4.DarkCyan,
                                    Margin = new MarginPadding { Top = 15 },
                                    Action = Hide // Closes the overlay!
                                }
                            }
                        }
                    }
                }
            };
        }

        // We call this right before showing the panel so it generates the right inputs!
        public void OpenForMode(NoteColorMode mode)
        {
            activeMode = mode;
            colorRowsContainer.Clear(); // Nuke the old UI

            if (mode == NoteColorMode.ByTrack)
            {
                titleText.Text = $"Track Colors ({ArenjiColorManager.ActiveTrackCount} Tracks)";
                for (int i = 0; i < ArenjiColorManager.ActiveTrackCount; i++)
                {
                    int index = i; 
                    Color4 startingColor = ArenjiColorManager.TrackColors.ContainsKey(index) 
                        ? ArenjiColorManager.TrackColors[index] 
                        : Color4.White;

                    colorRowsContainer.Add(createColorRow($"Track {index + 1}", startingColor, newColor => 
                    {
                        ArenjiColorManager.TrackColors[index] = newColor;
                    }));
                }
            }
            else if (mode == NoteColorMode.ByNote)
            {
                titleText.Text = "Pitch Class Colors";
                string[] pitchNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
                for (int i = 0; i < pitchNames.Length; i++)
                {
                    int index = i;
                    Color4 startingColor = ArenjiColorManager.NoteColors.ContainsKey(index) 
                        ? ArenjiColorManager.NoteColors[index] 
                        : Color4.White;

                    colorRowsContainer.Add(createColorRow($"Note {pitchNames[index]}", startingColor, newColor => 
                    {
                        ArenjiColorManager.NoteColors[index] = newColor;
                    }));
                }
            }
            else if (mode == NoteColorMode.ByChannel)
            {
                titleText.Text = "MIDI Channel Colors (1-16)";
                for (int i = 0; i < 16; i++) // MIDI has exactly 16 channels!
                {
                    int index = i;
                    Color4 startingColor = ArenjiColorManager.ChannelColors.ContainsKey(index) 
                        ? ArenjiColorManager.ChannelColors[index] 
                        : Color4.White;

                    colorRowsContainer.Add(createColorRow($"Channel {index + 1}", startingColor, newColor => 
                    {
                        ArenjiColorManager.ChannelColors[index] = newColor;
                    }));
                }
            }

            Show(); // Pops the window in!
        }

        // The UI helper that creates the label, the text box, AND the preview square!
        private Drawable createColorRow(string label, Color4 initialColor, Action<Color4> onColorChanged)
        {
            // 1. Build the square FIRST so the compiler knows it exists!
            var previewSquare = new Box { Width = 30, Height = 30, Colour = initialColor };

            var textBox = new BasicTextBox { Width = 200, Height = 30, PlaceholderText = "Hex or RGB", Text = ArenjiColorManager.ToHex(initialColor) };
            
            // 2. Now the text box can safely talk to the square
            textBox.Current.ValueChanged += e => 
            {
                Color4 parsed = ArenjiColorManager.ParseString(e.NewValue, initialColor);
                previewSquare.Colour = parsed; 
                onColorChanged?.Invoke(parsed); 
            };

            return new GridContainer
            {
                RelativeSizeAxes = Axes.X, Height = 30,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 100), 
                    new Dimension(GridSizeMode.Absolute, 210), 
                    new Dimension(GridSizeMode.AutoSize)       
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText { Text = label, Anchor = Anchor.CentreLeft, Origin = Anchor.CentreLeft },
                        textBox,
                        previewSquare
                    }
                }
            };
        }

        protected override void PopIn() => this.FadeIn(250, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(250, Easing.OutQuint);
    }
}