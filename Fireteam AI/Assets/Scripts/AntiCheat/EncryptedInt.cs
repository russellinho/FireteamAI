namespace Koobando.AntiCheat
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct EncryptedInt : IEncryptedType, IFormattable, IEquatable<EncryptedInt>, IComparable<EncryptedInt>, IComparable<int>, IComparable
    {
        [SerializeField]
		private int currentCryptoKey;
        [SerializeField]
        private bool initialized;
        [SerializeField]
        private int hiddenValue;
        [SerializeField]
        private int fakeValue;
        [SerializeField]
        private bool fakeValueActive;

        private EncryptedInt(int i) {
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(i, currentCryptoKey);
#if UNITY_EDITOR
            fakeValue = i;
            fakeValueActive = true;
#else
            bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? i : 0;
            fakeValueActive = detectorRunning;
#endif
            initialized = true;
        }

        public static int Encrypt(int value, int key) {
            return value ^ key;
        }

        public static int Decrypt(int value, int key) {
            return value ^ key;
        }

        public static EncryptedInt FromEncrypted(int encrypted, int key) {
            EncryptedInt instance = new EncryptedInt();
            instance.SetEncrypted(encrypted, key);
            return instance;
        }

        public static int GenerateKey() {
            return BasicUtils.GenerateIntKey();
        }

        public int GetEncrypted(out int key) {
			key = currentCryptoKey;
			return hiddenValue;
		}

        public void SetEncrypted(int encrypted, int key) {
            initialized = true;
            hiddenValue = encrypted;
            currentCryptoKey = key;
            if (EncryptedCheatingDetector.ExistsAndIsRunning) {
                fakeValueActive = false;
                fakeValue = InternalDecrypt();
                fakeValueActive = true;
            } else {
                fakeValueActive = false;
            }
        }

        public int GetDecrypted() {
            return InternalDecrypt();
        }

        public void RandomizeCryptoKey() {
            hiddenValue = InternalDecrypt();
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(hiddenValue, currentCryptoKey);
        }

        private int InternalDecrypt() {
            if (!initialized) {
                currentCryptoKey = GenerateKey();
                hiddenValue = Encrypt(0, currentCryptoKey);
                fakeValue = 0;
                fakeValueActive = false;
                initialized = true;
                return 0;
            }

            int decrypted = Decrypt(hiddenValue, currentCryptoKey);

            if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue) {
                EncryptedCheatingDetector.Instance.OnCheatingDetected();
            }

            return decrypted;
        }

        #region operators, overrides, interface implementations

        public static implicit operator EncryptedInt(int i) {
            return new EncryptedInt(i);
        }

        public static implicit operator int(EncryptedInt i) {
            return i.InternalDecrypt();
        }

        public static implicit operator EncryptedFloat(EncryptedInt i) {
            return i.InternalDecrypt();
        }

        public static implicit operator EncryptedDouble(EncryptedInt i) {
            return i.InternalDecrypt();
        }

        public static explicit operator EncryptedUInt(EncryptedInt i) {
            return (uint)i.InternalDecrypt();
        }

        private static EncryptedInt Increment(EncryptedInt input, int amount) {
            int decrypted = input.InternalDecrypt() + amount;
            input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

            if (EncryptedCheatingDetector.ExistsAndIsRunning) {
                input.fakeValue = decrypted;
                input.fakeValueActive = true;
            } else {
                input.fakeValueActive = false;
            }

            return input;
        }

        public static EncryptedInt operator ++(EncryptedInt i) {
            return Increment(i, 1);
        }

        public static EncryptedInt operator --(EncryptedInt i) {
            return Increment(i, -1);
        }

        public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

        public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}

        public override bool Equals(object obj)
		{
			return obj is EncryptedInt && Equals((EncryptedInt)obj);
		}

		public bool Equals(EncryptedInt obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedInt other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(int other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is int)) throw new ArgumentException("Argument must be int");
			return CompareTo((int)obj);
		}

        #endregion
    }
}
