# VrJpeg #

.NET Helper library for *.vr.jpg files created by the Google Cardboard Camera Android app.


Extract left eye and audio to separate files:

```csharp
    var xmpDirectories = VrJpegMetadataReader.ReadMetadata(input);
    GPanorama pano = new GPanorama(xmpDirectories.ToList());

    File.WriteAllBytes("left-eye.jpg", pano.ImageData);
    File.WriteAllBytes("audio.mp4", pano.AudioData);
```

Create a full equirectangular stereo-panorama, with poles filled in:
    
```csharp
    var xmpDirectories = VrJpegMetadataReader.ReadMetadata(input);
    GPanorama pano = new GPanorama(xmpDirectories.ToList());

    Bitmap right = new Bitmap(input);
    Bitmap left = VrJpegHelper.ExtractLeftEye(pano);

    int maxWidth = 8192;
    Bitmap rightEquir = VrJpegHelper.CreateEquirectangularImage(right, pano, maxWidth, true);
    Bitmap leftEquir = VrJpegHelper.CreateEquirectangularImage(left, pano, maxWidth, true);
    
    Bitmap stereoPano = VrJpegHelper.Compose(leftEquir, rightEquir, EyeImageGeometry.OverUnder);
    
    stereoPano.Save("stereopano.jpg");
```