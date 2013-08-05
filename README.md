# RetroCraft

A Minecraft time machine! This software lets you connect to Minecraft Classic servers from within modern
versions of Minecraft, without mods. You connect to RetroCraft instead of the server, and it connects to
the Classic server on your behalf.

RetroCraft uses [Craft.Net](https://github.com/SirCmpwn/Craft.Net) to do all the heavy lifting. It's an
awesome .NET library that does pretty much everything related to Minecraft.

RetroCraft runs on Linux, Mac, and Windows. On the former two, you need to install Mono first.

The current supported version of modern Minecraft is **1.6.2**.

**RetroCraft is a work in progress. Good luck**

## Usage

To use RetroCraft, you'll need to run it from the command line. A more user-friendly interface is planned,
but until then, do this to start it up:

    [mono] RetroCraft.exe http://minecraft.net/classic/play/...

Make sure you give it the URL of the classic server you wish to play on. It'll output an IP address for
you to connect your modern client to, and you should be good to go. Have fun!

## Compiling

To compile RetroCraft, install the relevant development tools for .NET and C#. Run this on Windows:

    msbuild.exe

From the root of the repository. On Linux or Mac, use this:

    xbuild

Again, from the root of the repo.

## Getting Help

\#craft.net on irc.freenode.net.

## Contributing

Feel free to make a fork and submit a pull request. Adhere to the coding style already in use. Be
prepared for your changes to be scrutinized and improvements to be suggested before your pull request
is accepted.
