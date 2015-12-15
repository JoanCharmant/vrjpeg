using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VrJpeg;
using MetadataExtractor.Formats.Xmp;

namespace Sample
{
  public class Program
  {
    public static void Main(string[] args)
    {
      string filename = @"D:\vr\images\Google\Orange County - 2015-12-13\IMG_20151213_163858.vr.jpg";

      // High level interface: creates a Bitmap object of the full stereo-sphere with both eyes painted on.
      Bitmap stereoPano = VrJpegLoader.Load(filename, EyeImageGeometry.OverUnder);
      
      string outputFile = string.Format("{0}-stereo.jpg", Path.GetFileNameWithoutExtension(filename));
      stereoPano.Save(Path.Combine(Path.GetDirectoryName(filename), outputFile));

      //----
      // Low level interface: to retrieve image and audio Base64 data directly or as byte arrays.
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);
      DumpProperties(xmpDirectories);

      GPanorama pano = new GPanorama(xmpDirectories.ToList(), true);
      byte[] jpegBuffer = pano.ImageData;

    }

    private static void DumpProperties(IEnumerable<XmpDirectory> directories)
    {
      foreach (var dir in directories)
      {
        foreach (var prop in dir.XmpMeta.Properties)
        {
          if (prop.Path != null && prop.Path.EndsWith(":Data"))
            Debug.WriteLine("{0} - {1}", prop.Namespace, prop.Path);
          else
            Debug.WriteLine("{0} - {1}:{2}", prop.Namespace, prop.Path, prop.Value);
        }
      }
    }
  }
}
