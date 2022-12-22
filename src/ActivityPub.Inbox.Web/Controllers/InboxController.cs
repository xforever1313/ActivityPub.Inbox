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
using KristofferStrube.ActivityStreams;
using Microsoft.AspNetCore.Mvc;
using SethCS.Extensions;

namespace ActivityPub.Inbox.Web.Controllers
{
    public class InboxController : Controller
    {
        // ---------------- Fields ----------------

        private readonly ILogger log;

        private readonly ActivityPubInboxApi api;

        // ---------------- Constructor ----------------

        public InboxController( ActivityPubInboxApi api, ILogger log )
        {
            this.log = log;
            this.api = api;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Index( string profileId )
        {
            using( StreamReader reader = new StreamReader( this.Request.Body ) )
            {
                string body = await reader.ReadToEndAsync();
                this.log.LogInformation( body );

                return NotFound( "Not implemented" );
            }

            if( this.api.SiteConfigs.ContainsKey( profileId ) == false )
            {
                return NotFound( "Profile not found" );
            }

            if( "POST".EqualsIgnoreCase( this.Request.Method ) )
            {
                Activity? act = await this.Request.ReadFromJsonAsync<Activity>();
                if( act is null )
                {
                    return BadRequest( "Body was null" );
                }

                await this.api.Inbox.AsyncHandleNewActivity( act );
                return Accepted();
            }
            else
            {
                this.HttpContext.Response.ContentType = "application/activity+json";
                OrderedCollection result = await this.api.Inbox.AsyncGetActivities();
                return new JsonResult( result );
            }
        }
    }
}
