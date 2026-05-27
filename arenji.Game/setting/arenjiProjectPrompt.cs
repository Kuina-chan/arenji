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
    public partial class arenjiProjectPrompt : FocusedOverlayContainer
    {
        private BasicTextBox pathInput;
        private BasicTextBox nameInput;
        
        // UPGRADED: Now passes (ParentPath, ProjectName)
        public Action<string, string> OnConfirm; 

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
                    Width = 450, AutoSizeAxes = Axes.Y,
                    Masking = true, CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical, Padding = new MarginPadding(25),
                            Spacing = new osuTK.Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new SpriteText { Text = "Save New Project", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan },
                                
                                // 1. The Parent Path Input
                                new SpriteText { Text = "Parent Folder Path:" },
                                pathInput = new BasicTextBox
                                {
                                    RelativeSizeAxes = Axes.X, Height = 35,
                                    PlaceholderText = @"e.g., C:\VisualizerProjects",
                                    // A nice UX touch: auto-fill their Documents folder as a default suggestion
                                    Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "arenjiProjects")
                                },

                                // 2. The Project Name Input
                                new SpriteText { Text = "Project Name:" },
                                nameInput = new BasicTextBox
                                {
                                    RelativeSizeAxes = Axes.X, Height = 35,
                                    PlaceholderText = "My Awesome Visualizer"
                                },
                                
                                new BasicButton
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40, Text = "Save & Load",
                                    BackgroundColour = Color4.DarkCyan,
                                    Margin = new MarginPadding { Top = 10 },
                                    Action = () => 
                                    {
                                        // Ensure they typed something in both boxes!
                                        if (!string.IsNullOrWhiteSpace(pathInput.Text) && !string.IsNullOrWhiteSpace(nameInput.Text))
                                        {
                                            OnConfirm?.Invoke(pathInput.Text, nameInput.Text);
                                            Hide();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            this.FadeIn(200, Easing.OutQuint);
            GetContainingFocusManager().ChangeFocus(nameInput);
        }
        
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}