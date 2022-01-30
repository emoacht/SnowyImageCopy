using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace SnowyImageCopy.Views.Controls
{
	public partial class VideoBox : UserControl
	{
		public VideoBox()
		{
			InitializeComponent();

			RegisterTimer();
			RegisterSliderDownUp();

			this.IsVisibleChanged += (_, e) =>
			{
				if (!(bool)e.NewValue)
					Stop();
			};

			this.Media.MouseDown += Media_MouseDown;

			this.Unloaded += OnUnloaded;
			void OnUnloaded(object sender, RoutedEventArgs e)
			{
				this.Unloaded -= OnUnloaded;

				Stop();
				this.Media.MouseDown -= Media_MouseDown;
				this.Media.Close();
				this.Media = null;
			}
		}

		private Track _track;

		private void PART_Track_Initialized(object sender, EventArgs e)
		{
			_track = (Track)sender;
		}

		public string SourcePath
		{
			get { return (string)GetValue(SourcePathProperty); }
			set { SetValue(SourcePathProperty, value); }
		}
		public static readonly DependencyProperty SourcePathProperty =
			DependencyProperty.Register(
				"SourcePath",
				typeof(string),
				typeof(VideoBox),
				new PropertyMetadata(
					null,
					(d, e) => ((VideoBox)d).Stop()));

		public double Position
		{
			get { return (int)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}
		public static readonly DependencyProperty PositionProperty =
			DependencyProperty.Register(
				"Position",
				typeof(double),
				typeof(VideoBox),
				new PropertyMetadata(
					0D,
					(d, e) => ((VideoBox)d).Reflect((double)e.NewValue)));

		private void PlayButton_Click(object sender, RoutedEventArgs e) => Play();
		private void PauseButton_Click(object sender, RoutedEventArgs e) => Pause();

		private void Media_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_timer.IsEnabled)
				Pause();
			else if (this.Slider.IsEnabled)
				Play();
		}

		private async void Play()
		{
			if (this.Media.Source is null)
			{
				if (!File.Exists(SourcePath))
					return;

				this.Media.Source = new Uri(SourcePath, UriKind.RelativeOrAbsolute);

				while (!this.Media.NaturalDuration.HasTimeSpan)
					await Task.Delay(TimeSpan.FromMilliseconds(20));

				this.Slider.Maximum = this.Media.NaturalDuration.TimeSpan.TotalSeconds;
				this.Slider.IsEnabled = true;
				this.Position = 0;
			}
			else if (this.Media.NaturalDuration.HasTimeSpan
				&& (this.Media.NaturalDuration.TimeSpan <= this.Media.Position))
			{
				this.Position = 0;
			}

			SetMediaPlaying();
			SetButtonsPlaying();

			// Make visible after start playing to reduce flickering.
			this.Media.Visibility = Visibility.Visible;
		}

		private void Pause()
		{
			SetMediaPaused();
			SetButtonsPaused();
		}

		public void Stop()
		{
			this.Media.Visibility = Visibility.Collapsed;

			Pause();

			this.Media.Source = null;
			this.Slider.IsEnabled = false;
			this.Position = 0;
		}

		#region Sync

		/// <summary>
		/// Whether Slider and MediaElement are to be synced
		/// </summary>
		private bool _isSynced = true;

		private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(10) };

		private void RegisterTimer()
		{
			_timer.Tick += (_, _) =>
			{
				if (_isSynced)
					this.Position = this.Media.Position.TotalSeconds;
			};
		}

		public void Reflect(double value)
		{
			if (_isSynced)
			{
				if (!this.Media.NaturalDuration.HasTimeSpan)
					return;

				var total = this.Media.NaturalDuration.TimeSpan.TotalSeconds;

				switch (value, (value < total))
				{
					case (0, _):
						this.Media.Position = TimeSpan.Zero;
						break;
					case (_, true):
						this.Media.Position = TimeSpan.FromSeconds(value);
						break;
					case (_, false):
						this.Media.Position = this.Media.NaturalDuration.TimeSpan;
						Pause();
						break;
				}
			}
		}

		private void RegisterSliderDownUp()
		{
			this.Slider.PreviewMouseDown += (_, _) => Down();
			this.Slider.PreviewStylusDown += (_, _) => Down();
			this.Slider.PreviewTouchDown += (_, _) => Down();

			this.Slider.PreviewMouseUp += (_, e) => Up(e.GetPosition(_track));
			this.Slider.PreviewStylusUp += (_, e) => Up(e.GetPosition(_track));
			this.Slider.PreviewTouchUp += (_, e) => Up(e.GetTouchPoint(_track).Position);
		}

		private void Down()
		{
			// Stop sync if playing.
			if (_timer.IsEnabled)
				_isSynced = false;
		}

		private void Up(Point position)
		{
			var isPlaying = _timer.IsEnabled;

			SetMediaPaused();

			// Restart sync.
			_isSynced = true;

			this.Position = _track.ValueFromPoint(position);

			if (isPlaying)
				SetMediaPlaying();
		}

		#endregion

		#region Media

		private readonly object _locker = new();

		private void SetMediaPaused()
		{
			lock (_locker)
			{
				this.Media.Pause();
				_timer.Stop();
			}
		}

		private void SetMediaPlaying()
		{
			lock (_locker)
			{
				this.Media.Play();
				_timer.Start();
			}
		}

		#endregion

		#region Buttons

		private void SetButtonsPlaying()
		{
			this.PlayButton.Visibility = Visibility.Hidden;
			this.PauseButton.Visibility = Visibility.Visible;
		}

		private void SetButtonsPaused()
		{
			this.PlayButton.Visibility = Visibility.Visible;
			this.PauseButton.Visibility = Visibility.Hidden;
		}

		#endregion
	}
}