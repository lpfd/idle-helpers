namespace UnityEngine
{
    public class Debug
    {
        public static void LogError(object message)
        {
            throw new Exception(message?.ToString() ?? "");
        }
    }
}
