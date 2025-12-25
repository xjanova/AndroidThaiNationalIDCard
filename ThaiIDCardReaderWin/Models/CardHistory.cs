using System;

namespace ThaiIDCardReader.Models;

/// <summary>
/// Represents a history record of card reading
/// </summary>
public class CardHistory
{
    public int Id { get; set; }
    public DateTime ReadDateTime { get; set; }
    public string PersonalID { get; set; } = string.Empty;
    public string NameTH { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string IssuerCode { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string ExpireDate { get; set; } = string.Empty;
    public string CardInfo { get; set; } = string.Empty;
    public byte[]? PhotoData { get; set; }
    public string ReaderName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Create CardHistory from PersonalInformation
    /// </summary>
    public static CardHistory FromPersonalInfo(PersonalInformation info, byte[]? photoData = null, string? readerName = null)
    {
        return new CardHistory
        {
            ReadDateTime = DateTime.Now,
            PersonalID = info.PersonalID,
            NameTH = info.NameTH,
            NameEN = info.NameEN,
            BirthDate = info.BirthDate,
            Address = info.Address,
            Issuer = info.Issuer,
            IssuerCode = info.IssuerCode,
            IssueDate = info.IssueDate,
            ExpireDate = info.ExpireDate,
            CardInfo = info.CardInfo,
            PhotoData = photoData,
            ReaderName = readerName ?? "Unknown"
        };
    }

    /// <summary>
    /// Convert to PersonalInformation
    /// </summary>
    public PersonalInformation ToPersonalInfo()
    {
        return new PersonalInformation
        {
            PersonalID = PersonalID,
            NameTH = NameTH,
            NameEN = NameEN,
            BirthDate = BirthDate,
            Address = Address,
            Issuer = Issuer,
            IssuerCode = IssuerCode,
            IssueDate = IssueDate,
            ExpireDate = ExpireDate,
            CardInfo = CardInfo
        };
    }
}
