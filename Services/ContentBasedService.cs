using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using WebFM_Style.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ContentBasedService : IRecommendationService
{
    private readonly FmStyleDbContext _context;
    private readonly MLContext _mlContext;

    public ContentBasedService(FmStyleDbContext context)
    {
        _context = context;
        _mlContext = new MLContext();
    }

    public async Task<List<RecommendedProductDTO>> GetRecommendedProducts(int userId)
    {
        var seedProducts = await GetSeedProducts(userId);

        if (!seedProducts.Any())
        {
            return new List<RecommendedProductDTO>();
        }

        var interestedCategories = seedProducts
            .Select(p => p.Category)
            .Distinct()
            .ToList();

        var candidateProducts = await GetCandidateProducts(interestedCategories);

        if (!candidateProducts.Any()) return new List<RecommendedProductDTO>();

        var allDataToFeaturize = new List<ProductDataDTO>();
        allDataToFeaturize.AddRange(seedProducts);
        allDataToFeaturize.AddRange(candidateProducts);

        allDataToFeaturize = allDataToFeaturize.GroupBy(p => p.Id).Select(g => g.First()).ToList();

        var allFeatures = FeaturizeContent(allDataToFeaturize);

        var seedIds = seedProducts.Select(p => p.Id).ToHashSet();
        var seedFeatures = allFeatures.Where(f => seedIds.Contains(f.ProductId)).ToList();
        var candidateFeatures = allFeatures.Where(f => !seedIds.Contains(f.ProductId)).ToList();

        var recommendations = CalculateSimilarity(seedFeatures, candidateFeatures);

        var resultIds = recommendations
            .OrderByDescending(r => r.Score)
            .Take(9)
            .Select(r => r.Id)
            .ToList();

        var finalResult = new List<RecommendedProductDTO>();

        foreach (var id in resultIds)
        {
            var product = candidateProducts.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                finalResult.Add(MapToDTO(product));
            }
        }

        return finalResult;
    }

    public async Task<List<RecommendedProductDTO>> GetProductDetailsRecommendations(int currentProductId)
    {
        var currentProduct = await _context.Products
            .Include(p => p.ProductType)
            .FirstOrDefaultAsync(p => p.Id == currentProductId);

        if (currentProduct == null) return new List<RecommendedProductDTO>();

        var candidates = await GetCandidateProducts(new List<string> { currentProduct.ProductType.Name });
        candidates = candidates.Where(p => p.Id != currentProductId).ToList();

        if (!candidates.Any()) return new List<RecommendedProductDTO>();

        var currentProductDto = new ProductDataDTO
        {
            Id = currentProduct.Id,
            Name = currentProduct.Name,
            Category = currentProduct.ProductType.Name,
            DescriptionSnippet = currentProduct.Description
        };

        var allData = new List<ProductDataDTO> { currentProductDto };
        allData.AddRange(candidates);

        var features = FeaturizeContent(allData);
        var targetVector = features.First(f => f.ProductId == currentProductId).Features;
        var candidateVectors = features.Where(f => f.ProductId != currentProductId).ToList();

        var results = new List<ProductSimilarityResult>();
        foreach (var item in candidateVectors)
        {
            double score = ComputeCosineSimilarity(targetVector, item.Features);
            if (score > 0.1)
            {
                results.Add(new ProductSimilarityResult { Id = item.ProductId, Score = score });
            }
        }

        return results
            .OrderByDescending(x => x.Score)
            .Take(6)
            .Select(r =>
            {
                var p = candidates.First(c => c.Id == r.Id);
                return MapToDTO(p);
            })
            .ToList();
    }

    private List<ProductSimilarityResult> CalculateSimilarity(
        List<ProductFeatures> seedFeatures,
        List<ProductFeatures> candidateFeatures)
    {
        var similarityResults = new ConcurrentDictionary<int, ProductSimilarityResult>();

        Parallel.ForEach(seedFeatures, seed =>
        {
            foreach (var candidate in candidateFeatures)
            {
                double score = ComputeCosineSimilarity(seed.Features, candidate.Features);
                if (score > 0.1)
                {
                    similarityResults.AddOrUpdate(candidate.ProductId,
                        new ProductSimilarityResult { Id = candidate.ProductId, Score = score },
                        (key, existing) => score > existing.Score
                            ? new ProductSimilarityResult { Id = key, Score = score }
                            : existing);
                }
            }
        });

        return similarityResults.Values.ToList();
    }

    private List<ProductFeatures> FeaturizeContent(List<ProductDataDTO> products)
    {
        var productMLData = products.Select(p => new ProductDataML
        {
            ProductId = p.Id,
            ContentText = $"{p.Category} {p.Category} {p.Category} {p.Name} {p.DescriptionSnippet}"
        }).ToList();

        IDataView dataView = _mlContext.Data.LoadFromEnumerable(productMLData);

        var featurizationPipeline = _mlContext.Transforms.Text.FeaturizeText(
            outputColumnName: "Features",
            inputColumnName: nameof(ProductDataML.ContentText)
        );

        ITransformer textFeaturizer = featurizationPipeline.Fit(dataView);
        IDataView transformedData = textFeaturizer.Transform(dataView);

        return _mlContext.Data.CreateEnumerable<ProductFeatures>(transformedData, reuseRowObject: false).ToList();
    }

    public static double ComputeCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length)
            return 0.0;

        double dotProduct = 0.0;
        double magnitude1 = 0.0;
        double magnitude2 = 0.0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        if (magnitude1 == 0.0 || magnitude2 == 0.0)
            return 0.0;

        return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
    }

    private async Task<List<ProductDataDTO>> GetCandidateProducts(List<string> categories)
    {
        var query = _context.Products
             .Include(x => x.ProductType)
             .Include(x => x.Images)
             .Where(x => x.Status == 1);

        if (categories != null && categories.Any())
        {
            query = query.Where(x => categories.Contains(x.ProductType.Name));
        }
        else
        {
            return new List<ProductDataDTO>();
        }

        return await query
             .OrderByDescending(p => p.Id)
             .Take(60)
             .Select(p => new ProductDataDTO
             {
                 Id = p.Id,
                 Name = p.Name,
                 Category = p.ProductType.Name,
                 DescriptionSnippet = p.Description,
                 Price = p.Price ?? 0,
                 ImageUrl = p.Images.OrderBy(img => img.Id).FirstOrDefault().Url
             })
             .ToListAsync();
    }

    private async Task<List<ProductDataDTO>> GetSeedProducts(int userId)
    {
        var seedProducts = new List<ProductDataDTO>();

        var boughtProducts = await _context.OrderItems
            .Include(oi => oi.Oder)
            .Include(oi => oi.ProductSizeColor.Product.ProductType)
            .Where(oi => oi.Oder.AccountId == userId && oi.ProductSizeColorId != null)
            .OrderByDescending(oi => oi.Oder.CreateDay)
            .Select(oi => oi.ProductSizeColor.Product)
            .Distinct()
            .Take(3)
            .Select(p => new ProductDataDTO
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.ProductType.Name,
                DescriptionSnippet = p.Description
            }).ToListAsync();

        seedProducts.AddRange(boughtProducts);

        if (seedProducts.Count < 3)
        {
            var existingIds = seedProducts.Select(p => p.Id).ToList();
            var needed = 3 - seedProducts.Count;

            var viewedProducts = await _context.ProductViews
                .Where(v => v.AccountId == userId && !existingIds.Contains(v.ProductId))
                .Include(v => v.Product.ProductType)
                .OrderByDescending(v => v.ViewTime)
                .Select(v => v.Product)
                .Distinct()
                .Take(needed)
                .Select(p => new ProductDataDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.ProductType.Name,
                    DescriptionSnippet = p.Description
                }).ToListAsync();

            seedProducts.AddRange(viewedProducts);
        }

        return seedProducts;
    }

    private RecommendedProductDTO MapToDTO(ProductDataDTO p)
    {
        return new RecommendedProductDTO
        {
            Id = p.Id,
            Name = p.Name ?? "Sản phẩm",
            Price = p.Price,
            ImageUrl = p.ImageUrl
        };
    }
}