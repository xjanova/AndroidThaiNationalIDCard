using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using PCSC;
using PCSC.Iso7816;
using ThaiIDCardReader.Helpers;
using ThaiIDCardReader.Models;

namespace ThaiIDCardReader.Services;

/// <summary>
/// Service for reading Thai National ID Card data
/// </summary>
public class ThaiIDCardService
{
    private readonly SmartCardService _smartCardService;

    public ThaiIDCardService(SmartCardService smartCardService)
    {
        _smartCardService = smartCardService;
    }

    /// <summary>
    /// Read personal information from Thai ID card
    /// </summary>
    public PersonalInformation? ReadPersonalInformation(string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return null;

        try
        {
            // Select storage applet
            if (!SelectAppletStorageData(reader))
            {
                System.Diagnostics.Debug.WriteLine("Failed to select storage applet");
                return null;
            }

            // Read data blocks
            byte[] buffer = new byte[377];

            // Block 1-1: 0xFF bytes
            var block1_1 = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
            {
                CLA = 0x80,
                INS = 0xB0,
                P1 = 0x00,
                P2 = 0x00,
                Le = 0xFF
            };

            var response1_1 = reader.Transmit(block1_1);
            if (!IsSuccess(response1_1) || response1_1.GetData()?.Length != 0xFF)
            {
                System.Diagnostics.Debug.WriteLine("Failed to read block 1-1");
                return null;
            }

            Array.Copy(response1_1.GetData()!, 0, buffer, 0, 0xFF);

            // Block 1-2: 0x7A bytes
            var block1_2 = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
            {
                CLA = 0x80,
                INS = 0xB0,
                P1 = 0x00,
                P2 = 0xFF,
                Le = 0x7A
            };

            var response1_2 = reader.Transmit(block1_2);
            if (!IsSuccess(response1_2) || response1_2.GetData()?.Length != 0x7A)
            {
                System.Diagnostics.Debug.WriteLine("Failed to read block 1-2");
                return null;
            }

            Array.Copy(response1_2.GetData()!, 0, buffer, 0xFF, 0x7A);

            // Block 2: Address data 0xAE bytes
            var block2 = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
            {
                CLA = 0x80,
                INS = 0xB0,
                P1 = 0x15,
                P2 = 0x79,
                Le = 0xAE
            };

            var response2 = reader.Transmit(block2);
            if (!IsSuccess(response2) || response2.GetData()?.Length != 0xAE)
            {
                System.Diagnostics.Debug.WriteLine("Failed to read block 2");
                return null;
            }

            byte[] addressData = response2.GetData()!;

            // Parse data
            var personalInfo = new PersonalInformation();

            // Extract name TH (bytes 17-117)
            byte[] nameTHBuffer = new byte[100];
            Array.Copy(buffer, 17, nameTHBuffer, 0, 100);
            personalInfo.NameTH = ByteHelper.TrimAndDecodeTIS620(nameTHBuffer);

            // Extract name EN (bytes 117-217)
            byte[] nameENBuffer = new byte[100];
            Array.Copy(buffer, 117, nameENBuffer, 0, 100);
            personalInfo.NameEN = ByteHelper.TrimAndDecodeTIS620(nameENBuffer);

            // Find issuer split point
            int issuerEndIndex = 246;
            for (int i = 246; i < 346; i++)
            {
                if (buffer[i] == 0x20)
                {
                    issuerEndIndex = i;
                    break;
                }
            }

            byte[] issuerBuffer = new byte[issuerEndIndex - 246];
            Array.Copy(buffer, 246, issuerBuffer, 0, issuerEndIndex - 246);

            // Extract address (first 160 bytes of block 2)
            byte[] addressBuffer = new byte[160];
            Array.Copy(addressData, 0, addressBuffer, 0, 160);
            personalInfo.Address = ByteHelper.TrimAndDecodeTIS620(addressBuffer);

            // Extract other fields using TIS-620 encoding
            personalInfo.CardInfo = FormatCardInfo(buffer);
            personalInfo.PersonalID = ExtractString(buffer, 4, 13);
            personalInfo.BirthDate = ExtractString(buffer, 217, 8);
            personalInfo.Issuer = ByteHelper.DecodeTIS620(issuerBuffer);
            personalInfo.IssuerCode = ExtractString(buffer, 346, 13);
            personalInfo.IssueDate = ExtractString(buffer, 359, 8);
            personalInfo.ExpireDate = ExtractString(buffer, 367, 8);
            personalInfo.PictureTag = ExtractString(addressData, 160, 14);

            return personalInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading personal information: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Read personal photo from Thai ID card
    /// </summary>
    public BitmapImage? ReadPersonalPhoto(string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return null;

        try
        {
            // Select storage applet
            if (!SelectAppletStorageData(reader))
            {
                System.Diagnostics.Debug.WriteLine("Failed to select storage applet");
                return null;
            }

            const int PERSONAL_PIC_LENGTH = 5118;
            const int START_OFFSET = 0x017B;

            using var pictureStream = new MemoryStream();
            int offset = START_OFFSET;
            int bytesRead = 0;

            while (bytesRead < PERSONAL_PIC_LENGTH)
            {
                int blockLength = Math.Min(0xFF, PERSONAL_PIC_LENGTH - bytesRead);

                var command = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
                {
                    CLA = 0x80,
                    INS = 0xB0,
                    P1 = (byte)((offset >> 8) & 0xFF),
                    P2 = (byte)(offset & 0xFF),
                    Le = blockLength
                };

                var response = reader.Transmit(command);
                if (!IsSuccess(response))
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to read photo block at offset {offset}");
                    return null;
                }

                var data = response.GetData();
                if (data == null || data.Length != blockLength)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid photo block length: {data?.Length} vs {blockLength}");
                    return null;
                }

                pictureStream.Write(data, 0, blockLength);
                offset += blockLength;
                bytesRead += blockLength;
            }

            // Remove trailing spaces
            byte[] pictureData = pictureStream.ToArray();
            int actualLength = pictureData.Length;
            while (actualLength > 0 && pictureData[actualLength - 1] == 0x20)
            {
                actualLength--;
            }

            // Create BitmapImage from byte array
            using var ms = new MemoryStream(pictureData, 0, actualLength);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading photo: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verify PIN code using 3DES encryption
    /// </summary>
    public (PinVerificationResult result, int remainingAttempts) VerifyPin(string pin, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return (PinVerificationResult.Error, 0);

        try
        {
            if (pin.Length != 4)
                return (PinVerificationResult.Error, 0);

            // Select extension applet
            if (!SelectAppletExtension(reader))
            {
                System.Diagnostics.Debug.WriteLine("Failed to select extension applet");
                return (PinVerificationResult.Error, 0);
            }

            // Get PIN challenge
            var challengeCmd = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
            {
                CLA = 0x80,
                INS = 0xB4,
                P1 = 0x00,
                P2 = 0x00,
                Le = 0x20
            };

            var challengeResponse = reader.Transmit(challengeCmd);
            if (!IsSuccess(challengeResponse) || challengeResponse.GetData()?.Length != 0x20)
            {
                System.Diagnostics.Debug.WriteLine("Failed to get PIN challenge");
                return (PinVerificationResult.Error, 0);
            }

            byte[] challenge = challengeResponse.GetData()!;

            // Prepare PIN
            byte[] expandPin = Enumerable.Repeat((byte)0xFF, 32).ToArray();
            for (int i = 0; i < 4; i++)
            {
                expandPin[i] = (byte)pin[i];
            }

            // Hash PIN with SHA-1
            byte[] sha1Hash;
            using (var sha1 = SHA1.Create())
            {
                sha1Hash = sha1.ComputeHash(expandPin);
            }

            // Derive key
            byte[] deriveKey = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                deriveKey[i] = (byte)i;
            }

            for (int i = 0, j = 0; i < 20; i += 4, j += 7)
            {
                Array.Copy(sha1Hash, i, deriveKey, j, Math.Min(4, 20 - i));
            }

            // Encrypt challenge with 3DES
            byte[] inputData = new byte[32];
            Array.Copy(challenge, 0, inputData, 0, 0x20);

            byte[] answerData = new byte[32];
            byte[] keyBytes = new byte[24];
            byte[] iv = new byte[8];

            for (int offset = 0; offset <= 16; offset += 8)
            {
                Array.Fill(iv, (byte)0);
                Array.Copy(deriveKey, offset, keyBytes, 0, 8);
                Array.Copy(deriveKey, offset + 8, keyBytes, 8, 8);
                Array.Copy(deriveKey, offset, keyBytes, 16, 8);

                using var tdes = TripleDES.Create();
                tdes.Key = keyBytes;
                tdes.IV = iv;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.None;

                using var encryptor = tdes.CreateEncryptor();
                answerData = encryptor.TransformFinalBlock(inputData, 0, inputData.Length);

                if (answerData.Length == 32)
                {
                    Array.Copy(answerData, 0, inputData, 0, 0x20);
                }
                else if (answerData.Length == 40)
                {
                    Array.Copy(answerData, 8, inputData, 0, 0x20);
                }
                else
                {
                    return (PinVerificationResult.Error, 0);
                }
            }

            // Send verification
            byte[] verifyCommand = new byte[37];
            verifyCommand[0] = 0x80;
            verifyCommand[1] = 0x20;
            verifyCommand[2] = 0x01;
            verifyCommand[3] = 0x00;
            verifyCommand[4] = 0x20;
            Array.Copy(answerData, 0, verifyCommand, 5, Math.Min(32, answerData.Length));

            var verifyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = verifyCommand[0],
                INS = verifyCommand[1],
                P1 = verifyCommand[2],
                P2 = verifyCommand[3],
                Data = verifyCommand.Skip(5).Take(32).ToArray()
            };

            var verifyResponse = reader.Transmit(verifyCmd);
            var responseData = verifyResponse.GetData();

            if (responseData != null && responseData.Length >= 2)
            {
                if (responseData[0] == 0x63)
                {
                    // PIN incorrect
                    int remaining = responseData[1];
                    return (PinVerificationResult.Failed, remaining);
                }
                else if (responseData[0] == 0x90)
                {
                    // PIN correct
                    return (PinVerificationResult.Success, 0);
                }
            }

            return (PinVerificationResult.Error, 0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error verifying PIN: {ex.Message}");
            return (PinVerificationResult.Error, 0);
        }
    }

    /// <summary>
    /// Get card ID
    /// </summary>
    public string? GetCardId(string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return null;

        try
        {
            if (!SelectAppletChipData(reader))
                return null;

            var command = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
            {
                CLA = 0x80,
                INS = 0xCA,
                P1 = 0x9F,
                P2 = 0x7F,
                Le = 45
            };

            var response = reader.Transmit(command);
            if (!IsSuccess(response) || response.GetData()?.Length != 45)
                return null;

            byte[] data = response.GetData()!;
            byte[] cardId = new byte[8];
            Array.Copy(data, 13, cardId, 0, 8);

            return ByteHelper.ToHexString(cardId);
        }
        catch
        {
            return null;
        }
    }

    #region Private Helper Methods

    private bool SelectAppletChipData(IsoReader reader)
    {
        var command = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
        {
            CLA = 0x00,
            INS = 0xA4,
            P1 = 0x04,
            P2 = 0x00,
            Le = 0
        };

        var response = reader.Transmit(command);
        return IsSuccess(response);
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
        return IsSuccess(response);
    }

    private bool SelectAppletExtension(IsoReader reader)
    {
        var command = new CommandApdu(IsoCase.Case4Short, SCardProtocol.Any)
        {
            CLA = 0x00,
            INS = 0xA4,
            P1 = 0x04,
            P2 = 0x00,
            Data = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x84, 0x06, 0x00, 0x02 }
        };

        var response = reader.Transmit(command);
        return IsSuccess(response);
    }

    private bool IsSuccess(Response response)
    {
        return response.SW1 == 0x90 && response.SW2 == 0x00 ||
               response.SW1 == 0x61 ||
               response.SW1 == 0x6C;
    }

    private string ExtractString(byte[] buffer, int offset, int length)
    {
        byte[] data = new byte[length];
        Array.Copy(buffer, offset, data, 0, length);
        return ByteHelper.DecodeTIS620(data);
    }

    private string FormatCardInfo(byte[] buffer)
    {
        var part1 = ExtractString(buffer, 375, 2);
        var part2 = ExtractString(buffer, 0, 4);
        var part3 = ExtractString(buffer, 226, 11);
        var part4 = ExtractString(buffer, 238, 8);
        return $"{part1}-{part2}-{part3}-{part4}";
    }

    #endregion

    #region Write Operations (Experimental - May Not Be Supported)

    /// <summary>
    /// WARNING: Thai National ID cards are typically READ-ONLY.
    /// This feature is experimental and may not work with most cards.
    /// Writing to card requires:
    /// 1. Card must support write operations
    /// 2. Must have verified PIN with admin/write privileges
    /// 3. Reader device must support write commands
    /// </summary>
    public enum WriteResult
    {
        Success,
        NotSupported,
        NoPermission,
        WriteFailed,
        Error
    }

    /// <summary>
    /// Attempt to write notes/memo to card (if supported)
    /// WARNING: Most Thai ID cards do NOT support writing
    /// </summary>
    public WriteResult WriteNotesToCard(string notes, string? readerName = null)
    {
        using var reader = _smartCardService.ConnectToCard(readerName);
        if (reader == null)
            return WriteResult.Error;

        try
        {
            // This is a theoretical implementation
            // Real Thai ID cards typically don't support writing user data

            System.Diagnostics.Debug.WriteLine("WARNING: Write operation attempted on Thai ID card");
            System.Diagnostics.Debug.WriteLine("Most Thai ID cards are READ-ONLY and will reject this operation");

            // Attempt to select a writable applet (unlikely to succeed)
            if (!SelectAppletExtension(reader))
            {
                return WriteResult.NotSupported;
            }

            // Try to write data (this will likely fail with permission error)
            byte[] notesBytes = System.Text.Encoding.UTF8.GetBytes(notes);
            if (notesBytes.Length > 255)
            {
                notesBytes = notesBytes.Take(255).ToArray();
            }

            var writeCommand = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = 0x00,
                INS = 0xD6, // UPDATE BINARY command
                P1 = 0x00,
                P2 = 0x00,
                Data = notesBytes
            };

            var response = reader.Transmit(writeCommand);

            if (response.SW1 == 0x90 && response.SW2 == 0x00)
            {
                return WriteResult.Success;
            }
            else if (response.SW1 == 0x69 && response.SW2 == 0x82)
            {
                // Security status not satisfied (no permission)
                return WriteResult.NoPermission;
            }
            else if (response.SW1 == 0x6D && response.SW2 == 0x00)
            {
                // Instruction not supported
                return WriteResult.NotSupported;
            }
            else
            {
                return WriteResult.WriteFailed;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error writing to card: {ex.Message}");
            return WriteResult.Error;
        }
    }

    /// <summary>
    /// Check if card supports write operations
    /// </summary>
    public bool SupportsWriteOperations(string? readerName = null)
    {
        // Thai National ID cards typically do NOT support write operations
        // This method returns false by default

        System.Diagnostics.Debug.WriteLine("Thai National ID cards are READ-ONLY by design");
        System.Diagnostics.Debug.WriteLine("Write operations are not supported for security reasons");

        return false;
    }

    #endregion
}
