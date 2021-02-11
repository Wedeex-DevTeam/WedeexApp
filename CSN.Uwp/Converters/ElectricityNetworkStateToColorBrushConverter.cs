namespace CSN.Uwp.Converters
{
    using CSN.Common.Models;
    using System;
    using System.Collections.Generic;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

    public class ElectricityNetworkStateToColorBrushConverter : IValueConverter
    {
        #region IValueConverter members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (Enum.TryParse(value?.ToString(), true, out ElectricityNetworkState networkState))
                {
                    return (SolidColorBrush)App.Current.Resources[$"NetworkState{networkState}ColorBrush"];
                }
            }
            catch (Exception ex)
            {
                App.Current.TelemetryClient.TrackException(ex, new Dictionary<string, string> { { "IsHandled", true.ToString() } });
            }

            return value;
        }

        /// <summary>
        /// This converter has no convert back logic
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language) 
            => throw new NotImplementedException();

        #endregion
    }
}
