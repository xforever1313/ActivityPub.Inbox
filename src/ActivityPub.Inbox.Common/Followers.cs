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
    public class Followers
    {
        // ---------------- Fields ----------------

        private readonly ActivityPubInboxApi api;

        private readonly ILogger log;

        // ---------------- Constructor ----------------

        public Followers( ActivityPubInboxApi api, ILogger log )
        {
            this.api = api;
            this.log = log;
        }

        // ---------------- Functions ----------------

        public void AddFollower( string siteId, Activity follower )
        {
        }

        public IEnumerable<string> GetFollowers( string siteId )
        {
            return new List<string>();
        }
    }
}
