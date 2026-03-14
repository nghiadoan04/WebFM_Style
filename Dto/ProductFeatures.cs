using Microsoft.ML.Data;

public class ProductFeatures
{
    public int ProductId { get; set; }
    
    [ColumnName("Features"), VectorType]
    public float[] Features { get; set; }
}