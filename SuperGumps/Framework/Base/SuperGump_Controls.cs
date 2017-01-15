#region Header
//   Vorspire    _,-'/-'/  SuperGump_Controls.cs
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
using System.Drawing;
using System.Linq;

using Server;
using Server.Gumps;

using Ultima;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public void AddPage()
		{
			AddPage(IndexedPage + 1);
		}

		public virtual void AddPageBackButton(int x, int y, int normalID, int pressedID)
		{
			AddPageBackButton(x, y, normalID, pressedID, IndexedPage);
		}

		public virtual void AddPageNextButton(int x, int y, int normalID, int pressedID)
		{
			AddPageNextButton(x, y, normalID, pressedID, IndexedPage);
		}

		public virtual void AddPageButton(int x, int y, int normalID, int pressedID)
		{
			AddPageButton(x, y, normalID, pressedID, IndexedPage);
		}

		public virtual void AddPageBackButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddPageButton(x, y, normalID, pressedID, page - 1);
			AddTooltip(1011067); // Previous Page
		}

		public virtual void AddPageNextButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddPageButton(x, y, normalID, pressedID, page + 1);
			AddTooltip(1011066); // Next Page
		}

		public virtual void AddPageButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddButton(x, y, normalID, pressedID, -1, GumpButtonType.Page, page);
		}

		public virtual void AddModalRegion(int x, int y, int w, int h)
		{
			Add(new GumpModal(x, y, w, h, 2624));
		}

		public virtual void AddModalRegion(int x, int y, int w, int h, int gumpID)
		{
			Add(new GumpModal(x, y, w, h, gumpID));
		}

		public virtual void AddPixel(int x, int y, Color color)
		{
			Add(new GumpPixel(x, y, color));
		}

		public virtual void AddRectangle(int x, int y, int w, int h, Color color, bool filled)
		{
			Add(new GumpRectangle(x, y, w, h, color, filled));
		}

		public virtual void AddRectangle(int x, int y, int w, int h, Color color, int borderSize)
		{
			Add(new GumpRectangle(x, y, w, h, color, borderSize));
		}

		public virtual void AddRectangle(int x, int y, int w, int h, Color fill, Color border, int borderSize)
		{
			Add(new GumpRectangle(x, y, w, h, fill, border, borderSize));
		}

		public virtual void AddRectangle(Rectangle2D bounds, Color color, bool filled)
		{
			Add(new GumpRectangle(bounds, color, filled));
		}

		public virtual void AddRectangle(Rectangle2D bounds, Color color, int borderSize)
		{
			Add(new GumpRectangle(bounds, color, borderSize));
		}

		public virtual void AddRectangle(Rectangle2D bounds, Color fill, Color border, int borderSize)
		{
			Add(new GumpRectangle(bounds, fill, border, borderSize));
		}

		public virtual void AddLine(IPoint2D start, IPoint2D end, Color color)
		{
			Add(new GumpLine(start, end, color, 1));
		}

		public virtual void AddLine(IPoint2D start, IPoint2D end, Color color, int size)
		{
			Add(new GumpLine(start, end, color, size));
		}

		public virtual void AddLine(IPoint2D start, Angle angle, int length, Color color)
		{
			Add(new GumpLine(start, angle, length, color, 1));
		}

		public virtual void AddLine(IPoint2D start, Angle angle, int length, Color color, int size)
		{
			Add(new GumpLine(start.X, start.Y, angle, length, color, size));
		}

		public virtual void AddLine(int x, int y, Angle angle, int length, Color color)
		{
			Add(new GumpLine(x, y, angle, length, color, 1));
		}

		public virtual void AddLine(int x, int y, Angle angle, int length, Color color, int size)
		{
			Add(new GumpLine(x, y, angle, length, color, size));
		}

		public virtual void AddGradient(int x, int y, int w, int h, Direction45 to, ColorGradient g)
		{
			Add(new GumpGradient(x, y, w, h, to, g));
		}

		public virtual void AddItemShadow(int x, int y, int itemID, int itemHue, Angle shadowAngle, int shadowOffset)
		{
			Add(new GumpItemShadow(x, y, itemID, itemHue, shadowAngle, shadowOffset));
		}

		public virtual void AddItemShadow(int x, int y, int itemID)
		{
			Add(new GumpItemShadow(x, y, itemID));
		}

		public virtual void AddItemShadow(int x, int y, int itemID, int itemHue)
		{
			Add(new GumpItemShadow(x, y, itemID, itemHue));
		}

		public virtual void AddItemShadow(int x, int y, int itemID, int itemHue, Angle shadowAngle)
		{
			Add(new GumpItemShadow(x, y, itemID, itemHue, shadowAngle));
		}

		public virtual void AddItemShadow(
			int x,
			int y,
			int itemID,
			int itemHue,
			Angle shadowAngle,
			int shadowOffset,
			int shadowHue)
		{
			Add(new GumpItemShadow(x, y, itemID, itemHue, shadowAngle, shadowOffset, shadowHue));
		}

		public virtual void AddImageShadow(int x, int y, int itemID, int itemHue, Angle shadowAngle, int shadowOffset)
		{
			Add(new GumpImageShadow(x, y, itemID, itemHue, shadowAngle, shadowOffset));
		}

		public virtual void AddImageShadow(int x, int y, int itemID)
		{
			Add(new GumpImageShadow(x, y, itemID));
		}

		public virtual void AddImageShadow(int x, int y, int itemID, int itemHue)
		{
			Add(new GumpImageShadow(x, y, itemID, itemHue));
		}

		public virtual void AddImageShadow(int x, int y, int itemID, int itemHue, Angle shadowAngle)
		{
			Add(new GumpImageShadow(x, y, itemID, itemHue, shadowAngle));
		}

		public virtual void AddImageShadow(
			int x,
			int y,
			int itemID,
			int itemHue,
			Angle shadowAngle,
			int shadowOffset,
			int shadowHue)
		{
			Add(new GumpImageShadow(x, y, itemID, itemHue, shadowAngle, shadowOffset, shadowHue));
		}

		public virtual void AddProgress(
			int x,
			int y,
			int w,
			int h,
			double progress,
			Direction dir = Direction.Right,
			Color? background = null,
			Color? foreground = null,
			Color? border = null,
			int borderSize = 0)
		{
			Add(new GumpProgress(x, y, w, h, progress, dir, background, foreground, border, borderSize));
		}

		public virtual void AddPaperdoll(int x, int y, bool props, Mobile m)
		{
			Add(new GumpPaperdoll(x, y, props, m));
		}

		public virtual void AddPaperdoll(
			int x,
			int y,
			bool props,
			List<Item> items,
			Body body,
			int bodyHue,
			int solidHue,
			int hairID,
			int hairHue,
			int facialHairID,
			int facialHairHue)
		{
			Add(new GumpPaperdoll(x, y, props, items, body, bodyHue, solidHue, hairID, hairHue, facialHairID, facialHairHue));
		}

		public virtual void AddClock(
			int x,
			int y,
			DateTime time,
			bool background = true,
			int backgroundHue = 0,
			bool face = true,
			int faceHue = 0,
			bool numerals = true,
			bool numbers = true,
			Color? numbersColor = null,
			bool hours = true,
			Color? hoursColor = null,
			bool minutes = true,
			Color? minutesColor = null,
			bool seconds = true,
			Color? secondsColor = null)
		{
			Add(
				new GumpClock(
					x,
					y,
					time,
					background,
					backgroundHue,
					face,
					faceHue,
					numerals,
					numbers,
					numbersColor,
					hours,
					hoursColor,
					minutes,
					minutesColor,
					seconds,
					secondsColor));
		}

		public virtual void AddImageNumber(int x, int y, int value, int hue = 0, Axis centering = Axis.None)
		{
			Add(new GumpImageNumber(x, y, value, hue, centering));
		}

		public virtual void AddImageTime(int x, int y, TimeSpan value, int hue = 0, Axis centering = Axis.None)
		{
			Add(new GumpImageTime(x, y, value, hue, centering));
		}

		public virtual void AddEnumSelect<TEnum>(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			TEnum selected,
			Action<TEnum> onSelect) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			AddEnumSelect(
				x,
				y,
				normalID,
				pressedID,
				labelXOffset,
				labelYOffset,
				labelWidth,
				labelHeight,
				labelHue,
				selected,
				onSelect,
				true);
		}

		public virtual void AddEnumSelect<TEnum>(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			TEnum selected,
			Action<TEnum> onSelect,
			bool resolveMenuPos) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!typeof(TEnum).IsEnum)
			{
				return;
			}

			var opts = new MenuGumpOptions();

			ListGumpEntry? def = null;

			foreach (var o in (default(TEnum) as Enum).EnumerateValues<TEnum>(false))
			{
				var v = o;
				var e = new ListGumpEntry(v.ToString(), b => onSelect(v));

				opts.AppendEntry(e);

				if (Equals(v, selected))
				{
					def = e;
				}
			}

			if (def != null)
			{
				AddMenuButton(
					x,
					y,
					normalID,
					pressedID,
					labelXOffset,
					labelYOffset,
					labelWidth,
					labelHeight,
					labelHue,
					opts,
					def.Value,
					resolveMenuPos);
			}
		}

		public virtual void AddMenuButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			MenuGumpOptions opts,
			ListGumpEntry defSelection)
		{
			AddMenuButton(
				x,
				y,
				normalID,
				pressedID,
				labelXOffset,
				labelYOffset,
				labelWidth,
				labelHeight,
				labelHue,
				opts,
				defSelection,
				true);
		}

		public virtual void AddMenuButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			MenuGumpOptions opts,
			ListGumpEntry defSelection,
			bool resolveMenuPos)
		{
			AddButton(x, y, normalID, pressedID, b => Send(new MenuGump(User, Refresh(), opts, resolveMenuPos ? b : null)));
			AddLabel(x + labelXOffset, y + labelYOffset, labelHue, defSelection.Label ?? String.Empty);
		}

		public virtual void AddScrollbar(
			Axis axis,
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			int trackX,
			int trackY,
			int trackW,
			int trackH,
			int trackBackgroundID,
			int trackForegroundID,
			int prevX,
			int prevY,
			int prevW,
			int prevH,
			int prevDisplayID,
			int prevPressedID,
			int prevDisabledID,
			int nextX,
			int nextY,
			int nextW,
			int nextH,
			int nextDisplayID,
			int nextPressedID,
			int nextDisabledID)
		{
			AddScrollbar(
				axis,
				x,
				y,
				range,
				value,
				prev,
				next,
				new Rectangle2D(trackX, trackY, trackW, trackH),
				new Rectangle2D(prevX, prevY, prevW, prevH),
				new Rectangle2D(nextX, nextY, nextW, nextH),
				Tuple.Create(trackBackgroundID, trackForegroundID),
				Tuple.Create(prevDisplayID, prevPressedID, prevDisabledID),
				Tuple.Create(nextDisplayID, nextPressedID, nextDisabledID));
		}

		public virtual void AddScrollbar(
			Axis axis,
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			Rectangle2D trackBounds,
			Rectangle2D prevBounds,
			Rectangle2D nextBounds,
			Tuple<int, int> trackIDs = null,
			Tuple<int, int, int> prevIDs = null,
			Tuple<int, int, int> nextIDs = null,
			bool toolTips = true)
		{
			switch (axis)
			{
				case Axis.Vertical:
					AddScrollbarV(x, y, range, value, prev, next, trackBounds, prevBounds, nextBounds, trackIDs, prevIDs, nextIDs);
					break;
				case Axis.Horizontal:
					AddScrollbarH(x, y, range, value, prev, next, trackBounds, prevBounds, nextBounds, trackIDs, prevIDs, nextIDs);
					break;
			}
		}

		public virtual void AddScrollbarV(
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			Rectangle2D trackBounds,
			Rectangle2D prevBounds,
			Rectangle2D nextBounds,
			Tuple<int, int> trackIDs = null,
			Tuple<int, int, int> prevIDs = null,
			Tuple<int, int, int> nextIDs = null,
			bool toolTips = true)
		{
			trackIDs = trackIDs ?? new Tuple<int, int>(10740, 10742);
			prevIDs = prevIDs ?? new Tuple<int, int, int>(10701, 10702, 10700);
			nextIDs = nextIDs ?? new Tuple<int, int, int>(10721, 10722, 10720);

			range = Math.Max(1, range);
			value = Math.Max(0, Math.Min(range - 1, value));

			var bh = Math.Min(trackBounds.Height, Math.Max(1, trackBounds.Height / (double)range));
			var by = Math.Min(trackBounds.Height, Math.Max(bh, trackBounds.Height * ((value + 1) / (double)range))) - bh;

			var barBounds = new Rectangle2D(trackBounds.X, trackBounds.Y + (int)by, trackBounds.Width, (int)bh);

			if (value > 0)
			{
				AddButton(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item1, prevIDs.Item2, prev);

				if (toolTips)
				{
					AddTooltip(1011067);
				}
			}
			else if (prevIDs.Item3 > 0)
			{
				AddImage(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item3);
			}

			AddImageTiled(x + trackBounds.X, y + trackBounds.Y, trackBounds.Width, trackBounds.Height, trackIDs.Item1);

			if (range > 1)
			{
				AddImageTiled(x + barBounds.X, y + barBounds.Y, barBounds.Width, barBounds.Height, trackIDs.Item2);
			}

			if (value + 1 < range)
			{
				AddButton(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item1, nextIDs.Item2, next);

				if (toolTips)
				{
					AddTooltip(1011066);
				}
			}
			else if (nextIDs.Item3 > 0)
			{
				AddImage(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item3);
			}
		}

		public virtual void AddScrollbarH(
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			Rectangle2D trackBounds,
			Rectangle2D prevBounds,
			Rectangle2D nextBounds,
			Tuple<int, int> trackIDs = null,
			Tuple<int, int, int> prevIDs = null,
			Tuple<int, int, int> nextIDs = null,
			bool toolTips = true)
		{
			trackIDs = trackIDs ?? new Tuple<int, int>(10740, 10742);
			prevIDs = prevIDs ?? new Tuple<int, int, int>(10731, 10732, 10730);
			nextIDs = nextIDs ?? new Tuple<int, int, int>(10711, 10712, 10710);

			range = Math.Max(1, range);
			value = Math.Max(0, Math.Min(range, value));

			var bw = Math.Min(trackBounds.Width, Math.Max(1, trackBounds.Width / (double)range));
			var bx = Math.Min(trackBounds.Width, Math.Max(bw, trackBounds.Width * ((value + 1) / (double)range))) - bw;

			var barBounds = new Rectangle2D(trackBounds.X + (int)bx, trackBounds.Y, (int)bw, trackBounds.Height);

			if (value > 0)
			{
				AddButton(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item1, prevIDs.Item2, prev);

				if (toolTips)
				{
					AddTooltip(1011067);
				}
			}
			else
			{
				AddImage(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item3);
			}

			AddImageTiled(x + trackBounds.X, y + trackBounds.Y, trackBounds.Width, trackBounds.Height, trackIDs.Item1);

			if (range > 1)
			{
				AddImageTiled(x + barBounds.X, y + barBounds.Y, barBounds.Width, barBounds.Height, trackIDs.Item2);
			}

			if (value + 1 < range)
			{
				AddButton(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item1, nextIDs.Item2, next);

				if (toolTips)
				{
					AddTooltip(1011066);
				}
			}
			else
			{
				AddImage(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item3);
			}
		}

		public void AddHtmlButton(
			int x,
			int y,
			int w,
			int h,
			Action<GumpButton> handler,
			string label,
			Color labelColor,
			Color fillColor)
		{
			AddHtmlButton(x, y, w, h, handler, label, labelColor, fillColor, Color.Empty, 0);
		}

		public void AddHtmlButton(
			int x,
			int y,
			int w,
			int h,
			Action<GumpButton> handler,
			string label,
			Color labelColor,
			Color fillColor,
			Color borderColor,
			int borderSize)
		{
			if (w * h <= 0)
			{
				return;
			}

			if (labelColor.IsEmpty)
			{
				labelColor = DefaultHtmlColor;
			}

			if (fillColor.IsEmpty)
			{
				fillColor = Color.Black;
			}

			if (borderColor.IsEmpty)
			{
				borderSize = 0;
			}

			const int bi = 87;
			const int bw = 16;
			const int bh = 16;

			w = Math.Max(bw, w);
			h = Math.Max(bh, h);

			var cols = (int)Math.Ceiling(w / (double)bw);
			var rows = (int)Math.Ceiling(h / (double)bh);
			
			int c = 0, r = 0, xo = x, yo = y, wo = x + w, ho = y + h;

			while (c++ < cols)
			{
				if (xo + bw > wo)
				{
					xo = wo - bw;
				}

				AddButton(xo, yo, bi, bi, handler);

				xo += bw;

				if (c % cols == 0)
				{
					xo = x;
					yo += bh;

					if (yo + bh > ho)
					{
						yo = ho - bh;
					}

					if (r++ < rows)
					{
						c = 0;
					}
					else
					{
						break;
					}
				}
			}

			AddRectangle(x, y, w, h, fillColor, borderColor, borderSize);

			w -= 10 + (borderSize * 2);
			h -= 10 + (borderSize * 2);

			if (w * h > 0 && !String.IsNullOrWhiteSpace(label))
			{
				h = Math.Max(40, h);

				AddHtml(x + 5, y + 5, w, h, label.WrapUOHtmlColor(labelColor, false), false, false);
			}
		}

		public void AddColoredButton(int x, int y, int w, int h, Color fillColor, Action<GumpButton> handler)
		{
			AddColoredButton(x, y, w, h, fillColor, Color.Empty, 0, handler);
		}

		public void AddColoredButton(
			int x,
			int y,
			int w,
			int h,
			Color fillColor,
			Color borderColor,
			int borderSize,
			Action<GumpButton> handler)
		{
			if (w * h <= 0)
			{
				return;
			}

			if (fillColor.IsEmpty)
			{
				fillColor = Color.Black;
			}

			if (borderColor.IsEmpty)
			{
				borderSize = 0;
			}

			const int bi = 87;
			const int bw = 16;
			const int bh = 16;

			w = Math.Max(bw, w);
			h = Math.Max(bh, h);

			var cols = (int)Math.Ceiling(w / (double)bw);
			var rows = (int)Math.Ceiling(h / (double)bh);

			int c = 0, r = 0, xo = x, yo = y, wo = x + w, ho = y + h;

			while (c++ < cols)
			{
				if (xo + bw > wo)
				{
					xo = wo - bw;
				}

				AddButton(xo, yo, bi, bi, handler);

				xo += bw;

				if (c % cols == 0)
				{
					xo = x;
					yo += bh;

					if (yo + bh > ho)
					{
						yo = ho - bh;
					}

					if (r++ < rows)
					{
						c = 0;
					}
					else
					{
						break;
					}
				}
			}

			AddRectangle(x, y, w, h, fillColor, borderColor, borderSize);
		}

		public void AddTileButton(int x, int y, int w, int h, Action<GumpButton> handler)
		{
			if (w * h <= 0)
			{
				return;
			}

			const int bi = 87;
			const int bw = 16;
			const int bh = 16;

			w = Math.Max(bw, w);
			h = Math.Max(bh, h);

			var cols = (int)Math.Ceiling(w / (double)bw);
			var rows = (int)Math.Ceiling(h / (double)bh);

			int c = 0, r = 0, xo = x, yo = y, wo = x + w, ho = y + h;

			while (c++ < cols)
			{
				if (xo + bw > wo)
				{
					xo = wo - bw;
				}

				AddButton(xo, yo, bi, bi, handler);

				xo += bw;

				if (c % cols == 0)
				{
					xo = x;
					yo += bh;

					if (yo + bh > ho)
					{
						yo = ho - bh;
					}

					if (r++ < rows)
					{
						c = 0;
					}
					else
					{
						break;
					}
				}
			}
		}

		private Dictionary<int, Enum> _Accordions;

		public void AddAccordion<TEnum>(
			int x,
			int y,
			int w,
			int h,
			TEnum? initValue,
			Action<int, int, int, int, TEnum> onRender) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var sup = SupportsUltimaStore;

			var pad = sup ? 15 : 10;
			var bgID = sup ? 40000 : 9270;
			var btnNormal = sup ? 40016 : 9909;
			var btnSelected = sup ? 40027 : 9904;

			AddAccordion(x, y, w, h, pad, bgID, btnNormal, btnSelected, Color.White, Color.Gold, initValue, onRender);
		}

		public void AddAccordion<TEnum>(
			int x,
			int y,
			int w,
			int h,
			int pad,
			int bgID,
			int btnNormal,
			int btnSelected,
			Color txtNormal,
			Color txtSelected,
			TEnum? initValue,
			Action<int, int, int, int, TEnum> onRender) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (w <= 0 || h <= 0 || pad * 2 >= w || pad * 2 >= h || onRender == null)
			{
				return;
			}

			var instance = default(TEnum) as Enum;

			if (instance == null)
			{
				return;
			}

			var values = instance.GetValues<Enum>(false);

			if (values.Length == 0)
			{
				return;
			}

			if (_Accordions == null)
			{
				_Accordions = new Dictionary<int, Enum>();
			}

			var hash = onRender.GetHashCode();
			var value = _Accordions.GetValue(hash);

			var ini = initValue != null ? initValue.Value as Enum : null;

			if (value == null || !values.Contains(value))
			{
				_Accordions[hash] = value = ini;
			}

			var btnSize = GumpsExtUtility.GetImageSize(btnNormal);

			var titleO = btnSize.Width + (pad * 2);
			var titleH = btnSize.Height + (pad * 2);

			var titleC = values.Length;

			var panelH = h - (titleC * titleH);

			foreach (var val in values.Where(v => v != null))
			{
				var v = val;
				var s = Enum.Equals(value, v);
				var l = v.ToString().SpaceWords().WrapUOHtmlBig().WrapUOHtmlColor(s ? txtSelected : txtNormal, false);

				if (bgID > -1)
				{
					AddBackground(x, y, w, s ? titleH + panelH : titleH, bgID);
				}

				AddButton(
					x + pad,
					y + pad,
					s ? btnSelected : btnNormal,
					s ? btnNormal : btnSelected,
					b =>
					{
						_Accordions[hash] = s ? ini : v;

						Refresh(true);
					});

				AddHtml(x + titleO, y + ((titleH / 2) - 10), w - titleO, 40, l, false, false);

				if (s)
				{
					y += titleH;

					onRender(x + pad, y, w - (pad * 2), panelH - (pad * 2), (TEnum)(object)v);

					y += panelH;
				}
				else
				{
					y += titleH;
				}
			}
		}

		public void AddLink(int x, int y, int w, int h, string text, string uri)
		{
			if (w > 0 && h > 0 && !String.IsNullOrWhiteSpace(text) && Uri.IsWellFormedUriString(uri, UriKind.Absolute))
			{
				text = text.WrapUOHtmlUrl(uri);
				
				AddHtml(x, y, w, h, text, false, false);
			}
		}

		public void AddLink(string uri)
		{
			if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) || Entries.Count <= 0)
			{
				return;
			}

			var html = Entries[Entries.Count - 1] as GumpHtml;

			if (html != null)
			{
				html.Text = html.Text.WrapUOHtmlUrl(uri);
			}
		}

		public void Add(SuperGumpEntry e)
		{
			if (OnBeforeAdd(e))
			{
				base.Add(e);

				OnAdded(e);
			}
		}

		protected virtual bool OnBeforeAdd(SuperGumpEntry e)
		{
			return e != null;
		}

		protected virtual void OnAdded(SuperGumpEntry e)
		{ }
	}
}