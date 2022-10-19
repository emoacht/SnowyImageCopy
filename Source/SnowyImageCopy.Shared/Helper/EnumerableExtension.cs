using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Extension methods for <see cref="IEnumerable{T}"/> 
	/// </summary>
	public static class EnumerableExtension
	{
		/// <summary>
		/// Returns the index of the first occurrence of an item in a sequence based on a predicate.
		/// </summary>
		/// <typeparam name="T">The object stored in the sequence</typeparam>
		/// <param name="source">The sequence</param>
		/// <param name="predicate">Function to test each item for a condition</param>
		/// <returns>The index of an item</returns>
		public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			int index = 0;
			foreach (T item in source)
			{
				if (predicate(item))
					return index;

				index++;
			}
			return -1;
		}
	}
}