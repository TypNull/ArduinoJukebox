using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArduinoJukebox.Core
{
    internal abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Notifies the Property Changed Event to update the GUI 
        /// </summary>
        /// <param name="name">Name of the Property</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Sets the Field value of an Property with Field and update the GUI
        /// </summary>
        /// <typeparam name="T">Class of Property and Field</typeparam>
        /// <param name="field">Reference to the Field</param>
        /// <param name="value">Value to set</param>
        /// <param name="propertyName">Name of the Property</param>
        /// <returns>An boolean that retruns false if Field and Property are equal</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

}
