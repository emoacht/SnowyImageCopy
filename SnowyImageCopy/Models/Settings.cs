using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

using SnowyImageCopy.Common;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// This application's settings
	/// </summary>
	public class Settings : NotificationObject, INotifyDataErrorInfo
	{
		#region INotifyDataErrorInfo member

		/// <summary>
		/// Holder of property name (key) and validation error messages (value)
		/// </summary>
		private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

		private bool ValidateProperty(object value, [CallerMemberName] string propertyName = null)
		{
			if (String.IsNullOrEmpty(propertyName))
				return false;

			var context = new ValidationContext(this) { MemberName = propertyName };
			var results = new List<ValidationResult>();
			var isValidated = Validator.TryValidateProperty(value, context, results);

			if (isValidated)
			{
				if (_errors.ContainsKey(propertyName))
					_errors.Remove(propertyName);
			}
			else
			{
				if (_errors.ContainsKey(propertyName))
					_errors[propertyName].Clear();
				else
					_errors[propertyName] = new List<string>();

				_errors[propertyName].AddRange(results.Select(x => x.ErrorMessage));
			}

			RaiseErrorsChanged(propertyName);

			return isValidated;
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		private void RaiseErrorsChanged(string propertyName)
		{
			var handler = this.ErrorsChanged;
			if (handler != null)
				handler(this, new DataErrorsChangedEventArgs(propertyName));
		}

		public IEnumerable GetErrors(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
				return null;

			return _errors[propertyName];
		}

		public bool HasErrors
		{
			get { return _errors.Any(); }
		}

		#endregion

		public static Settings Current { get { return _current; } }
		private static readonly Settings _current = new Settings();

		public WindowPlacement.WINDOWPLACEMENT? Placement { get; set; }

		#region Settings

		#region Path

		public string RemoteAddress
		{
			get { return _remoteAddress; }
			set
			{
				if (_remoteAddress == value)
					return;

				string root;
				string descendant;
				if (!TryParseRemoteAddress(value, out root, out descendant))
					return;

				_remoteAddress = root + descendant;
				_remoteRoot = root;
				_remoteDescendant = "/" + descendant.TrimEnd('/');
			}
		}
		private string _remoteAddress = @"http://flashair/"; // Default FlashAir Url

		public string RemoteRoot
		{
			get { return _remoteRoot ?? _remoteAddress; }
		}
		private string _remoteRoot;

		public string RemoteDescendant
		{
			get { return _remoteDescendant ?? String.Empty; }
		}
		private string _remoteDescendant;

		private static readonly Regex _rootPattern = new Regex(@"^https?://((?!/)\S){1,15}/", RegexOptions.Compiled);

		private bool TryParseRemoteAddress(string source, out string root, out string descendant)
		{
			root = null;
			descendant = null;

			if (Path.GetInvalidPathChars().Any(x => source.Contains(x)))
				return false;

			var match = _rootPattern.Match(source);
			if (!match.Success)
				return false;

			root = match.Value;

			descendant = source.Substring(match.Length).TrimStart('/');
			descendant = Regex.Replace(descendant, @"\s+", String.Empty);
			descendant = Regex.Replace(descendant, "/{2,}", "/");
			return true;
		}

		private const string _defaultLocalFolder = "FlashAirImages"; // Default local folder name

		public string LocalFolder
		{
			get
			{
				return _localFolder ?? (_localFolder =
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), _defaultLocalFolder));
			}
			set
			{
				if (_localFolder == value)
					return;

				_localFolder = value;
				RaisePropertyChanged();
			}
		}
		private string _localFolder;

		#endregion

		#region Date

		public FilePeriod TargetPeriod
		{
			get { return _targetPeriod; }
			set
			{
				if (_targetPeriod == value)
					return;

				_targetPeriod = value;
				RaisePropertyChanged();
			}
		}
		private FilePeriod _targetPeriod = FilePeriod.All; // Default

		public ObservableCollection<DateTime> TargetDates
		{
			get { return _targetDates ?? (_targetDates = new ObservableCollection<DateTime>()); }
			set
			{
				if ((_targetDates != null) && (_targetDates == value))
					return;

				_targetDates = value;

				if (TargetPeriod == FilePeriod.Select) // To prevent loading settings from firing event unnecessarily
					RaisePropertyChanged();
			}
		}
		private ObservableCollection<DateTime> _targetDates;

		#endregion

		#region Current image

		public bool IsCurrentImageVisible
		{
			get { return _isCurrentImageVisible; }
			set
			{
				if (_isCurrentImageVisible == value)
					return;

				_isCurrentImageVisible = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCurrentImageVisible;

		public double CurrentImageWidth
		{
			get { return _currentImageWidth; }
			set
			{
				if (value < ImageManager.ThumbnailSize.Width)
					return;

				_currentImageWidth = value;
				RaisePropertyChanged();
			}
		}
		private double _currentImageWidth = ImageManager.ThumbnailSize.Width;

		#endregion

		#region Dashboard

		public bool InstantCopy
		{
			get { return _instantCopy; }
			set
			{
				if (_instantCopy == value)
					return;

				_instantCopy = value;
				RaisePropertyChanged();
			}
		}
		private bool _instantCopy = true; // Default

		public bool DeleteOnCopy
		{
			get
			{
				if (!EnablesChooseDeleteOnCopy)
					_deleteOnCopy = false;

				return _deleteOnCopy;
			}
			set
			{
				if (_deleteOnCopy == value)
					return;

				_deleteOnCopy = value;
				RaisePropertyChanged();
			}
		}
		private bool _deleteOnCopy;

		#endregion

		#region Auto check

		public int AutoCheckInterval
		{
			get { return _autoCheckInterval; }
			set
			{
				if (_autoCheckInterval == value)
					return;

				_autoCheckInterval = value;
				RaisePropertyChanged();
			}
		}
		private int _autoCheckInterval = 30; // Default

		#endregion

		#region File

		public bool MakesFileExtensionLowercase
		{
			get { return _makesFileExtensionLowercase; }
			set
			{
				if (_makesFileExtensionLowercase == value)
					return;

				_makesFileExtensionLowercase = value;
				RaisePropertyChanged();
			}
		}
		private bool _makesFileExtensionLowercase = true; // Default

		public bool MovesFileToRecycle
		{
			get { return _movesFileToRecycle; }
			set
			{
				if (_movesFileToRecycle == value)
					return;

				_movesFileToRecycle = value;
				RaisePropertyChanged();
			}
		}
		private bool _movesFileToRecycle;

		public bool SelectsReadOnlyFile
		{
			get { return _selectsReadOnlyFile; }
			set
			{
				if (_selectsReadOnlyFile == value)
					return;

				_selectsReadOnlyFile = value;
				RaisePropertyChanged();
			}
		}
		private bool _selectsReadOnlyFile;

		public bool CreatesDatedFolder
		{
			get { return _createsDatedFolder; }
			set
			{
				if (_createsDatedFolder == value)
					return;

				_createsDatedFolder = value;
				RaisePropertyChanged();
			}
		}
		private bool _createsDatedFolder = true; // Default;

		public bool EnablesChooseDeleteOnCopy
		{
			get { return _enablesChooseDeleteOnCopy; }
			set
			{
				if (_enablesChooseDeleteOnCopy == value)
					return;

				_enablesChooseDeleteOnCopy = value;
				RaisePropertyChanged();

				if (!value)
					DeleteOnCopy = false;
			}
		}
		private bool _enablesChooseDeleteOnCopy;

		public bool CopyJpegsOnly
		{
			get { return _copyJpegsOnly; }
			set
			{
				if (_copyJpegsOnly == value)
					return;

				_copyJpegsOnly = value;
				RaisePropertyChanged();
			}
		}
		private bool _copyJpegsOnly = true; // Default

		#endregion

		#region Culture

		public string CultureName
		{
			get { return _cultureName; }
			set
			{
				_cultureName = value;
				RaisePropertyChanged();
			}
		}
		private string _cultureName;

		#endregion

		#endregion

		#region Load/Save

		public static bool IsLoaded { get; private set; }

		private const string _settingsFileName = "settings.xml";
		private static readonly string _settingsFilePath = Path.Combine(FolderPathAppData, _settingsFileName);

		public static void Load()
		{
			try
			{
				if (File.Exists(_settingsFilePath))
				{
					using (var fs = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read))
					{
						var serializer = new XmlSerializer(typeof(Settings));
						var loaded = (Settings)serializer.Deserialize(fs);

						typeof(Settings)
							.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
							.Where(x => x.CanWrite)
							.ToList()
							.ForEach(x => x.SetValue(Current, x.GetValue(loaded)));
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to load settings.", ex);
			}

			IsLoaded = true;
		}

		public static void Save()
		{
			if (!IsLoaded)
				return;

			try
			{
				PrepareFolderAppData();

				using (var fs = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write))
				{
					var serializer = new XmlSerializer(typeof(Settings));
					serializer.Serialize(fs, Current);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to save settings.", ex);
			}
		}

		#endregion

		#region Prepare

		private static string FolderPathAppData
		{
			get
			{
				if (_folderPathAppData == null)
				{
					var pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
					if (string.IsNullOrEmpty(pathAppData)) // This should not happen.
						throw new DirectoryNotFoundException();

					_folderPathAppData = Path.Combine(pathAppData, Assembly.GetExecutingAssembly().GetName().Name);
				}
				return _folderPathAppData;
			}
		}
		private static string _folderPathAppData;

		private static void PrepareFolderAppData()
		{
			if (!Directory.Exists(FolderPathAppData))
				Directory.CreateDirectory(FolderPathAppData);
		}

		#endregion
	}
}