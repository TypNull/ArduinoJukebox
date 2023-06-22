using ArduinoJukebox.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ArduinoJukebox.MVVM.Model
{
    internal static class SaveHandler
    {
        private static SettingsViewModel? SettingsVM => (MainViewModel.Instance?.VMs[1] as SettingsViewModel);
        private static HomeViewModel? HomeVM => (MainViewModel.Instance?.VMs[0] as HomeViewModel);

        public static DeviceInfo? LastConnectedDevice { get; set; }
        public static string SongPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string _savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArduinoJukebox", "appsettings.json");

        /// <summary>
        /// Save song path file and last used ble device
        /// </summary>
        public static void Save()
        {
            if (SettingsVM != null)
            {
                if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArduinoJukebox")))
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArduinoJukebox"));
                string save = JsonSerializer.Serialize(SongPath, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(_savePath, save);
            }
        }

        /// <summary>
        /// load song path file and last used ble device
        /// </summary>
        public static void Load()
        {
            if (SettingsVM != null)
            {
                if (File.Exists(_savePath))
                    SongPath = JsonSerializer.Deserialize<string>(File.ReadAllText(_savePath), new JsonSerializerOptions() { WriteIndented = true }) ?? _savePath;
            }
        }


        public static void LoadSongs()
        {
            if (SettingsVM != null && HomeVM != null)
            {
                try
                {
                    string[] files = Directory.GetFiles(SongPath, "*.song8");
                    List<Song> songs = new(files.Length);
                    for (int i = 0; i < files.Length; i++)
                    {
                        Song? song = JsonSerializer.Deserialize<Song>(File.ReadAllText(files[i]), new JsonSerializerOptions() { WriteIndented = true });
                        if (song != null)
                            songs.Add(song);
                    }
                    HomeVM.Songs = new(songs);
                    HomeVM.ActualSong = songs.FirstOrDefault();
                }
                catch (Exception)
                {

                }

            }
        }
    }
}
