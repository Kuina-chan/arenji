using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using System;

namespace arenji.Game.particles
{
    public partial class arenjiParticle : PoolableDrawable
    {
        private Vector2 currentVelocity;
        private float turbulenceAmount;
        private float baseSize;
        private readonly Random rng = new Random();

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            
            // Additive blending is CRITICAL for this look. 
            // It makes overlapping particles glow white-hot at the core.
            Blending = BlendingParameters.Additive;

            InternalChild = new Circle
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            };
        }

        public void Fire(Vector2 startPosition, Vector2 initialVelocity, Color4 color, double lifetimeMs, float turbulence, float size)
        {
            Position = startPosition;
            currentVelocity = initialVelocity;
            turbulenceAmount = turbulence;

            float depthScale = (float)(rng.NextDouble() * 1.5 + 0.5); 
            baseSize = size * depthScale;
            Size = new Vector2(baseSize); 
            
            float startingAlpha = depthScale > 1.2f ? 0.4f : 0.8f;
            
            Colour = color; 
            Alpha = startingAlpha;

            // Soft fade out over its lifetime
            this.FadeOut(lifetimeMs, Easing.InQuad).Expire(); 
        }

        protected override void Update()
        {
            base.Update();

            // 1. Turbulence (Drift)
            if (turbulenceAmount > 0)
            {
                float jitterX = (float)((rng.NextDouble() - 0.5) * turbulenceAmount);
                float jitterY = (float)((rng.NextDouble() - 0.5) * turbulenceAmount);
                currentVelocity += new Vector2(jitterX, jitterY);
            }

            // 2. Friction (Slows down gracefully)
            currentVelocity *= 0.95f;

            // 3. Movement
            Position += currentVelocity * (float)(Time.Elapsed / 1000.0);
            
            float speed = currentVelocity.Length;

            Rotation = MathHelper.RadiansToDegrees((float)Math.Atan2(currentVelocity.Y, currentVelocity.X)) + 90f;

            float stretchFactor = 1f + (speed * 0.004f); 
            Scale = new Vector2(1f, stretchFactor);
        }
    }
}