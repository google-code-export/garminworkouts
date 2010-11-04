using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GarminFitnessPlugin.View
{
    class FilledRoundedRectangle
    {
        public static void Draw(Graphics graphics, Color borderColor,
                                float borderWidth, Color backgroundColor,
                                RectangleF rectangle, float cornerRadius)
        {
            GraphicsPath rectanglePath = new GraphicsPath();
            Pen borderPen = new Pen(borderColor, borderWidth);
            SolidBrush backgroundBrush = new SolidBrush(backgroundColor);

            Draw(graphics, borderPen, backgroundBrush,
                 rectangle, cornerRadius);

            borderPen.Dispose();
            backgroundBrush.Dispose();
        }

        public static void Draw(Graphics graphics, Pen border, Brush background,
                                RectangleF rectangle, float cornerRadius)
        {
            Debug.Assert(rectangle.Height >= (2 * cornerRadius));
            GraphicsPath rectanglePath = new GraphicsPath();

            if (rectangle.Height > (2 * cornerRadius))
            {
                // Top left
                rectanglePath.AddArc(rectangle.Left, rectangle.Top,
                                     cornerRadius * 2, cornerRadius * 2,
                                     180, 90);
                // Top right
                rectanglePath.AddArc(rectangle.Right - (2 * cornerRadius), rectangle.Top,
                                     cornerRadius * 2, cornerRadius * 2,
                                     270, 90);
                // Bottom right
                rectanglePath.AddArc(rectangle.Right - (2 * cornerRadius), rectangle.Bottom - (2 * cornerRadius),
                                     cornerRadius * 2, cornerRadius * 2,
                                     0, 90);
                // Bottom left
                rectanglePath.AddArc(rectangle.Left, rectangle.Bottom - (2 * cornerRadius),
                                     cornerRadius * 2, cornerRadius * 2,
                                     90, 90);
            }
            else
            {
                // Left
                rectanglePath.AddArc(rectangle.Left, rectangle.Top,
                                     cornerRadius * 2, cornerRadius * 2,
                                     90, 180);
                // Right
                rectanglePath.AddArc(rectangle.Right - (2 * cornerRadius), rectangle.Top,
                                     cornerRadius * 2, cornerRadius * 2,
                                     270, 180);
            }
            rectanglePath.CloseFigure();

            graphics.FillPath(background, rectanglePath);
            graphics.DrawPath(border, rectanglePath);

            rectanglePath.Dispose();
        }
    }
}
