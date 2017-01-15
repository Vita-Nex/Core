#region Header
//   Vorspire    _,-'/-'/  FilterOption.cs
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
#endregion

namespace Server
{
	public struct FilterOption
	{
		public string Category { get; private set; }
		public string Name { get; private set; }

		public string Property { get; private set; }
		public object Value { get; private set; }

		public bool IsDefault { get; private set; }

		public bool IsEmpty { get { return String.IsNullOrWhiteSpace(Name) || String.IsNullOrWhiteSpace(Property); } }

		public FilterOption(string category)
			: this(category, String.Empty, String.Empty, null, false)
		{ }

		public FilterOption(string category, string name, string property, object value, bool isDefault)
			: this()
		{
			Category = category;
			Name = name;
			Property = property;
			Value = value;
			IsDefault = isDefault;
		}

		public bool IsSelected(IFilter filter)
		{
			if (filter == null || String.IsNullOrWhiteSpace(Property))
			{
				return false;
			}

			/*try
			{*/
			var p = filter.GetType().GetProperty(Property);

			if (p != null && p.CanRead)
			{
				return Equals(p.GetValue(filter, null), Value);
			}
			/*}
			catch
			{ }*/

			return false;
		}

		public bool Select(IFilter filter)
		{
			if (filter == null || String.IsNullOrWhiteSpace(Property))
			{
				return false;
			}

			/*try
			{*/
			var p = filter.GetType().GetProperty(Property);

			if (p != null && p.CanWrite)
			{
				p.SetValue(filter, Value, null);
				return true;
			}
			/*}
			catch
			{ }*/

			return false;
		}
	}
}