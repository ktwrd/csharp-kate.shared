# kate.shared
Kate's C# Shared Resources!

Here you will find multiple C# Projects that are mostly for a specific use-case (e.g; `kate.shared.Firebase` for helping with Google Firebase Intergration) or things that I'm not too fond of repeatedly copying and pasing from a known-good project (e.g; [`SixGrid.BuildService`](https://github.com/sixgrid/buildservice)) to a new project that may never be finished.

## Supported .NET Versions
| Version           | Status             |
| ----------------- | ------------------ |
| 6.x (recommended) | **Working**        |
| 5.x               | Not Tested         |
| Framework 4.x     | **Working**        |
| Framework 3.x     | Not Tested         |
| Framework 2.x     | Not Tested         |
| Core 3.x          | **Working**        |
| Core 2.x          | Not Tested         |
| Core 1.x          | Not Tested         |
## Projects

- `kate.shared`
    - Shared project that holds all the basic stuff that every other sub-project here kinda requires.
- `kate.shared.Firebase`
    - Intergration with Google Cloud Firebase. Currently only supports "fast" serialization/deserialization of a [DocumentRefrence](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/Google.Cloud.Firestore.DocumentReference) to a class and for safely getting the value(s) from a specified [DocumentSnapshot](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/Google.Cloud.Firestore.DocumentSnapshot).

## License
The code for `kate.shared` and all projects in this repository are licensed under the [Apache 2.0 License](https://opensource.org/licenses/Apache-2.0). Please see [the license file](LICENSE.md) FOR MORE INFORMATION.

[tl;dr](https://tldrlegal.com/license/apache-license-2.0-(apache-2.0))
You can do what you want with the software, as long you include the required notices. The developers aren't liable for any bad shit that happens.
