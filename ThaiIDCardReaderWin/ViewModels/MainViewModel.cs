using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ThaiIDCardReader.Models;
using ThaiIDCardReader.Services;

namespace ThaiIDCardReader.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SmartCardService _smartCardService;
    private readonly ThaiIDCardService _thaiIDCardService;
    private readonly ExcelService _excelService;
    private readonly HistoryService _historyService;

    [ObservableProperty]
    private PersonalInformation? _personalInformation;

    [ObservableProperty]
    private BitmapImage? _photoImage;

    [ObservableProperty]
    private string _statusMessage = "พร้อมใช้งาน";

    [ObservableProperty]
    private bool _isReading;

    [ObservableProperty]
    private bool _isCardInserted;

    [ObservableProperty]
    private string? _selectedReader;

    [ObservableProperty]
    private ObservableCollection<string> _availableReaders = new();

    [ObservableProperty]
    private ObservableCollection<CardHistory> _historyList = new();

    [ObservableProperty]
    private CardHistory? _selectedHistory;

    [ObservableProperty]
    private bool _isPinDialogOpen;

    [ObservableProperty]
    private string _pinInput = string.Empty;

    [ObservableProperty]
    private string _pinMessage = string.Empty;

    [ObservableProperty]
    private int _remainingPinAttempts;

    [ObservableProperty]
    private bool _autoSaveToExcel = true;

    [ObservableProperty]
    private int _totalRecords;

    [ObservableProperty]
    private int _uniqueCards;

    [ObservableProperty]
    private DateTime? _lastReadDate;

    [ObservableProperty]
    private bool _isHistoryPanelOpen;

    public MainViewModel()
    {
        _smartCardService = new SmartCardService();
        _thaiIDCardService = new ThaiIDCardService(_smartCardService);
        _excelService = new ExcelService();
        _historyService = new HistoryService(_excelService);

        // Subscribe to card events
        _smartCardService.CardInserted += OnCardInserted;
        _smartCardService.CardRemoved += OnCardRemoved;

        LoadHistory();
        UpdateStatistics();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            StatusMessage = "กำลังค้นหาเครื่องอ่านบัตร...";

            await Task.Run(() =>
            {
                var readers = _smartCardService.GetReaders();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableReaders.Clear();
                    foreach (var reader in readers)
                    {
                        AvailableReaders.Add(reader);
                    }

                    if (AvailableReaders.Count > 0)
                    {
                        SelectedReader = AvailableReaders[0];
                        StatusMessage = $"พบเครื่องอ่านบัตร {AvailableReaders.Count} เครื่อง";
                        _smartCardService.StartMonitoring(SelectedReader);
                    }
                    else
                    {
                        StatusMessage = "ไม่พบเครื่องอ่านบัตร กรุณาเชื่อมต่อเครื่องอ่านบัตร";
                    }
                });
            });

            CheckCardStatus();
        }
        catch (Exception ex)
        {
            StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ReadCardAsync()
    {
        if (IsReading) return;

        try
        {
            IsReading = true;
            StatusMessage = "กำลังอ่านข้อมูลบัตร...";

            await Task.Run(() =>
            {
                var personalInfo = _thaiIDCardService.ReadPersonalInformation(SelectedReader);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (personalInfo != null)
                    {
                        PersonalInformation = personalInfo;
                        StatusMessage = "อ่านข้อมูลสำเร็จ";

                        // Auto-save to Excel if enabled
                        if (AutoSaveToExcel)
                        {
                            SaveToHistory();
                        }
                    }
                    else
                    {
                        StatusMessage = "ไม่สามารถอ่านข้อมูลได้ กรุณาลองใหม่";
                    }
                });
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
        }
        finally
        {
            IsReading = false;
        }
    }

    [RelayCommand]
    private async Task ReadPhotoAsync()
    {
        if (IsReading) return;

        try
        {
            IsReading = true;
            StatusMessage = "กำลังอ่านรูปภาพ...";

            await Task.Run(() =>
            {
                var photo = _thaiIDCardService.ReadPersonalPhoto(SelectedReader);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (photo != null)
                    {
                        PhotoImage = photo;
                        StatusMessage = "อ่านรูปภาพสำเร็จ";
                    }
                    else
                    {
                        StatusMessage = "ไม่สามารถอ่านรูปภาพได้";
                    }
                });
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
        }
        finally
        {
            IsReading = false;
        }
    }

    [RelayCommand]
    private async Task ReadAllAsync()
    {
        await ReadCardAsync();
        if (PersonalInformation != null)
        {
            await ReadPhotoAsync();
        }
    }

    [RelayCommand]
    private void SaveToHistory()
    {
        if (PersonalInformation == null) return;

        try
        {
            byte[]? photoBytes = null;
            if (PhotoImage != null)
            {
                photoBytes = ImageToBytes(PhotoImage);
            }

            var history = CardHistory.FromPersonalInfo(
                PersonalInformation,
                photoBytes,
                SelectedReader
            );

            _historyService.AddHistory(history);
            LoadHistory();
            UpdateStatistics();

            StatusMessage = "บันทึกลง Excel เรียบร้อย";
        }
        catch (Exception ex)
        {
            StatusMessage = $"ไม่สามารถบันทึกได้: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LoadHistory()
    {
        try
        {
            var history = _historyService.GetAllHistory();
            Application.Current.Dispatcher.Invoke(() =>
            {
                HistoryList.Clear();
                foreach (var item in history)
                {
                    HistoryList.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"ไม่สามารถโหลดประวัติได้: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ViewHistory(CardHistory? history)
    {
        if (history == null) return;

        PersonalInformation = history.ToPersonalInfo();

        if (history.PhotoData != null && history.PhotoData.Length > 0)
        {
            PhotoImage = BytesToImage(history.PhotoData);
        }

        StatusMessage = $"แสดงข้อมูลจากประวัติ: {history.ReadDateTime:yyyy-MM-dd HH:mm}";
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"ThaiIDCard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                IsReading = true;
                StatusMessage = "กำลัง Export...";

                await Task.Run(() =>
                {
                    _historyService.ExportToFile(dialog.FileName);
                });

                StatusMessage = $"Export สำเร็จ: {Path.GetFileName(dialog.FileName)}";
                MessageBox.Show($"Export สำเร็จ!\n{dialog.FileName}", "สำเร็จ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export ไม่สำเร็จ: {ex.Message}";
            MessageBox.Show($"เกิดข้อผิดพลาด:\n{ex.Message}", "ผิดพลาด",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsReading = false;
        }
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                IsReading = true;
                StatusMessage = "กำลัง Import...";

                await Task.Run(() =>
                {
                    _historyService.ImportFromFile(dialog.FileName);
                });

                LoadHistory();
                UpdateStatistics();
                StatusMessage = "Import สำเร็จ";
                MessageBox.Show("Import สำเร็จ!", "สำเร็จ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import ไม่สำเร็จ: {ex.Message}";
            MessageBox.Show($"เกิดข้อผิดพลาด:\n{ex.Message}", "ผิดพลาด",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsReading = false;
        }
    }

    [RelayCommand]
    private void OpenExcelFile()
    {
        try
        {
            var excelPath = _excelService.GetDefaultExcelPath();
            if (File.Exists(excelPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = excelPath,
                    UseShellExecute = true
                });
                StatusMessage = "เปิดไฟล์ Excel";
            }
            else
            {
                MessageBox.Show("ยังไม่มีไฟล์ Excel\nกรุณาอ่านบัตรอย่างน้อย 1 ครั้ง", "แจ้งเตือน",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"ไม่สามารถเปิดไฟล์ได้: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleHistoryPanel()
    {
        IsHistoryPanelOpen = !IsHistoryPanelOpen;
    }

    [RelayCommand]
    private void ClearData()
    {
        PersonalInformation = null;
        PhotoImage = null;
        StatusMessage = "ล้างข้อมูลเรียบร้อย";
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = InitializeAsync();
    }

    [RelayCommand]
    private void OpenPinDialog()
    {
        PinInput = string.Empty;
        PinMessage = "กรุณาใส่รหัส PIN 4 หลัก";
        RemainingPinAttempts = 0;
        IsPinDialogOpen = true;
    }

    [RelayCommand]
    private async Task VerifyPinAsync()
    {
        if (string.IsNullOrEmpty(PinInput) || PinInput.Length != 4)
        {
            PinMessage = "กรุณาใส่รหัส PIN 4 หลัก";
            return;
        }

        try
        {
            IsReading = true;
            PinMessage = "กำลังตรวจสอบ PIN...";

            var (result, remaining) = await Task.Run(() =>
                _thaiIDCardService.VerifyPin(PinInput, SelectedReader));

            switch (result)
            {
                case PinVerificationResult.Success:
                    PinMessage = "✓ PIN ถูกต้อง";
                    IsPinDialogOpen = false;
                    StatusMessage = "ยืนยัน PIN สำเร็จ";
                    break;

                case PinVerificationResult.Failed:
                    RemainingPinAttempts = remaining;
                    PinMessage = $"✗ PIN ไม่ถูกต้อง (เหลือ {remaining} ครั้ง)";
                    PinInput = string.Empty;
                    break;

                case PinVerificationResult.Locked:
                    PinMessage = "✗ บัตรถูกล็อค กรุณาติดต่อเจ้าหน้าที่";
                    break;

                default:
                    PinMessage = "เกิดข้อผิดพลาดในการตรวจสอบ PIN";
                    break;
            }
        }
        catch (Exception ex)
        {
            PinMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
        }
        finally
        {
            IsReading = false;
        }
    }

    [RelayCommand]
    private void ClosePinDialog()
    {
        IsPinDialogOpen = false;
        PinInput = string.Empty;
    }

    private void UpdateStatistics()
    {
        var stats = _historyService.GetStatistics();
        TotalRecords = stats.TotalRecords;
        UniqueCards = stats.UniqueCards;
        LastReadDate = stats.LastRead;
    }

    private void CheckCardStatus()
    {
        if (string.IsNullOrEmpty(SelectedReader))
        {
            IsCardInserted = false;
            return;
        }

        IsCardInserted = _smartCardService.IsCardPresent(SelectedReader);

        if (IsCardInserted)
        {
            StatusMessage = "ตรวจพบบัตร พร้อมอ่านข้อมูล";
        }
        else
        {
            StatusMessage = "ไม่พบบัตร กรุณาใส่บัตร";
        }
    }

    private void OnCardInserted(object? sender, string readerName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsCardInserted = true;
            StatusMessage = "ตรวจพบบัตร";
        });
    }

    private void OnCardRemoved(object? sender, string readerName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsCardInserted = false;
            StatusMessage = "นำบัตรออก";
            ClearData();
        });
    }

    private byte[]? ImageToBytes(BitmapImage? image)
    {
        if (image == null) return null;

        try
        {
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private BitmapImage? BytesToImage(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;

        try
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Open Card Editor Window (Government Use Only)
    /// </summary>
    [RelayCommand]
    private void OpenCardEditor()
    {
        try
        {
            // Warning dialog
            var result = MessageBox.Show(
                "⚠️ หน้าต่างนี้สำหรับเจ้าหน้าที่ราชการเท่านั้น\n\n" +
                "การแก้ไขข้อมูลบัตรประชาชนต้องได้รับอนุญาตจากหน่วยงานที่เกี่ยวข้อง\n" +
                "ต้องมี Admin PIN และระบุเหตุผลในการแก้ไขทุกครั้ง\n" +
                "ระบบจะบันทึก Audit Log ทุกการแก้ไข\n\n" +
                "คุณมีอำนาจในการแก้ไขข้อมูลหรือไม่?",
                "ยืนยันสิทธิ์",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                var editorWindow = new Views.CardEditorWindow(_smartCardService, SelectedReader);
                editorWindow.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ไม่สามารถเปิดหน้าต่างแก้ไขได้:\n{ex.Message}",
                "ข้อผิดพลาด",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void Cleanup()
    {
        _smartCardService.StopMonitoring();
        _smartCardService.Dispose();
    }
}
