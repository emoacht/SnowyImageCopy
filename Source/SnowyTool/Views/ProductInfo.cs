using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Views
{
	/// <summary>
	/// This application's product information
	/// </summary>
	public static class ProductInfo
	{
		private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
		public static Version Version { get; } = _assembly.GetName().Version;

		#region Assembly attributes

		public static string Title => _title ??= GetAttribute<AssemblyTitleAttribute>(_assembly).Title;
		private static string _title;

		public static string Description => _description ??= GetAttribute<AssemblyDescriptionAttribute>(_assembly).Description;
		private static string _description;

		public static string Company => _company ??= GetAttribute<AssemblyCompanyAttribute>(_assembly).Company;
		private static string _company;

		public static string Product => _product ??= GetAttribute<AssemblyProductAttribute>(_assembly).Product;
		private static string _product;

		public static string Copyright => _copyright ??= GetAttribute<AssemblyCopyrightAttribute>(_assembly).Copyright;
		private static string _copyright;

		public static string Trademark => _trademark ??= GetAttribute<AssemblyTrademarkAttribute>(_assembly).Trademark;
		private static string _trademark;

		private static TAttribute GetAttribute<TAttribute>(Assembly assembly) where TAttribute : Attribute =>
			(TAttribute)Attribute.GetCustomAttribute(assembly, typeof(TAttribute));

		#endregion

		public static string ProductInfoLong => $"{Title} {ProductInfo.Version}";
		public static string ProductInfoMiddle => $"{Title} {ProductInfo.Version.ToString(3)}";
		public static string ProductInfoShort => $"{Title} {ProductInfo.Version.ToString(2)}";
	}
}