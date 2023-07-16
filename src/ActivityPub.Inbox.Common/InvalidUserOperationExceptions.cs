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
    /// <summary>
    /// Exception that is thrown if a user trying to write to
    /// the inbox did something wrong.
    /// </summary>
    public class InvalidUserOperationException : Exception
    {
        // ---------------- Constructor ----------------

        public InvalidUserOperationException( string message ) :
            base( message )
        {
        }
    }

    /// <summary>
    /// Exception that is thrown if the inbox does not support
    /// the given activity type.
    /// </summary>
    public class UnsupportedActivityType : InvalidUserOperationException
    {
        // ---------------- Constructor ----------------

        public UnsupportedActivityType( string type ) :
            base( $"The following Activity Type is not supported with this inbox: {type}" )
        {
            this.Type = type;
        }

        // ---------------- Properties ----------------

        public string Type { get; private set; }
    }
}
