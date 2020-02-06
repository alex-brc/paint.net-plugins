// Name: Spherical Aberration
// Submenu: Distort
// Author: brc.xyz
// Title: Spherical Aberration
// Version: 0.1
// Desc: Immitate an effect produced by cheap/vintage lenses
// Keywords: sphere|spherical|aberration|distorsion
// URL: https://brc.xyz/
// Help: 
#region UICode
DoubleSliderControl rf = 0.54; // [0,1] Red shift factor
DoubleSliderControl gf = 0.56; // [0,1] Green shift factor
DoubleSliderControl bf = 0.58; // [0,1] Blue shift factor
#endregion

private const int RED = 0, BLUE = 1, GREEN = 2; 

void PreRender(Surface dst, Surface src){
    // Adjust for image size
    double scaler = Math.Max(src.Width, src.Height);
    scaler *= scaler;
    rf /= -scaler;
    gf /= -scaler;
    bf /= -scaler;
}


void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.SelectionBounds;
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

    double r2;
    int xs, ys, xdInt, ydInt;
    ColorBgra output = new ColorBgra();
    ColorBgra temp;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {   
            // 0-centered coordinates
            xs = x - CenterX;
            ys = y - CenterY;
            r2 = xs*xs + ys*ys; // r square
            temp = distortColour(src, dst, BLUE, x, y, r2, xs, ys, CenterX, CenterY);
            output.R = distortColour(src, dst, RED, x, y, r2, xs, ys, CenterX, CenterY).R;
            output.G = distortColour(src, dst, GREEN, x, y, r2, xs, ys, CenterX, CenterY).G;
            output.B = temp.B;
            output.A = temp.A;
            dst[x,y] = output;

        }
    }
}

private static ColorBgra bilinearInterpolate(ColorBgra[] pixels, float xi, float yi){
    ColorBgra t1 = ColorBgra.Lerp(pixels[0], pixels[1], xi);
    ColorBgra t2 = ColorBgra.Lerp(pixels[2], pixels[3], xi);

    return ColorBgra.Lerp(t1, t2, yi);
}

private ColorBgra distortColour(Surface src, Surface dst, int COLOR, 
                    int x, int y, double r2, int xs, int ys, int xc, int yc){
    // Distance from center for this point;
    // Find source pixel position to send to dst[x,y]
    double k;
    switch(COLOR){
        case RED: k = rf;
            break;
        case GREEN: k = gf;
            break;
        case BLUE: k = bf;
            break;
        default: k = 0;
            break;
    }

    double c = 1 + k * r2;
    double xd = xs * c;
    double yd = ys * c;
    
    // Check if [xd,yd] is on the source
    int xdInt = (int)Math.Floor(xd) + xc;
    int ydInt = (int)Math.Floor(yd) + yc;
    double xi, yi;
    ColorBgra[] iPixels = new ColorBgra[4];
    ColorBgra result = new ColorBgra();
    if(xdInt > 1 && xdInt < src.Width - 1 && ydInt > 1 && ydInt < src.Height - 1){
        // Find interpolation values
        xi = xd - Math.Floor(xd);
        yi = yd - Math.Floor(yd);

        // Find interpolation pixels
        iPixels[0] = src[xdInt,                 ydInt];
        iPixels[1] = src[xdInt + Math.Sign(xi), ydInt];
        iPixels[2] = src[xdInt,                 ydInt + Math.Sign(yi)];
        iPixels[3] = src[xdInt + Math.Sign(xi), ydInt + Math.Sign(yi)];
        result = bilinearInterpolate(iPixels, (float)xi, (float)yi);
    }
    return result;
}
