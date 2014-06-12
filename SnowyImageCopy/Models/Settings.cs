using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
	public class Settings : NotificationObject
	{
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
				throw new Exception("Failed to read a XML file. {0}", ex);
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
				throw new Exception("Failed to write a XML file. {0}", ex);
			}
		}

		private static T ReadXmlFile<T>(string filePath) where T : new()
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File is not found.", filePath);

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
				AutoCheckInterval = 30,
				WillMoveFileToRecycle = false,
				WillMakeFileExtensionLowerCase = true,
			};
		}


		#region Path

		private readonly Regex rootPattern = new Regex("^http(|s)://.+/$", RegexOptions.Compiled);

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


		#region Instant copy

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

		public bool WillMoveFileToRecycle
		{
			get { return _willMoveFileToRecycle; }
			set
			{
				if (_willMoveFileToRecycle == value)
					return;

				_willMoveFileToRecycle = value;
				RaisePropertyChanged();
			}
		}
		private bool _willMoveFileToRecycle;

		public bool WillMakeFileExtensionLowerCase
		{
			get { return _willMakeFileExtensionLowerCase; }
			set
			{
				if (_willMakeFileExtensionLowerCase == value)
					return;

				_willMakeFileExtensionLowerCase = value;
				RaisePropertyChanged();
			}
		}
		private bool _willMakeFileExtensionLowerCase;

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
