namespace ThaiIDCardReader.Models;

/// <summary>
/// Represents personal information extracted from Thai National ID Card
/// </summary>
public class PersonalInformation
{
    public string CardInfo { get; set; } = string.Empty;
    public string PersonalID { get; set; } = string.Empty;
    public string NameTH { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PictureTag { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string IssuerCode { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string ExpireDate { get; set; } = string.Empty;

    /// <summary>
    /// Formatted birth date for display
    /// </summary>
    public string FormattedBirthDate
    {
        get
        {
            if (BirthDate.Length == 8)
            {
                return $"{BirthDate.Substring(6, 2)}/{BirthDate.Substring(4, 2)}/{BirthDate.Substring(0, 4)}";
            }
            return BirthDate;
        }
    }

    /// <summary>
    /// Formatted issue date for display
    /// </summary>
    public string FormattedIssueDate
    {
        get
        {
            if (IssueDate.Length == 8)
            {
                return $"{IssueDate.Substring(6, 2)}/{IssueDate.Substring(4, 2)}/{IssueDate.Substring(0, 4)}";
            }
            return IssueDate;
        }
    }

    /// <summary>
    /// Formatted expiry date for display
    /// </summary>
    public string FormattedExpireDate
    {
        get
        {
            if (ExpireDate.Length == 8)
            {
                return $"{ExpireDate.Substring(6, 2)}/{ExpireDate.Substring(4, 2)}/{ExpireDate.Substring(0, 4)}";
            }
            return ExpireDate;
        }
    }
}
