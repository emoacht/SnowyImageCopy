using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Views.Converters;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class EnumConverterTest
	{
		private enum TestEnum { None, One, Two }

		#region EnumToVisibilityConverter

		[TestMethod]
		public void TestEnumToVisibilityConverter()
		{
			TestEnumToVisibilityConverterConvert(TestEnum.Two, nameof(TestEnum.Two), Visibility.Visible);
			TestEnumToVisibilityConverterConvert(TestEnum.One, nameof(TestEnum.One).ToLower(), Visibility.Visible);
			TestEnumToVisibilityConverterConvert(TestEnum.One, nameof(TestEnum.Two), Visibility.Collapsed);
			TestEnumToVisibilityConverterConvert(TestEnum.One, string.Empty, DependencyProperty.UnsetValue);
			TestEnumToVisibilityConverterConvert(null, nameof(TestEnum.One), DependencyProperty.UnsetValue);
		}

		private static EnumToVisibilityConverter _enumToVisibilityConverter;

		private static void TestEnumToVisibilityConverterConvert(object value, object parameter, object expected)
		{
			_enumToVisibilityConverter ??= new EnumToVisibilityConverter();
			var actual = _enumToVisibilityConverter.Convert(value, typeof(TestEnum), parameter, CultureInfo.InvariantCulture);
			Assert.AreEqual(expected, actual);
		}

		#endregion

		#region EnumToBooleanConverter

		[TestMethod]
		public void TestEnumToBooleanConverter()
		{
			TestEnumToBooleanConverterConvert(TestEnum.Two, nameof(TestEnum.Two), true);
			TestEnumToBooleanConverterConvert(TestEnum.One, nameof(TestEnum.One).ToUpper(), true);
			TestEnumToBooleanConverterConvert(default(TestEnum), nameof(TestEnum.None), true);
			TestEnumToBooleanConverterConvert(TestEnum.One, nameof(TestEnum.Two), false);
			TestEnumToBooleanConverterConvert(TestEnum.One, null, DependencyProperty.UnsetValue);
			TestEnumToBooleanConverterConvert(string.Empty, nameof(TestEnum.One), DependencyProperty.UnsetValue);

			TestEnumToBooleanConverterConvertBack(true, nameof(TestEnum.Two), TestEnum.Two);
			TestEnumToBooleanConverterConvertBack("true", nameof(TestEnum.Two), DependencyProperty.UnsetValue);
			TestEnumToBooleanConverterConvertBack(false, nameof(TestEnum.Two), DependencyProperty.UnsetValue);
			TestEnumToBooleanConverterConvertBack(true, null, DependencyProperty.UnsetValue);
		}

		private static EnumToBooleanConverter _enumToBooleanConverter;

		private static void TestEnumToBooleanConverterConvert(object value, object parameter, object expected)
		{
			_enumToBooleanConverter ??= new EnumToBooleanConverter();
			var actual = _enumToBooleanConverter.Convert(value, typeof(TestEnum), parameter, CultureInfo.InvariantCulture);
			Assert.AreEqual(expected, actual);
		}

		private static void TestEnumToBooleanConverterConvertBack(object value, object parameter, object expected)
		{
			_enumToBooleanConverter ??= new EnumToBooleanConverter();
			var actual = _enumToBooleanConverter.ConvertBack(value, typeof(TestEnum), parameter, CultureInfo.InvariantCulture);
			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}