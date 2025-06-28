namespace Darp.Utils.Assets;

#pragma warning disable CA1812
internal sealed class AssetsFactory(IEnumerable<AssetsOptions> configuration) : IAssetsFactory
#pragma warning restore CA1812
{
    private readonly IEnumerable<AssetsOptions> _configuration = configuration;

    public TAssetsService GetReadOnlyAssets<TAssetsService>()
        where TAssetsService : IReadOnlyAssetsService
    {
        Type typeToResolve = typeof(TAssetsService);
        foreach (AssetsOptions xxx in _configuration)
        {
            if (!xxx.Type.IsAssignableTo(typeToResolve))
                continue;
            return (TAssetsService)xxx.Builder(xxx.Services);
        }
        throw new ArgumentException("No service was registered for the given type");
    }

    public TAssetsService GetReadOnlyAssets<TAssetsService>(string? name)
        where TAssetsService : IReadOnlyAssetsService
    {
        Type typeToResolve = typeof(TAssetsService);
        foreach (AssetsOptions xxx in _configuration)
        {
            if (name != xxx.Name)
                continue;
            if (!xxx.Type.IsAssignableTo(typeToResolve))
                throw new ArgumentOutOfRangeException(nameof(name), "Invalid type requested for named service");
            return (TAssetsService)xxx.Builder(xxx.Services);
        }
        throw new ArgumentException("No service was registered for the given type");
    }
}
