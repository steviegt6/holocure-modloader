namespace HoloCure.ModLoader.Logging
{
    public static class LogUtils
    {
        public static string GetMessageSignature(DateTime time, LogLevel level, string source) {
            return $"[{time:hh:mm:ss}] [{level.Name}] [{source}]: ";
        }

        public static string[] SplitNewlines(this string value) {
            return value.Split(new []{ "\r\n", "\n" }, StringSplitOptions.None);
        }
    }
}