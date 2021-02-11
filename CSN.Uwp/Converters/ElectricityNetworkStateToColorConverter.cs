namespace CSN.Uwp.Converters
{
    using CSN.Common.Models;
    using System;
    using Windows.UI;
    using Windows.UI.Xaml.Data;

    public class ElectricityNetworkStateToColorConverter : IValueConverter
    {
        #region IValueConverter members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (Enum.TryParse(value?.ToString(), true, out ElectricityNetworkState networkState))
                {
                    return (Color)App.Current.Resources[$"NetworkState{networkState}Color"];
                }
            }
            catch
            {
                // Ignore exception
            }

            return value;
        }

        /// <summary>
        /// This converter has no convert back logic
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();

        #endregion IValueConverter members
    }
}