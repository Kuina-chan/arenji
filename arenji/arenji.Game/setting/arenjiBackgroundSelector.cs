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
    public partial class arenjiBackgroundSelector : FocusedOverlayContainer
    {
        public Action<string> OnFileSelected;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.8f },
                new Container
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 600, Height = 500,
                    Masking = true, CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both, Direction = FillDirection.Vertical, Padding = new MarginPadding(20),
                            Children = new Drawable[]
                            {
                                new SpriteText { Text = "Select Background (Image or Video)", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan, Margin = new MarginPadding { Bottom = 10 } },
                                
                                // THE FIX: Correct property name and safe Bindable value assignment
                                // THE FIX: Removed the invalid property and added manual validation!
                                new BasicFileSelector
                                {
                                    RelativeSizeAxes = Axes.X, 
                                    Height = 380
                                }.With(s => 
                                {
                                    s.CurrentPath.Value = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                                    
                                    s.CurrentFile.ValueChanged += e => 
                                    {
                                        if (e.NewValue != null)
                                        {
                                            // 1. Grab the extension of the clicked file
                                            string ext = e.NewValue.Extension.ToLower();
                                            string[] allowed = { ".png", ".jpg", ".jpeg", ".mp4", ".avi" };
                                            
                                            // 2. Check if it is in our allowed list
                                            if (Array.Exists(allowed, x => x == ext))
                                            {
                                                OnFileSelected?.Invoke(e.NewValue.FullName);
                                                Hide();
                                            }
                                            else
                                            {
                                                // 3. Reset the selection if they clicked a bad file so they can try again
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