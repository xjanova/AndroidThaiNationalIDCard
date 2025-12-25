using System;
using System.Linq;
using System.Text;
using PCSC;
using PCSC.Iso7816;
using ThaiIDCardReader.Helpers;
using ThaiIDCardReader.Models;

namespace ThaiIDCardReader.Services;

/// <summary>
/// Service for writing data to Thai National ID Card (Government Use Only)
/// ⚠️ WARNING: Requires special authorization and hardware
/// </summary>
public class CardWriteService
{
    private readonly SmartCardService _smartCardService;

    public CardWriteService(SmartCardService smartCardService)
    {
        _smartCardService = smartCardService;
    }

    /// <summary>
    /// Write result status
    /// </summary>
    public enum WriteStatus
    {
        Success,
        AuthenticationRequired,
        InsufficientPermission,
        CardLocked,
        WriteProtected,
        InvalidData,
        HardwareNotSupported,
        Error
    }

    /// <summary>
    /// Update address on card (requires admin PIN)
    /// </summary>
    public WriteStatus UpdateAddress(string newAddress, string adminPin, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return WriteStatus.Error;

        try
        {
            // Verify admin PIN first
            if (!VerifyAdminPin(reader, adminPin))
            {
                return WriteStatus.InsufficientPermission;
            }

            // Select storage applet
            if (!SelectAppletStorageData(reader))
            {
                return WriteStatus.Error;
            }

            // Prepare address data (max 160 bytes)
            byte[] addressBytes = Encoding.GetEncoding("TIS-620").GetBytes(newAddress);
            if (addressBytes.Length > 160)
            {
                addressBytes = addressBytes.Take(160).ToArray();
            }

            // Pad with spaces
            byte[] paddedAddress = new byte[160];
            Array.Copy(addressBytes, paddedAddress, addressBytes.Length);
            for (int i = addressBytes.Length; i < 160; i++)
            {
                paddedAddress[i] = 0x20; // Space
            }

            // Write address data in blocks
            int offset = 0x1579; // Address offset on card
            int bytesWritten = 0;

            while (bytesWritten < paddedAddress.Length)
            {
                int blockSize = Math.Min(0xFF, paddedAddress.Length - bytesWritten);
                byte[] blockData = new byte[blockSize];
                Array.Copy(paddedAddress, bytesWritten, blockData, 0, blockSize);

                var writeCommand = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
                {
                    CLA = 0x00,
                    INS = 0xD6, // UPDATE BINARY
                    P1 = (byte)((offset >> 8) & 0xFF),
                    P2 = (byte)(offset & 0xFF),
                    Data = blockData
                };

                var response = reader.Transmit(writeCommand);

                if (response.SW1 != 0x90 || response.SW2 != 0x00)
                {
                    if (response.SW1 == 0x69 && response.SW2 == 0x82)
                        return WriteStatus.InsufficientPermission;
                    if (response.SW1 == 0x6D && response.SW2 == 0x00)
                        return WriteStatus.HardwareNotSupported;

                    return WriteStatus.Error;
                }

                offset += blockSize;
                bytesWritten += blockSize;
            }

            return WriteStatus.Success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating address: {ex.Message}");
            return WriteStatus.Error;
        }
    }

    /// <summary>
    /// Update personal photo on card (requires admin PIN)
    /// </summary>
    public WriteStatus UpdatePhoto(byte[] photoData, string adminPin, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return WriteStatus.Error;

        try
        {
            // Verify admin PIN
            if (!VerifyAdminPin(reader, adminPin))
            {
                return WriteStatus.InsufficientPermission;
            }

            // Select storage applet
            if (!SelectAppletStorageData(reader))
            {
                return WriteStatus.Error;
            }

            // Photo size limit: 5118 bytes
            const int MAX_PHOTO_SIZE = 5118;
            if (photoData.Length > MAX_PHOTO_SIZE)
            {
                return WriteStatus.InvalidData;
            }

            // Pad photo data
            byte[] paddedPhoto = new byte[MAX_PHOTO_SIZE];
            Array.Copy(photoData, paddedPhoto, photoData.Length);
            for (int i = photoData.Length; i < MAX_PHOTO_SIZE; i++)
            {
                paddedPhoto[i] = 0x20; // Space padding
            }

            // Write photo data in blocks
            int offset = 0x017B; // Photo offset on card
            int bytesWritten = 0;

            while (bytesWritten < paddedPhoto.Length)
            {
                int blockSize = Math.Min(0xFF, paddedPhoto.Length - bytesWritten);
                byte[] blockData = new byte[blockSize];
                Array.Copy(paddedPhoto, bytesWritten, blockData, 0, blockSize);

                var writeCommand = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
                {
                    CLA = 0x00,
                    INS = 0xD6,
                    P1 = (byte)((offset >> 8) & 0xFF),
                    P2 = (byte)(offset & 0xFF),
                    Data = blockData
                };

                var response = reader.Transmit(writeCommand);

                if (response.SW1 != 0x90 || response.SW2 != 0x00)
                {
                    if (response.SW1 == 0x69 && response.SW2 == 0x82)
                        return WriteStatus.InsufficientPermission;

                    return WriteStatus.Error;
                }

                offset += blockSize;
                bytesWritten += blockSize;
            }

            return WriteStatus.Success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating photo: {ex.Message}");
            return WriteStatus.Error;
        }
    }

    /// <summary>
    /// Update issue and expiry dates (requires admin PIN)
    /// </summary>
    public WriteStatus UpdateDates(string issueDate, string expiryDate, string adminPin, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return WriteStatus.Error;

        try
        {
            // Verify admin PIN
            if (!VerifyAdminPin(reader, adminPin))
            {
                return WriteStatus.InsufficientPermission;
            }

            // Select storage applet
            if (!SelectAppletStorageData(reader))
            {
                return WriteStatus.Error;
            }

            // Validate date format (YYYYMMDD)
            if (issueDate.Length != 8 || expiryDate.Length != 8)
            {
                return WriteStatus.InvalidData;
            }

            // Write issue date (8 bytes at offset 0x0167)
            var issueDateBytes = Encoding.ASCII.GetBytes(issueDate);
            if (!WriteDataBlock(reader, 0x0167, issueDateBytes))
            {
                return WriteStatus.Error;
            }

            // Write expiry date (8 bytes at offset 0x016F)
            var expiryDateBytes = Encoding.ASCII.GetBytes(expiryDate);
            if (!WriteDataBlock(reader, 0x016F, expiryDateBytes))
            {
                return WriteStatus.Error;
            }

            return WriteStatus.Success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating dates: {ex.Message}");
            return WriteStatus.Error;
        }
    }

    /// <summary>
    /// Update name (Thai and English) - requires admin PIN
    /// </summary>
    public WriteStatus UpdateName(string nameTH, string nameEN, string adminPin, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return WriteStatus.Error;

        try
        {
            if (!VerifyAdminPin(reader, adminPin))
            {
                return WriteStatus.InsufficientPermission;
            }

            if (!SelectAppletStorageData(reader))
            {
                return WriteStatus.Error;
            }

            // Thai name: 100 bytes at offset 0x0011
            byte[] nameTHBytes = Encoding.GetEncoding("TIS-620").GetBytes(nameTH);
            byte[] paddedNameTH = new byte[100];
            Array.Copy(nameTHBytes, paddedNameTH, Math.Min(nameTHBytes.Length, 100));
            for (int i = nameTHBytes.Length; i < 100; i++)
            {
                paddedNameTH[i] = 0x20;
            }

            if (!WriteDataBlock(reader, 0x0011, paddedNameTH))
            {
                return WriteStatus.Error;
            }

            // English name: 100 bytes at offset 0x0075
            byte[] nameENBytes = Encoding.ASCII.GetBytes(nameEN);
            byte[] paddedNameEN = new byte[100];
            Array.Copy(nameENBytes, paddedNameEN, Math.Min(nameENBytes.Length, 100));
            for (int i = nameENBytes.Length; i < 100; i++)
            {
                paddedNameEN[i] = 0x20;
            }

            if (!WriteDataBlock(reader, 0x0075, paddedNameEN))
            {
                return WriteStatus.Error;
            }

            return WriteStatus.Success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating name: {ex.Message}");
            return WriteStatus.Error;
        }
    }

    /// <summary>
    /// Verify admin PIN (different from user PIN)
    /// </summary>
    private bool VerifyAdminPin(IsoReader reader, string adminPin)
    {
        try
        {
            if (adminPin.Length != 8) // Admin PIN is typically 8 digits
                return false;

            // Select extension applet
            var selectCmd = new CommandApdu(IsoCase.Case4Short, SCardProtocol.Any)
            {
                CLA = 0x00,
                INS = 0xA4,
                P1 = 0x04,
                P2 = 0x00,
                Data = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x84, 0x06, 0x00, 0x02 }
            };

            var selectResponse = reader.Transmit(selectCmd);
            if (selectResponse.SW1 != 0x90)
                return false;

            // Verify admin PIN
            byte[] pinBytes = Encoding.ASCII.GetBytes(adminPin);
            byte[] pinData = new byte[8];
            Array.Copy(pinBytes, pinData, Math.Min(pinBytes.Length, 8));

            var verifyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = 0x00,
                INS = 0x20,
                P1 = 0x00,
                P2 = 0x01, // Admin PIN reference
                Data = pinData
            };

            var verifyResponse = reader.Transmit(verifyCmd);
            return (verifyResponse.SW1 == 0x90 && verifyResponse.SW2 == 0x00);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write data block to card
    /// </summary>
    private bool WriteDataBlock(IsoReader reader, int offset, byte[] data)
    {
        try
        {
            int bytesWritten = 0;

            while (bytesWritten < data.Length)
            {
                int blockSize = Math.Min(0xFF, data.Length - bytesWritten);
                byte[] blockData = new byte[blockSize];
                Array.Copy(data, bytesWritten, blockData, 0, blockSize);

                var writeCommand = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
                {
                    CLA = 0x00,
                    INS = 0xD6,
                    P1 = (byte)(((offset + bytesWritten) >> 8) & 0xFF),
                    P2 = (byte)((offset + bytesWritten) & 0xFF),
                    Data = blockData
                };

                var response = reader.Transmit(writeCommand);

                if (response.SW1 != 0x90 || response.SW2 != 0x00)
                {
                    return false;
                }

                bytesWritten += blockSize;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool SelectAppletStorageData(IsoReader reader)
    {
        var command = new CommandApdu(IsoCase.Case4Short, SCardProtocol.Any)
        {
            CLA = 0x00,
            INS = 0xA4,
            P1 = 0x04,
            P2 = 0x00,
            Data = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x54, 0x48, 0x00, 0x01 }
        };

        var response = reader.Transmit(command);
        return (response.SW1 == 0x90 && response.SW2 == 0x00);
    }

    /// <summary>
    /// Log write operation for audit trail
    /// </summary>
    public void LogWriteOperation(string operation, string adminId, string cardId, bool success)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                      $"Operation: {operation} | " +
                      $"Admin: {adminId} | " +
                      $"Card: {cardId} | " +
                      $"Status: {(success ? "SUCCESS" : "FAILED")}";

        System.Diagnostics.Debug.WriteLine(logEntry);

        // TODO: Write to audit log file or database
        var logPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ThaiIDCardReader",
            "AuditLog.txt"
        );

        try
        {
            System.IO.File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore logging errors
        }
    }
}
