using ArduinoConnect;
using ArduinoJukebox.Core;
using ArduinoJukebox.MVVM.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoJukebox.MVVM.ViewModel
{
    internal class SettingsViewModel : ObservableObject
    {
        public string LibraryPath { get => SaveHandler.SongPath; }

        public bool IsEnabled { get => _isEnabled; set => SetField(ref _isEnabled, value); }
        private bool _isEnabled = true;

        public RelayCommand DiscoverLibrary { get; }

        private DeviceInfo _selectedDevice = new("", "");
        public DeviceInfo SelectedItem { get => _selectedDevice; set => SetField(ref _selectedDevice, value); }
        private bool _isConnected = false;
        public bool IsConnected { get => _isConnected; set => SetField(ref _isConnected, value); }

        public ObservableCollection<DeviceInfo> Devices { get; set; } = new();

        public SettingsViewModel()
        {
            HomeViewModel? homeView = (MainViewModel.Instance?.VMs[0] as HomeViewModel);
            PropertyChanged += (s, o) =>
            {
                if (o.PropertyName == "SelectedItem")
                {
                    IsEnabled = false;
                    DeviceInfo deviceInfo = SelectedItem;
                    Task.Run(async () =>
                    {
                        if (deviceInfo != null)
                            if (await (BLEHandler.Instance?.ConnectFromId(deviceInfo.Id) ?? Task.FromResult(false)) == true)
                            {
                                IsConnected = true;
                                if (homeView != null)
                                    homeView.IsConnected = true;
                            }
                            else
                            {
                                IsConnected = false;
                                IsEnabled = true;
                                if (homeView != null)
                                    homeView.IsConnected = true;
                            }
                    });
                }
            };
            DiscoverLibrary = new(o =>
            {
                using FolderBrowserDialog fbd = new();
                fbd.InitialDirectory = SaveHandler.SongPath;
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    SaveHandler.SongPath = fbd.SelectedPath;
                OnPropertyChanged(nameof(LibraryPath));
                Task.Run(() =>
                {
                    SaveHandler.Save();
                    SaveHandler.LoadSongs();
                });
            });

        }
    }

    public class DeviceInfo
    {
        public DeviceInfo(string name, string id)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
    }
}
