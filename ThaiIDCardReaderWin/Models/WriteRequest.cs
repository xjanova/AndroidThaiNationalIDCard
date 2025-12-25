using System;

namespace ThaiIDCardReader.Models;

/// <summary>
/// Request model for write operations
/// </summary>
public class WriteRequest
{
    public string AdminPin { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestDateTime { get; set; } = DateTime.Now;
}

/// <summary>
/// Model for updating personal information
/// </summary>
public class PersonalInfoUpdateRequest : WriteRequest
{
    public string? NameTH { get; set; }
    public string? NameEN { get; set; }
    public string? Address { get; set; }
    public string? IssueDate { get; set; }
    public string? ExpireDate { get; set; }
    public byte[]? PhotoData { get; set; }
}

/// <summary>
/// Result of write operation
/// </summary>
public class WriteOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string AdminId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
}
