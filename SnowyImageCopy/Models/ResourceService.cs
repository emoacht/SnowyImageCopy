using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Properties;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Switch this application's Resources (languages).
	/// </summary>
	/// <remarks>
	/// This logic is based on http://grabacr.net/archives/1647
	/// </remarks>
	public class ResourceService : NotificationObject
	{
		private ResourceService()
		{ }

		public static ResourceService Current
		{
			get { return _current; }
		}
		private static readonly ResourceService _current = new ResourceService();

		/// <summary>
		/// Resources to be referred for binding
		/// </summary>
		public Resources Resources
		{
			get { return _resources; }
		}
		private readonly Resources _resources = new Resources();

		/// <summary>
		/// Supported Culture names
		/// </summary>
		private readonly string[] supportedCultureNames =
		{
			"en", // Resources.resx
			"ja-JP", // Resources.ja-JP.resx (Only "ja" is not enough to load this resource file)
		};

		/// <summary>
		/// Supported Cultures
		/// </summary>
		public IReadOnlyCollection<CultureInfo> SupportedCultures
		{
			get
			{
				if (_supportedCultures == null)
				{
					_supportedCultures = supportedCultureNames
						.Select(x =>
						{
							try
							{
								return CultureInfo.GetCultureInfo(x);
							}
							catch (CultureNotFoundException)
							{
								return null;
							}
						})
						.Where(x => x != null)
						.ToList();
				}

				return _supportedCultures;
			}
		}
		private IReadOnlyCollection<CultureInfo> _supportedCultures;

		/// <summary>
		/// Change Culture of this application's Resources by Culture name
		/// </summary>
		/// <param name="cultureName">Culture name</param>
		public void ChangeCulture(string cultureName)
		{
			var culture = SupportedCultures.SingleOrDefault(x => x.Name == cultureName);

			// If culture is null, Culture of this application's Resources will be automatically selected. 
			Resources.Culture = culture;

			// Notify this application's Resources is changed.
			RaisePropertyChanged("Resources");
		}
	}
}
