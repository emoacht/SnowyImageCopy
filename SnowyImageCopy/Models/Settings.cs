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
	/// This application's settings.
	/// </summary>
	public class Settings : NotificationObject, INotifyDataErrorInfo
	{
		#region INotifyDataErrorInfo member

		/// <summary>
		/// Holder of property name (key) and validation error messages (value)
		/// </summary>
		private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

		private bool ValidateProperty(object value, [CallerMemberName]string propertyName = null)
		{
			if (String.IsNullOrEmpty(propertyName))
				return false;

			var context = new ValidationContext(this) { MemberName = propertyName };
			var results = new List<ValidationResult>();
			var isValidated = Validator.TryValidateProperty(value, context, results);

			if (isValidated)
			{
				if (errors.ContainsKey(propertyName))
					errors.Remove(propertyName);
			}
			else
			{
				if (errors.ContainsKey(propertyName))
					errors[propertyName].Clear();
				else
					errors[propertyName] = new List<string>();

				errors[propertyName].AddRange(results.Select(x => x.ErrorMessage));
			}

			RaiseErrorsChanged(propertyName);

			return isValidated;
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		private void RaiseErrorsChanged(string propertyName)
		{
			var handler = this.ErrorsChanged;
			if (handler != null)
			{
				handler(this, new DataErrorsChangedEventArgs(propertyName));
			}
		}

		public IEnumerable GetErrors(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName) || !errors.ContainsKey(propertyName))
				return null;

			return errors[propertyName];
		}

		public bool HasErrors
		{
			get { return errors.Any(); }
		}

		#endregion


		public static Settings Current { get; set; }


		#region Lode/Save

		private const string settingsFile = "settings.xml";

		private static readonly string settingsPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			Assembly.GetExecutingAssembly().GetName().Name,
			settingsFile);

		public static void Load()
		{
			try
			{
				Current = ReadXmlFile<Settings>(settingsPath);
			}
			catch (FileNotFoundException)
			{
				// This exception is normal when this application runs the first time and so no previous settings file exists.
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to read a XML file.", ex);
			}

			if (Current == null)
				Current = GetDefaultSettings();
		}

		public static void Save()
		{
			try
			{
				WriteXmlFile<Settings>(Current, settingsPath);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to write a XML file.", ex);
			}
		}

		private static T ReadXmlFile<T>(string filePath) where T : new()
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File seems missing.", filePath);

			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(fs);
			}
		}

		private static void WriteXmlFile<T>(T data, string filePath) where T : new()
		{
			var folderPath = Path.GetDirectoryName(filePath);
			if (!String.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			using (var fs = new FileStream(filePath, FileMode.Create))
			{
				var serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(fs, data);
			}
		}

		#endregion


		#region Window Placement

		public WindowPlacement.WINDOWPLACEMENT? Placement;

		#endregion


		#region Settings

		private static Settings GetDefaultSettings()
		{
			return new Settings()
			{
				TargetPeriod = FilePeriod.All,
				IsCurrentImageVisible = false,
				InstantCopy = true,
				DeleteUponCopy = false,
				AutoCheckInterval = 30,
				MakesFileExtensionLowerCase = true,
				MovesFileToRecycle = false,
				EnablesChooseDeleteUponCopy = false,
			};
		}


		#region Path

		private readonly Regex rootPattern = new Regex(@"^https?://\S{1,15}/$", RegexOptions.Compiled);

		public string RemoteRoot
		{
			get { return _remoteRoot; }
			set
			{
				if (_remoteRoot == value)
					return;

				if (rootPattern.IsMatch(value))
					_remoteRoot = value;
			}
		}
		private string _remoteRoot = @"http://flashair/"; // Default FlashAir Url

		private const string defaultLocalFolder = "FlashAirImages"; // Default local folder name

		public string LocalFolder
		{
			get
			{
				if (String.IsNullOrEmpty(_localFolder))
					_localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), defaultLocalFolder);

				return _localFolder;
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
		private FilePeriod _targetPeriod = FilePeriod.All;

		public ObservableCollection<DateTime> TargetDates
		{
			get
			{
				return _targetDates ?? (_targetDates = new ObservableCollection<DateTime>());
			}
			set
			{
				if ((_targetDates != null) && (_targetDates == value))
					return;

				_targetDates = value;
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
		private bool _instantCopy;

		public bool DeleteUponCopy
		{
			get
			{
				if (!EnablesChooseDeleteUponCopy)
					_deleteUponCopy = false;

				return _deleteUponCopy;
			}
			set
			{
				if (_deleteUponCopy == value)
					return;

				_deleteUponCopy = value;
				RaisePropertyChanged();
			}
		}
		private bool _deleteUponCopy;

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
		private int _autoCheckInterval;

		#endregion


		#region File

		public bool MakesFileExtensionLowerCase
		{
			get { return _makesFileExtensionLowerCase; }
			set
			{
				if (_makesFileExtensionLowerCase == value)
					return;

				_makesFileExtensionLowerCase = value;
				RaisePropertyChanged();
			}
		}
		private bool _makesFileExtensionLowerCase;

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

		public bool EnablesChooseDeleteUponCopy
		{
			get { return _enablesChooseDeleteUponCopy; }
			set
			{
				if (_enablesChooseDeleteUponCopy == value)
					return;

				_enablesChooseDeleteUponCopy = value;
				RaisePropertyChanged();

				if (!value)
					DeleteUponCopy = false;
			}
		}
		private bool _enablesChooseDeleteUponCopy;

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
	}
}