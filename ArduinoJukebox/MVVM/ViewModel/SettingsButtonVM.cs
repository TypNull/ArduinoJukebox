using ArduinoJukebox.Core;

namespace ArduinoJukebox.MVVM.ViewModel
{
    internal class SettingsControlVM : ObservableObject
    {
        public RelayCommand SettingsPressedControl { get; }
        private string _name = "Home";
        public string Name { get => _name; set => SetField(ref _name, value); }



        public SettingsControlVM()
        {
            SettingsPressedControl = new(o =>
            {
                MainViewModel? instance = MainViewModel.Instance;
                if (instance == null)
                    return;
                Name = instance.CurrendName == "Home" ? "Settings" : "Home";
                instance.ChangeViewCommand.Execute(Name);
            });


        }
    }


}
