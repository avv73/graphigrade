using FluentAssertions;
using GraphiGrade.Extensions;

namespace GraphiGrade.Tests.UnitTests.Extensions;

public class MulitpartFormDataContentExtensionsTests
{
    [Fact]
    public async Task ToDictionaryAsync_ValidContent_ReturnsCorrectDictionary()
    {
        // Arrange
        var content = new MultipartFormDataContent
        {
            { new StringContent("Value1"), "Key1" },
            { new StringContent("Value2"), "Key2" }
        };

        // Act
        var result = await content.ToDictionaryAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("Key1");
        result.Should().ContainKey("Key2");
        result["Key1"].Should().Be("Value1");
        result["Key2"].Should().Be("Value2");
    }

    [Fact]
    public async Task ToDictionaryAsync_EmptyContent_ReturnsEmptyDictionary()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        // Act
        var result = await content.ToDictionaryAsync();

        // Assert
        result.Should().HaveCount(0);
    }
}
