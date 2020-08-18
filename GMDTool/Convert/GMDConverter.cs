using GFDLibrary;
using GFDLibrary.Materials;
using GFDLibrary.Models;
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
        private readonly ModelPack modelPack;
        private readonly string outputPath;
        private readonly string textureFolderName;
        private readonly string textureFolder;

        private List<Mesh> meshes;

        private GMDConverter(ModelPack modelPack, string outputPath)
        {
            this.modelPack = modelPack;
            this.outputPath = outputPath;
            this.textureFolderName = $@"{Path.GetFileNameWithoutExtension(outputPath)}_Textures";
            this.textureFolder = Path.Combine(Path.GetDirectoryName(outputPath), this.textureFolderName);
        }

        public static void Export(string input, string output)
        {
            ModelPack modelPack = Resource.Load<ModelPack>(input);

            var converter = new GMDConverter(modelPack, output);

            converter.Export();
        }

        private void Export()
        {
            this.GenerateXml();
            this.ExportTextures();
        }

        private void GenerateXml()
        {
            var document = new XmlDocument();

            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            var rootElement = document.CreateElement("COLLADA");
            rootElement.SetAttribute("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            rootElement.SetAttribute("version", "1.4.1");

            rootElement.AppendChild(this.CreateAssetXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryImagesXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryEffectsXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryMaterialsXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryGeometriesXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryControllersXmlElement(document));
            rootElement.AppendChild(this.CreateLibraryVisualScenesXmlElement(document));
            rootElement.AppendChild(this.CreateSceneXmlElement(document));

            document.AppendChild(rootElement);

            document.Save(Path.Combine(this.outputPath));
        }

        private XmlNode CreateAssetXmlElement(XmlDocument document)
        {
            var assetElement = document.CreateElement("asset");

            var contributorElement = document.CreateElement("contributor");

            var authorElement = document.CreateElement("author");
            authorElement.AppendChild(document.CreateTextNode("Poyo"));
            contributorElement.AppendChild(authorElement);

            var authoringToolElement = document.CreateElement("authoring_tool");
            authoringToolElement.AppendChild(document.CreateTextNode("GMDTool"));
            contributorElement.AppendChild(authoringToolElement);

            assetElement.AppendChild(contributorElement);

            var CreatedXmlElement = document.CreateElement("created");
            CreatedXmlElement.AppendChild(document.CreateTextNode(DateTime.Now.ToString("s")));
            assetElement.AppendChild(CreatedXmlElement);

            var modifiedElement = document.CreateElement("modified");
            modifiedElement.AppendChild(document.CreateTextNode(DateTime.Now.ToString("s")));
            assetElement.AppendChild(modifiedElement);

            var unitElement = document.CreateElement("unit");
            unitElement.SetAttribute("name", "meter");
            unitElement.SetAttribute("meter", "1");
            assetElement.AppendChild(unitElement);

            var upAxisElement = document.CreateElement("up_axis");
            upAxisElement.AppendChild(document.CreateTextNode("Y_UP"));
            assetElement.AppendChild(upAxisElement);

            return assetElement;
        }

        private XmlNode CreateLibraryImagesXmlElement(XmlDocument document)
        {
            var libraryImagesElement = document.CreateElement("library_images");

            foreach (Material material in this.modelPack.Materials.Materials)
            {
                if (material.Flags.HasFlag(MaterialFlags.HasDiffuseMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "diffuse", material.DiffuseMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasNormalMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "normal", material.NormalMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasSpecularMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "specular", material.SpecularMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasReflectionMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "reflection", material.ReflectionMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasHighlightMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "highlight", material.HighlightMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasGlowMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "glow", material.GlowMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasNightMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "night", material.NightMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasDetailMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "detail", material.DetailMap));
                }
                if (material.Flags.HasFlag(MaterialFlags.HasShadowMap))
                {
                    libraryImagesElement.AppendChild(this.CreateImageXmlElement(document, material.Name, "shadow", material.ShadowMap));
                }
            }

            return libraryImagesElement;
        }

        private XmlNode CreateImageXmlElement(XmlDocument document, string materialName, string textureMapType, TextureMap textureMap)
        {
            var imageElement = document.CreateElement("image");
            imageElement.SetAttribute("id", materialName + "-" + textureMapType + "-image");

            var initFromElement = document.CreateElement("init_from");
            initFromElement.AppendChild(document.CreateTextNode(Path.Combine(this.textureFolderName, textureMap.Name)));

            imageElement.AppendChild(initFromElement);

            return imageElement;
        }

        private XmlNode CreateLibraryEffectsXmlElement(XmlDocument document)
        {
            var libraryEffectsElement = document.CreateElement("library_effects");

            foreach (Material material in this.modelPack.Materials.Materials)
            {
                libraryEffectsElement.AppendChild(this.CreateEffectXmlElement(document, material));
            }

            return libraryEffectsElement;
        }

        private XmlNode CreateEffectXmlElement(XmlDocument document, Material material)
        {
            var effectElement = document.CreateElement("effect");
            effectElement.SetAttribute("id", material.Name + "-fx");
            effectElement.SetAttribute("name", material.Name);

            var profileCommonElement = document.CreateElement("profile_COMMON");

            if (material.Flags.HasFlag(MaterialFlags.HasDiffuseMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "diffuse"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "diffuse"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasNormalMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "normal"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "normal"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasSpecularMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "specular"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "specular"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasReflectionMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "reflection"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "reflection"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasHighlightMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "highlight"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "highlight"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasGlowMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "glow"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "glow"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasNightMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "night"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "night"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasDetailMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "detail"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "detail"));
            }
            if (material.Flags.HasFlag(MaterialFlags.HasShadowMap))
            {
                profileCommonElement.AppendChild(this.CreateTextureParamSurfaceXmlElement(document, material.Name, "shadow"));
                profileCommonElement.AppendChild(this.CreateTextureParamSamplerXmlElement(document, material.Name, "shadow"));
            }

            var techniqueElement = document.CreateElement("technique");
            techniqueElement.SetAttribute("sid", "standard");
            techniqueElement.AppendChild(this.CreatePhongXmlElement(document, material));

            profileCommonElement.AppendChild(techniqueElement);

            effectElement.AppendChild(profileCommonElement);

            return effectElement;
        }

        private XmlNode CreatePhongXmlElement(XmlDocument document, Material material)
        {
            var phongElement = document.CreateElement("phong");
            phongElement.AppendChild(this.CreatePhongChildXmlElement(document, material, "emission", "glow", material.EmissiveColor, material.Flags.HasFlag(MaterialFlags.HasGlowMap)));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(document, material, "ambient", "ambient", material.AmbientColor, false));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(document, material, "diffuse", "diffuse", material.DiffuseColor, material.Flags.HasFlag(MaterialFlags.HasDiffuseMap)));
            phongElement.AppendChild(this.CreatePhongChildXmlElement(document, material, "specular", "specular", material.SpecularColor, material.Flags.HasFlag(MaterialFlags.HasSpecularMap)));

            return phongElement;
        }

        private XmlNode CreatePhongChildXmlElement(XmlDocument document, Material material, string phongChildType, string textureType, Vector4 color, bool useTexture)
        {
            var element = document.CreateElement(phongChildType);

            if (useTexture)
            {
                var textureElement = document.CreateElement("texture");
                textureElement.SetAttribute("texture", material.Name + "-" + textureType + "-sampler");
                textureElement.SetAttribute("texcoord", "CHANNEL0");

                element.AppendChild(textureElement);
            }
            else
            {
                element.AppendChild(this.CreateColorXmlElement(document, phongChildType, color));
            }

            return element;
        }

        private XmlNode CreateColorXmlElement(XmlDocument document, string scopedId, Vector4 color)
        {
            var colorElement = document.CreateElement("color");
            colorElement.SetAttribute("sid", scopedId);
            colorElement.AppendChild(document.CreateTextNode($"{color.X} {color.Y} {color.Z} {color.W}"));

            return colorElement;
        }

        private XmlNode CreateTextureParamSurfaceXmlElement(XmlDocument document, string materialName, string textureType)
        {
            var newParamElement = document.CreateElement("newparam");
            newParamElement.SetAttribute("sid", materialName + "-" + textureType + "-surface");

            var surfaceElement = document.CreateElement("surface");
            surfaceElement.SetAttribute("type", "2D");

            var initFromElement = document.CreateElement("init_from");
            initFromElement.AppendChild(document.CreateTextNode(materialName + "-" + textureType + "-image"));

            surfaceElement.AppendChild(initFromElement);

            newParamElement.AppendChild(surfaceElement);

            return newParamElement;
        }

        private XmlNode CreateTextureParamSamplerXmlElement(XmlDocument document, string materialName, string textureType)
        {
            var newParamElement = document.CreateElement("newparam");
            newParamElement.SetAttribute("sid", materialName + "-" + textureType + "-sampler");

            var sampler2dElement = document.CreateElement("sampler2D");

            var sourceElement = document.CreateElement("source");
            sourceElement.AppendChild(document.CreateTextNode(materialName + "-" + textureType + "-surface"));

            sampler2dElement.AppendChild(sourceElement);

            newParamElement.AppendChild(sampler2dElement);

            return newParamElement;
        }

        private XmlNode CreateLibraryMaterialsXmlElement(XmlDocument document)
        {
            var libraryMaterialsElement = document.CreateElement("library_materials");

            foreach (var material in this.modelPack.Materials.Materials)
            {
                var materialElement = document.CreateElement("material");
                materialElement.SetAttribute("id", material.Name);
                materialElement.SetAttribute("name", material.Name);

                var instanceEffectElement = document.CreateElement("instance_effect");
                instanceEffectElement.SetAttribute("url", "#" + material.Name + "-fx");

                materialElement.AppendChild(instanceEffectElement);

                libraryMaterialsElement.AppendChild(materialElement);
            }

            return libraryMaterialsElement;
        }

        private XmlNode CreateLibraryGeometriesXmlElement(XmlDocument document)
        {
            var libraryGeometriesElement = document.CreateElement("library_geometries");

            this.meshes = new List<Mesh>();

            foreach (var node in this.modelPack.Model.Nodes)
            {
                this.meshes.AddRange(node.Meshes);
            }

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var mesh = this.meshes[i];
                string meshId = "meshId" + i;

                libraryGeometriesElement.AppendChild(this.CreateGeometryXmlElement(document, mesh, null, meshId));

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

                        libraryGeometriesElement.AppendChild(this.CreateGeometryXmlElement(document, mesh, morphTarget.Vertices, morphTargetId));
                    }
                }
            }

            return libraryGeometriesElement;
        }

        private XmlNode CreateGeometryXmlElement(XmlDocument document, Mesh mesh, List<Vector3> vertexAdditions, string meshId)
        {
            var geometryElement = document.CreateElement("geometry");
            geometryElement.SetAttribute("id", meshId);
            geometryElement.SetAttribute("name", meshId);

            var meshElement = document.CreateElement("mesh");

            float[] meshValues = mesh.Vertices.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x).ToArray();

            if (vertexAdditions != null)
            {
                float[] vertexAdditionsValues = vertexAdditions.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x).ToArray();

                for (int i = 0; i < meshValues.Length; i++)
                {
                    meshValues[i] += vertexAdditionsValues[i];
                }
            }

            meshElement.AppendChild(this.CreateSourceFloatArrayElement(document, meshId, "positions", meshValues, 3));

            var normalsValue = mesh.Normals.Select(x => new float[] { x.X, x.Y, x.Z }).SelectMany(x => x);
            meshElement.AppendChild(this.CreateSourceFloatArrayElement(document, meshId, "normals", normalsValue, 3));

            if (mesh.TexCoordsChannel0 != null)
            {
                var tex0Value = mesh.TexCoordsChannel0.Select(x => new float[] { x.X, 1f - x.Y }).SelectMany(x => x);
                meshElement.AppendChild(this.CreateSourceFloatArrayElement(document, meshId, "tex0", tex0Value, 2));
            }

            var verticesElement = document.CreateElement("vertices");
            verticesElement.SetAttribute("id", meshId + "-vertices");

            var inputElement = document.CreateElement("input");
            inputElement.SetAttribute("semantic", "POSITION");
            inputElement.SetAttribute("source", "#" + meshId + "-positions");

            verticesElement.AppendChild(inputElement);

            meshElement.AppendChild(verticesElement);

            var polylistElement = document.CreateElement("polylist");
            polylistElement.SetAttribute("count", mesh.TriangleCount.ToString());
            polylistElement.SetAttribute("material", "defaultMaterial");

            var inputVertexElement = document.CreateElement("input");
            inputVertexElement.SetAttribute("offset", "0");
            inputVertexElement.SetAttribute("semantic", "VERTEX");
            inputVertexElement.SetAttribute("source", "#" + meshId + "-vertices");

            polylistElement.AppendChild(inputVertexElement);

            var inputNormalElement = document.CreateElement("input");
            inputNormalElement.SetAttribute("offset", "0");
            inputNormalElement.SetAttribute("semantic", "NORMAL");
            inputNormalElement.SetAttribute("source", "#" + meshId + "-normals");

            polylistElement.AppendChild(inputNormalElement);

            if (mesh.TexCoordsChannel0 != null)
            {
                var inputTexcoordElement = document.CreateElement("input");
                inputTexcoordElement.SetAttribute("offset", "0");
                inputTexcoordElement.SetAttribute("semantic", "TEXCOORD");
                inputTexcoordElement.SetAttribute("source", "#" + meshId + "-tex0");

                polylistElement.AppendChild(inputTexcoordElement);
            }

            var vcountElement = document.CreateElement("vcount");
            vcountElement.AppendChild(document.CreateTextNode(String.Join(" ", Enumerable.Repeat(3, mesh.TriangleCount))));

            polylistElement.AppendChild(vcountElement);

            var pElement = document.CreateElement("p");
            var pValue = mesh.Triangles.Select(x => new uint[] { x.A, x.B, x.C }).SelectMany(x => x);
            pElement.AppendChild(document.CreateTextNode(String.Join(" ", pValue)));

            polylistElement.AppendChild(pElement);

            meshElement.AppendChild(polylistElement);

            geometryElement.AppendChild(meshElement);
            return geometryElement;
        }

        private XmlNode CreateSourceFloatArrayElement(XmlDocument document, string meshId, string sourceType, IEnumerable<float> values, int stride)
        {
            var sourceElement = document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-" + sourceType);
            sourceElement.SetAttribute("name", meshId + "-" + sourceType);

            var floatArrayElement = document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-" + sourceType + "-array");
            floatArrayElement.SetAttribute("count", values.Count().ToString());
            floatArrayElement.AppendChild(document.CreateTextNode(String.Join(" ", values)));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = document.CreateElement("technique_common");

            var accessorElement = document.CreateElement("accessor");
            accessorElement.SetAttribute("count", (values.Count() / stride).ToString());
            accessorElement.SetAttribute("offset", "0");
            accessorElement.SetAttribute("source", "#" + meshId + "-" + sourceType + "-array");
            accessorElement.SetAttribute("stride", stride.ToString());

            if (stride == 2)
            {
                accessorElement.AppendChild(this.CreateParamXmlElement(document, "S", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement(document, "T", "float"));
            }
            else if (stride == 3)
            {
                accessorElement.AppendChild(this.CreateParamXmlElement(document, "X", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement(document, "Y", "float"));
                accessorElement.AppendChild(this.CreateParamXmlElement(document, "Z", "float"));
            }

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);

            return sourceElement;
        }

        private XmlNode CreateParamXmlElement(XmlDocument document, string name, string type)
        {
            var paramElement = document.CreateElement("param");
            paramElement.SetAttribute("name", name);
            paramElement.SetAttribute("type", type);

            return paramElement;
        }

        private XmlNode CreateLibraryControllersXmlElement(XmlDocument document)
        {
            var libraryControllersElement = document.CreateElement("library_controllers");

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var mesh = this.meshes[i];
                string meshId = "meshId" + i;

                if (!mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
                {
                    continue;
                }

                var controllerElement = document.CreateElement("controller");
                controllerElement.SetAttribute("id", meshId + "-morph");
                controllerElement.SetAttribute("name", meshId + "-morph");

                var morphElement = document.CreateElement("morph");
                morphElement.SetAttribute("source", "#" + meshId);
                morphElement.SetAttribute("method", "NORMALIZED");
                morphElement.AppendChild(this.CreateTargetsSourceXmlElement(document, mesh, meshId));
                morphElement.AppendChild(this.CreateWeightsSourceXmlElement(document, mesh, meshId));

                var targetsElement = document.CreateElement("targets");

                var inputTargetElement = document.CreateElement("input");
                inputTargetElement.SetAttribute("semantic", "MORPH_TARGET");
                inputTargetElement.SetAttribute("source", "#" + meshId + "-targets");

                targetsElement.AppendChild(inputTargetElement);

                var inputWeightElement = document.CreateElement("input");
                inputWeightElement.SetAttribute("semantic", "MORPH_WEIGHT");
                inputWeightElement.SetAttribute("source", "#" + meshId + "-weights");

                targetsElement.AppendChild(inputWeightElement);

                morphElement.AppendChild(targetsElement);

                controllerElement.AppendChild(morphElement);

                libraryControllersElement.AppendChild(controllerElement);
            }

            return libraryControllersElement;
        }

        private XmlNode CreateTargetsSourceXmlElement(XmlDocument document, Mesh mesh, string meshId)
        {
            var sourceElement = document.CreateElement("source");
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

            var idrefArrayElement = document.CreateElement("IDREF_array");
            idrefArrayElement.SetAttribute("id", meshId + "-targets-array");
            idrefArrayElement.SetAttribute("count", morphTargetNames.Count.ToString());

            idrefArrayElement.AppendChild(document.CreateTextNode(String.Join(" ", morphTargetNames)));

            sourceElement.AppendChild(idrefArrayElement);

            var techniqueCommonElement = document.CreateElement("technique_common");

            var accessorElement = document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-targets-array");
            accessorElement.SetAttribute("count", morphTargetNames.Count.ToString());
            accessorElement.SetAttribute("stride", "1");

            var paramElement = document.CreateElement("param");
            paramElement.SetAttribute("name", "IDREF");
            paramElement.SetAttribute("type", "IDREF");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateWeightsSourceXmlElement(XmlDocument document, Mesh mesh, string meshId)
        {
            var sourceElement = document.CreateElement("source");
            sourceElement.SetAttribute("id", meshId + "-weights");

            int morphTargetCount = 0;

            foreach (MorphTarget morphTarget in mesh.MorphTargets)
            {
                if (!morphTarget.Vertices.All(x => x == Vector3.Zero))
                {
                    morphTargetCount++;
                }
            }

            var floatArrayElement = document.CreateElement("float_array");
            floatArrayElement.SetAttribute("id", meshId + "-weights-array");
            floatArrayElement.SetAttribute("count", morphTargetCount.ToString());
            floatArrayElement.AppendChild(document.CreateTextNode(String.Join(" ", Enumerable.Repeat("0", morphTargetCount))));

            sourceElement.AppendChild(floatArrayElement);

            var techniqueCommonElement = document.CreateElement("technique_common");

            var accessorElement = document.CreateElement("accessor");
            accessorElement.SetAttribute("source", "#" + meshId + "-weights-array");
            accessorElement.SetAttribute("count", morphTargetCount.ToString());
            accessorElement.SetAttribute("stride", "1");

            var paramElement = document.CreateElement("param");
            paramElement.SetAttribute("name", "WEIGHT");
            paramElement.SetAttribute("type", "float");

            accessorElement.AppendChild(paramElement);

            techniqueCommonElement.AppendChild(accessorElement);

            sourceElement.AppendChild(techniqueCommonElement);
            return sourceElement;
        }

        private XmlNode CreateLibraryVisualScenesXmlElement(XmlDocument document)
        {
            var libraryVisualScenesElement = document.CreateElement("library_visual_scenes");

            var visualSceneElement = document.CreateElement("visual_scene");
            visualSceneElement.SetAttribute("id", "RootNode");
            visualSceneElement.SetAttribute("name", "RootNode");

            this.RecurseNodeChildren(document, visualSceneElement, this.modelPack.Model.RootNode);

            libraryVisualScenesElement.AppendChild(visualSceneElement);

            return libraryVisualScenesElement;
        }

        private void RecurseNodeChildren(XmlDocument document, XmlElement parentElement, Node parent)
        {
            foreach (var node in parent.Children)
            {
                var nodeElement = document.CreateElement("node");
                nodeElement.SetAttribute("id", node.Name);
                nodeElement.SetAttribute("name", node.Name);
                nodeElement.SetAttribute("type", "NODE");

                var matrixElement = document.CreateElement("matrix");
                matrixElement.SetAttribute("sid", "matrix");

                var matrix = node.LocalTransform;
                string matrixString =
                    $"{matrix.M11} {matrix.M21} {matrix.M31} {matrix.M41} " +
                    $"{matrix.M12} {matrix.M22} {matrix.M32} {matrix.M42} " +
                    $"{matrix.M13} {matrix.M23} {matrix.M33} {matrix.M43} " +
                    $"{matrix.M14} {matrix.M24} {matrix.M34} {matrix.M44}";
                matrixElement.AppendChild(document.CreateTextNode(matrixString));

                nodeElement.AppendChild(matrixElement);

                for (int i = 0; i < node.Attachments.Count; i++)
                {
                    var attachment = node.Attachments[i];

                    switch (attachment.Type)
                    {
                        case NodeAttachmentType.Mesh:
                            var mesh = attachment.GetValue<Mesh>();
                            if (mesh.Flags.HasFlag(GeometryFlags.HasMorphTargets))
                            {
                                nodeElement.AppendChild(this.CreateInstanceControllerXmlElement(document, mesh));
                            }
                            else
                            {
                                nodeElement.AppendChild(this.CreateInstanceGeometryXmlElement(document, mesh));
                            }
                            break;
                        default:
                            // throw new NotImplementedException();
                            break;
                    }
                }

                this.RecurseNodeChildren(document, nodeElement, node);

                parentElement.AppendChild(nodeElement);
            }
        }

        private XmlNode CreateInstanceControllerXmlElement(XmlDocument document, Mesh mesh)
        {
            int meshIndex = this.meshes.IndexOf(mesh);
            string meshId = "meshId" + meshIndex;

            var instanceController = document.CreateElement("instance_controller");
            instanceController.SetAttribute("url", "#" + meshId + "-morph");

            var bindMaterialElement = document.CreateElement("bind_material");

            var techniqueCommonElement = document.CreateElement("technique_common");

            var instanceMaterialElement = document.CreateElement("instance_material");
            instanceMaterialElement.SetAttribute("symbol", "defaultMaterial");
            instanceMaterialElement.SetAttribute("target", "#" + mesh.MaterialName);

            if (mesh.TexCoordsChannel0 != null)
            {
                var bindVertexInputElement = document.CreateElement("bind_vertex_input");
                bindVertexInputElement.SetAttribute("semantic", "CHANNEL0");
                bindVertexInputElement.SetAttribute("input_semantic", "TEXCOORD");
                bindVertexInputElement.SetAttribute("input_set", "0");

                instanceMaterialElement.AppendChild(bindVertexInputElement);
            }

            techniqueCommonElement.AppendChild(instanceMaterialElement);

            bindMaterialElement.AppendChild(techniqueCommonElement);

            instanceController.AppendChild(bindMaterialElement);

            return instanceController;
        }

        private XmlNode CreateInstanceGeometryXmlElement(XmlDocument document, Mesh mesh)
        {
            int meshIndex = this.meshes.IndexOf(mesh);
            string meshId = "meshId" + meshIndex;

            var instanceGeometryElement = document.CreateElement("instance_geometry");
            instanceGeometryElement.SetAttribute("url", "#" + meshId);

            var bindMaterialElement = document.CreateElement("bind_material");

            var techniqueCommonElement = document.CreateElement("technique_common");

            var instanceMaterialElement = document.CreateElement("instance_material");
            instanceMaterialElement.SetAttribute("symbol", "defaultMaterial");
            instanceMaterialElement.SetAttribute("target", "#" + mesh.MaterialName);

            if (mesh.TexCoordsChannel0 != null)
            {
                var bindVertexInputElement = document.CreateElement("bind_vertex_input");
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

        private XmlNode CreateSceneXmlElement(XmlDocument document)
        {
            var sceneElement = document.CreateElement("scene");

            var instanceVisualSceneElement = document.CreateElement("instance_visual_scene");
            instanceVisualSceneElement.SetAttribute("url", "#RootNode");

            sceneElement.AppendChild(instanceVisualSceneElement);

            return sceneElement;
        }

        private void ExportTextures()
        {
            Directory.CreateDirectory(this.textureFolder);

            foreach (GFDLibrary.Textures.Texture texture in this.modelPack.Textures.Textures)
            {
                File.WriteAllBytes(Path.Combine(this.textureFolder, texture.Name), texture.Data);
            }
        }
    }
}
