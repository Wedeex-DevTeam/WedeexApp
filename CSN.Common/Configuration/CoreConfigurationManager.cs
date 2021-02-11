namespace CSN.Common.Core
{
    using CSN.Common.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.Storage;

    public sealed class CoreConfigurationManager
    {
        #region Fields

        private JsonSerializer serializer;

        #endregion

        #region Public methods

        public static CoreConfigurationManager Instance = new CoreConfigurationManager();

        public CoreConfiguration Configuration { get; private set; }

        #endregion

        #region Constructor

        public CoreConfigurationManager()
        {
            this.serializer = new JsonSerializer();
        }

        #endregion

        #region Public methods

        public async Task InitAsync()
        {
            await LoadCoreConfigurationAsync();
        }

        #endregion

        #region Private methods

        private async Task LoadCoreConfigurationAsync()
        {
            try
            {
                StorageFolder dataFolder = await Package.Current.InstalledLocation.GetFolderAsync("CSN.Common\\Configuration");
                var dataFile = await dataFolder.GetFileAsync("Configuration.json");
                if (dataFile != null)
                {
                    using (var stream = await dataFile.OpenStreamForReadAsync())
                    using (var sr = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        Configuration = serializer.Deserialize<CoreConfiguration>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when try to load core configuration: {ex.Message}");
            }
        }

        #endregion
    }
}
