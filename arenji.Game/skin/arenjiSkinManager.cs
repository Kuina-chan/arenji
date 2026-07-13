using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Logging;

namespace arenji.Game
{
    public static class arenjiSkinManager
    {
        public static TextureStore SkinTextures { get; private set; }

        public static void Initialize(GameHost host)
        {
            // 1. Create an empty master container that can hold multiple stores
            var fallbackStore = new ResourceStore<byte[]>();

            // 2. STORE #1: The Physical User Folder (Highest Priority)
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            var storage = new NativeStorage(exePath); 
            fallbackStore.AddStore(new StorageBackedResourceStore(storage));

            // 3. STORE #2: The Embedded .dll (Fallback Priority)
            var dllStore = new DllResourceStore(typeof(arenji.Resources.arenjiResources).Assembly);
            fallbackStore.AddStore(dllStore);
            
            foreach (string resourceName in dllStore.GetAvailableResources())
            {
                Logger.Log($"FOUND ASSET: '{resourceName}'", LoggingTarget.Runtime, LogLevel.Important);
            }
            SkinTextures = new TextureStore(host.Renderer, new TextureLoaderStore(fallbackStore));
        }
    }
}