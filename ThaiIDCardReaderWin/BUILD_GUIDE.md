# คู่มือการ Build และ Deploy
## Build & Deployment Guide

---

## 🛠️ การ Build โปรเจค

### Prerequisites

1. **Visual Studio 2022** (แนะนำ) หรือ **Visual Studio Code**
   - Workload: .NET Desktop Development
   - หรือใช้ .NET SDK 8.0+ ผ่าน command line

2. **Git** สำหรับ clone repository

---

## 📦 Build แบบต่างๆ

### 1. Debug Build (สำหรับพัฒนา)

```bash
cd ThaiIDCardReaderWin
dotnet restore
dotnet build --configuration Debug
```

ไฟล์จะอยู่ที่: `bin/Debug/net8.0-windows/`

### 2. Release Build (สำหรับใช้งานจริง)

```bash
dotnet build --configuration Release
```

ไฟล์จะอยู่ที่: `bin/Release/net8.0-windows/`

### 3. Publish สำหรับ Deployment

#### Framework-Dependent (ต้องติดตั้ง .NET Runtime)
```bash
dotnet publish --configuration Release \
  --output ./publish \
  --runtime win-x64 \
  --self-contained false
```

**ข้อดี:**
- ขนาดไฟล์เล็ก (~10 MB)
- อัปเดต .NET Runtime แยกได้

**ข้อเสีย:**
- ต้องติดตั้ง .NET 8.0 Runtime บนเครื่องผู้ใช้

#### Self-Contained (ไม่ต้องติดตั้ง Runtime)
```bash
dotnet publish --configuration Release \
  --output ./publish-standalone \
  --runtime win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:EnableCompressionInSingleFile=true
```

**ข้อดี:**
- ใช้งานได้ทันที ไม่ต้องติดตั้งอะไรเพิ่ม
- รวม .NET Runtime ไว้แล้ว

**ข้อเสีย:**
- ขนาดไฟล์ใหญ่ขึ้น (~50-70 MB)

---

## 📋 ไฟล์ที่จำเป็นสำหรับ Deploy

### Framework-Dependent Deployment
```
publish/
├── ThaiIDCardReader.exe          # Main executable
├── ThaiIDCardReader.dll
├── ThaiIDCardReader.deps.json
├── ThaiIDCardReader.runtimeconfig.json
├── ModernWpf.dll                 # UI library
├── PCSC.dll                      # Smart card library
├── PCSC.Iso7816.dll
└── CommunityToolkit.Mvvm.dll
```

### Self-Contained Deployment
```
publish-standalone/
└── ThaiIDCardReader.exe          # Single file executable
```

---

## 🎯 การสร้าง Installer

### วิธีที่ 1: WiX Toolset (แนะนำ)

1. **ติดตั้ง WiX Toolset**
   ```
   https://wixtoolset.org/
   ```

2. **สร้าง WiX Project**
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
     <Product Id="*"
              Name="Thai ID Card Reader"
              Version="1.0.0"
              Manufacturer="Your Company">
       <!-- WiX configuration -->
     </Product>
   </Wix>
   ```

3. **Build Installer**
   ```bash
   candle.exe Product.wxs
   light.exe Product.wixobj -o ThaiIDCardReaderSetup.msi
   ```

### วิธีที่ 2: NSIS (Nullsoft Scriptable Install System)

1. **ติดตั้ง NSIS**
   ```
   https://nsis.sourceforge.io/
   ```

2. **สร้าง NSIS Script**
   ```nsis
   Name "Thai ID Card Reader"
   OutFile "ThaiIDCardReaderSetup.exe"
   InstallDir "$PROGRAMFILES\ThaiIDCardReader"
   ```

3. **Build**
   ```bash
   makensis installer.nsi
   ```

### วิธีที่ 3: Inno Setup (ง่ายที่สุด)

1. **ติดตั้ง Inno Setup**
   ```
   https://jrsoftware.org/isinfo.php
   ```

2. **ใช้ Wizard สร้าง Script**
   - Application name: Thai ID Card Reader
   - Version: 1.0.0
   - Publisher: Your Company
   - Main executable: ThaiIDCardReader.exe

3. **Compile**
   - เปิด Inno Setup Compiler
   - โหลด .iss file
   - กด Compile

---

## 🔐 Code Signing (การเซ็นโค้ด)

### ทำไมต้อง Sign?
- Windows จะไม่เตือน "Unknown Publisher"
- ผู้ใช้ไว้ใจมากขึ้น
- ป้องกันการแก้ไขไฟล์

### วิธี Sign Code

1. **ซื้อ Code Signing Certificate**
   - Sectigo, DigiCert, GlobalSign

2. **Import Certificate**
   ```bash
   certutil -importPFX certificate.pfx
   ```

3. **Sign EXE File**
   ```bash
   signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com ThaiIDCardReader.exe
   ```

---

## 🚀 Deployment Strategies

### Strategy 1: Direct Download
1. Build โปรเจค
2. Upload `.exe` หรือ `.msi` ไป GitHub Releases
3. ผู้ใช้ download และติดตั้ง

**เหมาะสำหรับ:**
- โปรเจค Open Source
- จำนวนผู้ใช้ไม่มาก

### Strategy 2: Microsoft Store
1. Package เป็น MSIX
2. Submit ไป Microsoft Store
3. ผู้ใช้ติดตั้งผ่าน Store

**เหมาะสำหรับ:**
- โปรเจคที่ต้องการ auto-update
- ต้องการ wider distribution

### Strategy 3: ClickOnce
1. Publish ด้วย ClickOnce
2. Host บน Web Server
3. ผู้ใช้ติดตั้งผ่าน URL

**เหมาะสำหรับ:**
- Corporate deployment
- ต้องการ auto-update

---

## 📊 Build Configurations

### Debug Configuration
```xml
<PropertyGroup Condition="'$(Configuration)'=='Debug'">
  <Optimize>false</Optimize>
  <DefineConstants>DEBUG;TRACE</DefineConstants>
  <DebugType>full</DebugType>
</PropertyGroup>
```

**ใช้เมื่อ:**
- พัฒนาและ debug
- ต้องการ breakpoints
- ต้องการ detailed error messages

### Release Configuration
```xml
<PropertyGroup Condition="'$(Configuration)'=='Release'">
  <Optimize>true</Optimize>
  <DefineConstants>TRACE</DefineConstants>
  <DebugType>none</DebugType>
</PropertyGroup>
```

**ใช้เมื่อ:**
- Build สำหรับ production
- ต้องการ performance สูงสุด
- ขนาดไฟล์เล็กที่สุด

---

## 🧪 การทดสอบก่อน Release

### Checklist

- [ ] Build บน clean machine
- [ ] ทดสอบบน Windows 10 และ Windows 11
- [ ] ทดสอบกับเครื่องอ่านบัตรหลายรุ่น
- [ ] ทดสอบการติดตั้งและถอดติดตั้ง
- [ ] ทดสอบ offline (ไม่มี internet)
- [ ] Scan virus/malware
- [ ] ทดสอบ performance
- [ ] ตรวจสอบ memory leaks

---

## 📈 Version Management

### Semantic Versioning
```
MAJOR.MINOR.PATCH
1.0.0
```

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

### Update AssemblyInfo
```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <FileVersion>1.0.0.0</FileVersion>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
</PropertyGroup>
```

---

## 🔄 Continuous Integration (CI/CD)

### GitHub Actions Example

```yaml
name: Build and Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release

    - name: Publish
      run: dotnet publish --configuration Release --output ./publish

    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: ./publish/**
```

---

## 💡 Tips & Best Practices

### 1. เพิ่ม Application Icon
```xml
<PropertyGroup>
  <ApplicationIcon>Assets\app.ico</ApplicationIcon>
</PropertyGroup>
```

### 2. เพิ่ม Application Manifest
```xml
<ApplicationManifest>app.manifest</ApplicationManifest>
```

### 3. Optimize Publish Size
```bash
dotnet publish \
  --configuration Release \
  --self-contained true \
  -p:PublishTrimmed=true \
  -p:PublishReadyToRun=true \
  -p:TieredCompilation=false
```

### 4. Enable Assembly Compression
```xml
<PropertyGroup>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

---

## 🆘 Troubleshooting

### Error: PCSC.dll not found
**แก้ไข:** Copy PCSC.dll ไปใน output folder

### Error: ModernWpf styles not loading
**แก้ไข:** ตรวจสอบ App.xaml มี ResourceDictionary

### High Memory Usage
**แก้ไข:** เพิ่ม `PublishTrimmed` option

### Slow Startup
**แก้ไข:** ใช้ `PublishReadyToRun`

---

## 📚 Additional Resources

- [.NET Publishing Guide](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [WPF Deployment](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/deployment/)
- [Code Signing Guide](https://docs.microsoft.com/en-us/windows/win32/seccrypto/cryptography-tools)

---

**Happy Building! 🚀**
