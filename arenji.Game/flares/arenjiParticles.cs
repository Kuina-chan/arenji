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
        private readonly Random rng = new Random();

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Size = new Vector2(8); // Adjust this if you want bigger/smaller sparks
            
            // THE SECRET SAUCE: Additive Blending makes overlapping particles glow brightly!
            Blending = BlendingParameters.Additive;

            InternalChild = new Circle
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            };
        }

        public void Fire(Vector2 startPosition, Vector2 initialVelocity, Color4 color, double lifetimeMs, float turbulence)
        {
            Position = startPosition;
            currentVelocity = initialVelocity;
            turbulenceAmount = turbulence;
            
            // Strictly follow the falling note color
            Colour = color; 
            Scale = Vector2.One;
            Alpha = 1f;

            // We still let the framework handle shrinking, fading, and recycling!
            this.ScaleTo(0, lifetimeMs, Easing.OutQuint)
                .FadeOut(lifetimeMs, Easing.InQuad)
                .Expire(); 
        }

        protected override void Update()
        {
            base.Update();

            // 1. TURBULENCE: Randomly shove the particle off its path every frame
            if (turbulenceAmount > 0)
            {
                float jitterX = (float)((rng.NextDouble() - 0.5) * turbulenceAmount);
                float jitterY = (float)((rng.NextDouble() - 0.5) * turbulenceAmount);
                currentVelocity += new Vector2(jitterX, jitterY);
            }

            // 2. FRICTION: Slow the particle down over time (air resistance)
            // 0.95 means it keeps 95% of its speed each frame. Smooths out the burst!
            currentVelocity *= 0.95f;

            // 3. MOVEMENT: Apply the velocity to the position, scaled by frame time
            // This ensures it moves at the same speed regardless of the user's monitor Hz
            Position += currentVelocity * (float)(Time.Elapsed / 1000.0);
        }
    }
}