# GMDTool

GMDTool is a command-line utility to convert Persona .GMD model files to Collada .DAE format. Loading of GMD files is handled by [TGEnigma](https://github.com/TGEnigma)'s [GFDLibrary](https://github.com/TGEnigma/GFD-Studio/tree/master/GFDLibrary); this tool simply builds a Collada file from the data provided.

GMDTool exists as a stopgap until [GFD Studio](https://github.com/TGEnigma/GFD-Studio) is capable of exporting morph targets/blend shapes and has a command-line interface.

GMDTool has been tested on models from the following games using the GMD format:

- [x] Persona 4: Dancing All Night
- [ ] Persona 5
- [ ] Persona 3: Dancing in Moonlight/Persona 5: Dancing in Starlight
- [ ] Persona 5 Royal

GMDTool will probably work on most .GFS environment files, but they are currently not supported.

## Usage

```cmd
GMDTool [flags] <input file> [output file]
```

### Flags:

- `--help`: Shows the usage screen.
- `--version`: Shows the current version of the tool.
- `-b`, `--blender-output`: Enables Blender compatibility output (see below).
- `-i`, `--ignore-empty`: Ignores empty nodes.

#### Blender compatibility output

Blender's Collada IO is broken (as of v2.91)—it fails to import meshes which have both vertex weights (i.e. affected by an armature) and morph targets/blend shapes. The Blender ticket for this issue can be found [here](https://developer.blender.org/T42197).

There is a workaround for this, but it breaks support for other 3D applications. As such, the `--blender-output` flag will output two files—a general-purpose Collada file, and a Blender-only Collada file. However, if no meshes are affected by this issue, then the Blender-only file will be skipped and the general-purpose file can be used in both Blender and other 3D applications.

**Note that very few (if any) GMD meshes use both vertex weights as well as morph targets. As such, this flag is (almost?) always unnecessary.**

## Unsupported

Currently unsupported elements of GMD files include:

 - Multiple texture channels
 - Lights
 - Cameras
 - Animations