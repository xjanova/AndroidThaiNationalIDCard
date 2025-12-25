# ฟีเจอร์ Excel และการจัดเก็บข้อมูล
## Excel Integration & Data Management Features

---

## 🎯 ภาพรวมฟีเจอร์ใหม่

โปรแกรมตอนนี้สามารถ:
1. ✅ **บันทึกลง Excel อัตโนมัติ** - ทุกครั้งที่อ่านบัตร
2. ✅ **แสดงประวัติการอ่าน** - ดูข้อมูลย้อนหลังได้
3. ✅ **Export/Import Excel** - นำเข้า/ส่งออกไฟล์ Excel
4. ✅ **เปิดไฟล์ Excel** - เปิดดูข้อมูลโดยตรง
5. ✅ **แก้ไขข้อมูลใน Excel** - แก้ไขได้ด้วย Microsoft Excel
6. ⚠️ **เขียนลงบัตร** - ฟีเจอร์ทดลอง (บัตรไทยส่วนใหญ่ไม่รองรับ)

---

## 📊 การบันทึกอัตโนมัติ (Auto-Save)

### วิธีการทำงาน

```
1. อ่านบัตร → ReadCardAsync()
2. ตรวจสอบ AutoSaveToExcel = true
3. บันทึกอัตโนมัติ → SaveToHistory()
4. เก็บลง Excel:
   - ที่อยู่: Documents\ThaiIDCardReader\ThaiIDCardHistory.xlsx
   - รูปแบบ: ตารางพร้อม Headers
   - Encoding: TIS-620 (รองรับภาษาไทย)
```

### ข้อมูลที่บันทึก

| คอลัมน์ | ข้อมูล |
|---------|--------|
| ลำดับ | เลขที่บันทึก |
| วันที่/เวลาอ่าน | Timestamp การอ่าน |
| เลขบัตรประชาชน | 13 หลัก |
| ชื่อ-นามสกุล (ไทย) | ชื่อเต็ม TH |
| ชื่อ-นามสกุล (อังกฤษ) | ชื่อเต็ม EN |
| วันเกิด | วัน/เดือน/ปี |
| ที่อยู่ | ที่อยู่เต็ม |
| ผู้ออกบัตร | สำนักงาน |
| รหัสผู้ออกบัตร | รหัส 13 หลัก |
| วันออกบัตร | วันที่ออก |
| วันหมดอายุ | วันหมดอายุ |
| ข้อมูลการ์ด | Card metadata |
| เครื่องอ่าน | ชื่อเครื่องอ่าน |
| หมายเหตุ | ช่องหมายเหตุ (แก้ไขได้) |

---

## 📂 ตำแหน่งไฟล์ Excel

### Default Location

```
C:\Users\[YourUsername]\Documents\ThaiIDCardReader\
└── ThaiIDCardHistory.xlsx
```

### เปลี่ยนตำแหน่ง (Custom Location)

```csharp
// ใน ExcelService.cs
public string GetDefaultExcelPath()
{
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var appFolder = Path.Combine(documentsPath, "ThaiIDCardReader");

    // เปลี่ยนได้ที่นี่
    return Path.Combine(appFolder, "ThaiIDCardHistory.xlsx");
}
```

---

## 🔄 Export และ Import

### Export Excel

**วิธีใช้:**
```
1. กดปุ่ม "Export Excel" หรือเมนู File → Export
2. เลือกตำแหน่งและชื่อไฟล์
3. กด Save
```

**รูปแบบไฟล์:**
- Extension: `.xlsx`
- Format: Excel 2007+
- ชื่อไฟล์อัตโนมัติ: `ThaiIDCard_Export_YYYYMMDD_HHMMSS.xlsx`

**ใช้งาน:**
```csharp
// Export ทั้งหมด
await ExportExcelAsync();

// Export ไปยัง path ที่กำหนด
_historyService.ExportToFile("C:\\path\\to\\file.xlsx");
```

### Import Excel

**วิธีใช้:**
```
1. กดปุ่ม "Import Excel" หรือเมนู File → Import
2. เลือกไฟล์ .xlsx ที่ต้องการ
3. กด Open
4. ข้อมูลจะถูกเพิ่มเข้าระบบ
```

**ข้อกำหนด:**
- ไฟล์ต้องเป็น `.xlsx`
- ต้องมี Sheet ชื่อ "Card History"
- Headers ต้องตรงกับรูปแบบ

**ใช้งาน:**
```csharp
await ImportExcelAsync();
```

---

## 📖 การเปิดไฟล์ Excel

### เปิดด้วย Microsoft Excel

**วิธีที่ 1: ใช้ปุ่มในโปรแกรม**
```
กดปุ่ม "เปิด Excel" → ไฟล์จะเปิดด้วย Microsoft Excel อัตโนมัติ
```

**วิธีที่ 2: เปิดจาก Folder**
```
Documents\ThaiIDCardReader\ThaiIDCardHistory.xlsx
Double-click เพื่อเปิด
```

**Code:**
```csharp
[RelayCommand]
private void OpenExcelFile()
{
    var excelPath = _excelService.GetDefaultExcelPath();
    System.Diagnostics.Process.Start(new ProcessStartInfo
    {
        FileName = excelPath,
        UseShellExecute = true
    });
}
```

---

## ✏️ การแก้ไขข้อมูลใน Excel

### ขั้นตอน

1. **เปิดไฟล์ Excel**
   ```
   - กดปุ่ม "เปิด Excel" ในโปรแกรม
   - หรือเปิดจาก Documents\ThaiIDCardReader\
   ```

2. **แก้ไขข้อมูล**
   ```
   - แก้ไขเซลล์ที่ต้องการ
   - เพิ่มหมายเหตุในคอลัมน์ "หมายเหตุ"
   - สามารถเพิ่ม/ลบแถวได้
   ```

3. **บันทึก**
   ```
   Ctrl+S หรือ File → Save
   ```

4. **โหลดข้อมูลใหม่ในโปรแกรม**
   ```
   - กดปุ่ม "Refresh" หรือ F5
   - หรือปิด-เปิดโปรแกรมใหม่
   ```

### ⚠️ ข้อควรระวัง

- ❌ **อย่าลบ Header Row** (แถวที่ 1)
- ❌ **อย่าเปลี่ยนชื่อ Sheet** (ต้องเป็น "Card History")
- ❌ **อย่าเปลี่ยนชื่อคอลัมน์**
- ✅ แก้ไขข้อมูลในแถวได้
- ✅ เพิ่มแถวใหม่ได้
- ✅ ลบแถวที่ไม่ต้องการได้

---

## 📊 สถิติและประวัติ

### ดูสถิติ

```csharp
// Get statistics
var (TotalRecords, UniqueCards, LastRead) = _historyService.GetStatistics();

// Display
TotalRecords      → จำนวนบันทึกทั้งหมด
UniqueCards       → จำนวนบัตรไม่ซ้ำ
LastRead          → วันที่อ่านล่าสุด
```

### ดูประวัติ

**ตามเลขบัตร:**
```csharp
var history = _historyService.GetHistoryByPersonalId("1234567890123");
```

**ตามช่วงวันที่:**
```csharp
var history = _historyService.GetHistoryByDateRange(
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now
);
```

**ทั้งหมด:**
```csharp
var allHistory = _historyService.GetAllHistory();
```

---

## ⚠️ การเขียนข้อมูลลงบัตร (Experimental)

### คำเตือน

```
❌ บัตรประชาชนไทยเป็น READ-ONLY ตามการออกแบบ
❌ บัตรส่วนใหญ่ไม่รองรับการเขียนข้อมูล
❌ ต้องมี PIN authority พิเศษ
❌ อุปกรณ์อ่านบัตรต้องรองรับคำสั่ง Write
```

### ตรวจสอบว่าบัตรรองรับไหม

```csharp
bool supportsWrite = _thaiIDCardService.SupportsWriteOperations();
// Returns: false (บัตรไทยไม่รองรับ)
```

### ลองเขียน (จะไม่สำเร็จ)

```csharp
var result = _thaiIDCardService.WriteNotesToCard("Test Note");

switch (result)
{
    case WriteResult.Success:
        // ไม่น่าจะเกิด
        break;
    case WriteResult.NotSupported:
        // บัตรไม่รองรับ (ส่วนใหญ่)
        break;
    case WriteResult.NoPermission:
        // ไม่มีสิทธิ์เขียน
        break;
    case WriteResult.WriteFailed:
        // เขียนไม่สำเร็จ
        break;
}
```

### ทำไมไม่สามารถเขียนได้?

1. **ความปลอดภัย**: บัตรประชาชนเป็นเอกสารราชการ ไม่ให้ผู้ใช้ทั่วไปเขียนได้
2. **โครงสร้างบัตร**: พื้นที่บนบัตรถูก Lock โดย Government Authority
3. **มาตรฐาน**: ตาม ISO-7816 บัตรสามารถกำหนดให้เป็น Read-Only ได้
4. **PIN Authority**: ต้องมี Admin PIN ไม่ใช่ PIN ผู้ใช้ทั่วไป

---

## 🎨 UI Components ใหม่

### ปุ่มฟังก์ชัน

```
📁 เปิด Excel        → OpenExcelCommand
💾 บันทึกด้วยตัวเอง   → SaveToHistoryCommand
📤 Export Excel      → ExportExcelCommand
📥 Import Excel      → ImportExcelCommand
📊 แสดงประวัติ       → ToggleHistoryPanelCommand
🔄 Refresh           → RefreshCommand
```

### DataGrid (ตาราง)

```xaml
<DataGrid ItemsSource="{Binding HistoryList}"
          SelectedItem="{Binding SelectedHistory}"
          AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="วันที่" Binding="{Binding ReadDateTime}"/>
        <DataGridTextColumn Header="เลขบัตร" Binding="{Binding PersonalID}"/>
        <DataGridTextColumn Header="ชื่อ" Binding="{Binding NameTH}"/>
        <!-- ... -->
    </DataGrid.Columns>
</DataGrid>
```

---

## 🔧 Customization

### เปลี่ยนที่อยู่ไฟล์ Excel

**แก้ไขใน:** `Services/ExcelService.cs`
```csharp
public string GetDefaultExcelPath()
{
    // เปลี่ยนเป็น path ที่ต้องการ
    return @"D:\MyData\IDCards.xlsx";
}
```

### ปิดการบันทึกอัตโนมัติ

**แก้ไขใน:** ViewModel
```csharp
AutoSaveToExcel = false;  // ปิด
```

หรือเพิ่ม Checkbox ใน UI:
```xaml
<CheckBox IsChecked="{Binding AutoSaveToExcel}"
          Content="บันทึกลง Excel อัตโนมัติ"/>
```

### เพิ่มคอลัมน์ใหม่

**1. อัปเดต Model:**
```csharp
// Models/CardHistory.cs
public string NewField { get; set; } = string.Empty;
```

**2. อัปเดต ExcelService:**
```csharp
// Services/ExcelService.cs - เพิ่มใน headers array
var headers = new[] { "ลำดับ", ..., "คอลัมน์ใหม่" };

// เพิ่มใน export
worksheet.Cells[row, 15].Value = history.NewField;
```

---

## 📈 Performance Tips

### สำหรับข้อมูลจำนวนมาก

```csharp
// 1. Load แบบ Lazy Loading
var recentHistory = _historyService
    .GetAllHistory()
    .Take(100)  // เอาแค่ 100 รายการล่าสุด
    .ToList();

// 2. ใช้ Pagination
var page1 = history.Skip(0).Take(50);
var page2 = history.Skip(50).Take(50);

// 3. Filter ก่อน Load
var filtered = _historyService
    .GetHistoryByDateRange(DateTime.Now.AddDays(-7), DateTime.Now);
```

### Excel File Size

```
จำนวนบันทึก → ขนาดไฟล์โดยประมาณ
100 records   → ~50 KB
1,000 records → ~200 KB
10,000 records → ~2 MB
```

---

## 🆘 Troubleshooting

### ❌ Error: "The process cannot access the file..."

**สาเหตุ:** ไฟล์ Excel เปิดอยู่ใน Microsoft Excel

**แก้ไข:**
```
1. ปิด Microsoft Excel
2. ลองใหม่
```

### ❌ Error: "Invalid header format"

**สาเหตุ:** ไฟล์ Excel ถูกแก้ไข headers

**แก้ไข:**
```
1. ลบไฟล์ Excel เดิม
2. อ่านบัตรใหม่ (ไฟล์จะถูกสร้างใหม่อัตโนมัติ)
```

### ❌ รูปภาพไม่ถูกบันทึก

**สาเหตุ:** ไม่ได้อ่านรูปก่อนบันทึก

**แก้ไข:**
```
1. กดปุ่ม "อ่านทั้งหมด" แทนการอ่านแยก
2. หรือกด "อ่านรูป" ก่อนกด "บันทึก"
```

---

## 📚 API Reference

### ExcelService

```csharp
public class ExcelService
{
    // Export to Excel
    void ExportToExcel(List<CardHistory> historyList, string filePath)

    // Append one record
    void AppendToExcel(CardHistory history, string? filePath = null)

    // Import from Excel
    List<CardHistory> ImportFromExcel(string filePath)

    // Get default file path
    string GetDefaultExcelPath()
}
```

### HistoryService

```csharp
public class HistoryService
{
    // Add new history
    void AddHistory(CardHistory history)

    // Get all
    List<CardHistory> GetAllHistory()

    // Get by Personal ID
    List<CardHistory> GetHistoryByPersonalId(string personalId)

    // Get by date range
    List<CardHistory> GetHistoryByDateRange(DateTime start, DateTime end)

    // Update
    void UpdateHistory(CardHistory history)

    // Delete
    void DeleteHistory(int id)

    // Clear all
    void ClearHistory()

    // Statistics
    (int Total, int Unique, DateTime? Last) GetStatistics()
}
```

---

## 🎓 Best Practices

### 1. Backup Excel ประจำ

```
สร้างการ Backup อัตโนมัติทุกสัปดาห์:
Documents\ThaiIDCardReader\Backups\
└── ThaiIDCardHistory_20250125.xlsx
```

### 2. ตรวจสอบข้อมูลก่อนบันทึก

```csharp
if (PersonalInformation != null &&
    !string.IsNullOrEmpty(PersonalInformation.PersonalID))
{
    SaveToHistory();
}
```

### 3. Handle Errors

```csharp
try
{
    _historyService.AddHistory(history);
}
catch (IOException ex)
{
    // ไฟล์ถูกเปิดอยู่
    MessageBox.Show("กรุณาปิด Excel ก่อน");
}
catch (Exception ex)
{
    // Error อื่นๆ
    Log.Error(ex);
}
```

---

**ฟีเจอร์พร้อมใช้งาน!** 🎉

ทุกครั้งที่อ่านบัตร ข้อมูลจะถูกบันทึกลง Excel อัตโนมัติ
เปิดไฟล์ได้ทันที ดู แก้ไข และวิเคราะห์ข้อมูลได้สะดวก
