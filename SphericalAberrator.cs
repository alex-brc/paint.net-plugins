// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl q = 3; // [1,4] Upscaling factor
DoubleSliderControl k1 = 0.5; // [-1,1] Distortion factor
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.SelectionBounds;
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

    // Work on upscaled rectangle
    int xc = CenterX * q;
    int yc = CenterY * q;
    int sw = src.Width * q;
    int sh = src.Height * q;
    int xp, yp, xd, yd;
    ColorBgra CurrentPixel;
    double rSquared, seriesCoefficient;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {   
            for(int j = 0; j < q; j++)
                for(int i = 0; i < q; i++){
                    xp = x * q + i; 
                    yp = y * q + j;

                    // Compute distortion coefficient to this point
                    rSquared = distance(xp, yp, xc, yc);
                    rSquared *= rSquared;
                    seriesCoefficient = k1 * rSquared;

                    // Compute new position after distortion 
                    xd = (int)Math.Floor((double)xp + (xp - xc) * seriesCoefficient);
                    yd = (int)Math.Floor((double)yp + (yp - yc) * seriesCoefficient);

                    // Put it on the surface if it fits
                    if(xd > 0 && xd < sw &&
                       yd > 0 && yd < sh)
                        superDst[xd, yd] = superSrc[xp, yp];
                }
            
        }
    }

    // Downscale the result from superDst into dst
    List<ColorBgra> pixels;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {       
            pixels = new List<ColorBgra>();
            for(int j = 0; j < q; j++)
                for(int i = 0; i < q; i++){
                    xp = x * q + i; 
                    yp = y * q + j;
                    pixels.Add(superDst[xp, yp]);
                }
            dst[x,y] = ColorBgra.Blend(pixels.ToArray());
        }
    }
}

private static double distance(double x1, double y1, double x2, double y2){
    return Math.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
}

            // CurrentPixel = src[x,y];
            // TODO: Add pixel processing code here
            // Access RGBA values this way, for example:
            // CurrentPixel.R = PrimaryColor.R;
            // CurrentPixel.G = PrimaryColor.G;
            // CurrentPixel.B = PrimaryColor.B;
            // CurrentPixel.A = PrimaryColor.A;
            // dst[x,y] = CurrentPixel;
