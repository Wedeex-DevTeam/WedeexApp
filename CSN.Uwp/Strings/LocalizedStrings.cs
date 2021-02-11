namespace CSN.Uwp.Strings
{
    using Windows.ApplicationModel.Resources;

    public class LocalizedStrings
    {
        #region Static Fields

        private static ResourceLoader resourceLoader;

        #endregion

        #region Public Indexers

        public string this[string key] => GetString(key);

        #endregion

        #region Public Methods and Operators

        public static string GetString(string key)
        {
            if (resourceLoader == null)
            {
                resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            }

            return resourceLoader.GetString(key);
        }

        #endregion
    }
}
