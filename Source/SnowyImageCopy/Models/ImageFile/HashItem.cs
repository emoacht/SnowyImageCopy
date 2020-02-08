using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.ImageFile
{
	/// <summary>
	/// Container for hash
	/// </summary>
	/// <remarks>This class should be immutable.</remarks>
	internal class HashItem : IComparable<HashItem>
	{
		private static readonly HashAlgorithm _algorithm;

		/// <summary>
		/// Size (bytes) of underlying value 
		/// </summary>
		public static int Size { get; }

		static HashItem()
		{
			try
			{
				_algorithm = new MD5CryptoServiceProvider(); // MD5 is for less cost.
			}
			catch (InvalidOperationException)
			{
				_algorithm = new SHA1CryptoServiceProvider();
			}

			Size = _algorithm.HashSize / 8;
		}

		#region Constructor

		private readonly byte[] _value;

		private HashItem(byte[] value)
		{
			if (!(value?.Length == Size))
				throw new ArgumentException(nameof(value));

			this._value = value;
		}

		public static HashItem Compute(IEnumerable<byte> source)
		{
			var buff = source as byte[] ?? source?.ToArray() ?? throw new ArgumentNullException(nameof(source));

			return new HashItem(_algorithm.ComputeHash(buff));
		}

		public static HashItem Restore(byte[] source)
		{
			return new HashItem(source?.ToArray());
		}

		#endregion

		public byte[] ToByteArray() => _value.ToArray();

		public override string ToString() => BitConverter.ToString(_value);

		#region IComparable member

		public int CompareTo(HashItem other)
		{
			if (other is null)
				return 1;

			if (object.ReferenceEquals(this, other))
				return 0;

			for (int i = 0; i < Size; i++)
			{
				int comparison = this._value[i].CompareTo(other._value[i]);
				if (comparison != 0)
					return comparison;
			}

			return 0;
		}

		public override bool Equals(object obj) => this.Equals(obj as HashItem);

		public bool Equals(HashItem other)
		{
			if (other is null)
				return false;

			if (object.ReferenceEquals(this, other))
				return true;

			return this._value.SequenceEqual(other._value);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				foreach (byte element in this._value)
					hash = hash * 31 + element.GetHashCode();

				return hash;
			}
		}

		#endregion
	}
}