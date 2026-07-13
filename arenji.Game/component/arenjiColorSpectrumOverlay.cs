using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;
using System;

namespace arenji.Game.UI
{
    public partial class arenjiColorPickerOverlay : FocusedOverlayContainer
    {
        public Action<Color4> OnColorConfirmed;

        private Color4 selectedColor = Color4.White;
        private Box colorPreview;
        private BasicTextBox hexTextBox;
        private Sprite spectrumSprite;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                // 1. Darken the background when open
                new ClickableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = Hide,
                    Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.7f }
                },
                // 2. The Main Popup Panel
                new Container
                {
                    Anchor = Anchor.Centre, Origin = Anchor.Centre,
                    Width = 400, Height = 350,
                    Masking = true, CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(40, 40, 40, 255) },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both, Direction = FillDirection.Vertical,
                            Padding = new MarginPadding(20), Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                // 3. The Interactive Spectrum
                                new ClickableContainer
                                {
                                    RelativeSizeAxes = Axes.X, Height = 200,
                                    Child = spectrumSprite = new Sprite
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Texture = textures.Get("Statics/spectrum") // Ensure this matches your PNG name!
                                    }
                                },
                                
                                // 4. The Bottom Controls (Preview, Hex, Confirm)
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X, Height = 40,
                                    Direction = FillDirection.Horizontal, Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        colorPreview = new Box { Width = 40, Height = 40, Colour = Color4.White },
                                        hexTextBox = new BasicTextBox { Width = 120, Height = 40, Text = "#FFFFFF" },
                                        new BasicButton
                                        {
                                            Width = 150, Height = 40, Text = "Confirm",
                                            BackgroundColour = Color4.MediumSeaGreen,
                                            Action = () =>
                                            {
                                                OnColorConfirmed?.Invoke(selectedColor);
                                                Hide();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        // 5. Override mouse events to sample the color from the image!
        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (spectrumSprite.Contains(e.ScreenSpaceMousePosition))
            {
                sampleColorFromMouse(spectrumSprite.ToLocalSpace(e.ScreenSpaceMousePosition));
                return true;
            }
            return base.OnMouseDown(e);
        }
        protected override bool OnDragStart(DragStartEvent e)
        {
            if (spectrumSprite.Contains(e.ScreenSpaceMouseDownPosition))
            {
                return true;
            }
            return base.OnDragStart(e);
        }
        protected override void OnDrag(DragEvent e)
        {
            sampleColorFromMouse(spectrumSprite.ToLocalSpace(e.ScreenSpaceMousePosition));
            base.OnDrag(e);
        }

        private void sampleColorFromMouse(Vector2 localMousePos)
        {
            // Calculate where the mouse is relative to the width/height of the spectrum
            float xPercent = Math.Clamp(localMousePos.X / spectrumSprite.DrawWidth, 0f, 1f);
            float yPercent = Math.Clamp(localMousePos.Y / spectrumSprite.DrawHeight, 0f, 1f);

            // X = Hue (0 to 1), Y = Lightness (1 to 0, since Y grows downwards)
            float hue = xPercent;
            float lightness = 1.0f - yPercent;
            float saturation = 1.0f; // Kept at max for this specific spectrum image

            selectedColor = HSLToColor4(hue, saturation, lightness);

            // Update UI
            colorPreview.Colour = selectedColor;
            hexTextBox.Text = ColorToHex(selectedColor);
        }

        // Helper: Convert HSL back to standard RGB Color4
        private Color4 HSLToColor4(float h, float s, float l)
        {
            float r, g, b;
            if (s == 0) r = g = b = l;
            else
            {
                float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = hueToRGB(p, q, h + 1f / 3f);
                g = hueToRGB(p, q, h);
                b = hueToRGB(p, q, h - 1f / 3f);
            }
            return new Color4(r, g, b, 1f);
        }

        private float hueToRGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        private string ColorToHex(Color4 color) =>
            $"#{((int)(color.R * 255)):X2}{((int)(color.G * 255)):X2}{((int)(color.B * 255)):X2}";

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }
}
