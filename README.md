# Cassette.Owin

Owin middleware for [Cassette](https://github.com/andrewdavey/cassette)

This is an Owin implementation of Cassette.Aspnet to allow serving assets independently from your site's framework and without the need for System.Web.

**WARNING:** This project **is not** production ready. It's currently missing cache headers, tests, and a number of other things marked with `ToDo` comments. For these reasons there is currently no NuGet package for it.


## Usage

More detailed examples may be found in the [samples](/samples) folder.

#### Basic Setup

```csharp
public class Startup
{
    public static void Configuration(IAppBuilder app)
    {
        app.UseCassette(new CassetteOptions
        {
            // by default this is /cassette
            RouteRoot = "/asset-route"
        });

        // for now Cassette.Owin requires Microsoft.StaticFiles
        app.UseStaticFiles();

        app.UseNancy();
    }
}
```

#### Diagnostics

The diagnostic page may be viewed by navigating to the `RouteRoot` value which by default is `/cassette`.

#### Notes

- This project has only been tested with [NancyFX](http://nancyfx.org/) and the [SystemWeb host](https://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin.Host.SystemWeb/)
- Currently Cassette.Owin requires the use of [Microsoft.StaticFiles](https://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin.StaticFiles/) for handling assets under `/cassette/file/...`


## View Engines

The following view engines are currently supported.

- Nancy
  - Razor - sample project included

*For other setups you can use the Nancy Razor extension methods as a starting point.*


## Development


This repository contains `.git*` and `.hg*` files. This allows for the use of either Git or [Hg](http://mercurial.selenic.com/) using Fog Creek's [Kiln Harmony](http://www.fogcreek.com/kiln/).

To use Hg you will need to clone or fork this repository and then import it into your instance of Kiln as a Git repository. From there you will be able to work in Hg exclusively aside from when pushing changes up to GitHub. You will also need to enable the [EOL extension](http://mercurial.selenic.com/wiki/EolExtension) for this repository if you do not have it setup globally.


## Copyright and license

Copyright (c) 2013 Brian Surowiec under [the MIT License](LICENSE).