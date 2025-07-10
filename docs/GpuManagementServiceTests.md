# GpuManagementServiceTests

Provides a suite of unit tests for the `GpuManagementService` class, validating its device discovery, memory allocation, and device validation logic under various normal and error conditions.

## API

### GpuManagementServiceTests()
Instantiates a new test fixture.  
- **Parameters:** none  
- **Return:** a new `GpuManagementServiceTests` instance  
- **Throws:** none  

### Constructor_NullLogger_ThrowsArgumentNullException
Verifies that constructing `GpuManagementService` with a `null` logger throws an `ArgumentNullException`.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** `ArgumentNullException` if the service constructor does not throw as expected; otherwise the test fails via the unit‑testing framework.

### GetAvailableDevices_AlwaysContainsAtLeastOneDevice
Confirms that `GetAvailableDevices` returns a collection containing at least one GPU device.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the returned collection is empty.

### GetBestDevice_ReturnsDeviceWithHighestPerformanceScore
Ensures that `GetBestDevice` selects the device with the highest performance score among available devices.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the returned device does not have the maximal performance score.

### GetDeviceWithMostMemory_ReturnsDeviceWithLargestMemory
Checks that `GetDeviceWithMostMemory` returns the device possessing the largest memory capacity.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the returned device does not have the maximal memory size.

### AllocateMemory_ZeroBytes_ReturnsFalse
Validates that requesting allocation of zero bytes returns `false` without altering internal state.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the allocation call returns `true` or throws.

### AllocateMemory_NegativeBytes_ReturnsFalse
Ensures that a negative byte request results in a `false` return value and leaves allocation state unchanged.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the method returns `true` or throws.

### AllocateMemory_InvalidDevice_ThrowsGpuException
Confirms that attempting to allocate memory on an invalid device identifier throws a `GpuException`.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** `GpuException` if the allocation does not throw as expected; otherwise the test fails.

### AllocateMemory_SufficientMemory_ReturnsTrue
Checks that allocation succeeds (`true`) when the requested size is within the available memory of the selected device.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the allocation returns `false` or throws.

### AllocateMemory_InsufficientMemory_ThrowsGpuException
Verifies that requesting more memory than is available on a device throws a `GpuException`.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** `GpuException` if the allocation does not throw as expected; otherwise the test fails.

### AllocateMemory_DecreasesAvailableMemory
Ensures that a successful allocation reduces the device’s reported free memory by the allocated amount.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the free memory after allocation does not match the expected decrease.

### DeallocateMemory_ZeroBytes_DoesNothing
Confirms that a deallocation request for zero bytes leaves allocation state unchanged and returns without error.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if internal state is altered or an exception is thrown.

### DeallocateMemory_NegativeBytes_DoesNothing
Ensures that a negative byte deallocation request is treated as a no‑op and does not affect allocation state.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if state changes or an exception occurs.

### DeallocateMemory_IncreaseAvailableMemory
Validates that deallocating a previously allocated block increases the device’s free memory by the exact size of the block.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the free memory after deallocation does not reflect the expected increase.

### GetTotalAllocatedMemory_ReturnsAccumulatedAllocation
Checks that `GetTotalAllocatedMemory` returns the sum of all currently allocated blocks across devices.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the returned total does not match the expected sum.

### ValidateDevice_ValidDeviceWithSufficientMemory_ReturnsTrue
Confirms that `ValidateDevice` returns `true` for a device identifier that exists and has enough memory for a given request.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the method returns `false` or throws.

### ValidateDevice_InsufficientMemory_ReturnsFalse
Ensures that `ValidateDevice` returns `false` when the selected device lacks sufficient memory for the request.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the method returns `true` or throws.

### ValidateDevice_InsufficientComputeUnits_ReturnsFalse
Verifies that `ValidateDevice` returns `false` for a device that does not meet the required compute unit count.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the method returns `true` or throws.

### ValidateDevice_InvalidDeviceId_ReturnsFalse
Checks that `ValidateDevice` returns `false` when supplied with an identifier that does not correspond to any known GPU device.  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if the method returns `true` or throws.

### GetMemoryStatistics_ReturnsAllMetrics
Validates that `GetMemoryStatistics` returns an object containing all expected memory‑related metrics (allocated, free, total, etc.).  
- **Parameters:** none  
- **Return:** none  
- **Throws:** fails if any expected metric is missing or has an incorrect value.

## Usage

```csharp
using NUnit.Framework;
using GpuImageProcessing.Services;
using GpuImageProcessing.Tests;

[TestFixture]
public class GpuManagementServiceTestsTests
{
    [Test]
    public void GetAvailableDevices_ReturnsAtLeastOneDevice()
    {
        // Arrange
        var test = new GpuManagementServiceTests();

        // Act & Assert (the test method contains its own assertions)
        test.GetAvailableDevices_AlwaysContainsAtLeastOneDevice();
        // If no assertion exception is thrown, the test passes.
    }

    [Test]
    public void AllocateMemory_InvalidDevice_Throws()
    {
        // Arrange
        var test = new GpuManagementServiceTests();

        // Act & Assert
        Assert.Throws<GpuException>(() =>
            test.AllocateMemory_InvalidDevice_ThrowsGpuException());
    }
}
```

```csharp
using Xunit;
using GpuImageProcessing.Services;
using GpuImageProcessing.Tests;

public class GpuManagementServiceTests : IDisposable
{
    private readonly GpuManagementServiceTests _testFixture;

    public GpuManagementServiceTests()
    {
        _testFixture = new GpuManagementServiceTests();
    }

    public void Dispose()
    {
        // Clean up any shared state if necessary
    }

    [Fact]
    public void AllocateMemory_SufficientMemory_Succeeds()
    {
        // Arrange
        // (test fixture already configured with a valid device and enough memory)

        // Act & Assert
        _testFixture.AllocateMemory_SufficientMemory_ReturnsTrue();
        Assert.True(true); // placeholder – actual assertions are inside the test method
    }
}
```

## Notes

- All test methods are **void**; they express their expectations through assertions internal to the method. A test passes when no assertion exception is thrown.
- The class holds no static state; each test method should be executed with a fresh instance of `GpuManagementServiceTests` to avoid cross‑test contamination caused by the mutable state of the underlying `GpuManagementService`.
- Thread safety is not a concern for the test class itself, but the tested `GpuManagementService` is **not** thread‑safe. Consequently, tests that allocate or deallocate memory must not run concurrently against the same service instance.
- Edge cases covered include zero or negative byte requests, invalid device identifiers, insufficient memory, and insufficient compute units. The tests verify that the service reacts predictably (returning `false` or throwing `GpuException`) in these scenarios.
- When extending the test suite, follow the naming convention: `<MethodUnderTest>_<Scenario>_<ExpectedOutcome>` to maintain readability and consistency with the existing tests.
