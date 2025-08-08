/*
   Copyright 2022-2025 Kate Ward <kate@dariox.club>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Eto.Drawing;
using System;
using System.IO;
using System.Linq;

using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace kate.shared.EtoForms;

public static class EtoDrawingHelper
{
    /// <summary>
    /// Scale an <see cref="System.Drawing.Image"/> down to the <paramref name="maxWidth"/> and <paramref name="maxHeight"/> provided.
    /// </summary>
    public static System.Drawing.Bitmap ScaleImage(System.Drawing.Image image, int maxWidth, int maxHeight)
    {
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var newImage = new System.Drawing.Bitmap(newWidth, newHeight);
        using var g = System.Drawing.Graphics.FromImage(newImage);
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(image, 0, 0, newWidth, newHeight);
        var bmp = new System.Drawing.Bitmap(newImage);

        return bmp;
    }

    /// <summary>
    /// Create a <see cref="Stream"/> of a <c>.png</c> file based off the <paramref name="svgStream"/> provided.
    /// </summary>
    /// <param name="svgStream">SVG Document Stream</param>
    /// <param name="size">Output size of the PNG</param>
    /// <param name="highQuality">When <see langword="true"/>, a high quality SVG will be rendered (at the size of <paramref name="highQualitySize"/>), then scaled down with high quality image interpolation.</param>
    /// <param name="highQualitySize">Quality to render the SVG at before scaling down when <paramref name="highQuality"/> is <see langword="true"/></param>
    /// <returns><see cref="Stream"/> of the generated <c>.png</c></returns>
    public static Stream PngStreamFromSvg(Stream svgStream, Size size, bool highQuality, Size? highQualitySize)
    {
        var doc = Svg.SvgDocument.Open<Svg.SvgDocument>(svgStream);
        var bm = highQualitySize != null && highQualitySize.HasValue && highQuality
            ? ScaleImage(doc.Draw(highQualitySize.Value.Width, highQualitySize.Value.Height), size.Width, size.Height)
            : doc.Draw(size.Width, size.Height);
        var ms = new MemoryStream();
        bm.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        bm.Dispose();
        return ms;
    }

    /// <inheritdoc cref="PngStreamFromSvg(Stream, Size, bool, Size?)"/>
    public static Stream PngStreamFromSvg(Stream svgStream, Size size)
        => PngStreamFromSvg(svgStream, size, false, null);

    public static Bitmap BitmapFromSvg(Stream svgStream, Size size, bool highQuality, Size? highQualitySize)
    {
        var stream = PngStreamFromSvg(svgStream, size, highQuality, highQualitySize);
        return new Bitmap(stream);
    }

    /// <summary>
    /// Create a <see cref="Stream"/> of an <c>.ico</c> file based off all the <paramref name="images"/> provided.
    /// </summary>
    /// <param name="images">Array of PNG Image Streams and it's sizes.</param>
    /// <returns>Generated <c>.ico</c> stream.</returns>
    public static Stream IconStreamFromPngStreams(params (Stream Stream, Size Size)[] images)
    {
        var pngParts = images.DistinctBy(e => e.Size).OrderByDescending(e => e.Size).ToArray();
        var resultStream = new MemoryStream();
        var sizeCountAsUShort = Convert.ToUInt16(pngParts.Length);
        var sizeCountAsBytes = BitConverter.GetBytes(sizeCountAsUShort);

        // .ico header
        // source: https://en.wikipedia.org/wiki/ICO_(file_format)#Headers
        var header = new byte[6];
        header[0] = 0x00;
        header[1] = 0x00;
        header[2] = 0x01;
        header[3] = 0x00;
        header[4] = sizeCountAsBytes[0];
        header[5] = sizeCountAsBytes[1];
        resultStream.Write(header.AsSpan());

        // headers are always written next to eachother.
        // image data can be anywhere in the file after the headers
        // source: https://en.wikipedia.org/wiki/ICO_(file_format)#Structure_of_image_directory
        var fakePosition = resultStream.Position + (pngParts.Length * 16);
        for (int i = 0; i < pngParts.Length; i++)
        {
            var part = pngParts[i];
            var width = Convert.ToByte(part.Size.Width >= 256 ? 0 : part.Size.Width);
            var height = Convert.ToByte(part.Size.Height >= 256 ? 0 : part.Size.Height);
            var partSize = BitConverter.GetBytes(Convert.ToUInt32(part.Stream.Length));
            var imageStart = BitConverter.GetBytes(Convert.ToUInt32(fakePosition));
            var partHeader = new byte[]
            {
            width, height,
                0x00, 0x00,
                0x01, 0x00,
                0x20, 0x00, // assume 32bits per-pixel
                partSize[0], partSize[1],
                partSize[2], partSize[3],
                imageStart[0], imageStart[1],
                imageStart[2], imageStart[3]
            };
            resultStream.Write(partHeader);
            fakePosition += part.Stream.Length;
        }

        // just write the generated png files as-is after the headers.
        // no extra fluff required, since we're not going to use
        // the BMP format (uses too much space compared to png)
        // source: https://en.wikipedia.org/wiki/ICO_(file_format)#PNG_format
        for (int i = 0; i < pngParts.Length; i++)
        {
            var part = pngParts[i].Stream;
            part.CopyTo(resultStream);
        }
        resultStream.Seek(0, SeekOrigin.Begin);
        return resultStream;
    }

    /// <summary>
    /// Create a <see cref="Stream"/> containing an <c>.ico</c> file, that was generated from all the <paramref name="bitmaps"/> provided.
    /// </summary>
    public static Stream IconStreamFromSystemBitmaps(params System.Drawing.Bitmap[] bitmaps)
    {
        var bitmapArray = bitmaps.DistinctBy(e => e.Size).OrderByDescending(e => e.Size).ToArray();
        var pngParts = new (Stream Stream, Size Size)[bitmapArray.Length];
        for (int i = 0; i < bitmapArray.Length; i++)
        {
            var b = bitmapArray[i];
            var ms = new MemoryStream();
            bitmapArray[i].Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            pngParts[i] = (ms, new(b.Width, b.Height));
        }

        return IconStreamFromPngStreams(pngParts);
    }

    /// <summary>
    /// Create a <see cref="Stream"/> containing an <c>.ico</c> file, that was generated from various <paramref name="sizes"/> of the <paramref name="svgStream"/> provided.
    /// </summary>
    /// <param name="svgStream">SVG Document Stream. Must be seekable!</param>
    /// <param name="sizes">Various Sizes. Width or Height cannot be greater than <c>256</c> (limitation of ICO format)</param>
    /// <returns>Seekable stream of an <c>.ico</c> file, containing various PNG files of the provided <paramref name="svgStream"/>.</returns>
    public static Stream IconStreamFromSvg(Stream svgStream, Size[] sizes)
    {
        if (!svgStream.CanSeek)
            throw new ArgumentException(nameof(svgStream.CanSeek) + " is false.", nameof(svgStream));
        var sizeArray = sizes.Distinct().ToArray();
        var pngParts = new (Stream Stream, Size Size)[sizes.Length];
        for (int i = 0; i < sizeArray.Length; i++)
        {
            svgStream.Seek(0, SeekOrigin.Begin);
            var doc = Svg.SvgDocument.Open<Svg.SvgDocument>(svgStream);
            var bm = doc.Draw(sizes[i].Width, sizes[i].Height);
            var ms = new MemoryStream();
            bm.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            bm.Dispose();
            pngParts[i] = (ms, sizes[i]);
        }
        return IconStreamFromPngStreams(pngParts);
    }
}