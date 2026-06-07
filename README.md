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

### Login & Register
![Login](screenshots/localhost_7123_Identity_Account_Login(Nest Hub Max).png)
![Register](screenshots/localhost_7123_Identity_Account_Register(Nest Hub Max).png)

### User Dashboard
![User Dashboard](screenshots/localhost_7123_User_Dashboard(Nest Hub Max).png)

### Exam Page
![Exam](screenshots/localhost_7123_Exam_Start_moduleId=1(Nest Hub Max).png)

### User Results
![User Results](screenshots/localhost_7123_User_Results(Nest Hub Max).png)

### Admin Dashboard
![Admin Dashboard](screenshots/localhost_7123_Admin_Dashboard(Nest Hub Max).png)

### Admin Questions
![Admin Questions](screenshots/localhost_7123_Admin_Questions_moduleId=1(Nest Hub Max).png)
![Add Question](screenshots/localhost_7123_Admin_AddQuestion_moduleId=1(Nest Hub Max).png)
![Edit Question](screenshots/localhost_7123_Admin_EditQuestion_2(Nest Hub Max).png)

### Admin Sessions
![Admin Sessions](screenshots/localhost_7123_AdminSession(Nest Hub Max).png)
![Session Results](screenshots/localhost_7123_AdminSession_Results_id=11(Nest Hub Max).png)

### Admin Users
![All Users](screenshots/localhost_7123_AdminUser(Nest Hub Max).png)
![User Details](screenshots/localhost_7123_AdminUser_Details_e7f97f29-2a45-4548-bc3f-5bac8f9d5a35(Nest Hub Max).png)
![Assign Student ID](screenshots/localhost_7123_AdminUser_AssignStudentId_e7f97f29-2a45-4548-bc3f-5bac8f9d5a35(Nest Hub Max).png)
![Reset Password](screenshots/localhost_7123_AdminUser_ResetPassword_e7f97f29-2a45-4548-bc3f-5bac8f9d5a35(Nest Hub Max).png)

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
git clone https://github.com/SabbirIqbalFarhan/Online-MCQ-Exam-Test-using-ASP-Dot-Net-Core-MVC.git

2. Open the solution file `Exam_Test.slnx` in Visual Studio

3. Update the connection string in `appsettings.json` to point to your SQL Server

4. Open Package Manager Console and run

   Add-Migration Init
   Update-Database

5. Run the project with **F5**

---

## Default Roles

The app uses two roles:
- **Admin** — manages everything
- **User** — takes exams

You need to manually seed the roles or create them via the registration page on first run.

---

## License

This project is for educational purposes.
