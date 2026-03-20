public interface IRecommendationService
{
    Task<List<RecommendedProductDTO>> GetRecommendedProducts(int userId);

    Task<List<RecommendedProductDTO>> GetProductDetailsRecommendations(int currentProductId);
}