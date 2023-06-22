using ArduinoConnect;
using ArduinoJukebox.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ArduinoJukebox.MVVM.Model
{
    internal static class SongStreamHandler
    {
        private static CancellationTokenSource CTS = new();
        private static CancellationToken Token => CTS.Token;
        private static BLEHandler Ble => BLEHandler.Instance!;
        public static void StartSendSong(Song song)
        {
            CTS.Cancel();
            CTS.Dispose();
            CTS = new();
            Task.Run(async () => await SendSong(song));
        }

        public static event EventHandler? SongFinished;
        private static readonly LinkedList<int> _durationList = new();

        private static void AddToDuration(int item)
        {
            if (_durationList.Count >= 8)
                _durationList.RemoveLast();
            _durationList.AddFirst(item);
        }

        private static async Task SendSong(Song song)
        {
            try
            {
                CancellationToken token = Token;
                HomeViewModel? vm = (MainViewModel.Instance?.VMs[0] as HomeViewModel);
                if (vm?.IsConnected != true || Ble == null)
                    return;
                await Ble.Send(new byte[] { 200, song.Speed });
                for (int i = 0; i < song.Data.Length && !token.IsCancellationRequested && vm.IsConnected; i++)
                {
                    byte fequencByte = _melodyDictionary[song.Data[i]];
                    int length = song.Data[++i];
                    int speed = ((60000 * 4) / (song.Speed * 2));
                    int noteDuration = (int)((speed / length) * (length < 0 ? -1.5f : 1));

                    if (!vm.IsMute)
                        await Ble.Send(new byte[] { fequencByte, (byte)(length < 0 ? ((length * -1) + 150) : length + 100) });
                    AddToDuration((int)(noteDuration * 0.5f));
                    await Task.Delay((int)(noteDuration * 0.6f));
                    if (BufferIsFull)
                    {
                        await Task.Delay(_durationList.Sum() / 3 * 2);
                        BufferIsFull = false;
                    }
                    while (!vm.IsPlaying)
                        await Task.Delay(1000);
                }
                if (!token.IsCancellationRequested)
                    SongFinished?.Invoke(song, new());
            }
            catch (Exception)
            {

            }

        }
        public static bool BufferIsFull { get; set; }

        public static async Task<bool> Pause() => await Ble.Send(new byte[] { 202 });
        public static async Task<bool> Mute() => await Ble.Send(new byte[] { 203 });
        public static async Task<bool> UnMute() => await Ble.Send(new byte[] { 204 });
        public static async Task<bool> Play() => await Ble.Send(new byte[] { 201 });

        public static async Task<bool> Dance() => await Ble.Send(new byte[] { 205 });
        public static async Task<bool> NotDance() => await Ble.Send(new byte[] { 206 });
        private static readonly Random rng = new();

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            list = list.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }

        private static HomeViewModel? home;

        internal static void OnMessageRecived(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.UnicodeEncoding = UnicodeEncoding.Utf8;
            string receivedString = reader.ReadString(reader.UnconsumedBufferLength);
            home ??= (MainViewModel.Instance?.VMs[0] as HomeViewModel);
            switch (receivedString)
            {
                case "1":
                    Debug.WriteLine("play");
                    if (home != null)
                        home.IsPlaying = true;
                    break;
                case "2":
                    Debug.WriteLine("pause");
                    if (home != null)
                        home.IsPlaying = false;
                    break;
                case "3":
                    Debug.WriteLine("Buffer");
                    BufferIsFull = true;
                    break;
            }


        }

        private static readonly Dictionary<int, byte> _melodyDictionary = new()
        {{ 31,1},{ 33,2},{ 35,3},{ 37,4},{ 39,5},{ 41,6},{ 44,7},{ 46,8},{ 49,9},{ 52,10},{ 55,11},{ 58,12},{ 62,13},{ 65,14},
            { 69,15},{ 73,16},{ 78,17},{ 82,18},{ 87,19},{ 93,20},{ 98,21},{ 104,22},{ 110,23},{ 117,24},{ 123,25},{ 131,26},
            { 139,27},{ 147,28},{ 156,29},{ 165,30},{ 175,31},{ 185,32},{ 196,33},{ 208,34},
            { 220,35},{ 233,36},{ 247,37},{ 262,38},{ 277,39},{ 294,40},{ 311,41},{ 330,42},
            { 349,43},{ 370,44},{ 392,45},{ 415,46},{ 440,47},{ 466,48},{ 494,49},{ 523,50},
            { 554,51},{ 587,52},{ 622,53},{ 659,54},{ 698,55},{ 740,56},{ 784,57},{ 831,58},
            { 880,59},{ 932,60},{ 988,61},{ 1047,62},{ 1109,63},{ 1175,64},{ 1245,65},{ 1319,66},
            { 1397,67},{ 1480,68},{ 1568,69},{ 1661,70},{ 1760,71},{ 1865,72},{ 1976,73},{ 2093,74},
            { 2217,75},{ 2349,76},{ 2489,77},{ 2637,78},{ 2794,79},{ 2960,80},{ 3136,81},{ 3322,82},
            { 3520,83},{ 3729,84},{ 3951,85},{ 4186,86},{ 4435,87},{ 4699,88},{ 4978,89},{ 0,90}};
    }
}
