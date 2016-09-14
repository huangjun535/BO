using Settings = Ashe.Config.Auto;

namespace Ashe.Modes
{
    internal sealed class PermaActive : ModeBase
    {
        internal override bool ShouldBeExecuted()
        {
            return true;
        }

        internal override void Execute() { }
    }
}
