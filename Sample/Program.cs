#region License
// 
// 2016 Joan Charmant
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
    /// <summary>
    /// Sample code for the VrJpeg library.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
      string filename = @"Resources\CardboardCamera1.vr.jpg";
      
      // The examples are sorted from lower abstraction level to higher abstraction level.
      Example1(filename);
      Example2(filename);
      Example3(filename);
    }

    /// <summary>
    /// Extracts the left eye and audio files to separate files.
    /// This example directly manipulates the embedded content as bytes.
    /// </summary>
    private static void Example1(string filename)
    {
      string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
    
      // Read raw XMP metadata.
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);

      // Parse metadata into a dedicated class.
      GPanorama pano = new GPanorama(xmpDirectories.ToList());

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
    }
     
    /// <summary>
    /// Extract the left eye panorama and creates a full equirectangular image from it.
    /// The resulting image is suitable for viewing in a spherical viewer.
    /// </summary>
    private static void Example2(string filename)
    {
      // Read raw XMP metadata.
      var xmpDirectories = VrJpegMetadataReader.ReadMetadata(filename);

      // Parse metadata into a dedicated class.
      GPanorama pano = new GPanorama(xmpDirectories.ToList());

      // Extract left eye to a Bitmap.
      Bitmap left = VrJpegHelper.ExtractLeftEye(pano);

      // Generate an equirectangular image.
      int maxWidth = 8192;
      bool fillPoles = true;
      Bitmap leftEquir = VrJpegHelper.Equirectangularize(left, pano, fillPoles, maxWidth);

      // Save the result.
      string leftEquirFilename = string.Format("{0}_left_equir.jpg", Path.GetFileNameWithoutExtension(filename));
      string leftEquirFile = Path.Combine(Path.GetDirectoryName(filename), leftEquirFilename);
      leftEquir.Save(leftEquirFile);
    }

    /// <summary>
    /// Extract both eyes and creates a stereo equirectangular image.
    /// The resulting image is suitable for viewing in viewers supporting omni-stereo content.
    /// </summary>
    private static void Example3(string filename)
    {
      // Extract both eyes and create an equirectangular stereo image.
      int maxWidth = 8192;
      bool fillPoles = true;
      Bitmap composite = VrJpegHelper.CreateStereoEquirectangular(filename, EyeImageGeometry.OverUnder, fillPoles, maxWidth);

      // Save the result.
      string compositeFilename = string.Format("{0}_TB.jpg", Path.GetFileNameWithoutExtension(filename));
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
