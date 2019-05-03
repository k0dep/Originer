Originer
=========

The package for Unity3d after the addition of which makes it possible to automatically resolve and enable the "Unity package manager" system dependencies on projects located on Github.

The essence of the problem
--------------------------

Unity package manager allow include packages from git repositories together with default upm repository.  

It looks like this:  
`./Packages/manifest.json`
```json
{
  "dependencies": {
    "bindingrx": "https://github.com/k0dep/bindingrx.git#2.1.1",
    "com.unity.package-manager-ui": "2.1.2",
    "com.unity.some-default-packages-from-upm": "1.0.0"
  },
}
```

This feature is described in [the UPM dedicated UPM forum.](https://forum.unity.com/threads/git-support-on-package-manager.573673/)  

But if the `BuindingRx` package will have dependencies on the packages not added to `manifest.json` inside it, this will result in Unity not resolving this dependency.  

This problem can be solved with the help of scoped registries. This feature is described in [this thread.](https://forum.unity.com/threads/setup-for-scoped-registries-private-registries.573934/#post-3819754).  

But this method requires certain package naming (the package must have a certain prefix, for example `com.unity.*`) And deploy its own solution for storing and distributing npm packages, for example [Verdaccio](https://github.com/verdaccio/verdaccio), which is not a convenient solution for small and / or open source projects.  

This package solve this issue. If the project has the `Originer` package in dependencies, then after starting the unity, the package will try to find unresolved dependencies in the installed projects, try to find them in Github according to certain rules described below and install them in `manifest.json`.

Repository requirements
-----------------------

In order for `Originer` to be able to find your repository in github as a dependency and successfully include it in the proget, you need to add a repository with a __topic__ named `upm-package`

Еxamples of repositories as packages:
* [BindingRx](https://github.com/k0dep/BindingRx)
* [type-inspector](https://github.com/k0dep/type-inspector)
* [AutoInjector](https://github.com/k0dep/AutoInjector)
* [MoonSharp](https://github.com/k0dep/MoonSharp)
* [UniRx](https://github.com/k0dep/UniRx)

[Full list of available repositories](https://github.com/topics/upm-package) for use as upm package by `Originer`  

[More about Github topics](https://github.blog/2017-01-31-introducing-topics/)

Using
-----

For start using this package add lines into `./Packages/manifest.json` like next sample:  
```json
{
  "dependencies": {
    "originer": "https://github.com/k0dep/originer.git"
  }
}
```

After this step, just add your package in `./Packages/manifest.json` and open Unity editor. `Originer` will ask you for permission to make changes to manifest.json. Enjoy!
