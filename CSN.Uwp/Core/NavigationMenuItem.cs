namespace CSN.Uwp.Core
{
    using System;
    using Windows.UI.Xaml.Controls;

    public class NavigationMenuItem
    {
        #region Constructor
        
        public NavigationMenuItem(string displayName, Type pageType, Symbol icon)
        {
            DisplayName = displayName;
            Icon = new SymbolIcon(icon);
            PageType = pageType;
        }

        #endregion

        #region Properties

        public string DisplayName { get; set; }

        public SymbolIcon Icon { get; set; }

        public Type PageType { get; set; }

        #endregion
    }
}