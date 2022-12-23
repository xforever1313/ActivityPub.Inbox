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

using System.Diagnostics;
using ActivityPub.Inbox.Common;
using ActivityPub.Inbox.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ActivityPub.Inbox.Web.Controllers
{
    public sealed class HomeController : Controller
    {
        // ---------------- Fields ----------------

        private readonly ActivityPubInboxApi api;

        private readonly Serilog.ILogger log;

        // ---------------- Constructor ----------------

        public HomeController( ActivityPubInboxApi api, Serilog.ILogger log )
        {
            this.api = api;
            this.log = log;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            return View( new HomeModel( this.api ) );
        }

        public IActionResult License()
        {
            return View( new HomeModel( this.api ) );
        }

        public IActionResult Credits()
        {
            return View( new HomeModel( this.api ) );
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                } 
            );
        }
    }
}
