#region Header
//   Vorspire    _,-'/-'/  AutoDonate.cs
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
using System.Drawing;
using System.Linq;
using System.Net;

using Server;
using Server.Accounting;

using VitaNex.IO;
using VitaNex.SuperGumps;
using VitaNex.Web;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public static partial class AutoDonate
	{
		public const AccessLevel Access = AccessLevel.Owner;

		public static DonationOptions CMOptions { get; private set; }

		public static BinaryDataStore<string, DonationTransaction> Transactions { get; private set; }

		public static BinaryDirectoryDataStore<IAccount, DonationProfile> Profiles { get; private set; }

		private static void OnLogin(LoginEventArgs e)
		{
			SpotCheck(e.Mobile);
		}

		public static void SpotCheck(IAccount a)
		{
			SpotCheck(a.FindMobiles(p => p != null && p.IsOnline()).FirstOrDefault());
		}

		public static void SpotCheck(Mobile user)
		{
			if (user == null || user.Deleted || !user.Alive || !user.IsOnline() || Profiles.Status != DataStoreStatus.Idle ||
				!CMOptions.ModuleEnabled || CMOptions.CurrencyType == null || CMOptions.CurrencyPrice <= 0)
			{
				return;
			}

			var profile = FindProfile(user.Account);

			if (profile == null)
			{
				return;
			}

			if (profile.Processed.Any())
			{
				var message = String.Format("Hey, {0}!\nYou have unclaimed donation credit!", user.RawName);

				user.SendNotification(
					message,
					false,
					1.0,
					3.0,
					Color.LawnGreen,
					ui =>
					{
						ui.CanClose = false;
						ui.CanDispose = false;

						ui.AddOption(
							"Claim!",
							b =>
							{
								CheckDonate(user);
								ui.Close();
							});
					});
			}
		}

		private static void HandleWebForm(WebAPIContext context)
		{
			if (!CMOptions.WebForm.Enabled)
			{
				context.Response.Data = String.Empty;
				context.Response.Status = HttpStatusCode.NoContent;
				return;
			}

			context.Response.Data = CMOptions.WebForm.Generate();
			context.Response.ContentType = "html";
		}

		private static void HandleAccountCheck(WebAPIContext context)
		{
			if (!String.IsNullOrWhiteSpace(context.Request.Queries["username"]))
			{
				var acc = Accounts.GetAccount(context.Request.Queries["username"]);

				context.Response.Data = acc != null ? "VALID" : "INVALID";
			}
			else
			{
				context.Response.Data = "INVALID";
			}

			context.Response.ContentType = "txt";
		}
		
		private static void HandleIPN(WebAPIContext context)
		{
			var test = context.Request.Queries["test"] != null || Insensitive.Contains(context.Request.Data, "test_ipn=1");

			var endpoint = test ? "www.sandbox" : "www";

			var paypal = String.Format("https://{0}.paypal.com/cgi-bin/webscr", endpoint);
			
			WebAPI.BeginRequest(paypal, context.Request.Data, BeginVerification, EndVerification);
		}

		private static void BeginVerification(HttpWebRequest webReq, string state)
		{
			webReq.Method = "POST";
			webReq.ContentType = "application/x-www-form-urlencoded";
			webReq.SetContent("cmd=_notify-validate&" + state);
		}

		private static void EndVerification(HttpWebRequest webReq, string state, HttpWebResponse webRes)
		{
			var content = webRes.GetContent();

			if (Insensitive.Contains(content, "VERIFIED"))
			{
				using (var queries = new WebAPIQueries(state))
				{
					ProcessTransaction(queries);
				}

				RefreshAdminUI();
			}
		}

		private static void ProcessTransaction(WebAPIQueries queries)
		{
			var id = queries["txn_id"];

			if (String.IsNullOrWhiteSpace(id))
			{
				return;
			}

			var status = queries["payment_status"];

			if (String.IsNullOrWhiteSpace(status))
			{
				return;
			}

			TransactionState state;

			status = status.Trim();

			switch (status.ToUpper())
			{
				case "PENDING":
				case "IN-PROGRESS":
					state = TransactionState.Pending;
					break;
				case "COMPLETED":
					state = TransactionState.Processed;
					break;
				default:
					state = TransactionState.Voided;
					break;
			}

			long credit;
			double value;

			ExtractCart(queries, out credit, out value);

			var custom = queries["custom"] ?? String.Empty;

			var trans = Transactions.GetValue(id);

			var create = trans == null;

			if (create)
			{
				var email = queries["payer_email"] ?? String.Empty;
				var notes = queries["payer_note"] ?? String.Empty;
				var extra = queries["extra_info"] ?? String.Empty;

				var a = Accounts.GetAccount(custom) ?? CMOptions.FallbackAccount;

				Transactions[id] = trans = new DonationTransaction(id, a, email, value, credit, notes, extra);

				var profile = EnsureProfile(a);

				if (profile == null)
				{
					state = TransactionState.Voided;
					trans.Extra += "{VOID: NO PROFILE}";
				}
				else
				{
					profile.Add(trans);
				}
			}

			if (!VerifyValue(queries, "business", CMOptions.Business) && !VerifyValue(queries, "receiver_email", CMOptions.Business) &&
				!VerifyValue(queries, "receiver_id", CMOptions.Business))
			{
				state = TransactionState.Voided;
				trans.Extra += "{VOID: UNEXPECTED BUSINESS}";
			}

			if (trans.Total != value)
			{
				state = TransactionState.Voided;
				trans.Extra += "{VOID: TOTAL CHANGED}";
			}

			if (queries["test"] != null || queries["test_ipn"] != null)
			{
				state = TransactionState.Voided;
				trans.Extra += "{VOID: TESTING}";
			}

			switch (state)
			{
				case TransactionState.Processed:
					trans.Process();
					break;
				case TransactionState.Voided:
					trans.Void();
					break;
			}

			if (create && trans.State == TransactionState.Pending)
			{
				DonationEvents.InvokeTransPending(trans);
			}

			SpotCheck(trans.Account);
		}

		private static bool VerifyValue(WebAPIQueries queries, string key, string val)
		{
			return !String.IsNullOrWhiteSpace(queries[key]) && Insensitive.Contains(queries[key], val);
		}

		private static void ExtractCart(WebAPIQueries queries, out long credit, out double value)
		{
			credit = 0;
			value = 0;

			foreach (var kv in queries)
			{
				var i = kv.Key.IndexOf("item_number", StringComparison.OrdinalIgnoreCase);

				if (i < 0)
				{
					continue;
				}

				string k = "0";

				if (i + 11 < kv.Key.Length)
				{
					k = kv.Key.Substring(i + 11);
				}

				if (String.IsNullOrWhiteSpace(k))
				{
					k = "0";
				}

				if (!Int32.TryParse(k, out i))
				{
					continue;
				}

				if (Insensitive.Equals(kv.Value, CMOptions.CurrencyType.TypeName))
				{
					var subTotal = Math.Max(0, Int64.Parse(queries["quantity" + i.ToString("#")] ?? "0"));
					var subGross = Math.Max(0, Double.Parse(queries["mc_gross" + i.ToString("#")] ?? "0"));

					if (subTotal <= 0)
					{
						subTotal = (long)(subGross / CMOptions.CurrencyPrice);
					}
					else
					{
						var expGross = subTotal * CMOptions.CurrencyPrice;

						if (Math.Abs(expGross - subGross) > CMOptions.CurrencyPrice)
						{
							subGross = expGross;
						}
					}

					credit += subTotal;
					value += subGross;
				}
			}
		}

		public static DonationProfile FindProfile(IAccount a)
		{
			return Profiles.GetValue(a);
		}

		public static DonationProfile EnsureProfile(IAccount a)
		{
			var p = Profiles.GetValue(a);

			if (p == null && a != null)
			{
				Profiles[a] = p = new DonationProfile(a);
			}

			return p;
		}

		public static void CheckDonate(Mobile user, bool message = true)
		{
			if (user == null || user.Deleted)
			{
				return;
			}

			if (!CMOptions.ModuleEnabled)
			{
				if (message)
				{
					user.SendMessage("The donation exchange is currently unavailable, please try again later.");
				}

				return;
			}

			if (!user.Alive)
			{
				if (message)
				{
					user.SendMessage("You must be alive to do that!");
				}

				return;
			}

			if (Profiles.Status != DataStoreStatus.Idle)
			{
				if (message)
				{
					user.SendMessage("The donation exchange is busy, please try again in a few moments.");
				}

				return;
			}

			if (CMOptions.CurrencyType == null || CMOptions.CurrencyPrice <= 0)
			{
				if (message)
				{
					user.SendMessage("Currency conversion is currently disabled, contact a member of staff to handle your donations.");
				}

				return;
			}

			var profile = FindProfile(user.Account);

			if (profile != null)
			{
				var count = profile.Visible.Count();

				if (count == 0)
				{
					if (message)
					{
						user.SendMessage("There are no current donation records for your account.");
					}
				}
				else
				{
					if (message)
					{
						user.SendMessage("Thank you for your donation{0}, {1}!", count != 1 ? "s" : "", user.RawName);
					}

					DonationProfileUI.DisplayTo(user, profile, profile.Visible.FirstOrDefault());
				}
			}
			else if (message)
			{
				user.SendMessage("There are no current donation records for your account.");
			}
		}

		public static void CheckConfig(Mobile user)
		{
			if (user != null && !user.Deleted && user.AccessLevel >= Access)
			{
				(SuperGump.EnumerateInstances<DonationAdminUI>(user).FirstOrDefault(g => g != null && !g.IsDisposed) ??
				 new DonationAdminUI(user)).Refresh(true);
			}
		}

		public static void RefreshAdminUI()
		{
			foreach (var g in SuperGump.GlobalInstances.Values.OfType<DonationAdminUI>())
			{
				g.Refresh(true);
			}
		}
	}
}