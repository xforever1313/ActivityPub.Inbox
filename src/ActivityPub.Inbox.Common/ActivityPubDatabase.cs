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

using ActivityPub.Inbox.Common.DatabaseSchema;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ActivityPub.Inbox.Common
{
    public class ActivityPubDatabase : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly FileInfo dbFile;

        private readonly ILogger log;

        private readonly bool pool;

        // ---------------- Constructor ----------------

        public ActivityPubDatabase( FileInfo dbFile, ILogger log ) :
            this( dbFile, log, true )
        {
        }

        public ActivityPubDatabase( FileInfo dbFile, ILogger log, bool pool )
        {
            this.log = log;
            this.dbFile = dbFile;
            this.pool = pool;
        }

        // ---------------- Functions ----------------

        public void EnsureCreated()
        {
            this.log.Information( $"Ensuring database exists at: {this.dbFile.FullName}" );
            using( DatabaseConnection databaseConnection = Connect() )
            {
                databaseConnection.EnsureCreated();
            }
        }

        /// <summary>
        /// Adds the sites to the database so that they can be foreign
        /// keys.  If a site whose <see cref="ActivityPubSiteConfig.Id"/>
        /// already exists in the database, it is not added an a no-op happens.
        /// </summary>
        public void AddSites( IEnumerable<ActivityPubSiteConfig> sites )
        {
            using( DatabaseConnection db = Connect() )
            {
                DbSet<Site> sitesTable = db.SafeGetSitesTable();
                foreach( ActivityPubSiteConfig site in sites )
                {
                    if( sitesTable.Any( s => s.SiteId == site.Id ) == false )
                    {
                        var newSite = new Site
                        {
                            SiteId = site.Id
                        };

                        sitesTable.Add( newSite );
                        db.SaveChanges();

                        this.log.Debug( $"Added {site.Id} to database." );
                    }
                }
            }
        }

        public IEnumerable<string> GetAllSiteIds()
        {
            using( DatabaseConnection db = Connect() )
            {
                if( db.Sites is null )
                {
                    return Array.Empty<string>();
                }

                return db.Sites.Select( s => s.SiteId ).ToArray();
            }
        }

        /// <param name="actorId">
        /// The actor ID from ActivityPub.  This is usually
        /// the URL to their profile.
        /// </param>
        /// <returns>
        /// The new ID of the added follower.
        /// </returns>
        public int AddFollower( string siteId, string actorId )
        {
            int id;
            using( DatabaseConnection db = Connect() )
            {
                DbSet<Site> siteTable = db.SafeGetSitesTable();
                Site? site = siteTable.Where( s => siteId == s.SiteId ).FirstOrDefault();
                if( site is null )
                {
                    throw new ArgumentException(
                        $"Can not find site: {siteId}",
                        nameof( siteId )
                    );
                }

                var follower = new Follower
                {
                    ActorId = actorId,
                    SiteId = site.SiteId,
                    AcceptedAttempts = 0,
                };

                DbSet<Follower> followerTable = db.SafeGetFollowerTable();
                followerTable.Add( follower );
                db.SaveChanges();

                id = follower.Id;

                this.log.Debug( $"Added '{actorId}' as a follower of {siteId}. ID: {id}" );
            }

            return id;
        }

        public IEnumerable<string> GetAllFollowersForSite( string siteId )
        {
            using( DatabaseConnection db = Connect() )
            {
                DbSet<Site> siteTable = db.SafeGetSitesTable();
                Site site = SafeGetSite( siteTable, siteId );

                DbSet<Follower> followers = db.SafeGetFollowerTable();

                return followers.Where(
                    f => ( f.SiteId == site.SiteId )
                ).Select( f => f.ActorId )
                .ToArray();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize( this );
        }

        private DatabaseConnection Connect()
        {
            return new DatabaseConnection( this.dbFile, this.pool );
        }

        private Site SafeGetSite( DbSet<Site> siteTable, string siteId )
        {
            Site? site = siteTable.Where( s => siteId == s.SiteId ).FirstOrDefault();
            if( site is null )
            {
                throw new ArgumentException(
                    $"Can not find site: {siteId}",
                    nameof( siteId )
                );
            }

            return site;
        }
    }
}
