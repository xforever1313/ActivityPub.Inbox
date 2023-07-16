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

using System.Collections.ObjectModel;
using Serilog;

namespace ActivityPub.Inbox.Common
{
    public class ActivityPubInboxApi : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly ILogger log;

        private readonly ActivityPubDatabase db;

        private bool isDisposed;

        // ---------------- Constructor ----------------

        public ActivityPubInboxApi(
            IActivityPubInboxConfig config,
            ILogger log
        )
        {
            this.isDisposed = false;

            config.Validate();

            this.Config = config;
            this.log = log;

            this.db = new ActivityPubDatabase(
                this.Config.SqliteDatabaseLocation,
                this.log
            );

            this.Inbox = new InboxHandler( this.log );
            this.Followers = new Followers( this.log );
            this.Version = GetType().Assembly.GetName().Version?.ToString( 3 ) ?? "Unknown Version";

            var siteConfigs = new Dictionary<string, ActivityPubSiteConfig>();
            foreach( ActivityPubSiteConfig siteConfig in config.Sites )
            {
                siteConfigs.Add( siteConfig.Id, siteConfig );
            }
            this.SiteConfigs = new ReadOnlyDictionary<string,ActivityPubSiteConfig>(
                siteConfigs
            );
        }

        // ---------------- Properties ----------------

        public IActivityPubInboxConfig Config { get; private set; }

        public IReadOnlyDictionary<string, ActivityPubSiteConfig> SiteConfigs { get; private set; }

        public InboxHandler Inbox { get; private set; }

        public Followers Followers { get; private set; }

        public string Version { get; private set; }

        // ---------------- Functions ----------------

        public void Init()
        {
            this.db.EnsureCreated();
        }

        public void Dispose()
        {
            if( this.isDisposed )
            {
                return;
            }

            this.db.Dispose();

            GC.SuppressFinalize( this );
            this.isDisposed = true;
        }
    }
}
