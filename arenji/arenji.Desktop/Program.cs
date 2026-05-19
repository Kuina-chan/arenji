using System;
using arenji.Game;
using osu.Framework;
using osu.Framework.Platform;

namespace arenji.Desktop
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"arenji"))
            using (arenjiGame game = new arenjiGame(args))
                host.Run(game);
        }
    }
}