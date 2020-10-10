
using AmongUsMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectX_Renderer;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using ThreadHandler;
using DirectX_Renderer.GUI;

namespace YourCheat
{
    class Program
    {
        static int tableWidth = 100;

        static Task initCheatTask = null;
        static Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();


        static List<PlayerData> playerDatas = new List<PlayerData>();

        static void UpdateCheat()
        {
            while (!BaseGUI_Constants.GetExeWasClosedValue())
            {
                //Console.Clear();
                Console.WriteLine("Test Read Player Datas..");
                PrintRow("offset", "Name", "OwnerId", "PlayerId", "spawnid", "spawnflag", "isImpostor");
                PrintLine();

                foreach (var data in playerDatas)
                {
                    if (data.IsLocalPlayer)
                        Console.ForegroundColor = ConsoleColor.Green;
                    if (data.PlayerInfo.Value.IsDead == 1)
                        Console.ForegroundColor = ConsoleColor.Red;

                    var Name = AmongUsMemory.Utils.ReadString(data.PlayerInfo.Value.PlayerName);
                    PrintRow($"{(data.IsLocalPlayer == true ? "Me->" : "")}{data.offset_str}", $"{Name}", $"{data.Instance.OwnerId}", $"{data.Instance.PlayerId}", $"{data.Instance.SpawnId}", $"{data.Instance.SpawnFlags}", $"{(data.PlayerInfo.Value.IsImpostor == 1 ? "Yes" : "No")}");
                    Console.ForegroundColor = ConsoleColor.White;

                    /*if (data.PlayerInfo.Value.IsImpostor == 1 && !ValuesDx3.impostorName.Equals(Name)) {
                        ValuesDx3.impostorName = Name;
                    }*/

                    PrintLine();
                }
                System.Threading.Thread.Sleep(5000);
            }

        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {

            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(ThreadException.UIThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(ThreadException.CurrentDomain_UnhandledException);

            // we need to init and set up the version
            VersionController.InitVersion();


            while (true) {
                // Cheat init
                if (initCheatTask == null) {

                    Console.WriteLine("Searching for process Among Us.exe");

                    CancellationTokenSource cts = new CancellationTokenSource();
                    initCheatTask = Task.Factory.StartNew(
                        InitCheat
                    , cts.Token);

                    // Catch task Exception
                    initCheatTask.ContinueWith(ThreadException.Task_UnhandledException, TaskContinuationOptions.OnlyOnFaulted);

                    Tokens.Add("InitCheat", cts);


                    // CHECK STATE CHEAT AND RESTART IF NECESARY
                    CancellationTokenSource ctsStateCheat = new CancellationTokenSource();
                    var checkStateCheatTask = Task.Factory.StartNew(
                        CheckStateCheatAndRestart
                    , ctsStateCheat.Token);
                    checkStateCheatTask.ContinueWith(ThreadException.Task_UnhandledException, TaskContinuationOptions.OnlyOnFaulted);
                    Tokens.Add("CheckStateCheatAndRestart", cts);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        static void InitCheat() {
            while (Tokens.ContainsKey("InitCheat") && Tokens["InitCheat"].IsCancellationRequested == false) {

                if (AmongUsMemory.MemoryData.Init())
                {

                    // Starts the GUI
                    ValuesDx3_Controller.StartGUI();
                    MenuDx3_Controller.StartGUI();


                    // Update Player Data When Every Game
                    AmongUsMemory.MemoryData.ObserveShipStatus((x) =>
                    {

                        Console.WriteLine("Done!");

                        // Enter on join game and on exit game.

                        foreach (var player in playerDatas)
                        {
                            player.StopObserveState();
                        }

                        //ValuesDx3.impostorName = "none";


                        playerDatas = AmongUsMemory.MemoryData.GetAllPlayers();


                        foreach (var player in playerDatas)
                        {
                            player.onDie += (pos, colorId) => {
                                Console.WriteLine("OnPlayerDied! Color ID :" + colorId);
                            };
                            // player state check
                            player.StartObserveState();
                        }


                    });

                    // Cheat Logic
                    CancellationTokenSource cts = new CancellationTokenSource();
                    var taskUpdateCheat = Task.Factory.StartNew(
                        UpdateCheat
                    , cts.Token);

                    // Catch task Exception
                    taskUpdateCheat.ContinueWith(ThreadException.Task_UnhandledException, TaskContinuationOptions.OnlyOnFaulted);


                    //Ends Init Cheat
                    if (Tokens.ContainsKey("InitCheat") && Tokens["InitCheat"].IsCancellationRequested == false)
                    {
                        Tokens["InitCheat"].Cancel();
                        Tokens.Remove("InitCheat");
                    }
                }
            }
        }

        public static void CheckStateCheatAndRestart() {
            while (Tokens.ContainsKey("CheckStateCheatAndRestart") && Tokens["CheckStateCheatAndRestart"].IsCancellationRequested == false)
            {
                if (BaseGUI_Constants.GetExeWasClosedValue())
                {
                    initCheatTask = null;
                    BaseGUI_Constants.CleanProcess();
                    BaseGUI_Constants.CleanExeWasClosedValue();
                    if (Tokens.ContainsKey("UpdateCheat") && !Tokens["UpdateCheat"].IsCancellationRequested) {
                        Tokens["UpdateCheat"].Cancel();
                        Tokens.Remove("UpdateCheat");
                    }
                    Tokens["CheckStateCheatAndRestart"].Cancel();
                    Tokens.Remove("CheckStateCheatAndRestart");
                }
            }
        }

        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);

            
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        } 
    }
}


