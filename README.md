Installation of the loader: Unzip the zip file in releases such that "KsaLoader.exe" is in your game root.
Usage of the loaader: Run "KsaLoader.exe"

Mod assemblies go into a `<Mod>/Assemblies`, any class that inherits from `CodeMod` will automatically be constructed just before the games main function is run

For an example mod, create an Assemblies folder under content/core and drag ExampleThrustMod into it, this should allow space bar pulsing the thruster again (until an update breaks it likely)
