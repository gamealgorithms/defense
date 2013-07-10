//-----------------------------------------------------------------------------
// MarkupTextEngine from astroboid:
// http://astroboid.com/2011/06/markup-text-rendering-in-xna.html
//
// This is used to generate all the pretty color-coded text that wraps.
//
// Used under the Microsoft Permissive license.
// See LICENSE.txt for full details.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace defense.Utils
{
  internal struct FormatInstruction {
    public readonly Color Color;
    public readonly SpriteFont Font;

    public FormatInstruction(SpriteFont font, Color color) {
      Font = font;
      Color = color;
    }
  }

  public interface ICompiledElement {
    Vector2 Size { get; }
    Vector2 Position { get; set; }
    void Draw(SpriteBatch spriteBatch, Vector2 offset);
    void Start();
    void Stop();
  }

  internal struct CompiledTextElement : ICompiledElement {
    private readonly FormatInstruction formatInstruction;
    private readonly Vector2 size;
    private readonly string text;

    public CompiledTextElement(string text, Vector2 position, FormatInstruction formatInstruction)
      : this() {
      this.text = text;
      Position = position;
      this.formatInstruction = formatInstruction;
      size = formatInstruction.Font.MeasureString(text);
    }

    #region ICompiledElement Members
    public Vector2 Position { get; set; }

    public Vector2 Size {
      get { return size; }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offset) {
      var origin = new Vector2(0, Size.Y / 2f);
      spriteBatch.DrawString(formatInstruction.Font, text, offset + Position, formatInstruction.Color, 0, origin, Vector2.One, SpriteEffects.None, 0);
    }

    public void Start() {
    }

    public void Stop() {
    }
    #endregion
  }


  internal struct CompiledImageElement : ICompiledElement {
    private readonly Color color;
    private readonly Texture2D image;
    private readonly Vector2 scale;
    private readonly Vector2 size;

    public CompiledImageElement(Texture2D image, Color color, Vector2 position, Vector2 scale)
      : this() {
      this.image = image;
      Position = position;
      this.color = color;
      this.scale = scale;
      size = new Vector2(image.Width, image.Height);
    }

    #region ICompiledElement Members
    public Vector2 Position { get; set; }

    public Vector2 Size {
      get { return size * scale; }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offset) {
      var origin = new Vector2(0, size.Y / 2f);
      spriteBatch.Draw(image, offset + Position, null, color, 0, origin, scale, SpriteEffects.None, 0);
    }

    public void Start() {
    }

    public void Stop() {
    }
    #endregion
  }

  public class CompiledMarkup : List<ICompiledElement> {
    public Vector2 Size { get; internal set; }
    public string Text { get; internal set; }

    public void Draw(SpriteBatch spriteBatch, Vector2 position) {
      for (int i = 0; i < Count; i++) {
        this[i].Draw(spriteBatch, position);
      }
    }

    public void Start() {
      for (int i = 0; i < Count; i++) {
        this[i].Start();
      }
    }

    public void Stop() {
      for (int i = 0; i < Count; i++) {
        this[i].Stop();
      }
    }
  }

  public class MarkupTextEngine {
    private readonly Func<string, bool> ConditionalResolver;
    private readonly Func<string, SpriteFont> FontResolver;
    private readonly Func<string, Texture2D> ImageResolver;

    public MarkupTextEngine(Func<string, SpriteFont> fontResolver, Func<string, Texture2D> imageResolver, Func<string, bool> conditionalResolver) {
      FontResolver = fontResolver;
      ImageResolver = imageResolver;
      ConditionalResolver = conditionalResolver;
    }

    public void Compile(string text, float width, CompiledMarkup compiledMarkup) {
      XmlReader reader = XmlReader.Create(new StringReader(text));
      Vector2 position = Vector2.Zero;
      var formatingStack = new Stack<FormatInstruction>();
      var conditionalsStack = new Stack<bool>();
      var lineBuffer = new List<ICompiledElement>();
      float currentLineHeight;
      float currentTotalHeight = 0;
      float maxLineWidth = float.MinValue;
      float currentLineWidth = 0;

      while (reader.Read()) {
        switch (reader.NodeType) {
          case XmlNodeType.Element:
            switch (reader.Name) {
              case "font": {
                SpriteFont font;
                string s = reader.GetAttribute("face");
                if (!string.IsNullOrEmpty(s)) {
                  font = FontResolver.Invoke(s);
                } else if (formatingStack.Count > 0) {
                  font = formatingStack.Peek().Font;
                } else {
                  throw new InvalidOperationException("Need a font.");
                }

                Color color;
                s = reader.GetAttribute("color");
                if (!string.IsNullOrEmpty(s)) {
                  color = ToColor(s);
                } else if (formatingStack.Count > 0) {
                  color = formatingStack.Peek().Color;
                } else {
                  throw new InvalidOperationException("Need a color.");
                }
                formatingStack.Push(new FormatInstruction(font, color));
              }
                break;
              case "if": {
                string clause = reader.GetAttribute("clause");
                bool condition = ConditionalResolver.Invoke(clause);
                conditionalsStack.Push(condition);
              }
                break;
              case "br": {
                if (lineBuffer.Count > 0) {
                  position = WrapLine(position, lineBuffer, out currentLineHeight, compiledMarkup);
                  currentTotalHeight += currentLineHeight;
                } else {
                  position.Y += formatingStack.Peek().Font.LineSpacing;
                  currentTotalHeight += formatingStack.Peek().Font.LineSpacing;
                }
                maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                currentLineWidth = 0;
              }
                break;
              case "img": {
                if (conditionalsStack.Count != 0 && !conditionalsStack.Peek()) {
                  break;
                }
                string imgSrc = reader.GetAttribute("src");
                Color color = Color.White;
                string s = reader.GetAttribute("color");
                if (!string.IsNullOrEmpty(s)) {
                  color = ToColor(s);
                }
                Texture2D image = ImageResolver.Invoke(imgSrc);
                if (position.X + image.Width > width) {
                  position = WrapLine(position, lineBuffer, out currentLineHeight, compiledMarkup);
                  currentTotalHeight += currentLineHeight;
                }
                Vector2 scale = Vector2.One;
                s = reader.GetAttribute("scale");
                if (!string.IsNullOrEmpty(s)) {
                  scale = ToVector2(s);
                }
                lineBuffer.Add(new CompiledImageElement(image, color, position, scale));
                position.X += image.Width * scale.X;
                currentLineWidth += image.Width * scale.Y;
                break;
              }
              case "nbsp": {
                FormatInstruction currentFormatting = formatingStack.Peek();
                float spaceX = currentFormatting.Font.MeasureString(" ").X;
                if (position.X + spaceX < width) {
                  position.X += spaceX;
                  currentLineWidth += spaceX;
                }
                break;
              }
            }
            break;
          case XmlNodeType.Text: {
            if (conditionalsStack.Count != 0 && !conditionalsStack.Peek()) {
              break;
            }
            FormatInstruction currentFormatting = formatingStack.Peek();
            string str = reader.Value;
            var re = new Regex(@"\s+");
            string[] words = re.Split(str);
            float spaceX = currentFormatting.Font.MeasureString(" ").X;
            for (int i = 0; i < words.Length; i++) {
              string word = words[i];
              Vector2 wordSz = currentFormatting.Font.MeasureString(word);
              if (position.X + wordSz.X > width) {
                position = WrapLine(position, lineBuffer, out currentLineHeight, compiledMarkup);
                currentTotalHeight += currentLineHeight;
                maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                currentLineWidth = 0;
              }

              lineBuffer.Add(new CompiledTextElement(word, position, currentFormatting));
              position.X += wordSz.X;
              currentLineWidth += wordSz.X;
              if (i < words.Length - 1) {
                position.X += spaceX;
                currentLineWidth += spaceX;
              }
            }
            break;
          }
          case XmlNodeType.EndElement: {
            switch (reader.Name) {
              case "font":
                formatingStack.Pop();
                break;
              case "if":
                conditionalsStack.Pop();
                break;
            }
          }
            break;
        }
      }
      if (lineBuffer.Count > 0) {
        WrapLine(position, lineBuffer, out currentLineHeight, compiledMarkup);
        for (int i = 0; i < lineBuffer.Count; i++) {
          ICompiledElement element = lineBuffer[i];
          element.Position = new Vector2(element.Position.X, position.Y + currentLineHeight / 2f);
        }
        currentTotalHeight += currentLineHeight;
        maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
        compiledMarkup.AddRange(lineBuffer);
        lineBuffer.Clear();
      }

      compiledMarkup.Size = new Vector2(maxLineWidth, currentTotalHeight);
      compiledMarkup.Text = text;
    }

    private Vector2 WrapLine(Vector2 position, List<ICompiledElement> lineBuffer, out float currentLineHeight, CompiledMarkup compiledMarkup) {
      currentLineHeight = 0;
      for (int i = 0; i < lineBuffer.Count; i++) {
        currentLineHeight = Math.Max(currentLineHeight, lineBuffer[i].Size.Y);
      }
      for (int i = 0; i < lineBuffer.Count; i++) {
        lineBuffer[i].Position = new Vector2(lineBuffer[i].Position.X, position.Y + currentLineHeight / 2f);
      }
      compiledMarkup.AddRange(lineBuffer);
      lineBuffer.Clear();
      position.X = 0;
      position.Y += currentLineHeight;
      return position;
    }

    public CompiledMarkup Compile(string text, float width) {
      var compiledMarkupText = new CompiledMarkup();
      Compile(text, width, compiledMarkupText);
      return compiledMarkupText;
    }

    private static Color ToColor(string hexString) {
      if (hexString.StartsWith("#"))
        hexString = hexString.Substring(1);
      uint hex = uint.Parse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      Color color = Color.White;
      if (hexString.Length == 8) {
        color.A = (byte) (hex >> 24);
        color.R = (byte) (hex >> 16);
        color.G = (byte) (hex >> 8);
        color.B = (byte) (hex);
      } else if (hexString.Length == 6) {
        color.R = (byte) (hex >> 16);
        color.G = (byte) (hex >> 8);
        color.B = (byte) (hex);
      } else {
        throw new InvalidOperationException();
      }
      return color;
    }

    private static Vector2 ToVector2(String vectorString) {
      string[] split = Regex.Split(vectorString, @"[\\s,]+");
      return new Vector2(float.Parse(split[0], CultureInfo.InvariantCulture), float.Parse(split[1], CultureInfo.InvariantCulture));
    }
  }
}