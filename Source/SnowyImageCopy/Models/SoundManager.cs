using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Plays wave sound.
	/// </summary>
	internal class SoundManager
	{
		public static void PlayError() => SystemSounds.Hand.Play();
		public static void PlayInterrupted() => SystemSounds.Asterisk.Play();

		public static bool PlayCopyStarted() => CopyStarted.Play();
		public static bool PlayOneCopied() => OneCopied.Play();
		public static bool PlayAllCopied() => AllCopied.Play() || OneCopied.Play();

		public static bool PlaysSound { get; set; }

		public class SoundWorker
		{
			public string Path
			{
				get => _path;
				set
				{
					_path = value;
					_isValid = null;
				}
			}
			private string _path;

			public bool IsValid => (_isValid ??= File.Exists(Path));
			private bool? _isValid = null;

			private static SoundPlayer _player;

			public bool Play()
			{
				if (!PlaysSound || !IsValid)
					return false;

				try
				{
					if (_player is not null)
					{
						_player.Stop();
						_player.Dispose();
					}
					_player = new SoundPlayer(Path);
					_player.Play();

					return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to play a sound. {Path}\r\n{ex}");
					_isValid = false;
					return false;
				}
			}
		}

		public static SoundWorker CopyStarted { get; } = new SoundWorker();
		public static SoundWorker OneCopied { get; } = new SoundWorker();
		public static SoundWorker AllCopied { get; } = new SoundWorker();
	}
}