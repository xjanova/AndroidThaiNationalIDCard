using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ThaiIDCardReader.Models;
using ThaiIDCardReader.Services;

namespace ThaiIDCardReader.ViewModels;

/// <summary>
/// ViewModel for editing and writing card data (Government use)
/// </summary>
public partial class CardEditorViewModel : ObservableObject
{
    private readonly CardWriteService _writeService;
    private readonly ThaiIDCardService _readService;
    private readonly SmartCardService _smartCardService;

    [ObservableProperty]
    private PersonalInformation? _currentCardData;

    [ObservableProperty]
    private BitmapImage? _currentPhoto;

    [ObservableProperty]
    private string _editNameTH = string.Empty;

    [ObservableProperty]
    private string _editNameEN = string.Empty;

    [ObservableProperty]
    private string _editAddress = string.Empty;

    [ObservableProperty]
    private string _editIssueDate = string.Empty;

    [ObservableProperty]
    private string _editExpireDate = string.Empty;

    [ObservableProperty]
    private BitmapImage? _editPhoto;

    [ObservableProperty]
    private string _adminPin = string.Empty;

    [ObservableProperty]
    private string _adminId = string.Empty;

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "พร้อมแก้ไขข้อมูล";

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isAuthenticationDialogOpen;

    [ObservableProperty]
    private string _selectedReader = string.Empty;

    private string _pendingOperation = string.Empty;

    public CardEditorViewModel(SmartCardService smartCardService)
    {
        _smartCardService = smartCardService;
        _writeService = new CardWriteService(_smartCardService);
        _readService = new ThaiIDCardService(_smartCardService);
    }

    /// <summary>
    /// Load current card data
    /// </summary>
    [RelayCommand]
    private async Task LoadCardDataAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "กำลังอ่านข้อมูลจากบัตร...";

            await Task.Run(() =>
            {
                var cardData = _readService.ReadPersonalInformation(SelectedReader);
                var photo = _readService.ReadPersonalPhoto(SelectedReader);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (cardData != null)
                    {
                        CurrentCardData = cardData;
                        EditNameTH = cardData.NameTH;
                        EditNameEN = cardData.NameEN;
                        EditAddress = cardData.Address;
                        EditIssueDate = cardData.IssueDate;
                        EditExpireDate = cardData.ExpireDate;

                        CurrentPhoto = photo;
                        EditPhoto = photo;

                        StatusMessage = "โหลดข้อมูลสำเร็จ - พร้อมแก้ไข";
                    }
                    else
                    {
                        StatusMessage = "ไม่สามารถอ่านข้อมูลได้";
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
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Select new photo file
    /// </summary>
    [RelayCommand]
    private void SelectPhoto()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*",
                Title = "เลือกรูปภาพ"
            };

            if (dialog.ShowDialog() == true)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(dialog.FileName);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                EditPhoto = image;
                StatusMessage = "เลือกรูปภาพใหม่แล้ว";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"เลือกรูปไม่สำเร็จ: {ex.Message}";
        }
    }

    /// <summary>
    /// Request authentication before write
    /// </summary>
    [RelayCommand]
    private void RequestAuthentication(string operation)
    {
        _pendingOperation = operation;
        AdminPin = string.Empty;
        AdminId = string.Empty;
        Reason = string.Empty;
        IsAuthenticationDialogOpen = true;
    }

    /// <summary>
    /// Authenticate and proceed with write operation
    /// </summary>
    [RelayCommand]
    private async Task AuthenticateAndWriteAsync()
    {
        if (string.IsNullOrEmpty(AdminPin) || AdminPin.Length != 8)
        {
            MessageBox.Show("กรุณาใส่ Admin PIN 8 หลัก", "ข้อมูลไม่ครบ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(AdminId))
        {
            MessageBox.Show("กรุณาใส่รหัสเจ้าหน้าที่", "ข้อมูลไม่ครบ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(Reason))
        {
            MessageBox.Show("กรุณาระบุเหตุผล", "ข้อมูลไม่ครบ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsAuthenticationDialogOpen = false;
        await ExecuteWriteOperationAsync(_pendingOperation);
    }

    /// <summary>
    /// Execute write operation based on type
    /// </summary>
    private async Task ExecuteWriteOperationAsync(string operation)
    {
        try
        {
            IsProcessing = true;
            StatusMessage = $"กำลัง{operation}...";

            CardWriteService.WriteStatus result = CardWriteService.WriteStatus.Error;

            await Task.Run(() =>
            {
                switch (operation)
                {
                    case "อัปเดตชื่อ":
                        result = _writeService.UpdateName(EditNameTH, EditNameEN, AdminPin, SelectedReader);
                        break;

                    case "อัปเดตที่อยู่":
                        result = _writeService.UpdateAddress(EditAddress, AdminPin, SelectedReader);
                        break;

                    case "อัปเดตวันที่":
                        result = _writeService.UpdateDates(EditIssueDate, EditExpireDate, AdminPin, SelectedReader);
                        break;

                    case "อัปเดตรูปภาพ":
                        var photoBytes = ImageToBytes(EditPhoto);
                        if (photoBytes != null)
                        {
                            result = _writeService.UpdatePhoto(photoBytes, AdminPin, SelectedReader);
                        }
                        break;

                    case "อัปเดตทั้งหมด":
                        result = UpdateAll();
                        break;
                }

                // Log operation
                _writeService.LogWriteOperation(
                    operation,
                    AdminId,
                    CurrentCardData?.PersonalID ?? "Unknown",
                    result == CardWriteService.WriteStatus.Success
                );
            });

            // Show result
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowWriteResult(operation, result);
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            MessageBox.Show($"เกิดข้อผิดพลาด:\n{ex.Message}", "ข้อผิดพลาด",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
            AdminPin = string.Empty; // Clear sensitive data
        }
    }

    /// <summary>
    /// Update all fields
    /// </summary>
    private CardWriteService.WriteStatus UpdateAll()
    {
        // Update name
        var nameResult = _writeService.UpdateName(EditNameTH, EditNameEN, AdminPin, SelectedReader);
        if (nameResult != CardWriteService.WriteStatus.Success)
            return nameResult;

        // Update address
        var addressResult = _writeService.UpdateAddress(EditAddress, AdminPin, SelectedReader);
        if (addressResult != CardWriteService.WriteStatus.Success)
            return addressResult;

        // Update dates
        var datesResult = _writeService.UpdateDates(EditIssueDate, EditExpireDate, AdminPin, SelectedReader);
        if (datesResult != CardWriteService.WriteStatus.Success)
            return datesResult;

        // Update photo (if changed)
        if (EditPhoto != CurrentPhoto)
        {
            var photoBytes = ImageToBytes(EditPhoto);
            if (photoBytes != null)
            {
                var photoResult = _writeService.UpdatePhoto(photoBytes, AdminPin, SelectedReader);
                if (photoResult != CardWriteService.WriteStatus.Success)
                    return photoResult;
            }
        }

        return CardWriteService.WriteStatus.Success;
    }

    /// <summary>
    /// Show write result to user
    /// </summary>
    private void ShowWriteResult(string operation, CardWriteService.WriteStatus result)
    {
        string message;
        MessageBoxImage icon;

        switch (result)
        {
            case CardWriteService.WriteStatus.Success:
                message = $"{operation}สำเร็จ!\n\nข้อมูลถูกบันทึกลงบัตรเรียบร้อย";
                icon = MessageBoxImage.Information;
                StatusMessage = $"{operation}สำเร็จ";
                break;

            case CardWriteService.WriteStatus.InsufficientPermission:
                message = $"{operation}ไม่สำเร็จ\n\nAdmin PIN ไม่ถูกต้องหรือไม่มีสิทธิ์";
                icon = MessageBoxImage.Error;
                StatusMessage = "ไม่มีสิทธิ์";
                break;

            case CardWriteService.WriteStatus.HardwareNotSupported:
                message = $"{operation}ไม่สำเร็จ\n\nเครื่องอ่านบัตรไม่รองรับการเขียนข้อมูล";
                icon = MessageBoxImage.Warning;
                StatusMessage = "ฮาร์ดแวร์ไม่รองรับ";
                break;

            case CardWriteService.WriteStatus.CardLocked:
                message = $"{operation}ไม่สำเร็จ\n\nบัตรถูกล็อค";
                icon = MessageBoxImage.Error;
                StatusMessage = "บัตรถูกล็อค";
                break;

            case CardWriteService.WriteStatus.InvalidData:
                message = $"{operation}ไม่สำเร็จ\n\nข้อมูลไม่ถูกต้อง";
                icon = MessageBoxImage.Warning;
                StatusMessage = "ข้อมูลไม่ถูกต้อง";
                break;

            default:
                message = $"{operation}ไม่สำเร็จ\n\nกรุณาลองใหม่อีกครั้ง";
                icon = MessageBoxImage.Error;
                StatusMessage = "เกิดข้อผิดพลาด";
                break;
        }

        MessageBox.Show(message, "ผลการดำเนินการ", MessageBoxButton.OK, icon);
    }

    /// <summary>
    /// Cancel authentication dialog
    /// </summary>
    [RelayCommand]
    private void CancelAuthentication()
    {
        IsAuthenticationDialogOpen = false;
        AdminPin = string.Empty;
        AdminId = string.Empty;
        Reason = string.Empty;
    }

    /// <summary>
    /// Reset all edits
    /// </summary>
    [RelayCommand]
    private void ResetEdits()
    {
        if (CurrentCardData != null)
        {
            EditNameTH = CurrentCardData.NameTH;
            EditNameEN = CurrentCardData.NameEN;
            EditAddress = CurrentCardData.Address;
            EditIssueDate = CurrentCardData.IssueDate;
            EditExpireDate = CurrentCardData.ExpireDate;
            EditPhoto = CurrentPhoto;
            StatusMessage = "รีเซ็ตการแก้ไขแล้ว";
        }
    }

    /// <summary>
    /// Convert BitmapImage to byte array
    /// </summary>
    private byte[]? ImageToBytes(BitmapImage? image)
    {
        if (image == null) return null;

        try
        {
            var encoder = new JpegBitmapEncoder { QualityLevel = 85 };
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
}
