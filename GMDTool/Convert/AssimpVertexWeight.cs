namespace GMDTool.Convert
{
    public class AssimpVertexWeight
    {
        public int VertexId;
        public float Weight;

        public AssimpVertexWeight(int vertexId, float weight)
        {
            this.VertexId = vertexId;
            this.Weight = weight;
        }
    }
}