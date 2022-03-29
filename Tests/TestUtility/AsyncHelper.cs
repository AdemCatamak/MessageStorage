namespace TestUtility;

public class AsyncHelper
{
    public static async Task WaitAsync(TimeSpan? timeSpan = null)
    {
        TimeSpan wait = timeSpan ?? TimeSpan.FromSeconds(1);
        await Task.Delay(wait);
    }
}