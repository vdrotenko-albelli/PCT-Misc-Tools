namespace Albelli.MiscUtils.Lib.Extensions
{
    public static class PollyRetryDurations
    {
        public static readonly TimeSpan[] ThreeTimesFast = {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(100)
        };

        public static readonly TimeSpan[] TenSeconds = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(4)
        };

        public static readonly TimeSpan[] ThirtySeconds = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(6)
        };

        public static readonly TimeSpan[] Minute = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10)
        };

        public static readonly TimeSpan[] TwoMinutes = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15)
        };

        public static readonly TimeSpan[] ThreeMinutes = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromMinutes(1),
        };

        public static readonly TimeSpan[] FiveMinutes = {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1),
        };
    }
}