using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using SnowyImageCopy.Models;
using SnowyImageCopy.Properties;

namespace SnowyImageCopy.Views.Converters
{
	/// <summary>
	/// Converts FileStatus to corresponding string.
	/// </summary>
	[ValueConversion(typeof(FileStatus), typeof(string))]
	public class FileStatusToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is FileStatus))
				return DependencyProperty.UnsetValue;

			switch ((FileStatus)value)
			{
				case FileStatus.NotCopied:
					return Resources.FileStatus_NotCopied;
				case FileStatus.ToBeCopied:
					return Resources.FileStatus_ToBeCopied;
				case FileStatus.Copying:
					return Resources.FileStatus_Copying;
				case FileStatus.Copied:
					return Resources.FileStatus_Copied;
				case FileStatus.Weird:
					return Resources.FileStatus_Weird;
				case FileStatus.Recycled:
					return Resources.FileStatus_Recycled;
				default: // FileStatus.Unknown
					return Resources.FileStatus_Unknown;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}