// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// Enumeration of processing job status states
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>Job is queued and waiting to be processed</summary>
        Pending = 0,

        /// <summary>Job is currently being processed</summary>
        Running = 1,

        /// <summary>Job has completed successfully</summary>
        Completed = 2,

        /// <summary>Job failed during processing</summary>
        Failed = 3,

        /// <summary>Job was cancelled by user</summary>
        Cancelled = 4,

        /// <summary>Job is paused and can be resumed</summary>
        Paused = 5,

        /// <summary>Job encountered a warning but continues processing</summary>
        WarningState = 6
    }

    /// <summary>
    /// Helper class for processing status operations
    /// </summary>
    public static class ProcessingStatusHelper
    {
        /// <summary>
        /// Checks if the status represents a terminal state
        /// </summary>
        public static bool IsTerminal(ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Cancelled => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if processing is still active
        /// </summary>
        public static bool IsActive(ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Running or ProcessingStatus.Pending or ProcessingStatus.WarningState => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets a human-readable status description
        /// </summary>
        public static string GetDescription(ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Pending => "Waiting in queue for processing",
                ProcessingStatus.Running => "Currently processing images",
                ProcessingStatus.Completed => "Successfully completed",
                ProcessingStatus.Failed => "Failed during processing",
                ProcessingStatus.Cancelled => "Cancelled by user",
                ProcessingStatus.Paused => "Paused (can be resumed)",
                ProcessingStatus.WarningState => "Processing with warnings",
                _ => "Unknown status"
            };
        }

        /// <summary>
        /// Gets a status emoji representation
        /// </summary>
        public static string GetEmoji(ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Pending => "⏳",
                ProcessingStatus.Running => "🔄",
                ProcessingStatus.Completed => "✓",
                ProcessingStatus.Failed => "✗",
                ProcessingStatus.Cancelled => "⊘",
                ProcessingStatus.Paused => "⏸",
                ProcessingStatus.WarningState => "⚠",
                _ => "?"
            };
        }
    }
}
