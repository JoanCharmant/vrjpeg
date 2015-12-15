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
  public static class VrJpegLoader
  {
    /// <summary>
    /// Load a .vr.jpg image and produces a single Bitmap with the whole stereo-panorama painted on.
    /// The resulting image cover the full sphere for both eyes. Nadir and zenith holes will be black.
    /// </summary>
    public static Bitmap Load(string filename, EyeImageGeometry geometry)
    {
      Bitmap left = new Bitmap(filename);
      
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);
      
      if (xmpDirectories == null || xmpDirectories.Count != 2)
        return left;

      GPanorama pano = new GPanorama(xmpDirectories.ToList(), false);
      if (pano.ImageData == null)
        return left;

      Bitmap right = null;
      using (var stream = new MemoryStream(pano.ImageData))
        right = new Bitmap(stream);

      if (right == null)
        return left;

      Bitmap result;
      if (geometry == EyeImageGeometry.LeftRight)
      {
        result = new Bitmap(pano.PanoFullPanoWidthPixels * 2, pano.PanoFullPanoHeightPixels, left.PixelFormat);
        Graphics g = Graphics.FromImage(result);
        g.DrawImageUnscaled(left, new Point(pano.PanoCroppedAreaLeftPixels, pano.PanoCroppedAreaTopPixels));
        g.DrawImageUnscaled(right, new Point(pano.PanoFullPanoWidthPixels + pano.PanoCroppedAreaLeftPixels, pano.PanoCroppedAreaTopPixels));
        g.Dispose();
      }
      else
      {
        result = new Bitmap(pano.PanoFullPanoWidthPixels, pano.PanoFullPanoHeightPixels * 2, left.PixelFormat);
        Graphics g = Graphics.FromImage(result);
        g.DrawImageUnscaled(left, new Point(pano.PanoCroppedAreaLeftPixels, pano.PanoCroppedAreaTopPixels));
        g.DrawImageUnscaled(right, new Point(pano.PanoCroppedAreaLeftPixels, pano.PanoFullPanoHeightPixels + pano.PanoCroppedAreaTopPixels));
        g.Dispose();
      }

      return result;
    }
  }
}
