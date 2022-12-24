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

using dotenv.net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mono.Options;
using Serilog;
using Serilog.Sinks.Telegram;

namespace ActivityPub.WebBuilder
{
    public abstract class ActivityPubWebBuilder
    {
        // ---------------- Fields ----------------

        private readonly string[] args;

        private ActivityPubWebConfig? webConfig;

        // ---------------- Constructor ----------------

        public ActivityPubWebBuilder( string[] args )
        {
            this.args = args;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// This is null until <see cref="Run"/> is called.
        /// </summary>
        public ILogger? Log { get; private set; }

        /// <summary>
        /// When help is passed in, it gets written
        /// to this.
        /// </summary>
        public abstract TextWriter HelpWriter { get; }

        /// <summary>
        /// The applicatoin name, used when writing to Telegram.
        /// </summary>
        public abstract string ApplicationName { get; }

        // ---------------- Functions ----------------

        protected virtual void ConfigureBuilder( WebApplicationBuilder builder )
        {
        }

        protected virtual void ConfigureApp( WebApplication app )
        {
        }

        protected abstract void PrintVersion();

        protected abstract void PrintCredits();

        protected abstract void PrintLicense();

        public int Run()
        {
            bool showHelp = false;
            bool showVersion = false;
            bool showLicense = false;
            bool showCredits = false;
            string envFile = string.Empty;

            var options = new OptionSet
            {
                {
                    "h|help",
                    "Shows thie mesage and exits.",
                    v => showHelp = ( v is not null )
                },
                {
                    "version",
                    "Shows the version and exits.",
                    v => showVersion = ( v is not null )
                },
                {
                    "print_license",
                    "Prints the software license and exits.",
                    v => showLicense = ( v is not null )
                },
                {
                    "print_credits",
                    "Prints the third-party notices and credits.",
                    v => showCredits = ( v is not null )
                },
                {
                    "env=",
                    "The .env file that contains the environment variable settings.",
                    v => envFile = v
                }
            };

            try
            {
                options.Parse( args );

                if( showHelp )
                {
                    options.WriteOptionDescriptions( this.HelpWriter );
                    return 0;
                }
                else if( showVersion )
                {
                    PrintVersion();
                    return 0;
                }
                else if( showLicense )
                {
                    PrintLicense();
                    return 0;
                }
                else if( showCredits )
                {
                    PrintCredits();
                    return 0;
                }

                options.Parse( args );

                if( string.IsNullOrWhiteSpace( envFile ) == false )
                {
                    Console.WriteLine( $"Using .env file located at '{envFile}'" );
                    DotEnv.Load( new DotEnvOptions( envFilePaths: new string[] { envFile } ) );
                }

                RunInternal();

                this.Log?.Information( "Application Exiting" );
                return 0;
            }
            catch( Exception e )
            {
                this.Log?.Fatal( "FATAL ERROR:" + Environment.NewLine + e );
                return -1;
            }
        }

        private void RunInternal()
        {
            this.webConfig = ActivityPubWebConfigExtensions.FromEnvVar();
            this.Log = CreateLog();

            WebApplicationBuilder builder = WebApplication.CreateBuilder( args );
            this.ConfigureBuilder( builder );

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Host.UseSerilog( this.Log );

            WebApplication app = builder.Build();
            if( string.IsNullOrWhiteSpace( this.webConfig.BasePath ) == false )
            {
                app.Use(
                    ( HttpContext context, RequestDelegate next ) =>
                    {
                        context.Request.PathBase = this.webConfig.BasePath;
                        return next( context );
                    }
                );
            }

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                }
            );

            if( this.webConfig.AllowPorts == false )
            {
                app.Use(
                    ( HttpContext context, RequestDelegate next ) =>
                    {
                        int? port = context.Request.Host.Port;
                        if( port is not null )
                        {
                            // Kill the connection,
                            // and stop all processing.
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            context.Connection.RequestClose();
                            return Task.CompletedTask;
                        }

                        return next( context );
                    }
                );
            }

            app.UseHostFiltering();

            // Configure the HTTP request pipeline.
            if( !app.Environment.IsDevelopment() )
            {
                app.UseExceptionHandler( "/Home/Error" );
            }

            app.UseRouting();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }

        private ILogger CreateLog()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console( Serilog.Events.LogEventLevel.Information );

            bool useFileLogger = false;
            bool useTelegramLogger = false;

            FileInfo? logFile = this.webConfig.LogFile;
            if( logFile is not null )
            {
                useFileLogger = true;
                logger.WriteTo.File(
                    logFile.FullName,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    retainedFileCountLimit: 10,
                    fileSizeLimitBytes: 512 * 1000 * 1000, // 512 MB
                    shared: false
                );
            }

            string? telegramBotToken = this.webConfig.TelegramBotToken;
            string? telegramChatId = this.webConfig.TelegramChatId;
            if(
                ( string.IsNullOrWhiteSpace( telegramBotToken ) == false ) &&
                ( string.IsNullOrWhiteSpace( telegramChatId ) == false )
            )
            {
                useTelegramLogger = true;
                var telegramOptions = new TelegramSinkOptions(
                    botToken: telegramBotToken,
                    chatId: telegramChatId,
                    dateFormat: "dd.MM.yyyy HH:mm:sszzz",
                    applicationName: $"{this.ApplicationName}",
                    failureCallback: this.OnTelegramFailure
                );
                logger.WriteTo.Telegram(
                    telegramOptions,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning
                );
            }

            ILogger log = logger.CreateLogger();
            log.Information( $"Using File Logging: {useFileLogger}." );
            log.Information( $"Using Telegram Logging: {useTelegramLogger}." );

            return log;
        }

        private void OnTelegramFailure( Exception e )
        {
            this.Log?.Warning( $"Telegram message did not send:{Environment.NewLine}{e}" );
        }
    }
}