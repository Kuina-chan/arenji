using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osuTK.Graphics;

namespace arenji.Game
{
    public partial class arenjiSaberLayer : Container
    {
        private Video saberVideo;
        
        public arenjiSaberLayer()
        {
            RelativeSizeAxes = Axes.X;
            Height = 250; 
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Y = 5; 
        }
        [BackgroundDependencyLoader]
        private void load() 
        {
            var resources = new osu.Framework.IO.Stores.DllResourceStore(typeof(arenji.Resources.arenjiResources).Assembly);
            
            var videoStream = resources.GetStream("saber/saber.mp4"); 
            
            if (videoStream != null)
            {
                LoadVideo(videoStream);
            }
            else
            {
                osu.Framework.Logging.Logger.Log("Failed to find saber/saber.mp4 in resources! Check your .csproj and folder names.", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
            }
        }

        // 3. Update this to accept a raw Stream instead of a file path
        public void LoadVideo(Stream stream)
        {
            ClearInternal(); 
            if (stream == null) return;

            try
            {
                // We no longer need FileStream, we just pass the embedded stream directly!
                saberVideo = new Video(stream, false)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Stretch, 
                    Loop = true,
                    Blending = BlendingParameters.Additive 
                };
                
                AddInternal(saberVideo);
            }
            catch (Exception ex)
            {
                osu.Framework.Logging.Logger.Log($"Saber Video Error: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
            }
        }

        public void UpdateSaber(Color4 color, float opacity, float brightness)
        {
            this.Alpha = opacity;

            if (saberVideo != null)
            {
                saberVideo.Colour = new Color4(
                    Math.Clamp(color.R * brightness, 0f, 1f),
                    Math.Clamp(color.G * brightness, 0f, 1f),
                    Math.Clamp(color.B * brightness, 0f, 1f),
                    1f
                );
            }
        }
    }
}