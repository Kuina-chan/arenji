using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

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
            // We tell it to look at the exact folder where the .exe is running
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            var storage = new NativeStorage(exePath); 
            fallbackStore.AddStore(new StorageBackedResourceStore(storage));

            // 3. STORE #2: The Embedded .dll (Fallback Priority)
            // If the user didn't put a file in their folder, pull your default out of the code!
            fallbackStore.AddStore(new DllResourceStore(typeof(arenji.Resources.arenjiResources).Assembly));

            // 4. Build the TextureStore using our master container
            SkinTextures = new TextureStore(host.Renderer, new TextureLoaderStore(fallbackStore));
        }
    }
}