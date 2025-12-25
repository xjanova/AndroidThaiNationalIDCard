using System;
using System.Collections.Generic;
using System.Linq;
using PCSC;
using PCSC.Iso7816;
using PCSC.Monitoring;

namespace ThaiIDCardReader.Services;

/// <summary>
/// Service for managing smart card reader connections using PC/SC
/// </summary>
public class SmartCardService : IDisposable
{
    private ISCardContext? _context;
    private ISCardMonitor? _monitor;
    private string? _readerName;
    private bool _disposed;

    public event EventHandler<string>? CardInserted;
    public event EventHandler<string>? CardRemoved;
    public event EventHandler<string>? ReaderConnected;
    public event EventHandler<string>? ReaderDisconnected;

    /// <summary>
    /// Get all available smart card readers
    /// </summary>
    public List<string> GetReaders()
    {
        try
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);
            var readers = context.GetReaders();
            return readers?.ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting readers: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Start monitoring for card reader events
    /// </summary>
    public void StartMonitoring(string? readerName = null)
    {
        try
        {
            _context = ContextFactory.Instance.Establish(SCardScope.System);
            _readerName = readerName;

            var readers = _context.GetReaders();
            if (readers == null || readers.Length == 0)
            {
                throw new Exception("No smart card readers found");
            }

            // Use specified reader or first available
            var monitorReaders = readerName != null
                ? new[] { readerName }
                : readers;

            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
            _monitor.CardInserted += OnCardInserted;
            _monitor.CardRemoved += OnCardRemoved;
            _monitor.MonitorException += OnMonitorException;

            _monitor.Start(monitorReaders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting monitor: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Stop monitoring for card reader events
    /// </summary>
    public void StopMonitoring()
    {
        if (_monitor != null)
        {
            _monitor.Cancel();
            _monitor.CardInserted -= OnCardInserted;
            _monitor.CardRemoved -= OnCardRemoved;
            _monitor.MonitorException -= OnMonitorException;
            _monitor.Dispose();
            _monitor = null;
        }
    }

    /// <summary>
    /// Check if a card is present in the reader
    /// </summary>
    public bool IsCardPresent(string? readerName = null)
    {
        try
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);
            var readers = context.GetReaders();

            if (readers == null || readers.Length == 0)
                return false;

            var targetReader = readerName ?? readers[0];
            var status = context.GetStatus(new[] { targetReader });

            return status != null &&
                   status.Length > 0 &&
                   (status[0].State & SCRState.Present) != 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Connect to smart card and execute APDU commands
    /// </summary>
    public IsoReader? ConnectToCard(string? readerName = null)
    {
        try
        {
            var context = ContextFactory.Instance.Establish(SCardScope.System);
            var readers = context.GetReaders();

            if (readers == null || readers.Length == 0)
                return null;

            var targetReader = readerName ?? readers[0];

            var reader = context.ConnectReader(
                targetReader,
                SCardShareMode.Shared,
                SCardProtocol.T0 | SCardProtocol.T1);

            return new IsoReader(reader);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error connecting to card: {ex.Message}");
            return null;
        }
    }

    private void OnCardInserted(object sender, CardStatusEventArgs e)
    {
        CardInserted?.Invoke(this, e.ReaderName);
    }

    private void OnCardRemoved(object sender, CardStatusEventArgs e)
    {
        CardRemoved?.Invoke(this, e.ReaderName);
    }

    private void OnMonitorException(object sender, PCSCException e)
    {
        System.Diagnostics.Debug.WriteLine($"Monitor exception: {e.Message}");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopMonitoring();
            _context?.Dispose();
            _context = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
