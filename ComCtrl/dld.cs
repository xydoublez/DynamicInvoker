using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ComCtrl
{
	// Token: 0x02000003 RID: 3
	public class dld
	{
		// Token: 0x06000005 RID: 5
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string lpFileName);

		// Token: 0x06000006 RID: 6
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		// Token: 0x06000007 RID: 7
		[DllImport("kernel32", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		// Token: 0x06000008 RID: 8 RVA: 0x000022A0 File Offset: 0x000004A0
		public void LoadDll(string lpFileName)
		{
			this.hModule = dld.LoadLibrary(lpFileName);
			if (this.hModule == IntPtr.Zero)
			{
				throw new Exception(" 没有找到 :" + lpFileName + ".");
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000022E8 File Offset: 0x000004E8
		public void LoadDll(IntPtr HMODULE)
		{
			if (HMODULE == IntPtr.Zero)
			{
				throw new Exception(" 所传入的函数库模块的句柄 HMODULE 为空 .");
			}
			this.hModule = HMODULE;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000231C File Offset: 0x0000051C
		public void LoadFun(string lpProcName)
		{
			if (this.hModule == IntPtr.Zero)
			{
				throw new Exception(" 函数库模块的句柄为空 , 请确保已进行 LoadDll 操作 !");
			}
			this.farProc = dld.GetProcAddress(this.hModule, lpProcName);
			if (this.farProc == IntPtr.Zero)
			{
				throw new Exception(" 没有找到 :" + lpProcName + " 这个函数的入口点 ");
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000238C File Offset: 0x0000058C
		public void LoadFun(string lpFileName, string lpProcName)
		{
			this.hModule = dld.LoadLibrary(lpFileName);
			if (this.hModule == IntPtr.Zero)
			{
				throw new Exception(" 没有找到 :" + lpFileName + ".");
			}
			this.farProc = dld.GetProcAddress(this.hModule, lpProcName);
			if (this.farProc == IntPtr.Zero)
			{
				throw new Exception(" 没有找到 :" + lpProcName + " 这个函数的入口点 ");
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002414 File Offset: 0x00000614
		public void UnLoadDll()
		{
			try
			{
				dld.FreeLibrary(this.hModule);
				this.hModule = IntPtr.Zero;
				this.farProc = IntPtr.Zero;
			}
			catch (Exception ex)
			{
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002460 File Offset: 0x00000660
		public object Invoke(object[] ObjArray_Parameter, Type[] TypeArray_ParameterType, dld.ModePass[] ModePassArray_Parameter, Type Type_Return)
		{
			if (this.hModule == IntPtr.Zero)
			{
				throw new Exception(" 函数库模块的句柄为空 , 请确保已进行 LoadDll 操作 !");
			}
			if (this.farProc == IntPtr.Zero)
			{
				throw new Exception(" 函数指针为空 , 请确保已进行 LoadFun 操作 !");
			}
			if (ObjArray_Parameter.Length != ModePassArray_Parameter.Length)
			{
				throw new Exception(" 参数个数及其传递方式的个数不匹配 .");
			}
			AssemblyName MyAssemblyName = new AssemblyName();
			MyAssemblyName.Name = "InvokeFun";
			AssemblyBuilder MyAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(MyAssemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder MyModuleBuilder = MyAssemblyBuilder.DefineDynamicModule("InvokeDll");
			MethodBuilder MyMethodBuilder = MyModuleBuilder.DefineGlobalMethod("MyFun", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, Type_Return, TypeArray_ParameterType);
			ILGenerator IL = MyMethodBuilder.GetILGenerator();
			for (int i = 0; i < ObjArray_Parameter.Length; i++)
			{
				switch (ModePassArray_Parameter[i])
				{
				case dld.ModePass.ByValue:
					IL.Emit(OpCodes.Ldarg, i);
					break;
				case dld.ModePass.ByRef:
					IL.Emit(OpCodes.Ldarga, i);
					break;
				default:
					throw new Exception(" 第 " + (i + 1).ToString() + " 个参数没有给定正确的传递方式 .");
				}
			}
			if (IntPtr.Size == 4)
			{
				IL.Emit(OpCodes.Ldc_I4, this.farProc.ToInt32());
			}
			else
			{
				if (IntPtr.Size != 8)
				{
					throw new PlatformNotSupportedException();
				}
				IL.Emit(OpCodes.Ldc_I8, this.farProc.ToInt64());
			}
			IL.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, Type_Return, TypeArray_ParameterType);
			IL.Emit(OpCodes.Ret);
			MyModuleBuilder.CreateGlobalFunctions();
			MethodInfo MyMethodInfo = MyModuleBuilder.GetMethod("MyFun");
			return MyMethodInfo.Invoke(null, ObjArray_Parameter);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000262C File Offset: 0x0000082C
		public object Invoke(IntPtr IntPtr_Function, object[] ObjArray_Parameter, Type[] TypeArray_ParameterType, dld.ModePass[] ModePassArray_Parameter, Type Type_Return)
		{
			if (this.hModule == IntPtr.Zero)
			{
				throw new Exception(" 函数库模块的句柄为空 , 请确保已进行 LoadDll 操作 !");
			}
			if (IntPtr_Function == IntPtr.Zero)
			{
				throw new Exception(" 函数指针 IntPtr_Function 为空 !");
			}
			this.farProc = IntPtr_Function;
			return this.Invoke(ObjArray_Parameter, TypeArray_ParameterType, ModePassArray_Parameter, Type_Return);
		}

		// Token: 0x04000002 RID: 2
		private IntPtr farProc = IntPtr.Zero;

		// Token: 0x04000003 RID: 3
		private IntPtr hModule = IntPtr.Zero;

		// Token: 0x02000004 RID: 4
		public enum ModePass
		{
			// Token: 0x04000005 RID: 5
			ByValue = 1,
			// Token: 0x04000006 RID: 6
			ByRef
		}
	}
}
