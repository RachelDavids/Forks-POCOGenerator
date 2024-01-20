using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace POCOGeneratorUI.ConnectionDialog
{
	public enum RegistryHive
	{
		Wow64,
		Wow6432
	}

	/// <summary>
	/// Class RegistryValueDataReader.
	/// </summary>
	/// <remarks>
	/// See <see href="https://stackoverflow.com/a/6852901/4694175">HERE</see> for details
	/// </remarks>
	public static class RegistryValueDataReader
	{
		private static readonly int KEY_WOW64_32KEY = 0x200;
		private static readonly int KEY_WOW64_64KEY = 0x100;

		private static readonly UIntPtr HKEY_LOCAL_MACHINE = 0x80000002;

		private static readonly int KEY_QUERY_VALUE = 0x1;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
		private static extern int RegOpenKeyEx(UIntPtr hKey,
											   string subKey,
											   uint options,
											   int sam,
											   out IntPtr phkResult);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern int RegQueryValueEx(IntPtr hKey,
												  string lpValueName,
												  int lpReserved,
												  out uint lpType,
												  IntPtr lpData,
												  ref uint lpcbData);

		private static int GetRegistryHiveKey(RegistryHive registryHive) => registryHive == RegistryHive.Wow64 ? KEY_WOW64_64KEY : KEY_WOW64_32KEY;

		private static UIntPtr GetRegistryKeyUIntPtr(RegistryKey registry) => registry == Registry.LocalMachine ? HKEY_LOCAL_MACHINE : UIntPtr.Zero;

		public static string[] ReadRegistryValueData(RegistryHive registryHive, RegistryKey registryKey, string subKey, string valueName)
		{
			string[] instanceNames = new string[0];

			int key = GetRegistryHiveKey(registryHive);
			UIntPtr registryKeyUIntPtr = GetRegistryKeyUIntPtr(registryKey);

			int res = RegOpenKeyEx(registryKeyUIntPtr, subKey, 0, KEY_QUERY_VALUE | key, out nint hResult);

			if (res == 0)
			{
				uint dataLen = 0;

				RegQueryValueEx(hResult, valueName, 0, out uint type, IntPtr.Zero, ref dataLen);

				byte[] databuff = new byte[dataLen];
				byte[] temp = new byte[dataLen];

				List<string> values = [];

				GCHandle handle = GCHandle.Alloc(databuff, GCHandleType.Pinned);
				try
				{
					RegQueryValueEx(hResult, valueName, 0, out type, handle.AddrOfPinnedObject(), ref dataLen);
				}
				finally
				{
					handle.Free();
				}

				int i = 0;
				int j = 0;

				while (i < databuff.Length)
				{
					if (databuff[i] == '\0')
					{
						j = 0;
						string str = Encoding.Default.GetString(temp).Trim('\0');

						if (!string.IsNullOrEmpty(str))
						{
							values.Add(str);
						}

						temp = new byte[dataLen];
					}
					else
					{
						temp[j++] = databuff[i];
					}

					++i;
				}

				instanceNames = new string[values.Count];
				values.CopyTo(instanceNames);
			}

			return instanceNames;
		}
	}
}
