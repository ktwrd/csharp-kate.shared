## v1.6.0 (24th Oct, 2025)

>[!IMPORTANT]
> This release drops support for `netstandard2.0`, and is only directly compiled for `net6.0`, `net8.0`, and `net9.0`

- Updated `System.CommandLine` to `2.0.0-rc.2.25502.107`
    - Migration guide for `beta5` and earlier: [https://learn.microsoft.com/en-us/dotnet/standard/commandline/migration-guide-2.0.0-beta5](https://learn.microsoft.com/en-us/dotnet/standard/commandline/migration-guide-2.0.0-beta5)
- Multiple aliases for parameters/options can be defined with `ActionParameterAliasAttribute`.

`ActionParameterAttribute`
- Fixed runtime issues with constructor having parameter type `Nullable<char>`, which was replaced with `char`
- Add ability to enable/disable `System.CommandLine.Option.AllowMultipleArgumentsPerToken` via property `AllowMultipleArgumentsPerToken`
- Property `ShortNameAlias` is no longer nullable, and is stored as `string`.

## v1.5.0 (8th Aug, 2025)

- Add copyright & license to the top of every file.
