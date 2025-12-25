using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ThaiIDCardReader.Models;

namespace ThaiIDCardReader.Services;

/// <summary>
/// Service for Excel file operations
/// </summary>
public class ExcelService
{
    private const string DEFAULT_EXCEL_FILE = "ThaiIDCardHistory.xlsx";

    static ExcelService()
    {
        // Set EPPlus license context (required for v5+)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Export card history to Excel file
    /// </summary>
    public void ExportToExcel(List<CardHistory> historyList, string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        using var package = new ExcelPackage(fileInfo);

        // Remove existing worksheet if present
        var existingWorksheet = package.Workbook.Worksheets["Card History"];
        if (existingWorksheet != null)
        {
            package.Workbook.Worksheets.Delete(existingWorksheet);
        }

        // Create new worksheet
        var worksheet = package.Workbook.Worksheets.Add("Card History");

        // Set headers
        var headers = new[]
        {
            "ลำดับ", "วันที่/เวลาอ่าน", "เลขบัตรประชาชน", "ชื่อ-นามสกุล (ไทย)",
            "ชื่อ-นามสกุล (อังกฤษ)", "วันเกิด", "ที่อยู่", "ผู้ออกบัตร",
            "รหัสผู้ออกบัตร", "วันออกบัตร", "วันหมดอายุ", "ข้อมูลการ์ด",
            "เครื่องอ่าน", "หมายเหตุ"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
        }

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Add data
        for (int i = 0; i < historyList.Count; i++)
        {
            var row = i + 2;
            var history = historyList[i];

            worksheet.Cells[row, 1].Value = i + 1;
            worksheet.Cells[row, 2].Value = history.ReadDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 3].Value = history.PersonalID;
            worksheet.Cells[row, 4].Value = history.NameTH;
            worksheet.Cells[row, 5].Value = history.NameEN;
            worksheet.Cells[row, 6].Value = history.BirthDate;
            worksheet.Cells[row, 7].Value = history.Address;
            worksheet.Cells[row, 8].Value = history.Issuer;
            worksheet.Cells[row, 9].Value = history.IssuerCode;
            worksheet.Cells[row, 10].Value = history.IssueDate;
            worksheet.Cells[row, 11].Value = history.ExpireDate;
            worksheet.Cells[row, 12].Value = history.CardInfo;
            worksheet.Cells[row, 13].Value = history.ReaderName;
            worksheet.Cells[row, 14].Value = history.Notes;

            // Add borders
            using (var range = worksheet.Cells[row, 1, row, headers.Length])
            {
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Set minimum column widths
        for (int col = 1; col <= headers.Length; col++)
        {
            if (worksheet.Column(col).Width < 10)
            {
                worksheet.Column(col).Width = 10;
            }
            if (worksheet.Column(col).Width > 50)
            {
                worksheet.Column(col).Width = 50;
            }
        }

        // Save
        package.Save();
    }

    /// <summary>
    /// Append new record to existing Excel file
    /// </summary>
    public void AppendToExcel(CardHistory history, string? filePath = null)
    {
        filePath ??= DEFAULT_EXCEL_FILE;
        var fileInfo = new FileInfo(filePath);

        ExcelPackage package;
        ExcelWorksheet worksheet;

        if (fileInfo.Exists)
        {
            // Open existing file
            package = new ExcelPackage(fileInfo);
            worksheet = package.Workbook.Worksheets["Card History"]
                        ?? package.Workbook.Worksheets.Add("Card History");
        }
        else
        {
            // Create new file
            package = new ExcelPackage(fileInfo);
            worksheet = package.Workbook.Worksheets.Add("Card History");

            // Add headers
            var headers = new[]
            {
                "ลำดับ", "วันที่/เวลาอ่าน", "เลขบัตรประชาชน", "ชื่อ-นามสกุล (ไทย)",
                "ชื่อ-นามสกุล (อังกฤษ)", "วันเกิด", "ที่อยู่", "ผู้ออกบัตร",
                "รหัสผู้ออกบัตร", "วันออกบัตร", "วันหมดอายุ", "ข้อมูลการ์ด",
                "เครื่องอ่าน", "หมายเหตุ"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
        }

        // Find next row
        int nextRow = worksheet.Dimension?.End.Row + 1 ?? 2;

        // Add new data
        worksheet.Cells[nextRow, 1].Value = nextRow - 1;
        worksheet.Cells[nextRow, 2].Value = history.ReadDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        worksheet.Cells[nextRow, 3].Value = history.PersonalID;
        worksheet.Cells[nextRow, 4].Value = history.NameTH;
        worksheet.Cells[nextRow, 5].Value = history.NameEN;
        worksheet.Cells[nextRow, 6].Value = history.BirthDate;
        worksheet.Cells[nextRow, 7].Value = history.Address;
        worksheet.Cells[nextRow, 8].Value = history.Issuer;
        worksheet.Cells[nextRow, 9].Value = history.IssuerCode;
        worksheet.Cells[nextRow, 10].Value = history.IssueDate;
        worksheet.Cells[nextRow, 11].Value = history.ExpireDate;
        worksheet.Cells[nextRow, 12].Value = history.CardInfo;
        worksheet.Cells[nextRow, 13].Value = history.ReaderName;
        worksheet.Cells[nextRow, 14].Value = history.Notes;

        // Auto-fit
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Save
        package.Save();
        package.Dispose();
    }

    /// <summary>
    /// Import card history from Excel file
    /// </summary>
    public List<CardHistory> ImportFromExcel(string filePath)
    {
        var historyList = new List<CardHistory>();
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            return historyList;
        }

        using var package = new ExcelPackage(fileInfo);
        var worksheet = package.Workbook.Worksheets["Card History"];

        if (worksheet == null || worksheet.Dimension == null)
        {
            return historyList;
        }

        // Skip header row, start from row 2
        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            try
            {
                var history = new CardHistory
                {
                    ReadDateTime = DateTime.Parse(worksheet.Cells[row, 2].Text),
                    PersonalID = worksheet.Cells[row, 3].Text,
                    NameTH = worksheet.Cells[row, 4].Text,
                    NameEN = worksheet.Cells[row, 5].Text,
                    BirthDate = worksheet.Cells[row, 6].Text,
                    Address = worksheet.Cells[row, 7].Text,
                    Issuer = worksheet.Cells[row, 8].Text,
                    IssuerCode = worksheet.Cells[row, 9].Text,
                    IssueDate = worksheet.Cells[row, 10].Text,
                    ExpireDate = worksheet.Cells[row, 11].Text,
                    CardInfo = worksheet.Cells[row, 12].Text,
                    ReaderName = worksheet.Cells[row, 13].Text,
                    Notes = worksheet.Cells[row, 14].Text
                };

                historyList.Add(history);
            }
            catch
            {
                // Skip invalid rows
                continue;
            }
        }

        return historyList;
    }

    /// <summary>
    /// Get default Excel file path
    /// </summary>
    public string GetDefaultExcelPath()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var appFolder = Path.Combine(documentsPath, "ThaiIDCardReader");

        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        return Path.Combine(appFolder, DEFAULT_EXCEL_FILE);
    }
}
