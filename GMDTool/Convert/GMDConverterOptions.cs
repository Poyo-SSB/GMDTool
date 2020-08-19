using CommandLine;

namespace GMDTool.Convert
{
    public class GMDConverterOptions
    {
        [Option('b', "blender-output")]
        public bool EnableBlenderCompatibilityOutput { get; set; }
        [Option('i', "ignore-empty")]
        public bool IgnoreEmptyNodes { get; set; }

        [Value(0, Required = true)]
        public string Input { get; set; }
        [Value(1)]
        public string Output { get; set; }
    }
}
