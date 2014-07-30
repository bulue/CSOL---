using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace 档案汇总
{
    class Bit
    {
        //转换C#代码：

        //结构体转换成字节流

        public static byte[] StructToBytes<T>(T obj)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, bufferPtr, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(bufferPtr, bytes, 0, size);

                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in StructToBytes ! " + ex.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }


        //字节流转换成结构体

        public static T BytesToStruct<T>(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null) return default(T);
            if (bytes.Length <= 0) return default(T);
            int objLength = Marshal.SizeOf(typeof(T));
            IntPtr bufferPtr = Marshal.AllocHGlobal(objLength);
            try//struct_bytes转换
            {
                Marshal.Copy(bytes, startIndex, bufferPtr, objLength);
                return (T)Marshal.PtrToStructure(bufferPtr, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception("Error in BytesToStruct ! " + ex.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]  //变量在内存中的对齐方式 
    public struct MsgHeader
    {
        public ushort wMsgLen;
    };

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct stMsg
    {
        //字符串，SizeConst为字符串的最大长度
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string Cmd;
    }
}
