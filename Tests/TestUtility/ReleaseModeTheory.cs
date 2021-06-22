using Xunit;

namespace TestUtility
{
    public sealed class ReleaseModeTheory : TheoryAttribute
    {
        public ReleaseModeTheory()
        {
            bool isReleaseMode = false;
#if RELEASE
            isReleaseMode = true;
#endif
            if (!isReleaseMode)
            {
                Skip = "This theory run only release mode";
            }
        }
    }
}