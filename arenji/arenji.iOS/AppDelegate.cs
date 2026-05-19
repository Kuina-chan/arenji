using osu.Framework.iOS;
using arenji.Game;

namespace arenji.iOS
{
    /// <inheritdoc />
    public class AppDelegate : GameApplicationDelegate
    {
        /// <inheritdoc />
        protected override osu.Framework.Game CreateGame() => new arenjiGame();
    }
}
