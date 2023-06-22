using ArduinoJukebox.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ArduinoJukebox.MVVM.Model
{
    class Song : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public byte Speed { get; set; } = 100;
        public int[] Data { get; set; } = Array.Empty<int>();
        public byte[] ImageData { set => Image = ByteToBitmapSource(value); }
        public BitmapSource? Image { get; set; }
        private bool _isPlaying = false;
        public bool IsPlaying { get => _isPlaying; set => SetField(ref _isPlaying, value); }
        public static BitmapSource ByteToBitmapSource(byte[] bytes) => BitmapFrame.Create(new MemoryStream(bytes));

    }
}
