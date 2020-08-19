# GMDTool

GMDTool is a command-line utility to convert Persona .GMD model files to Collada .DAE format. Loading of GMD files is handled by [TGEnigma](https://github.com/TGEnigma)'s [GFDLibrary](https://github.com/TGEnigma/GFD-Studio/tree/master/GFDLibrary); this tool simply builds a Collada file from the data provided.

GMDTool exists as a stopgap until [GFD Studio](https://github.com/TGEnigma/GFD-Studio) is capable of exporting morph targets/blend shapes and has a command-line interface.

GMDTool has been tested on models from the following games using the GMD format:

- [ ] Persona 5
- [x] Persona 4: Dancing All Night
- [ ] Persona 3: Dancing in Moonlight/Persona 5: Dancing in Starlight
- [ ] Persona 5 Royal

GMDTool will probably work on .GFS environment files, but they are currently not supported.

## Usage

```cmd
GMDTool [flags] <input file> [output file]
```

## Unsupported

Currently unsupported elements of GMD files include:

 - Multiple texture channels
 - Lights
 - Cameras
 - Animations