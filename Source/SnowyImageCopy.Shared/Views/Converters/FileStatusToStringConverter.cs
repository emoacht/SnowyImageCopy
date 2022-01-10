using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using SnowyImageCopy.Lexicon.Properties;
using SnowyImageCopy.Models.ImageFile;

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
			if (value is not FileStatus status)
				return DependencyProperty.UnsetValue;

			return status switch
			{
				FileStatus.NotCopied => Resources.FileStatus_NotCopied,
				FileStatus.ToBeCopied => Resources.FileStatus_ToBeCopied,
				FileStatus.Copying => Resources.FileStatus_Copying,
				FileStatus.Copied => Resources.FileStatus_Copied,
				FileStatus.OnceCopied => Resources.FileStatus_OnceCopied,
				FileStatus.Weird => Resources.FileStatus_Weird,
				FileStatus.Recycled => Resources.FileStatus_Recycled,
				_ => Resources.FileStatus_Unknown,
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}