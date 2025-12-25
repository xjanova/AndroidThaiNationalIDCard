using System;
using System.Text;

namespace ThaiIDCardReader.Helpers;

/// <summary>
/// Helper methods for byte array operations
/// </summary>
public static class ByteHelper
{
    /// <summary>
    /// Convert byte array to hex string
    /// </summary>
    public static string ToHexString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Convert hex string to byte array
    /// </summary>
    public static byte[] FromHexString(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Array.Empty<byte>();

        int length = hex.Length;
        byte[] bytes = new byte[length / 2];

        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    /// <summary>
    /// Decode TIS-620 Thai encoding to string
    /// </summary>
    public static string DecodeTIS620(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        try
        {
            var encoding = Encoding.GetEncoding("TIS-620");
            return encoding.GetString(bytes);
        }
        catch
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }

    /// <summary>
    /// Trim trailing spaces from byte array and convert to string using TIS-620
    /// </summary>
    public static string TrimAndDecodeTIS620(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        int length = bytes.Length;
        while (length > 0 && bytes[length - 1] == 0x20)
        {
            length--;
        }

        var trimmed = new byte[length];
        Array.Copy(bytes, 0, trimmed, 0, length);

        // Replace # with space
        for (int i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] == 0x23)
            {
                trimmed[i] = 0x20;
            }
        }

        return DecodeTIS620(trimmed);
    }
}
