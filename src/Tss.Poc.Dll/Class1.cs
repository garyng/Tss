using System;
using System.Linq;
using System.Runtime.InteropServices;
using net.r_eg.Conari.Types;

namespace Tss.Poc.Dll
{
	// MarshalAs: https://sites.google.com/site/robertgiesecke/Home/uploads/unmanagedexports

	[StructLayout(LayoutKind.Sequential)]
	public struct Data
	{
		public int Index;
		//public string Name;
	}

	public static class TssTest
	{
		[DllExport]
		public static int ReturnInt()
		{
			return 1;
		}

		[DllExport]
		public static int ReceiveInt(int num)
		{
			return num * 100;
		}


		[DllExport]
		[return: MarshalAs(UnmanagedType.BStr)]
		public static string ReturnStr()
		{
			return "asdasd";
		}

		[DllExport]
		[return: MarshalAs(UnmanagedType.BStr)]
		public static string ReceiveStr([MarshalAs(UnmanagedType.BStr)]
			string msg)
		{
			return $"echo: {msg}";
		}

		[DllExport]
		public static void ReturnDataByRef(out int length)
		{
			length = 100;
		}

		[DllExport]
		public static void ReturnData(out int length, ref int[] datas)
		{
			length = 3;
			//datas = new[]
			//{
			//	//new Data {Index = 0, Name = "Name 1"},
			//	//new Data {Index = 1, Name = "New longer name ✅"}
			//	new Data {Index = 100},
			//	new Data {Index = 200},
			//	new Data {Index = 300},
			//};
			datas = new[]
			{
				10, 20, 30
			};
			//}.Select(d =>
			//{
			//	var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(d));
			//	Marshal.StructureToPtr(d, ptr, false);
			//	return ptr;
			//}).ToArray();
		}


		private static string _msg = "";

		[DllExport]
		public static void SetString([MarshalAs(UnmanagedType.BStr)]
			string msg)
		{
			_msg = msg;
		}

		[DllExport]
		[return: MarshalAs(UnmanagedType.BStr)]
		public static string GetString()
		{
			return _msg;
		}
	}
}