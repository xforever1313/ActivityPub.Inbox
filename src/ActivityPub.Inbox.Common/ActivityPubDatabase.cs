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


        public void Dispose()
        {
            GC.SuppressFinalize( this );
        }

        private DatabaseConnection Connect()
        {
            return new DatabaseConnection( this.dbFile, this.pool );
        }
    }
}
