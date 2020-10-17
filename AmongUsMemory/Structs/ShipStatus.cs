using AmongUsMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

/*[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct ShipStatus
{
    [System.Runtime.InteropServices.FieldOffset(0)]     public uint instance;
    [System.Runtime.InteropServices.FieldOffset(4)]     public uint _null;
    [System.Runtime.InteropServices.FieldOffset(8)]     public uint m_CachedPtr;
    [System.Runtime.InteropServices.FieldOffset(12)]    public uint SpawnId;
    [System.Runtime.InteropServices.FieldOffset(16)]    public uint NetId;
    [System.Runtime.InteropServices.FieldOffset(20)]    public uint DirtyBits;
    [System.Runtime.InteropServices.FieldOffset(24)]    public uint SpawnFlags;
    [System.Runtime.InteropServices.FieldOffset(25)]    public uint sendMode;
    [System.Runtime.InteropServices.FieldOffset(28)]    public uint OwnerId;
    [System.Runtime.InteropServices.FieldOffset(32)]    public byte DespawnOnDestroy;
    [System.Runtime.InteropServices.FieldOffset(36)]    public uint CameraColor;
    [System.Runtime.InteropServices.FieldOffset(52)]    public float MaxLightRadius;
    [System.Runtime.InteropServices.FieldOffset(56)]    public float MinLightRadius;
    [System.Runtime.InteropServices.FieldOffset(60)]    public float MapScale;
    [System.Runtime.InteropServices.FieldOffset(64)]    public IntPtr MapPrefab;
    [System.Runtime.InteropServices.FieldOffset(68)]    public IntPtr ExileCutscenePrefab;
    [System.Runtime.InteropServices.FieldOffset(72)]    public uint InitialSpawnCenter;
    [System.Runtime.InteropServices.FieldOffset(80)]    public uint MeetingSpawnCenter;
    [System.Runtime.InteropServices.FieldOffset(88)]    public uint MeetingSpawnCenter2;
    [System.Runtime.InteropServices.FieldOffset(96)]    public float SpawnRadius;
    [System.Runtime.InteropServices.FieldOffset(100)]   public IntPtr CommonTasks;
    [System.Runtime.InteropServices.FieldOffset(104)]   public IntPtr LongTasks;
    [System.Runtime.InteropServices.FieldOffset(108)]   public IntPtr NormalTasks;
    [System.Runtime.InteropServices.FieldOffset(112)]   public IntPtr SpecialTasks;
    [System.Runtime.InteropServices.FieldOffset(116)]   public IntPtr DummyLocations;
    [System.Runtime.InteropServices.FieldOffset(120)]   public IntPtr AllCameras;
    [System.Runtime.InteropServices.FieldOffset(124)]   public IntPtr AllDoors;
    [System.Runtime.InteropServices.FieldOffset(128)]   public IntPtr AllConsoles;
    [System.Runtime.InteropServices.FieldOffset(132)]   public IntPtr Systems;
    [System.Runtime.InteropServices.FieldOffset(136)]   public IntPtr AllStepWatchers;
    [System.Runtime.InteropServices.FieldOffset(140)]   public IntPtr AllRooms;
    [System.Runtime.InteropServices.FieldOffset(144)]   public IntPtr FastRooms;
    [System.Runtime.InteropServices.FieldOffset(148)]   public IntPtr AllVents;
    [System.Runtime.InteropServices.FieldOffset(152)]   public IntPtr WeaponFires;
    [System.Runtime.InteropServices.FieldOffset(156)]   public IntPtr WeaponsImage;
    [System.Runtime.InteropServices.FieldOffset(160)]   public IntPtr VentMoveSounds;
    [System.Runtime.InteropServices.FieldOffset(164)]   public IntPtr VentEnterSound;
    [System.Runtime.InteropServices.FieldOffset(168)]   public IntPtr HatchActive;
    [System.Runtime.InteropServices.FieldOffset(172)]   public IntPtr Hatch;
    [System.Runtime.InteropServices.FieldOffset(176)]   public IntPtr HatchParticles;
    [System.Runtime.InteropServices.FieldOffset(180)]   public IntPtr ShieldsActive;
    [System.Runtime.InteropServices.FieldOffset(184)]   public IntPtr ShieldsImages;
    [System.Runtime.InteropServices.FieldOffset(188)]   public IntPtr ShieldBorder;
    [System.Runtime.InteropServices.FieldOffset(192)]   public IntPtr ShieldBorderOn;
    [System.Runtime.InteropServices.FieldOffset(196)]   public IntPtr MedScanner;
    [System.Runtime.InteropServices.FieldOffset(200)]   public uint WeaponFireIdx;
    [System.Runtime.InteropServices.FieldOffset(204)]   public float Timer;
    [System.Runtime.InteropServices.FieldOffset(208)]   public float EmergencyCooldown;
    [System.Runtime.InteropServices.FieldOffset(212)]   public uint Type;
}*/


public static class ShipStatusThreads {

    public static Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();
    public static Action<ShipStatus> onMatchStartsCallBack = null;
    public static Action<ShipStatus> onMatchEndCallBack = null;
    public static IntPtr BASE_SHIP_STATUS_PTR = IntPtr.Zero;

    public static List<IntPtr> ShipStatusBase_Offsets = new List<IntPtr> {
                (IntPtr)0x50, // offset0
                (IntPtr)0x20, // offset1
                (IntPtr)0x20, // offset2
                (IntPtr)0x50, // offset3
                (IntPtr)0x64, // offset4
                (IntPtr)0x8 // offset5
            };

}


public class ShipStatus {

    private uint _NetId;
    private IntPtr _AllVents;
    private float _MapScale;

    public bool isMatchStarted = false;

    public ShipStatus() {
        if (!ShipStatusThreads.Tokens.ContainsKey("StartObserver")) {
            // we get the base pointer of shipstatus
            ShipStatusThreads.BASE_SHIP_STATUS_PTR = Utils.GetSumOfAddressFromMemory(MemoryData.process, Pattern.ShipStatus_Pointer);

            CancellationTokenSource cts = new CancellationTokenSource();
            var taskUpdateCheat = Task.Factory.StartNew(
                new Action(StartObserver)
            , cts.Token);
            ShipStatusThreads.Tokens.Add("StartObserver", cts);
        }
    }

    public uint NetId {
        get { return _NetId; }
    }

    public IntPtr AllVents
    {
        get { return _AllVents; }
    }

    public float MapScale
    {
        get { return _MapScale; }
    }

    private void StartObserver() {

        while (ShipStatusThreads.Tokens.ContainsKey("StartObserver") && !ShipStatusThreads.Tokens["StartObserver"].IsCancellationRequested) {
            Thread.Sleep(250);

            List<IntPtr> NetId_Offsets = new List<IntPtr>(ShipStatusThreads.ShipStatusBase_Offsets);
            NetId_Offsets.Add((IntPtr)0x10);

            IntPtr netIdPtr = Utils.GetPtrFromOffsets(ShipStatusThreads.BASE_SHIP_STATUS_PTR, NetId_Offsets.ToArray());

            if (netIdPtr.IsValid()) {

                uint currentNetId = (uint)MemoryData.mem.ReadInt(netIdPtr.GetAddress());

                if (currentNetId < 100000 && currentNetId != this._NetId)
                {
                    if (ShipStatusThreads.Tokens.ContainsKey("StartObserver") && !ShipStatusThreads.Tokens["StartObserver"].IsCancellationRequested)
                    {
                        this._NetId = currentNetId;
                        GetAndSet_AllVents();
                        GetAndSet_MapScale();
                        OnNetIdChange();
                    }
                }
                else if (currentNetId > 100000 && isMatchStarted) {
                    // check if the user exits match
                    OnNetIdChange();
                }

            } else if (isMatchStarted) {
                // check if the user exits match
                OnNetIdChange();
            }
        }


    }

    public void StopObserver()
    {
        if (ShipStatusThreads.Tokens.ContainsKey("StartObserver") && !ShipStatusThreads.Tokens["StartObserver"].IsCancellationRequested)
        {
            Console.WriteLine("Stoping StartObserver!!");
            ShipStatusThreads.Tokens["StartObserver"].Cancel();
            ShipStatusThreads.Tokens.Remove("StartObserver");
        }
    }


    public void OnNetIdChange() {
        if (!isMatchStarted)
        {
            ShipStatusThreads.onMatchStartsCallBack?.Invoke(this);
            isMatchStarted = true;
        }
        else {
            ShipStatusThreads.onMatchEndCallBack?.Invoke(this);
            isMatchStarted = false;
        }
    }

    public void OnMatchStart(Action<ShipStatus> callback)
    {
        if (ShipStatusThreads.onMatchStartsCallBack == null) {
            ShipStatusThreads.onMatchStartsCallBack = callback;
        }
    }


    public void OnMatchEnd(Action<ShipStatus> callback) {
        if (ShipStatusThreads.onMatchEndCallBack == null)
        {
            ShipStatusThreads.onMatchEndCallBack = callback;
        }
    }

    private void GetAndSet_AllVents() {

        /*IntPtr[] AllVents_Offsets = {
                (IntPtr)0x50, // offset0
                (IntPtr)0x8, // offset1
                (IntPtr)0x28, // offset2
                (IntPtr)0x5C, // offset3
                (IntPtr)0x0, // offset4
                (IntPtr)0x94, // offset5
                (IntPtr)0x0 // offset6 - Final Offset
            };

        IntPtr allVentsPtr = Utils.GetPtrFromOffsets(ShipStatusThreads.BASE_SHIP_STATUS_PTR, AllVents_Offsets);

        Console.WriteLine(allVentsPtr.GetAddress());*/
    }

    private void GetAndSet_MapScale()
    {

    }
}