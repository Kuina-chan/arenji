using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
using System;
using System.IO;

namespace arenji.Game
{
    public partial class arenjiAudioSelector : FocusedOverlayContainer
    {
        public Action<string> OnFileSelected;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

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
                    Width = 600, Height = 500,
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
                                new SpriteText { Text = "Select Backing Audio", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan, Margin = new MarginPadding { Bottom = 10 } },
                                
                                new BasicFileSelector
                                {
                                    RelativeSizeAxes = Axes.X, Height = 380
                                }.With(s => 
                                {
                                    s.CurrentPath.Value = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                                    
                                    s.CurrentFile.ValueChanged += e => 
                                    {
                                        if (e.NewValue != null)
                                        {
                                            string ext = e.NewValue.Extension.ToLower();
                                            string[] allowed = { ".mp3", ".wav", ".ogg" };
                                            
                                            if (Array.Exists(allowed, x => x == ext))
                                            {
                                                OnFileSelected?.Invoke(e.NewValue.FullName);
                                                Hide();
                                            }
                                            else
                                            {
                                                s.CurrentFile.Value = null; 
                                            }
                                        }
                                    };
                                }),

                                new BasicButton
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40, Text = "Cancel",
                                    BackgroundColour = Color4.DarkRed, Margin = new MarginPadding { Top = 10 },
                                    Action = Hide
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