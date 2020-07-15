
# Moodle LTI Client Library

[![Build & Test](https://github.com/JanWichelmann/MoodleLti/workflows/Build%20&%20Test/badge.svg)](https://github.com/JanWichelmann/MoodleLti/actions)
[![MoodleLti at NuGet](https://buildstats.info/nuget/MoodleLti)](https://www.nuget.org/packages/MoodleLti/)

The MoodleLti library offers an abstracted way to access Moodle's gradebook via the LTI interface.

## MoodleLti

Classes:
- `MoodleAuthenticationTools`: Parses a LTI tool launch request and allows accessing its parameters.
- `MoodleLtiApi`: Implements low-level functions to communicate with Moodle via the LTI API.
- `MoodleGradebook`: Provides abstracted means to read and write to the Moodle gradebook: Get/create/update/delete columns, set scores.
- `CachedMoodleGradebook`: Is based on `MoodleGradebook`, but caches retrieved data to reduce expensive LTI API queries. This should be used when a lot of gradebook operations are performed.

## MoodleLti.DependencyInjection

[![MoodleLti.DependencyInjection at NuGet](https://buildstats.info/nuget/MoodleLti.DependencyInjection)](https://www.nuget.org/packages/MoodleLti.DependencyInjection/)

This is an utility library, which adds several extensions methods to the `IServiceCollection` used for dependency injection.

## Examples

The SampleToolProvider project contains a basic ASP.NET Core web application, which uses the functions provided by MoodleLti.
