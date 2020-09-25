namespace Koobando.AntiCheat
{
	using System;

	internal class ThreadSafeRandom
	{
		private static readonly Random Global = new Random();

		[ThreadStatic]
		private static Random local;

		private static void Init() {
			if (local == null) {
				int seed;
				lock (Global) {
					seed = Global.Next();
				}
				local = new Random(seed);
			}
		}

		public static int NextInt(int minInclusive, int maxExclusive) {
			Init();
			return local.Next(minInclusive, maxExclusive);
		}

		public static long NextLong(long minInclusive, long maxExclusive) {
			Init();
			long result = (long)local.Next((int)(minInclusive >> 32), (int)(maxExclusive >> 32));
			result <<= 32;
			result |= (uint)local.Next((int)minInclusive, (int)maxExclusive);
			return result;
		}

		public static void NextBytes(byte[] buffer) {
			Init();
			local.NextBytes(buffer);
		}

		public static void NextChars(char[] buffer) {
			Init();
			for (var i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = (char) (local.Next() % 256);
			}
		}

		public static int NextInt(int maxExclusive = int.MaxValue) {
			return NextInt(1, maxExclusive);
		}
	}
}