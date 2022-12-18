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
using dotenv.net;
using Mono.Options;
using Serilog;
using Serilog.Sinks.Telegram;

namespace ActivityPub.Inbox.Web
{
    public class Program
    {
        // ---------------- Fields ----------------

        private static Serilog.ILogger? log = null;

        // ---------------- Functions ----------------

        public static int Main( string[] args )
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
                    PrintHelp( options );
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

                Run( args );
                return 0;
            }
            catch( Exception e )
            {
                if( log is null )
                {
                    Console.Error.WriteLine( "FATAL ERROR:" );
                    Console.Error.WriteLine( e.ToString() );
                }
                else
                {
                    log.Fatal( "FATAL ERROR:" + Environment.NewLine + e );
                }
                return -1;
            }
            finally
            {
                log?.Information( "Application Exiting" );
            }
        }

        private static void Run( string[] args )
        {
            ActivityPubInboxConfig config = ActivityPubInboxConfigExtensions.FromEnvVar();
            log = CreateLog( config, OnTelegramFailure );

            using( var api = new ActivityPubInboxApi( config, log ) )
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder( args );
                builder.Services.AddSingleton<ActivityPubInboxApi>( api );

                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Host.UseSerilog( log );
                builder.WebHost.UseUrls( config.Urls );

                WebApplication app = builder.Build();

                // Configure the HTTP request pipeline.
                if( !app.Environment.IsDevelopment() )
                {
                    app.UseExceptionHandler( "/Home/Error" );
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}" );

                app.Run();
            }
        }

        public static Serilog.ILogger CreateLog(
            ActivityPubInboxConfig config,
            Action<Exception> onTelegramFailure
        )
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console( Serilog.Events.LogEventLevel.Information );

            bool useFileLogger = false;
            bool useTelegramLogger = false;

            FileInfo? logFile = config.LogFile;
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

            string? telegramBotToken = config.TelegramBotToken;
            string? telegramChatId = config.TelegramChatId;
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
                    applicationName: $"{typeof( Program ).Assembly.GetName().Name} v{typeof(Program).Assembly.GetName().Version}",
                    failureCallback: onTelegramFailure
                );
                logger.WriteTo.Telegram(
                    telegramOptions,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning
                );
            }

            Serilog.ILogger log = logger.CreateLogger();
            log.Information( $"Using File Logging: {useFileLogger}." );
            log.Information( $"Using Telegram Logging: {useTelegramLogger}." );

            return log;
        }

        private static void OnTelegramFailure( Exception e )
        {
            log?.Warning( $"Telegram message did not send:{Environment.NewLine}{e}" );
        }

        private static void PrintHelp( OptionSet options )
        {
            options.WriteOptionDescriptions( Console.Out );
        }

        private static void PrintVersion()
        {
            Console.WriteLine( typeof( Program ).Assembly.GetName().Version?.ToString( 3 ) ?? "Unknown Version" );
        }

        private static void PrintLicense()
        {
            Console.WriteLine( "NOT IMPLEMENTED YET!" );
        }

        private static void PrintCredits()
        {
            Console.WriteLine( "NOT IMPLEMENTED YET!" );
        }
    }
}
