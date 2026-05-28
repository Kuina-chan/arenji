using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using osuTK;
using System;

namespace arenji.Game
{
    public partial class arenjiSettings : FocusedOverlayContainer
    {
        public readonly BindableFloat WhiteNoteWidth = new BindableFloat(1.0f) { MinValue = 0.2f, MaxValue = 1.0f };
        public readonly BindableFloat BlackNoteWidth = new BindableFloat(0.6f) { MinValue = 0.1f, MaxValue = 0.8f };
        public readonly BindableFloat NoteRoundness = new BindableFloat(0.5f) { MinValue = 0f, MaxValue = 20f };
        public readonly BindableFloat NoteOpacity = new BindableFloat(1.0f) { MinValue = 0f, MaxValue = 1.0f };
        public readonly BindableFloat BackgroundOpacity = new BindableFloat(1.0f) { MinValue = 0f, MaxValue = 1.0f };
        public readonly BindableFloat BackgroundOffset = new BindableFloat(0f) { MinValue = -10f, MaxValue = 10f };
        public readonly BindableFloat SoundFontVolume = new BindableFloat(0f) { MinValue = 0f, MaxValue = 1.0f };
        public readonly BindableFloat BackingAudioVolume = new BindableFloat(0f) { MinValue = 0f, MaxValue = 1.0f };
        public readonly BindableFloat ParticleLifeTime = new BindableFloat(0.1f) { MinValue = 0f, MaxValue = 3f};
        public readonly BindableFloat ParticleTurbulance = new BindableFloat(20f) { MinValue = 0f, MaxValue = 200f};
        public readonly BindableFloat ParticleSpeed = new BindableFloat(1.2f) { MinValue = 0f, MaxValue = 1f};
        public readonly BindableFloat ParticleSize = new BindableFloat(26f) {MinValue = 0f, MaxValue = 40f};
        public readonly BindableFloat ParticleCount = new BindableFloat(10) {MinValue = 0, MaxValue = 100, Precision = 1f};
        public readonly BindableBool MuteSoundfont = new BindableBool(false);
        public readonly BindableBool MuteBackingAudio = new BindableBool(false);
        public Action OnRequestAudioImport;
        
        public Action<NoteColorMode> OnRequestAdvancedColors;
        public Action OnRequestBackgroundChange;
        public Action OnRequestImport;
        
        private FillFlowContainer solidSettingsGroup;
        private BasicButton advancedColorsButton;
        private BasicButton modeCycleButton;

        // Allows the user to press Escape to close the panel
        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == osuTK.Input.Key.Escape && State.Value == Visibility.Visible)
            {
                Hide(); 
                return true;
            }
            
            return base.OnKeyDown(e);
        }

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

            var BackgroundImport = new BasicButton
            {
                RelativeSizeAxes = Axes.X, Height = 40, 
                Text = "Change Background...", 
                BackgroundColour = Color4.MediumPurple,
                Margin = new MarginPadding { Bottom = 20 },
                Action = () => OnRequestBackgroundChange?.Invoke()
            };

            SoundFontVolume.BindValueChanged(e => 
            {
                if (e.NewValue <= 0f) MuteSoundfont.Value = true;
                else if (e.NewValue > 0f && MuteSoundfont.Value) MuteSoundfont.Value = false;
            });
            BackingAudioVolume.BindValueChanged(e => 
            {
                if (e.NewValue <= 0f) MuteBackingAudio.Value = true;
                else if (e.NewValue > 0f && MuteBackingAudio.Value) MuteBackingAudio.Value = false;
            });

            MuteSoundfont.BindValueChanged(e => 
            {
                if (!e.NewValue) MuteBackingAudio.Value = true;
            });

            MuteBackingAudio.BindValueChanged(e => 
            {
                if (!e.NewValue) MuteSoundfont.Value = true;
            });
            Children = new Drawable[]
            {
                new ClickableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = Hide,
                    Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.7f }
                },
                new ClickableContainer
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 500, AutoSizeAxes = Axes.Y,
                    Masking = true, CornerRadius = 15,
                    Action = () => { },
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
                                    importButton,

                                    new SpriteText { Text = "Visualizer Settings", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan },
                                    createLabeledSlider("White Note Width", WhiteNoteWidth),
                                    createLabeledSlider("Black Note Width", BlackNoteWidth),
                                    createLabeledSlider("Note Roundness", NoteRoundness),
                                    createLabeledSlider("Note Opacity", NoteOpacity),
                                    new SpriteText
                                    {
                                        Text = "Background Setting",
                                        Font = FrameworkFont.Regular.With(size: 24), 
                                        Colour = Color4.Cyan,
                                    },
                                    BackgroundImport,
                                    createLabeledSlider("Background Opacity", BackgroundOpacity),
                                    createLabeledSlider("Background Offset (s)", BackgroundOffset),
                                    new SpriteText { Text = "Audio Settings", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan, Margin = new MarginPadding { Top = 20, Bottom = 5 } },
                                    new BasicButton
                                    {
                                        RelativeSizeAxes = Axes.X, Height = 40, Text = "Import Backing Audio...", 
                                        BackgroundColour = Color4.MediumSeaGreen, Margin = new MarginPadding { Bottom = 10 },
                                        Action = () => OnRequestAudioImport?.Invoke()
                                    },
                                    createToggleButton("Soundfont (MIDI)", MuteSoundfont),
                                    createLabeledSlider("Soundfont Volume", SoundFontVolume),
                                    createToggleButton("Backing Audio", MuteBackingAudio),
                                    createLabeledSlider("Backing Volume", BackingAudioVolume),
                                    new SpriteText 
                                    { 
                                        Text = "Color Settings", 
                                        Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan,
                                        Margin = new MarginPadding { Top = 20 } 
                                    },
                                    createModeCycleButton(),
                                    solidSettingsGroup,
                                    advancedColorsButton,
                                    new SpriteText
                                    {
                                        Text = "Particle Setting", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan, Margin = new MarginPadding { Top = 20, Bottom = 5 }
                                    },
                                    createLabeledSlider("Particle Lifetime", ParticleLifeTime),
                                    createLabeledSlider("Particle Speed", ParticleSpeed),
                                    createLabeledSlider("Particle Turbulance", ParticleTurbulance),
                                    createLabeledSlider("Particle Size", ParticleSize),
                                    createLabeledSlider("Particle Count", ParticleCount)
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

        private Drawable createLabeledSlider(string labelText, BindableFloat bindable)
        {
            var textBox = new BasicTextBox
            {
                Width = 70,
                Height = 30,
                Text = bindable.Value.ToString("0.00")
            };

            var slider = new BasicSliderBar<float>
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.75f, 
                Height = 15,
                Current = bindable
            };

            textBox.Current.ValueChanged += e =>
            {
                if (float.TryParse(e.NewValue, out float parsed))
                {
                    bindable.Value = parsed; 
                }
            };

            textBox.OnCommit += (sender, isNew) =>
            {
                textBox.Text = bindable.Value.ToString("0.00");
            };

            bindable.BindValueChanged(e =>
            {
                if (!textBox.HasFocus)
                {
                    textBox.Text = e.NewValue.ToString("0.00");
                }
            }, true); 

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(0, 5),
                Children = new Drawable[]
                {
                    new SpriteText 
                    { 
                        Text = labelText, 
                        Font = FrameworkFont.Regular.With(size: 20),
                        Colour = Color4.White
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new osuTK.Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            slider,
                            textBox
                        }
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

        private Drawable createToggleButton(string label, BindableBool bindable)
        {
            var button = new BasicButton { RelativeSizeAxes = Axes.X, Height = 40 };

            // Update visuals when the bindable changes
            bindable.BindValueChanged(e =>
            {
                button.Text = label + (e.NewValue ? " [MUTED]" : " [ON]");
                button.BackgroundColour = e.NewValue ? Color4.DarkRed : Color4.DarkGreen;
            }, true);

            button.Action = () => bindable.Toggle();
            return button;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() 
        {
            this.FadeOut(200, Easing.OutQuint);
            arenjiProjectManager.SaveCurrentProject(this);
        }

        
    }
}