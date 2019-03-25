using System;
using System.IO;
using System.Reflection;

namespace ComCtrl
{
	// Token: 0x02000002 RID: 2
	public class ldfs
	{
		// Token: 0x06000001 RID: 1 RVA: 0x000020D0 File Offset: 0x000002D0
		private byte[] LoadDll(string lpFileName)
		{
			Assembly NowAssembly = Assembly.GetEntryAssembly();
			Stream fs = null;
			try
			{
				fs = NowAssembly.GetManifestResourceStream(NowAssembly.GetName().Name + "." + lpFileName);
			}
			finally
			{
				if (fs == null && !File.Exists(lpFileName))
				{
					throw new Exception(" 找不到文件 :" + lpFileName);
				}
				if (fs == null && File.Exists(lpFileName))
				{
					FileStream Fs = new FileStream(lpFileName, FileMode.Open);
					fs = Fs;
				}
			}
			byte[] buffer = new byte[(int)fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			fs.Close();
			return buffer;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002188 File Offset: 0x00000388
		public void UnLoadDll()
		{
			ldfs.MyAssembly = null;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002194 File Offset: 0x00000394
		public object Invoke(string lpFileName, string Namespace, string ClassName, string lpProcName, object[] ObjArray_Parameter)
		{
			try
			{
				ldfs.MyAssembly = Assembly.Load(this.LoadDll(lpFileName));
				Type[] type = ldfs.MyAssembly.GetTypes();
				bool flag = false;
				Type[] array = type;
				int j = 0;
				while (j < array.Length)
				{
					Type t = array[j];
					if (t.Namespace == Namespace && t.Name == ClassName)
					{
						MethodInfo i = t.GetMethod(lpProcName);
						if (i != null)
						{
							object o = Activator.CreateInstance(t);
							return i.Invoke(o, ObjArray_Parameter);
						}
						throw new Exception(" 装载出错 !");
					}
					else
					{
						j++;
					}
				}
				if (!flag)
				{
					throw new Exception(string.Format(" 未找到命名空间为{0}的类{1} ", Namespace, ClassName));
				}
			}
			catch (NullReferenceException e)
			{
				throw new Exception(e.Message);
			}
			return 0;
		}

		// Token: 0x04000001 RID: 1
		private static Assembly MyAssembly;
	}
}
