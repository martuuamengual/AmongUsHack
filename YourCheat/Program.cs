
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

namespace YourCheat
{
    class Program
    {
        static int tableWidth = 75;

       
        static List<PlayerData> playerDatas = new List<PlayerData>(); 
        static void UpdateCheat()
        {
       
            while (true)
            { 
                Console.Clear();
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
                   PrintRow($"{(data.IsLocalPlayer == true ? "Me->" : "")}{data.offset_str}", $"{Name}", $"{data.Instance.OwnerId}", $"{data.Instance.PlayerId}", $"{data.Instance.SpawnId}", $"{data.Instance.SpawnFlags}", $"{data.PlayerInfo.Value.IsImpostor}");
                   Console.ForegroundColor = ConsoleColor.White; 
            
                   PrintLine();
                }
                Console.ReadLine();
                System.Threading.Thread.Sleep(100);
            }
        }
        static void Main(string[] args)
        {
            // Cheat Init
            if (AmongUsMemory.MemoryData.Init())
            {

                Console.WriteLine("Searching");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ValuesDx3(MemoryData.process));

                // Update Player Data When Every Game
                AmongUsMemory.MemoryData.ObserveShipStatus((x) =>
                {
                    
                    foreach(var player in playerDatas)
                    {
                        player.StopObserveState();
                    }


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
                Task.Factory.StartNew(
                    UpdateCheat
                , cts.Token); 
            }

            System.Threading.Thread.Sleep(1000000);
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


