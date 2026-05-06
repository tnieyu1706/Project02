🏰 Project02: Unity Hybrid Strategy Game ⚔️

Build, Defend & Attack - Một dự án game chiến thuật kết hợp đa chiều trên nền tảng Unity 6.0

(Gợi ý: Bạn có thể thay thế dòng này bằng một ảnh GIF hoặc Banner giới thiệu game của bạn)

📖 Giới thiệu tổng quan (Overview)

Dự án là một trò chơi chiến thuật lai (Hybrid Strategy) hoạt động ngoại tuyến (Offline, Single-player), được phát triển trên nền tảng Unity 6.0.

Trò chơi là sự kết hợp độc đáo giữa ba cơ chế lối chơi cốt lõi:

🔨 Kiến thiết căn cứ (Building)

🛡️ Phòng thủ tháp (Tower Defense)

⚔️ Tấn công theo đợt (Wave Attack)

Thay vì đi theo lối mòn của các game thuần thủ thành, dự án tạo ra một vòng lặp trò chơi (Game Loop) đa chiều, đòi hỏi người chơi phải liên tục đưa ra các quyết định linh hoạt: từ khai thác tài nguyên, xây dựng phòng tuyến cho đến chủ động dàn quân tấn công căn cứ địch.

🎮 Tính năng nổi bật (Core Gameplay)

Hệ thống Xây dựng (Building System): Cho phép người chơi khai thác tài nguyên, đặt móng xây dựng và nâng cấp các công trình trên bản đồ lưới (Grid Map).

Hệ thống Phòng thủ (Tower Defense System): Xây dựng và quản lý các tháp canh, tính toán sát thương và tầm nhìn để chống lại các đợt tiến công từ hệ thống.

Hệ thống Tấn công (Wave Attack System): Chủ động thiết lập cấu hình, điều động lính và triển khai các đợt tấn công để tiêu diệt căn cứ địch.

Smart Wave Converter: Hệ thống tự động phân giải cấu hình tĩnh (Wave Config) thành các thông số nội suy phức tạp (thời gian chuẩn, tuyến đường, điểm spawn) để điều phối chiến trường tự động.

🛠️ Kỹ thuật & Kiến trúc phần mềm (Tech Stack & Architecture)

Dự án được xây dựng với định hướng Module hóa (Modular) và Liên kết lỏng (Loose coupling), tuân thủ chặt chẽ các nguyên lý lập trình hướng đối tượng (OOP) và Design Patterns:

🧩 Kiến trúc PBR (Preset - Behaviour - Runtime): Lấy cảm hứng từ hệ thống ECS. Dữ liệu cấu hình (Preset/Scriptable Objects) được tách biệt hoàn toàn khỏi logic nghiệp vụ (POCO classes), giúp việc quản lý hàng loạt thực thể trong game trở nên độc lập và dễ mở rộng.

⚡ Kiến trúc Hướng sự kiện (Event-driven): Đảm bảo đồng bộ hóa dữ liệu thời gian thực giữa Data (Backend) và UI (Frontend) mà không tạo ra sự phụ thuộc cứng.

🎬 Multi-scene Management: Tách biệt môi trường game thành các bối cảnh độc lập (Bootstrapper, Global Gameplay, Building, Base Fight) nhằm nạp/gỡ tài nguyên linh hoạt theo ngữ cảnh.

🚀 Tối ưu hóa Hiệu năng (Zero-allocation & Memory Management):

Tích hợp thư viện UniTask để xử lý bất đồng bộ, triệt tiêu chi phí cấp phát bộ nhớ (Garbage Collection).

Sử dụng LitMotion cho các hiệu ứng tweening mượt mà, tối ưu tài nguyên.

Ứng dụng Addressables để quản lý nạp/gỡ Asset và Object Pooling để quản lý vòng đời thực thể (spawn/destroy lính, đạn pháo).

🧠 Utility-based AI: Trí tuệ nhân tạo điều khiển kẻ địch và tháp phòng thủ được lập trình dựa trên thuật toán đánh giá trọng số (threat score/weights) để tự động hóa các quyết định chiến thuật phức tạp.

🚀 Hướng dẫn cài đặt (Getting Started)

Yêu cầu môi trường

Unity Editor: Phiên bản 6.0 trở lên.

Unity Hub: Dùng để quản lý project và version.

Các bước khởi chạy

Clone repository này về máy cục bộ của bạn:

git clone [https://github.com/tnieyu1706/Project02.git](https://github.com/tnieyu1706/Project02.git)


Mở Unity Hub, chọn Add Project và trỏ tới thư mục vừa clone. (Đảm bảo mở bằng đúng phiên bản Unity 6.0).

Chờ Unity tự động khôi phục các package cần thiết (như UniTask, LitMotion, Addressables) thông qua Package Manager.

Trong cửa sổ Project, điều hướng tới thư mục chứa các Scene:

Assets/_Project/Scenes/


Mở scene hệ thống cốt lõi: Bootstrapper.unity.

Nhấn nút ▶ Play trên Editor để trải nghiệm trò chơi.

⚠️ Lưu ý quan trọng: Game áp dụng kiến trúc Multi-scene, bắt buộc phải chạy từ scene Bootstrapper để hệ thống khởi tạo các DataManager và Controller dùng chung.

🗺️ Lộ trình phát triển (Roadmap)

Dự án hiện tại đóng vai trò như một bản Proof of Concept về mặt kiến trúc hệ thống. Các định hướng phát triển tiếp theo bao gồm:

[ ] Editor Tooling: Xây dựng công cụ nội bộ (Level Builder, Wave Configurator) ngay trong Unity Inspector để hỗ trợ thiết kế màn chơi trực quan.

[ ] Advanced AI: Nâng cấp Utility-based AI với các tham số động (Dynamic Parameters) và tích hợp Cây hành vi (Behaviour Tree) cho lính/kẻ địch.

[ ] Asset Pre-loading: Nâng cấp thuật toán nạp tài nguyên theo lô (Batch Loading) của Addressables để tối ưu hóa thời gian chờ.

[ ] Mobile Optimization: Tinh chỉnh UI Toolkit, tối ưu bộ nhớ RAM và đồ họa để sẵn sàng xuất bản (publish) trò chơi trên nền tảng di động.
