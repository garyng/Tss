using System;
using System.Runtime.InteropServices;
using Tss.Core;

namespace Tss.Unmanaged
{
	public static class TssServiceUnmanaged
	{
		private static TssService _service;

		[DllExport]
		[return: MarshalAs(UnmanagedType.BStr)]
		public static string Init([MarshalAs(UnmanagedType.BStr)]
			string credentialsPath)
		{
			_service = new TssService(credentialsPath);         //_service = new TssService(credentialsPath);
			return "asdasd";
		}


		[DllExport]
		public static void MoveCurrentToGood()
		{
			// _service.MoveToGoodPlaylist().Wait();
		}
	}
}