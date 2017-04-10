using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Diagnostics;

using Mono.Options;

namespace SlipStream.Server
{
    class DevServerProgram
    {
        private static ShellSettings CreateSettingsFromProgramArgs(string[] args)
        {
            bool isShowHelp = false;
            bool isShowVersion = false;
            string configPath = null;
            var p = new OptionSet() {
                { "h|?|help",  v => isShowHelp = v != null },
                { "version",   v => isShowVersion = v != null },
                { "c|_shellSettings=", v => configPath = v },
            };

            var extra = p.Parse(args);

            if (isShowHelp)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  slipserver.exe [Options]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  -c CONF_FILE\t\tLoad settings from CONF_FILE");
                Console.WriteLine("  --version\t\tShow version information");
                Console.WriteLine("  -h|-?|--help\t\tShow this help text");
                System.Environment.Exit(0);
            }

            if (isShowVersion)
            {
                //TODO 显示版本
                System.Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(configPath))
            {
                return new ShellSettings();
            }
            else
            {
                return ShellSettings.Load(configPath);
            }
        }

        private static void OnExit(object sender, EventArgs args)
        {
            Console.WriteLine("ObjectServer Server was existed successfully.");
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("ObjectServer Development Server\n");

            try
            {
                InitializeFramework(args);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

                using (var server = new ServerProcess())
                {
                    var serverThreadProc = new ThreadStart(delegate
                    {
                        server.Run();
                    });

                    var serverThread = new Thread(serverThreadProc);
                    serverThread.Start();

                    Console.WriteLine("Waiting for client requests...");
                    WaitToQuit();

                    Console.WriteLine("Starting to broadcast the TERMINATE command...");
                    var role = SlipstreamEnvironment.Settings.Role;
                    if (role == ServerRoles.Standalone || role == ServerRoles.Controller)
                    {
                        server.BeginStopAll();
                    }
                    else if (role == ServerRoles.HttpServer)
                    {
                        server.BeginStopHttpServer();
                    }
                    else if (role == ServerRoles.Worker)
                    {
                        server.BeginStopRpcWorkers();
                    }

                    Console.WriteLine("Terminating...");
                }

                return 0;
            }
            catch (OptionException ex)
            {
                Console.WriteLine("Bad argument(s): ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `slipserver.exe --help' for more information.");
                return -1;
            }
            catch (Exception ex)
            {
                LoggerProvider.EnvironmentLogger.ErrorException(
                    "Unhandled Exception: " + ex.Message, ex);
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unhandled exception:\n{0}", ex.Message);
                Console.ForegroundColor = oldColor;
                return -1;
            }
            finally
            {
                SlipstreamEnvironment.Shutdown();
            }
        }

        private static void WaitToQuit()
        {
            do
            {
                Console.WriteLine("Press 'Q' to stop.");
            } while (Char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Q');
        }

        private static void InitializeFramework(string[] args)
        {

            Debug.Assert(!SlipstreamEnvironment.Initialized);

            var cfg = CreateSettingsFromProgramArgs(args);
            cfg.LogToConsole = true;

            Console.WriteLine("Initializing Slipstream framework...");
            SlipstreamEnvironment.Initialize(cfg);

            Console.WriteLine("Slipstream framework has benn initialized.");

            Console.WriteLine("Log Directory=[{0}]，Application Server URL=[{1}]",
                SlipstreamEnvironment.Settings.LogPath,
                SlipstreamEnvironment.Settings.RpcBusUrl);
        }

    }
}
