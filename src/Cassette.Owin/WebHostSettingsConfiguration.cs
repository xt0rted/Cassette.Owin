using System;

using Cassette.IO;

namespace Cassette.Owin
{
    internal class WebHostSettingsConfiguration : IConfiguration<CassetteSettings>
    {
        public void Configure(CassetteSettings settings)
        {
            var configSettings = GetConfigurationSection();

            settings.IsDebuggingEnabled = configSettings.Debug.GetValueOrDefault();
            settings.IsHtmlRewritingEnabled = configSettings.RewriteHtml;
            settings.AllowRemoteDiagnostics = configSettings.AllowRemoteDiagnostics;

            settings.SourceDirectory = new FileSystemDirectory(AppDomainAppPath);
            settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
            settings.IsFileSystemWatchingEnabled = true;

            settings.Version += AppDomainAppPath;
        }

        protected virtual string AppDomainAppPath
        {
            get { return AppDomain.CurrentDomain.SetupInformation.ApplicationBase; }
        }

        protected virtual CassetteConfigurationSection GetConfigurationSection()
        {
            return (System.Configuration.ConfigurationManager.GetSection("cassette") as CassetteConfigurationSection) ?? new CassetteConfigurationSection();
        }
    }
}