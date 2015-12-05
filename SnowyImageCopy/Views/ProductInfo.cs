using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Views
{
	/// <summary>
	/// This application's product information
	/// </summary>
	public static class ProductInfo
	{
		private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
		public static readonly Version Version = _assembly.GetName().Version;

		#region Assembly attributes

		public static string Title
		{
			get
			{
				if (String.IsNullOrEmpty(_title))
					_title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute))).Title;

				return _title;
			}
		}
		private static string _title;

		public static string Description
		{
			get
			{
				if (String.IsNullOrEmpty(_description))
					_description = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute))).Description;

				return _description;
			}
		}
		private static string _description;

		public static string Company
		{
			get
			{
				if (String.IsNullOrEmpty(_company))
					_company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCompanyAttribute))).Company;

				return _company;
			}
		}
		private static string _company;

		public static string Product
		{
			get
			{
				if (String.IsNullOrEmpty(_product))
					_product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyProductAttribute))).Product;

				return _product;
			}
		}
		private static string _product;

		public static string Copyright
		{
			get
			{
				if (String.IsNullOrEmpty(_copyright))
					_copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCopyrightAttribute))).Copyright;

				return _copyright;
			}
		}
		private static string _copyright;

		public static string Trademark
		{
			get
			{
				if (String.IsNullOrEmpty(_trademark))
					_trademark = ((AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTrademarkAttribute))).Trademark;

				return _trademark;
			}
		}
		private static string _trademark;

		#endregion

		public static string ProductInfoLong
		{
			get { return String.Format("{0} {1}", Title, Version); }
		}

		public static string ProductInfoMiddle
		{
			get { return String.Format("{0} {1}", Title, Version.ToString(3)); }
		}

		public static string ProductInfoShort
		{
			get { return String.Format("{0} {1}", Title, Version.ToString(2)); }
		}
	}
}