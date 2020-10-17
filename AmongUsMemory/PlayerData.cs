﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThreadHandler;

namespace AmongUsMemory
{
    public class PlayerData
    {
        #region ObserveStates
        private bool observe_dieFlag = false;
        #endregion

        public PlayerControl Instance;
        public System.Action<Vector2, byte> onDie; 
        public IntPtr PlayerControl_GetData_Offset = IntPtr.Zero;


        private string playerInfoOffset = null;
        public IntPtr playerInfoOffset_ptr;
        public IntPtr offset_ptr;
        public string offset_str;


        Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();


        public void ObserveState()
        {
            if (PlayerInfo.HasValue)
            {
                if (observe_dieFlag == false && PlayerInfo.Value.IsDead == 1)
                {
                    observe_dieFlag = true;
                    onDie?.Invoke(Position, PlayerInfo.Value.ColorId);
                }
            }
        }


        public PlayerInfo? PlayerInfo
        {
            get
            {
                if (playerInfoOffset_ptr == IntPtr.Zero)
                {
                    var ptr =  Methods.Call_PlayerControl_GetData(this.offset_ptr);
                    playerInfoOffset = ptr.GetAddress();
                    PlayerInfo pInfo = Utils.FromBytes<PlayerInfo>(MemoryData.mem.ReadBytes(playerInfoOffset, Utils.SizeOf<PlayerInfo>()));
                    playerInfoOffset_ptr = new IntPtr(ptr);
                    m_pInfo = pInfo;
                    return m_pInfo;

                }
                else
                {
                    PlayerInfo pInfo = Utils.FromBytes<PlayerInfo>(MemoryData.mem.ReadBytes(playerInfoOffset, Utils.SizeOf<PlayerInfo>()));
                    m_pInfo = pInfo;
                    return m_pInfo;
                }

            }
        }
        private PlayerInfo? m_pInfo = null;

        
        public LightSource LightSource
        {
            get
            {
                var lsPtr = Instance.myLight;
                Console.WriteLine("light source : " + lsPtr.GetAddress());
                var lsBytes = MemoryData.mem.ReadBytes(lsPtr.GetAddress(), Utils.SizeOf<LightSource>());
                var ls = Utils.FromBytes<LightSource>(lsBytes);
                return ls; 
            }
        }
        public void WriteMemory_LightRange(float value)
        {
            var targetPointer = Utils.GetMemberPointer(Instance.myLight, typeof(LightSource), "LightRadius");
            MemoryData.mem.WriteMemory(targetPointer.GetAddress(), "float", value.ToString("0.0"));
        }

        public void WriteMemory_Impostor(byte value)
        {
            var targetPointer = Utils.GetMemberPointer(playerInfoOffset_ptr, typeof(PlayerInfo), "IsImpostor");
            MemoryData.mem.WriteMemory(targetPointer.GetAddress(), "byte", value.ToString());
        }
        /// <summary>
        /// Set Player Dead State.
        /// </summary>
        /// <param name="value"></param>
        public void WriteMemory_IsDead(byte value)
        {
            var targetPointer = Utils.GetMemberPointer(playerInfoOffset_ptr, typeof(PlayerInfo), "IsDead");
            MemoryData.mem.WriteMemory(targetPointer.GetAddress(), "byte", value.ToString());
        }
        /// <summary>
        /// Set Player KillTimer
        /// </summary>
        /// <param name="value"></param>
        public void WriteMemory_KillTimer(float value)
        {
            var targetPointer = Utils.GetMemberPointer(offset_ptr, typeof(PlayerControl), "killTimer");
            MemoryData.mem.WriteMemory(targetPointer.GetAddress(), "float", value.ToString());
        }

        
        
        public void StopObserveState()
        {
            var key = Tokens.ContainsKey("ObserveState");
            if(key)
            {
                if (Tokens["ObserveState"].IsCancellationRequested == false)
                {
                    Tokens["ObserveState"].Cancel();
                    Tokens.Remove("ObserveState");
                }
            } 
        }
        public void StartObserveState()
        {
            if(Tokens.ContainsKey("ObserveState"))
            {
                Console.WriteLine("Already Observed!");
                return;
            }
            else
            {
                CancellationTokenSource cts = new CancellationTokenSource(); 
                var task = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (PlayerInfo.HasValue)
                        {
                            if (observe_dieFlag == false && PlayerInfo.Value.IsDead == 1)
                            {
                                observe_dieFlag = true;
                                onDie?.Invoke(Position, PlayerInfo.Value.ColorId);
                            }
                        }
                        System.Threading.Thread.Sleep(1000); 
                    }
                }, cts.Token);

                // Catch task Exception
                task.ContinueWith(ThreadException.Task_UnhandledException, TaskContinuationOptions.OnlyOnFaulted);

                Tokens.Add("ObserveState", cts);
            }
          
        }

        public Vector2 Position
        {
            get
            {
                if (IsLocalPlayer)
                    return GetMyPosition();
                else
                    return GetSyncPosition();
            }
        }

        public void ReadMemory()
        {
            Instance = Utils.FromBytes<PlayerControl>(MemoryData.mem.ReadBytes(offset_str, Utils.SizeOf<PlayerControl>()));
        }

        public bool IsLocalPlayer
        {
            get
            {
                if (Instance.myLight == IntPtr.Zero) return false;
                else
                {
                    return true;
                }
            }
        }


        public Vector2 GetSyncPosition()
        {
            try
            {
                int _offset_vec2_position = 60;
                int _offset_vec2_sizeOf = 8;
                var netTransform = ((int)Instance.NetTransform + _offset_vec2_position).ToString("X");
                var vec2Data= MemoryData.mem.ReadBytes($"{netTransform}",_offset_vec2_sizeOf); // 주소로부터 8바이트 읽는다   
                if (vec2Data != null && vec2Data.Length != 0)
                {
                    var vec2 = Utils.FromBytes<Vector2>(vec2Data);
                    return vec2;
                }
                else
                {
                    return Vector2.Zero;
                }
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                return Vector2.Zero;
            }
        }
        public Vector2 GetMyPosition()
        {
            try
            {
                int _offset_vec2_position = 80;
                int _offset_vec2_sizeOf = 8;
                var netTransform = ((int)Instance.NetTransform + _offset_vec2_position).ToString("X");
                var vec2Data= MemoryData.mem.ReadBytes($"{netTransform}",_offset_vec2_sizeOf);
                if (vec2Data != null && vec2Data.Length != 0)
                {
                    var vec2 = Utils.FromBytes<Vector2>(vec2Data);
                    return vec2;
                }
                else
                {
                    return Vector2.Zero;
                }
            }
            catch
            {
                return Vector2.Zero;
            }
        }


 

    }
}