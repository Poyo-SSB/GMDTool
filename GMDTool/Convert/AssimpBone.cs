using System.Collections.Generic;

namespace GMDTool.Convert
{
    public class AssimpBone
    {
        public List<AssimpVertexWeight> VertexWeights;

        public AssimpBone() => this.VertexWeights = new List<AssimpVertexWeight>();
    }
}