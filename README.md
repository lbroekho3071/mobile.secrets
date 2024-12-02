# Mobile.Secrets
This small package is used to generate secrets from an `appsettings` file or your environment. This project is aimed to be used with MAUI. It can also be used in other projects/frameworks such as ASP.NET.

## How to use
1. Download this package from NuGet into your project.
2. For local development, create an `appsettings.json` file in the root of your project, not your solution.
3. Build your application.

You will find a `Settings.Generated.cs` file in your `obj` directory. The secrets from your `appsettings.json` have been generated and compiled and are ready to use in your project.

To use this in your pipeline, simply add your secrets to your environment. The name of each secret should be prefixed with `Secret_`, that way Mobile.Secrets can find your secrets in your environment.

## Notes
1. I created this project because I was not able to migrate the [Mobile.BuildTools](https://github.com/dansiegel/Mobile.BuildTools) package from Xamarin to MAUI. We were only using configuration, so I figured it would be interesting to create my own small solution. That being said, without the Mobile.BuildTools package I wouldn't have come far.
2. It is possible use nested objects in your `appsettings.json` file. Keep in mind that the names of each object are chained together with an `_`. The nested `ConnectionStrings` in this [test `appsettings` file](https://github.com/lbroekho3071/mobile.secrets/blob/main/TestApplication/appsettings.Example.json) are renamed to `ConnectionStrings_Default` in the generated class.
3. Currently, arrays are unsupported and will not be generated.

## Solution structure
This solution contains a couple of projects, these are:
1. `Mobile.Secrets`: the actual package.
2. `TestApplication`: an application I used to do some local testing/development.
3. `UnitTests`: mostly used for development and currently not intended for CI.