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
      // The first example extracts the right eye image and audio file to separate files.
      // The second example creates a full equirectangular stereo panorama with eye images covering the correct long/lat.

      string filename = @"Resources\CardboardCamera1.vr.jpg";
      string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
      
      //-----------------------
      // Example 1.
      //-----------------------

      // Read raw XMP metadata.
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);

      // Parse metadata into a dedicated class.
      GPanorama pano = new GPanorama(xmpDirectories.ToList());
      // Extract embedded data.
      if (pano.ImageData != null)
      {
        string rightEyeFilename = string.Format("{0}-right.jpg", filenameWithoutExtension);
        string rightEyeFile = Path.Combine(Path.GetDirectoryName(filename), rightEyeFilename);

        File.WriteAllBytes(rightEyeFile, pano.ImageData);
      }

      if (pano.AudioData != null)
      {
        string audioFilename = string.Format("{0}-audio.mp4", filenameWithoutExtension);
        string audioFile = Path.Combine(Path.GetDirectoryName(filename), audioFilename);

        File.WriteAllBytes(audioFile, pano.AudioData);
      }

      //-----------------------
      // Example 2.
      //-----------------------
      
      Bitmap stereoPano = VrJpegLoader.Load(filename, EyeImageGeometry.OverUnder);

      string stereoPanoFilename = string.Format("{0}-stereo.jpg", filenameWithoutExtension);
      string stereoPanoFile = Path.Combine(Path.GetDirectoryName(filename), stereoPanoFilename);

      stereoPano.Save(stereoPanoFile);
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
