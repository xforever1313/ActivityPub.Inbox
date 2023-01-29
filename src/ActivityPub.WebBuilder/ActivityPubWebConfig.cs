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

namespace ActivityPub.WebBuilder
{
    public record class ActivityPubWebConfig
    {
        // ---------------- Properties ----------------

        /// <summary>
        /// Set this if the service is running not on the root
        /// of the URL.
        /// </summary>
        public string BasePath { get; init; } = "";

        /// <summary>
        /// If the given request has a port in
        /// the URL, should we process it?
        /// 
        /// If false, then each request will 400.
        /// </summary>
        public bool AllowPorts { get; init; } = true;

        /// <summary>
        /// If the requested URL that contains "//" this will
        /// set it to "/" instead if true.
        /// </summary>
        public bool RewriteDoubleSlashes { get; init; } = false;

        /// <summary>
        /// Where to log information or greater messages to.
        /// Leave null for no logging to files.
        /// </summary>
        public FileInfo? LogFile { get; init; } = null;

        public string? TelegramBotToken { get; init; } = null;

        public string? TelegramChatId { get; init; } = null;
    }

    internal static class ActivityPubWebConfigExtensions
    {
        // ---------------- Functions ----------------

        public static ActivityPubWebConfig FromEnvVar()
        {
            bool NotNull( string envName, out string envValue )
            {
                envValue = Environment.GetEnvironmentVariable( envName ) ?? "";
                return string.IsNullOrWhiteSpace( envValue ) == false;
            }

            var settings = new ActivityPubWebConfig();

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

            if( NotNull( "APP_STRIP_DOUBLE_SLASH", out string stripDoubleSlash ) )
            {
                settings = settings with
                {
                    RewriteDoubleSlashes = bool.Parse( stripDoubleSlash )
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
