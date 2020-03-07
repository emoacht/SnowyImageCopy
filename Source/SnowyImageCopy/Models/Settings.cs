using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models.ImageFile;

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

		public bool HasErrors => _errors.Any();

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public IEnumerable GetErrors(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
				return null;

			return _errors[propertyName];
		}

		private bool ValidateProperty(object value, [CallerMemberName] string propertyName = null)
		{
			if (string.IsNullOrEmpty(propertyName))
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

		private void RaiseErrorsChanged(string propertyName) =>
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

		#endregion

		public static Settings Current { get; } = new Settings();

		private Settings()
		{ }

		#region Settings

		#region Path

		public string RemoteAddress
		{
			get => _remoteAddress;
			set
			{
				if (_remoteAddress == value)
					return;

				if (!TryParseRemoteAddress(value, out var root, out var descendant))
					return;

				_remoteAddress = root + descendant;
				_remoteRoot = root;
				_remoteDescendant = "/" + descendant.TrimEnd('/');
				RaisePropertyChanged();
			}
		}
		private string _remoteAddress = @"http://flashair/"; // Default FlashAir Url

		public string RemoteRoot => _remoteRoot ?? _remoteAddress;
		private string _remoteRoot;

		public string RemoteDescendant => _remoteDescendant ?? string.Empty;
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
			descendant = Regex.Replace(descendant, @"\s+", string.Empty);
			descendant = Regex.Replace(descendant, "/{2,}", "/");
			return true;
		}

		private const string DefaultLocalFolder = "FlashAirImages"; // Default local folder name

		public string LocalFolder
		{
			get
			{
				if (string.IsNullOrEmpty(_localFolder))
					_localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), DefaultLocalFolder);

				return _localFolder;
			}
			set
			{
				string buff;
				if (string.IsNullOrEmpty(value))
				{
					buff = value;
				}
				else if (!PathAddition.TryNormalizePath(value, out buff))
					return;

				SetPropertyValue(ref _localFolder, buff);
				CheckLocalFolderValid();
			}
		}
		private string _localFolder;

		[XmlIgnore]
		public bool IsLocalFolderValid
		{
			get => _isLocalFolderValid;
			private set => SetPropertyValue(ref _isLocalFolderValid, value);
		}
		private bool _isLocalFolderValid = true;

		internal bool CheckLocalFolderValid()
		{
			return IsLocalFolderValid = Directory.Exists(Path.GetPathRoot(LocalFolder));
		}

		#endregion

		#region Date

		public FilePeriod TargetPeriod
		{
			get => _targetPeriod;
			set => SetPropertyValue(ref _targetPeriod, value);
		}
		private FilePeriod _targetPeriod = FilePeriod.All; // Default

		public ObservableCollection<DateTime> TargetDates
		{
			get => _targetDates ??= new ObservableCollection<DateTime>();
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
			get => _isCurrentImageVisible;
			set => SetPropertyValue(ref _isCurrentImageVisible, value);
		}
		private bool _isCurrentImageVisible;

		public double CurrentImageWidth
		{
			get => _currentImageWidth;
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
			get => _instantCopy;
			set => SetPropertyValue(ref _instantCopy, value);
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
			set => SetPropertyValue(ref _deleteOnCopy, value);
		}
		private bool _deleteOnCopy;

		#endregion

		#region Connection

		// XmlSerializer cannot work with TimeSpan.
		public int AutoCheckInterval
		{
			get => _autoCheckInterval;
			set => SetPropertyValue(ref _autoCheckInterval, value);
		}
		private int _autoCheckInterval = 30; // Default

		public int TimeoutDuration
		{
			get => _timeoutDuration;
			set => SetPropertyValue(ref _timeoutDuration, value);
		}
		private int _timeoutDuration = 10; // Default

		#endregion

		#region File

		public bool OrdersFromNewer
		{
			get => !FileItem.OrderByAscendingDate;
			set
			{
				if (FileItem.OrderByAscendingDate != value)
					return;

				FileItem.OrderByAscendingDate = !value;
				RaisePropertyChanged();
			}
		}

		public bool MakesFileExtensionLowercase
		{
			get => _makesFileExtensionLowercase;
			set => SetPropertyValue(ref _makesFileExtensionLowercase, value);
		}
		private bool _makesFileExtensionLowercase = true; // Default

		#region Dated folder

		public bool CreatesDatedFolder
		{
			get => _createsDatedFolder;
			set => SetPropertyValue(ref _createsDatedFolder, value);
		}
		private bool _createsDatedFolder = true; // Default;

		[XmlIgnore]
		public bool CustomizesDatedFolder
		{
			get => _customizesDatedFolder || (CustomDatedFolder != null);
			set
			{
				if (_customizesDatedFolder == value)
					return;

				_customizesDatedFolder = value;
				if (!value)
					CustomDatedFolder = null;
			}
		}
		private bool _customizesDatedFolder;

		[XmlIgnore]
		public string DatedFolder
		{
			get => CustomDatedFolder ?? DefaultDatedFolder;
			set
			{
				var buffer = value?.Trim();
				if (string.IsNullOrEmpty(buffer) ||
					string.Equals(buffer, DefaultDatedFolder, StringComparison.OrdinalIgnoreCase))
				{
					CustomDatedFolder = null;
				}
				else if (IsDatedFolderValid(buffer))
				{
					CustomDatedFolder = buffer;
				}
			}
		}

		public string CustomDatedFolder
		{
			get => _customDatedFolder;
			set
			{
				SetPropertyValue(ref _customDatedFolder, value);
				RaisePropertyChanged(nameof(DatedFolder));
			}
		}
		private string _customDatedFolder;

		private const string DefaultDatedFolder = "yyyyMMdd";

		private static bool IsDatedFolderValid(string format)
		{
			// year:  y{1,4}
			// month: M{1,4}
			// day:   d{1,2}
			// delimiter: [-_]?
			const string datePattern = "^(?:y{1,4}[-_]?M{1,4}[-_]?d{1,2}|d{1,2}[-_]?M{1,4}[-_]?y{1,4})$";

			return new Regex(datePattern).IsMatch(format ?? string.Empty);
		}

		#endregion

		public bool HandlesJpegFileOnly
		{
			get => _handlesJpegFileOnly;
			set => SetPropertyValue(ref _handlesJpegFileOnly, value);
		}
		private bool _handlesJpegFileOnly;

		public bool SelectsReadOnlyFile
		{
			get => _selectsReadOnlyFile;
			set => SetPropertyValue(ref _selectsReadOnlyFile, value);
		}
		private bool _selectsReadOnlyFile;

		public bool SkipsOnceCopiedFile
		{
			get => _skipsOnceCopiedFile;
			set
			{
				SetPropertyValue(ref _skipsOnceCopiedFile, value);

				if (!value)
					Signatures.Clear();
			}
		}
		private bool _skipsOnceCopiedFile;

		public int OnceCopiedCapacity
		{
			get => Signatures.MaxCount;
			set
			{
				if (Signatures.MaxCount == value)
					return;

				Signatures.MaxCount = value;
				RaisePropertyChanged();
			}
		}

		public bool MovesFileToRecycle
		{
			get => _movesFileToRecycle;
			set => SetPropertyValue(ref _movesFileToRecycle, value);
		}
		private bool _movesFileToRecycle;

		public bool EnablesChooseDeleteOnCopy
		{
			get => _enablesChooseDeleteOnCopy;
			set
			{
				SetPropertyValue(ref _enablesChooseDeleteOnCopy, value);

				if (!value)
					DeleteOnCopy = false;
			}
		}
		private bool _enablesChooseDeleteOnCopy;

		#endregion

		#region Sound

		public bool PlaysSound
		{
			get => SoundManager.PlaysSound;
			set
			{
				if (SoundManager.PlaysSound == value)
					return;

				SoundManager.PlaysSound = value;
				RaisePropertyChanged();
			}
		}

		public string CopyStarted
		{
			get => SoundManager.CopyStarted.Path;
			set
			{
				SoundManager.CopyStarted.Path = value;
				RaisePropertyChanged();
			}
		}

		public void TestCopyStarted() => SoundManager.CopyStarted.Play();

		public string OneCopied
		{
			get => SoundManager.OneCopied.Path;
			set
			{
				SoundManager.OneCopied.Path = value;
				RaisePropertyChanged();
			}
		}

		public void TestOneCopied() => SoundManager.OneCopied.Play();

		public string AllCopied
		{
			get => SoundManager.AllCopied.Path;
			set
			{
				SoundManager.AllCopied.Path = value;
				RaisePropertyChanged();
			}
		}

		public void TestAllCopied() => SoundManager.AllCopied.Play();

		#endregion

		#region Culture

		public string CultureName
		{
			get => _cultureName;
			set => SetPropertyValue(ref _cultureName, value);
		}
		private string _cultureName;

		#endregion

		#endregion

		#region Start/Stop

		private IDisposable _subscription;

		public void Start()
		{
			Load(Current);

			_subscription = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
				handler => handler.Invoke,
				handler => PropertyChanged += handler,
				handler => PropertyChanged -= handler)
				.Throttle(TimeSpan.FromSeconds(1))
				.Subscribe(_ => Save(Current));
		}

		public void Stop()
		{
			_subscription.Dispose();
		}

		private const string SettingsFileName = "settings.xml";
		private static readonly string _settingsFilePath = Path.Combine(FolderService.AppDataFolderPath, SettingsFileName);

		private static void Load<T>(T instance) where T : class
		{
			var fileInfo = new FileInfo(_settingsFilePath);
			if (!fileInfo.Exists || (fileInfo.Length == 0))
				return;

			try
			{
				using (var fs = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read))
				{
					var serializer = new XmlSerializer(typeof(T));
					var loaded = (T)serializer.Deserialize(fs);

					typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
						.Where(x => x.CanWrite)
						.ToList()
						.ForEach(x => x.SetValue(instance, x.GetValue(loaded)));
				}
			}
			catch (Exception ex)
			{
				try
				{
					File.Delete(_settingsFilePath);
				}
				catch
				{ }

				throw new Exception("Failed to load settings.", ex);
			}
		}

		private static void Save<T>(T instance) where T : class
		{
			try
			{
				FolderService.AssureAppDataFolder();

				using (var fs = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write))
				{
					var serializer = new XmlSerializer(typeof(T));
					serializer.Serialize(fs, instance);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to save settings.", ex);
			}
		}

		#endregion
	}
}