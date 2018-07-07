#region Header
//   Vorspire    _,-'/-'/  ProfileUI.cs
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
using System.Text;

using Server;
using Server.Accounting;
using Server.Gumps;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationProfileUI : TreeGump
	{
		public static void DisplayTo(Mobile user, DonationProfile profile, DonationTransaction trans)
		{
			DisplayTo(user, profile, false, trans);
		}

		public static void DisplayTo(Mobile user, DonationProfile profile, bool refreshOnly, DonationTransaction trans)
		{
			var node = trans.IsPending
				? ("Transactions|Pending|" + trans.ID)
				: trans.IsProcessed //
					? ("Transactions|Claim|" + trans.ID)
					: !trans.Hidden //
						? ("History|" + trans.FullPath)
						: "History";

			DisplayTo(user, profile, refreshOnly, node);
		}

		public static void DisplayTo(Mobile user)
		{
			DisplayTo(user, null);
		}

		public static void DisplayTo(Mobile user, DonationProfile profile)
		{
			DisplayTo(user, profile, String.Empty);
		}

		public static void DisplayTo(Mobile user, DonationProfile profile, string node)
		{
			DisplayTo(user, profile, false, node);
		}

		public static void DisplayTo(Mobile user, DonationProfile profile, bool refreshOnly)
		{
			DisplayTo(user, profile, refreshOnly, String.Empty);
		}

		public static void DisplayTo(Mobile user, DonationProfile profile, bool refreshOnly, string node)
		{
			var info = EnumerateInstances<DonationProfileUI>(user).FirstOrDefault(g => g != null && !g.IsDisposed && g.IsOpen);

			if (info == null)
			{
				if (refreshOnly)
				{
					return;
				}

				info = new DonationProfileUI(user, profile);
			}
			else if (profile != null)
			{
				info.Profile = profile;
			}

			if (!String.IsNullOrWhiteSpace(node))
			{
				info.SelectedNode = node;
			}

			info.Refresh(true);
		}

		private bool _Admin;

		private int _DonationTier;
		private int _DonationCount;
		private double _DonationValue;
		private long _DonationCredit;

		private int[,] _Indicies;
		private List<DonationTransaction> _Transactions;

		public DonationProfile Profile { get; set; }

		public DonationProfileUI(Mobile user, DonationProfile profile = null)
			: base(user, null, null, null, null, null, "Donations")
		{
			_Indicies = new int[3, 2];
			_Transactions = new List<DonationTransaction>();

			Profile = profile;

			CanMove = true;
			CanClose = true;
			CanDispose = true;
			CanResize = false;

			Sorted = false;

			ForceRecompile = true;
		}

		protected override void MainButtonHandler(GumpButton b)
		{
			if (_Admin)
			{
				new DonationAdminUI(User, Hide()).Send();
				return;
			}

			base.MainButtonHandler(b);
		}

		protected override void Compile()
		{
			_Admin = User.AccessLevel >= AutoDonate.Access;

			if (Profile == null || Profile.Account == null || (!_Admin && User.AccessLevel <= Profile.Account.AccessLevel &&
															   !Profile.Account.IsSharedWith(User.Account)))
			{
				Profile = AutoDonate.EnsureProfile(User.Account);
			}

			if (Profile != null)
			{
				_DonationTier = Profile.Tier;
				_DonationValue = Profile.TotalValue;
				_DonationCredit = Profile.TotalCredit;
				_DonationCount = _Admin ? Profile.Transactions.Count : Profile.Visible.Count();

				Title = "Donations: " + Profile.Account;
			}

			base.Compile();
		}

		protected override void CompileNodes(Dictionary<TreeGumpNode, Action<Rectangle, int, TreeGumpNode>> list)
		{
			list.Clear();

			list["Transactions"] = CompileTransactions;
			list["Transactions|Pending"] = CompileTransactions;
			list["Transactions|Claim"] = CompileTransactions;

			var t = _Admin ? Profile.Transactions.Values : Profile.Visible;

			foreach (var trans in t.OrderByDescending(o => o.Time))
			{
				if (trans.IsPending)
				{
					list["Transactions|Pending|" + trans.ID] = CompileTransaction;
				}
				else if (trans.IsProcessed)
				{
					list["Transactions|Claim|" + trans.ID] = CompileTransaction;
				}

				list["History|" + trans.FullPath] = CompileTransaction;
			}

			base.CompileNodes(list);
		}

		protected override void CompileNodeLayout(
			SuperGumpLayout layout,
			int x,
			int y,
			int w,
			int h,
			int index,
			TreeGumpNode node)
		{
			base.CompileNodeLayout(layout, x, y, w, h, index, node);

			if (Nodes == null || !Nodes.ContainsKey(node))
			{
				CompileEmptyNodeLayout(layout, x, y, w, h, index, node);
			}
		}

		protected override void CompileEmptyNodeLayout(
			SuperGumpLayout layout,
			int x,
			int y,
			int w,
			int h,
			int index,
			TreeGumpNode node)
		{
			base.CompileEmptyNodeLayout(layout, x, y, w, h, index, node);

			layout.Add("node/page/" + index, () => CompileOverview(new Rectangle(x, y, w, h), index, node));
		}

		protected virtual void CompileOverview(Rectangle bounds, int index, TreeGumpNode node)
		{
			var info = new StringBuilder();

			info.AppendLine(
				"Welcome to the Donation Exchange, {0}!",
				User.RawName.WrapUOHtmlColor(User.GetNotorietyColor(), HtmlColor));
			info.AppendLine();
			info.AppendLine("Select a category on the left to browse your transactions.");
			info.AppendLine();
			info.AppendLine("MY EXCHANGE".WrapUOHtmlBold().WrapUOHtmlColor(Color.Gold, false));
			info.AppendLine();

			GetProfileOverview(info);

			AddHtml(
				bounds.X + 5,
				bounds.Y,
				bounds.Width - 5,
				bounds.Height,
				info.ToString().WrapUOHtmlColor(HtmlColor),
				false,
				true);
		}

		protected virtual void CompileTransactions(Rectangle b, int index, TreeGumpNode node)
		{
			_Transactions.Clear();

			int i;

			switch (node.Name)
			{
				case "Pending":
				{
					_Transactions.AddRange(_Admin ? Profile.Pending : Profile.Visible.Where(t => t.IsPending));
					i = 2;
				}
					break;
				case "Claim":
				{
					_Transactions.AddRange(_Admin ? Profile.Processed : Profile.Visible.Where(t => t.IsProcessed));
					i = 1;
				}
					break;
				default:
				{
					_Transactions.AddRange(_Admin ? Profile.Transactions.Values : Profile.Visible);
					i = 0;
				}
					break;
			}

			_Transactions.Sort();

			_Indicies[i, 1] = _Transactions.Count;
			_Indicies[i, 0] = Math.Max(0, Math.Min(_Indicies[i, 1] - 1, _Indicies[i, 0]));

			var idx = 0;

			foreach (var t in _Transactions.Skip(_Indicies[i, 0]).Take(7))
			{
				var xx = b.X;
				var yy = b.Y + (idx++ * 50);

				AddRectangle(xx, yy, b.Width - 25, 45, Color.Black, t.IsProcessed ? Color.PaleGoldenrod : Color.Silver, 2);

				xx += 5;
				yy += 5;

				var label = String.Format("{0} ({1:#,0} {2})", t.ID, t.CreditTotal, AutoDonate.CMOptions.CurrencyName)
								  .WrapUOHtmlColor(HtmlColor, false);

				AddHtml(xx, yy, b.Width - 105, 40, label, false, false);

				xx = b.X + (b.Width - 85);

				label = String.Format(
								  "{0}{1:#,0.00} {2}",
								  AutoDonate.CMOptions.MoneySymbol,
								  t.Total,
								  AutoDonate.CMOptions.MoneyAbbr)
							  .WrapUOHtmlColor(HtmlColor, false);

				AddHtml(xx, yy, 65, 40, label, false, false);
			}

			_Transactions.Clear();

			AddBackground(b.X + (b.Width - 25), b.Y, 28, 351, 2620);

			AddScrollbarV(
				b.X + (b.Width - 24),
				b.Y,
				_Indicies[i, 1],
				_Indicies[i, 0],
				p =>
				{
					--_Indicies[i, 0];
					Refresh(true);
				},
				n =>
				{
					++_Indicies[i, 0];
					Refresh(true);
				},
				new Rectangle(6, 42, 13, 267),
				new Rectangle(6, 10, 13, 28),
				new Rectangle(6, 313, 13, 28),
				Tuple.Create(10740, 10742),
				Tuple.Create(10701, 10702, 10700),
				Tuple.Create(10721, 10722, 10720));
		}

		protected virtual void CompileTransaction(Rectangle b, int index, TreeGumpNode node)
		{
			var trans = Profile[node.Name];

			if (trans == null)
			{
				CompileOverview(b, index, node);
				return;
			}

			var cpHeight = _Admin ? 60 : 30;

			var html = new StringBuilder();

			GetTransactionOverview(trans, html, true, true, true);

			AddHtml(
				b.X + 5,
				b.Y,
				b.Width - 5,
				b.Height - cpHeight,
				html.ToString().WrapUOHtmlColor(Color.White, false),
				false,
				true);

			var bw = b.Width;
			var bh = cpHeight;

			if (_Admin)
			{
				bh /= 2;
			}

			switch (trans.State)
			{
				case TransactionState.Voided:
				{
					AddHtmlButton(
						b.X,
						b.Y + (b.Height - bh),
						bw,
						bh,
						o => OnVoidedTransaction(trans),
						"[VOIDED]",
						Color.OrangeRed,
						Color.Black,
						Color.OrangeRed,
						2);
				}
					break;
				case TransactionState.Pending:
				{
					AddHtmlButton(
						b.X,
						b.Y + (b.Height - bh),
						bw,
						bh,
						o => OnPendingTransaction(trans),
						"[PENDING]",
						Color.Yellow,
						Color.Black,
						Color.Yellow,
						2);
				}
					break;
				case TransactionState.Processed:
				{
					AddHtmlButton(
						b.X,
						b.Y + (b.Height - bh),
						bw,
						bh,
						o => OnClaimTransaction(trans),
						"[CLAIM]",
						Color.SkyBlue,
						Color.Black,
						Color.SkyBlue,
						2);
				}
					break;
				case TransactionState.Claimed:
				{
					AddHtmlButton(
						b.X,
						b.Y + (b.Height - bh),
						bw,
						bh,
						o => OnClaimedTransaction(trans),
						"[CLAIMED]",
						Color.LawnGreen,
						Color.Black,
						Color.LawnGreen,
						2);
				}
					break;
			}

			if (_Admin)
			{
				bw /= 2;

				AddHtmlButton(
					b.X,
					b.Y + (b.Height - (bh * 2)),
					bw,
					bh,
					o => OnTransactionEdit(trans),
					"[EDIT]",
					Color.Gold,
					Color.Black,
					Color.Gold,
					2);

				AddHtmlButton(
					b.X + bw,
					b.Y + (b.Height - (bh * 2)),
					bw,
					bh,
					o => OnTransactionTransfer(trans),
					"[TRANSFER]",
					Color.Gold,
					Color.Black,
					Color.Gold,
					2);
			}
		}

		protected virtual void GetTransactionOverview(
			DonationTransaction trans,
			StringBuilder info,
			bool details,
			bool exchange,
			bool status)
		{
			if (details)
			{
				info.AppendLine();
				info.AppendLine("Details");
				info.AppendLine();
				info.AppendLine("ID: {0}", trans.ID);
				info.AppendLine("Date: {0}", trans.Time.Value);

				if (_Admin)
				{
					info.AppendLine();
					info.AppendLine("Notes: {0}", trans.Notes);
					info.AppendLine();
					info.AppendLine("Extra: {0}", trans.Extra);
				}
			}

			if (exchange)
			{
				info.AppendLine();
				info.AppendLine("Exchange");
				info.AppendLine();
				info.AppendLine(
					"Value: {0}{1:#,0.00} {2}",
					AutoDonate.CMOptions.MoneySymbol,
					trans.Total,
					AutoDonate.CMOptions.MoneyAbbr);

				long credit, bonus;
				var total = trans.GetCredit(Profile, out credit, out bonus);

				info.AppendLine("Credit: {0:#,0} {1}", credit, AutoDonate.CMOptions.CurrencyName);
				info.AppendLine("Bonus: {0:#,0} {1}", bonus, AutoDonate.CMOptions.CurrencyName);
				info.AppendLine("Total: {0:#,0} {1}", total, AutoDonate.CMOptions.CurrencyName);
			}

			if (status)
			{
				info.AppendLine();
				info.AppendLine("Status");
				info.AppendLine();
				info.AppendLine("State: {0}", trans.State);

				switch (trans.State)
				{
					case TransactionState.Voided:
						info.AppendLine("Transaction has been voided.");
						break;
					case TransactionState.Pending:
						info.AppendLine("Transaction is pending verification.");
						break;
					case TransactionState.Processed:
						info.AppendLine("Transaction is complete and can be claimed.");
						break;
					case TransactionState.Claimed:
					{
						info.AppendLine("Transaction has been delivered.");
						info.AppendLine();
						info.AppendLine("Date: {0}", trans.DeliveryTime.Value);

						if (trans.DeliveredTo != null)
						{
							info.AppendLine("Recipient: {0}", trans.DeliveredTo);
						}
					}
						break;
				}
			}
		}

		public void GetProfileOverview(StringBuilder info)
		{
			var ms = AutoDonate.CMOptions.MoneySymbol;
			var ma = AutoDonate.CMOptions.MoneyAbbr;
			var cn = AutoDonate.CMOptions.CurrencyName;

			var val = _DonationTier.ToString("#,0").WrapUOHtmlColor(Color.LawnGreen, HtmlColor);
			info.AppendLine("Donation Tier: {0}", val);

			val = _DonationValue.ToString("#,0.00").WrapUOHtmlColor(Color.LawnGreen, HtmlColor);
			info.AppendLine("Donations Total: {0}{1} {2}", ms, val, ma);

			val = _DonationCredit.ToString("#,0").WrapUOHtmlColor(Color.LawnGreen, HtmlColor);
			info.AppendLine("Donations Claimed: {0} {1}", val, cn);
		}

		protected virtual void OnTransactionEdit(DonationTransaction trans)
		{
			Refresh();

			if (_Admin)
			{
				User.SendGump(new PropertiesGump(User, trans));
			}
		}

		protected virtual void OnTransactionTransfer(DonationTransaction trans)
		{
			new InputDialogGump(User, Refresh())
			{
				Title = "Transfer Transaction",
				Html = "Enter the account name of the recipient for the transfer.",
				InputText = trans.Account != null ? trans.Account.Username : String.Empty,
				Callback = (b, a) =>
				{
					if (User.AccessLevel >= AutoDonate.Access)
					{
						var acc = Accounts.GetAccount(a);

						if (acc == null)
						{
							User.SendMessage(34, "The account '{0}' does not exist.", a);
						}
						else if (trans.Account == acc)
						{
							User.SendMessage(34, "The transaction is already bound to '{0}'", a);
						}
						else
						{
							trans.SetAccount(acc);
							User.SendMessage(85, "The transaction has been transferred to '{0}'", a);
						}
					}

					Refresh(true);
				}
			}.Send();
		}

		protected virtual void OnVoidedTransaction(DonationTransaction trans)
		{
			var html = new StringBuilder();

			GetTransactionOverview(trans, html, false, false, true);

			new NoticeDialogGump(User, Refresh())
			{
				Title = "Transaction Voided",
				Html = html.ToString()
			}.Send();
		}

		protected virtual void OnPendingTransaction(DonationTransaction trans)
		{
			var html = new StringBuilder();

			GetTransactionOverview(trans, html, false, false, true);

			new NoticeDialogGump(User, Refresh())
			{
				Title = "Transaction Pending",
				Html = html.ToString()
			}.Send();
		}

		protected virtual void OnClaimTransaction(DonationTransaction trans)
		{
			var html = new StringBuilder();

			GetTransactionOverview(trans, html, false, true, false);

			html.AppendLine();
			html.AppendLine("Click OK to claim this transaction!");

			new ConfirmDialogGump(User, Refresh())
			{
				Title = "Reward Claim",
				Html = html.ToString(),
				AcceptHandler = b =>
				{
					if (trans.Claim(User))
					{
						SelectedNode = "Transactions|" + trans.FullPath;

						User.SendMessage(85, "You claimed the transaction!");
					}

					Refresh(true);
				}
			}.Send();
		}

		protected virtual void OnClaimedTransaction(DonationTransaction trans)
		{
			var info = new StringBuilder();

			GetTransactionOverview(trans, info, false, false, true);

			new NoticeDialogGump(User, Refresh())
			{
				Title = "Reward Delivered",
				Html = info.ToString()
			}.Send();
		}

		protected override void OnDisposed()
		{
			_Indicies = null;

			_Transactions.Free(true);
			_Transactions = null;

			base.OnDisposed();
		}
	}
}