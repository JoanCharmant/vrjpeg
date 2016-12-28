#region License
// 
// 2015 Joan Charmant
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrJpeg
{
  /// <summary>
  /// A collection of higher level functions to extract and manipulate images from the .vr.jpg file.
  /// </summary>
  public static class VrJpegHelper
  {
    /// <summary>
    /// Extracts the embedded image from the panorama and returns it as a Bitmap.
    /// Note: the right eye can be extracted simply by loading the original file into a regular Bitmap.
    /// </summary>
    public static Bitmap ExtractLeftEye(GPanorama pano)
    {
      if (pano.ImageData == null)
        return null;

      Bitmap bmp = null;
      using (var stream = new MemoryStream(pano.ImageData))
        bmp = new Bitmap(stream);

      return bmp;
    }
    
    /// <summary>
    /// Takes a single eye Bitmap and paints it at the correct latitude-longitude within a fully spherical equirectangular image.
    /// Optionally fills in the poles.
    /// </summary>
    public static Bitmap CreateEquirectangularImage(Bitmap bitmap, GPanorama pano, int maxWidth, bool fillPoles)
    {
      if (bitmap == null)
        return null;

      Rectangle crop = new Rectangle(pano.PanoCroppedAreaLeftPixels, pano.PanoCroppedAreaTopPixels, pano.PanoCroppedAreaImageWidthPixels, pano.PanoCroppedAreaImageHeightPixels);

      int imgWidth = pano.PanoFullPanoWidthPixels;
      int imgHeight = pano.PanoFullPanoHeightPixels;
      
      if (maxWidth > 0 && imgWidth > maxWidth)
      {
        float ratio = (float)maxWidth / imgWidth;

        imgWidth = maxWidth;
        imgHeight = (int)(imgHeight * ratio);

        crop = new Rectangle((int)(crop.Left * ratio), (int)(crop.Top * ratio), (int)(crop.Width * ratio), (int)(crop.Height * ratio));
      }

      Bitmap result = new Bitmap(imgWidth, imgHeight, bitmap.PixelFormat);
      Graphics g = Graphics.FromImage(result);
      g.DrawImage(bitmap, crop);
      g.Dispose();
      
      if (fillPoles)
        PoleFiller.Fill(result, crop);

      return result;
    }
    
    /// <summary>
    /// Combines two equirectangular images into a single stereo image, either left/right or top/bottom.
    /// </summary>
    public static Bitmap Compose(Bitmap left, Bitmap right, EyeImageGeometry geometry)
    {
      if (left == null || right == null || left.Size != right.Size || left.PixelFormat != right.PixelFormat)
        return null;

      Size imgSize = geometry == EyeImageGeometry.LeftRight ? new Size(left.Width * 2, left.Height) : new Size(left.Width, left.Height * 2);
      Point rightLocation = geometry == EyeImageGeometry.LeftRight ? new Point(left.Width, 0) : new Point(0, left.Height);
      
      Bitmap result = new Bitmap(imgSize.Width, imgSize.Height, left.PixelFormat);
      using (Graphics g = Graphics.FromImage(result))
      {
        g.DrawImageUnscaled(left, Point.Empty);
        g.DrawImageUnscaled(right, rightLocation);
      }
      
      return result;
    }
  }
}
