//
// ActivityPub.Inbox - Inbox service for https://activitypub.shendrick.net
// Copyright (C) 2022 Seth Hendrick
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Xml.Linq;
using ActivityPub.Inbox.Common;
using SethCS.Exceptions;

namespace ActivityPub.Inbox.Web
{
    public record class ActivityPubInboxConfig : IActivityPubInboxConfig
    {
        // ---------------- Properties ----------------

        public string BasePath { get; init; } = "";

        /// <summary>
        /// If the given request has a port in
        /// the URL, should we process it?
        /// 
        /// If false, then each request will 400.
        /// </summary>
        public bool AllowPorts { get; init; } = true;

        public IEnumerable<ActivityPubSiteConfig> Sites { get; init; } = Array.Empty<ActivityPubSiteConfig>();

        public FileInfo? LogFile { get; init; } = null;

        public string? TelegramBotToken { get; init; } = null;

        public string? TelegramChatId { get; init; } = null;

        // ---------------- Functions ----------------

        public void Validate()
        {
            var errors = new List<string>();

            foreach( ActivityPubSiteConfig siteConfig in this.Sites )
            {
                errors.AddRange( siteConfig.TryValidate() );
            }

            if( errors.Any() )
            {
                throw new ListedValidationException(
                    $"Errors when validating {nameof( ActivityPubInboxConfig )}",
                    errors
                );
            }
        }
    }

    public static class ActivityPubInboxConfigExtensions
    {
        // ---------------- Functions ----------------

        public static ActivityPubInboxConfig FromEnvVar()
        {
            bool NotNull( string envName, out string envValue )
            {
                envValue = Environment.GetEnvironmentVariable( envName ) ?? "";
                return string.IsNullOrWhiteSpace( envValue ) == false;
            }

            var settings = new ActivityPubInboxConfig();

            if( NotNull( "APP_BASEPATH", out string basePath ) )
            {
                settings = settings with
                {
                    BasePath = basePath
                };
            }

            if( NotNull( "APP_ALLOW_PORTS", out string allowPorts ) )
            {
                settings = settings with
                {
                    AllowPorts = bool.Parse( allowPorts )
                };
            }

            if( NotNull( "APP_SITE_CONFIG_FILE", out string siteConfigFile ) )
            {
                XDocument doc = XDocument.Load( siteConfigFile );
                settings = settings with
                { 
                    Sites = ActivityPubSiteConfigExtensions.DeserializeSiteConfigs( doc )
                };
            }

            if( NotNull( "APP_LOG_FILE", out string logFile ) )
            {
                settings = settings with { LogFile = new FileInfo( logFile ) };
            }

            if( NotNull( "APP_TELEGRAM_BOT_TOKEN", out string tgBotToken ) )
            {
                settings = settings with { TelegramBotToken = tgBotToken };
            }

            if( NotNull( "APP_TELEGRAM_CHAT_ID", out string tgChatId ) )
            {
                settings = settings with { TelegramChatId = tgChatId };
            }

            return settings;
        }
    }
}
