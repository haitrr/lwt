namespace Lwt.Test.Utilities;

using System.Collections.Generic;
using Lwt.Utilities;
using Xunit;

/// <summary>
/// test chinese text splitter.
/// </summary>
public class ChineseTextSplitterTest
{
    private readonly ChineseTextSplitter chineseTextSplitter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChineseTextSplitterTest"/> class.
    /// </summary>
    public ChineseTextSplitterTest()
    {
        this.chineseTextSplitter = new ChineseTextSplitter();
    }

    /// <summary>
    /// should work.
    /// </summary>
    /// <param name="input">the input text.</param>
    /// <param name="output">the expected output.</param>
    [Theory]
    [InlineData("你在干什么", new[] { "你", "在", "干什么" })]
    [InlineData("上海浦东开发与建设同步", new[] { "上海", "浦东", "开发", "与", "建设", "同步" })]
    public void SplitShouldWork(string input, IEnumerable<string> output)
    {
        IEnumerable<string> result = this.chineseTextSplitter.Split(input);
        Assert.Equal(output, result);
    }
}