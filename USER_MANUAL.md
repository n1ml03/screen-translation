# Screen Translation Tool - User Manual

## Giới thiệu (Introduction)

**Screen Translation Tool** là một ứng dụng WPF được viết bằng C# với các thành phần Python hỗ trợ, được thiết kế để dịch văn bản trên màn hình theo thời gian thực. Ứng dụng sử dụng công nghệ OCR (Optical Character Recognition) để nhận dạng văn bản từ màn hình và dịch sang ngôn ngữ mong muốn thông qua API ChatGPT.

## Tính năng chính (Main Features)

### 1. Chụp màn hình và nhận dạng văn bản (Screen Capture & OCR)
- **Windows OCR**: Sử dụng công cụ OCR tích hợp của Windows (hỗ trợ nhiều ngôn ngữ)
- **OneOCR**: Công cụ OCR nhanh với hiệu suất cao
- **PaddleOCR**: Công cụ OCR dựa trên PaddlePaddle với hỗ trợ đa ngôn ngữ

### 2. Dịch thuật thời gian thực (Real-time Translation)
- Tích hợp API ChatGPT để dịch văn bản
- Hỗ trợ nhiều cặp ngôn ngữ
- Dịch tự động hoặc theo yêu cầu

### 3. Chọn vùng dịch (Area Selection)
- Chọn vùng màn hình cụ thể để dịch
- Lưu nhiều vùng dịch khác nhau (tối đa 5 vùng)
- Chuyển đổi nhanh giữa các vùng đã lưu

### 4. Hiển thị lớp phủ (Overlay Display)
- Hiển thị văn bản dịch chồng lên màn hình
- Tùy chỉnh màu sắc, kích thước font
- Tự động điều chỉnh kích thước văn bản

### 5. ChatBox (Communication Interface)
- Giao diện chat để giao tiếp với AI
- Lưu lịch sử cuộc trò chuyện
- Tùy chỉnh giao diện chat

### 6. Text-to-Speech (TTS)
- Đọc văn bản dịch bằng giọng nói
- Hỗ trợ nhiều giọng đọc Windows
- Tích hợp dịch vụ TTS tùy chỉnh

### 7. Hệ thống phím tắt (Hotkey System)
- Phím tắt toàn cục để điều khiển ứng dụng
- Tùy chỉnh phím tắt cho từng chức năng
- Hỗ trợ phím kết hợp (Ctrl, Alt, Shift)

## Cài đặt và thiết lập (Installation & Setup)

### Yêu cầu hệ thống (System Requirements)
- **Hệ điều hành**: Windows 10/11 (64-bit)
- **Framework**: .NET 9.0
- **RAM**: Tối thiểu 4GB
- **Python**: 3.8+ (cho thành phần OCR)
- **API Key**: ChatGPT API key

### Quy trình cài đặt (Installation Steps)

1. **Tải và cài đặt .NET 9.0**
   ```
   # Download from Microsoft website
   https://dotnet.microsoft.com/download/dotnet/9.0
   ```

2. **Cài đặt Python và dependencies**
   ```bash
   pip install -r requirements.txt
   pip install paddlepaddle paddleocr
   ```

3. **Chạy ứng dụng**
   ```bash
   # Build and run
   dotnet build
   dotnet run --project ST.csproj
   ```

### Thiết lập ban đầu (Initial Setup)

Khi chạy ứng dụng lần đầu, cửa sổ **Quick Start** sẽ xuất hiện để hướng dẫn thiết lập:

1. **Bước 1**: Chào mừng và giới thiệu
2. **Bước 2**: Thiết lập ngôn ngữ nguồn và đích
3. **Bước 3**: Cấu hình OCR
4. **Bước 4**: Thiết lập dịch thuật
5. **Bước 5**: Hoàn thành thiết lập

## Hướng dẫn sử dụng (Usage Guide)

### Giao diện chính (Main Interface)

#### Thanh điều khiển chính (Main Control Bar)
- **Start/Stop**: Bắt đầu/dừng quá trình dịch
- **Overlay**: Bật/tắt hiển thị lớp phủ
- **SelectArea**: Chọn vùng màn hình để dịch
- **ShowArea**: Hiển thị vùng đã chọn
- **ClearArea**: Xóa vùng đã chọn
- **Select Window**: Chọn cửa sổ cụ thể để dịch
- **SetupOCR**: Thiết lập máy chủ OCR
- **StartOCR/StopOCR**: Khởi động/dừng máy chủ OCR
- **ChatBox**: Mở cửa sổ chat
- **Log**: Xem nhật ký ứng dụng
- **Settings**: Mở cửa sổ thiết lập

### Thiết lập ngôn ngữ (Language Settings)

1. Mở cửa sổ **Settings** > tab **Language**
2. Chọn **Source Language** (ngôn ngữ nguồn)
3. Chọn **Target Language** (ngôn ngữ đích)
4. Nhấn nút ⇄ để đổi chỗ hai ngôn ngữ

**Các ngôn ngữ hỗ trợ:**
- Tiếng Anh (en)
- Tiếng Nhật (ja)
- Tiếng Trung giản thể (ch_sim)
- Tiếng Trung phồn thể (ch_tra)
- Tiếng Hàn (ko)
- Tiếng Việt (vi)
- Tiếng Pháp (fr), Đức (de), Nga (ru), Tây Ban Nha (es), Ý (it)
- Và nhiều ngôn ngữ khác...

### Thiết lập OCR (OCR Configuration)

#### 1. Chọn phương thức OCR
- **Windows OCR**: Sử dụng OCR tích hợp Windows (khuyên dùng)
- **OneOCR**: Công cụ OCR nhanh
- **PaddleOCR**: OCR với độ chính xác cao

#### 2. Thiết lập máy chủ OCR
1. Nhấn nút **SetupOCR**
2. Cấu hình các thông số:
   - **Port**: Cổng kết nối (mặc định: 9998)
   - **Language**: Ngôn ngữ OCR
   - **Confidence thresholds**: Ngưỡng độ tin cậy

3. Nhấn **StartOCR** để khởi động máy chủ

#### 3. Tùy chỉnh OCR nâng cao
- **Min text fragment size**: Kích thước tối thiểu của đoạn văn bản
- **Block detection scale**: Tỷ lệ phát hiện khối
- **Min letter/line confidence**: Độ tin cậy tối thiểu

### Thiết lập dịch thuật (Translation Settings)

#### 1. Cấu hình ChatGPT
1. Mở tab **Translation** trong Settings
2. Nhập **ChatGPT Endpoint**
3. Nhập **Username** và **Password**
4. Kiểm tra kết nối

#### 2. Tùy chỉnh dịch thuật
- **Max context pieces**: Số lượng ngữ cảnh tối đa
- **Min context size**: Kích thước ngữ cảnh tối thiểu
- **Game info**: Thông tin game (cho ngữ cảnh)

### Chọn vùng dịch (Area Selection)

#### Chọn vùng thủ công
1. Nhấn nút **SelectArea**
2. Di chuột để chọn vùng trên màn hình
3. Nhấn Enter để xác nhận hoặc ESC để hủy

#### Chọn cửa sổ
1. Nhấn nút **Select Window**
2. Chọn cửa sổ từ danh sách hoặc click vào cửa sổ mong muốn
3. Ứng dụng sẽ tự động chọn toàn bộ vùng cửa sổ

#### Quản lý nhiều vùng
- Lưu tối đa 5 vùng dịch
- Sử dụng phím tắt để chuyển đổi nhanh giữa các vùng
- Xóa vùng không cần thiết bằng **ClearArea**

### Hệ thống phím tắt (Hotkey System)

#### Phím tắt mặc định
- **Ctrl+Shift+S**: Start/Stop dịch
- **Ctrl+Shift+O**: Bật/tắt Overlay
- **Ctrl+Shift+A**: Select Area
- **Ctrl+Shift+C**: Clear Areas
- **Ctrl+Shift+W**: Show Area
- **Ctrl+Shift+B**: Mở ChatBox
- **Ctrl+Shift+L**: Mở Log
- **Ctrl+Shift+T**: Mở Settings

#### Tùy chỉnh phím tắt
1. Mở **Settings** > tab **Hotkeys**
2. Nhấn vào ô phím tắt muốn thay đổi
3. Nhấn tổ hợp phím mới
4. Nhấn **Save** để lưu

### ChatBox (Communication Interface)

#### Sử dụng ChatBox
1. Nhấn nút **ChatBox** hoặc phím tắt
2. Nhập câu hỏi hoặc yêu cầu dịch
3. AI sẽ trả lời và có thể dịch văn bản

#### Tùy chỉnh giao diện
- **Font Family**: Loại font chữ
- **Font Size**: Kích thước chữ
- **Colors**: Màu sắc nền, chữ, viền
- **Opacity**: Độ trong suốt
- **Lines of History**: Số dòng lịch sử lưu

### Text-to-Speech (TTS)

#### Thiết lập TTS
1. Mở **Settings** > tab **TTS**
2. Bật **TTS Enabled**
3. Chọn **TTS Service**:
   - Windows TTS (mặc định)
   - Custom service

4. Chọn **Voice**: Giọng đọc mong muốn

#### Tùy chỉnh TTS
- **Auto-translate audio**: Tự động dịch audio
- **Exclude character names**: Loại trừ tên nhân vật
- **Audio processing provider**: Nhà cung cấp xử lý audio

### Nhật ký và gỡ lỗi (Logging & Debugging)

#### Xem nhật ký
1. Nhấn nút **Log** hoặc phím tắt
2. Xem thông tin chi tiết về:
   - Quá trình OCR
   - Kết nối API
   - Lỗi phát sinh

#### Cấp độ ghi nhật ký
- **INFO**: Thông tin chung
- **WARNING**: Cảnh báo
- **ERROR**: Lỗi nghiêm trọng
- **DEBUG**: Thông tin chi tiết (phát triển)

## Các mẹo và thủ thuật (Tips & Tricks)

### Tối ưu hiệu suất
1. **Chọn vùng nhỏ**: Chỉ chọn vùng cần thiết để tăng tốc độ
2. **Sử dụng Windows OCR**: Thường nhanh và chính xác hơn
3. **Điều chỉnh confidence**: Tăng ngưỡng để giảm văn bản nhiễu

### Xử lý sự cố thường gặp
1. **OCR không hoạt động**: Kiểm tra ngôn ngữ được cài đặt
2. **Dịch không chính xác**: Kiểm tra kết nối API và ngôn ngữ
3. **Ứng dụng lag**: Giảm tần suất capture hoặc chọn vùng nhỏ hơn

### Phím tắt hiệu quả
- Sử dụng phím tắt để chuyển vùng nhanh
- Thiết lập phím tắt phù hợp với thói quen
- Kết hợp với game hoặc ứng dụng khác

## Cấu trúc dự án (Project Structure)

```
screen-translation/
├── src/
│   ├── App/
│   │   ├── App.xaml/cs          # Điểm khởi đầu ứng dụng
│   ├── Core/
│   │   ├── Logic.cs             # Logic chính của ứng dụng
│   │   ├── TextObject.cs        # Xử lý đối tượng văn bản
│   │   └── TranslationEventArgs.cs
│   ├── Infrastructure/
│   │   ├── LogManager.cs        # Quản lý nhật ký
│   │   ├── MouseManager.cs      # Quản lý chuột
│   │   └── SocketManager.cs     # Quản lý socket
│   ├── Managers/
│   │   ├── ConfigManager.cs     # Quản lý cấu hình
│   │   ├── OcrServerManager.cs  # Quản lý máy chủ OCR
│   │   ├── WindowsOCRManager.cs # OCR Windows
│   │   ├── OneOCRManager.cs     # OCR OneOCR
│   │   └── BlockDetectionManager.cs
│   ├── Services/
│   │   ├── ChatGptTranslationService.cs
│   │   ├── ITranslationService.cs
│   │   └── WindowsTTSService.cs
│   ├── UI/
│   │   ├── MainWindow.xaml/cs   # Cửa sổ chính
│   │   ├── SettingsWindow.xaml/cs # Cửa sổ thiết lập
│   │   ├── QuickstartWindow.xaml/cs # Hướng dẫn thiết lập
│   │   └── ChatBoxWindow.xaml/cs # Cửa sổ chat
│   └── Utilities/
│       └── KeyboardShortcuts.cs # Hệ thống phím tắt
├── app/
│   ├── webserver/
│   │   └── PaddleOCR/           # Máy chủ OCR Python
│   ├── config.txt               # Cấu hình ứng dụng
│   └── chatgpt_config.txt       # Cấu hình ChatGPT
└── ST.csproj                    # File dự án
```

## API và Tích hợp (API & Integration)

### ChatGPT API Integration
- Sử dụng REST API của OpenAI
- Hỗ trợ streaming responses
- Xử lý lỗi và retry logic

### Socket Communication
- Máy chủ OCR chạy trên port 9998
- Giao tiếp JSON giữa C# và Python
- Xử lý đa luồng và timeout

### Windows API Integration
- Win32 API cho screen capture
- Windows OCR API
- Global keyboard hooks

