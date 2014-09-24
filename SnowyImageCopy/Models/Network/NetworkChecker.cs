using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Network
{
	/// <summary>
	/// Check network connection.
	/// </summary>
	internal static class NetworkChecker
	{
		/// <summary>
		/// Check if PC is connected to a network. 
		/// </summary>
		internal static bool IsNetworkConnected()
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}

		/// <summary>
		/// Check if PC is connected to a network and a specified wireless network if applicable.
		/// </summary>
		/// <param name="card">FlashAir card information</param>
		internal async static Task<bool> IsNetworkConnectedAsync(CardInfo card)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			if ((card == null) || String.IsNullOrEmpty(card.Ssid) || !card.IsWirelessConnected)
				return true;

			return await IsWirelessNetworkConnectedAsync(card.Ssid);
		}

		/// <summary>
		/// Check if PC is connected to a specified wireless network.
		/// </summary>
		/// <param name="ssid">SSID of wireless network</param>
		internal static async Task<bool> IsWirelessNetworkConnectedAsync(string ssid)
		{
			if (String.IsNullOrEmpty(ssid))
				throw new ArgumentNullException("ssid");

			if (NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == OperationalStatus.Up)
				.All(x => x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
				return false;

			var ssids = await GetConnectedSsidAsync();

#if (DEBUG)
			ssids.ToList().ForEach(x => Debug.WriteLine(String.Format("Found SSID: {0}", x)));
#endif

			return ssids.Any(x => x.Equals(ssid, StringComparison.Ordinal));
		}


		#region Netsh command

		/// <summary>
		/// Network interface information by Netsh
		/// </summary>
		/// <remarks>Items are not limited to.</remarks>
		private class InterfaceInfo
		{
			public readonly string Name = null;
			public readonly string State = null;
			public readonly string Ssid = null;
			public readonly string NetworkType = null;
			public readonly string HostedNetworkStatus = null;

			public static readonly Regex NamePattern = new Regex(@"\s*Name\s+:", RegexOptions.Compiled);
			public static readonly Regex StatePattern = new Regex(@"\s*State\s+:", RegexOptions.Compiled);
			public static readonly Regex SsidPattern = new Regex(@"\s*SSID\s+:", RegexOptions.Compiled);
			public static readonly Regex NetworkTypePattern = new Regex(@"\s*Network type\s+:", RegexOptions.Compiled);
			public static readonly Regex HostedNetworkStatusPattern = new Regex(@"\s*Hosted network status\s+:", RegexOptions.Compiled);

			public bool IsValid
			{
				get { return new string[] { Name, State, HostedNetworkStatus }.All(x => !String.IsNullOrEmpty(x)); }
			}

			public bool IsConnected
			{
				get { return State.Equals("connected", StringComparison.OrdinalIgnoreCase); }
			}

			public InterfaceInfo(string[] lines)
			{
				foreach (var line in lines)
				{
					var source = line;

					// This order must match the order of Netsh's output.
					FindItem(NamePattern, ref source, ref Name);
					FindItem(StatePattern, ref source, ref State);
					FindItem(SsidPattern, ref source, ref Ssid);
					FindItem(NetworkTypePattern, ref source, ref NetworkType);
					FindItem(HostedNetworkStatusPattern, ref source, ref HostedNetworkStatus);
				}
			}

			private static void FindItem(Regex pattern, ref string source, ref string target)
			{
				if (String.IsNullOrEmpty(source) || (target != null))
					return;

				var match = pattern.Match(source);
				if (match.Success)
					target = source.Substring(match.Value.Length).Trim();

				source = String.Empty;
			}
		}

		/// <summary>
		/// Get currently connected SSIDs using Netsh.
		/// </summary>
		/// <returns>SSIDs</returns>
		private async static Task<string[]> GetConnectedSsidAsync()
		{
			var outputLines = await ExecuteNetshAsync();

			var interfaceInfoList = new List<InterfaceInfo>();
			var bufferLines = new List<string>();

			while (outputLines.Any())
			{
				var outputLine = outputLines.Dequeue();

				if ((!String.IsNullOrEmpty(outputLine) && InterfaceInfo.NamePattern.IsMatch(outputLine)) ||
					!outputLines.Any())
				{
					if (bufferLines.Any())
					{
						var info = new InterfaceInfo(bufferLines.ToArray());
						if (info.IsValid)
							interfaceInfoList.Add(info);

						bufferLines.Clear(); // End of preceding interface information
					}

					bufferLines.Add(outputLine); // Beginning of succeeding interface information
				}

				if (bufferLines.Any())
					bufferLines.Add(outputLine);
			}

			return interfaceInfoList
				.Where(x => x.IsConnected && !String.IsNullOrEmpty(x.Ssid))
				.Select(x => x.Ssid)
				.ToArray();
		}

		/// <summary>
		/// Execute Netsh for wireless network interface.
		/// </summary>
		private static async Task<Queue<string>> ExecuteNetshAsync()
		{
			var commands = new string[]
			{
				"chcp 437", // Change code page to US (English).
				"netsh wlan show interface",
				"exit",
			};

			using (var proc = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = Environment.GetEnvironmentVariable("ComSpec") ?? String.Empty,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
				},
			})
			{
				var output = new Queue<string>();

				DataReceivedEventHandler received = (sender, e) => output.Enqueue(e.Data);
				proc.OutputDataReceived += received;

				var tcs = new TaskCompletionSource<Queue<string>>();

				EventHandler exited = (sender, e) => tcs.SetResult(output);
				proc.Exited += exited;
				proc.EnableRaisingEvents = true;

				proc.Start();
				proc.BeginOutputReadLine();

				using (var sw = proc.StandardInput)
				{
					if (sw.BaseStream.CanWrite)
					{
						foreach (var command in commands)
							sw.WriteLine(command);
					}
				}

				var outputLines = await tcs.Task;

				proc.OutputDataReceived -= received;
				proc.Exited -= exited;

				return outputLines;
			}
		}

		#endregion
	}
}