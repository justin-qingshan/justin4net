using System.Configuration;
using System.Web.Configuration;

namespace just4net.util
{
    public class ConfigUtil
    {
        private static AppSettingsStatic _appSettingsStatic = new AppSettingsStatic();
        private static ConnStringsStatic _connStringStatic = new ConnStringsStatic();

        private Configuration _config;
        private AppSettingsSection _appSettings;
        private ConnectionStringsSection _connStrings;
        //private ApplicationSettingsGroup _applicationSettings;

        private AppSettingsClass _appSettingsClass;
        private ConnectionStringsClass _connStringsClass;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configType"><see cref="ConfigType"/></param>
        /// <param name="path">When config type is <see cref="ConfigType.EXE"/>, it is exe path.</param>
        public ConfigUtil(ConfigType configType = ConfigType.WEB, string path = null)
        {
            if (configType == ConfigType.WEB)
                _config = WebConfigurationManager.OpenWebConfiguration(path == null ? "~" : path);
            else
                _config =
                    path == null
                    ? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                    : ConfigurationManager.OpenExeConfiguration(path);

            _appSettings = (AppSettingsSection)_config.GetSection("appSettings");
            _connStrings = (ConnectionStringsSection)_config.GetSection("connectionStrings");
            //_applicationSettings = (ApplicationSettingsGroup)_config.GetSectionGroup("applicationSettings");

            _appSettingsClass = new AppSettingsClass(this);
            _connStringsClass = new ConnectionStringsClass(this);
        }


        public static AppSettingsStatic Settings
        {
            get { return _appSettingsStatic; }
        }


        public static ConnStringsStatic ConnStrings
        {
            get { return _connStringStatic; }
        }


        public AppSettingsClass AppSettings
        {
            get { return _appSettingsClass; }
        }


        public ConnectionStringsClass ConnectionStrings
        {
            get { return _connStringsClass; }
        }


        /// <summary>
        /// Get app setting value by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetAppSetting(string key)
        {
            return _appSettings.Settings[key].Value;
        }


        /// <summary>
        /// Add or modify app setting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddAppSetting(string key, string value)
        {
            if (_appSettings.Settings[key] == null)
                _appSettings.Settings.Add(key, value);
            else
                _appSettings.Settings[key].Value = value;
        }


        /// <summary>
        /// Get connection string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConnectionString(string key)
        {
            return _connStrings.ConnectionStrings[key] == null ? null
                : ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }


        /// <summary>
        /// Add or midify connection string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="connectionString"></param>
        public void AddConnectionString(string key, string connectionString)
        {
            if (_connStrings.ConnectionStrings[key] == null)
            {
                ConnectionStringSettings connectionSetting = new ConnectionStringSettings(key, connectionString);
                _connStrings.ConnectionStrings.Add(connectionSetting);
            }
            else
                _connStrings.ConnectionStrings[key].ConnectionString = connectionString;
        }


        /// <summary>
        /// Save the changes to disk.
        /// </summary>
        public void Save()
        {
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");
            //ConfigurationManager.RefreshSection("applicationSettings");
        }
        
    }

    public class AppSettingsClass
    {
        private ConfigUtil util;


        public AppSettingsClass(ConfigUtil util)
        {
            this.util = util;
        }


        public string this[string key]
        {
            get { return util.GetAppSetting(key); }
            set { util.AddAppSetting(key, value); }
        }
    }


    public class ConnectionStringsClass
    {
        private ConfigUtil util;


        public ConnectionStringsClass(ConfigUtil util)
        {
            this.util = util;
        }


        public string this[string key]
        {
            get { return util.GetConnectionString(key); }
            set { util.AddConnectionString(key, value); }
        }
    }


    public class AppSettingsStatic
    {
        public string this[string key]
        {
            get { return ConfigurationManager.AppSettings[key]; }
        }
    }


    public class ConnStringsStatic
    {
        public string this[string key]
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings[key] != null)
                    return ConfigurationManager.ConnectionStrings[key].ConnectionString;

                return null;
            }
        }
    }

    public enum ConfigType
    {
        EXE, WEB
    }
}
