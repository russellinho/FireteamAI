namespace Koobando.AntiCheat
{
    using UnityEngine;

    internal class BasicUtils
    {
        public static string GenerateRandomString(int length)
		{
			char[] stringChars = new char[length];
			GenerateCharArrayKey(stringChars);
			return new string(stringChars);
		}

        internal static byte GenerateByteKey()
		{
			return (byte)ThreadSafeRandom.NextInt(100, 255);
		}

		internal static sbyte GenerateSByteKey()
		{
			return (sbyte)ThreadSafeRandom.NextInt(100, 127);
		}

        internal static char GenerateCharKey()
		{
			return (char)ThreadSafeRandom.NextInt(10000, 60000);
		}

		internal static short GenerateShortKey()
		{
			return (short)ThreadSafeRandom.NextInt(10000, short.MaxValue);
		}

        internal static ushort GenerateUShortKey()
		{
			return (ushort)ThreadSafeRandom.NextInt(10000, ushort.MaxValue);
		}

		internal static uint GenerateUIntKey()
		{
			return (uint)GenerateIntKey();
		}

        internal static int GenerateIntKey() {
            return ThreadSafeRandom.NextInt(1000000000, int.MaxValue);
        }

        internal static long GenerateLongKey()
		{
			return ThreadSafeRandom.NextLong(1000000000000000000, long.MaxValue);
		}

        internal static ulong GenerateULongKey()
		{
			return (ulong)GenerateLongKey();
		}

        internal static char[] GenerateCharArrayKey(char[] arrayToFill = null)
		{
			if (arrayToFill == null)
			{
				arrayToFill = new char[7];
			}
			else if (arrayToFill.Length < 7)
			{
				arrayToFill = new char[7];
			}

			ThreadSafeRandom.NextChars(arrayToFill);

			return arrayToFill;
		}
    }
}
