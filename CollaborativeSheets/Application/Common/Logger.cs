namespace CollaborativeSheets.Application.Common
{
    public class Logger
    {
        private static readonly string LogFile = "collaborative_system.log";
        private static readonly object LockObj = new object();

        public static void Log(string message)
        {
            lock (LockObj)
            {
                try
                {
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                    File.AppendAllText(LogFile, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to log file: {ex.Message}");
                }
            }
        }
    }

}
