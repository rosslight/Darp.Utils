namespace Darp.Utils.Tests.Assets;

using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Utils.Assets;

public sealed class UseCases
{
    [Fact]
    public void JustUseTheService()
    {
        var service = new FolderAssetsService("some/path", "relative");
        service.BasePath.ShouldBe("some/path/relative");
    }

    [Fact]
    public void UseSingleServiceUsingDi()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddFolderAssetsService("some/path", "relative")
            .BuildServiceProvider();

        IReadOnlyAssetsService s2 = provider.GetRequiredService<IReadOnlyAssetsService>();
        IAssetsService s3 = provider.GetRequiredService<IAssetsService>();
        IFolderAssetsService service = provider.GetRequiredService<IFolderAssetsService>();
        service.BasePath.ShouldBe("some/path/relative");
        s2.BasePath.ShouldBe("some/path/relative");
        s3.BasePath.ShouldBe("some/path/relative");
    }

    [Fact]
    public void UseSingleServiceUsingFactory()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddFolderAssetsService("some/path", "relative")
            .BuildServiceProvider();

        IAssetsFactory factory = provider.GetRequiredService<IAssetsFactory>();
        IFolderAssetsService service = factory.GetReadOnlyAssets<IFolderAssetsService>();
        IAssetsService s1 = factory.GetAssets();
        IReadOnlyAssetsService s2 = factory.GetReadOnlyAssets();
        service.BasePath.ShouldBe("some/path/relative");
        s1.BasePath.ShouldBe("some/path/relative");
        s2.BasePath.ShouldBe("some/path/relative");
    }

    [Fact]
    public static void UseMultipleServiceUsingFactory()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddFolderAssetsService("FolderService1", "some/path", "some")
            .AddFolderAssetsService("FolderService2", "some/path", "other")
            .BuildServiceProvider();

        IAssetsFactory factory = provider.GetRequiredService<IAssetsFactory>();
        IFolderAssetsService s11 = factory.GetReadOnlyAssets<IFolderAssetsService>();
        IAssetsService s12 = factory.GetAssets();
        IReadOnlyAssetsService s13 = factory.GetReadOnlyAssets();
        s11.BasePath.ShouldBe("some/path/some");
        s12.BasePath.ShouldBe("some/path/some");
        s13.BasePath.ShouldBe("some/path/some");

        IFolderAssetsService s21 = factory.GetReadOnlyAssets<IFolderAssetsService>("FolderService1");
        IAssetsService s22 = factory.GetAssets("FolderService1");
        IReadOnlyAssetsService s23 = factory.GetReadOnlyAssets("FolderService1");
        s21.BasePath.ShouldBe("some/path/some");
        s22.BasePath.ShouldBe("some/path/some");
        s23.BasePath.ShouldBe("some/path/some");

        IFolderAssetsService s31 = factory.GetReadOnlyAssets<IFolderAssetsService>("FolderService2");
        IAssetsService s32 = factory.GetAssets("FolderService2");
        IReadOnlyAssetsService s33 = factory.GetReadOnlyAssets("FolderService2");
        s31.BasePath.ShouldBe("some/path/other");
        s32.BasePath.ShouldBe("some/path/other");
        s33.BasePath.ShouldBe("some/path/other");
    }
}
