using Xunit;

namespace TestUtility
{
    public sealed class ReleaseModeFact : FactAttribute
    {
        public ReleaseModeFact()
        {
            bool isReleaseMode = false;
#if RELEASE
            isReleaseMode = true;
#endif
            if (!isReleaseMode)
            {
                Skip = "This fact run only release mode";
            }
        }
    }
}