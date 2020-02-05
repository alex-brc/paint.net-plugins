// Name: Radial Distortion
// Submenu: Distort
// Author: brc.xyz
// Title: Radial Distortion
// Version: 0.23
// Desc: Radially distort the selection. 
// Keywords: radial|distortion|radius|distort|barrel|pincushion
// URL: https://brc.xyz/
// Help:
#region UICode
DoubleSliderControl k1 = 3; // [-10,10] Quadratic distortion factor
CheckboxControl bilinear = true; // Anti-aliasing
#endregion

ColorBgra zeroPixel;
void PreRender(Surface dst, Surface src){
    k1 *= -0.1; 
    // Adjust for image size
    double scaler = Math.Max(src.Width, src.Height);
    scaler *= scaler;
    k1 /= scaler;

    zeroPixel.B = zeroPixel.G = 
    zeroPixel.R = zeroPixel.A = 0;
}


void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.SelectionBounds;
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

    double r2, r4, c;
    double xd, yd, xs, ys, xi, yi;
    int xdInt, ydInt;
    ColorBgra[] iPixels = new ColorBgra[4];
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {   
            // 0-centered coordinates
            xs = x - CenterX;
            ys = y - CenterY;

            // Distance from center for this point;
            r2 = xs*xs + ys*ys; // r square
            // Find source pixel position to send to dst[x,y]
            c = 1 + k1 * r2;
            xd = xs * c;
            yd = ys * c;
            
            // Put it on the surface if it fits
            xdInt = (int)Math.Floor(xd) + CenterX;
            ydInt = (int)Math.Floor(yd) + CenterY;
            if(xdInt > 1 && xdInt < src.Width - 1 && ydInt > 1 && ydInt < src.Height - 1){
                if(bilinear){
                // Find interpolation values
                xi = xd - Math.Floor(xd);
                yi = yd - Math.Floor(yd);

                // Find interpolation pixels
                iPixels[0] = src[xdInt,                 ydInt];
                iPixels[1] = src[xdInt + Math.Sign(xi), ydInt];
                iPixels[2] = src[xdInt,                 ydInt + Math.Sign(yi)];
                iPixels[3] = src[xdInt + Math.Sign(xi), ydInt + Math.Sign(yi)];
                dst[x,y] = bilinearInterpolate(iPixels, (float)xi, (float)yi);
                }
                else 
                    dst[x,y] = src[xdInt, ydInt];
            }
            else
                dst[x,y] = zeroPixel;
        }
    }
}

private static ColorBgra bilinearInterpolate(ColorBgra[] pixels, float xi, float yi){
    ColorBgra t1 = ColorBgra.Lerp(pixels[0], pixels[1], xi);
    ColorBgra t2 = ColorBgra.Lerp(pixels[2], pixels[3], xi);

    return ColorBgra.Lerp(t1, t2, yi);
}