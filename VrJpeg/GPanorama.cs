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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetadataExtractor.Formats.Xmp;
using XmpCore;

namespace VrJpeg
{
  /// <summary>
  /// A high level representation of the metadata contained in Google's *.vr.jpeg files.
  /// </summary>
  public class GPanorama
  {
    public int PanoCroppedAreaLeftPixels { get; private set; }
    public int PanoCroppedAreaTopPixels { get; private set; }
    public int PanoCroppedAreaImageWidthPixels { get; private set; }
    public int PanoCroppedAreaImageHeightPixels { get; private set; }
    public int PanoFullPanoWidthPixels { get; private set; }
    public int PanoFullPanoHeightPixels { get; private set; }
    public int PanoInitialViewHeadingDegrees { get; private set; }
    public string ImageMime { get; private set; }
    public string AudioMime { get; private set; }
    public byte[] ImageData { get; private set; }
    public byte[] AudioData { get; private set; }

    private const string nsPano = "http://ns.google.com/photos/1.0/panorama/";
    private const string nsImage = "http://ns.google.com/photos/1.0/image/";
    private const string nsAudio = "http://ns.google.com/photos/1.0/audio/";

    public GPanorama(List<XmpDirectory> directories, bool importAudio)
    {
      // The first directory always contains the regular metadata and the second directory the extended data.
      if (directories == null || directories.Count != 2)
        return;

      ParseDirectory(directories[0].XmpMeta);
      ParseExtendedDirectory(directories[1].XmpMeta, importAudio);
    }

    private void ParseDirectory(IXmpMeta meta)
    {
      try
      {
        PanoCroppedAreaLeftPixels = meta.GetPropertyInteger(nsPano, "GPano:CroppedAreaLeftPixels");
        PanoCroppedAreaTopPixels = meta.GetPropertyInteger(nsPano, "GPano:CroppedAreaTopPixels");
        PanoCroppedAreaImageWidthPixels = meta.GetPropertyInteger(nsPano, "GPano:CroppedAreaImageWidthPixels");
        PanoCroppedAreaImageHeightPixels = meta.GetPropertyInteger(nsPano, "GPano:CroppedAreaImageHeightPixels");
        PanoFullPanoWidthPixels = meta.GetPropertyInteger(nsPano, "GPano:FullPanoWidthPixels");
        PanoFullPanoHeightPixels = meta.GetPropertyInteger(nsPano, "GPano:FullPanoHeightPixels");
        PanoInitialViewHeadingDegrees = meta.GetPropertyInteger(nsPano, "GPano:InitialViewHeadingDegrees");
        ImageMime = meta.GetPropertyString(nsImage, "GImage:Mime");
        AudioMime = meta.GetPropertyString(nsAudio, "GAudio:Mime");
      }
      catch
      {
        // Silent catch. Not sure what to do.
      }
    }

    private void ParseExtendedDirectory(IXmpMeta meta, bool importAudio)
    {
      try
      {
        ImageData = meta.GetPropertyBase64(nsImage, "GImage:Data");

        if (importAudio)
          AudioData = meta.GetPropertyBase64(nsAudio, "GAudio:Data");
      }
      catch
      {

      }
    }
  }
}
