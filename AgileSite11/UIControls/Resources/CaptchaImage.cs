using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CMS.UIControls
{
    /// <summary>
    /// Summary description for CaptchaImage.
    /// </summary>
    public class CaptchaImage
    {
        #region "Variables"

        private string text;
        private int width;
        private int height;
        private string familyName;
        private Bitmap image;
        private bool mUseWarp = true;

        // For generating random numbers.
        private Random random = new Random();

        #endregion


        #region "Properties"

        /// <summary>
        /// Uses warp.
        /// </summary>
        public bool UseWarp
        {
            get
            {
                return mUseWarp;
            }
        }


        /// <summary>
        /// Public properties (all read-only).
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
        }


        /// <summary>
        /// Image.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                return image;
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the CaptchaImage class using the specified text, width and height.
        /// </summary>
        /// <param name="s">Text</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public CaptchaImage(string s, int width, int height)
        {
            text = s;
            SetDimensions(width, height);
            GenerateImage();
        }


        /// <summary>
        /// Initializes a new instance of the CaptchaImage class using the specified text, width, height and font family.
        /// </summary>
        /// <param name="s">Text</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="familyName">Family name</param>
        public CaptchaImage(string s, int width, int height, string familyName)
            : this(s, width, height, familyName, true)
        {
        }


        /// <summary>
        /// Initializes a new instance of the CaptchaImage class using the specified text, width, height and font family.
        /// </summary>
        /// <param name="s">Text</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="familyName">Family name</param>
        /// <param name="useWarp">Indicates if shlould be used warp</param>
        public CaptchaImage(string s, int width, int height, string familyName, bool useWarp)
        {
            mUseWarp = useWarp;
            text = s;
            SetDimensions(width, height);
            SetFamilyName(familyName);
            GenerateImage();
        }

        #endregion


        #region "Destructor"

        /// <summary>
        /// This member overrides Object.Finalize.
        /// </summary>
        ~CaptchaImage()
        {
            Dispose(false);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }


        /// <summary>
        /// Custom Dispose method to clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates if disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of the bitmap.
                image.Dispose();
            }
        }


        /// <summary>
        /// Sets the image width and height.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        private void SetDimensions(int width, int height)
        {
            // Check the width and height.
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "Argument out of range, must be greater than zero.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", height, "Argument out of range, must be greater than zero.");
            }

            this.width = width;
            this.height = height;
        }


        /// <summary>
        /// Sets the font used for the image text.
        /// </summary>
        /// <param name="familyName">Family name</param>
        private void SetFamilyName(string familyName)
        {
            // If the named font is not installed, default to a system font.
            try
            {
                Font font = new Font(this.familyName, 50F);
                this.familyName = familyName;
                font.Dispose();
            }
            catch
            {
                this.familyName = FontFamily.GenericSerif.Name;
            }
        }


        /// <summary>
        /// Creates the bitmap image.
        /// </summary>
        private void GenerateImage()
        {
            // Create a new 32-bit bitmap image.
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Create a graphics object for drawing.
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, width, height);

            // Set up the text font.
            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;

            // Fill in the background.
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.ZigZag, Color.LightGray, Color.White);
            g.FillRectangle(hatchBrush, rect);
            //hatchBrush = new HatchBrush(HatchStyle.LightHorizontal, Color.LightPink, Color.Transparent);
            //g.FillRectangle(hatchBrush, rect);

            // Set up the text format.
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Near;

            // Adjust the font size until the text fits within the image.
            do
            {
                fontSize--;
                font = new Font(FontFamily.GenericSansSerif.Name, fontSize, FontStyle.Bold);
                size = g.MeasureString(text, font, new PointF(0, 0), format);
            } while (size.Width > (rect.Width + 20));

            // Create a path using the text and warp it randomly.
            GraphicsPath path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(0, 0), format);

            if (UseWarp)
            {
                float v = 4F;
                PointF[] points =
                    {
                        new PointF(random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                        new PointF(rect.Width - random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                        new PointF(random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v),
                        new PointF(rect.Width - random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v)
                    };
                Matrix matrix = new Matrix();
                matrix.Translate(0F, 0F);
                path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
            }

            // Draw the text.
            hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.Black, Color.Black);
            g.FillPath(hatchBrush, path);

            // Add some random noise.
            int m = Math.Max(rect.Width, rect.Height);
            int noiseWidth = Math.Min(m / 50, 6);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 20F); i++)
            {
                int x = random.Next(rect.Width);
                int y = random.Next(rect.Height);
                int w = random.Next(noiseWidth);
                int h = random.Next(noiseWidth);
                g.FillEllipse(hatchBrush, x, y, w, h);
            }

            // Clean up.
            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();

            // Set the image.
            image = bitmap;
        }

        #endregion
    }
}