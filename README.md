# VrJpeg #

.NET Helper library for *.vr.jpg files created by the Google Cardboard Camera Android app.


Extract right eye and audio to separate files:

```csharp
    var xmpDirectories = VrJpegMetadataReader.ReadMetadata(input);
    GPanorama pano = new GPanorama(xmpDirectories.ToList());

    File.WriteAllBytes("right-eye.jpg", pano.ImageData);
    File.WriteAllBytes("audio.mp4", pano.AudioData);
```

Create an equirectangular stereo panorama:
    
    Bitmap stereoPano = VrJpegLoader.Load(input, EyeImageGeometry.OverUnder);
    stereoPano.Save("stereopano.jpg");

