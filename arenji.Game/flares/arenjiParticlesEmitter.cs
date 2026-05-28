using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osuTK;
using osuTK.Graphics;
using System;

namespace arenji.Game.particles
{
    public partial class ParticleEmitter : CompositeDrawable
    {
        private DrawablePool<arenjiParticle> particlePool;
        private readonly Random rng = new Random();

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both; 
            particlePool = new DrawablePool<arenjiParticle>(512);
            AddInternal(particlePool);
        }

        /// <summary>
        /// Fires a burst of particles. Connect your UI settings directly to these parameters!
        /// </summary>
        public void Emit(Vector2 startPosition, Color4 color, int particleCount, double lifetimeMs, float speedMultiplier, float turbulence, float particleSize)
        {
            for (int i = 0; i < particleCount; i++)
            {
                var particle = particlePool.Get();
                AddInternal(particle);
                
                float baseSpeed = (float)(rng.NextDouble() * 800 + 400); 
                float finalSpeed = baseSpeed * speedMultiplier;
                
                Vector2 velocity = new Vector2(0, -finalSpeed);

                double randomizedLifetime = lifetimeMs * (0.8 + (rng.NextDouble() * 0.4)); 

                particle.Fire(startPosition, velocity, color, randomizedLifetime, turbulence, particleSize);
            }
        }
    }
}