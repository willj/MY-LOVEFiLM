using System;
using System.Net;

namespace Madebywill.Helpers
{
    /// <summary>
    /// Token
    /// </summary>
    public class T
    {
        public T(string p1, string p2)
        {
            K = p1 + p2;
        }

        public string K { get; private set; }
    }

    public static class Keys
    {
        /// <summary>
        /// Get Consumer Key
        /// </summary>
        /// <returns></returns>
        public static string GetConsumerKey()
        {
			// Removed
        }

        /// <summary>
        /// Get Consumer Secret
        /// </summary>
        /// <returns></returns>
        public static string GetConsumerSecret()
        {
			// Removed
        }
    }
}
