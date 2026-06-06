# Arenji MIDI visualizer
[![Discord](https://discordapp.com/api/guilds/1283308435902824499/widget.png?style=shield)](https://discord.gg/VHVj4JpmdP)

A MIDI visualizer based on [osu-framework](https://github.com/ppy/osu-framework)

## Q: Why did I created this when other apps like SeeMusic and Embers exist?
Seemusic is a unity-based visualizer has caused me numerous headache over the year (especially some of their UI/UX handling); and given that it's a subscription software, it's not tolerable at all.
Ember: I also use over the time, but the decision to lock out some of the crucial function behind a paywall also make my experience not that good.

## Great, I want to help developing this program!
First of all, from the bottom of my heart, thank you so much for paying attention to this niche software
### Requirement (It's actually the same as osu-framework):
- A desktop platform with the [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- An IDE with intellisense and syntax highlighting.

##Running a branch for testing:
```sh
git clone https://github.com/Kuina-chan/arenji <branch>
cd arenji
dotnet build
```
## Contributing:
Contributions are welcomed here, via pull requests.
Please first check out the [opened issues](https://github.com/Kuina-chan/arenji/issues). Some of them are denoted with [Suggestion], some are [Bugs]. The Bugs are more important to resolve.
If you using AI to vibecode or debug a new problem, please make sure that **you** are the one who resposible for the fix and/or new feature are *mostly* bug-free.

## License:
The [MeltySynth](https://github.com/sinshu/meltysynth), [osu-framework](https://github.com/ppy/osu-framework) and [DryWetMidi](https://github.com/melanchall/drywetmidi) is licensed under MIT License

This software itself licensed under GPL v3
