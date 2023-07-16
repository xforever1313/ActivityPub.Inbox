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

using System.ComponentModel.DataAnnotations;

namespace ActivityPub.Inbox.Common.DatabaseSchema
{
    internal sealed record class Follower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ActorId { get; set; } = "";

        [Required]
        public string SiteId { get; set; } = "";

        /// <summary>
        /// The number of times we tried to accept the follow
        /// request.  Null means we were successful in sending
        /// out a request.
        /// </summary>
        public int? AcceptedAttempts = 0;
    }
}
