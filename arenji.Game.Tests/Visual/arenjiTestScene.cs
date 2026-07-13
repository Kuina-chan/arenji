using osu.Framework.Testing;

namespace arenji.Game.Tests.Visual
{
    public abstract partial class arenjiTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new arenjiTestSceneTestRunner();

        private partial class arenjiTestSceneTestRunner : arenjiGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
