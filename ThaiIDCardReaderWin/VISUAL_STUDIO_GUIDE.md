# คู่มือการใช้งานกับ Visual Studio
## Visual Studio Development Guide

---

## 🎯 การเริ่มต้นใช้งาน

### ความต้องการ

**Visual Studio 2022** (แนะนำ Community Edition ฟรี)
- Download: https://visualstudio.microsoft.com/

**Workloads ที่ต้องติดตั้ง:**
- ✅ .NET Desktop Development
- ✅ Windows Presentation Foundation (WPF)

**Optional Workloads (แนะนำ):**
- Universal Windows Platform development
- .NET Multi-platform App UI development

---

## 📂 เปิดโปรเจคใน Visual Studio

### วิธีที่ 1: เปิดจาก Solution File (แนะนำ)

1. Double-click `ThaiIDCardReader.sln`
2. Visual Studio จะเปิดอัตโนมัติ
3. รอให้ restore NuGet packages เสร็จ

### วิธีที่ 2: เปิดจาก Visual Studio

1. เปิด Visual Studio 2022
2. File → Open → Project/Solution
3. เลือกไฟล์ `ThaiIDCardReader.sln`
4. คลิก Open

### วิธีที่ 3: เปิดจาก Folder

1. File → Open → Folder
2. เลือกโฟลเดอร์ `ThaiIDCardReaderWin`
3. Visual Studio จะตรวจจับ .csproj อัตโนมัติ

---

## ▶️ การ Run และ Debug

### Run แบบ Debug (F5)

```
1. กด F5 หรือ Debug → Start Debugging
2. โปรแกรมจะ compile และเปิดขึ้นมา
3. สามารถใช้ breakpoints ได้
4. ดู debug output ใน Output window
```

**Breakpoints:**
- คลิกซ้ายของหมายเลขบรรทัด เพื่อวาง breakpoint
- F9 - Toggle breakpoint
- F10 - Step Over
- F11 - Step Into
- Shift+F11 - Step Out

### Run แบบ Release (Ctrl+F5)

```
1. กด Ctrl+F5 หรือ Debug → Start Without Debugging
2. รันเร็วกว่า (มี optimizations)
3. ไม่สามารถ debug ได้
```

---

## 🔧 Configuration และ Platform

### เปลี่ยน Configuration

**Debug vs Release:**
```
Debug Configuration:
✅ เหมาะสำหรับพัฒนา
✅ มี debugging symbols
✅ ไม่มี optimizations
❌ ทำงานช้ากว่า
❌ ไฟล์ใหญ่กว่า

Release Configuration:
✅ เหมาะสำหรับ production
✅ มี optimizations
✅ ทำงานเร็ว
❌ Debug ยากขึ้น
```

**วิธีเปลี่ยน:**
1. ดูที่ toolbar ด้านบน
2. คลิก dropdown "Debug" หรือ "Release"
3. เลือก configuration ที่ต้องการ

### Platform Target

- **Any CPU**: รองรับทั้ง 32-bit และ 64-bit
- **x64**: เฉพาะ 64-bit (แนะนำสำหรับ Windows 11)
- **x86**: เฉพาะ 32-bit

---

## 📦 NuGet Package Management

### ดู Packages ที่ติดตั้งแล้ว

```
1. Solution Explorer → คลิกขวาที่ Project
2. Manage NuGet Packages...
3. ไปที่แท็บ "Installed"
```

### อัปเดต Packages

```
1. Manage NuGet Packages...
2. แท็บ "Updates"
3. เลือก packages ที่ต้องการอัปเดต
4. คลิก "Update"
```

### ติดตั้ง Package ใหม่

```
1. Manage NuGet Packages...
2. แท็บ "Browse"
3. ค้นหาชื่อ package
4. คลิก "Install"
```

**Packages ที่ใช้:**
```
ModernWpfUI           - Fluent Design UI
PCSC                  - Smart Card Reader
PCSC.Iso7816          - ISO-7816 Support
CommunityToolkit.Mvvm - MVVM Helpers
System.Text.Encoding.CodePages - TIS-620 Support
```

---

## 🎨 XAML Designer

### เปิด Designer

```
1. Double-click ไฟล์ .xaml (เช่น MainWindow.xaml)
2. จะเห็น 2 panels:
   - บน: Visual Designer
   - ล่าง: XAML Code
3. สามารถ drag-drop controls ได้
```

### Split View Modes

**แนวนอน (แนะนำ):**
```
View → Designer → Split View Horizontally
```

**แนวตั้ง:**
```
View → Designer → Split View Vertically
```

**XAML อย่างเดียว:**
```
View → Designer → XAML Only
```

### IntelliSense สำหรับ XAML

- พิมพ์ `<` เพื่อดู available elements
- `Ctrl+Space` เพื่อดู properties
- `Ctrl+J` เพื่อดู snippets

---

## 🔍 Navigation และ Search

### Quick Navigation

```
Ctrl+,           - Go to All (search anything)
Ctrl+T           - Go to Type
Ctrl+F           - Find in current file
Ctrl+Shift+F     - Find in entire solution
F12              - Go to Definition
Alt+F12          - Peek Definition
Ctrl+-           - Navigate backward
Ctrl+Shift+-     - Navigate forward
```

### Solution Explorer

```
Ctrl+Alt+L       - เปิด Solution Explorer
Ctrl+Shift+E     - Focus Solution Explorer
```

### Search Shortcuts

```
Ctrl+F3          - Find Next (current word)
F3               - Find Next
Shift+F3         - Find Previous
Ctrl+H           - Find and Replace
```

---

## 🛠️ Build และ Clean

### Build Commands

```
Ctrl+Shift+B     - Build Solution
F6               - Build Project
Ctrl+Break       - Cancel Build
```

### Build Menu Options

```
Build → Build Solution          - Build ทั้งหมด
Build → Rebuild Solution        - Clean + Build
Build → Clean Solution          - ลบ build output
Build → Build [ProjectName]     - Build เฉพาะโปรเจค
```

### View Build Output

```
View → Output (Ctrl+Alt+O)
```

---

## 🎭 Live Visual Tree (Runtime Debugging)

### เปิด Live Visual Tree

```
Debug → Windows → Live Visual Tree
```

**ฟีเจอร์:**
- ดู UI hierarchy แบบ real-time
- Inspect properties ของ UI elements
- ใช้ได้ตอน debug เท่านั้น (F5)

### Live Property Explorer

```
Debug → Windows → Live Property Explorer
```

- ดู/แก้ไข properties แบบ real-time
- Test UI changes โดยไม่ต้อง rebuild

---

## 🐛 Debugging Tools

### Debug Windows

```
Debug → Windows → Locals         - ตัวแปรใน scope
Debug → Windows → Autos          - ตัวแปรที่เกี่ยวข้อง
Debug → Windows → Watch 1        - Watch expressions
Debug → Windows → Call Stack     - Function call stack
Debug → Windows → Breakpoints    - จัดการ breakpoints
Debug → Windows → Output         - Debug output
```

### Immediate Window

```
Ctrl+Alt+I      - เปิด Immediate Window
```

**ใช้ทำอะไรได้:**
```csharp
// ประเมินค่า expression
? myVariable

// เรียก method
myObject.DoSomething()

// แก้ไขค่าตัวแปร
myVariable = "new value"
```

---

## 📊 Performance Profiler

### เปิด Profiler

```
Alt+F2          - Performance Profiler
Debug → Performance Profiler
```

**Profiling Options:**
- CPU Usage
- Memory Usage
- GPU Usage
- Application Timeline

### วิธีใช้

```
1. เลือก profiling type
2. คลิก "Start"
3. ใช้งานโปรแกรม
4. คลิก "Stop Collection"
5. วิเคราะห์ผลลัพธ์
```

---

## 🎨 Customization

### Themes

```
Tools → Options → Environment → General → Color theme
```

**Themes:**
- Light
- Dark (แนะนำ)
- Blue
- Additional themes (ติดตั้งเพิ่มได้)

### Fonts

```
Tools → Options → Environment → Fonts and Colors
```

**แนะนำ:**
- Font: Cascadia Code, Fira Code, JetBrains Mono
- Size: 10-12 pt

### Keyboard Shortcuts

```
Tools → Options → Environment → Keyboard
```

หรือดาวน์โหลด cheat sheet:
```
Help → Keyboard Shortcut Reference
```

---

## 🔌 Extensions (แนะนำ)

### ติดตั้ง Extensions

```
Extensions → Manage Extensions
```

**Extensions ที่แนะนำ:**

1. **Productivity Power Tools**
   - เพิ่ม productivity features

2. **XAML Styler**
   - Format XAML code อัตโนมัติ

3. **ReSharper** (ไม่ฟรี แต่คุ้มค่า)
   - Code analysis และ refactoring

4. **GitHub Copilot** (AI coding assistant)
   - AI ช่วยเขียนโค้ด

5. **CodeMaid**
   - Clean และ organize code

---

## 🧪 Testing

### Unit Test Project

**สร้าง Test Project:**
```
File → Add → New Project → MSTest Test Project
```

**เขียน Test:**
```csharp
[TestMethod]
public void TestPersonalInfoReading()
{
    var service = new ThaiIDCardService(_mockSmartCardService);
    var result = service.ReadPersonalInformation();
    Assert.IsNotNull(result);
}
```

**Run Tests:**
```
Test → Run All Tests (Ctrl+R, A)
View → Test Explorer
```

---

## 📁 Project Structure ใน Solution Explorer

```
ThaiIDCardReader (Solution)
└── ThaiIDCardReader (Project)
    ├── Dependencies
    │   └── Packages (NuGet packages)
    ├── Models/
    ├── Services/
    ├── ViewModels/
    ├── Views/
    ├── Helpers/
    ├── Assets/
    ├── App.xaml
    ├── MainWindow.xaml
    └── Properties/
```

---

## 🔧 Useful Tools

### Package Manager Console

```
View → Other Windows → Package Manager Console
```

**คำสั่งที่ใช้บ่อย:**
```powershell
# Restore packages
dotnet restore

# Update package
Update-Package PackageName

# Install package
Install-Package PackageName

# List packages
Get-Package
```

### Task List

```
View → Task List
```

**ใช้ TODO comments:**
```csharp
// TODO: Implement fingerprint verification
// HACK: Temporary workaround
// NOTE: Important information
```

---

## 🚀 Publish จาก Visual Studio

### Publish Wizard

```
1. Solution Explorer → คลิกขวาที่ Project
2. Publish...
3. เลือก target:
   - Folder
   - Azure
   - Microsoft Store
   - ClickOnce
4. Configure settings
5. คลิก "Publish"
```

### Publish Profiles

**Framework-Dependent:**
```
Target: Folder
Configuration: Release | Any CPU
Target Framework: net8.0-windows
Deployment Mode: Framework-dependent
```

**Self-Contained:**
```
Target: Folder
Configuration: Release | x64
Target Framework: net8.0-windows
Deployment Mode: Self-contained
Produce single file: ✓
Enable ReadyToRun compilation: ✓
Trim unused code: ✓
```

---

## ⚡ Performance Tips

### 1. Disable IntelliSense Auto-popup (ถ้ารู้สึกช้า)
```
Tools → Options → Text Editor → C# → IntelliSense
ยกเลิก "Show completion list after a character is typed"
```

### 2. Disable CodeLens
```
Tools → Options → Text Editor → All Languages → CodeLens
ยกเลิก "Enable CodeLens"
```

### 3. Reduce XAML Designer Auto-reload
```
Tools → Options → XAML Designer
เลือก "Disable XAML Designer"
```

### 4. Use Lightweight Solution Load
```
Tools → Options → Projects and Solutions
เลือก "Lightweight solution load for all solutions"
```

---

## 🆘 Troubleshooting

### Error: NuGet packages ไม่ restore

**แก้ไข:**
```
1. Tools → NuGet Package Manager → Package Manager Settings
2. Clear All NuGet Cache(s)
3. Rebuild Solution
```

### Error: XAML Designer ไม่แสดง

**แก้ไข:**
```
1. Tools → Options → XAML Designer
2. ลอง restart Visual Studio
3. ถ้ายังไม่ได้ใช้ "XAML Only" view
```

### Error: Build ไม่ผ่าน

**แก้ไข:**
```
1. Build → Clean Solution
2. ลบโฟลเดอร์ bin/ และ obj/
3. Build → Rebuild Solution
```

### Visual Studio ช้า

**แก้ไข:**
```
1. ปิด extensions ที่ไม่ใช้
2. ใช้ Lightweight Solution Load
3. เพิ่ม RAM
4. ติดตั้ง Visual Studio บน SSD
```

---

## 📚 Learning Resources

### Official Documentation
```
https://docs.microsoft.com/visualstudio/
https://docs.microsoft.com/dotnet/
https://docs.microsoft.com/windows/apps/desktop/modernize/
```

### Keyboard Shortcuts Cheat Sheet
```
Help → Keyboard Shortcut Reference
หรือกด Ctrl+K, Ctrl+R
```

### Samples และ Templates
```
File → New → Project → Browse Templates Online
```

---

## 💡 Pro Tips

1. **Use Snippets**
   ```
   พิมพ์ "prop" แล้วกด Tab 2 ครั้ง
   → สร้าง property อัตโนมัติ
   ```

2. **Multi-cursor Editing**
   ```
   Ctrl+Alt+Click - เพิ่ม cursor
   Alt+Shift+. - เลือก occurrence ถัดไป
   ```

3. **Quick Actions**
   ```
   Ctrl+. - เปิด Quick Actions menu
   (ใช้ refactor, generate code, etc.)
   ```

4. **Format Document**
   ```
   Ctrl+K, Ctrl+D - Format ทั้ง document
   Ctrl+K, Ctrl+F - Format selected code
   ```

5. **Comment/Uncomment**
   ```
   Ctrl+K, Ctrl+C - Comment
   Ctrl+K, Ctrl+U - Uncomment
   ```

---

**Happy Coding with Visual Studio! 🎉**
