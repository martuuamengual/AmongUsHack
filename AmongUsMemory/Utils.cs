using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace AmongUsMemory
{
    public static class Utils
    {
        static Dictionary<(Type, string), int> _offsetMap = new Dictionary<(Type, string), int>();

        public static T FromBytes<T>(byte[] bytes)
        {
            GCHandle gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return data;
        }

        public static T FromByteArrayToObject<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        public static IntPtr FromByteArrayGetAddress(byte[] data)
        {
            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            pinnedArray.Free();
            return pointer;
        }

        public static IntPtr ConvertStringToIntPtr(string address) {
            return new IntPtr(Convert.ToInt64(address, 16));
        }

        public static IntPtr GetSumOfAddressFromMemory(Process process, string address)
        {
            string[] addressArray = address.Split('+');
            List<IntPtr> intPtrList = new List<IntPtr>();

            foreach (string addr in addressArray) {
                if (addr.Contains('.'))
                {
                    // if is .dll or .exe
                    IntPtr baseAddressPTR = (IntPtr)GetModuleAddress(process, addr);
                    intPtrList.Add(baseAddressPTR);
                }
                else {
                    IntPtr addrPTR = ConvertStringToIntPtr(addr);
                    intPtrList.Add(addrPTR);
                }
            }

            IntPtr lastPtr = IntPtr.Zero;

            foreach (IntPtr ptr in intPtrList) {
                lastPtr = lastPtr.Sum(ptr);
            }

            return lastPtr;
        }

        public static int SizeOf<T>()
        {
            var size = Marshal.SizeOf(typeof(T));
            return size;
        }

        public static bool IsValid(this IntPtr value) {
            if (value != null && value != IntPtr.Zero) {
                return true;
            }
            return false;
        }


        public static string GetAddress(this long value) { return value.ToString("X"); }
        public static string GetAddress(this int value) { return value.ToString("X"); }
        public static string GetAddress(this uint value) { return value.ToString("X"); }
        public static string GetAddress(this IntPtr value) { return value.ToInt32().GetAddress(); }
        public static string GetAddress(this UIntPtr value) { return value.ToUInt32().GetAddress(); }

        public static IntPtr Sum(this IntPtr ptr, IntPtr ptr2) { return (IntPtr)(ptr.ToInt32() + ptr2.ToInt32()); }
        public static IntPtr Sum(this IntPtr ptr, UIntPtr ptr2) { return (IntPtr)(ptr.ToInt32() + (int)ptr2.ToUInt32()); }
        public static IntPtr Sum(this UIntPtr ptr, IntPtr ptr2) { return (IntPtr)(ptr.ToUInt32() + ptr2.ToInt32()); }
        public static IntPtr Sum(this int ptr, IntPtr ptr2) { return (IntPtr)(ptr + ptr2.ToInt32()); }
        public static IntPtr Sum(this IntPtr ptr, int ptr2) { return (IntPtr)(ptr.ToInt32() + ptr2); }

        public static IntPtr GetMemberPointer(IntPtr basePtr, Type type, string fieldName)
        {
            var offset = GetOffset(type, fieldName); 
            return basePtr.Sum(offset);
        }
        public static int GetOffset(Type type, string fieldName)
        {
            if (_offsetMap.ContainsKey((type, fieldName)))
            {
                return _offsetMap[(type, fieldName)];
            }
            var field = type.GetField(fieldName);
            var atts = field.GetCustomAttributes(true);
            foreach (var att in atts)
            {
                if (att.GetType() == typeof(FieldOffsetAttribute))
                {
                    _offsetMap.Add((type, fieldName), (att as FieldOffsetAttribute).Value);
                    return (att as FieldOffsetAttribute).Value;
                }
            }

            return -1;
        }

        /// <summary>
        /// Support All Language.
        /// </summary> 
        public static string ReadString(IntPtr offset)
        {
            //string pointer + 8 = length
            var length = MemoryData.mem.ReadInt(offset.Sum(8).GetAddress());

            //unit of string is 2byte.
            var format_length = length * 2;

            //string pointer + 12 = value
            var strByte = MemoryData.mem.ReadBytes(offset.Sum(12).GetAddress(), format_length);

            StringBuilder sb = new StringBuilder(); 
            for (int i = 0; i < strByte.Length; i += 2)
            {
                // english = 1byte
                if (strByte[i + 1] == 0) 
                    sb.Append((char)strByte[i]); 
                // korean & unicode = 2byte
                else
                    sb.Append(System.Text.Encoding.Unicode.GetString(new byte[] { strByte[i], strByte[i + 1] }));
            }

            return sb.ToString();
        }

        public static bool isValidByteArray(byte[] bytes) { 
            if (bytes == null)
            {
                return false;
            }
            foreach (var _byte in bytes) {
                if (_byte == 0) {
                    return false;
                }
            }
            return true;
        }

        public static int GetModuleAddress(Process process, String dllName)
        {
            foreach (ProcessModule pm in process.Modules) {
                if (pm.ModuleName.Equals(dllName))
                    return (int)pm.BaseAddress;
            }
            return 0;
        }

        public static IntPtr GetPtrFromOffsets(IntPtr base_ptr, IntPtr[] offsets) {
            int valueOfBasePtr = MemoryData.mem.ReadInt(base_ptr.GetAddress());
            IntPtr base_ptr_value = ConvertStringToIntPtr(valueOfBasePtr.GetAddress());

            IntPtr last_ptr_Value = base_ptr_value;

            for (int i=0; i < offsets.Length-1; i++) {
                IntPtr current_offset = offsets[i];
                IntPtr offset = last_ptr_Value.Sum(current_offset);
                int valueOffset = MemoryData.mem.ReadInt(offset.GetAddress());
                last_ptr_Value = ConvertStringToIntPtr(valueOffset.GetAddress());
            }

            IntPtr lastOffset = offsets[offsets.Length - 1];
            IntPtr valuePtr = last_ptr_Value.Sum(lastOffset);

            // Detects if is valid the pointer value
            if (valuePtr == lastOffset) {
                valuePtr = IntPtr.Zero;
            }

            return valuePtr;
        }

    }
}
