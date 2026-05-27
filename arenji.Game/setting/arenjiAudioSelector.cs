using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using System;
using System.IO;

namespace arenji.Game
{
    public partial class arenjiAudioSelector : FocusedOverlayContainer
    {
        public Action<string> OnFileSelected;
        private string selectedFilePath = string.Empty;
        private BasicButton confirmButton;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            // THE FIX: Build the button first so it exists in memory before the file selector fires!
            confirmButton = new BasicButton
            {
                RelativeSizeAxes = Axes.None, Width = 280, Height = 40, Text = "Import Audio",
                BackgroundColour = Color4.Gray,
                Action = () =>
                {
                    if (!string.IsNullOrEmpty(selectedFilePath))
                    {
                        OnFileSelected?.Invoke(selectedFilePath);
                        Hide();
                    }
                }
            };
            // Start it disabled by default
            confirmButton.Enabled.Value = false; 

            Children = new Drawable[]
            {
                new ClickableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = Hide,
                    Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.8f }
                },
                new ClickableContainer
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 600, Height = 520,
                    Masking = true, CornerRadius = 15,
                    Action = () => { }, 
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both, Direction = FillDirection.Vertical, Padding = new MarginPadding(20),
                            Children = new Drawable[]
                            {
                                new SpriteText 
                                { 
                                    Text = "Select Backing Audio", 
                                    Font = FontUsage.Default.With(size: 24), 
                                    Colour = Color4.Cyan, 
                                    Margin = new MarginPadding { Bottom = 10 } 
                                },
                                
                                new BasicFileSelector
                                {
                                    RelativeSizeAxes = Axes.X, Height = 380
                                }.With(s => 
                                {
                                    s.CurrentPath.Value = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                                    
                                    s.CurrentFile.BindValueChanged(e => 
                                    {
                                        if (e.NewValue != null)
                                        {
                                            string ext = e.NewValue.Extension.ToLower();
                                            string[] allowed = { ".mp3", ".wav", ".ogg" };
                                            
                                            if (Array.Exists(allowed, x => x == ext))
                                            {
                                                selectedFilePath = e.NewValue.FullName;
                                                confirmButton.Enabled.Value = true;
                                                confirmButton.BackgroundColour = Color4.MediumSeaGreen;
                                                return;
                                            }
                                        }
                                        
                                        selectedFilePath = string.Empty;
                                        confirmButton.Enabled.Value = false;
                                        confirmButton.BackgroundColour = Color4.Gray;
                                    }, true); // Now this is safe, because the button exists!
                                }),

                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40,
                                    Direction = FillDirection.Horizontal, Spacing = new Vector2(10, 0),
                                    Margin = new MarginPadding { Top = 15 },
                                    Children = new Drawable[]
                                    {
                                        new BasicButton
                                        {
                                            RelativeSizeAxes = Axes.None, Width = 270, Height = 40, Text = "Cancel",
                                            BackgroundColour = Color4.DarkRed, Action = Hide
                                        },
                                        confirmButton
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}