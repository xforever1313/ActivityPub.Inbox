﻿//
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

using SethCS.IO;

namespace ActivityPub.Inbox.Web
{
    public sealed class Resources
    {
        // ---------------- Constructor ----------------

        public Resources()
        {
        }

        // ---------------- Functions ----------------

        public string GetLicense()
        {
            return AssemblyResourceReader.ReadStringResource(
                typeof( Resources ).Assembly, $"{nameof( ActivityPub )}.{nameof( Inbox )}.{nameof( Web )}.License.md"
            );
        }

        public string GetCredits()
        {
            return AssemblyResourceReader.ReadStringResource(
                typeof( Resources ).Assembly, $"{nameof( ActivityPub )}.{nameof( Inbox )}.{nameof( Web )}.Credits.md"
            );
        }
    }
}
