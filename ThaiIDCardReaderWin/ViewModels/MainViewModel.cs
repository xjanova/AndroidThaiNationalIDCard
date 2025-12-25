using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThaiIDCardReader.Models;
using ThaiIDCardReader.Services;

namespace ThaiIDCardReader.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SmartCardService _smartCardService;
    private readonly ThaiIDCardService _thaiIDCardService;

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
    private bool _isPinDialogOpen;

    [ObservableProperty]
    private string _pinInput = string.Empty;

    [ObservableProperty]
    private string _pinMessage = string.Empty;

    [ObservableProperty]
    private int _remainingPinAttempts;

    public MainViewModel()
    {
        _smartCardService = new SmartCardService();
        _thaiIDCardService = new ThaiIDCardService(_smartCardService);

        // Subscribe to card events
        _smartCardService.CardInserted += OnCardInserted;
        _smartCardService.CardRemoved += OnCardRemoved;
        _smartCardService.ReaderConnected += OnReaderConnected;
        _smartCardService.ReaderDisconnected += OnReaderDisconnected;
    }

    partial void OnSelectedReaderChanged(string? value)
    {
        CheckCardStatus();
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

                        // Start monitoring
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
                // Read personal information
                var personalInfo = _thaiIDCardService.ReadPersonalInformation(SelectedReader);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (personalInfo != null)
                    {
                        PersonalInformation = personalInfo;
                        StatusMessage = "อ่านข้อมูลสำเร็จ";
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

    private void OnReaderConnected(object? sender, string readerName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"เครื่องอ่านบัตรเชื่อมต่อ: {readerName}";
            _ = InitializeAsync();
        });
    }

    private void OnReaderDisconnected(object? sender, string readerName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"เครื่องอ่านบัตรถูกถอด: {readerName}";
        });
    }

    public void Cleanup()
    {
        _smartCardService.StopMonitoring();
        _smartCardService.Dispose();
    }
}
