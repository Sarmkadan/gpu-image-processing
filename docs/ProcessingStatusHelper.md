# ProcessingStatusHelper

The `ProcessingStatusHelper` class provides a centralized set of static utility methods for interpreting and presenting the current state of GPU image processing operations. It serves as a presentation layer abstraction, mapping internal processing states to human-readable descriptions, visual indicators (emoji), and boolean flags that determine whether a process is currently running or has reached a final conclusion.

## API

### `IsTerminal`
```csharp
public static bool IsTerminal
```
Determines whether the current processing state represents a final outcome, meaning no further state transitions are expected without external intervention. This property returns `true` if the process has completed successfully, failed irrecoverably, or was cancelled. It returns `false` if the process is pending, running, or pausing. This property does not accept parameters and does not throw exceptions.

### `IsActive`
```csharp
public static bool IsActive
```
Indicates whether the processing operation is currently in progress. This property returns `true` only when the state is actively consuming resources or executing kernels on the GPU. It returns `false` for initial, paused, completed, failed, or cancelled states. This property does not accept parameters and does not throw exceptions.

### `GetDescription`
```csharp
public static string GetDescription
```
Retrieves a human-readable text description corresponding to the current processing status. This method is typically used for logging or displaying status messages in a user interface. It returns a non-null string describing the state (e.g., "Processing", "Completed", "Failed"). No parameters are required. This method does not throw exceptions under normal operation.

### `GetEmoji`
```csharp
public static string GetEmoji
```
Returns a Unicode emoji character representing the visual status of the operation. This is intended for use in rich console output or modern UI dashboards to provide immediate visual feedback (e.g., ✅ for success, ⏳ for processing, ❌ for failure). It returns a string containing the emoji character. No parameters are required. This method does not throw exceptions.

## Usage

### Example 1: Console Status Monitoring
The following example demonstrates how to use the helper to display a real-time status line in a command-line application, updating the emoji and description based on the current state.

```csharp
using System;
using System.Threading;
using GpuImageProcessing;

public class StatusMonitor
{
    public static void RenderStatus()
    {
        // Retrieve visual and textual representations
        string emoji = ProcessingStatusHelper.GetEmoji;
        string description = ProcessingStatusHelper.GetDescription;
        
        Console.WriteLine($"[{emoji}] {description}");

        // Check if the process has finished to determine if the loop should exit
        if (ProcessingStatusHelper.IsTerminal)
        {
            Console.WriteLine("Processing pipeline has reached a final state.");
            return;
        }

        // Continue monitoring if active
        if (ProcessingStatusHelper.IsActive)
        {
            Thread.Sleep(500); // Poll interval
            RenderStatus();
        }
    }
}
```

### Example 2: UI State Logic
This example illustrates how to drive UI element enablement and visibility logic based on the terminal and active states within a hypothetical view model.

```csharp
using System;
using GpuImageProcessing;

public class ProcessingViewModel
{
    public bool CanCancelOperation { get; private set; }
    public bool IsProgressBarIndeterminate { get; private set; }
    public string StatusText { get; private set; }

    public void UpdateState()
    {
        // Update text display
        StatusText = $"{ProcessingStatusHelper.GetEmoji} {ProcessingStatusHelper.GetDescription}";

        // The cancel button should only be enabled if the process is active 
        // and not already in a terminal state.
        CanCancelOperation = ProcessingStatusHelper.IsActive && !ProcessingStatusHelper.IsTerminal;

        // Progress bar should only spin if the process is actively running
        IsProgressBarIndeterminate = ProcessingStatusHelper.IsActive;
    }
}
```

## Notes

*   **State Consistency**: The boolean properties `IsTerminal` and `IsActive` are logically mutually exclusive in most valid workflows; however, implementations should treat them as independent snapshots of the current state. A state where both are `false` typically indicates a pending or paused condition.
*   **Thread Safety**: As all members are static and appear to be stateless accessors (taking no arguments and relying on an implicit global or context-based state), callers must ensure that the underlying state mechanism they query is thread-safe if accessed concurrently from multiple threads (e.g., a UI thread and a GPU completion callback thread). The helper methods themselves do not perform locking.
*   **Return Value Guarantees**: `GetDescription` and `GetEmoji` are expected to always return a valid, non-null string. Implementations should default to a generic "Unknown" status or a question mark emoji if the internal state is undefined, rather than returning null.
*   **Side Effects**: These methods are read-only observers. Invoking any of these members will not trigger state transitions, start processes, or modify the underlying processing pipeline.
