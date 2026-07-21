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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace kate.shared.EtoForms;

public static class EtoResourceHelper
{
    private readonly static Dictionary<string, Icon> IconCache = new Dictionary<string, Icon>();
    private readonly static Dictionary<string, Bitmap> BitmapCache = new Dictionary<string, Bitmap>();

    /// <summary>
    /// Clear all items in <see cref="IconCache"/> and <see cref="BitmapCache"/>
    /// </summary>
    public static void ClearCache()
    {
        lock (IconCache)
        {
            IconCache.Clear();
        }
        lock (BitmapCache)
        {
            BitmapCache.Clear();
        }
    }

    private static Assembly[] GetAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    /// <summary>
    /// Find Embedded Resource Stream with the provided <paramref name="name"/>
    /// in all assemblies provided.
    /// </summary>
    /// <param name="name">Name of the embedded resource</param>
    /// <param name="assemblies">Assemblies to search in</param>
    /// <returns>
    /// Stream of the Embedded Resource.</returns>
    /// <exception cref="EmbeddedResourceException">
    /// Thrown when the resource couldn't be found, or
    /// <see cref="Assembly.GetManifestResourceStream(string)"/> returns
    /// <see langword="null"/>
    /// </exception>
    internal static Stream GetStream(string name, params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
        {
            var targetName = asm.GetManifestResourceNames()
                .FirstOrDefault(e => e.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(targetName))
            {
                var stream = asm.GetManifestResourceStream(targetName);
                if (stream == null)
                {
                    throw new EmbeddedResourceException($"Resource \"{name}\" stream is null, but it exists in assembly {asm}")
                    {
                        Assembly = asm,
                        SearchedAssemblies = assemblies,
                        ResourceName = name,
                        ResourceExists = true,
                    };
                }
                return stream;
            }
        }
        throw new EmbeddedResourceException($"Could not find resource \"{name}\" in any of the assemblies provided.")
        {
            Assembly = null,
            SearchedAssemblies = assemblies,
            ResourceName = name,
            ResourceExists = false
        };
    }

    /// <summary>
    /// <para>Generate/Get a <see cref="Eto.Drawing.Bitmap"/> from an embedded resource.</para>
    /// If previously loaded, then a cached bitmap will be returned from <see cref="BitmapCache"/>
    /// </summary>
    /// <param name="resourceName">Full resource name.</param>
    /// <param name="assemblies">Assemblies to search in.</param>
    /// <returns>Bitmap of the resource.</returns>
    public static Bitmap Bitmap(string resourceName, params Assembly[] assemblies)
    {
        lock (BitmapCache)
        {
            var key = string.Join("\n", resourceName);
            if (!BitmapCache.TryGetValue(key, out var bitmap))
            {
                var stream = GetStream(resourceName, assemblies);
                bitmap = new Bitmap(stream);
                BitmapCache[key] = bitmap;
            }
            return bitmap;
        }
    }

    /// <summary>
    /// <inheritdoc cref="Bitmap" path="/summary"/>
    /// <para>Will search in all loaded assemblies in the current app domain.</para>
    /// </summary>
    /// <param name="resourceName"><inheritdoc cref="Bitmap(string, Assembly[])" path="/param[@name='resourceName']"/></param>
    /// <returns><inheritdoc cref="Bitmap(string, Assembly[])" path="/returns"/></returns>
    public static Bitmap Bitmap(string resourceName)
        => Bitmap(resourceName, GetAssemblies());

    /// <summary>
    /// Generate/Get a <see cref="Eto.Drawing.Bitmap"/> from an embedded resource that's an SVG.
    /// </summary>
    /// <param name="resourceName">Full resource name.</param>
    /// <param name="size">Size to render the SVG at.</param>
    /// <param name="assemblies">Assemblies to search in.</param>
    /// <returns>Generated <see cref="Eto.Drawing.Bitmap"/> (or cached in <see cref="BitmapCache"/>)</returns>
    public static Bitmap BitmapFromSvg(string resourceName, Size size, params Assembly[] assemblies)
    {
        lock (BitmapCache)
        {
            var keyItems = new string[]
            {
                resourceName,
                "svg-to-bitmap",
                size.Width.ToString(),
                size.Height.ToString()
            };
            var key = string.Join("\n", keyItems);
            if (!BitmapCache.TryGetValue(key, out var svgPng))
            {
                var stream = GetStream(resourceName, assemblies);
                var svgPngStream = EtoDrawingHelper.PngStreamFromSvg(stream, size);
                svgPng = new Bitmap(svgPngStream);
                BitmapCache[key] = svgPng;
            }
            return svgPng;
        }
    }

    /// <summary>
    /// <inheritdoc cref="BitmapFromSvg(string, Size, Assembly[])" path="/summary"/>
    /// <para>Will search in all loaded assemblies in the current app domain.</para>
    /// </summary>
    /// <param name="resourceName"><inheritdoc cref="BitmapFromSvg(string, Size, Assembly[])" path="/param[@name='resourceName']"/></param>
    /// <param name="size"><inheritdoc cref="BitmapFromSvg(string, Size, Assembly[])" path="/param[@name='size']"/></param>
    /// <returns><inheritdoc cref="BitmapFromSvg(string, Size, Assembly[])" path="/returns"/></returns>
    public static Bitmap BitmapFromSvg(string resourceName, Size size)
        => BitmapFromSvg(resourceName, size, GetAssemblies());

    /// <summary>
    /// Get an <see cref="Icon"/> from an embedded resource that's an SVG.
    /// </summary>
    /// <param name="resourceName">Full resource name.</param>
    /// <param name="sizes">Sizes to generate.</param>
    /// <param name="assemblies">Assemblies to search in.</param>
    /// <returns>Generated <see cref="Icon"/> via <see cref="EtoDrawingHelper.IconStreamFromSvg(Stream, Size[])"/>.
    /// Can be cached from <see cref="IconCache"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more item in <paramref name="sizes"/> has a width or height that is greater than <c>256</c>
    /// </exception>
    public static Icon IconFromSvg(string resourceName, Size[] sizes, params Assembly[] assemblies)
    {
        if (sizes.Any(e => e.Width > 256 || e.Height > 256))
            throw new ArgumentException("One or more sizes has a width or height greater than 256");
        lock (IconCache)
        {
            var keyItems = new string[2 + (sizes.Length * 2)];
            keyItems[0] = resourceName;
            keyItems[1] = "svg-to-bitmap-to-icon";
            for (int i = 0; i < sizes.Length; i++)
            {
                var b = (i + 1) * 2;
                keyItems[b] = sizes[i].Width.ToString();
                keyItems[b + 1] = sizes[i].Height.ToString();
            }

            var key = string.Join("\n", keyItems);
            if (!IconCache.TryGetValue(key, out var svgIco))
            {
                var svgStream = new MemoryStream();
                Stream? s = null;
                using (s = GetStream(resourceName, assemblies))
                {
                    s.CopyTo(svgStream);
                }
                var resultStream = EtoDrawingHelper.IconStreamFromSvg(svgStream, sizes);
                var icon = new Icon(resultStream);
                svgIco = icon;
                IconCache[key] = svgIco;
            }

            return svgIco;
        }
    }

    /// <summary>
    /// <para>Generate/Get an <see cref="Icon"/> from an embedded resource that's an SVG.</para>
    /// If not in cache, then this will be generated at: 16x16, 32x32, 64x64, 128x128, 256x256
    /// </summary>
    /// <param name="resourceName"><inheritdoc cref="IconFromSvg(string, Size[], Assembly[])" path="/param[@name='resourceName']"/></param>
    /// <param name="resourceName"><inheritdoc cref="IconFromSvg(string, Size[], Assembly[])" path="/param[@name='assemblies']"/></param>
    /// <returns><inheritdoc cref="IconFromSvg(string, Size[], Assembly[])" path="/returns"/></returns>
    public static Icon IconFromSvg(string resourceName, params Assembly[] assemblies)
        => IconFromSvg(
            resourceName,
            new Size[]
            {
                new(16, 16), new(32, 32), new(64, 64), new(128, 128), new(256, 256)
            },
            assemblies);

    /// <summary>
    /// <para>Generate/Get an <see cref="Icon"/> from an embedded resource that's an SVG.</para>
    /// Will search in all loaded assemblies in the current app domain.
    /// See <see cref="IconFromSvg(string, Assembly[])"/> for more information.
    /// </summary>
    /// <param name="resourceName"><inheritdoc cref="IconFromSvg(string, Size[], Assembly[])" path="/param[@name='resourceName']"/></param>
    /// <returns><inheritdoc cref="IconFromSvg(string, Size[], Assembly[])" path="/returns"/></returns>
    public static Icon IconFromSvg(string resourceName)
        => IconFromSvg(resourceName, GetAssemblies());
}