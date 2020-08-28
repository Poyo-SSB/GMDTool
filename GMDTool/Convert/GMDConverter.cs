using GFDLibrary;
using GFDLibrary.Materials;
using GFDLibrary.Models;
using GFDLibrary.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace GMDTool.Convert
{
    public class GMDConverter
    {
        private readonly GMDConverterOptions options;

        private readonly ModelPack modelPack;

        private readonly string outputPath;
        private readonly string textureFolderName;
        private readonly string textureFolder;

        // whether the converter is currently running in Blender-compatibility mode.
        // set after the first (non-Blender) run through if the Blender output option is enabled, before the second run through.
        private bool blenderMode;

        // on the first (non-Blender) run through of the model, there are various points where the converter can realize that Blender output is necessary, and will mark it here.
        private bool blenderOutputNecessary = false;

        private XmlDocument document;
        private XmlElement libraryImagesElement;
        private XmlElement libraryEffectsElement;
        private XmlElement libraryMaterialsElement;
        private XmlElement libraryGeometriesElement;
        private XmlElement libraryControllersElement;
        private XmlElement visualSceneElement;

        private List<Mesh> meshes;

        public GMDConverter(GMDConverterOptions options)
        {
            this.options = options;

            this.modelPack = Resource.Load<ModelPack>(options.Input);
            this.outputPath = options.Output;
            this.textureFolderName = $@"{Path.GetFileNameWithoutExtension(options.Output)}_Textures";
            this.textureFolder = Path.Combine(Path.GetDirectoryName(options.Output), this.textureFolderName);

            Console.WriteLine($"input: {options.Input}");
            Console.WriteLine($"output: {options.Output}");
        }

        public void Export()
        {
            this.GenerateXml();

            if (this.options.EnableBlenderCompatibilityOutput)
            {
                if (this.blenderOutputNecessary)
                {
                    this.blenderMode = true;
                    this.GenerateXml();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Skipping unnecessary Blender compatibility output.");
                    Console.ResetColor();
                }
            }

            this.ExportTextures();
        }

        private void GenerateXml()
        {
            this.document = new XmlDocument();

            this.document.AppendChild(this.document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            var rootElement = this.document.CreateElement("COLLADA");
            rootElement.SetAttribute("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            rootElement.SetAttribute("version", "1.4.1");

            rootElement.AppendChild(this.CreateAssetXmlElement());
            rootElement.AppendChild(this.CreateLibraryImagesXmlElement());
            rootElement.AppendChild(this.CreateLibraryEffectsXmlElement());
            rootElement.AppendChild(this.CreateLibraryMaterialsXmlElement());
            rootElement.AppendChild(this.CreateLibraryGeometriesXmlElement());
            rootElement.AppendChild(this.CreateLibraryControllersXmlElement());
            rootElement.AppendChild(this.CreateLibraryVisualScenesXmlElement());
            rootElement.AppendChild(this.CreateSceneXmlElement());

            this.document.AppendChild(rootElement);

            if (this.blenderMode)
            {
                string directory = Path.GetDirectoryName(this.outputPath);
                string fileName = Path.GetFileNameWithoutExtension(this.outputPath);
                string fileExtension = Path.GetExtension(this.outputPath);
                string blenderOutput = Path.Combine(directory, fileName + "_blender" + fileExtension);
                this.document.Save(Path.Combine(blenderOutput));
            }
            else
            {
                this.document.Save(Path.Combine(this.outputPath));
            }
        }

        private XmlNode CreateAssetXmlElement()
        {
            var assetElement = this.document.CreateElement("asset");

            var contributorElement = this.document.CreateElement("contributor");

            var authorElement = this.document.CreateElement("author");
            authorElement.AppendChild(this.document.CreateTextNode("Poyo"));
            contributorElement.AppendChild(authorElement);

            var authoringToolElement = this.document.CreateElement("authoring_tool");
            authoringToolElement.AppendChild(this.document.CreateTextNode("GMDTool"));
            contributorElement.AppendChild(authoringToolElement);

            assetElement.AppendChild(contributorElement);

            var CreatedXmlElement = this.document.CreateElement("created");
            CreatedXmlElement.AppendChild(this.document.CreateTextNode(DateTime.Now.ToString("s")));
            assetElement.AppendChild(CreatedXmlElement);

            var modifiedElement = this.document.CreateElement("modified");
            modifiedElement.AppendChild(this.document.CreateTextNode(DateTime.Now.ToString("s")));
            assetElement.AppendChild(modifiedElement);

            var unitElement = this.document.CreateElement("unit");
            unitElement.SetAttribute("name", "meter");
            unitElement.SetAttribute("meter", "1");
            assetElement.AppendChild(unitElement);

            var upAxisElement = this.document.CreateElement("up_axis");
            upAxisElement.AppendChild(this.document.CreateTextNode("Y_UP"));
            assetElement.AppendChild(upAxisElement);

            return assetElement;
        }

        private XmlNode CreateLibraryImagesXmlElement()
        {
            this.libraryImagesElement = this.document.CreateElement("library_images");

            foreach (Material material in this.modelPack.Materials.Materials)
            {
                if (material.Flags.HasFlag(MaterialFlags.HasDiffuseMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "diffuse", material.DiffuseMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasNormalMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "normal", material.NormalMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasSpecularMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "specular", material.SpecularMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasReflectionMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "reflection", material.ReflectionMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasHighlightMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "highlight", material.HighlightMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasGlowMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "glow", material.GlowMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasNightMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "night", material.NightMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasDetailMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "detail", material.DetailMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasShadowMap))
                {
                    this.libraryImagesElement.AppendChild(this.CreateImageXmlElement(material.Name, "shadow", material.ShadowMap));
                }
            }

            return this.libraryImagesElement;
        }

        private XmlNode CreateImageXmlElement(string materialName, string textureMapType, TextureMap textureMap)
        {
            var imageElement = this.document.CreateElement("image");
            imageElement.SetAttribute("id", materialName + "-" + textureMapType + "-image");

            var initFromElement = this.document.CreateElement("init_from");
            initFromElement.AppendChild(this.document.CreateTextNode(Path.Combine(this.textureFolderName, textureMap.Name)));

            imageElement.AppendChild(initFromElement);

            return imageElement;
        }

        private XmlNode CreateLibraryEffectsXmlElement()
        {
            this.libraryEffectsElement = this.document.CreateElement("library_effects");

            foreach (Material material in this.modelPack.Materials.Materials)
            {
                this.libraryEffectsElement.AppendChild(this.CreateEffectXmlElement(material));
            }

            return this.libraryEffectsElement;
        }

        private XmlNode CreateEffectXmlElement(Material material)
        {
            var effectElement = this.document.CreateElement("effect");
            effectElement.SetAttribute("id", material.Name + "-fx");
            effectElement.SetAttribute("name", material.Name);

            var profileCommonElement = this.document.CreateElement("profile_COMMON");

            if (material.Flags.HasFlag(MaterialFlags.HasDiffuseMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "diffuse"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "diffuse"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasNormalMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "normal"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "normal"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasSpecularMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "specular"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "specular"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasReflectionMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "reflection"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "reflection"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasHighlightMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "highlight"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "highlight"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasGlowMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "glow"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "glow"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasNightMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "night"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "night"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasDetailMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "detail"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "detail"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasShadowMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(material.Name, "shadow"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(material.Name, "shadow"));
            }

            var techniqueElement = this.document.CreateElement("technique");
            techniqueElement.SetAttribute("sid", "standard");
            techniqueElement.AppendChild(this.CreatePhongXmlElement(material));

            profileCommonElement.AppendChild(techniqueElement);

            effectElement.AppendChild(profileCommonElement);

            return effectElement;
        }

        private XmlNode CreatePhongXmlElement(Material material)
        {
            var phongElement = this.document.CreateElement("phong");
            phongElement.AppendChild(this.CreatePhongChildXmlElement(material, "emission", "glow", material.EmissiveColor, material.Flags.HasFlag(MaterialFlags.HasGlowMap)));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(material, "ambient", "ambient", material.AmbientColor, false));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(material, "diffuse", "diffuse", material.DiffuseColor, material.Flags.HasFlag(MaterialFlags.HasDiffuseMap)));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(material, "specular", "specular", material.SpecularColor, material.Flags.HasFlag(MaterialFlags.HasSpecularMap)));

            return phongElement;
        }

        private XmlNode CreatePhongChildXmlElement(Material material, string phongChildType, string textureType, Vector4 color, bool useTexture)
        {
            var element = this.document.CreateElement(phongChildType);

            if (useTexture)
            {
                var textureElement = this.document.CreateElement("texture");
                textureElement.SetAttribute("texture", material.Name + "-" + textureType + "-sampler");
                textureElement.SetAttribute("texcoord", "CHANNEL0");

                element.AppendChild(textureElement);
            }
            else
            {
                element.AppendChild(this.CreateColorXmlElement(phongChildType, color));
            }

            return element;
        }

        private XmlNode CreateColorXmlElement(string scopedId, Vector4 color)
        {
            var colorElement = this.document.CreateElement("color");
            colorElement.SetAttribute("sid", scopedId);
            colorElement.AppendChild(this.document.CreateTextNode($"{color.X} {color.Y} {color.Z} {color.W}"));

            return colorElement;
        }

        private XmlNode CreateTextureParamSurfaceXmlElement(string materialName, string textureType)
        {
            var newParamElement = this.document.CreateElement("newparam");
            newParamElement.SetAttribute("sid", materialName + "-" + textureType + "-surface");

            var surfaceElement = this.document.CreateElement("surface");
            surfaceElement.SetAttribute("type", "2D");

            var initFromElement = this.document.CreateElement("init_from");
            initFromElement.AppendChild(this.document.CreateTextNode(materialName + "-" + textureType + "-image"));

            surfaceElement.AppendChild(initFromElement);

            newParamElement.AppendChild(surfaceElement);

            return newParamElement;
        }

        private XmlNode CreateTextureParamSamplerXmlElement(string materialName, string textureType)
        {
            var newParamElement = this.document.CreateElement("newparam");
            newParamElement.SetAttribute("sid", materialName + "-" + textureType + "-sampler");

            var sampler2dElement = this.document.CreateElement("sampler2D");

            var sourceElement = this.document.CreateElement("source");
            sourceElement.AppendChild(this.document.CreateTextNode(materialName + "-" + textureType + "-surface"));

            sampler2dElement.AppendChild(sourceElement);

            newParamElement.AppendChild(sampler2dElement);

            return newParamElement;
        }

        private XmlNode CreateLibraryMaterialsXmlElement()
        {
            this.libraryMaterialsElement = this.document.CreateElement("library_materials");

            foreach (var material in this.modelPack.Materials.Materials)
            {
                var materialElement = this.document.CreateElement("material");
                materialElement.SetAttribute("id", material.Name);
                materialElement.SetAttribute("name", material.Name);

                var instanceEffectElement = this.document.CreateElement("instance_effect");
                instanceEffectElement.SetAttribute("url", "#" + material.Name + "-fx");

                materialElement.AppendChild(instanceEffectElement);

                this.libraryMaterialsElement.AppendChild(materialElement);
            }

            return this.libraryMaterialsElement;
        }

        private XmlNode CreateLibraryGeometriesXmlElement()
        {
            this.libraryGeometriesElement = this.document.CreateElement("library_geometries");

            this.meshes = new List<Mesh>();

            foreach (var node in this.modelPack.Model.Nodes)
            {
                this.meshes.AddRange(node.Meshes);
            }

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var mesh = this.meshes[i];
                string meshId = "meshId" + i;

                this.libraryGeometriesElement.AppendChild(this.CreateGeometryXmlElement(mesh, null, meshId));

                if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
                {
                    for (int j = 0; j < mesh.MorphTargets.Count; j++)
                    {
                        var morphTarget = mesh.MorphTargets[j];

                        if (morphTarget.Vertices.All(x => x == Vector3.Zero))
                        {
                            continue;
                        }

                        string morphTargetId = meshId + "_morphId" + j;

                        this.libraryGeometriesElement.AppendChild(this.CreateGeometryXmlElement(mesh, morphTarget.Vertices, morphTargetId));
                    }
                }
            }

            return this.libraryGeometriesElement;
        }

        private XmlNode CreateGeometryXmlElement(Mesh mesh, List<Vector3> vertexAdditions, string meshId)
        {
            var geometryElement = this.document.CreateElement("geometry");
            geometryElement.SetAttribute("id", meshId);
            geometryElement.SetAttribute("name", meshId);

            var meshElement = this.document.CreateElement("mesh");

            float[] meshValues = mesh.Vertices.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x).ToArray();

            if (vertexAdditions != null)
            {
                float[] vertexAdditionsValues = vertexAdditions.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x).ToArray();

                for (int i = 0; i < meshValues.Length; i++)
                {
                    meshValues[i] += vertexAdditionsValues[i];
                }
            }

            meshElement.AppendChild(this.CreateSourceFloatArrayElement(meshId, "positions", meshValues, 3));

            var normalsValue = mesh.Normals.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x);
            meshElement.AppendChild(this.CreateSourceFloatArrayElement(meshId, "normals", normalsValue, 3));

            if (mesh.TexCoordsChannel0 != null)
            {
                var tex0Value = mesh.TexCoordsChannel0.Select(x => new float[] { x.X, 1f - x.Y }).SelectMany(x => x);
                meshElement.AppendChild(this.CreateSourceFloatArrayElement(meshId, "tex0", tex0Value, 2));
            }

            var verticesElement = this.document.CreateElement("vertices");
            verticesElement.SetAttribute("id", meshId + "-vertices");

            var inputElement = this.document.CreateElement("input");
            inputElement.SetAttribute("semantic", "POSITION");
            inputElement.SetAttribute("source", "#" + meshId + "-positions");

            verticesElement.AppendChild(inputElement);

            meshElement.AppendChild(verticesElement);

            var polylistElement = this.document.CreateElement("polylist");
            polylistElement.SetAttribute("count", mesh.TriangleCount.ToString());
            polylistElement.SetAttribute("material", "defaultMaterial");

            var inputVertexElement = this.document.CreateElement("input");
            inputVertexElement.SetAttribute("offset", "0");
            inputVertexElement.SetAttribute("semantic", "VERTEX");
            inputVertexElement.SetAttribute("source", "#" + meshId + "-vertices");

            polylistElement.AppendChild(inputVertexElement);

            var inputNormalElement = this.document.CreateElement("input");
            inputNormalElement.SetAttribute("offset", "0");
            inputNormalElement.SetAttribute("semantic", "NORMAL");
            inputNormalElement.SetAttribute("source", "#" + meshId + "-normals");

            polylistElement.AppendChild(inputNormalElement);

            if (mesh.TexCoordsChannel0 != null)
            {
                var inputTexcoordElement = this.document.CreateElement("input");
                inputTexcoordElement.SetAttribute("offset", "0");
                inputTexcoordElement.SetAttribute("semantic", "TEXCOORD");
                inputTexcoordElement.SetAttribute("source", "#" + meshId + "-tex0");

                polylistElement.AppendChild(inputTexcoordElement);
            }

            var vcountElement = this.document.CreateElement("vcount");
            vcountElement.AppendChild(this.document.CreateTextNode(String.Join(" ", Enumerable.Repeat(3, mesh.TriangleCount))));

            polylistElement.AppendChild(vcountElement);

            var pElement = this.document.CreateElement("p");
            var pValue = mesh.Triangles.Select(x => new uint[] { x.A, x.B, x.C }).SelectMany(x => x);
            pElement.AppendChild(this.document.CreateTextNode(String.Join(" ", pValue)));

            polylistElement.AppendChild(pElement);

            meshElement.AppendChild(polylistElement);

            geometryElement.AppendChild(meshElement);
            return geometryElement;
        }

        private XmlNode CreateSourceFloatArrayElement(string meshId, string sourceType, IEnumerable<float> values, int stride)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-" + sourceType);
            sourceElement.SetAttribute("name", meshId + "-" + sourceType);

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-" + sourceType + "-array");
            floatArrayElement.SetAttribute("count", values.Count().ToString());
            floatArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", values)));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("count", (values.Count() / stride).ToString());
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-" + sourceType + "-array");
            accessorElement.SetAttribute("stride", stride.ToString());

            if (stride == 2)
            {
                accessorElement.AppendChild(this.CreateParamXmlElement("S", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement("T", "float"));
            }
            else if (stride == 3)
            {
                accessorElement.AppendChild(this.CreateParamXmlElement("X", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement("Y", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement("Z", "float"));
            }

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);

            return sourceElement;
        }

        private XmlNode CreateParamXmlElement(string name, string type)
        {
            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", name);
            paramElement.SetAttribute("type", type);

            return paramElement;
        }

        private XmlNode CreateLibraryControllersXmlElement()
        {
            this.libraryControllersElement = this.document.CreateElement("library_controllers");

            if (this.meshes.Any(x => x.Flags.HasFlag(GeometryFlags.HasVertexWeights) && x.Flags.HasFlag(GeometryFlags.HasMorphTargets)))
            {
                this.blenderOutputNecessary = true;
            }

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var mesh = this.meshes[i];
                string meshId = "meshId" + i;

                var meshNode = this.modelPack.Model.Nodes.First(x => x.Meshes.Contains(mesh)); // this is very crusty.

                if (!mesh.Flags.HasFlag(GeometryFlags.HasVertexWeights))
                {
                    continue;
                }

                var controllerElement = this.document.CreateElement("controller");
                controllerElement.SetAttribute("id", meshId + "-skin");
                controllerElement.SetAttribute("name", meshId + "-skin");

                var skinElement = this.document.CreateElement("skin");

                if (this.blenderMode)
                {
                    // in blender, morph-targets are not per-node, they are per-mesh. this luckily provides a workaround its broken io
                    // despite not being referenced anywhere, the fact that it is attached to the mesh will make every instance of the mesh have the morph targets
                    skinElement.SetAttribute("source", "#" + meshId);
                }
                else
                {
                    if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
                    {
                        skinElement.SetAttribute("source", "#" + meshId + "-morph");
                    }
                    else
                    {
                        skinElement.SetAttribute("source", "#" + meshId);
                    }
                }

                // this is an identity matrix by default. we'll write the element, to be safe
                var bindShapeMatrixElement = this.document.CreateElement("bind_shape_matrix");
                bindShapeMatrixElement.AppendChild(this.document.CreateTextNode(this.GenerateMatrixString(Matrix4x4.Identity)));

                skinElement.AppendChild(bindShapeMatrixElement);

                // adapted from GMDLibrary; restructures the GMD vertex weight format into a more standard bone->vertex weight format
                var boneMap = new Dictionary<int, AssimpBone>();

                for (int j = 0; j < mesh.VertexWeights.Length; j++)
                {
                    var vertexWeight = mesh.VertexWeights[j];

                    for (int k = 0; k < 4; k++)
                    {
                        float boneWeight = vertexWeight.Weights[k];
                        if (boneWeight == 0f)
                        {
                            continue;
                        }

                        byte boneIndex = vertexWeight.Indices[k];
                        ushort nodeIndex = this.modelPack.Model.Bones[boneIndex].NodeIndex;

                        if (!boneMap.ContainsKey(nodeIndex))
                        {
                            var bone = new AssimpBone();
                            var boneNode = this.modelPack.Model.GetNode(nodeIndex);

                            bone.VertexWeights.Add(new AssimpVertexWeight(j, boneWeight));

                            boneMap[nodeIndex] = bone;
                        }
                        else
                        {
                            boneMap[nodeIndex].VertexWeights.Add(new AssimpVertexWeight(j, boneWeight));
                        }
                    }
                }

                var bones = boneMap.Values.ToList();

                skinElement.AppendChild(this.CreateSkinJointsSourceXmlElement(mesh, meshId, out List<Node> joints));
                skinElement.AppendChild(this.CreateSkinBindPosesSourceXmlElement(meshNode, mesh, meshId, out List<float> bindPoses));
                skinElement.AppendChild(this.CreateSkinWeightsSourceXmlElement(bones, meshId));

                var jointsElement = this.document.CreateElement("joints");

                var inputJointsJointElement = this.document.CreateElement("input");
                inputJointsJointElement.SetAttribute("semantic", "JOINT");
                inputJointsJointElement.SetAttribute("source", "#" + meshId + "-skin-joints");

                jointsElement.AppendChild(inputJointsJointElement);

                var inputJointsInvBindMatrixElement = this.document.CreateElement("input");
                inputJointsInvBindMatrixElement.SetAttribute("semantic", "INV_BIND_MATRIX");
                inputJointsInvBindMatrixElement.SetAttribute("source", "#" + meshId + "-skin-bind-poses");

                jointsElement.AppendChild(inputJointsInvBindMatrixElement);

                skinElement.AppendChild(jointsElement);

                var vertexWeightsElement = this.document.CreateElement("vertex_weights");
                vertexWeightsElement.SetAttribute("count", mesh.VertexCount.ToString());

                var inputVertexWeightsJointElement = this.document.CreateElement("input");
                inputVertexWeightsJointElement.SetAttribute("semantic", "JOINT");
                inputVertexWeightsJointElement.SetAttribute("source", "#" + meshId + "-skin-joints");
                inputVertexWeightsJointElement.SetAttribute("offset", "0");

                vertexWeightsElement.AppendChild(inputVertexWeightsJointElement);

                var inputVertexWeightsWeightElement = this.document.CreateElement("input");
                inputVertexWeightsWeightElement.SetAttribute("semantic", "WEIGHT");
                inputVertexWeightsWeightElement.SetAttribute("source", "#" + meshId + "-skin-weights");
                inputVertexWeightsWeightElement.SetAttribute("offset", "1");

                vertexWeightsElement.AppendChild(inputVertexWeightsWeightElement);

                var vcountElement = this.document.CreateElement("vcount");
                string vcountElementText = String.Join(" ", mesh.VertexWeights.Select(x => x.Weights.Count(y => y != 0)));
                vcountElement.AppendChild(this.document.CreateTextNode(vcountElementText));

                vertexWeightsElement.AppendChild(vcountElement);

                var vElement = this.document.CreateElement("v");

                var vElementValues = new List<int>();

                // adapted from Assimp; magically produces proper Collada weights. somehow
                var influences = Enumerable.Repeat(0, mesh.VertexCount).ToList();
                for (int j = 0; j < bones.Count; j++)
                {
                    for (int k = 0; k < bones[j].VertexWeights.Count; ++k)
                    {
                        influences[bones[j].VertexWeights[k].VertexId]++;
                    }
                }

                int jointWeightIndicesLength = 0;
                var accumInfluences = new List<int>();
                for (int j = 0; j < influences.Count; j++)
                {
                    accumInfluences.Add(jointWeightIndicesLength);
                    jointWeightIndicesLength += influences[j];
                }

                int weightIndex = 0;
                var jointWeightIndices = Enumerable.Repeat(-1, 2 * jointWeightIndicesLength).ToList();
                for (int j = 0; j < bones.Count; j++)
                {
                    for (int k = 0; k < bones[j].VertexWeights.Count; ++k)
                    {
                        int vertexId = bones[j].VertexWeights[k].VertexId;
                        for (int l = 0; l < influences[vertexId]; l++)
                        {
                            if (jointWeightIndices[2 * (accumInfluences[vertexId] + l)] == -1)
                            {
                                jointWeightIndices[2 * (accumInfluences[vertexId] + l)] = j;
                                jointWeightIndices[(2 * (accumInfluences[vertexId] + l)) + 1] = weightIndex;
                                break;
                            }
                        }
                        weightIndex++;
                    }
                }

                vElementValues.AddRange(jointWeightIndices);

                vElement.AppendChild(this.document.CreateTextNode(String.Join(" ", vElementValues)));

                vertexWeightsElement.AppendChild(vElement);

                skinElement.AppendChild(vertexWeightsElement);

                controllerElement.AppendChild(skinElement);

                this.libraryControllersElement.AppendChild(controllerElement);
            }

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var mesh = this.meshes[i];
                string meshId = "meshId" + i;

                if (!mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
                {
                    continue;
                }

                var controllerElement = this.document.CreateElement("controller");
                controllerElement.SetAttribute("id", meshId + "-morph");
                controllerElement.SetAttribute("name", meshId + "-morph");

                var morphElement = this.document.CreateElement("morph");
                morphElement.SetAttribute("source", "#" + meshId);
                morphElement.SetAttribute("method", "NORMALIZED");
                morphElement.AppendChild(this.CreateMorphTargetsSourceXmlElement(mesh, meshId));
                morphElement.AppendChild(this.CreateMorphWeightsSourceXmlElement(mesh, meshId));

                var targetsElement = this.document.CreateElement("targets");

                var inputTargetElement = this.document.CreateElement("input");
                inputTargetElement.SetAttribute("semantic", "MORPH_TARGET");
                inputTargetElement.SetAttribute("source", "#" + meshId + "-targets");

                targetsElement.AppendChild(inputTargetElement);

                var inputWeightElement = this.document.CreateElement("input");
                inputWeightElement.SetAttribute("semantic", "MORPH_WEIGHT");
                inputWeightElement.SetAttribute("source", "#" + meshId + "-weights");

                targetsElement.AppendChild(inputWeightElement);

                morphElement.AppendChild(targetsElement);

                controllerElement.AppendChild(morphElement);

                this.libraryControllersElement.AppendChild(controllerElement);
            }

            return this.libraryControllersElement;
        }

        private XmlNode CreateSkinJointsSourceXmlElement(Mesh mesh, string meshId, out List<Node> joints)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-joints");
            sourceElement.SetAttribute("name", meshId + "-skin-joints");

            var nameArrayElement = this.document.CreateElement("Name_array"); // i don't know why this is Name instead of something else like IDREF, but i'm not gonna change it, to be safe
            nameArrayElement.SetAttribute("id", meshId + "-skin-joints-array");

            joints = new List<Node>();

            for (int i = 0; i < mesh.VertexWeights.Length; i++)
            {
                var vertexWeight = mesh.VertexWeights[i];

                for (int j = 0; j < 4; j++)
                {
                    float boneWeight = vertexWeight.Weights[j];
                    if (boneWeight == 0)
                    {
                        continue;
                    }

                    byte boneIndex = vertexWeight.Indices[j];
                    ushort nodeIndex = this.modelPack.Model.Bones[boneIndex].NodeIndex;

                    var boneNode = this.modelPack.Model.GetNode(nodeIndex);

                    if (!joints.Contains(boneNode))
                    {
                        joints.Add(boneNode);
                    }
                }
            }

            nameArrayElement.SetAttribute("count", joints.Count.ToString());
            nameArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", joints.Select(x => x.Name))));

            sourceElement.AppendChild(nameArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-joints-array");
            accessorElement.SetAttribute("count", joints.Count.ToString());
            accessorElement.SetAttribute("stride", "1");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "JOINT");
            paramElement.SetAttribute("type", "Name");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);

            return sourceElement;
        }

        private XmlNode CreateSkinBindPosesSourceXmlElement(Node meshNode, Mesh mesh, string meshId, out List<float> bindPoses)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-bind-poses");
            sourceElement.SetAttribute("name", meshId + "-skin-bind-poses");

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-skin-bind-poses-array");

            var bindPosesNodes = new List<Node>();

            for (int i = 0; i < mesh.VertexWeights.Length; i++)
            {
                var vertexWeight = mesh.VertexWeights[i];

                for (int j = 0; j < 4; j++)
                {
                    float boneWeight = vertexWeight.Weights[j];
                    if (boneWeight == 0)
                    {
                        continue;
                    }

                    byte boneIndex = vertexWeight.Indices[j];
                    ushort nodeIndex = this.modelPack.Model.Bones[boneIndex].NodeIndex;

                    var boneNode = this.modelPack.Model.GetNode(nodeIndex);

                    if (!bindPosesNodes.Contains(boneNode))
                    {
                        bindPosesNodes.Add(boneNode);
                    }
                }
            }

            floatArrayElement.SetAttribute("count", (bindPosesNodes.Count * 16).ToString()); // 4x4 matrix contains 16 values. obvs.

            bindPoses = bindPosesNodes.SelectMany(x =>
            {
                // literally no idea what this does or why but it works
                Matrix4x4.Invert(meshNode.WorldTransform, out Matrix4x4 invertedMeshNodeWorldTransform);
                Matrix4x4.Invert(x.WorldTransform * invertedMeshNodeWorldTransform, out Matrix4x4 offsetMatrix);
                return this.GenerateMatrixArray(offsetMatrix);
            }).ToList();

            floatArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", bindPoses)));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("count", bindPosesNodes.Count.ToString());
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-bind-poses-array");
            accessorElement.SetAttribute("stride", "16");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "TRANSFORM");
            paramElement.SetAttribute("type", "float4x4");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);

            return sourceElement;
        }

        private XmlNode CreateSkinWeightsSourceXmlElement( List<AssimpBone> bones, string meshId)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-weights");
            sourceElement.SetAttribute("name", meshId + "-skin-weights");

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-skin-weights-array");

            var floatArrayElementValues = new List<float>();

            for (int i = 0; i < bones.Count; ++i)
            {
                for (int j = 0; j < bones[i].VertexWeights.Count; j++)
                {
                    floatArrayElementValues.Add(bones[i].VertexWeights[j].Weight);
                }
            }

            floatArrayElement.SetAttribute("count", floatArrayElementValues.Count.ToString());
            floatArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", floatArrayElementValues)));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("count", floatArrayElementValues.Count.ToString());
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-weights-array");
            accessorElement.SetAttribute("stride", "1");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "WEIGHT");
            paramElement.SetAttribute("type", "float");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);

            return sourceElement;
        }

        private XmlNode CreateMorphTargetsSourceXmlElement(Mesh mesh, string meshId)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-targets");

            var morphTargetNames = new List<string>();

            for (int i = 0; i < mesh.MorphTargets.Count; i++)
            {
                if (mesh.MorphTargets[i].Vertices.All(x => x == Vector3.Zero))
                {
                    continue;
                }

                string morphTargetId = "morphId" + i;
                morphTargetNames.Add(meshId + "_" + morphTargetId);
            }

            var idrefArrayElement = this.document.CreateElement("IDREF_array");
            idrefArrayElement.SetAttribute("id", meshId + "-targets-array");
            idrefArrayElement.SetAttribute("count", morphTargetNames.Count.ToString());

            idrefArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", morphTargetNames)));

            sourceElement.AppendChild(idrefArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-targets-array");
            accessorElement.SetAttribute("count", morphTargetNames.Count.ToString());
            accessorElement.SetAttribute("stride", "1");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "IDREF");
            paramElement.SetAttribute("type", "IDREF");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateMorphWeightsSourceXmlElement(Mesh mesh, string meshId)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-weights");

            int morphTargetCount = 0;

            foreach (MorphTarget morphTarget in mesh.MorphTargets)
            {
                if (!morphTarget.Vertices.All(x => x == Vector3.Zero))
                {
                    morphTargetCount++;
                }
            }

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-weights-array");
            floatArrayElement.SetAttribute("count", morphTargetCount.ToString());
            floatArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", Enumerable.Repeat("0", morphTargetCount))));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-weights-array");
            accessorElement.SetAttribute("count", morphTargetCount.ToString());
            accessorElement.SetAttribute("stride", "1");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "WEIGHT");
            paramElement.SetAttribute("type", "float");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateLibraryVisualScenesXmlElement()
        {
            var libraryVisualScenesElement = this.document.CreateElement("library_visual_scenes");

            this.visualSceneElement = this.document.CreateElement("visual_scene");
            this.visualSceneElement.SetAttribute("id", "RootNode");
            this.visualSceneElement.SetAttribute("name", "RootNode");

            this.RecurseNodeChildren(this.visualSceneElement, this.modelPack.Model.RootNode);

            libraryVisualScenesElement.AppendChild(this.visualSceneElement);

            return libraryVisualScenesElement;
        }

        private void RecurseNodeChildren(XmlElement parentElement, Node parent)
        {
            var bones = this.modelPack.Model.Bones;

            Bone parentNodeBone = null;
            if (bones != null)
            {
                foreach (var bone in bones)
                {
                    if (parent == this.modelPack.Model.GetNode(bone.NodeIndex))
                    {
                        parentNodeBone = bone;
                        break;
                    }
                }
            }

            foreach (var node in parent.Children)
            {
                Bone nodeBone = null;
                if (bones != null)
                {
                    foreach (var bone in bones)
                    {
                        if (node == this.modelPack.Model.GetNode(bone.NodeIndex))
                        {
                            nodeBone = bone;
                            break;
                        }
                    }
                }

                if (this.options.IgnoreEmptyNodes && !node.HasAttachments && !node.HasChildren && nodeBone == null)
                {
                    // useless garbage trash boy node
                    continue;
                }

                var nodeElement = this.document.CreateElement("node");
                nodeElement.SetAttribute("id", node.Name);
                nodeElement.SetAttribute("sid", node.Name); // not sure when this is unnecessary, so we'll always include it. doesn't seem to cause problems
                nodeElement.SetAttribute("name", node.Name);

                if (nodeBone == null)
                {
                    nodeElement.SetAttribute("type", "NODE");
                }
                else
                {
                    nodeElement.SetAttribute("type", "JOINT");
                }

                var matrixElement = this.document.CreateElement("matrix");
                matrixElement.SetAttribute("sid", "matrix");

                matrixElement.AppendChild(this.document.CreateTextNode(this.GenerateMatrixString(node.LocalTransform)));

                nodeElement.AppendChild(matrixElement);

                bool skipNode = false;

                for (int i = 0; i < node.Attachments.Count; i++)
                {
                    var attachment = node.Attachments[i];

                    switch (attachment.Type)
                    {
                        case NodeAttachmentType.Mesh:
                            var mesh = attachment.GetValue<Mesh>();

                            if (parentNodeBone != null)
                            {
                                // uh oh! a mesh is attached to a joint.
                                // blender doesn't like this, so we'll have to do some tomfoolery to place it elsewhere while still having it be affected
                                this.blenderOutputNecessary = true;

                                if (this.blenderMode)
                                {
                                    this.HandleJointMeshAttachment(mesh, parent, node);
                                    skipNode = true;
                                    // we'll assume that there are no other attachments. please. 
                                    break;
                                }
                            }

                            if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets) || mesh.Flags.HasFlag(GeometryFlags.HasVertexWeights))
                            {
                                nodeElement.AppendChild(this.CreateInstanceControllerXmlElement(mesh));
                            }
                            else
                            {
                                nodeElement.AppendChild(this.CreateInstanceGeometryXmlElement(mesh));
                            }
                            break;
                        case NodeAttachmentType.Morph:
                            // morph attachment specify the default weights for a morph target
                            // i don't want to deal with these, so they're all set to zero. that'll be fine, right? right???
                            break;
                        default:
                            //throw new NotImplementedException();
                            break;
                    }
                }


                if (!skipNode)
                {
                    this.RecurseNodeChildren(nodeElement, node);

                    parentElement.AppendChild(nodeElement);
                }
            }
        }

        private void HandleJointMeshAttachment(Mesh mesh, Node parent, Node node)
        {
            // i am going to make two assumptions here.
            // the first is that the mesh does not have vertex weights. if it does, then why would it be here...?

            if (mesh.Flags.HasFlag(GeometryFlags.HasVertexWeights))
            {
                throw new ApplicationException("A joint mesh attachment has vertex weights.");
            }

            // the second is that the mesh does not have morph targets. if it does, i will literally vape my own brain.

            if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
            {
                throw new ApplicationException("A joint mesh attachment has morph targets. See, Poyo, you idiot? You thought this would never happen and it did. You absolute buffoon.");
            }

            string meshId = null;
            for (int i = 0; i < this.meshes.Count; i++)
            {
                if (mesh == this.meshes[i])
                {
                    meshId = "meshId" + i;
                }
            }

            if (meshId == null)
            {
                throw new ApplicationException("Joint mesh attachment is somehow not in the list of meshes!!! What in tarnation!!!");
            }

            // first create a controller

            var controllerElement = this.document.CreateElement("controller");
            controllerElement.SetAttribute("id", meshId + "-skin");
            controllerElement.SetAttribute("name", meshId + "-skin");

            var skinElement = this.document.CreateElement("skin");

            skinElement.SetAttribute("source", "#" + meshId);

            var bindShapeMatrixElement = this.document.CreateElement("bind_shape_matrix");
            bindShapeMatrixElement.AppendChild(this.document.CreateTextNode(this.GenerateMatrixString(node.WorldTransform)));

            skinElement.AppendChild(bindShapeMatrixElement);

            var meshNode = this.modelPack.Model.Nodes.First(x => x.Meshes.Contains(mesh));

            skinElement.AppendChild(this.CreateJointMeshAttachmentSkinJointsSourceXmlElement(parent, meshId));
            skinElement.AppendChild(this.CreateJointMeshAttachmentSkinBindPosesSourceXmlElement(node, meshNode, meshId));
            skinElement.AppendChild(this.CreateJointMeshAttachmentSkinWeightsSourceXmlElement(mesh, meshId));

            var jointsElement = this.document.CreateElement("joints");

            var inputJointsJointElement = this.document.CreateElement("input");
            inputJointsJointElement.SetAttribute("semantic", "JOINT");
            inputJointsJointElement.SetAttribute("source", "#" + meshId + "-skin-joints");

            jointsElement.AppendChild(inputJointsJointElement);

            var inputJointsInvBindMatrixElement = this.document.CreateElement("input");
            inputJointsInvBindMatrixElement.SetAttribute("semantic", "INV_BIND_MATRIX");
            inputJointsInvBindMatrixElement.SetAttribute("source", "#" + meshId + "-skin-bind-poses");

            jointsElement.AppendChild(inputJointsInvBindMatrixElement);

            skinElement.AppendChild(jointsElement);

            var vertexWeightsElement = this.document.CreateElement("vertex_weights");
            vertexWeightsElement.SetAttribute("count", mesh.VertexCount.ToString());

            var inputVertexWeightsJointElement = this.document.CreateElement("input");
            inputVertexWeightsJointElement.SetAttribute("semantic", "JOINT");
            inputVertexWeightsJointElement.SetAttribute("source", "#" + meshId + "-skin-joints");
            inputVertexWeightsJointElement.SetAttribute("offset", "0");

            vertexWeightsElement.AppendChild(inputVertexWeightsJointElement);

            var inputVertexWeightsWeightElement = this.document.CreateElement("input");
            inputVertexWeightsWeightElement.SetAttribute("semantic", "WEIGHT");
            inputVertexWeightsWeightElement.SetAttribute("source", "#" + meshId + "-skin-weights");
            inputVertexWeightsWeightElement.SetAttribute("offset", "1");

            vertexWeightsElement.AppendChild(inputVertexWeightsWeightElement);

            var vcountElement = this.document.CreateElement("vcount");
            vcountElement.AppendChild(this.document.CreateTextNode(String.Join(" ", Enumerable.Repeat(1, mesh.VertexCount))));

            vertexWeightsElement.AppendChild(vcountElement);

            var vElement = this.document.CreateElement("v");

            var vElementValues = new List<int>();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                vElementValues.Add(0); // bone[0];
                vElementValues.Add(i); // weight[i]
            }

            vElement.AppendChild(this.document.CreateTextNode(String.Join(" ", vElementValues)));

            vertexWeightsElement.AppendChild(vElement);

            skinElement.AppendChild(vertexWeightsElement);

            controllerElement.AppendChild(skinElement);

            this.libraryControllersElement.AppendChild(controllerElement);

            // now we instantiate the controller in the scene manually.

            var nodeElement = this.document.CreateElement("node");
            nodeElement.SetAttribute("id", node.Name);
            nodeElement.SetAttribute("sid", node.Name);
            nodeElement.SetAttribute("name", node.Name);

            nodeElement.SetAttribute("type", "NODE");

            var matrixElement = this.document.CreateElement("matrix");
            matrixElement.SetAttribute("sid", "matrix");

            matrixElement.AppendChild(this.document.CreateTextNode(this.GenerateMatrixString(node.LocalTransform)));

            nodeElement.AppendChild(matrixElement);

            var instanceController = this.document.CreateElement("instance_controller");
            instanceController.SetAttribute("url", "#" + meshId + "-skin");

            var skeletonElement = this.document.CreateElement("skeleton");
            skeletonElement.AppendChild(this.document.CreateTextNode("#root"));

            instanceController.AppendChild(skeletonElement);

            var bindMaterialElement = this.document.CreateElement("bind_material");

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var instanceMaterialElement = this.document.CreateElement("instance_material");
            instanceMaterialElement.SetAttribute("symbol", "defaultMaterial");
            instanceMaterialElement.SetAttribute("target", "#" + mesh.MaterialName);

            if (mesh.TexCoordsChannel0 != null)
            {
                var bindVertexInputElement = this.document.CreateElement("bind_vertex_input");
                bindVertexInputElement.SetAttribute("semantic", "CHANNEL0");
                bindVertexInputElement.SetAttribute("input_semantic", "TEXCOORD");
                bindVertexInputElement.SetAttribute("input_set", "0");

                instanceMaterialElement.AppendChild(bindVertexInputElement);
            }

            techniqueCommonElement.AppendChild(instanceMaterialElement);

            bindMaterialElement.AppendChild(techniqueCommonElement);

            instanceController.AppendChild(bindMaterialElement);

            nodeElement.AppendChild(instanceController);

            this.visualSceneElement.AppendChild(nodeElement);
        }

        private XmlNode CreateJointMeshAttachmentSkinJointsSourceXmlElement(Node parentBoneNode, string meshId)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-joints");
            sourceElement.SetAttribute("name", meshId + "-skin-joints");

            var nameArrayElement = this.document.CreateElement("Name_array");
            nameArrayElement.SetAttribute("id", meshId + "-skin-joints-array");
            nameArrayElement.SetAttribute("count", "1");
            nameArrayElement.AppendChild(this.document.CreateTextNode(parentBoneNode.Name));

            sourceElement.AppendChild(nameArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-joints-array");
            accessorElement.SetAttribute("count", "1");
            accessorElement.SetAttribute("stride", "1");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "JOINT");
            paramElement.SetAttribute("type", "Name");

            techniqueCommonElement.AppendChild(accessorElement);

            accessorElement.AppendChild(paramElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateJointMeshAttachmentSkinBindPosesSourceXmlElement(Node parentBoneNode, Node meshNode, string meshId)
        {
            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-bind-poses");
            sourceElement.SetAttribute("name", meshId + "-skin-bind-poses");

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-skin-bind-poses-array");
            floatArrayElement.SetAttribute("count", "16");

            Matrix4x4.Invert(meshNode.WorldTransform, out Matrix4x4 invertedMeshNodeWorldTransform);
            Matrix4x4.Invert(parentBoneNode.WorldTransform * invertedMeshNodeWorldTransform, out Matrix4x4 offsetMatrix);
            floatArrayElement.AppendChild(this.document.CreateTextNode(this.GenerateMatrixString(offsetMatrix)));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("count", "1");
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-bind-poses-array");
            accessorElement.SetAttribute("stride", "16");

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "TRANSFORM");
            paramElement.SetAttribute("type", "float4x4");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateJointMeshAttachmentSkinWeightsSourceXmlElement(Mesh mesh, string meshId)
        {

            var sourceElement = this.document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-skin-weights");
            sourceElement.SetAttribute("name", meshId + "-skin-weights");

            var floatArrayElement = this.document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-skin-weights-array");
            floatArrayElement.SetAttribute("count", mesh.VertexCount.ToString());

            floatArrayElement.AppendChild(this.document.CreateTextNode(String.Join(" ", Enumerable.Repeat(1, mesh.VertexCount))));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var accessorElement = this.document.CreateElement("accessor");
            accessorElement.SetAttribute("count", mesh.VertexCount.ToString());
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-skin-weights-array");
            accessorElement.SetAttribute("stride", "1");

            techniqueCommonElement.AppendChild(accessorElement);

            var paramElement = this.document.CreateElement("param");
            paramElement.SetAttribute("name", "WEIGHT");
            paramElement.SetAttribute("type", "float");

            accessorElement.AppendChild(paramElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateInstanceControllerXmlElement(Mesh mesh)
        {
            int meshIndex = this.meshes.IndexOf(mesh);
            string meshId = "meshId" + meshIndex;

            var instanceControllerElement = this.document.CreateElement("instance_controller");
            if (mesh.Flags.HasFlag(GeometryFlags.HasVertexWeights))
            {
                instanceControllerElement.SetAttribute("url", "#" + meshId + "-skin");
            }
            else if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
            {
                instanceControllerElement.SetAttribute("url", "#" + meshId + "-morph");
            }
            else
            {
                // should never happen
                throw new ApplicationException("CreateInstanceControllerXmlElement was called for a mesh that has neither vertex weights nor morph targets.");
            }

            if (mesh.Flags.HasFlag(GeometryFlags.HasVertexWeights))
            {
                var skeletonElement = this.document.CreateElement("skeleton");
                skeletonElement.AppendChild(this.document.CreateTextNode("#root")); // more like doot haha

                instanceControllerElement.AppendChild(skeletonElement);
            }

            var bindMaterialElement = this.document.CreateElement("bind_material");

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var instanceMaterialElement = this.document.CreateElement("instance_material");
            instanceMaterialElement.SetAttribute("symbol", "defaultMaterial");
            instanceMaterialElement.SetAttribute("target", "#" + mesh.MaterialName);

            if (mesh.TexCoordsChannel0 != null)
            {
                var bindVertexInputElement = this.document.CreateElement("bind_vertex_input");
                bindVertexInputElement.SetAttribute("semantic", "CHANNEL0");
                bindVertexInputElement.SetAttribute("input_semantic", "TEXCOORD");
                bindVertexInputElement.SetAttribute("input_set", "0");

                instanceMaterialElement.AppendChild(bindVertexInputElement);
            }

            techniqueCommonElement.AppendChild(instanceMaterialElement);

            bindMaterialElement.AppendChild(techniqueCommonElement);

            instanceControllerElement.AppendChild(bindMaterialElement);

            return instanceControllerElement;
        }

        private XmlNode CreateInstanceGeometryXmlElement(Mesh mesh)
        {
            int meshIndex = this.meshes.IndexOf(mesh);
            string meshId = "meshId" + meshIndex;

            var instanceGeometryElement = this.document.CreateElement("instance_geometry");
            instanceGeometryElement.SetAttribute("url", "#" + meshId);

            var bindMaterialElement = this.document.CreateElement("bind_material");

            var techniqueCommonElement = this.document.CreateElement("technique_common");

            var instanceMaterialElement = this.document.CreateElement("instance_material");
            instanceMaterialElement.SetAttribute("symbol", "defaultMaterial");
            instanceMaterialElement.SetAttribute("target", "#" + mesh.MaterialName);

            if (mesh.TexCoordsChannel0 != null)
            {
                var bindVertexInputElement = this.document.CreateElement("bind_vertex_input");
                bindVertexInputElement.SetAttribute("semantic", "CHANNEL0");
                bindVertexInputElement.SetAttribute("input_semantic", "TEXCOORD");
                bindVertexInputElement.SetAttribute("input_set", "0");

                instanceMaterialElement.AppendChild(bindVertexInputElement);
            }

            techniqueCommonElement.AppendChild(instanceMaterialElement);

            bindMaterialElement.AppendChild(techniqueCommonElement);

            instanceGeometryElement.AppendChild(bindMaterialElement);

            return instanceGeometryElement;
        }

        private XmlNode CreateSceneXmlElement()
        {
            var sceneElement = this.document.CreateElement("scene");

            var instanceVisualSceneElement = this.document.CreateElement("instance_visual_scene");
            instanceVisualSceneElement.SetAttribute("url", "#RootNode");

            sceneElement.AppendChild(instanceVisualSceneElement);

            return sceneElement;
        }

        private void ExportTextures()
        {
            Directory.CreateDirectory(this.textureFolder);

            foreach (Texture texture in this.modelPack.Textures.Textures)
            {
                File.WriteAllBytes(Path.Combine(this.textureFolder, texture.Name), texture.Data);
            }
        }

        private string GenerateMatrixString(Matrix4x4 matrix)
        {
            return
                $"{matrix.M11} {matrix.M21} {matrix.M31} {matrix.M41} " +
                $"{matrix.M12} {matrix.M22} {matrix.M32} {matrix.M42} " +
                $"{matrix.M13} {matrix.M23} {matrix.M33} {matrix.M43} " +
                $"{matrix.M14} {matrix.M24} {matrix.M34} {matrix.M44}";
        }

        private float[] GenerateMatrixArray(Matrix4x4 matrix)
        {
            return new float[]
            {
                matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                matrix.M14, matrix.M24, matrix.M34, matrix.M44
            };
        }
    }
}
