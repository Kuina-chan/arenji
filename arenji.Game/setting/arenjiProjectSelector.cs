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
    public partial class arenjiProjectSelector : FocusedOverlayContainer
    {
        public Action<string> OnFolderConfirmed;
        private string selectedFolderPath = string.Empty;
        private BasicButton confirmButton;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            confirmButton = new BasicButton
            {
                RelativeSizeAxes = Axes.None, Width = 280, Height = 40, Text = "Confirm Import",
                BackgroundColour = Color4.Gray,
                Action = () =>
                {
                    if (!string.IsNullOrEmpty(selectedFolderPath))
                    {
                        OnFolderConfirmed?.Invoke(selectedFolderPath);
                        Hide();
                    }
                }
            };

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
                    Width = 600, Height = 550,
                    Masking = true, CornerRadius = 15,
                    Action = () => { }, // Shield click
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
                                    Text = "Select Project Folder", 
                                    Font = FontUsage.Default.With(size: 24), 
                                    Colour = Color4.Cyan, 
                                    Margin = new MarginPadding { Bottom = 10 } 
                                },
                                
                                new BasicDirectorySelector
                                {
                                    RelativeSizeAxes = Axes.X, Height = 380
                                }.With(s => 
                                {
                                    s.CurrentPath.Value = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                                    
                                    s.CurrentPath.BindValueChanged(e => 
                                    {
                                        if (e.NewValue != null && Directory.Exists(e.NewValue.FullName))
                                        {
                                            selectedFolderPath = e.NewValue.FullName;
                                            confirmButton.Enabled.Value = true;
                                            confirmButton.BackgroundColour = Color4.ForestGreen;
                                        }
                                        else
                                        {
                                            selectedFolderPath = string.Empty;
                                            confirmButton.Enabled.Value = false;
                                            confirmButton.BackgroundColour = Color4.Gray;
                                        }
                                    }, true);
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
                                        
                                        // Just inject the pre-built button here!
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