using AmongUsMemory;
using System;
using System.Collections.Generic;
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

public class ShipStatus {

    private uint _NetId;
    private IntPtr _AllVents;
    private float _MapScale;

    public Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();
    private Action<ShipStatus> CallBack = null;

    public ShipStatus() {
        if (Tokens.ContainsKey("StartObserver") && Tokens["StartObserver"].IsCancellationRequested) { 
            CancellationTokenSource cts = new CancellationTokenSource();
            var taskUpdateCheat = Task.Factory.StartNew(
                StartObserver
            , cts.Token);
            Tokens.Add("StartObserver", cts);
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
        IntPtr GameAssemblyPTR = (IntPtr)Utils.GetModuleAddress(MemoryData.process, Pattern.GameAssembly_Pointer);

        IntPtr ShipStatusPTR = new IntPtr(Convert.ToInt64(Pattern.ShipStatusPTRStr, 16));

        //https://stackoverflow.com/questions/63949242/memory-address-knowing-the-offsets-and-the-base-address-of-a-dll-how-can-i-obta

        uint currentNetId = 0;
        if (currentNetId != this._NetId) {
            if (Tokens.ContainsKey("StartObserver") && !Tokens["StartObserver"].IsCancellationRequested) {
                this._NetId = currentNetId;
                GetAndSet_AllVents();
                GetAndSet_MapScale();
                OnNetIdChange();
            }
        }
    }

    public void StopObserver()
    {
        if (Tokens.ContainsKey("StartObserver") && !Tokens["StartObserver"].IsCancellationRequested)
        {
            Console.WriteLine("Stoping StartObserver!!");
            Tokens["StartObserver"].Cancel();
            Tokens.Remove("StartObserver");
        }
    }


    public void OnNetIdChange() {
        CallBack?.Invoke(this);
        StopObserver();
    }

    public void OnMatchStart(Action<ShipStatus> callback)
    {
        if (CallBack == null) {
            this.CallBack = callback;
        }
    }

    private void GetAndSet_AllVents() { 
        
    }

    private void GetAndSet_MapScale()
    {

    }
}