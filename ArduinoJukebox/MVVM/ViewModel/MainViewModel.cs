using ArduinoConnect;
using ArduinoJukebox.Core;
using ArduinoJukebox.MVVM.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ArduinoJukebox.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public static MainViewModel? Instance { get; private set; }
        public RelayCommand ChangeViewCommand { get; }

        private object _currendView;
        public object CurrentView
        {
            get => _currendView;
            set => SetField(ref _currendView, value);
        }

        public ObservableObject[] VMs = new ObservableObject[2];
        public string CurrendName { get; private set; } = "Home";

        public MainViewModel()
        {
            Instance = this;
            VMs[0] = new HomeViewModel();
            VMs[1] = new SettingsViewModel();
            _currendView = VMs[0];
            ChangeViewCommand = new(o =>
            {
                foreach (ObservableObject vm in VMs)
                    if (vm.GetType().Name == o + "ViewModel")
                    {
                        CurrentView = vm;
                        CurrendName = o.ToString()!;
                        OnPropertyChanged(nameof(CurrendName));
                    }
            });
            Task.Run(StartUp);
        }

        private async Task StartUp()
        {
            SaveHandler.Load();
            SaveHandler.LoadSongs();
            await BLEHandler.ScanBluetoothRadio();
            await BLEHandler.EnableBluetoothAsync();
            BLEHandler ble = new();
            if (BLEHandler.DeviceWatcher != null)
            {
                SettingsViewModel vm = (SettingsViewModel)VMs[1];
                BLEHandler.DeviceWatcher.Removed += (o, s) =>
                {
                    try
                    {
                        List<DeviceInfo> devices = vm.Devices.ToList();
                        DeviceInfo? deviceInformation = devices.FirstOrDefault(device => device?.Id == s.Id);
                        if (deviceInformation != null && vm.SelectedItem != null && deviceInformation.Id != vm.SelectedItem.Id)
                            Application.Current.Dispatcher.BeginInvoke(
  DispatcherPriority.Background, () => vm.Devices.Remove(deviceInformation));
                    }
                    catch (System.Exception)
                    {
                    }
                };

                BLEHandler.DeviceWatcher.Added += (o, s) =>
                {
                    try
                    {
                        List<DeviceInfo> devices = vm.Devices.ToList();
                        if (devices.FirstOrDefault(d => d.Id.Equals(s.Id) || d.Name.Equals(s.Name)) == null && (s.Name != string.Empty && s.Id != string.Empty))
                            Application.Current.Dispatcher.BeginInvoke(
    DispatcherPriority.Background, () => vm.Devices.Add(new DeviceInfo(s.Name, s.Id)));
                    }
                    catch (System.Exception)
                    {
                    }

                };
                ble.ConnectionStatusChanged += (s, o) =>
                {
                    vm.IsConnected = ((bool)(s ?? false));
                    vm.IsEnabled = !vm.IsConnected;
                    if (vm.IsEnabled)
                        vm.SelectedItem = new("", "");
                    ((HomeViewModel)VMs[0]).IsConnected = false;
                };

                ble.MessageRecived += SongStreamHandler.OnMessageRecived;
            }
        }
    }
}
