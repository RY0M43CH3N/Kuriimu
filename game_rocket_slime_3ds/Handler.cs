﻿using game_rocket_slime_3ds.Properties;
using KuriimuContract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace game_rocket_slime_3ds
{
	public class Handler : IGameHandler
	{
		Dictionary<string, string> _pairs = new Dictionary<string, string> {
			["<prompt>"] ="\x1F\x20",
			["<player>"] ="\x1F\x05",
			["<name>"] ="\x1F\x02",
			["</name>"] ="\x02",
			["<top?>"] ="\x1F\x100",
			["<middle?>"] ="\x1F\x103",
			["<bottom?>"] ="\x1F\x200",
			["<u1>"] ="\x1F\x00",
			["<next>"] ="\x1F\x15",
			["<end>"] ="\x1F\x115",
			["<u3>"] ="\x17",
    
			// Color
			["<color-default>"] = "\x13\x00",
			["<color-red>"] = "\x13\x01",
			["<color-???>"] = "\x13\x02",
			["<color-blue>"] = "\x13\x03"
		};

		Dictionary<char, BitFont> _font = null;

		#region Properties

		// Information
		public string Name => "Rocket Slime 3DS";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		#endregion

		public Handler()
		{
			InitializeFont();
		}

		public string GetKuriimuString(string rawString)
		{
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public Bitmap GeneratePreview(string rawString)
		{
			Bitmap img = new Bitmap(Resources.blank_top);

			Graphics gfx = Graphics.FromImage(img);
			gfx.SmoothingMode = SmoothingMode.HighQuality;
			gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

			Rectangle rectName = new Rectangle(33, 3, 114, 15);
			Rectangle rectText = new Rectangle(32, 21, 366, 60);

			string str = rawString.Replace(_pairs["<player>"], "Player");
			double sDefault = 1.06;
			double sName = 0.86;
			double sCurrent = sDefault;
			int x = rectText.X, pX = x;
			int y = rectText.Y, pY = y;
			int yAdjust = 3;
			Color cDefault = Color.FromArgb(255, 37, 66, 167);
			Color cCurrent = cDefault;

			for (int i = 0; i < str.Length; i++)
			{
				bool notEOS = i + 2 < str.Length;
				char c = str[i];

				char c2 = ' ';
				if (notEOS)
					c2 = str[i + 1];

				BitFont bf = GetCharacter(c);

				// Handle control codes
				if (c == 0x001F && c2 == 0x0002) // Start Name
				{
					cCurrent = Color.White;
					sCurrent = sName;
					int width = MeasureString(str.Substring(i + 2), (char)0x0002, sCurrent);
					pX = x;
					pY = y;
					x = rectName.X + (rectName.Width / 2) - (width / 2);
					y = rectName.Y;
					i++;
					continue;
				}
				else if (c == 0x001F && (c2 == 0x0000 || c2 == 0x0100 || c2 == 0x0200 || c2 == 0x0103 || c2 == 0x0020 || c2 == 0x0115)) // Unknown/No Render Effect
				{
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0000) // Default
				{
					cCurrent = cDefault;
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0001) // Red
				{
					cCurrent = Color.Red;
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0003) // Light Blue
				{
					cCurrent = Color.FromArgb(255, 54, 129, 216);
					i++;
					continue;
				}
				else if (c == 0x001F && c2 == 0x0015) // End Dialog
				{
					i++;
					continue;
				}
				else if (c == 0x0017) // End End Dialog?
					continue;
				else if (c == 0x0002) // End Name
				{
					cCurrent = cDefault;
					sCurrent = sDefault;
					x = pX;
					y = pY;
					continue;
				}
				else if (c == '\n' || x + bf.Width - rectText.X > rectText.Width) // New Line/End of Textbox
				{
					x = rectText.X;
					y += bf.Character.Height + yAdjust;
					continue;
				}

				// Draw character
				gfx.DrawImage(PrepareCharacter(bf, cCurrent), x - bf.Offset, y, (int)(bf.Character.Width * sCurrent), (int)(bf.Character.Height * sCurrent));
				x += (int)(bf.Width * sCurrent);
			}

			return img;
		}

		private BitFont GetCharacter(char character)
		{
			if (_font.ContainsKey(character))
				return _font[character];
			else
				return _font['?'];
		}

		private int MeasureString(string text, char stopChar, double scale = 1.0)
		{
			int width = 0;

			foreach (char b in text)
			{
				BitFont bf = GetCharacter(b);
				if (b == stopChar || bf == null)
					break;
				width += bf.Width;
			}

			return (int)Math.Round(width * scale);
		}

		private Bitmap PrepareCharacter(BitFont bf, Color color)
		{
			Bitmap prepared = new Bitmap(bf.Character);
			for (int i = 0; i < prepared.Width * prepared.Height; i++)
			{
				int cx = i % prepared.Width;
				int cy = i / prepared.Height;

				Color c = prepared.GetPixel(cx, cy);

				prepared.SetPixel(cx, cy, Color.FromArgb((c.R - 255) * -1, color.R, color.G, color.B));
			}
			return prepared;
		}

		private void InitializeFont()
		{
			_font = new Dictionary<char, BitFont>();

			Bitmap img = new Bitmap(Resources.MainFont);
			Graphics gfx = Graphics.FromImage(img);

			int w = 21, h = 23;

			for (int i = (int)' '; i <= (int)'~'; i++)
			{
				int x = ((i - 32) % 16) * w;
				int y = ((i - 32) / 16) * h;
				int characterDimension = 17;
				int imageOffset = 18;
				int kerningOffset = 20;

				Bitmap character = new Bitmap(characterDimension, characterDimension, PixelFormat.Format32bppArgb);
				int offset = 0;
				int width = 0;

				// Read Kerning Data
				for (int j = x + 2; j <= x + imageOffset; j++)
				{
					Color result = img.GetPixel(j, y + kerningOffset);

					if (width == 0 && EqualARGB(result, Color.White))
						offset++;
					if (EqualARGB(result, Color.Red))
						width++;
				}

				// Copy Character Bitmap
				Graphics cgfx = Graphics.FromImage(character);
				cgfx.DrawImage(img, 0, 0, new Rectangle(x + 2, y + 2, x + imageOffset, y + imageOffset), GraphicsUnit.Pixel);

				_font.Add((char)i, new BitFont(character, offset, width));
			}
		}

		private bool EqualARGB(Color lhs, Color rhs)
		{
			return (lhs.R == rhs.R && lhs.G == rhs.G && lhs.B == rhs.B);
		}
	}

	public class BitFont
	{
		public Bitmap Character { get; private set; }
		public int Offset { get; private set; }
		public int Width { get; private set; }

		public BitFont(Bitmap character, int offset, int width)
		{
			Character = character;
			Offset = offset;
			Width = width;
		}
	}
}