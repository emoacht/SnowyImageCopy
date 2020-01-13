using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Manages date and time based on FAT file system (MS-DOS date and time).
	/// </summary>
	/// <remarks>
	/// https://docs.microsoft.com/en-us/windows/desktop/sysinfo/ms-dos-date-and-time
	/// </remarks>
	public static class FatDateTime
	{
		/// <summary>
		/// Converts int representing date and int representing time to <see cref="DateTime"/>.
		/// </summary>
		/// <param name="date">Int representing date</param>
		/// <param name="time">Int representing time</param>
		/// <param name="kind">DateTimeKind</param>
		/// <returns>DateTime</returns>
		/// <remarks>
		/// <para>
		/// Date format:
		/// Bits 0–4: Day
		/// Bits 5–8: Month
		/// Bits 9–15: Count of years from 1980
		/// Time format:
		/// Bits 0–4: Count of 2 seconds
		/// Bits 5–10: Minute
		/// Bits 11–15: Hour
		/// </para>
		/// The structure of MS-DOS date and time is shown at
		/// https://docs.microsoft.com/en-us/windows/desktop/sysinfo/ms-dos-date-and-time
		/// </remarks>
		public static DateTime ConvertFromDateIntAndTimeIntToDateTime(int date, int time, DateTimeKind kind)
		{
			if ((date <= 0) || (time < 0))
				return default;

			var baDate = new BitArray(new[] { date });
			var baTime = new BitArray(new[] { time });

			var year = ConvertFromBitArrayToInt(baDate, 9, count: 7) + 1980;
			var month = ConvertFromBitArrayToInt(baDate, 5, count: 4);
			var day = ConvertFromBitArrayToInt(baDate, 0, count: 5);
			var hour = ConvertFromBitArrayToInt(baTime, 11, count: 5);
			var minute = ConvertFromBitArrayToInt(baTime, 5, count: 6);
			var second = ConvertFromBitArrayToInt(baTime, 0, count: 5) * 2;

			try
			{
				return new DateTime(year, month, day, hour, minute, second, kind);
			}
			catch (ArgumentOutOfRangeException)
			{
				// If time stamp of source folder or file is weird, date or time parameters may become invalid
				// and so this exception will be thrown.
				return default;
			}
		}

		/// <summary>
		/// Converts <see cref="DateTime"/> to int representing date.
		/// </summary>
		/// <param name="dateTime">DateTime</param>
		/// <returns>Int representing date</returns>
		public static int ConvertFromDateTimeToDateInt(DateTime dateTime)
		{
			var baDate = new BitArray(16);

			var baYear = new BitArray(new[] { dateTime.Year - 1980 });
			var baMonth = new BitArray(new[] { dateTime.Month });
			var baDay = new BitArray(new[] { dateTime.Day });

			Copy(baYear, 0, baDate, 9, count: 7);
			Copy(baMonth, 0, baDate, 5, count: 4);
			Copy(baDay, 0, baDate, 0, count: 5);

			return ConvertFromBitArrayToInt(baDate);
		}

		/// <summary>
		/// Converts <see cref="DateTime"/> to int representing time.
		/// </summary>
		/// <param name="dateTime">DateTime</param>
		/// <returns>Int representing time</returns>
		public static int ConvertFromDateTimeToTimeInt(DateTime dateTime)
		{
			var baTime = new BitArray(16);

			var baHour = new BitArray(new[] { dateTime.Hour });
			var baMinute = new BitArray(new[] { dateTime.Minute });
			var baSecond = new BitArray(new[] { dateTime.Second / 2 });

			Copy(baHour, 0, baTime, 11, count: 5);
			Copy(baMinute, 0, baTime, 5, count: 6);
			Copy(baSecond, 0, baTime, 0, count: 5);

			return ConvertFromBitArrayToInt(baTime);
		}

		#region Helper

		/// <summary>
		/// Converts BitArray to int by BitArray's index.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <param name="index">Start index of source BitArray</param>
		/// <param name="count">Number of elements to convert</param>
		/// <returns>Int</returns>
		private static int ConvertFromBitArrayToInt(BitArray source, int index, int count)
		{
			var destination = new BitArray(count);
			Copy(source, index, destination, 0, count);
			return ConvertFromBitArrayToInt(destination);
		}

		/// <summary>
		/// Converts BitArray to int.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <returns>Int</returns>
		private static int ConvertFromBitArrayToInt(BitArray source)
		{
			var target = new int[1];
			source.CopyTo(target, 0);
			return target[0];
		}

		/// <summary>
		/// Copies a range of elements from BitArray to BitArray.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <param name="sourceIndex">Start index of source BitArray</param>
		/// <param name="destination">Destination BitArray</param>
		/// <param name="destinationIndex">Start index of destination BitArray</param>
		/// <param name="count">Number of elements to copy</param>
		private static void Copy(BitArray source, int sourceIndex, BitArray destination, int destinationIndex, int count)
		{
			for (int i = sourceIndex, j = destinationIndex;
				(i < source.Length) && (i < sourceIndex + count) && (j < destination.Length);
				i++, j++)
			{
				destination[j] = source[i];
			}
		}

		#endregion
	}
}