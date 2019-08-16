using Nhaama.Memory;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BurningDownTheHouse.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private NhaamaProcess GameProcess { get; set; }
		private string GameVersion { get; set; }

		public MainWindowViewModel()
		{
			// Initialize Nhaama.
			InitializeNhaama();

			// Get the current version.
			var gameDirectory = new DirectoryInfo(GameProcess.BaseProcess.MainModule.FileName);
			GameVersion = File.ReadAllText(Path.Combine(gameDirectory.Parent.FullName, "ffxivgame.ver"));
		}

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
	}
}
