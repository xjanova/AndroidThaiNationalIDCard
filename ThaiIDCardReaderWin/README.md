# โปรแกรมอ่านบัตรประชาชนไทย สำหรับ Windows 11
## Thai National ID Card Reader for Windows 11

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=for-the-badge&logo=windows)
![C#](https://img.shields.io/badge/C%23-Latest-239120?style=for-the-badge&logo=c-sharp)

โปรแกรมอ่านบัตรประชาชนไทยที่ทันสมัย สวยงาม และใช้งานง่าย สำหรับ Windows 11

</div>

---

## ✨ คุณสมบัติเด่น

### 🎨 **Modern & Beautiful UI**
- ออกแบบตาม **Windows 11 Fluent Design**
- **Acrylic effects** และ **smooth animations**
- รองรับ **Dark/Light Mode** อัตโนมัติ
- UI ภาษาไทยที่เข้าใจง่าย

### 📖 **ฟีเจอร์การอ่านบัตร**
- ✅ อ่านข้อมูลส่วนบุคคล (เลขบัตร, ชื่อ-นามสกุล, ที่อยู่, วันเกิด)
- ✅ อ่านรูปถ่ายจากบัตร
- ✅ ยืนยัน PIN Code ด้วยการเข้ารหัส 3DES
- ✅ แสดงข้อมูลผู้ออกบัตรและวันหมดอายุ
- ✅ ตรวจจับการใส่/ถอดบัตรอัตโนมัติ

### 🚀 **เทคโนโลยีทันสมัย**
- **MVVM Pattern** - โครงสร้างโค้ดที่ชัดเจน
- **Async/Await** - ไม่มีการค้าง UI
- **PC/SC Standard** - รองรับเครื่องอ่านบัตรทุกรุ่น
- **TIS-620 Encoding** - รองรับภาษาไทยเต็มรูปแบบ

---

## 📋 ความต้องการของระบบ

### Windows
- **OS**: Windows 11 (แนะนำ) หรือ Windows 10 version 1809+
- **RAM**: 4 GB ขึ้นไป
- **.NET Runtime**: .NET 8.0 Runtime (Desktop)

### ฮาร์ดแวร์
- **Smart Card Reader** ที่รองรับ PC/SC
  - เครื่องอ่านบัตรประเภท USB
  - รองรับ ISO-7816 standard
  - ตัวอย่างเครื่องที่รองรับ:
    - ACS ACR38U
    - Gemalto PC Twin Reader
    - Precise Biometrics Precise 200/250 MC
    - อื่นๆ ที่รองรับ PC/SC

---

## 🛠️ การติดตั้ง

### วิธีที่ 1: Build จาก Source Code

#### 1. ติดตั้ง .NET SDK
ดาวน์โหลดและติดตั้ง .NET 8.0 SDK จาก:
```
https://dotnet.microsoft.com/download/dotnet/8.0
```

#### 2. Clone Repository
```bash
git clone https://github.com/yourusername/ThaiIDCardReader.git
cd ThaiIDCardReader/ThaiIDCardReaderWin
```

#### 3. Restore Packages
```bash
dotnet restore
```

#### 4. Build โปรเจค
```bash
dotnet build --configuration Release
```

#### 5. Run โปรแกรม
```bash
dotnet run
```

### วิธีที่ 2: ใช้ไฟล์ที่ Build แล้ว

1. ดาวน์โหลด .NET 8.0 Desktop Runtime:
   ```
   https://dotnet.microsoft.com/download/dotnet/8.0/runtime
   ```

2. ดาวน์โหลดไฟล์ .exe จาก Releases

3. Double-click `ThaiIDCardReader.exe` เพื่อเริ่มใช้งาน

---

## 📖 วิธีใช้งาน

### ขั้นตอนการอ่านบัตร

1. **เชื่อมต่อเครื่องอ่านบัตร**
   - เสียบเครื่องอ่านบัตร USB เข้ากับคอมพิวเตอร์
   - รอให้ Windows ติดตั้ง driver อัตโนมัติ
   - โปรแกรมจะตรวจจับเครื่องอ่านอัตโนมัติ

2. **ใส่บัตรประชาชน**
   - ใส่บัตรประชาชนชิปลงในเครื่องอ่าน
   - โปรแกรมจะแสดงสถานะ "ตรวจพบบัตร"
   - ไฟสถานะจะเปลี่ยนเป็นสีเขียว

3. **อ่านข้อมูล**
   - กดปุ่ม **"📋 อ่านทั้งหมด"** เพื่ออ่านข้อมูลและรูปภาพ
   - หรือเลือกอ่านแยก:
     - **"📖 อ่านข้อมูล"** - อ่านเฉพาะข้อมูลส่วนบุคคล
     - **"🖼️ อ่านรูป"** - อ่านเฉพาะรูปถ่าย

4. **ยืนยัน PIN (ถ้าต้องการ)**
   - กดปุ่ม **"🔑 ยืนยัน PIN"**
   - ใส่รหัส PIN 4 หลัก
   - กด "ยืนยัน"

5. **ล้างข้อมูล**
   - กดปุ่ม **"🗑️ ล้างข้อมูล"** เพื่อล้างข้อมูลที่แสดง
   - ถอดบัตรออก - ข้อมูลจะถูกล้างอัตโนมัติ

---

## 🏗️ สถาปัตยกรรม

### โครงสร้างโปรเจค

```
ThaiIDCardReaderWin/
├── Models/                    # Data models
│   ├── PersonalInformation.cs
│   ├── ChipCardADM.cs
│   └── CardReaderStatus.cs
├── Services/                  # Business logic
│   ├── SmartCardService.cs    # PC/SC communication
│   └── ThaiIDCardService.cs   # Thai ID card reading
├── ViewModels/                # MVVM ViewModels
│   └── MainViewModel.cs
├── Helpers/                   # Utility classes
│   └── ByteHelper.cs
├── Assets/                    # Images & resources
├── App.xaml                   # Application entry
├── MainWindow.xaml            # Main UI
└── ThaiIDCardReader.csproj    # Project file
```

### Technology Stack

| Component | Technology |
|-----------|-----------|
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Design System** | ModernWpf (Fluent Design) |
| **Smart Card** | PCSC-sharp (PC/SC wrapper) |
| **MVVM Toolkit** | CommunityToolkit.Mvvm |
| **Runtime** | .NET 8.0 |
| **Language** | C# 12 |
| **Thai Encoding** | TIS-620 |

### NuGet Packages

```xml
<PackageReference Include="ModernWpfUI" Version="0.9.6" />
<PackageReference Include="PCSC" Version="6.2.0" />
<PackageReference Include="PCSC.Iso7816" Version="6.2.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="System.Text.Encoding.CodePages" Version="8.0.0" />
```

---

## 🔒 ความปลอดภัย

### การเข้ารหัส PIN
- ใช้ **3DES (Triple DES)** encryption
- **SHA-1** hashing สำหรับ PIN
- **Challenge-Response** mechanism
- ไม่มีการเก็บ PIN ในระบบ

### การสื่อสาร
- ใช้ **PC/SC standard protocol**
- **ISO-7816** compliant APDU commands
- ไม่มีการส่งข้อมูลออกนอกเครื่อง
- ทำงาน **offline** ทั้งหมด

---

## 🐛 การแก้ปัญหา

### ไม่พบเครื่องอ่านบัตร
1. ตรวจสอบว่าเสียบ USB เข้ากับคอมพิวเตอร์แล้ว
2. ติดตั้ง driver ของเครื่องอ่านบัตร
3. ลอง unplug และ plug ใหม่
4. ตรวจสอบ Device Manager ว่ามี "Smart Card Readers"

### อ่านข้อมูลไม่ได้
1. ตรวจสอบว่าใส่บัตรถูกทิศทาง (ชิปหันขึ้น)
2. ทำความสะอาดชิปบัตร
3. ลองถอดบัตรแล้วใส่ใหม่
4. Reset เครื่องอ่านบัตร (ถอด USB แล้วเสียบใหม่)

### PIN ไม่ถูกต้อง
- ตรวจสอบว่าใส่ PIN ถูกต้อง 4 หลัก
- มีจำนวนครั้งจำกัดในการลอง
- หากลองผิด 3 ครั้ง บัตรจะถูกล็อค
- ติดต่อกรมการปกครองเพื่อปลดล็อค

### ภาษาไทยแสดงผิด
- โปรแกรมรองรับ TIS-620 อัตโนมัติ
- ตรวจสอบว่า Windows รองรับภาษาไทย
- ติดตั้ง Thai Language Pack ใน Windows

---

## 📊 ข้อมูลที่อ่านได้

| ข้อมูล | คำอธิบาย |
|--------|----------|
| **เลขบัตรประชาชน** | เลข 13 หลัก |
| **ชื่อ-นามสกุล (ไทย)** | ชื่อภาษาไทย |
| **ชื่อ-นามสกุล (อังกฤษ)** | ชื่อภาษาอังกฤษ |
| **วันเกิด** | วัน/เดือน/ปี (พ.ศ.) |
| **ที่อยู่** | ที่อยู่ตามทะเบียนบ้าน |
| **วันออกบัตร** | วันที่ออกบัตร |
| **วันหมดอายุ** | วันที่บัตรหมดอายุ |
| **ผู้ออกบัตร** | สำนักงานที่ออกบัตร |
| **รูปถ่าย** | รูปถ่ายจากบัตร (JPEG) |

---

## 🎯 Roadmap

### Version 1.1 (Coming Soon)
- [ ] Export ข้อมูลเป็น PDF
- [ ] Export ข้อมูลเป็น JSON/XML
- [ ] บันทึกรูปภาพแยกไฟล์
- [ ] Multi-language support (English)

### Version 1.2 (Planned)
- [ ] Batch reading (อ่านหลายบัตร)
- [ ] Database integration
- [ ] QR Code generation
- [ ] Print support

### Version 2.0 (Future)
- [ ] Fingerprint verification
- [ ] Cloud backup (optional)
- [ ] REST API service
- [ ] Mobile app companion

---

## 🤝 การมีส่วนร่วม

เรายินดีรับ contributions! หากต้องการมีส่วนร่วม:

1. Fork repository
2. สร้าง feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. เปิด Pull Request

---

## 📄 License

โปรเจคนี้เป็น Open Source ภายใต้ MIT License

---

## 👨‍💻 Credits

### Original Android Library
- **AndroidThaiNationalIDCard** by [Advanced Logic](https://github.com/Advanced-Logic)
- License: MIT

### Windows C# Port
- Ported and modernized for Windows 11
- Enhanced with Fluent Design UI
- Improved architecture with MVVM pattern

### Libraries Used
- [ModernWpf](https://github.com/Kinnara/ModernWpf) - Fluent Design for WPF
- [PCSC-sharp](https://github.com/danm-de/pcsc-sharp) - PC/SC wrapper
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM helpers

---

## 📞 ติดต่อ & สนับสนุน

- **Issues**: [GitHub Issues](https://github.com/yourusername/ThaiIDCardReader/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/ThaiIDCardReader/discussions)
- **Email**: support@example.com

---

## ⚠️ ข้อจำกัดความรับผิดชอบ

โปรแกรมนี้พัฒนาขึ้นเพื่อวัตถุประสงค์ในการศึกษาและใช้งานทั่วไป
ผู้พัฒนาไม่รับผิดชอบต่อความเสียหายใดๆ ที่เกิดจากการใช้งาน
กรุณาใช้งานอย่างรับผิดชอบและเคารพความเป็นส่วนตัว

---

<div align="center">

**Made with ❤️ for Thailand**

⭐ ถ้าชอบโปรเจคนี้ กด Star ให้ด้วยนะครับ!

</div>
