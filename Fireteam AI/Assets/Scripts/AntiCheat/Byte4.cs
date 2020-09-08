namespace Koobando.AntiCheat
{
	using System;

	[Serializable]
	internal struct Byte4
	{
		public byte b1;
		public byte b2;
		public byte b3;
		public byte b4;

		public void Shuffle()
		{
			byte buffer = b2;
			b2 = b3;
			b3 = buffer;
		}

		public void UnShuffle()
		{
			byte buffer = b3;
			b3 = b2;
			b2 = buffer;
		}
	}
}