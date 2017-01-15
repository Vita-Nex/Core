#region Header
//   Vorspire    _,-'/-'/  SuperGumpLayout.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace VitaNex.SuperGumps
{
	public class SuperGumpLayout : Dictionary<string, Action<string>>
	{
		private readonly Dictionary<string, Action<string>> _Buffer = new Dictionary<string, Action<string>>();

		public SuperGumpLayout()
			: base(0x20)
		{ }

		public SuperGumpLayout(IDictionary<string, Action<string>> entries)
			: base(entries)
		{ }

		public SuperGumpLayout(IEnumerable<KeyValuePair<string, Action<string>>> entries)
		{
			foreach (var kv in entries.Where(kv => !String.IsNullOrWhiteSpace(kv.Key) && kv.Value != null))
			{
				this[kv.Key] = kv.Value;
			}
		}

		public void Combine(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || value == null)
			{
				return;
			}

			if (ContainsKey(xpath))
			{
				this[xpath] += x => value();
			}
			else
			{
				this[xpath] = x => value();
			}
		}

		public void Combine(string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || value == null)
			{
				return;
			}

			if (ContainsKey(xpath))
			{
				this[xpath] += value;
			}
			else
			{
				this[xpath] = value;
			}
		}

		public void Replace(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = x => value();
		}

		public void Replace(string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = value;
		}

		[Obsolete("Use Replace instead.")]
		public void AddReplace(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = x => value();
		}

		[Obsolete("Use Replace instead.")]
		public void AddReplace(string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = value;
		}

		public void Add(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				return;
			}

			Add(xpath, x => value());
		}

		public new void Add(string xpath, Action<string> value)
		{
			if (!String.IsNullOrWhiteSpace(xpath) && value != null)
			{
				base.Add(xpath, value);
			}
		}

		public void AddBefore(string search, string xpath, Action value)
		{
			if (!String.IsNullOrWhiteSpace(xpath) && value != null)
			{
				AddBefore(search, xpath, x => value());
			}
		}

		public void AddBefore(string search, string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || value == null)
			{
				return;
			}

			var index = Keys.IndexOf(search);

			if (index != -1)
			{
				_Buffer.Clear();

				this.For(
					(i, k, v) =>
					{
						if (i == index)
						{
							_Buffer[xpath] = value;
						}

						_Buffer[k] = v;
					});

				Clear();

				foreach (var kv in _Buffer)
				{
					this[kv.Key] = kv.Value;
				}

				_Buffer.Clear();
			}
			else
			{
				this[xpath] = value;
			}
		}

		public void AddAfter(string search, string xpath, Action value)
		{
			if (!String.IsNullOrWhiteSpace(xpath) && value != null)
			{
				AddAfter(search, xpath, x => value());
			}
		}

		public void AddAfter(string search, string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || value == null)
			{
				return;
			}

			var index = Keys.IndexOf(search);

			if (index != -1)
			{
				_Buffer.Clear();

				this.For(
					(i, k, v) =>
					{
						if (i == index + 1)
						{
							_Buffer[xpath] = value;
						}

						_Buffer[k] = v;
					});

				Clear();

				foreach (var kv in _Buffer)
				{
					this[kv.Key] = kv.Value;
				}

				_Buffer.Clear();
			}
			else
			{
				this[xpath] = value;
			}
		}

		public virtual void ApplyTo(SuperGump gump)
		{
			foreach (var kvp in this.Where(kvp => !String.IsNullOrEmpty(kvp.Key) && kvp.Value != null))
			{
				kvp.Value(kvp.Key);
			}
		}
	}
}