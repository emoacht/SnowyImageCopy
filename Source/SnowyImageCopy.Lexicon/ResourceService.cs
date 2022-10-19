using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Lexicon.Properties;

namespace SnowyImageCopy.Lexicon
{
	/// <summary>
	/// Switches this application's Resources (languages).
	/// </summary>
	/// <remarks>
	/// This logic is based on http://grabacr.net/archives/1647
	/// </remarks>
	public class ResourceService : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

		public static ResourceService Current { get; } = new ResourceService();

		private ResourceService()
		{ }

		/// <summary>
		/// Resources to be referred for binding
		/// </summary>
		public Resources Resources { get; } = new Resources();

		/// <summary>
		/// Supported Culture names
		/// </summary>
		private static readonly string[] _supportedCultureNames =
		{
			"en", // Resources.resx
			"ja-JP", // Resources.ja-JP.resx (Only "ja" is not enough to load this resource file)
		};

		/// <summary>
		/// Supported Cultures
		/// </summary>
		public IReadOnlyCollection<CultureInfo> SupportedCultures
		{
			get => _supportedCultures ??= _supportedCultureNames
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
				.Where(x => x is not null)
				.ToArray();
		}
		private CultureInfo[] _supportedCultures;

		/// <summary>
		/// Culture name currently used by this application's Resources
		/// </summary>
		public string CultureName => Resources.Culture?.Name;

		/// <summary>
		/// Changes Culture of this application's Resources by Culture name
		/// </summary>
		/// <param name="cultureName">Culture name</param>
		public void ChangeCulture(string cultureName)
		{
			var culture = SupportedCultures.SingleOrDefault(x => x.Name == cultureName);

			// If culture is null, Culture of this application's Resources will be automatically selected.
			// Since Culture property is static, setting this property will change culture of all instances.
			Resources.Culture = culture;

			// Notify this application's Resources is changed.
			OnPropertyChanged(nameof(Resources));
		}
	}
}