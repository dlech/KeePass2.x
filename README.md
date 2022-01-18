# KeePass2.x
This is an unofficial mirror of the official KeePass2.x source code.

## Branches

The default branch is `VS2022`. You should be able to clone this branch and build it
in Visual Studio 2022 with no changes.

The `official` branch contains exactly the official source code that is distributed on
SourceForge. Some manual changes are required to get this branch to build.

## NuGet

A pre-compiled debug build with debug symbols and embedded source code is
available on NuGet.

https://www.nuget.org/packages/KeePass/

## Changes

The `VS2022` branch tries to keep a minimal diff to the `official` branch. Only
changes required to make it work with newer versions of Visual Studio are made.
The diff can be seen [here](https://github.com/dlech/KeePass2.x/compare/official...VS2022#files_bucket).

If you are interested in submitting changes to KeePass 2.x, you can use `git`
to create a diff and post the patch at https://sourceforge.net/p/keepass/patches/.

Pull Requests to this repository that fix an issue with building in Visual Studio
or to the NuGet package will be considered, but all other patches should be
submitted upstream.
