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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetadataExtractor.Formats.Xmp;
using XmpCore;

namespace VrJpeg
{
  /// <summary>
  /// A high level representation of the metadata contained in Google Camera depth map images.
  /// </summary>
  public class GDepth
  {
    // Ref
    // https://developers.google.com/depthmap-metadata/reference

    public double FocusBlurAtInfinity { get; private set; }
    public double FocusFocalDistance { get; private set; }
    public double FocusFocalPointX { get; private set; }
    public double FocusFocalPointY { get; private set; }
    public string ImageMime { get; private set; }
    public string DepthFormat { get; private set; }
    public double DepthNear { get; private set; }
    public double DepthFar { get; private set; }
    public string DepthMime { get; private set; }
    public byte[] ImageData { get; private set; }
    public byte[] DepthData { get; private set; }

    private const string nsImage = "http://ns.google.com/photos/1.0/image/";
    private const string nsFocus = "http://ns.google.com/photos/1.0/focus/";
    private const string nsDepthmap = "http://ns.google.com/photos/1.0/depthmap/";

    public GDepth(List<XmpDirectory> directories)
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      // The first directory always contains the regular metadata and the second directory the extended data.
      if (directories == null || directories.Count != 2)
        return;

      ParseDirectory(directories[0].XmpMeta);
      ParseExtendedDirectory(directories[1].XmpMeta);
    }

    private void ParseDirectory(IXmpMeta meta)
    {
      try
      {
        FocusBlurAtInfinity = meta.GetPropertyDouble(nsFocus, "GFocus:BlurAtInfinity");
        FocusFocalDistance = meta.GetPropertyDouble(nsFocus, "GFocus:FocalDistance");
        FocusFocalPointX = meta.GetPropertyDouble(nsFocus, "GFocus:FocalPointX");
        FocusFocalPointY = meta.GetPropertyDouble(nsFocus, "GFocus:FocalPointY");
        ImageMime = meta.GetPropertyString(nsImage, "GImage:Mime");
        DepthFormat = meta.GetPropertyString(nsDepthmap, "GDepth:Format");
        DepthNear = meta.GetPropertyDouble(nsDepthmap, "GDepth:Near");
        DepthFar = meta.GetPropertyDouble(nsDepthmap, "GDepth:Far");
        DepthMime = meta.GetPropertyString(nsDepthmap, "GDepth:Mime");
      }
      catch (Exception e)
      {
        Debug.WriteLine(e);
        // Silent catch. Not sure what to do.
      }
    }

    private void ParseExtendedDirectory(IXmpMeta meta)
    {
      try
      {
        ImageData = meta.GetPropertyBase64(nsImage, "GImage:Data");
        DepthData = meta.GetPropertyBase64(nsDepthmap, "GDepth:Data"); 
      }
      catch
      {

      }
    }
  }
}
