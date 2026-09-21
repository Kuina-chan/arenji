using System;
using System.IO;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Logging;

namespace arenji.Game
{
    public static class arenjiSkinManager
    {
        public static TextureStore SkinTextures { get; private set; }
        private static GameHost currentHost;

        public static void Initialize(GameHost host)
        {
            currentHost = host;
            UpdateSkinStore(null, false);
        }

        public static void UpdateSkinStore(string projectFolder, bool useProjectSkin)
        {
            if (currentHost == null) return;

            var fallbackStore = new ResourceStore<byte[]>();

            // 1. Project Skin (Highest Priority)
            if (useProjectSkin && !string.IsNullOrEmpty(projectFolder))
            {
                string skinPath = Path.Combine(projectFolder, "skin");
                if (System.IO.Directory.Exists(skinPath))
                {
                    fallbackStore.AddStore(new StorageBackedResourceStore(new NativeStorage(skinPath)));
                    Logger.Log($"[SKIN] Added project skin path: {skinPath}", LoggingTarget.Runtime, LogLevel.Important);
                }
            }

            // 2. Global Physical Folder
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            var storage = new NativeStorage(exePath); 
            fallbackStore.AddStore(new StorageBackedResourceStore(storage));

            // 3. Embedded .dll (Fallback Priority)
            var dllStore = new DllResourceStore(typeof(arenji.Resources.arenjiResources).Assembly);
            fallbackStore.AddStore(dllStore);
            
            foreach (string resourceName in dllStore.GetAvailableResources())
            {
                Logger.Log($"FOUND ASSET: '{resourceName}'", LoggingTarget.Runtime, LogLevel.Important);
            }
            SkinTextures = new TextureStore(currentHost.Renderer, new TextureLoaderStore(fallbackStore));
        }
    }
}