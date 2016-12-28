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
      // Sample code for the VrJpeg library.
      // - The first example extracts the left eye and audio files to separate files.
      // - The second example creates a full equirectangular stereo panorama with eye images covering the correct long/lat and poles filled.

      string filename = @"Resources\CardboardCamera1.vr.jpg";
      string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

      // Read raw XMP metadata.
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);

      // Parse metadata into a dedicated class.
      GPanorama pano = new GPanorama(xmpDirectories.ToList());

      //-----------------------
      // Example 1.
      //-----------------------
      
      // Extract embedded image.
      // The primary image is actually the right eye, the left eye is embedded in the metadata.
      if (pano.ImageData != null)
      {
        string leftEyeFilename = string.Format("{0}_left.jpg", filenameWithoutExtension);
        string leftEyeFile = Path.Combine(Path.GetDirectoryName(filename), leftEyeFilename);

        File.WriteAllBytes(leftEyeFile, pano.ImageData);
      }

      // Extract embedded audio.
      if (pano.AudioData != null)
      {
        string audioFilename = string.Format("{0}_audio.mp4", filenameWithoutExtension);
        string audioFile = Path.Combine(Path.GetDirectoryName(filename), audioFilename);

        File.WriteAllBytes(audioFile, pano.AudioData);
      }

      //-----------------------
      // Example 2.
      //-----------------------
      Bitmap right = new Bitmap(filename);
      Bitmap left = VrJpegHelper.ExtractLeftEye(pano);

      int maxWidth = 8192;
      Bitmap rightEquir = VrJpegHelper.CreateEquirectangularImage(right, pano, maxWidth, true);
      Bitmap leftEquir = VrJpegHelper.CreateEquirectangularImage(left, pano, maxWidth, true);

      Bitmap composite = VrJpegHelper.Compose(leftEquir, rightEquir, EyeImageGeometry.OverUnder);

      string compositeFilename = string.Format("{0}_TB.jpg", filenameWithoutExtension);
      string compositeFile = Path.Combine(Path.GetDirectoryName(filename), compositeFilename);
      composite.Save(compositeFile);
    }
    
    /// <summary>
    /// Helper function to log the raw XMP metadata values.
    /// </summary>
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
