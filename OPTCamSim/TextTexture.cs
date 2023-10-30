using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;

namespace OPTCAMSim
{
    public class TextTexture
    {
        public string FontBitmapFilename = "test.png";

        public string Text;

        // Used to offset rendering glyphs to bitmap
        public int FontSize = 20;
        public bool BitmapFont = false;
        public string FromFile; //= "joystix monospace.ttf";
        public string FontName = "Arial";

        //Texture vars
        public int FontTextureID;
        public int TextureWidth;
        public int TextureHeight;

        // font image
        public Bitmap FontBitmap;
        public Color BackgroundColor = Color.Yellow;
        public Color TextColor = Color.Black;
        /// <summary>
        /// Store part texture coordinates
        /// </summary>
        public List<Vector4> PartsTex = new List<Vector4> { };

        public TextTexture(NestedPanels panel)
        {
            FontBitmap = GenerateFontImage(panel);
            LoadTexture();
        }

        Bitmap GenerateFontImage(NestedPanels panel)
        {
            Font font;
            if (!String.IsNullOrWhiteSpace(FromFile))
            {
                var collection = new PrivateFontCollection();
                collection.AddFontFile(FromFile);
                var fontFamily = new FontFamily(Path.GetFileNameWithoutExtension(FromFile), collection);
                font = new Font(fontFamily, FontSize);
            }
            else
            {
                font = new Font(new FontFamily(FontName), FontSize);
            }

            //Size s = TextRenderer.MeasureText(Text, font);
            //int bitmapWidth = s.Width;
            //int bitmapHeight = s.Height;
            int bitmapWidth = panel.PanelWidth;
            int bitmapHeight = panel.PanelHeight;

            Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);            

            using (var g = Graphics.FromImage(bitmap))
            {
                if (BitmapFont)
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                }
                else
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                }
                //fill background
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(new PointF[] { new Point(0, 0), new Point(bitmapWidth, 0), new Point(bitmapWidth, bitmapHeight), new Point(0, bitmapHeight) });

                    using (var brush = new SolidBrush(BackgroundColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
                foreach (NestedPartList p in panel.PartList)
                {
                    string text = p.partName + Environment.NewLine + p.partID;
                    Size s = TextRenderer.MeasureText(text, font);
                    float x = (float)(p.X + p.Width / 2 - s.Width / 2);
                    float ytex = (float)(p.Y + p.Height / 2 + s.Height / 2);
                    float y = bitmapHeight - ytex;

                    g.DrawString(text, font, new SolidBrush(TextColor), x, y);
                    PartsTex.Add(new Vector4(x, ytex, s.Width, s.Height));
                }
                               
            }
            //bitmap.Save(FontBitmapFilename);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
            
            //Process.Start(FontBitmapFilename);
            
        }

        void LoadTexture()
        {
            GL.GenTextures(1, out FontTextureID);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FontTextureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            BitmapData data = FontBitmap.LockBits(new Rectangle(0, 0, FontBitmap.Width, FontBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, FontBitmap.Width, FontBitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            FontBitmap.UnlockBits(data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            TextureWidth = FontBitmap.Width; TextureHeight = FontBitmap.Height;
        }

    }
}


