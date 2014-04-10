using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Views
{
	public static class ProductInfo
	{
		private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
		public static readonly Version Version = assembly.GetName().Version;


		#region Assembly attributes

		public static string Title
		{
			get
			{
				if (String.IsNullOrEmpty(_productTitle))
					_productTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;

				return _productTitle;
			}
		}
		private static string _productTitle;

		public static string Description
		{
			get
			{
				if (String.IsNullOrEmpty(_description))
					_description = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;

				return _description;
			}
		}
		private static string _description;

		public static string Company
		{
			get
			{
				if (String.IsNullOrEmpty(_company))
					_company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute))).Company;

				return _company;
			}
		}
		private static string _company;

		public static string Product
		{
			get
			{
				if (String.IsNullOrEmpty(_product))
					_product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;

				return _product;
			}
		}
		public static string _product;

		public static string Copyright
		{
			get
			{
				if (String.IsNullOrEmpty(_copyright))
					_copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute))).Copyright;

				return _copyright;
			}
		}
		private static string _copyright;

		public static string Trademark
		{
			get
			{
				if (String.IsNullOrEmpty(_trademark))
					_trademark = ((AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTrademarkAttribute))).Trademark;

				return _trademark;
			}
		}
		private static string _trademark;

		#endregion


		public static string ProductInfoLong
		{
			get { return String.Format("{0} {1}", Title, Version); }
		}

		public static string ProductInfoShort
		{
			get { return String.Format("{0} {1}.{2}", Title, Version.Major, Version.Minor); }
		}
	}
}
