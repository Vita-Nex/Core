#region Header
//   Vorspire    _,-'/-'/  Tree.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Server;
using Server.Gumps;

using VitaNex.Collections;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public abstract class TreeGump : SuperGumpList<TreeGumpNode>
	{
		public static string DefaultTitle = "Tree View";

		public string Title { get; set; }
		public Color TitleColor { get; set; }

		public Color HtmlColor { get; set; }

		public TreeGumpNode SelectedNode { get; set; }

		public int Width { get; set; }
		public int Height { get; set; }

		public virtual Dictionary<TreeGumpNode, Action<Rectangle, int, TreeGumpNode>> Nodes { get; private set; }

		public TreeGump(
			Mobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<TreeGumpNode> nodes = null,
			TreeGumpNode selected = null,
			string title = null)
			: base(user, parent, x, y, nodes)
		{
			Nodes = new Dictionary<TreeGumpNode, Action<Rectangle, int, TreeGumpNode>>();

			Title = title ?? DefaultTitle;
			TitleColor = DefaultHtmlColor;

			HtmlColor = DefaultHtmlColor;

			HighlightHue = 1258;
			TextHue = 1153;

			Width = 600;
			Height = 400;

			SelectedNode = selected ?? String.Empty;

			ForceRecompile = true;
			CanMove = true;
			Sorted = true;
		}

		protected override void Compile()
		{
			Width = Math.Max(600, Width);
			Height = Math.Max(400, Height);

			EntriesPerPage = (int)Math.Floor((Height - 88) / 22.0);

			if (Nodes == null)
			{
				Nodes = new Dictionary<TreeGumpNode, Action<Rectangle, int, TreeGumpNode>>();
			}

			CompileNodes(Nodes);

			base.Compile();
		}

		protected virtual void CompileNodes(Dictionary<TreeGumpNode, Action<Rectangle, int, TreeGumpNode>> list)
		{ }

		protected override void CompileList(List<TreeGumpNode> list)
		{
			foreach (var n in Nodes.Keys)
			{
				list.AddOrReplace(n);
			}

			var nodes = ListPool<TreeGumpNode>.AcquireObject();
			var selected = ListPool<TreeGumpNode>.AcquireObject();
			var parents = ListPool<TreeGumpNode>.AcquireObject();

			nodes.Capacity = list.Count;

			foreach (var n in list)
			{
				foreach (var p in n.GetParents())
				{
					nodes.AddOrReplace(p);
				}

				nodes.AddOrReplace(n);
			}

			if (SelectedNode.HasParent)
			{
				nodes.AddOrReplace(SelectedNode.Parent);
			}

			selected.AddRange(SelectedNode.GetParents());

			nodes.RemoveAll(
				c =>
				{
					parents.AddRange(c.GetParents());

					var remove = false;

					if (parents.Count > 0)
					{
						if (parents.Count <= selected.Count && c != SelectedNode && !parents.Contains(SelectedNode) &&
							!selected.Any(p => p == c || c.Parent == p))
						{
							remove = true;
						}
						else if (parents.Count > selected.Count && c.Parent != SelectedNode)
						{
							remove = true;
						}
					}

					parents.Clear();

					return remove;
				});

			list.Clear();
			list.AddRange(nodes);

			ObjectPool.Free(ref nodes);
			ObjectPool.Free(ref selected);
			ObjectPool.Free(ref parents);

			base.CompileList(list);
		}

		public override int SortCompare(TreeGumpNode l, TreeGumpNode r)
		{
			var res = 0;

			if (l.CompareNull(r, ref res))
			{
				return res;
			}

			return Insensitive.Compare(l.FullName, r.FullName);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			var sup = SupportsUltimaStore;
			var ec = IsEnhancedClient;
			var bgID = ec ? 83 : sup ? 40000 : 9270;

			layout.Add(
				"body/bg",
				() =>
				{
					AddBackground(0, 43, Width, Height, 9260);
					AddImage(15, 18, 1419);
					AddBackground(15, 58, 234, 50, bgID);
					AddImage(92, 0, 1417);
				});

			layout.Add("body/mainbutton", () => AddButton(101, 9, 5545, 5546, MainButtonHandler));

			layout.Add("panel/left", () => AddBackground(45, 115, 204, Height - 88, bgID));

			layout.Add(
				"panel/left/overlay",
				() =>
				{
					if (!ec)
					{
						AddImageTiled(55, 125, 184, Height - 108, 2624);
					}
				});

			CompileTreeLayout(layout);

			layout.Add("panel/right", () => AddBackground(255, 58, Width - 270, Height - 30, bgID));

			layout.Add(
				"panel/right/overlay",
				() =>
				{
					if (!ec)
					{
						AddImageTiled(265, 68, Width - 290, Height - 50, 2624);
					}
				});

			layout.Add(
				"title",
				() => AddHtml(25, 78, 215, 40, Title.WrapUOHtmlCenter().WrapUOHtmlColor(TitleColor, false), false, false));

			layout.Add("dragon", () => AddImage(Width - 33, 0, 10441, 0));

			if (SelectedNode.IsEmpty)
			{
				CompileEmptyNodeLayout(layout, 265, 70, Width - 290, Height - 55, List.IndexOf(SelectedNode), SelectedNode);
			}
			else
			{
				CompileNodeLayout(layout, 265, 70, Width - 290, Height - 55, List.IndexOf(SelectedNode), SelectedNode);
			}
		}

		protected virtual void CompileTreeLayout(SuperGumpLayout layout)
		{
			var sup = SupportsUltimaStore;
			var ec = IsEnhancedClient;
			var bgID = ec ? 83 : sup ? 40000 : 9270;
			var bgCol = Color.Black;

			layout.Add(
				"tree/scrollbar",
				() =>
				{
					AddBackground(15, 115, 25, Height - 88, bgID);

					AddScrollbarV(
						15,
						115,
						PageCount,
						Page,
						PreviousPage,
						NextPage,
						new Rectangle(6, 40, 13, Height - 166),
						new Rectangle(6, 5, 13, 28),
						new Rectangle(6, Height - 120, 13, 28),
						// track, handle
						Tuple.Create(10740, 10742),
						// normal, pressed, inactive
						Tuple.Create(10701, 10702, 10700),
						// normal, pressed, inactive
						Tuple.Create(10721, 10722, 10720));
				});

			var cIndex = 0;

			foreach (var c in EnumerateListRange())
			{
				var node = c;
				var index = cIndex++;
				var offset = Math.Min(150, node.Depth * 10);

				layout.Add(
					"tree/button/" + index,
					() =>
					{
						AddHtmlButton(
							65 + offset,
							125 + (21 * index),
							165 - offset,
							20,
							btn => SelectNode(node),
							GetNodeName(node),
							GetNodeColor(node),
							bgCol);
					});
			}
		}

		protected virtual void CompileEmptyNodeLayout(
			SuperGumpLayout layout,
			int x,
			int y,
			int w,
			int h,
			int index,
			TreeGumpNode node)
		{ }

		protected virtual void CompileNodeLayout(
			SuperGumpLayout layout,
			int x,
			int y,
			int w,
			int h,
			int index,
			TreeGumpNode node)
		{
			if (Nodes == null || Nodes.Count <= 0)
			{
				return;
			}

			Action<Rectangle, Int32, TreeGumpNode> nodeLayout;

			if (Nodes.TryGetValue(node, out nodeLayout) && nodeLayout != null)
			{
				var o = new Rectangle(x, y, w, h);

				layout.Add("node/page/" + index, () => nodeLayout(o, index, node));
			}
		}

		public override void InvalidatePageCount()
		{
			PageCount = 1 + Math.Max(0, List.Count - EntriesPerPage);
			Page = Math.Max(0, Math.Min(PageCount - 1, Page));
		}

		public override Dictionary<int, TreeGumpNode> GetListRange()
		{
			return GetListRange(Page, EntriesPerPage);
		}

		public virtual int GetNodeHue(TreeGumpNode node)
		{
			return node != null && SelectedNode != node && !SelectedNode.IsChildOf(node) ? TextHue : HighlightHue;
		}

		public virtual Color GetNodeColor(TreeGumpNode node)
		{
			return node != null && SelectedNode != node && !SelectedNode.IsChildOf(node) ? Color.White : Color.Gold;
		}

		public virtual string GetNodeName(TreeGumpNode node)
		{
			return node == null || String.IsNullOrWhiteSpace(node.Name) ? "..." : node.Name;
		}

		public void SelectNode(TreeGumpNode node)
		{
			var old = SelectedNode;

			if (SelectedNode != node)
			{
				SelectedNode = node;
			}
			else if (SelectedNode.HasParent)
			{
				SelectedNode = SelectedNode.Parent;
			}
			else
			{
				SelectedNode = TreeGumpNode.Empty;
			}

			OnSelected(old, SelectedNode);
		}

		protected virtual void OnSelected(TreeGumpNode oldNode, TreeGumpNode newNode)
		{
			Refresh(true);
		}

		protected virtual void MainButtonHandler(GumpButton b)
		{
			Refresh(true);
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			VitaNexCore.TryCatch(Nodes.Clear);
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();

			SelectedNode = null;

			Nodes = null;
		}
	}
}