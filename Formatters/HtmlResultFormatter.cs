#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Formats processing results as HTML for web display and reporting.
    /// Generates interactive tables and charts with embedded CSS styling.
    /// </summary>
    public class HtmlResultFormatter : IResultFormatter
    {
        public string GetFileExtension() => ".html";

        public string GetMimeType() => "text/html";

        public string FormatResult(ProcessingResult result) => Format(new List<ProcessingResult> { result });

        public string FormatResults(List<ProcessingResult> results) => Format(results);

        public string FormatJob(ProcessingJob job)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"job\">");
            html.AppendLine($"  <h2>Job: {job.Name}</h2>");
            html.AppendLine($"  <p>Status: {job.Status}</p>");
            html.AppendLine($"  <p>Progress: {job.ProcessedImages}/{job.TotalImages} ({job.ProgressPercentage:F1}%)</p>");
            if (!string.IsNullOrEmpty(job.ErrorMessage))
                html.AppendLine($"  <p class=\"error\">Error: {job.ErrorMessage}</p>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string FormatDevice(DeviceInfo device)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"device\">");
            html.AppendLine($"  <h2>Device: {device.Name}</h2>");
            html.AppendLine($"  <p>Vendor: {device.Vendor}</p>");
            html.AppendLine($"  <p>Type: {device.DeviceType}</p>");
            html.AppendLine($"  <p>Available: {device.IsAvailable}</p>");
            html.AppendLine($"  <p>Global Memory: {FormatBytes(device.GlobalMemoryBytes)}</p>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string FormatError(string errorMessage, string? errorCode = null, Exception? exception = null)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"error-report\">");
            html.AppendLine($"  <h2>Error</h2>");
            html.AppendLine($"  <p>{errorMessage}</p>");
            if (!string.IsNullOrEmpty(errorCode))
                html.AppendLine($"  <p>Code: {errorCode}</p>");
            if (exception != null)
                html.AppendLine($"  <pre>{exception}</pre>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string Format(List<ProcessingResult> results)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"UTF-8\">");
            html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("  <title>GPU Image Processing Results</title>");
            html.AppendLine(GetEmbeddedStyles());
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine("  <div class=\"container\">");
            html.AppendLine("    <h1>GPU Image Processing Results</h1>");
            html.AppendLine($"    <p class=\"timestamp\">Generated: {DateTime.UtcNow:O}</p>");

            if (results == null || results.Count == 0)
            {
                html.AppendLine("    <p class=\"no-results\">No processing results found.</p>");
            }
            else
            {
                html.AppendLine("    <div class=\"summary\">");
                html.AppendLine($"      <p>Total Results: <strong>{results.Count}</strong></p>");
                html.AppendLine("    </div>");

                html.AppendLine("    <table class=\"results-table\">");
                html.AppendLine("      <thead>");
                html.AppendLine("        <tr>");
                html.AppendLine("          <th>ID</th>");
                html.AppendLine("          <th>Image ID</th>");
                html.AppendLine("          <th>Filter/Transform</th>");
                html.AppendLine("          <th>Status</th>");
                html.AppendLine("          <th>Duration (ms)</th>");
                html.AppendLine("          <th>Output Size</th>");
                html.AppendLine("        </tr>");
                html.AppendLine("      </thead>");
                html.AppendLine("      <tbody>");

                foreach (var result in results)
                {
                    var statusClass = result.IsSuccessful ? "success" : "failure";
                    html.AppendLine("        <tr class=\"" + statusClass + "\">");
                    html.AppendLine($"          <td>{result.Id}</td>");
                    html.AppendLine($"          <td>{result.ImageId}</td>");
                    html.AppendLine($"          <td>{GetOperationLabel(result)}</td>");
                    html.AppendLine($"          <td><span class=\"status {statusClass}\">{(result.IsSuccessful ? "✓ Success" : "✗ Failed")}</span></td>");
                    html.AppendLine($"          <td>{result.ProcessingTimeMs:F2}</td>");
                    html.AppendLine($"          <td>{FormatBytes(result.OutputFileSizeBytes)}</td>");
                    html.AppendLine("        </tr>");

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        html.AppendLine("        <tr class=\"error-detail\">");
                        html.AppendLine($"          <td colspan=\"6\"><strong>Error:</strong> {result.ErrorMessage}</td>");
                        html.AppendLine("        </tr>");
                    }
                }

                html.AppendLine("      </tbody>");
                html.AppendLine("    </table>");

                html.AppendLine(GenerateStatistics(results));
            }

            html.AppendLine("  </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string GetEmbeddedStyles()
        {
            return @"
  <style>
    * {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
    }

    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: #333;
      padding: 20px;
    }

    .container {
      max-width: 1200px;
      margin: 0 auto;
      background: white;
      border-radius: 8px;
      padding: 30px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
    }

    h1 {
      color: #667eea;
      margin-bottom: 10px;
      font-size: 28px;
    }

    .timestamp {
      color: #999;
      font-size: 12px;
      margin-bottom: 20px;
    }

    .summary {
      background: #f0f4ff;
      padding: 15px;
      border-radius: 4px;
      margin-bottom: 20px;
      border-left: 4px solid #667eea;
    }

    .summary p {
      margin: 5px 0;
    }

    .results-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 30px;
    }

    .results-table thead {
      background: #f8f9fa;
      border-bottom: 2px solid #667eea;
    }

    .results-table th {
      padding: 12px;
      text-align: left;
      font-weight: 600;
      color: #333;
    }

    .results-table td {
      padding: 12px;
      border-bottom: 1px solid #eee;
    }

    .results-table tbody tr:hover {
      background: #f8f9fa;
    }

    .results-table tr.success {
      border-left: 4px solid #28a745;
    }

    .results-table tr.failure {
      border-left: 4px solid #dc3545;
    }

    .status {
      padding: 4px 8px;
      border-radius: 3px;
      font-size: 12px;
      font-weight: 600;
    }

    .status.success {
      background: #d4edda;
      color: #155724;
    }

    .status.failure {
      background: #f8d7da;
      color: #721c24;
    }

    .error-detail {
      background: #fff3cd;
      font-size: 12px;
    }

    .statistics {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 15px;
      margin-top: 20px;
    }

    .stat-card {
      background: #f8f9fa;
      padding: 15px;
      border-radius: 4px;
      border-left: 4px solid #667eea;
    }

    .stat-card h3 {
      font-size: 12px;
      color: #999;
      text-transform: uppercase;
      margin-bottom: 8px;
    }

    .stat-card .value {
      font-size: 24px;
      font-weight: 700;
      color: #667eea;
    }
  </style>";
        }

        private static string GetOperationLabel(ProcessingResult result)
        {
            var operations = result.AppliedFilters.Concat(result.AppliedTransforms).ToList();
            return operations.Count > 0 ? string.Join(", ", operations) : "N/A";
        }

        private string GenerateStatistics(List<ProcessingResult> results)
        {
            var successCount = 0;
            var failureCount = 0;
            var totalDuration = 0.0;
            var totalOutputSize = 0L;

            foreach (var result in results)
            {
                if (result.IsSuccessful)
                    successCount++;
                else
                    failureCount++;

                totalDuration += result.ProcessingTimeMs;
                totalOutputSize += result.OutputFileSizeBytes;
            }

            var successRate = (successCount / (double)results.Count) * 100;
            var avgDuration = totalDuration / results.Count;

            var html = new StringBuilder();
            html.AppendLine("    <div class=\"statistics\">");
            html.AppendLine("      <div class=\"stat-card\">");
            html.AppendLine("        <h3>Success Rate</h3>");
            html.AppendLine($"        <div class=\"value\">{successRate:F1}%</div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <div class=\"stat-card\">");
            html.AppendLine("        <h3>Avg Duration</h3>");
            html.AppendLine($"        <div class=\"value\">{avgDuration:F2}ms</div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <div class=\"stat-card\">");
            html.AppendLine("        <h3>Total Output Size</h3>");
            html.AppendLine($"        <div class=\"value\">{FormatBytes(totalOutputSize)}</div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <div class=\"stat-card\">");
            html.AppendLine("        <h3>Failed Operations</h3>");
            html.AppendLine($"        <div class=\"value\">{failureCount}</div>");
            html.AppendLine("      </div>");
            html.AppendLine("    </div>");

            return html.ToString();
        }

        private string FormatBytes(long bytes)
        {
            var size = (double)bytes;
            var units = new[] { "B", "KB", "MB", "GB" };
            var index = 0;

            while (size >= 1024 && index < units.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:F2} {units[index]}";
        }
    }
}
