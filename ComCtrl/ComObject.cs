using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ComCtrl
{
	// Token: 0x02000006 RID: 6
	[Guid("75473DCA-91C3-4802-A2DE-BE9B13CE6CFF")]
	public class ComObject
	{
        public ComObject(string iniFile,string LogPath)
        {
            this.iniFile = iniFile;
            this.LogPath = LogPath;
        }
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002B08 File Offset: 0x00000D08
		// (set) Token: 0x06000023 RID: 35 RVA: 0x00002B1F File Offset: 0x00000D1F
		public string ErrorMsg { get; set; }

        // Token: 0x06000024 RID: 36 RVA: 0x00002B28 File Offset: 0x00000D28
        /// <summary>
        /// 动态调用DLL
        /// </summary>
        /// <param name="subCtrlName">控件名称</param>
        /// <param name="functionName">方法名</param>
        /// <param name="withParametersFlag">是否传递参数，true传</param>
        /// <param name="ObjArray_ParameterStr">参数列表（按分割分开）</param>
        /// <param name="ParameterSplitStr">参数列表分隔符</param>
        /// <returns></returns>
        public object Call(string subCtrlName, string functionName, string withParametersFlag, string ObjArray_ParameterStr, string ParameterSplitStr)
		{
			this.ErrorMsg = null;
			object ret = null;
			string functionTrace = string.Format("[{0}_{1}]", DateTime.Now.ToString("yyyyMMddHHmmssfff"), functionName);
			string AppCurrentDirectory = Directory.GetCurrentDirectory();
			this.WriteLog(functionTrace, string.Format("获取当前工作目录CurrentDirectory={0}", AppCurrentDirectory));
			try
			{
				this.WriteLog(functionTrace, string.Format("{0}  subCtrlName={1},functionName={2},ObjArray_ParameterStr={3},ParameterSplitStr={4}", new object[]
				{
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
					subCtrlName,
					functionName,
					ObjArray_ParameterStr,
					ParameterSplitStr
				}));
				object[] ObjArray_Parameter = null;
				if (withParametersFlag == "true")
				{
					this.WriteLog(functionTrace, string.Format("{0}  【带参数:{1}】", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ObjArray_ParameterStr));
					if (string.IsNullOrEmpty(ObjArray_ParameterStr))
					{
						ObjArray_Parameter = new string[]
						{
							""
						};
					}
					else if (string.IsNullOrEmpty(ParameterSplitStr))
					{
						ObjArray_Parameter = new object[]
						{
							ObjArray_ParameterStr
						};
					}
					else
					{
						ObjArray_Parameter = Regex.Split(ObjArray_ParameterStr, ParameterSplitStr, RegexOptions.IgnoreCase);
					}
				}
				else
				{
					this.WriteLog(functionTrace, string.Format("{0}  【无参数。】", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
				}
				Type retDataType = typeof(string);
				string INIFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.iniFile);
				string CurrentDirectory = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "CurrentDirectory", null);
				string Dll = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "Dll", null);
				string Language = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "Language", null);
				string Namespace = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "Namespace", null);
				string ClassName = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "ClassName", null);
				string FunctionCount = Win32API.INIGetStringValue(INIFilePath, subCtrlName, "FunctionCount", null);
				string configError = null;
				string functionError = null;
				string[] ObjArray_ParameterType = null;
				object[] ObjArray_ParameterChanged = null;
				Type[] ObjArray_ParameterTypeChanged = null;
				int ObjArray_ParameterTypeIndex = 0;
				if (ObjArray_Parameter != null)
				{
					ObjArray_ParameterType = new string[ObjArray_Parameter.Length];
					ObjArray_ParameterChanged = new object[ObjArray_Parameter.Length];
					ObjArray_ParameterTypeChanged = new Type[ObjArray_Parameter.Length];
				}
				if (string.IsNullOrEmpty(CurrentDirectory))
				{
					configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "CurrentDirectory", CurrentDirectory);
				}
				else if (string.IsNullOrEmpty(Dll))
				{
					configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "Dll", Dll);
				}
				else if (string.IsNullOrEmpty(Language))
				{
					configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "Language", Language);
				}
				else
				{
					bool checkLanguage = false;
					if (Language.ToLower() == "c#.net" || Language.ToLower() == "vb.net")
					{
						checkLanguage = true;
					}
					if (checkLanguage && string.IsNullOrEmpty(Namespace))
					{
						configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "Namespace", Namespace);
					}
					else if (checkLanguage && string.IsNullOrEmpty(ClassName))
					{
						configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "ClassName", ClassName);
					}
					else if (string.IsNullOrEmpty(FunctionCount))
					{
						configError += string.Format("[{0}]小节【{1}={2}】未设置或设置错误,", subCtrlName, "FunctionCount", FunctionCount);
					}
					else
					{
						int functionCount = 0;
						int.TryParse(FunctionCount, out functionCount);
						if (functionCount <= 0)
						{
							configError += string.Format("[{0}]小节{1}={2}设置错误,应设置为大于0的整数,", subCtrlName, "FunctionCount", FunctionCount);
							throw new Exception(string.Format("ConfigError:{0}", configError.TrimEnd(new char[]
							{
								','
							})));
						}
						ArrayList functionConfigList = new ArrayList();
						for (int i = 1; i <= functionCount; i++)
						{
							string _FunctionName = string.Format("Function{0}", i);
							string _FunctionValue = Win32API.INIGetStringValue(INIFilePath, subCtrlName, _FunctionName, null);
							if (string.IsNullOrEmpty(_FunctionValue))
							{
								configError += string.Format("[{0}]小节{1}={2}未设置或设置错误,", subCtrlName, _FunctionName, _FunctionValue);
							}
							else
							{
								functionConfigList.Add(_FunctionValue);
							}
						}
						ArrayList functionList = new ArrayList();
						if (string.IsNullOrEmpty(configError))
						{
							foreach (object obj in functionConfigList)
							{
								string item = (string)obj;
								string returnValueType = null;
								string methodName = null;
								ArrayList paramTypelist = new ArrayList();
								ArrayList paramlist = new ArrayList();
								int partCount = 0;
								bool paramFlag = false;
								string tempParamType = null;
								string tempParam = null;
								string funtion = item.Trim();
								char[] chrs = funtion.ToCharArray();
								for (int i = 0; i < chrs.Length; i++)
								{
									if (partCount == 0)
									{
										if (chrs[i] != ' ')
										{
											returnValueType += chrs[i];
										}
										else
										{
											partCount = 1;
										}
									}
									else if (partCount == 1)
									{
										if (chrs[i] != ' ' && chrs[i] != '(')
										{
											methodName += chrs[i];
										}
										else
										{
											partCount = 2;
										}
									}
									else if (partCount == 2)
									{
										if (chrs[i] != '(')
										{
											if (!paramFlag)
											{
												if (chrs[i] != ' ')
												{
													tempParamType += chrs[i];
												}
												else
												{
													paramFlag = true;
												}
											}
											else if (chrs[i] != ' ')
											{
												tempParam += chrs[i];
											}
											if (chrs[i] == ',' || chrs[i] == ')')
											{
												paramFlag = false;
												if (tempParamType != null && tempParam != null)
												{
													paramTypelist.Add(tempParamType);
													paramlist.Add(tempParam);
													if (ObjArray_ParameterType != null && ObjArray_ParameterType.Length - 1 >= ObjArray_ParameterTypeIndex)
													{
														ObjArray_ParameterType[ObjArray_ParameterTypeIndex] = tempParamType;
													}
													ObjArray_ParameterTypeIndex++;
												}
												tempParamType = null;
												tempParam = null;
											}
										}
									}
								}
								functionList.Add(new object[]
								{
									returnValueType,
									methodName,
									paramTypelist,
									paramlist
								});
							}
							string selectReturnValueType = null;
							string selectMethod = null;
							ArrayList selectParamTypelist = new ArrayList();
							ArrayList selectParamlist = new ArrayList();
							foreach (object obj2 in functionList)
							{
								object[] item2 = (object[])obj2;
								if (functionName == item2[1].ToString())
								{
									selectReturnValueType = item2[0].ToString();
									selectMethod = item2[1].ToString();
									selectParamTypelist = (ArrayList)item2[2];
									selectParamlist = (ArrayList)item2[3];
									break;
								}
							}
							if (string.IsNullOrEmpty(selectMethod))
							{
								functionError += string.Format("配置文件中函数{0}未定义,", functionName);
							}
							else
							{
								string text = selectReturnValueType;
								switch (text)
								{
								case "string":
									retDataType = typeof(string);
									goto IL_8A9;
								case "bool":
									retDataType = typeof(bool);
									goto IL_8A9;
								case "int":
									retDataType = typeof(int);
									goto IL_8A9;
								case "float":
									retDataType = typeof(float);
									goto IL_8A9;
								case "double":
									retDataType = typeof(double);
									goto IL_8A9;
								case "decimal":
									retDataType = typeof(decimal);
									goto IL_8A9;
								}
								functionError += string.Format("函数{0}返回值类型{1}配置错误,", selectMethod, selectReturnValueType);
								IL_8A9:
								int paramCount = (ObjArray_Parameter == null) ? 0 : ObjArray_Parameter.Length;
								if (selectParamlist.Count != paramCount)
								{
									functionError += string.Format("函数{0}参数个数配置错误：定义个数={1},入参个数={2}", selectMethod, selectParamlist.Count, paramCount);
								}
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(configError))
				{
					throw new Exception(string.Format("ConfigError:{0}", configError.TrimEnd(new char[]
					{
						','
					})));
				}
				if (!string.IsNullOrEmpty(functionError))
				{
					throw new Exception(string.Format("FunctionError:{0}", functionError.TrimEnd(new char[]
					{
						','
					})));
				}
				if (ObjArray_Parameter != null)
				{
					int i = 0;
					while (i < ObjArray_Parameter.Length)
					{
						Type paramDataType = typeof(string);
						string text = ObjArray_ParameterType[i];
						switch (text)
						{
						case "string":
							ObjArray_ParameterChanged[i] = Convert.ToString(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(string);
							break;
						case "bool":
							ObjArray_ParameterChanged[i] = Convert.ToBoolean(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(bool);
							break;
						case "int":
							ObjArray_ParameterChanged[i] = Convert.ToInt32(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(int);
							break;
						case "float":
							ObjArray_ParameterChanged[i] = Convert.ToSingle(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(float);
							break;
						case "double":
							ObjArray_ParameterChanged[i] = Convert.ToDouble(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(double);
							break;
						case "decimal":
							ObjArray_ParameterChanged[i] = Convert.ToDecimal(ObjArray_Parameter[i]);
							ObjArray_ParameterTypeChanged[i] = typeof(decimal);
							break;
						}
						IL_B06:
						i++;
						continue;
						goto IL_B06;
					}
				}
                CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CurrentDirectory);
				this.WriteLog(functionTrace, string.Format("设置当前工作目录CurrentDirectory={0}",  CurrentDirectory));
				Directory.SetCurrentDirectory(CurrentDirectory);
				if (Language.ToLower() == "c++" || Language.ToLower() == "delphi" || Language.ToLower() == "pb")
				{
					this.WriteLog(functionTrace, string.Format("{0}  Language={1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), Language, "创建一个dld类对象"));
					dld myfun = new dld();
					myfun.LoadDll(Dll);
					myfun.LoadFun(functionName);
					dld.ModePass[] themode = new dld.ModePass[ObjArray_ParameterChanged.Length];
					for (int i = 0; i < themode.Length; i++)
					{
						themode[i] = dld.ModePass.ByValue;
					}
					this.WriteLog(functionTrace, string.Format("{0}  Invoke:ObjArray_Parameter={1}, TypeArray_ParameterType={2},ModePassArray_Parameter={3}, Type_Return={4}", new object[]
					{
						DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
						ObjArray_ParameterChanged,
						ObjArray_ParameterTypeChanged,
						themode,
						retDataType
					}));
					ret = myfun.Invoke(ObjArray_ParameterChanged, ObjArray_ParameterTypeChanged, themode, retDataType);
					myfun.UnLoadDll();
					this.WriteLog(functionTrace, string.Format("{0}  {1}返回ret={2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), functionName, ret));
				}
				else
				{
					if (!(Language.ToLower() == "c#.net") && !(Language.ToLower() == "java") && !(Language.ToLower() == "vb.net"))
					{
						throw new Exception(string.Format("ConfigError:暂不支持{0}语言！", Language));
					}
					this.WriteLog(functionTrace, string.Format("{0}  Language={1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), Language, "用Assembly类来动态调用托管DLL"));
					this.WriteLog(functionTrace, string.Format("{0}  Invoke:Dll={1}, Namespace={2}, ClassName={3}, functionName={4}, ObjArray_Parameter={5}", new object[]
					{
						DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
						Dll,
						Namespace,
						ClassName,
						functionName,
						ObjArray_ParameterChanged
					}));
					ret = this.Invoke(Dll, Namespace, ClassName, functionName, ObjArray_ParameterChanged);
				}
			}
			catch (Exception ex)
			{
				this.ErrorMsg = ex.Message;
				this.WriteLog(functionTrace, string.Format("{0}  {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ex.Message, ex.StackTrace));
			}
			finally
			{
				this.WriteLog(functionTrace, string.Format("恢复当前工作目录CurrentDirectory={0}", AppCurrentDirectory));
				this.WriteLog(functionTrace, string.Format("ret={0}\r\n", ret));
			}
			return ret;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003984 File Offset: 0x00001B84
		public void WriteLog(string functionTrace, string log)
		{
			DateTime date = DateTime.Now;
			string dateMonth = date.ToString("yyyy-MM");
			string dateDay = date.ToString("yyyy-MM-dd");
			this.WriteLogFile(functionTrace + log, this.LogPath, string.Format("{0}", dateMonth), dateDay + ".txt");
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000039DC File Offset: 0x00001BDC
		public void WriteLogFile(string input, string folder, string yearMonth, string file)
		{
			string fname = string.Format("{0}\\{1}\\{2}", folder, yearMonth, file);
			string sPath = string.Format("{0}\\{1}", folder, yearMonth);
			if (!Directory.Exists(sPath))
			{
				Directory.CreateDirectory(sPath);
			}
			StreamWriter sw = File.AppendText(fname);
			sw.WriteLine(input);
			sw.Flush();
			sw.Close();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003A38 File Offset: 0x00001C38
		private object Invoke(string lpFileName, string Namespace, string ClassName, string lpProcName, object[] ObjArray_Parameter)
		{
			try
			{
				Assembly MyAssembly = Assembly.LoadFrom(lpFileName);
				Type[] type = MyAssembly.GetTypes();
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
			}
			catch (NullReferenceException e)
			{
				throw new Exception(e.Message);
			}
			return 0;
		}

		// Token: 0x04000007 RID: 7
		private string iniFile = "C:\\Program Files\\浪潮集团金融事业部\\DevCtrler\\module\\ComCtrl\\ComCtrl.ini";

		// Token: 0x04000008 RID: 8
		private string LogPath = "C:\\Program Files\\浪潮集团金融事业部\\devctrler\\module\\ComCtrl\\log";
        
	}
}
