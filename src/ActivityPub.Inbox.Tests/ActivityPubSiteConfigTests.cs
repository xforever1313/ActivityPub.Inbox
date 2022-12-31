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

namespace ActivityPub.Inbox.Tests
{
    [TestClass]
    public class ActivityPubSiteConfigTests
    {
        // ---------------- Tests ----------------

        [TestMethod]
        public void DeserializeTest()
        {
            // Setup
            const string xmlString =
@"
<Sites>
    <Site id=""roclongboarding"">
        <PrivateKeyFile>roclongboarding/private.pem</PrivateKeyFile>
        <PublicKeyFile>roclongboarding/public.pem</PublicKeyFile>
        <ProfileUrl>https://www.roclongboarding.info/activitypub/profile.json</ProfileUrl>
    </Site>
    <Site id=""troop53stories"">
        <PrivateKeyFile>troop53stories/private.pem</PrivateKeyFile>
        <PublicKeyFile>troop53stories/public.pem</PublicKeyFile>
        <ProfileUrl>https://troop53stories.shendrick.net/activitypub/profile.json</ProfileUrl>
    </Site>
</Sites>
";
            var expected0 = new ActivityPubSiteConfig(
                PrivateKeyFile: new FileInfo( "roclongboarding/private.pem" ),
                PublicKeyFile: new FileInfo( "roclongboarding/public.pem" ),
                ProfileUrl: new Uri( "https://www.roclongboarding.info/activitypub/profile.json" ),
                Id: "roclongboarding"
            );

            var expected1 = new ActivityPubSiteConfig(
                PrivateKeyFile: new FileInfo( "troop53stories/private.pem" ),
                PublicKeyFile: new FileInfo( "troop53stories/public.pem" ),
                ProfileUrl: new Uri( "https://troop53stories.shendrick.net/activitypub/profile.json" ),
                Id: "troop53stories"
            );

            // Act
            XDocument doc = XDocument.Parse( xmlString );
            List<ActivityPubSiteConfig> configs = ActivityPubSiteConfigExtensions.DeserializeSiteConfigs(
                doc
            ).ToList();

            // Check
            Assert.AreEqual( 2, configs.Count );
            Assert.AreEqual( expected0, configs[0] );
            Assert.AreEqual( expected1, configs[1] );
        }

        [TestMethod]
        public void XmlMissingPrivateKey()
        {
            // Setup
            const string xmlString =
@"
<Sites>
    <Site id=""roclongboarding"">
        <PublicKeyFile>roclongboarding/public.pem</PublicKeyFile>
        <ProfileUrl>https://www.roclongboarding.info/activitypub/profile.json</ProfileUrl>
    </Site>
</Sites>
";

            // Act
            XDocument doc = XDocument.Parse( xmlString );
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => ActivityPubSiteConfigExtensions.DeserializeSiteConfigs( doc )
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void XmlMissingPublicKey()
        {
            // Setup
            const string xmlString =
@"
<Sites>
    <Site id=""roclongboarding"">
        <PrivateKeyFile>roclongboarding/private.pem</PrivateKeyFile>
        <ProfileUrl>https://www.roclongboarding.info/activitypub/profile.json</ProfileUrl>
    </Site>
</Sites>
";

            // Act
            XDocument doc = XDocument.Parse( xmlString );
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => ActivityPubSiteConfigExtensions.DeserializeSiteConfigs( doc )
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void XmlMissingProfileUrl()
        {
            // Setup
            const string xmlString =
@"
<Sites>
    <Site id=""roclongboarding"">
        <PublicKeyFile>roclongboarding/public.pem</PublicKeyFile>
        <PrivateKeyFile>roclongboarding/private.pem</PrivateKeyFile>
    </Site>
</Sites>
";

            // Act
            XDocument doc = XDocument.Parse( xmlString );
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => ActivityPubSiteConfigExtensions.DeserializeSiteConfigs( doc )
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void XmlMissingId()
        {
            // Setup
            const string xmlString =
@"
<Sites>
    <Site>
        <PublicKeyFile>roclongboarding/public.pem</PublicKeyFile>
        <PrivateKeyFile>roclongboarding/private.pem</PrivateKeyFile>
        <ProfileUrl>https://www.roclongboarding.info/activitypub/profile.json</ProfileUrl>
    </Site>
</Sites>
";

            // Act
            XDocument doc = XDocument.Parse( xmlString );
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => ActivityPubSiteConfigExtensions.DeserializeSiteConfigs( doc )
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void ValidatePrivateKeyTest()
        {
            // Setup
            var uut = new ActivityPubSiteConfig(
                PrivateKeyFile: new FileInfo( "DoesNotExist.pem" ),
                // Pretend our assembly is our public key so this part passes.
                PublicKeyFile: new FileInfo( this.GetType().Assembly.Location ),
                ProfileUrl: new Uri( "https://www.roclongboarding.info/activitypub/profile.json" ),
                Id: "roclongboarding"
            );

            // Act
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => uut.Validate()
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void ValidatePublicKeyTest()
        {
            // Setup
            var uut = new ActivityPubSiteConfig(
                // Pretend our assembly is our private key so this part passes.
                PrivateKeyFile: new FileInfo( this.GetType().Assembly.Location ),
                PublicKeyFile: new FileInfo( "DoesNotExist.pem" ),
                ProfileUrl: new Uri( "https://www.roclongboarding.info/activitypub/profile.json" ),
                Id: "roclongboarding"
            );

            // Act
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => uut.Validate()
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }

        [TestMethod]
        public void ValidateEndPointTest()
        {
            // Setup
            var uut = new ActivityPubSiteConfig(
                // Pretend our assembly is our keys so this part passes.
                PrivateKeyFile: new FileInfo( this.GetType().Assembly.Location ),
                PublicKeyFile: new FileInfo( this.GetType().Assembly.Location ),
                ProfileUrl: new Uri( "https://www.roclongboarding.info/activitypub/profile.json" ),
                Id: ""
            );

            // Act
            ListedValidationException e = Assert.ThrowsException<ListedValidationException>(
                () => uut.Validate()
            );

            // Check
            Assert.AreEqual( 1, e.Errors.Count() );
        }
    }
}