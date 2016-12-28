using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrJpeg
{
  public static class PoleFiller
  {
    /// <summary>
    /// Fills the nadir and zenith caps by averaging the top/bottom row.
    /// The filling is applied in-place over the provided image.
    /// 
    /// Pole filling is done by averaging a number of neighboring pixels from the nearest valid row (either the topmost or bottommost captured row).
    /// The number of neighbors used in the average is a non-linear mapping over the latitude of the target pixel.
    /// Based on the idea described by Leo Sutic at https://monochrome.sutic.nu/2012/11/04/filling-in-the-blanks.html
    /// </summary>
    public static unsafe void Fill(Bitmap bmp, Rectangle crop)
    {
      if (bmp == null)
        return;

      BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
      
      try
      {
        int width = bmpData.Width;
        int height = bmpData.Height;
        int pixelSize = 3;
        int offset = bmpData.Stride - width * pixelSize;
        int cropBottom = crop.Top + crop.Height - 1;

        byte* p0 = (byte*)bmpData.Scan0.ToPointer();
        
        int start = -1;
        int end = -1;
        int span = -1;
        int oldStart = -1;

        // Process all rows where data is missing.
        for (int i = 0; i < height; i++)
        {
          int n = GetNeighborhood(i, crop, width, height);
          if (n <= 0)
            continue;

          // Ensure neighborhood is always odd to simplify computations.
          if (n % 2 == 0)
            n--;

          span = (n - 1) / 2;

          // Reusable accumulators for the average.
          int b = 0;
          int g = 0;
          int r = 0;

          int refRow = i < crop.Top ? crop.Top : cropBottom;
          byte* pRefRow = (byte*)bmpData.Scan0.ToPointer() + (bmpData.Stride * refRow);
          
          // Process each pixel of the row.
          byte* pDst = p0 + i * bmpData.Stride;

          for (int j = 0; j < width; j++)
          {
            // To speed up the computation of the average we compute the sum only once, 
            // and then just remove and add pixels at both ends of the segment.

            start = j - span;
            end = j + span;

            if (j == 0)
            {
              // Initialize the accumulators.
              for (int k = start; k <= end; k++)
              {
                // Remap coordinate for line wrapping.
                int pixel = k < 0 ? bmpData.Width + k : k;
                byte* pPixel = pRefRow + pixel * pixelSize;

                b += pPixel[0];
                g += pPixel[1];
                r += pPixel[2];
              }
            }
            else
            {
              // Update the accumulators.

              // Remove the old value at the start. It's always a single pixel shift.
              int pixel = oldStart < 0 ? bmpData.Width + oldStart : oldStart;
              byte* pPixel = pRefRow + pixel * pixelSize;
              b -= pPixel[0];
              g -= pPixel[1];
              r -= pPixel[2];
              
              // Add the new value at the end.
              pixel = end < 0 ? bmpData.Width + end : end;
              pPixel = pRefRow + pixel * pixelSize;
              b += pPixel[0];
              g += pPixel[1];
              r += pPixel[2];
            }
            
            oldStart = start;

            // Compute averages and assign final value.
            pDst[0] = (byte)Math.Min((float)b / n, 255.0f);
            pDst[1] = (byte)Math.Min((float)g / n, 255.0f);
            pDst[2] = (byte)Math.Min((float)r / n, 255.0f); 

            pDst += pixelSize;
          }
        }
      }
      finally
      {
        bmp.UnlockBits(bmpData);
      }
    }

    /// <summary>
    /// Computes the number of pixels that we need to include in the average.
    /// This number is valid for every pixels of the row.
    /// This is dependent on the distance to the nearest valid row (aka, latitude).
    /// </summary>
    private static int GetNeighborhood(int row, Rectangle crop, int width, int height)
    {
      int cropBottom = crop.Top + crop.Height - 1;

      if (row < crop.Top)
      {
        double ratio = (double)(crop.Top - row) / crop.Top;
        double n = width * Map(ratio);
        return (int)Math.Max(1, n);
      }
      else if (row > cropBottom)
      {
        double ratio = (double)(row - cropBottom) / (height - 1 - cropBottom);
        double n = width * Map(ratio);
        return (int)Math.Max(1, n);
      }
      else
      {
        return 0;
      }
    }

    /// <summary>
    /// Remap the normalized distance between the nearest valid row and the pole.
    /// Various functions can be used here to change the falloff effect.
    /// </summary>
    private static double Map(double v)
    {
      //return v <= 0.5 ? v / 2 : v * v; // bigshot.
      //return 1.0 - Math.Pow(Math.Cos(v), 0.5);
      return 1.0 - Math.Cos(v);
    }
  }
}
