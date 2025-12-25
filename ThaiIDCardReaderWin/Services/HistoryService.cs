using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThaiIDCardReader.Models;

namespace ThaiIDCardReader.Services;

/// <summary>
/// Service for managing card reading history using simple file storage
/// </summary>
public class HistoryService
{
    private readonly ExcelService _excelService;
    private readonly string _historyFilePath;
    private List<CardHistory> _cache;

    public HistoryService(ExcelService excelService)
    {
        _excelService = excelService;
        _historyFilePath = _excelService.GetDefaultExcelPath();
        _cache = new List<CardHistory>();
        LoadCache();
    }

    /// <summary>
    /// Add new card reading record
    /// </summary>
    public void AddHistory(CardHistory history)
    {
        // Add to cache
        history.Id = _cache.Count > 0 ? _cache.Max(h => h.Id) + 1 : 1;
        _cache.Add(history);

        // Save to Excel
        try
        {
            _excelService.AppendToExcel(history, _historyFilePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving to Excel: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all history records
    /// </summary>
    public List<CardHistory> GetAllHistory()
    {
        return _cache.OrderByDescending(h => h.ReadDateTime).ToList();
    }

    /// <summary>
    /// Get history by personal ID
    /// </summary>
    public List<CardHistory> GetHistoryByPersonalId(string personalId)
    {
        return _cache
            .Where(h => h.PersonalID == personalId)
            .OrderByDescending(h => h.ReadDateTime)
            .ToList();
    }

    /// <summary>
    /// Get history within date range
    /// </summary>
    public List<CardHistory> GetHistoryByDateRange(DateTime startDate, DateTime endDate)
    {
        return _cache
            .Where(h => h.ReadDateTime >= startDate && h.ReadDateTime <= endDate)
            .OrderByDescending(h => h.ReadDateTime)
            .ToList();
    }

    /// <summary>
    /// Update existing history record
    /// </summary>
    public void UpdateHistory(CardHistory history)
    {
        var existing = _cache.FirstOrDefault(h => h.Id == history.Id);
        if (existing != null)
        {
            var index = _cache.IndexOf(existing);
            _cache[index] = history;
            SaveAllToExcel();
        }
    }

    /// <summary>
    /// Delete history record
    /// </summary>
    public void DeleteHistory(int id)
    {
        var history = _cache.FirstOrDefault(h => h.Id == id);
        if (history != null)
        {
            _cache.Remove(history);
            SaveAllToExcel();
        }
    }

    /// <summary>
    /// Clear all history
    /// </summary>
    public void ClearHistory()
    {
        _cache.Clear();
        if (File.Exists(_historyFilePath))
        {
            File.Delete(_historyFilePath);
        }
    }

    /// <summary>
    /// Export history to specific file
    /// </summary>
    public void ExportToFile(string filePath)
    {
        _excelService.ExportToExcel(_cache, filePath);
    }

    /// <summary>
    /// Import history from file
    /// </summary>
    public void ImportFromFile(string filePath)
    {
        var imported = _excelService.ImportFromExcel(filePath);
        foreach (var item in imported)
        {
            item.Id = _cache.Count > 0 ? _cache.Max(h => h.Id) + 1 : 1;
            _cache.Add(item);
        }
        SaveAllToExcel();
    }

    /// <summary>
    /// Get statistics
    /// </summary>
    public (int TotalRecords, int UniqueCards, DateTime? LastRead) GetStatistics()
    {
        return (
            TotalRecords: _cache.Count,
            UniqueCards: _cache.Select(h => h.PersonalID).Distinct().Count(),
            LastRead: _cache.Count > 0 ? _cache.Max(h => h.ReadDateTime) : null
        );
    }

    private void LoadCache()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                _cache = _excelService.ImportFromExcel(_historyFilePath);
                // Ensure IDs are set
                for (int i = 0; i < _cache.Count; i++)
                {
                    _cache[i].Id = i + 1;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading cache: {ex.Message}");
            _cache = new List<CardHistory>();
        }
    }

    private void SaveAllToExcel()
    {
        try
        {
            _excelService.ExportToExcel(_cache, _historyFilePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving to Excel: {ex.Message}");
        }
    }
}
