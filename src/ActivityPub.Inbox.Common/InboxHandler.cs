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

using KristofferStrube.ActivityStreams;
using Serilog;
using SethCS.Extensions;

namespace ActivityPub.Inbox.Common
{
    public class InboxHandler
    {
        // ---------------- Fields ----------------

        private readonly ActivityPubInboxApi api;

        private readonly ILogger log;

        // ---------------- Constructor ----------------

        public InboxHandler( ActivityPubInboxApi api, ILogger log )
        {
            this.api = api;
            this.log = log;
        }

        // ---------------- Functions ----------------

        public void HandleNewActivity( string siteId, Activity json )
        {
            string? type = json.Type?.FirstOrDefault();

            if( string.IsNullOrWhiteSpace( type ) )
            {
                throw new InvalidUserOperationException(
                    "Missing 'type' field in Activity request"
                );
            }
            else if( type.EqualsIgnoreCase( "follow" ) )
            {
                // https://www.w3.org/ns/activitystreams#Follow
                this.log.Verbose( $"Received Follow request for site: {siteId}." );
            }
            else if( type.EqualsIgnoreCase( "like" ) )
            {
                // https://www.w3.org/ns/activitystreams#Like
                this.log.Verbose( $"Received Like request for site: {siteId}." );
            }
            else if( type.EqualsIgnoreCase( "announce" ) )
            {
                // https://www.w3.org/ns/activitystreams#Announce
                this.log.Verbose( $"Received Announce request for site: {siteId}." );
            }
            else if( type.EqualsIgnoreCase( "dislike" ) )
            {
                // https://www.w3.org/ns/activitystreams#Dislike
                this.log.Verbose( $"Received Dislike request for site: {siteId}." );
            }
            else if( type.EqualsIgnoreCase( "undo" ) )
            {
                // https://www.w3.org/ns/activitystreams#Undo
                this.log.Verbose( $"Received Undo request for site: {siteId}." );
            }
            else
            {
                throw new UnsupportedActivityType( type );
            }
        }

        public Task AsyncHandleNewActivity( string siteId, Activity json )
        {
            return Task.Run( () => HandleNewActivity( siteId, json ) );
        }

        public OrderedCollection GetActivities( string siteId )
        {
            return new OrderedCollection
            {
                Type = new string[] { "OrderedCollection" }
            };
        }

        public Task<OrderedCollection> AsyncGetActivities( string siteId )
        {
            return Task.Run( () => GetActivities( siteId ) );
        }
    }
}
