#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using Xunit;

namespace GpuImageProcessing.Tests;

public class FilterConfigurationRepositoryTests
{
    private static FilterConfiguration CreateFilter(string name, FilterType type = FilterType.Grayscale) =>
        new() { Name = name, FilterType = type };

    [Fact]
    public async Task CreateAsync_ValidFilter_AssignsIdAndTimestampsAndStoresIt()
    {
        var repository = new FilterConfigurationRepository();
        var filter = CreateFilter("gray");
        var placeholderId = filter.Id;

        var created = await repository.CreateAsync(filter);

        Assert.Same(filter, created);
        Assert.NotEqual(placeholderId, created.Id);
        Assert.True(created.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.Same(created, await repository.GetByIdAsync(created.Id));
        Assert.Equal(1, await repository.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_NullOrInvalidFilter_Throws()
    {
        var repository = new FilterConfigurationRepository();

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null!));
        await Assert.ThrowsAsync<InvalidFilterException>(() => repository.CreateAsync(CreateFilter("   ")));
        Assert.Equal(0, await repository.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ExistingAndUnknownIds_ReplacesOrThrows()
    {
        var repository = new FilterConfigurationRepository();
        var original = await repository.CreateAsync(CreateFilter("gray"));
        var replacement = new FilterConfiguration { Id = original.Id, Name = "renamed", FilterType = FilterType.Blur };

        await repository.UpdateAsync(replacement);

        Assert.Same(replacement, await repository.GetByIdAsync(original.Id));
        Assert.Equal(1, await repository.CountAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(CreateFilter("stranger")));
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesOnlyThatFilter()
    {
        var repository = new FilterConfigurationRepository();
        var keep = await repository.CreateAsync(CreateFilter("keep"));
        var drop = await repository.CreateAsync(CreateFilter("drop"));

        Assert.True(await repository.DeleteAsync(drop.Id));
        Assert.False(await repository.DeleteAsync(drop.Id));
        Assert.False(await repository.ExistsAsync(drop.Id));
        Assert.True(await repository.ExistsAsync(keep.Id));
        Assert.Null(await repository.GetByIdAsync(drop.Id));
        Assert.Equal(1, await repository.CountAsync());
    }

    [Fact]
    public async Task EmptyRepository_ReturnsEmptyResultsAndNullOrFalseLookups()
    {
        var repository = new FilterConfigurationRepository();

        Assert.Empty(await repository.GetAllAsync());
        Assert.Empty(await repository.GetByTypeAsync(FilterType.Blur));
        Assert.Empty(await repository.GetPagedAsync(1, 10));
        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid()));
        Assert.False(await repository.ExistsAsync(Guid.NewGuid()));
        Assert.Equal(0, await repository.CountAsync());
    }

    [Fact]
    public async Task GetPagedAsync_LastPartialPageAndPastEnd_ReturnsExpectedSlices()
    {
        var repository = new FilterConfigurationRepository();
        for (var i = 1; i <= 5; i++)
            await repository.CreateAsync(CreateFilter($"f{i}"));

        var page2 = (await repository.GetPagedAsync(2, 2)).Select(f => f.Name).ToList();
        var lastPartial = (await repository.GetPagedAsync(3, 2)).Select(f => f.Name).ToList();
        var pastEnd = await repository.GetPagedAsync(4, 2);

        Assert.Equal(new[] { "f3", "f4" }, page2);
        Assert.Equal(new[] { "f5" }, lastPartial);
        Assert.Empty(pastEnd);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    public async Task GetPagedAsync_NonPositivePageOrSize_ThrowsArgumentException(int pageNumber, int pageSize)
    {
        var repository = new FilterConfigurationRepository();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.GetPagedAsync(pageNumber, pageSize));
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsOnlyMatchingFilters()
    {
        var repository = new FilterConfigurationRepository();
        await repository.CreateAsync(CreateFilter("g1", FilterType.Grayscale));
        await repository.CreateAsync(CreateFilter("b1", FilterType.Blur));
        await repository.CreateAsync(CreateFilter("g2", FilterType.Grayscale));

        var grayscale = (await repository.GetByTypeAsync(FilterType.Grayscale)).Select(f => f.Name).ToList();
        var blurs = await repository.GetByCriteriaAsync(f => f.FilterType == FilterType.Blur);

        Assert.Equal(new[] { "g1", "g2" }, grayscale);
        Assert.Single(blurs);
    }
}
