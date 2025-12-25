namespace ThaiIDCardReader.Models;

/// <summary>
/// Represents the status of card reader connection
/// </summary>
public enum CardReaderStatus
{
    NotConnected,
    Connected,
    CardInserted,
    Reading,
    Error
}

/// <summary>
/// Result of PIN verification
/// </summary>
public enum PinVerificationResult
{
    Success,
    Failed,
    Error,
    Locked
}
