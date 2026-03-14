using Microsoft.ML.Data;

public class ProductDataML
{
    [LoadColumn(0)] 
    public int ProductId { get; set; }

    [LoadColumn(1)]
    [ColumnName("ContentText")]
    public string ContentText { get; set; } 
}