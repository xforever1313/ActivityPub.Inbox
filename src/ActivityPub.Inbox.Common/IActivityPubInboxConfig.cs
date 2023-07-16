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

namespace ActivityPub.Inbox.Common
{
    public interface IActivityPubInboxConfig
    {
        // ---------------- Properties ----------------

        IEnumerable<ActivityPubSiteConfig> Sites { get; }

        /// <summary>
        /// Location where the database that contains
        /// the followers, likes, etc is located.
        /// </summary>
        FileInfo SqliteDatabaseLocation { get; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Should validate to make sure the configuration is correct.
        /// Should throw an exception if not.
        /// </summary>
        void Validate();
    }
}
