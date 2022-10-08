#region Header
//   Vorspire    _,-'/-'/  Bootstrap.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2021  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#if !ServUO58

#region References
using System;
using System.Collections.Generic;
using System.Reflection;

using VitaNex;
#endregion

namespace Ultima
{
	public static class Bootstrap
	{
		private static readonly Dictionary<string, Type> _Modules = new Dictionary<string, Type>();

		public static Assembly UltimaSDK { get; private set; }

		public static bool Loaded { get; private set; }
		public static bool Warned { get; private set; }

		static Bootstrap()
		{
			Load();
		}

		private static void Load()
		{
			if (Loaded)
				return;

			try
			{
				UltimaSDK = Assembly.LoadFrom("Ultima.dll");

				Loaded = true;
				Warned = false;
			}
			catch (Exception e)
			{
				UltimaSDK = null;

				VitaNexCore.ToConsole("Could not load Ultima.dll");

				if (!Warned)
				{
					VitaNexCore.ToConsole(e);

					Warned = true;
				}
			}
		}

		private static Type GetModule(string name)
		{
			Load();

			if (!Loaded)
				return null;

			try
			{
				if (!_Modules.TryGetValue(name, out var type))
					_Modules[name] = type = UltimaSDK.GetType($"Ultima.{name}");

				return type;
			}
			catch (Exception e)
			{
				VitaNexCore.ToConsole($"Could not find Ultima.{name}:");
				VitaNexCore.ToConsole(e);

				return null;
			}
		}

		public static T Invoke<T>(string module, string method, params object[] args)
		{
			try
			{
				return GetModule(module).CallMethod<T>(method, args);
			}
			catch (Exception e)
			{
				VitaNexCore.ToConsole($"Could not invoke Ultima.{module}.{method}:");
				VitaNexCore.ToConsole(e);

				return default(T);
			}
		}

		public static object Invoke(string module, string method, params object[] args)
		{
			try
			{
				return GetModule(module).CallMethod(method, args);
			}
			catch (Exception e)
			{
				VitaNexCore.ToConsole($"Could not invoke Ultima.{module}.{method}:");
				VitaNexCore.ToConsole(e);

				return null;
			}
		}
	}
}

#endif
