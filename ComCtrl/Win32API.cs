using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ComCtrl
{
	// Token: 0x02000005 RID: 5
	public class Win32API
	{
		// Token: 0x06000010 RID: 16
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

		// Token: 0x06000011 RID: 17
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

		// Token: 0x06000012 RID: 18
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [In] [Out] char[] lpReturnedString, uint nSize, string lpFileName);

		// Token: 0x06000013 RID: 19
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		// Token: 0x06000014 RID: 20
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, uint nSize, string lpFileName);

		// Token: 0x06000015 RID: 21
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

		// Token: 0x06000016 RID: 22
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

		// Token: 0x06000017 RID: 23 RVA: 0x000026B0 File Offset: 0x000008B0
		public static string[] INIGetAllSectionNames(string iniFile)
		{
			uint MAX_BUFFER = 32767u;
			string[] sections = new string[0];
			IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)(MAX_BUFFER * 2u));
			uint bytesReturned = Win32API.GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniFile);
			if (bytesReturned != 0u)
			{
				string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();
				string text = local;
				char[] separator = new char[1];
				sections = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			}
			Marshal.FreeCoTaskMem(pReturnedString);
			return sections;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002718 File Offset: 0x00000918
		public static string[] INIGetAllItems(string iniFile, string section)
		{
			uint MAX_BUFFER = 32767u;
			string[] items = new string[0];
			IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)(MAX_BUFFER * 2u));
			uint bytesReturned = Win32API.GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);
			if (bytesReturned != MAX_BUFFER - 2u || bytesReturned == 0u)
			{
				string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
				string text = returnedString;
				char[] separator = new char[1];
				items = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			}
			Marshal.FreeCoTaskMem(pReturnedString);
			return items;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002788 File Offset: 0x00000988
		public static string[] INIGetAllItemKeys(string iniFile, string section)
		{
			string[] value = new string[0];
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			char[] chars = new char[10240];
			uint bytesReturned = Win32API.GetPrivateProfileString(section, null, null, chars, 10240u, iniFile);
			if (bytesReturned != 0u)
			{
				string text = new string(chars);
				char[] separator = new char[1];
				value = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			}
			return value;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002804 File Offset: 0x00000A04
		public static string INIGetStringValue(string iniFile, string section, string key, string defaultValue)
		{
			string value = defaultValue;
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("必须指定键名称(key)", "key");
			}
			StringBuilder sb = new StringBuilder(10240);
			uint bytesReturned = Win32API.GetPrivateProfileString(section, key, defaultValue, sb, 10240u, iniFile);
			if (bytesReturned != 0u)
			{
				value = sb.ToString();
			}
			return value;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000288C File Offset: 0x00000A8C
		public static bool INIWriteItems(string iniFile, string section, string items)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			if (string.IsNullOrEmpty(items))
			{
				throw new ArgumentException("必须指定键值对", "items");
			}
			return Win32API.WritePrivateProfileSection(section, items, iniFile);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000028E4 File Offset: 0x00000AE4
		public static bool INIWriteValue(string iniFile, string section, string key, string value)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("必须指定键名称", "key");
			}
			if (value == null)
			{
				throw new ArgumentException("值不能为null", "value");
			}
			return Win32API.WritePrivateProfileString(section, key, value, iniFile);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002958 File Offset: 0x00000B58
		public static bool INIDeleteKey(string iniFile, string section, string key)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("必须指定键名称", "key");
			}
			return Win32API.WritePrivateProfileString(section, key, null, iniFile);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000029B0 File Offset: 0x00000BB0
		public static bool INIDeleteSection(string iniFile, string section)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			return Win32API.WritePrivateProfileString(section, null, null, iniFile);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000029EC File Offset: 0x00000BEC
		public static bool INIEmptySection(string iniFile, string section)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentException("必须指定节点名称", "section");
			}
			return Win32API.WritePrivateProfileSection(section, string.Empty, iniFile);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002A28 File Offset: 0x00000C28
		private void Test()
		{
			string file = "e:\\3.ini";
			Win32API.INIWriteValue(file, "Desktop", "Color", "Red");
			Win32API.INIWriteValue(file, "Desktop", "Width", "3270");
			Win32API.INIWriteValue(file, "Toolbar", "Items", "Save,Delete,Open");
			Win32API.INIWriteValue(file, "Toolbar", "Dock", "True");
			Win32API.INIWriteItems(file, "Menu", "File=文件\0View=视图\0Edit=编辑");
			string[] sections = Win32API.INIGetAllSectionNames(file);
			string[] items = Win32API.INIGetAllItems(file, "Menu");
			string[] keys = Win32API.INIGetAllItemKeys(file, "Menu");
			string value = Win32API.INIGetStringValue(file, "Desktop", "color", null);
			Win32API.INIDeleteKey(file, "desktop", "color");
			Win32API.INIDeleteSection(file, "desktop");
			Win32API.INIEmptySection(file, "toolbar");
		}
	}
}
