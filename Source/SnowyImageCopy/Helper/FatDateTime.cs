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
				return default(DateTime);

			var baDate = new BitArray(new[] { date });
			var baTime = new BitArray(new[] { time });

			var year = ConvertFromBitArrayToInt(baDate, 9, 15) + 1980;
			var month = ConvertFromBitArrayToInt(baDate, 5, 8);
			var day = ConvertFromBitArrayToInt(baDate, 0, 4);
			var hour = ConvertFromBitArrayToInt(baTime, 11, 15);
			var minute = ConvertFromBitArrayToInt(baTime, 5, 10);
			var second = ConvertFromBitArrayToInt(baTime, 0, 4) * 2;

			try
			{
				return new DateTime(year, month, day, hour, minute, second, kind);
			}
			catch (ArgumentOutOfRangeException)
			{
				// If time stamp of source folder or file is weird, date or time parameters may become invalid
				// and so this exception will be thrown.
				return default(DateTime);
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

			CopyByTargetIndex(baYear, ref baDate, 9, 15);
			CopyByTargetIndex(baMonth, ref baDate, 5, 8);
			CopyByTargetIndex(baDay, ref baDate, 0, 4);

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

			CopyByTargetIndex(baHour, ref baTime, 11, 15);
			CopyByTargetIndex(baMinute, ref baTime, 5, 10);
			CopyByTargetIndex(baSecond, ref baTime, 0, 4);

			return ConvertFromBitArrayToInt(baTime);
		}

		#region Helper

		/// <summary>
		/// Converts BitArray to int by BitArray's index.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <param name="startIndex">Start index of source BitArray</param>
		/// <param name="endIndex">End index of source BitArray</param>
		/// <returns>Int</returns>
		private static int ConvertFromBitArrayToInt(BitArray source, int startIndex, int endIndex)
		{
			var target = new BitArray(endIndex - startIndex + 1);
			CopyBySourceIndex(source, ref target, startIndex, endIndex);
			return ConvertFromBitArrayToInt(target);
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
		/// Copies bits from BitArray to BitArray by source BitArray's index.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <param name="target">Target BitArray</param>
		/// <param name="startSourceIndex">Start index of source BitArray</param>
		/// <param name="endSourceIndex">End index of source BitArray</param>
		private static void CopyBySourceIndex(BitArray source, ref BitArray target, int startSourceIndex, int endSourceIndex)
		{
			for (int i = startSourceIndex, j = 0;
				(i < source.Length) && (i <= endSourceIndex) && (j < target.Length);
				i++, j++)
			{
				target[j] = source[i];
			}
		}

		/// <summary>
		/// Copies bits from BitArray to BitArray by target BitArray's index.
		/// </summary>
		/// <param name="source">Source BitArray</param>
		/// <param name="target">Target BitArray</param>
		/// <param name="startTargetIndex">Start index of target BitArray</param>
		/// <param name="endTargetIndex">End index of target BitArray</param>
		private static void CopyByTargetIndex(BitArray source, ref BitArray target, int startTargetIndex, int endTargetIndex)
		{
			for (int i = 0, j = startTargetIndex;
				(i < source.Length) && (j < target.Length) && (j <= endTargetIndex);
				i++, j++)
			{
				target[j] = source[i];
			}
		}

		#endregion
	}
}