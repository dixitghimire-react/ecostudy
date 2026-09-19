# EcoStudy — Economics Study Portal

A modern, academic study portal for Economics learners and educators built with **ASP.NET Core MVC**, **.NET 8 LTS**, **Entity Framework Core**, **SQL Server**, and **Bootstrap 5**.

---

## 🌟 Features

### 1. Curriculum Architecture
- **Economics $\rightarrow$ Chapters $\rightarrow$ Notes + Important Questions**
- Dedicated categorizations for **Microeconomics**, **Macroeconomics**, **Money & Banking**, and **Quantitative Economics**.

### 2. Role-Based Portals & Authorization
- **Faculty / Teacher Portal (`[Authorize(Roles = "Teacher")]`):**
  - **Executive Dashboard:** Total Chapters, Study Notes, Important Questions, and Enrolled Students with recently added content streams.
  - **Chapter Management (Full CRUD):** Create, update, view, and delete curriculum chapters with duplicate detection.
  - **Notes Management:** Upload, update, download, and delete study notes and handouts (`.pdf`, `.doc`, `.docx`, `.ppt`, `.pptx` up to 25 MB).
  - **Important Questions Bank:** High-yield exam questions categorized into **Short**, **Long**, and **Numerical** with marks weightage, difficulty, and scoring hints.
  - **Student Directory:** View enrolled student rosters.
- **Student Study Desk (`[Authorize(Roles = "Student")]`):**
  - **Learning Dashboard:** Quick overview of available chapters, study notes, and practice questions.
  - **Curriculum Browser:** Open chapters, view syllabus scope, and study attached materials.
  - **Question Practice:** Filter by **Short Questions**, **Long Questions**, and **Numerical Problems** with collapsible solution keys.
  - **Profile Management:** Update personal profile and academic level.

### 3. Polish & User Experience
- **Unified Global Search:** Search across curriculum chapters, study notes, and questions with category filter pills and suggested topic chips.
- **Dark & Light Mode:** Zero-flicker `<head>` initialization with custom CSS theme tokens and persistent state via `localStorage`.
- **Responsive Mobile Layout:** Off-canvas drawer sidebar with a backdrop overlay and quick tap-to-dismiss behavior.
- **Economics-Themed Error Pages:** Friendly, branded status code pages (404 Resource Scarcity, 403 Regulatory Authorization Required, 500 Server Disequilibrium).
- **Safety & UX:** Standardized Bootstrap 5 delete confirmation dialogs, submit button loading spinners, and explicit CSRF anti-forgery protection.

---

## 🛠️ Technology Stack

- **Framework:** .NET 8.0 (LTS) & ASP.NET Core MVC
- **Database:** Microsoft SQL Server (LocalDB / Azure SQL / SQL Server)
- **ORM:** Entity Framework Core 8 with Code-First Migrations & Split Query Optimization
- **Authentication & Security:** ASP.NET Core Identity with role-based authorization and CSRF validation
- **Styling:** Bootstrap 5.3, Bootstrap Icons 1.11, and custom CSS design system

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB included with Visual Studio / VS Code C# Dev Kit, or any SQL Server instance)

### Installation & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/dixitghimire-react/ecostudy.git
   cd ecostudy
   ```

2. **Restore dependencies & Build:**
   ```bash
   dotnet build
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Open in browser:**
   Navigate to `http://localhost:5101` (or the URL shown in the terminal).

> **Database Note:** You do **not** need to run any manual migration commands. The application automatically creates the SQL Server database, applies all EF Core migrations, and seeds the default roles and sample data on initial startup via `DbInitializer.cs`.

---

## 🔑 Demo Accounts

The portal comes pre-seeded with sample accounts for immediate testing:

| Role | Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Teacher** | `teacher@ecostudy.com` | `Teacher@123` | Full Chapter, Note & Question CRUD, Student Roster |
| **Student** | `student@ecostudy.com` | `Student@123` | Study Notes, Downloads, Question Bank, Profile |
| **Student (Alt)** | `sarah.jenkins@ecostudy.com` | `Student@123` | Student learning workspace |

New students can also self-register directly via the **Student Registration** page.

---

## 📁 Project Structure

```
econotes/
├── Controllers/
│   ├── AccountController.cs       # Identity login, register, logout, access denied
│   ├── HomeController.cs          # Public landing, global search, friendly error pages
│   ├── StudentController.cs       # Student study desk, chapters, notes, questions
│   └── TeacherController.cs       # Faculty dashboard, chapter/note/question CRUD
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core DbContext with Cascade & SplitQuery configuration
│   └── DbInitializer.cs           # Automated migration & seed data runner
├── Models/
│   ├── ApplicationUser.cs         # Extended IdentityUser model
│   ├── Chapter.cs                 # Curriculum Chapter entity
│   ├── Note.cs                    # Study note & handout metadata
│   ├── ImportantQuestion.cs       # Categorized exam question entity
│   └── ViewModels/                # DTOs & view models for search, dashboard, and forms
├── Services/
│   ├── IFileService.cs            # File storage abstraction
│   └── FileService.cs             # Safe file upload, validation & deletion
├── Views/
│   ├── Account/                   # Login, register, access denied
│   ├── Home/                      # Index, about, global search, error pages
│   ├── Shared/                    # _Layout, _DashboardLayout, partials
│   ├── Student/                   # Student views (Dashboard, Chapters, Notes, Questions, Profile)
│   └── Teacher/                   # Teacher views (Dashboard, Chapters, Notes, Questions, Students)
└── wwwroot/
    ├── css/site.css               # Design system & dark mode tokens
    └── uploads/notes/             # Secure local storage for uploaded notes
```

---

## 📄 License
This project is open-source and available under the [MIT License](LICENSE).
