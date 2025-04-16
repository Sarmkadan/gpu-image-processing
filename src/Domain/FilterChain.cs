// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents a sequence of filters to be applied to an image.
/// </summary>
public class FilterChain
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FilterStep> Steps { get; set; } = [];
    public bool IsEnabled { get; set; }
    public int ExecutionOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Dictionary<string, object> ChainOptions { get; set; } = new();
    public bool AllowParallelExecution { get; set; }
    public int MaxParallelSteps { get; set; }
    public bool CacheIntermediateResults { get; set; }

    public FilterChain()
    {
        Id = Guid.NewGuid();
        IsEnabled = true;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        MaxParallelSteps = Constants.Processing.DefaultThreadCount;
    }

    /// <summary>
    /// Adds a filter step to the chain.
    /// </summary>
    public void AddStep(Guid filterId, int order = -1)
    {
        if (order == -1)
            order = Steps.Count;

        Steps.Add(new FilterStep
        {
            FilterId = filterId,
            Order = order,
            IsEnabled = true
        });

        // Sort in-place to avoid allocating an intermediate collection.
        Steps.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a filter step by filter ID.
    /// </summary>
    public bool RemoveStep(Guid filterId)
    {
        var removed = Steps.RemoveAll(s => s.FilterId == filterId) > 0;
        if (removed)
        {
            for (int i = 0; i < Steps.Count; i++)
                Steps[i].Order = i;
            ModifiedAt = DateTime.UtcNow;
        }
        return removed;
    }

    /// <summary>
    /// Reorders steps in the chain.
    /// </summary>
    public void ReorderSteps(List<Guid> filterIds)
    {
        if (filterIds.Count != Steps.Count)
            throw new ArgumentException("Filter list must match current step count");

        for (int i = 0; i < filterIds.Count; i++)
        {
            var step = Steps.FirstOrDefault(s => s.FilterId == filterIds[i]);
            if (step != null)
                step.Order = i;
        }

        Steps.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets enabled filter steps in order.
    /// </summary>
    public List<FilterStep> GetEnabledSteps()
    {
        return Steps.Where(s => s.IsEnabled).OrderBy(s => s.Order).ToList();
    }

    /// <summary>
    /// Validates the filter chain.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (Steps.Count == 0)
            return false;

        if (!IsEnabled)
            return false;

        if (GetEnabledSteps().Count == 0)
            return false;

        if (AllowParallelExecution && MaxParallelSteps is < 1 or > Constants.Processing.DefaultThreadCount * 4)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the total estimated processing time.
    /// </summary>
    public double EstimateTotalProcessingTime()
    {
        return GetEnabledSteps().Sum(s => s.EstimatedExecutionTimeMs);
    }

    /// <summary>
    /// Creates a copy of this filter chain.
    /// </summary>
    public FilterChain Clone()
    {
        var clone = new FilterChain
        {
            Name = Name + " (Copy)",
            Description = Description,
            IsEnabled = IsEnabled,
            ExecutionOrder = ExecutionOrder,
            AllowParallelExecution = AllowParallelExecution,
            MaxParallelSteps = MaxParallelSteps,
            CacheIntermediateResults = CacheIntermediateResults,
            ChainOptions = new Dictionary<string, object>(ChainOptions)
        };

        foreach (var step in Steps)
        {
            clone.Steps.Add(new FilterStep
            {
                FilterId = step.FilterId,
                Order = step.Order,
                IsEnabled = step.IsEnabled,
                EstimatedExecutionTimeMs = step.EstimatedExecutionTimeMs
            });
        }

        return clone;
    }

    /// <summary>
    /// Gets the number of enabled filters in the chain.
    /// </summary>
    public int GetEnabledFilterCount()
    {
        // Count directly to avoid allocating the full List<FilterStep> from GetEnabledSteps().
        int count = 0;
        foreach (var step in Steps)
            if (step.IsEnabled) count++;
        return count;
    }
}

/// <summary>
/// Represents a single step in a filter chain.
/// </summary>
public class FilterStep
{
    public Guid StepId { get; set; } = Guid.NewGuid();
    public Guid FilterId { get; set; }
    public int Order { get; set; }
    public bool IsEnabled { get; set; }
    public double EstimatedExecutionTimeMs { get; set; }
}
