# kate.shared

![license](https://img.shields.io/github/license/ktwrd/csharp-kate.shared)
[![kate.shared - version](https://img.shields.io/github/v/tag/ktwrd/csharp-kate.shared?sort=date&label=latest%20version)](https://nuget.org/packages/kate.shared)
[![kate.shared.CommandLine - version](https://img.shields.io/nuget/v/kate.shared.CommandLine?label=kate.shared.CommandLine)](https://nuget.org/packages/kate.shared.CommandLine)
[![kate.shared.EtoForms - version](https://img.shields.io/nuget/v/kate.shared.EtoForms?label=kate.shared.EtoForms)](https://nuget.org/packages/kate.shared.EtoForms)

kate's C# Shared Library!

A series of utility/helper classes to make my life easier with rapidly
developing projects at home, and at work.

>[!IMPORTANT]
> Since I mostly use all of these libraries for personal use, I might not accept PRs for new enhancements unless I see some purpose in it.

- kate.shared: Platform-agnostic helper classes
  - Embedded Resource exception
  - Pre-made Regular Expressions for detecting Base64
  - Randomly selecting items in a collection with weights (e.g: 30% chance for A
    to be selected, 15% chance for B to be selected, etc...)
  - Easily retry running a function if it times out in any way.
    (`kate.shared/Helpers/ExceptionHelper.cs`)
  - Detect if an exception could be classified as a timeout.
  - Array element shifting (in ArrayExtensions)
  - Parse hexadecimal in strings as an array of bytes
  - Easily read embedded resources and check if they exist
- kate.shared.EtoForms: Helper classes for Eto.Forms, which includes;
  - Bitmap/Icon caching
  - Svg to Bitmap/Icon conversion from Embedded Resource at runtime
  - Helper methods for updating font size, style, and family without creating a
    new instance (e.g: `myFont.WithFontSize(9f)`)
  - Svg to Png conversion
  - Svg to Ico conversion (with 32bpp ico support!)
  - Image scaling (via `System.Drawing.Image`)
- kate.shared.CommandLine:
  - Easily generate commands and command options for use with
    [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)

> [!NOTE]
> Currently there aren't any good samples for a lot of the things in these
> projects, but samples on how to use (for example) `kate.shared.CommandLine`
> will come in the future ^w^
>
> The same goes for Unit Tests. They will be made sometime in the future, but not too far away
> > kate, 2026/07/21

## Supported .NET Versions

- ✔️ - Supported and working.
- ⚠️ - Not Tested
- ❌ - Not Supported

| Supported .NET Version | `kate.shared`           | `kate.shared.CommandLine` | `kate.shared.EtoForms` |
| ---------------------- | ----------------------- | ------------------------- | ---------------------- |
| .NET 10.x              | ✔️                      | ✔️                        | ✔️                     |
| .NET 9.x               | ✔️ (via .NET 8)         | ✔️ (via .NET 8)           | ✔️ (via .NET 8)        |
| .NET 8.x (recommended) | ✔️                      | ✔️                        | ✔️                     |
| .NET 6.x               | ⚠️ (via netstandard2.0) | ❌                        | ❌                     |
| Framework 2.x to 4.x   | ⚠️                      | ❌                        | ❌                     |
| Core 1.x to 3.x        | ⚠️                      | ❌                        | ❌                     |

**Notes**

- .NET Framework 4.x to 2.x, and .NET Core 3.x to 1.x, and .NET 6 are inferred
  since `kate.shared` targets `netstandard2.0`.
- .NET 9 support is inferred, since all projects target .NET 8

## License

The code for `kate.shared` and all projects in this repository are licensed
under the [Apache 2.0 License](https://opensource.org/licenses/Apache-2.0).
Please see [the license file](LICENSE.md) FOR MORE INFORMATION.

[tl;dr](https://tldrlegal.com/license/apache-license-2.0-(apache-2.0)) You can
do what you want with the software, as long you include the required notices
(which would be the license & copyright). The developers aren't liable for
anything bad that happens.

## Mirrors

In the event something happens to my Github account, I've mirrored this repository (and the NuGet packages) to other places on the internet.

### Git Repository

- [Github](https://github.com/ktwrd/csharp-kate.shared)
- [git.redfur.cloud](https://git.redfur.cloud/kate/csharp-kate.shared/packages)
- [Gitlab](https://gitlab.com/ktwrd/csharp-kate-shared)

### NuGet Packages

| Name                      | Mirrors                                                                                                                                                                                                                                               |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `kate.shared`             | [NuGet.org](https://nuget.org/packages/kate.shared), [Github](https://github.com/ktwrd/csharp-kate.shared/pkgs/nuget/kate.shared), [git.redfur.cloud](https://git.redfur.cloud/kate/-/packages/nuget/kate.shared)                                     |
| `kate.shared.CommandLine` | [NuGet.org](https://nuget.org/packages/kate.shared.CommandLine), [Github](https://github.com/ktwrd/csharp-kate.shared/pkgs/nuget/kate.shared.CommandLine), [git.redfur.cloud](https://git.redfur.cloud/kate/-/packages/nuget/kate.shared.CommandLine) |
| `kate.shared.EtoForms`    | [NuGet.org](https://nuget.org/packages/kate.shared.EtoForms), [Github](https://github.com/ktwrd/csharp-kate.shared/pkgs/nuget/kate.shared.EtoForms), [git.redfur.cloud](https://git.redfur.cloud/kate/-/packages/nuget/kate.shared.EtoForms)          |
