namespace Lwt.Test.Utilities;

using Lwt.Utilities;
using Xunit;

/// <summary>
/// TextNormalizer tester.
/// </summary>
public class TextNormalizerTester
{
    /// <summary>
    /// should get resolved by DI.
    /// </summary>
    [Fact]
    public void ShouldGetResolveByDi()
    {
        var helper = new DependencyResolverHelper();
        Assert.IsType<TextNormalizer>(helper.GetService<ITextNormalizer>());
    }
}