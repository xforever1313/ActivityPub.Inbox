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
using SethCS.Exceptions;
using SethCS.Extensions;

namespace ActivityPub.Inbox.Common
{
    public record class ActivityPubSiteConfig(
        string Id,
        FileInfo PrivateKeyFile,
        FileInfo PublicKeyFile,
        Uri ProfileUrl
    )
    {
        public virtual bool Equals( ActivityPubSiteConfig? other )
        {
            if( other is null )
            {
                return false;
            }

            return
                this.PrivateKeyFile.FullName.Equals( other.PrivateKeyFile.FullName ) &&
                this.PublicKeyFile.FullName.Equals( other.PublicKeyFile.FullName ) &&
                this.ProfileUrl.Equals( other.ProfileUrl ) &&
                this.Id.Equals( other.Id );
        }

        public override int GetHashCode()
        {
            return
                this.PrivateKeyFile.FullName.GetHashCode() +
                this.PublicKeyFile.FullName.GetHashCode() +
                this.ProfileUrl.GetHashCode() +
                this.Id.GetHashCode();
        }

        public IEnumerable<string> TryValidate()
        {
            var errors = new List<string>();

            if( this.PrivateKeyFile.Exists == false )
            {
                errors.Add( $"{this.PrivateKeyFile.FullName} does not exist!" );
            }

            if( this.PublicKeyFile.Exists == false )
            {
                errors.Add( $"{this.PublicKeyFile.FullName} does not exist!" );
            }

            if( string.IsNullOrWhiteSpace( this.Id ) )
            {
                errors.Add( $"{this.Id} can not be null, empty, or whitespace!" );
            }

            return errors;
        }

        public void Validate()
        {
            IEnumerable<string> errors = TryValidate();

            if( errors.Any() )
            {
                throw new ListedValidationException(
                    $"Errors found when validating {nameof( ActivityPubSiteConfig )}",
                    errors
                );
            }
        }
    }

    public static class ActivityPubSiteConfigExtensions
    {
        // ---------------- Fields ----------------

        private const string SiteConfigElementName = "Site";

        // ---------------- Functions ----------------

        public static ActivityPubSiteConfig DeserializeSiteConfig( XElement siteElement )
        {
            if( SiteConfigElementName.EqualsIgnoreCase( siteElement.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Passed in XML element doesn't have correct name.  Expected: {SiteConfigElementName}, Got: {siteElement.Name.LocalName}.",
                    nameof( siteElement )
                );
            }

            FileInfo? privateKeyFile = null;
            FileInfo? publicKeyFile = null;
            Uri? profileUrl = null;
            string? id = null;

            foreach( XElement element in siteElement.Elements() )
            {
                string name = element.Name.LocalName;
                if( string.IsNullOrWhiteSpace( name ) )
                {
                    continue;
                }
                else if( "PrivateKeyFile".EqualsIgnoreCase( name ) )
                {
                    privateKeyFile = new FileInfo( element.Value );
                }
                else if( "PublicKeyFile".EqualsIgnoreCase( name ) )
                {
                    publicKeyFile = new FileInfo( element.Value );
                }
                else if( "ProfileUrl".EqualsIgnoreCase( name ) )
                {
                    profileUrl = new Uri( element.Value );
                }
            }

            foreach( XAttribute attr in siteElement.Attributes() )
            {
                string name = attr.Name.LocalName;
                if( string.IsNullOrEmpty( name ) )
                {
                    continue;
                }
                else if( "id".EqualsIgnoreCase( name ) )
                {
                    id = attr.Value;
                }
            }

            if(
                ( privateKeyFile is null ) ||
                ( publicKeyFile is null ) ||
                ( profileUrl is null ) ||
                ( id is null )
            )
            {
                var missing = new List<string>();

                if( privateKeyFile is null ) { missing.Add( nameof( privateKeyFile ) ); }
                if( publicKeyFile is null ) { missing.Add( nameof( publicKeyFile ) ); }
                if( profileUrl is null ) { missing.Add( nameof( profileUrl ) ); }
                if( id is null ) { missing.Add( nameof( id ) ); }

                throw new ListedValidationException(
                    "Missing the following from the XML file for a site.",
                    missing
                );
            }

            return new ActivityPubSiteConfig(
                id,
                privateKeyFile,
                publicKeyFile,
                profileUrl
            );
        }

        public static IEnumerable<ActivityPubSiteConfig> DeserializeSiteConfigs( XDocument doc )
        {
            XElement? root = doc.Root;
            if( root is null )
            {
                throw new ArgumentException(
                    "Root of the site config XML document is null.",
                    nameof( doc )
                );
            }

            var configs = new List<ActivityPubSiteConfig>();

            foreach( XElement element in root.Elements() )
            {
                if( SiteConfigElementName.EqualsIgnoreCase( element.Name.LocalName))
                {
                    configs.Add( DeserializeSiteConfig( element ) );
                }
            }

            return configs;
        }
    }
}
