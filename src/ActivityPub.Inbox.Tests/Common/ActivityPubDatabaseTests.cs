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

using ActivityPub.Inbox.Common;
using Moq;

namespace ActivityPub.Inbox.Tests.Common
{
    [TestClass]
    [DoNotParallelize] // <- Makes sure we don't share the .db file on multiple threads.
    public sealed class ActivityPubDatabaseTests
    {
        // ---------------- Fields ----------------

        private ActivityPubSiteConfig[]? sites;

        private FileInfo? dbFile;

        private ActivityPubDatabase? uut;

        private Mock<Serilog.ILogger>? mockLog;

        // ---------------- Setup / Teardown ----------------

        [TestInitialize]
        public void TestSetup()
        {
            sites = new ActivityPubSiteConfig[]
            {
                new ActivityPubSiteConfig(
                    "roclongboarding.info",
                    new FileInfo( "private" ),
                    new FileInfo( "public" ),
                    new Uri( "https://www.roclongboarding.info/activitypub/profile.json" )
                ),
                new ActivityPubSiteConfig(
                    "troop53stories",
                    new FileInfo( "private" ),
                    new FileInfo( "public" ),
                    new Uri( "https://troop53stories.shendrick.net/activitypub/profile.json" )
                )
            };

            string assemblyPath = typeof( ActivityPubDatabaseTests ).Assembly.Location;
            string? directory = Path.GetDirectoryName( assemblyPath );
            if( directory is null )
            {
                throw new InvalidOperationException(
                    "Assembly directory name somehow null"
                );
            }

            this.dbFile = new FileInfo(
                Path.Combine( directory, "test.db" )
            );

            if( this.dbFile.Exists )
            {
                File.Delete( dbFile.FullName );
            }

            this.mockLog = new Mock<Serilog.ILogger>( MockBehavior.Loose );
            this.uut = new ActivityPubDatabase(
                this.dbFile,
                this.mockLog.Object,
                false // <- Needs to be false for some reason in unit test land.
            );
            this.uut.EnsureCreated();
        }

        [TestCleanup]
        public void TestTeardown()
        {
            this.uut?.Dispose();
            this.uut = null;

            if( ( this.dbFile is not null ) && this.dbFile.Exists )
            {
                File.Delete( dbFile.FullName );
                this.dbFile = null;
            }
        }

        // ---------------- Properties ----------------

        public ActivityPubSiteConfig[] Sites
        {
            get
            {
                Assert.IsNotNull( this.sites );
                return this.sites;
            }
        }

        public ActivityPubDatabase Uut
        {
            get
            {
                Assert.IsNotNull( this.uut );
                return this.uut;
            }
        }

        // ---------------- Tests ----------------

        [TestMethod]
        public void AddSitesTest()
        {
            // Setup
            var sites = new ActivityPubSiteConfig[]
            {
                new ActivityPubSiteConfig(
                    "roclongboarding.info",
                    new FileInfo( "private" ),
                    new FileInfo( "public" ),
                    new Uri( "https://www.roclongboarding.info/activitypub/profile.json" )
                ),
                new ActivityPubSiteConfig(
                    "troop53stories",
                    new FileInfo( "private" ),
                    new FileInfo( "public" ),
                    new Uri( "https://troop53stories.shendrick.net/activitypub/profile.json" )
                )
            };

            // Act
            AddSites();
            IEnumerable<string> siteIds = this.Uut.GetAllSiteIds();

            // Check
            Assert.AreEqual( 2, siteIds.Count() );
            Assert.IsTrue( siteIds.Contains( sites[0].Id ) );
            Assert.IsTrue( siteIds.Contains( sites[1].Id ) );

            // Make sure if we try to add the sites again,
            // nothing happens.
            AddSites();
            siteIds = this.Uut.GetAllSiteIds();
            Assert.AreEqual( 2, siteIds.Count() );
            Assert.IsTrue( siteIds.Contains( sites[0].Id ) );
            Assert.IsTrue( siteIds.Contains( sites[1].Id ) );
        }

        [TestMethod]
        public void EmptySitesTest()
        {
            // Act
            IEnumerable<string> siteIds = this.Uut.GetAllSiteIds();

            // Check
            Assert.AreEqual( 0, siteIds.Count() );
        }

        [TestMethod]
        public void AddFollowerTest()
        {
            // Setup
            const string followerId = "someactor";
            string siteId = this.Sites[0].Id;

            AddSites();

            // Act
            int newId = this.Uut.AddFollower( siteId, followerId );
            IEnumerable<string> newFollowers = this.Uut.GetAllFollowersForSite( siteId );

            // Check
            Assert.AreEqual( 1, newId );
            Assert.AreEqual( 1, newFollowers.Count() );
            Assert.AreEqual( followerId, newFollowers.First() );
        }

        [TestMethod]
        public void AddFollowersFromMultipleSitesTest()
        {
            // Setup
            const string followerId1 = "someactor1";
            const string followerId2 = "someactor2";
            string siteId1 = this.Sites[0].Id;
            string siteId2 = this.Sites[1].Id;

            AddSites();

            // Act
            int newId1 = this.Uut.AddFollower( siteId1, followerId1 );
            int newId2 = this.Uut.AddFollower( siteId2, followerId2 );

            IEnumerable<string> newFollowers1 = this.Uut.GetAllFollowersForSite( siteId1 );
            IEnumerable<string> newFollowers2 = this.Uut.GetAllFollowersForSite( siteId2 );

            // Check
            Assert.AreEqual( 1, newId1 );
            Assert.AreEqual( 1, newFollowers1.Count() );
            Assert.AreEqual( followerId1, newFollowers1.First() );

            Assert.AreEqual( 2, newId2 );
            Assert.AreEqual( 1, newFollowers2.Count() );
            Assert.AreEqual( followerId2, newFollowers2.First() );
        }

        [TestMethod]
        public void GetEmptyFollowrsTest()
        {
            // Setup
            string siteId1 = this.Sites[0].Id;
            string siteId2 = this.Sites[1].Id;

            AddSites();

            // Act
            IEnumerable<string> followers1 = this.Uut.GetAllFollowersForSite( siteId1 );
            IEnumerable<string> followers2 = this.Uut.GetAllFollowersForSite( siteId2 );

            // Check
            Assert.AreEqual( 0, followers1.Count() );
            Assert.AreEqual( 0, followers2.Count() );
        }

        // ---------------- Test Helpers ----------------

        private void AddSites()
        {
            this.Uut.AddSites( this.Sites );
        }
    }
}
