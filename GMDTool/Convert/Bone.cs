using System.Collections.Generic;

namespace GMDTool.Convert
{
    public class Bone
    {
        public List<VertexWeight> VertexWeights;

        public Bone() => this.VertexWeights = new List<VertexWeight>();
    }
}