# GMDTool

GMDTool is a command-line utility to convert Persona .GMD model files to Collada .DAE format. Loading of GMD files is handled by [TGEnigma](https://github.com/TGEnigma)'s [GFDLibrary](https://github.com/TGEnigma/GFD-Studio/tree/master/GFDLibrary); this tool simply builds a Collada file from the data provided. This tool does not allow conversion to GMD.

GMDTool has the following advantages over [GFD Studio](https://github.com/TGEnigma/GFD-Studio) (for now):
 - Command-line interface
 - Support for exporting morph targets/blend shapes
 - Optional Blender compatibility output

GMDTool has been tested on models from the following games using the GMD format:

- [x] Persona 4: Dancing All Night
- [ ] Persona 5
- [ ] Persona 3: Dancing in Moonlight/Persona 5: Dancing in Starlight
- [ ] Persona 5 Royal

GMDTool will generally work on .GFS environment model files, but this has not been thoroughly tested.

## Usage

```cmd
GMDTool [flags] <input file> [output file]
```

### Flags

- `--help`: Shows the usage screen.
- `--version`: Shows the current version of the tool.
- `-b`, `--blender-output`: Enables Blender compatibility output (see below).
- `-i`, `--ignore-empty`: Ignores empty nodes.

### Blender compatibility output

In certain cases, the standard Collada .DAE output will not import properly into Blender. The `--blender-output` file will—*only if necessary*—create a Blender-specific file which may or may not function correctly in other 3D software but will import correctly in Blender. This will happen in the following cases:

#### Mesh contains both vertex weights and morph targets

Blender's Collada IO is broken (as of v2.91)—it fails to import meshes which have both vertex weights (i.e. affected by an armature) and morph targets/blend shapes. The Blender ticket for this issue can be found [here](https://developer.blender.org/T42197). Note that very few (if any) GMD meshes use both vertex weights as well as morph targets, so this is rarely (if ever) an issue.

#### Joint has a mesh as an attachment

Blender's armature structure exists as an exclusive hierarchy of bones; no other objects may exist within it. As such, models which contain meshes as an attachment to joint (bone) nodes will not import correctly into Blender. This structure is intrinisic to Blender's design and as such is unlikely to change. Maybe.

Affected models are not uncommon, and the relevant meshes usually represent non-deforming objects attached to the head (e.g. hats or glasses).

## Unsupported

Currently unsupported elements of GMD files include:

 - Multiple texture channels
 - Lights
 - Cameras
 - Animations

Blender compatibility has not yet been implemented for models which contain meshes influenced by multiple armatures.