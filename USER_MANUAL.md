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
2. Chọn **Source Language** (ngôn ngữ nguồn) từ danh sách các ngôn ngữ được hỗ trợ
3. Chọn **Target Language** (ngôn ngữ đích)
4. Nhấn nút ⇄ để đổi chỗ hai ngôn ngữ
5. Nhấn **Check** để kiểm tra language pack Windows OCR (chỉ hiển thị khi chọn Windows OCR)

**Các ngôn ngữ hỗ trợ:**
- Tiếng Anh (en), Tiếng Nhật (ja), Tiếng Trung giản thể (ch_sim), Tiếng Trung phồn thể (ch_tra)
- Tiếng Hàn (ko), Tiếng Việt (vi), Tiếng Pháp (fr), Tiếng Đức (de), Tiếng Nga (ru)
- Tiếng Tây Ban Nha (es), Tiếng Ý (it), Tiếng Hindi (hi), Tiếng Bồ Đào Nha (pt)
- Tiếng Ả Rập (ar), Tiếng Hà Lan (nl), Tiếng Ba Lan (pl), Tiếng Romania (ro)
- Tiếng Ba Tư (fa), Tiếng Séc (cs), Tiếng Indonesia (id), Tiếng Thái (th), Tiếng Croatia (hr)

**Lưu ý:** Một số ngôn ngữ có thể yêu cầu cài đặt language pack bổ sung cho Windows OCR.

### Thiết lập OCR (OCR Configuration)

#### 1. Chọn phương thức OCR
- **Windows OCR**: Sử dụng OCR tích hợp của Windows. Ưu điểm: nhanh, chính xác cho nhiều ngôn ngữ, không cần cài đặt thêm
- **OneOCR**: Công cụ OCR nhanh với hiệu suất cao, sử dụng model AI nhẹ
- **PaddleOCR**: Công cụ OCR dựa trên PaddlePaddle với độ chính xác cao, hỗ trợ đa ngôn ngữ tốt

#### 2. Các tham số OCR cơ bản
- **Windows OCR integration**: Sử dụng Windows OCR làm bộ lọc trước khi gửi đến các OCR khác. Cải thiện hiệu suất nhưng có thể giảm độ chính xác trong một số trường hợp
- **Auto Translate**: Tự động dịch văn bản ngay khi OCR phát hiện. Tắt để chỉ dịch khi được yêu cầu
- **Smallest text fragment**: Kích thước tối thiểu (số ký tự) của đoạn văn bản để được xử lý. Các đoạn nhỏ hơn sẽ bị bỏ qua (mặc định: 2)
- **Min letter confidence**: Ngưỡng độ tin cậy tối thiểu cho từng ký tự (0.0-1.0). Ký tự có độ tin cậy thấp hơn sẽ bị lọc (mặc định: 0.1)
- **Min line confidence**: Ngưỡng độ tin cậy tối thiểu cho toàn bộ dòng (0.0-1.0). Dòng có độ tin cậy trung bình thấp hơn sẽ bị lọc (mặc định: 0.2)

#### 3. Block Detection Settings
- **Block Power**: Mức độ mạnh của việc ghép nhóm văn bản. Giá trị cao hơn làm văn bản dễ bị ghép thành đoạn lớn hơn, giá trị thấp hơn tách nhỏ hơn (tốt cho menu/button nhỏ). Ảnh hưởng đến việc nhóm ký tự theo cấp độ (mặc định: 5)
- **Settle Time**: Thời gian chờ (giây) để văn bản ổn định trước khi chụp (mặc định: 0.5)
- **Text Similar Threshold**: Kiểm tra độ tương tự giữa hai văn bản liên tiếp. Ví dụ: 0.5 = 50% - nếu văn bản 1 có độ tương tự >= 50% sẽ bị bỏ qua (mặc định: 0.75)
- **Char Level**: Chia kết quả OCR thành từng ký tự riêng lẻ

#### 4. Các tùy chọn khác
- **Multi Selection Area**: Cho phép chọn nhiều vùng dịch cùng lúc (tối đa 5 vùng)
- **Leave translation onscreen**: Luôn hiển thị bản dịch trên cửa sổ Monitor, không hiển thị ngôn ngữ gốc để dễ đọc bản dịch hơn
- **Select Screen**: Chọn màn hình để thực hiện OCR (quan trọng khi có nhiều màn hình)
- **Auto OCR**: Tự động OCR theo thời gian thực. Tắt tính năng này sẽ dừng dịch tự động

### Thiết lập dịch thuật (Translation Settings)

#### 1. Cấu hình ChatGPT
1. Mở tab **Translation** trong Settings
2. Chọn **Translation Service**: Hiện tại chỉ hỗ trợ ChatGPT
3. Nhập **Endpoint**: URL API endpoint của dịch vụ ChatGPT
4. Nhập **Username** và **Password**: Thông tin xác thực
5. Chỉnh sửa **Prompt Template**: Template prompt để hướng dẫn AI dịch thuật

#### 2. Các nút chức năng
- **Save Prompt**: Lưu template prompt đã chỉnh sửa
- **Restore Default Prompt**: Khôi phục về prompt mặc định

#### 3. Template Prompt
Template prompt là văn bản hướng dẫn AI cách dịch. Bao gồm:
- Hướng dẫn ngôn ngữ nguồn và đích
- Phong cách dịch thuật
- Ngữ cảnh cụ thể của game/ứng dụng
- Các quy tắc đặc biệt

**Ví dụ prompt mặc định:**
```
Translate the following text from {source_lang} to {target_lang}. Maintain the original meaning and tone. Keep the translation natural and fluent.

Text to translate:
{text}

Translation:
```

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

### Thiết lập ngữ cảnh (Context Settings)

#### 1. Các tham số ngữ cảnh
- **Max Previous Context**: Số lượng đoạn văn bản trước đó tối đa để đưa vào ngữ cảnh. Đặt 0 để tắt ngữ cảnh (mặc định: 3)
- **Min Context Size**: Kích thước tối thiểu (số ký tự) của đoạn văn bản để được đưa vào ngữ cảnh. Giúp tránh thêm menu/button nhỏ làm ngữ cảnh (mặc định: 20)
- **Min ChatBox Text Size**: Kích thước tối thiểu (số ký tự) của văn bản để hiển thị trong ChatBox. Văn bản nhỏ hơn sẽ bị bỏ qua (mặc định: 2)
- **Info about the game**: Thông tin về game đang chơi để giúp LLM dịch chính xác hơn

#### 2. Quản lý ngữ cảnh
- **Clear Translation Context**: Xóa tất cả lịch sử dịch thuật và ngữ cảnh. Buộc dịch mới trên lần chụp tiếp theo

### Thiết lập hiển thị (Overlay Settings)

#### 1. Cấu hình hiển thị
- **Text Overlay Config**: Điều chỉnh màu nền và màu chữ cho overlay
- **Auto Set Overlay Background Color**: Tự động chọn màu nền cho overlay dựa trên hình ảnh
- **Show Icon Signal When Start OCR**: Hiển thị biểu tượng nhỏ khi bắt đầu OCR và dịch

### Thiết lập lọc văn bản (Text Filtering)

#### 1. Quản lý cụm từ bỏ qua
- **Danh sách cụm từ**: Danh sách các cụm từ sẽ bị bỏ qua khi dịch
- **Exact Match**: Nếu bật, toàn bộ văn bản sẽ bị bỏ qua nếu khớp chính xác. Nếu tắt, cụm từ sẽ bị xóa khỏi văn bản

#### 2. Thao tác với danh sách
- **Add new phrase**: Thêm cụm từ mới vào danh sách
- **Remove**: Xóa cụm từ đã chọn khỏi danh sách

### Thiết lập Text-to-Speech (TTS)

#### 1. Cấu hình TTS cơ bản
- **Enable TTS**: Bật/tắt chức năng chuyển văn bản thành giọng nói
- **TTS Service**: Chọn dịch vụ TTS (hiện tại chỉ hỗ trợ Windows TTS)
- **Windows TTS Voice**: Chọn giọng đọc từ danh sách các giọng có sẵn

#### 2. Tùy chỉnh nâng cao
- **Exclude Character Name**: Tự động loại trừ tên nhân vật trong hội thoại. Định dạng yêu cầu: `<tên nhân vật>: <hội thoại>`

#### 3. Cài đặt giọng đọc Windows
Để cài đặt giọng đọc robot:
1. Vào Settings > Time & language > Speech > Add voices
2. Chọn ngôn ngữ muốn thêm và nhấn Add
3. Đợi giọng đọc được thêm vào hệ thống
4. Khởi động lại ScreenTranslation để hiển thị giọng mới

### Thiết lập phím tắt (HotKeys)

#### 1. Cách thiết lập phím tắt
1. Chọn **HotKey Functions** từ dropdown
2. Chọn **Combine Keys** (CTRL, SHIFT, ALT)
3. Chọn phím bổ sung (A-Z, 0-9, F1-F12, etc.)
4. Nhấn **Set HotKey** để áp dụng

#### 2. Danh sách phím tắt mặc định
- **Start/Stop**: ALT+G (bắt đầu/dừng dịch)
- **Overlay**: ALT+F (bật/tắt lớp phủ)
- **Setting**: ALT+P (mở cửa sổ thiết lập)
- **Log**: ALT+L (mở nhật ký)
- **Select Area**: ALT+Q (chọn vùng)
- **Clear Areas**: ALT+R (xóa tất cả vùng)
- **Clear Selected Area**: ALT+H (xóa vùng đã chọn)
- **Show Area**: ALT+B (hiển thị vùng)
- **ChatBox**: ALT+C (mở cửa sổ chat)
- **Area 1-5**: ALT+1 đến ALT+5 (chuyển đến vùng tương ứng)

### Thiết lập máy chủ (Server Settings)

#### 1. Cài đặt máy chủ
- **Install Server**: Cài đặt môi trường Python và dependencies cho máy chủ dịch (chỉ cần 1 lần)
- **Start Server**: Khởi động máy chủ dịch để hiển thị bản dịch trên các thiết bị khác

#### 2. Cách sử dụng máy chủ dịch
1. Nhấn 'Install' (thiết lập 1 lần - tạo môi trường Python và cài đặt dependencies)
2. Nhấn 'Start' để khởi động máy chủ
3. Truy cập máy chủ từ bất kỳ thiết bị nào trong mạng bằng URL hiển thị trong console

**Lưu ý**: Máy chủ sẽ tự động nhận dữ liệu dịch. Hữu ích để xem bản dịch trên điện thoại hoặc máy tính bảng thay vì dùng nhiều màn hình.

### Hồ sơ game (Game Profile)

#### 1. Quản lý hồ sơ
- **Profile Name**: Tên hồ sơ (không dấu, không khoảng trắng, ví dụ: GodOfWar, GTAV)
- **Create Profile**: Tạo hồ sơ mới
- **Remove Profile**: Xóa hồ sơ đã chọn
- **Update Profile**: Cập nhật hồ sơ hiện tại
- **Load Profile**: Tải hồ sơ đã lưu

#### 2. Lưu ý khi tạo hồ sơ
- Tên hồ sơ phải là ký tự không dấu, không có khoảng trắng
- Ví dụ: GodOfWar, GTAV, TheLastOfUs, etc.
- Mỗi hồ sơ lưu tất cả thiết lập của ứng dụng cho game cụ thể

## Các mẹo và thủ thuật (Tips & Tricks)

### Tối ưu hiệu suất
1. **Chọn vùng nhỏ**: Chỉ chọn vùng cần thiết để tăng tốc độ
2. **Sử dụng Windows OCR**: Thường nhanh và chính xác hơn cho các ngôn ngữ phổ biến
3. **Điều chỉnh confidence**: Tăng ngưỡng để giảm văn bản nhiễu, giảm false positive
4. **Tắt Multi Selection Area**: Nếu chỉ cần 1 vùng để cải thiện hiệu suất
5. **Điều chỉnh Block Power**: Giá trị thấp hơn cho UI game với nhiều text nhỏ

### Xử lý sự cố thường gặp
1. **OCR không hoạt động**: Kiểm tra ngôn ngữ được cài đặt trong Windows Settings
2. **Dịch không chính xác**: Kiểm tra kết nối API, endpoint URL và ngôn ngữ được chọn
3. **Ứng dụng lag**: Giảm tần suất capture, chọn vùng nhỏ hơn, hoặc tăng settle time
4. **Không có âm thanh TTS**: Kiểm tra Windows TTS voices đã được cài đặt
5. **Phím tắt không hoạt động**: Kiểm tra xem có ứng dụng khác đang sử dụng phím tắt đó không

### Phím tắt hiệu quả
- Sử dụng phím tắt để chuyển vùng nhanh (ALT+1, ALT+2, etc.)
- Thiết lập phím tắt phù hợp với thói quen gaming
- Kết hợp với game hoặc ứng dụng khác để workflow mượt mà

### Tối ưu hóa cho các loại nội dung khác nhau
1. **Game RPG**: Bật context để AI hiểu được câu chuyện
2. **Game Action**: Tăng settle time để text ổn định trước khi OCR
3. **Menu/UI**: Giảm block power để tách riêng các item menu
4. **Chat/Dialog**: Bật exclude character name để chỉ đọc nội dung hội thoại

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

