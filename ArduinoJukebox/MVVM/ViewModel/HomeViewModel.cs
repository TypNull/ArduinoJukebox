using ArduinoConnect;
using ArduinoJukebox.Core;
using ArduinoJukebox.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArduinoJukebox.MVVM.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        public RelayCommand PlaySong { get; }
        public RelayCommand PlayPauseCommand { get; }
        public RelayCommand NextPreviousCommand { get; }
        public RelayCommand FastDisconnectConnectCommand { get; }
        public RelayCommand MuteUnMuteConnectCommand { get; }
        public RelayCommand DanceCommand { get; }
        public RelayCommand RandomNormalConnectCommand { get; }


        public ObservableCollection<Song> Songs
        {
            get => _songs; set
            {
                _songs = value;
                if (IsRandom)
                    PlayList = Songs.Shuffle().ToArray();
                else
                    PlayList = Songs.ToArray();
            }
        }
        public ObservableCollection<Song> _songs = new();
        public Song[] PlayList { get; set; } = Array.Empty<Song>();

        private Song? _actualSong;
        public Song? ActualSong { get => _actualSong; set => SetField(ref _actualSong, value); }


        private bool _isPlaying = false;
        public bool IsPlaying { get => _isPlaying; set => SetField(ref _isPlaying, value); }

        private bool _isRandom = false;
        public bool IsRandom { get => _isRandom; set => SetField(ref _isRandom, value); }

        private bool _isMute = false;
        public bool IsMute { get => _isMute; set => SetField(ref _isMute, value); }
        private bool _isDanceing = false;
        public bool IsDanceing { get => _isDanceing; set => SetField(ref _isDanceing, value); }
        public bool IsConnected
        {
            get => (MainViewModel.Instance?.VMs[1] as SettingsViewModel)?.IsConnected == true; set => OnPropertyChanged(nameof(IsConnected));
        }

        public HomeViewModel()
        {
            SettingsViewModel? settingVM = MainViewModel.Instance?.VMs[1] as SettingsViewModel;
            PlaySong = new(o =>
            {
                if ((Song)o == ActualSong && ActualSong.IsPlaying) return;

                if (ActualSong != null)
                    ActualSong.IsPlaying = false;
                ActualSong = (Song)o;

                ActualSong.IsPlaying = true;
                IsPlaying = true;
                SongStreamHandler.StartSendSong(ActualSong);

            });

            DanceCommand = new(o =>
            {
                IsDanceing = !IsDanceing;
                if (IsConnected)
                    if (IsDanceing)
                        _ = SongStreamHandler.Dance();
                    else _ = SongStreamHandler.NotDance();
            });


            SongStreamHandler.SongFinished += (s, o) =>
            {
                Song? song = (s as Song);
                if (song == null)
                    return;
                song.IsPlaying = false;
                IsPlaying = false;
            };

            RandomNormalConnectCommand = new(o =>
            {
                IsRandom = !IsRandom;
                if (IsRandom)
                    PlayList = Songs.Shuffle().ToArray();
                else
                    PlayList = Songs.ToArray();
            });

            MuteUnMuteConnectCommand = new(o =>
            {
                IsMute = !IsMute;
                if (IsConnected)
                    if (IsMute) _ = SongStreamHandler.Mute();
                    else _ = SongStreamHandler.UnMute();
            });

            PlayPauseCommand = new(o =>
                {
                    if (ActualSong?.IsPlaying == false)
                    {
                        PlaySong.Execute(ActualSong);
                        return;
                    }
                    IsPlaying = !IsPlaying;
                    if (IsPlaying)
                        _ = SongStreamHandler.Play();
                    else
                        _ = SongStreamHandler.Pause();
                });

            NextPreviousCommand = new(o =>
            {
                if ((o as string) == "next")
                {
                    for (int i = 0; i < PlayList.Length; i++)
                        if (PlayList[i] == ActualSong)
                        {
                            i++;
                            if (i < PlayList.Length)
                                PlaySong.Execute(PlayList[i]);
                            else
                                PlaySong.Execute(PlayList[0]);
                        }
                }
                else if ((o as string) == "previous")
                {
                    for (int i = 0; i < PlayList.Length; i++)
                        if (PlayList[i] == ActualSong)
                        {
                            i--;
                            if (i > -1)
                                PlaySong.Execute(PlayList[i]);
                            else
                                PlaySong.Execute(PlayList.Last());
                        }

                }
            });

            FastDisconnectConnectCommand = new(o =>
            {
                if (IsConnected == true && (o as string) == "disconnect")
                    BLEHandler.Instance?.Disconnect();

                if (IsConnected == false && BLEHandler.Instance?.IsScaned == true && (o as string) == "fast")
                    BLEHandler.Instance?.ConnectFromId(BLEHandler.Instance.FoundDeviceNames.First().Id);
            });

        }
    }
}
