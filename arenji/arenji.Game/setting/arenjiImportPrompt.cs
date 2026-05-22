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
    public partial class arenjiImportPrompt : FocusedOverlayContainer
    {
        private BasicTextBox pathInput;
        private SpriteText errorText;
        
        // Passes the fully validated folder path back to the visualizer!
        public Action<string> OnConfirm; 

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
                                new SpriteText { Text = "Import Project", Font = FrameworkFont.Regular.With(size: 24), Colour = Color4.Cyan },
                                
                                new SpriteText { Text = "Paste the path to your project folder:" },
                                pathInput = new BasicTextBox
                                {
                                    RelativeSizeAxes = Axes.X, Height = 35,
                                    PlaceholderText = @"e.g., C:\VisualizerProjects\MyVis"
                                },

                                // A hidden error message that only shows up if they type a bad path
                                errorText = new SpriteText 
                                { 
                                    Text = "Could not find 'project.ini' in that folder!", 
                                    Colour = Color4.Red, 
                                    Alpha = 0 // Hidden by default
                                },
                                
                                new BasicButton
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40, Text = "Load Project",
                                    BackgroundColour = Color4.DarkCyan,
                                    Margin = new MarginPadding { Top = 10 },
                                    Action = attemptLoad
                                },
                                
                                new BasicButton
                                {
                                    RelativeSizeAxes = Axes.X, Height = 30, Text = "Cancel",
                                    BackgroundColour = Color4.DarkRed,
                                    Action = Hide
                                }
                            }
                        }
                    }
                }
            };

            // Hide the error text if they start typing again
            pathInput.Current.ValueChanged += _ => errorText.Alpha = 0;
        }

        private void attemptLoad()
        {
            string folderPath = pathInput.Text.Trim();
            string expectedIniPath = Path.Combine(folderPath, "project.ini");

            // Validate that the folder actually contains a project!
            if (Directory.Exists(folderPath) && File.Exists(expectedIniPath))
            {
                errorText.Alpha = 0;
                OnConfirm?.Invoke(folderPath);
                Hide();
            }
            else
            {
                // Show the error warning!
                errorText.Alpha = 1; 
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(200, Easing.OutQuint);
            errorText.Alpha = 0; // Reset error state
            pathInput.Text = string.Empty; // Clear old input
            GetContainingFocusManager().ChangeFocus(pathInput);
        }
        
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}