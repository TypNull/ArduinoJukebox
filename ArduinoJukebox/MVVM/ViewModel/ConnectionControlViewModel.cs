using ArduinoJukebox.Core;


namespace ArduinoJukebox.MVVM.ViewModel
{
    internal class ConnectionControlViewModel : ObservableObject
    {
        public bool IsEnabeld { get => _isEnabeld; set => SetField(ref _isEnabeld, value); }
        private bool _isEnabeld = false;

        public ConnectionControlViewModel() => ArduinoConnect.BLEHandler.BluetoothStateChanged += OnBluethoothChanged;
        private void OnBluethoothChanged(object? sender, System.EventArgs args)
        {
            IsEnabeld = ArduinoConnect.BLEHandler.IsBluetoothEnabled;
            if (!IsEnabeld)
                _ = ArduinoConnect.BLEHandler.EnableBluetoothAsync();
        }

    }


}
