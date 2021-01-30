using System;
using System.Linq;

namespace Microsoft.Nnn.ApplicationCore.Services
{
    public class RandomString
    {
        private static readonly Random Random = new Random();
        
        public static string GenerateString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}