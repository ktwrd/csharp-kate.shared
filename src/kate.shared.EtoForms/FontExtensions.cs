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
using Eto.Forms;
using System;

namespace kate.shared.EtoForms;

/// <summary>
/// Extensions for <see cref="Font"/>
/// </summary>
public static class FontExtensions
{
    /// <summary>
    /// Duplicate the <paramref name="instance"/> provided, and set <see cref="Font.Size"/>
    /// to the <paramref name="value"/> provided.
    /// </summary>
    /// <param name="instance">Instance to duplicate</param>
    /// <param name="value">New value for <see cref="Font.Size"/></param>
    public static Font WithSize(this Font instance, float value)
    {
        return new(
            instance.Family,
            value,
            instance.FontStyle,
            instance.FontDecoration);
    }

    /// <summary>
    /// Duplicate the <paramref name="instance"/> provided, and set <see cref="Font.FontStyle"/>
    /// to the <paramref name="value"/> provided.
    /// </summary>
    /// <param name="instance">Instance to duplicate</param>
    /// <param name="style">New value for <see cref="Font.FontStyle"/></param>
    public static Font WithStyle(this Font instance, FontStyle style)
    {
        return new(
            instance.Family,
            instance.Size,
            style,
            instance.FontDecoration);
    }

    /// <summary>
    /// Duplicate the <paramref name="instance"/> provided, and set <see cref="Font.FamilyName"/>
    /// to the <paramref name="value"/> provided.
    /// </summary>
    /// <param name="instance">Instance to duplicate</param>
    /// <param name="family">New value for <see cref="Font.FamilyName"/></param>
    public static Font WithFamily(this Font instance, string family)
    {
        return new(
            family,
            instance.Size,
            instance.FontStyle,
            instance.FontDecoration);
    }

    /// <summary>
    /// Use a monospace font (via <see cref="Fonts.Monospace(float, FontStyle, FontDecoration)"/>)
    /// for the <paramref name="control"/> provided.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Font.Handler"/> isn't an instance of <see cref="Font.IHandler"/> (font is <see cref="Fonts.Monospace(float, FontStyle, FontDecoration)"/>)
    /// </exception>
    public static void UseMonospaceFont<TControl>(this TControl control)
        where TControl : CommonControl
    {
        var targetFont = Fonts.Monospace(
            control.Font.Size,
            control.Font.FontStyle,
            control.Font.FontDecoration);
        if (targetFont.Handler is Font.IHandler fh)
        {
            control.Font = control.Font.WithFamily(fh.Family.LocalizedName);
        }
        else
        {
            throw new InvalidOperationException($"Unknown type {targetFont.Handler.GetType()} for property {nameof(targetFont.Handler)} in {targetFont.GetType()}");
        }
    }
}
