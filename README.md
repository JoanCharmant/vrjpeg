# VrJpeg #

.NET Helper library for *.vr.jpg files. These files are produced by the Google Cardboard Camera Android app.

Code samples:

- High level abstraction: create a full equirectangular stereo-panorama with poles filled in.

```csharp
bool fillPoles = true;
int maxWidth = 8192;
Bitmap stereoPano = VrJpegHelper.CreateStereoEquirectangular(input, EyeImageGeometry.OverUnder, fillPoles, maxWidth);
stereoPano.save("stereopano.jpg");
```

- Mid level abstraction: extract the left-eye from the metadata, then convert it to an equirectangular image.

```csharp
var xmpDirectories = VrJpegMetadataReader.ReadMetadata(input);
GPanorama pano = new GPanorama(xmpDirectories.ToList());

Bitmap left = VrJpegHelper.ExtractLeftEye(pano);
Bitmap leftEquir = VrJpegHelper.Equirectangularize(left, pano);
leftEquir.save("left-equir.jpg");
```

- Low level abstraction: extract left-eye JPEG bytes and audio MP4 bytes directly from the metadata.

```csharp
var xmpDirectories = VrJpegMetadataReader.ReadMetadata(input);
GPanorama pano = new GPanorama(xmpDirectories.ToList());

File.WriteAllBytes("left-eye.jpg", pano.ImageData);
File.WriteAllBytes("audio.mp4", pano.AudioData);
```
