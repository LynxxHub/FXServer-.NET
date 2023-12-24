using CitizenFX.Core;
using System;

namespace lxEF.Server.Managers
{
    internal static class LoggingManager
    {
        public static void PrintExceptions(Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
            Debug.WriteLine("INNER: " + ex.InnerException);
            Debug.WriteLine("STACK: " + ex.StackTrace);
        }
    }
}
