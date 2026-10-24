#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GpuImageProcessing.Configuration;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ComputeShaderPipeline"/> covering constructor guards,
    /// pass validation, dispatch ordering, failure reporting, cancellation,
    /// workgroup optimisation, and statistics.
    /// </summary>
    public class ComputeShaderPipelineTests
    {
        private readonly Guid _deviceId = Guid.NewGuid();
        private readonly GpuDevice _device;
        private readonly WorkgroupConfiguration _config = new() { WorkgroupSizeX = 16, WorkgroupSizeY = 16, EstimatedOccupancy = 0.75 };
        private readonly Mock<IWorkgroupOptimizer> _optimizer = new();
        private readonly Mock<GpuManagementService> _gpu = new(Mock.Of<ILogger<GpuManagementService>>());
        private readonly Mock<PerformanceMonitoringService> _perf = new();

        public ComputeShaderPipelineTests()
        {
            _device = new GpuDevice { Id = _deviceId, Name = "Test GPU", IsAvailable = true };
            _gpu.Setup(g => g.GetDeviceById(_deviceId)).Returns(_device);
            _gpu.Setup(g => g.AllocateMemory(It.IsAny<long>(), _deviceId)).Returns(true);
            _optimizer.Setup(o => o.Compute(It.IsAny<GpuDevice>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<WorkgroupOptimizationStrategy>())).Returns(_config);
        }

        private ComputeShaderPipeline CreatePipeline() => new(
            _optimizer.Object, _gpu.Object, _perf.Object,
            new ComputeShaderPipelineOptions(), Mock.Of<ILogger<ComputeShaderPipeline>>());

        [Fact]
        public void Constructor_NullDependency_ThrowsArgumentNullException()
        {
            var logger = Mock.Of<ILogger<ComputeShaderPipeline>>();
            var options = new ComputeShaderPipelineOptions();

            Action noOptimizer = () => new ComputeShaderPipeline(null!, _gpu.Object, _perf.Object, options, logger);
            Action noGpu = () => new ComputeShaderPipeline(_optimizer.Object, null!, _perf.Object, options, logger);
            Action noPerf = () => new ComputeShaderPipeline(_optimizer.Object, _gpu.Object, null!, options, logger);
            Action noOptions = () => new ComputeShaderPipeline(_optimizer.Object, _gpu.Object, _perf.Object, null!, logger);
            Action noLogger = () => new ComputeShaderPipeline(_optimizer.Object, _gpu.Object, _perf.Object, options, null!);

            noOptimizer.Should().Throw<ArgumentNullException>().WithParameterName("optimizer");
            noGpu.Should().Throw<ArgumentNullException>().WithParameterName("gpuService");
            noPerf.Should().Throw<ArgumentNullException>().WithParameterName("performanceMonitor");
            noOptions.Should().Throw<ArgumentNullException>().WithParameterName("options");
            noLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
            ((Action)(() => CreatePipeline())).Should().NotThrow();
        }

        [Fact]
        public async Task ExecuteAsync_TwoPasses_RunsByPriorityAndReleasesMemory()
        {
            var image = new Image(64, 32, 4);
            var late = new ComputeShaderPass("late", priority: 5);
            var early = new ComputeShaderPass("early", priority: 1);
            early.InputImages.Add(image);

            var result = await CreatePipeline().ExecuteAsync(new[] { late, early }, _deviceId);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
            result.PassesExecuted.Should().Be(2);
            result.PassesFailed.Should().Be(0);
            result.PassRecords.Select(r => r.PassId).Should().ContainInOrder(early.Id, late.Id);
            result.TotalPixelsProcessed.Should().Be(image.CalculatePixelDataSize());
            result.DeviceId.Should().Be(_deviceId);
            _gpu.Verify(g => g.DeallocateMemory(It.IsAny<long>(), _deviceId), Times.Once);
            _perf.Verify(p => p.RecordOperation(It.IsAny<double>(), true), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidPassCount_ThrowsOrAcceptsAtLimit()
        {
            Func<Task> nullPasses = () => CreatePipeline().ExecuteAsync(null!, _deviceId);
            Func<Task> emptyPasses = () => CreatePipeline().ExecuteAsync(Array.Empty<ComputeShaderPass>(), _deviceId);
            var tooDeep = Enumerable.Range(0, 65).Select(i => new ComputeShaderPass($"k{i}")).ToList();
            Func<Task> depthExceeded = () => CreatePipeline().ExecuteAsync(tooDeep, _deviceId);
            var atLimit = Enumerable.Range(0, 64).Select(i => new ComputeShaderPass($"k{i}")).ToList();

            await nullPasses.Should().ThrowAsync<ArgumentNullException>().WithParameterName("passes");
            await emptyPasses.Should().ThrowAsync<ArgumentException>().WithParameterName("passes");
            await depthExceeded.Should().ThrowAsync<ArgumentException>().WithParameterName("passes");

            var result = await CreatePipeline().ExecuteAsync(atLimit, _deviceId);
            result.PassesExecuted.Should().Be(64);
        }

        [Fact]
        public async Task ExecuteAsync_UnavailableDevice_ThrowsGpuExceptionWithDeviceNotAvailableCode()
        {
            _gpu.Setup(g => g.GetDeviceById(_deviceId))
                .Returns(new GpuDevice { Id = _deviceId, Name = "Offline GPU", IsAvailable = false });

            Func<Task> act = () => CreatePipeline().ExecuteAsync(new[] { new ComputeShaderPass("blur") }, _deviceId);

            var ex = await act.Should().ThrowAsync<GpuException>();
            ex.Which.ErrorCode.Should().Be(AppConstants.ErrorCodes.DeviceNotAvailable);
            ex.Which.DeviceName.Should().Be("Offline GPU");
        }

        [Fact]
        public async Task ExecuteAsync_OptimizerThrows_ReturnsFailedResultAndReleasesMemory()
        {
            _optimizer.Setup(o => o.Compute(It.IsAny<GpuDevice>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<WorkgroupOptimizationStrategy>()))
                .Throws(new InvalidOperationException("optimizer boom"));

            var result = await CreatePipeline().ExecuteAsync(new[] { new ComputeShaderPass("blur") }, _deviceId);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("optimizer boom");
            result.PassesExecuted.Should().Be(0);
            _gpu.Verify(g => g.DeallocateMemory(It.IsAny<long>(), _deviceId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_CancelledToken_ThrowsOperationCanceledAndReleasesMemory()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var pass = new ComputeShaderPass("blur");

            Func<Task> act = () => CreatePipeline().ExecuteAsync(new[] { pass }, _deviceId, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
            _gpu.Verify(g => g.DeallocateMemory(It.IsAny<long>(), _deviceId), Times.Once);
        }

        [Fact]
        public async Task OptimizeWorkgroupAsync_ImageAttached_StoresConfigurationOnPass()
        {
            var pass = new ComputeShaderPass("blur");
            pass.InputImages.Add(new Image(640, 480, 3));

            var result = await CreatePipeline().OptimizeWorkgroupAsync(pass, _deviceId);

            result.Should().BeSameAs(_config);
            pass.WorkgroupConfiguration.Should().BeSameAs(_config);
            _optimizer.Verify(o => o.Compute(_device, 640, 480, It.IsAny<int>(),
                It.IsAny<WorkgroupOptimizationStrategy>()), Times.Once);

            Func<Task> nullPass = () => CreatePipeline().OptimizeWorkgroupAsync(null!, _deviceId);
            await nullPass.Should().ThrowAsync<ArgumentNullException>().WithParameterName("pass");
        }

        [Fact]
        public async Task Statistics_AfterRun_CountsAndResetClearsCounters()
        {
            var pipeline = CreatePipeline();
            var image = new Image(16, 16, 4);
            var pass = new ComputeShaderPass("blur");
            pass.InputImages.Add(image);

            await pipeline.ExecuteAsync(new[] { pass }, _deviceId);

            var stats = await pipeline.GetStatisticsAsync();
            stats.TotalExecutions.Should().Be(1);
            stats.TotalPassesExecuted.Should().Be(1);
            stats.TotalPixelsProcessed.Should().Be(image.CalculatePixelDataSize());
            stats.SuccessRate.Should().Be(1.0);

            await pipeline.ResetStatisticsAsync();

            var reset = await pipeline.GetStatisticsAsync();
            reset.TotalExecutions.Should().Be(0);
            reset.TotalPassesExecuted.Should().Be(0);
            reset.TotalPixelsProcessed.Should().Be(0);
            reset.SuccessRate.Should().Be(1.0);
        }
    }
}
