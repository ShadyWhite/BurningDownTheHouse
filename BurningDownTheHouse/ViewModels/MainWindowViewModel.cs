using BurningDownTheHouse.Models;
using Nhaama.Memory;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace BurningDownTheHouse.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private static readonly Uri OffsetUrl = new Uri("https://raw.githubusercontent.com/LeonBlade/BurningDownTheHouse/master/Offsets/");

		private NhaamaProcess GameProcess { get; set; }
		private string GameVersion { get; set; }
		private Offsets Offset { get; set; }
		private ulong FFXIV_DX11 { get; set; }
		public bool PlaceAnywhere { get; set; }

		public ICommand PACheckedCommand { get; private set; }
		public ICommand ShortcutPreviewCommand { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MainWindowViewModel()
		{
			// Initialize Nhaama.
			InitializeNhaama();

			// Fetch the latest offsets for this version.
			FetchOffsets();

			// Store the ffxiv_dx11 main module base address.
			FFXIV_DX11 = (ulong)GameProcess.BaseProcess.MainModule.BaseAddress.ToInt64();

			// Create commands.
			PACheckedCommand = new DelegateCommand(OnPlaceAnywhereChecked);
			ShortcutPreviewCommand = new DelegateCommand<KeyEventArgs>(OnPreviewShortcutKey);
		}

		/// <summary>
		/// Fetch offsets from the web.
		/// </summary>
		private void FetchOffsets()
		{
			// Get the current version.
			var gameDirectory = new DirectoryInfo(GameProcess.BaseProcess.MainModule.FileName);
			GameVersion = File.ReadAllText(Path.Combine(gameDirectory.Parent.FullName, "ffxivgame.ver"));

			// Create client to fetch 
			using (var client = new WebClient())
			{
				var uri = new Uri(OffsetUrl, $"{GameVersion}.xml");
				var serializer = new XmlSerializer(typeof(Offsets));

				try
				{
					var offsetXml = client.DownloadString(uri);
					Offset = (Offsets)serializer.Deserialize(new StringReader(offsetXml));
				}
				catch (WebException ex)
				{
					throw new Exception("Couldn't not get the offsets for version: " + uri, ex);
				}
			}
		}

		/// <summary>
		/// Initialize Nhaama.
		/// </summary>
		private void InitializeNhaama()
		{
			// Get processes matching for FFXIV.
			var procs = Process.GetProcessesByName("ffxiv_dx11");

			// No proccess found for FFXIV.
			if (procs.Length == 0)
			{
				MessageBox.Show("Could not find ffxiv_dx11.exe!", "Burning Down the House", MessageBoxButton.OK);
				Environment.Exit(-1);
			}

			// Get the Nhaama process from the first process that matches for FFXIV.
			GameProcess = procs[0].GetNhaamaProcess();

			// Enable raising events.
			GameProcess.BaseProcess.EnableRaisingEvents = true;
			// Listen to some stuff.
			GameProcess.BaseProcess.Exited += (_, e) => MessageBox.Show("FINAL FANTASY XIV has shut down. Please restart this application after you relaunch FFXIV.", "Burning Down the House", MessageBoxButton.OK);
		}

		/// <summary>
		/// Place Anywhere checkbox.
		/// </summary>
		private void OnPlaceAnywhereChecked()
		{
			if (PlaceAnywhere)
				EnablePlaceAnywhere();
			else
				DisablePlaceAnywhere();
		}

		private void OnPreviewShortcutKey(KeyEventArgs args)
		{
			Console.WriteLine(args.SystemKey);
		}

		/// <summary>
		/// Turns the place anywhere on.
		/// </summary>
		private void EnablePlaceAnywhere()
		{
			GameProcess.WriteBytes(FFXIV_DX11 + Offset.PlaceAnywhere.ToHex(), new byte[] { 0xC6, 0x87, 0x73, 0x01, 0x00, 0x00, 0x01 });
			GameProcess.WriteBytes(FFXIV_DX11 + Offset.WallPartition.ToHex(), new byte[] { 0xC6, 0x87, 0x73, 0x01, 0x00, 0x00, 0x01 });
		}

		/// <summary>
		/// Turns off place anywhere.
		/// </summary>
		private void DisablePlaceAnywhere()
		{
			GameProcess.WriteBytes(FFXIV_DX11 + Offset.PlaceAnywhere.ToHex(), new byte[] { 0xC6, 0x87, 0x73, 0x01, 0x00, 0x00, 0x00 });
			GameProcess.WriteBytes(FFXIV_DX11 + Offset.WallPartition.ToHex(), new byte[] { 0xC6, 0x87, 0x73, 0x01, 0x00, 0x00, 0x00 });
		}
	}
}
