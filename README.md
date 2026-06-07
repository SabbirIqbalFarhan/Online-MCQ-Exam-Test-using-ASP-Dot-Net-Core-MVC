# Online MCQ Exam System

A web-based multiple choice exam platform built with ASP.NET Core MVC. Administrators can manage questions, sessions, and users. Students can take exams module by module and instantly see their results.

---

## Features

### Admin
- Create and manage exam sessions (activate/deactivate)
- Add, edit, and delete questions per module (with image support via ImgBB)
- Set maximum question limit per module
- Grant or revoke exam access per user
- View detailed results for every user per session

### User / Student
- Request exam access from admin
- Take exams when session is live (Module 1, 2, 3)
- Randomized question order on every attempt
- Instant result with score, correct/wrong breakdown
- Full question review with correct answers highlighted

---

## Screenshots

> Add your screenshots here after uploading them (see setup guide below)

---

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity (role-based auth: Admin / User)
- ImgBB API (image hosting for questions)
- Bootstrap 5

---

## Getting Started

### Prerequisites
- Visual Studio 2022
- .NET 8 SDK
- SQL Server

### Setup Steps

1. Clone the repository
   git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git

2. Open the solution file `Exam_Test.slnx` in Visual Studio

3. Update the connection string in `appsettings.json` to point to your SQL Server

4. Open Package Manager Console and run:
   Add-Migration Init
   Update-Database

5. Run the project with F5

---

## Default Roles

The app uses two roles:
- **Admin** — manages everything
- **User** — takes exams

You need to manually seed the roles or create them via the registration page on first run.

---

## License

This project is for educational purposes.
