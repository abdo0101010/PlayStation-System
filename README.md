# 🎮 PlayStation Cafe & Gaming Lounge Management System

نظام ويب متكامل لإدارة صالات ألعاب البلايستيشن، تتبع الجلسات النشطة، حساب تعرفة الوقت تلقائياً، إدارة مبيعات البوفيه، ومتابعة خزينة الورديات ومديونيات العملاء.

---

## 🌟 المميزات الرئيسية (Features)

### 1️⃣ إدارة الجلسات والأجهزة (Sessions & Devices)
* **عداد حي ومباشر (Live Real-Time Stopwatch):** تتبع وقت اللعب بالثواني والدقائق وحساب التكلفة اللحظية تلقائياً.
* **تسعير مرن:** دعم أنظمة اللعب الفردي والزوجي (`Single` / `Multi`) لكل جهاز على حدة.
* **إنهاء الجلسة والفاتورة الذكية:** حساب تكلفة وقت اللعب ودمج طلبات البوفيه مع إمكانية تطبيق خصومات وإغلاق الفاتورة في خطوة واحدة.

### 2️⃣ البوفيه والمخزون (Buffet & Inventory)
* تسجيل مبيعات المشروبات والمسليات أثناء إنهاء الجلسة أو بشكل منفصل.
* خصم تلقائي من كميات المخزون عند إتمام الفاتورة لمنع عجز العهدة.

### 3️⃣ الورديات والخزينة (Shifts & Cash Drawer)
* إدارة فتح وإغلاق الشيفتات مع تسجيل رصيد البداية والنهاية.
* فصل إيرادات اللعب عن إيرادات البوفيه لمراجعة مالية دقيقة لكل كاشير.

### 4️⃣ إدارة العملاء والمديونيات (CRM & Debt Tracking)
* تسجيل بيانات العملاء ورصيد النقاط وسجل المديونيات.
* واجهة مخصصة لسداد الديون وطباعة كشف الحساب.

### 5️⃣ الأمان وإدارة الصلاحيات (Security & RBAC)
* نظام حماية متكامل باستخدام **ASP.NET Core Identity**.
* صلاحيات متعددة للأدوار (`Admin`, `Cashier`) مع شاشات لإدارة الرولز والمستخدمين.

---

## 🛠️ التقنيات المستخدمة (Tech Stack)

* **Backend:** ASP.NET Core MVC (.NET 8)
* **Database & ORM:** SQL Server / Entity Framework Core (Code-First)
* **Authentication & Authorization:** ASP.NET Core Identity
* **Frontend:** Bootstrap 5, FontAwesome, JavaScript (Vanilla JS for dynamic calculations), HTML5/CSS3
* **Architecture:** MVC Pattern, Generic Repository & Service Pattern

---

## 🚀 طريقة التثبيت والتشغيل (Getting Started)

### المتطلبات (Prerequisites)
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) أو أحدث.
* [SQL Server](https://www.microsoft.com/en-us/sql-server) أو LocalDB.
* Visual Studio 2022 أو VS Code.

### خطوات التشغيل (Installation)

1. **استنساخ المستودع (Clone Repository):**
   ```bash
   git clone [https://github.com/your-username/PlaystationSystem.git](https://github.com/your-username/PlaystationSystem.git)
   cd PlaystationSystem
