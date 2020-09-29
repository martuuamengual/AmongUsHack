using DirectX_Renderer;
using ProcessUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadHandler;

namespace AmongUsMemory
{
    public static class MemoryData
    {

        public static Memory.Mem mem = new Memory.Mem();
        public static ProcessMemory ProcessMemory = null;
        public static Process process = null;


        // Constants

        public static string VERSION_GAME = "v2020.9.9s";


        public static bool Init()
        {
            var isReady = OpenProcessAndCheckIsReady();

            if (isReady)
            {
                Overlay_SharpDX_Constants.Restart();
                Methods.Init();
                return true;
            }
            return false;
        }

        static bool OpenProcessAndCheckIsReady() {
            var isOpen = OpenProcess("Among Us");
            if (isOpen) {
                byte[] versionBytes = null;
                int timerLimit = 3;
                int timerCount = 0;
                bool isTimeOut = false;
                while (!Utils.isValidByteArray(versionBytes) && timerCount <= timerLimit)
                {
                    // We need to close process and then open it again because Data is malformed when game initialize.
                    // So we need to wait.
                    Thread.Sleep(2000); // Prevents over processing
                    mem.CloseProcess();
                    var preventFakeData_State = OpenProcess("Among Us");
                    if (preventFakeData_State)
                    {
                        versionBytes = MemoryData.mem.ReadBytes(Pattern.Version_Pointer, VERSION_GAME.Length);
                    }

                    // Time out
                    timerCount++;

                    if (timerCount == timerLimit+1) {
                        isTimeOut = true;
                    }
                }
                string version = System.Text.Encoding.UTF8.GetString(versionBytes);
                Console.WriteLine(version);
                if (version != VERSION_GAME || isTimeOut)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("IMPORTANT! We detect that the cheat may not work correctly, we recommend to restart it, and if persist please update it to the latest version published.");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("CHEAT VERSION: " + "v1.0" + "-" + VERSION_GAME);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (isTimeOut) {
                        Thread.Sleep(100000000);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        static bool OpenProcess(string processName)
        {
            var state = mem.OpenProcess(processName);

            if (state)
            {
                Process proc = Process.GetProcessesByName(processName)[0];
                process = proc;
                ProcessMemory = new ProcessMemory(proc);
                ProcessMemory.Open(ProcessAccess.AllAccess);
                return true;
            }

            return false;
        }

        //private static ShipStatus shipStatus;
        static Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();
        static System.Action<uint> onChangeShipStatus;


        static void _ObserveShipStatus()
        {
            while (Tokens.ContainsKey("ObserveShipStatus") && Tokens["ObserveShipStatus"].IsCancellationRequested == false)
            {
                Thread.Sleep(250);
                Console.WriteLine("ObserveShipStatus-1");
                ShipStatus shipStatus = new ShipStatus();
                shipStatus.OnMatchStart((ShipStatus current) => {
                    Console.WriteLine("Mach starts!");
                    onChangeShipStatus?.Invoke((uint)100);
                });

                shipStatus.OnMatchEnd((ShipStatus current) => {
                    Console.WriteLine("Match ends!");
                    shipStatus = null;
                });
            }
        }


        public static void ObserveShipStatus(System.Action<uint> onChangeShipStatus)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (Tokens.ContainsKey("ObserveShipStatus"))
            {
                Tokens["ObserveShipStatus"].Cancel();
                Tokens.Remove("ObserveShipStatus");
            }

            Tokens.Add("ObserveShipStatus", cts);
            MemoryData.onChangeShipStatus = onChangeShipStatus;
            var task = Task.Factory.StartNew(_ObserveShipStatus, cts.Token);

            // Catch task Exception
            task.ContinueWith(ThreadException.Task_UnhandledException, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static Exception Exception(string v)
        {
            throw new NotImplementedException();
        }

        public static string MakeAobString(byte[] aobTarget, int length, string unknownText = "?? ?? ?? ??")
        {
            int cnt = 0;
            // aob pattern
            string aobData = "";
            // read 4byte aob pattern.
            foreach (var _byte in aobTarget)
            {
                if (_byte < 16)
                    aobData += "0" + _byte.ToString("X");
                else
                    aobData += _byte.ToString("X");

                if (cnt + 1 != 4)
                    aobData += " ";

                cnt++;
                if (cnt == length)
                {
                    aobData += $" {unknownText}";
                    break;
                }
            }
            return aobData;
        }
        public static List<PlayerData> GetAllPlayers()
        {
            List<PlayerData > datas = new List<PlayerData>();

            // find player pointer
            byte[] playerAoB = MemoryData.mem.ReadBytes(Pattern.PlayerControl_Pointer, Utils.SizeOf<PlayerControl>());
            // aob pattern
            string aobData = MakeAobString(playerAoB, 4, "?? ?? ?? ??"); 
            // get result 
            var result = MemoryData.mem.AoBScan(aobData, true, true);
            result.Wait();


            var results =    result.Result;
            // real-player
            foreach (var x in results)
            {
                var bytes = MemoryData.mem.ReadBytes(x.GetAddress(), Utils.SizeOf<PlayerControl>());
                var PlayerControl = Utils.FromBytes<PlayerControl>(bytes);
                // filter garbage instance datas.
                if (PlayerControl.SpawnFlags == 257 && PlayerControl.NetId < uint.MaxValue - 10000)
                {
                    datas.Add(new PlayerData()
                    {
                        Instance = PlayerControl,
                        offset_str = x.GetAddress(),
                        offset_ptr = new IntPtr((int)x)
                    });
                }
            }
            Console.WriteLine("data => " + datas.Count);
            return datas;
        }


    }
}
