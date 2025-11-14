# 📚 Student Performance Tracker

A comprehensive web-based student performance tracking system built with ASP.NET Core, PostgreSQL, and modern web technologies. This system enables educational institutions to manage students, teachers, courses, grades, and generate detailed analytics reports.

## 🎯 Features

### 👨‍🎓 Student Portal
- **User Registration & Login** - Secure authentication with role-based access (login via username or email)
- **Account Status** - Pending approval status with highlighted notifications for inactive accounts
- **Dashboard** - Overview of enrolled classes, recent grades, and academic statistics
- **Enrollment System** - Comprehensive enrollment management with:
  - **Pending Enrollment Flow** - Select subjects with 12-unit cap, pending admin approval
  - **Browse Available Classes** - Filter by year level and course
  - **Browse by Section** - Semester-based section browsing with visual semester highlighting
  - **Year Level Filtering** - Automatically filtered to show only relevant sections for student's year level
  - **Entire Section Enrollment** - Option to enroll in entire sections
- **My Classes** - View all enrolled subjects with schedules, teachers, and EDP codes
- **My Grades** - Detailed grade breakdown by subject with Midterm and Final grades
- **Grade Reports** - Generate and download PDF/Excel reports of academic performance
- **Announcements** - View announcements including:
  - Class-specific announcements from teachers
  - General announcements from admins (with visual distinction)
- **Profile Management** - View and update personal information (first name, last name, phone) with real academic statistics (GPA, units, subjects)
- **Auto-Generated Student ID** - Unique student ID format: `ucmn-{YYMMDD}{studentId}`

### 👨‍🏫 Teacher Portal
- **Dashboard** - Overview of assigned classes, student statistics, and recent activity
- **My Classes** - Manage all assigned classes with student enrollment details
- **Grade Management** - Add, update, and delete student grades (Midterm and Final)
- **Class Reports** - Generate PDF/Excel reports for entire classes
- **Analytics Dashboard** - Comprehensive analytics with:
  - Grade distribution charts
  - Pass/Fail rate analysis
  - Class performance comparison
  - Student performance trends
  - Midterm vs Final grade comparison
- **Analytics Export** - Export analytics dashboard to PDF/Excel
- **Announcements** - Create and manage:
  - Class-scoped announcements for students
  - View general announcements from admins (with visual distinction)
- **Profile Management** - View and update profile with:
  - Personal information (first name, last name, phone)
  - Professional information (highest qualification, emergency contact)
  - Real-time teaching statistics (classes, students, subjects, total units)

### 👨‍💼 Admin Portal
- **Dashboard** - System-wide statistics and overview
- **Student Management** - Comprehensive student management with:
  - Create students (automatically set to "Active" status)
  - View all students with search and filter capabilities
  - Edit student details (first name, last name, email, phone, course, year level)
  - Username and Student ID are read-only (auto-generated)
  - Auto-generated Student ID format: `ucmn-{YYMMDD}{studentId}`
- **Teacher Management** - Create, view, update, and delete teacher accounts
- **User Management** - Manage all user accounts with status control
- **Course Management** - Create and manage academic courses
- **Subject Management** - Manage subjects with units and descriptions
- **Section Management** - Advanced section management with:
  - Create sections with duplicate validation
  - Search sections by name
  - Filter sections by course
  - Edit section details (name, max capacity, course, year level, semester)
- **Enrollment Management** - Comprehensive enrollment management:
  - View pending enrollments grouped by student
  - Approve or reject pending enrollments
  - Capacity validation during approval
  - Unit cap enforcement (12 units maximum)
- **General Announcements** - Create and manage system-wide announcements:
  - Visible to all students and teachers
  - Create, edit, and delete general announcements
  - Only the creating admin can edit/delete their announcements
  - Priority levels: General, Important, Urgent
- **Analytics Dashboard** - System-wide analytics with:
  - Student enrollment by course
  - Grade distribution across all courses
  - Pass/Fail rates
  - Faculty load distribution
  - Course performance overview
- **Analytics Export** - Export analytics dashboard to PDF/Excel with timestamp
- **Scrollable Sidebar** - Responsive sidebar navigation that scrolls on smaller screens

## 🏗️ System Architecture

```mermaid
graph TB
    subgraph "Frontend Layer"
        A[Razor Pages] --> B[Student Portal]
        A --> C[Teacher Portal]
        A --> D[Admin Portal]
    end
    
    subgraph "API Layer"
        E[AuthController] --> F[JWT Authentication]
        G[GradeController] --> H[Grade Management]
        I[ReportController] --> J[PDF/Excel Generation]
        K[AnnouncementController] --> L[Announcement System]
        M[AdminController] --> N[Admin Operations]
        O[TeacherController] --> P[Teacher Operations]
        Q[StudentController] --> R[Student Operations]
    end
    
    subgraph "Service Layer"
        S[AuthService] --> T[Authentication Logic]
        U[ReportService] --> V[QuestPDF/EPPlus]
        W[UserManagementService] --> X[User Operations]
    end
    
    subgraph "Repository Layer"
        Y[GradeRepository] --> Z[Database]
        AA[StudentRepository] --> Z
        AB[TeacherRepository] --> Z
        AC[AnnouncementRepository] --> Z
    end
    
    subgraph "Database"
        Z --> AD[(PostgreSQL)]
    end
    
    B --> E
    B --> Q
    C --> O
    C --> G
    C --> K
    D --> M
    D --> I
    E --> S
    G --> Y
    I --> U
    K --> AC
    M --> W
    S --> AA
    U --> Y
    W --> AA
```

## 🔐 User Roles & Permissions

```mermaid
graph LR
    A[User] --> B{Login}
    B --> C[Student]
    B --> D[Teacher]
    B --> E[Admin]
    
    C --> C1[View Own Grades]
    C --> C2[View Own Classes]
    C --> C3[View Announcements]
    C --> C4[Generate Reports]
    
    D --> D1[Manage Grades]
    D --> D2[View Assigned Classes]
    D --> D3[Create Announcements]
    D --> D4[View Analytics]
    D --> D5[Export Reports]
    
    E --> E1[Manage All Users]
    E --> E2[Manage Courses/Subjects]
    E --> E3[Manage Sections]
    E --> E4[View System Analytics]
    E --> E5[Export Analytics]
```

## 📊 Database Schema

```mermaid
erDiagram
    User ||--o| Student : has
    User ||--o| Teacher : has
    User ||--o| Admin : has
    
    Student ||--o{ Enrollment : has
    Student ||--o{ StudentSubject : enrolled_in
    StudentSubject }o--|| EnrollmentStatus : has_status
    Enrollment }o--|| Course : for
    Enrollment }o--|| YearLevel : in
    Enrollment }o--|| Semester : in
    
    StudentSubject }o--|| SectionSubject : belongs_to
    StudentSubject ||--o{ Grade : has
    
    SectionSubject }o--|| Section : part_of
    SectionSubject }o--|| Subject : teaches
    SectionSubject }o--|| Teacher : assigned_to
    
    Course ||--o{ CourseSubject : contains
    CourseSubject }o--|| Subject : includes
    CourseSubject }o--|| YearLevel : for
    CourseSubject }o--|| Semester : in
    
    Section }o--|| Course : belongs_to
    Section }o--|| YearLevel : for
    Section }o--|| Semester : in
    
    Teacher ||--o{ Announcement : creates
    Admin ||--o{ Announcement : creates_general
    Announcement }o--o| SectionSubject : scoped_to
    Announcement }o--o| Teacher : created_by
    Announcement }o--o| Admin : created_by
    
    Teacher ||--o{ TeacherDepartment : belongs_to
    TeacherDepartment }o--|| Department : part_of
```

### Database Schema Details

#### User & Authentication Tables

##### Users
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| Username | string(100) | Required, Unique | User login username |
| PasswordHash | string | Required | BCrypt hashed password |
| Role | string(20) | Required, Default: "Student" | User role: Student, Teacher, Admin |
| Status | string(20) | Required, Default: "Inactive" | Account status: Active, Inactive |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- One-to-One with Students (CASCADE DELETE)
- One-to-One with Teachers (CASCADE DELETE)
- One-to-One with Admins (CASCADE DELETE)

##### Students
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| UserId | int | Required, FK → Users.Id | Foreign key to Users table (CASCADE DELETE) |
| StudentId | string(50) | Required, Unique | Auto-generated student ID (format: ucmn-{YYMMDD}{studentId}) |
| Email | string(100) | Required, Unique, Email format | Student email address |
| FirstName | string(50) | Required | Student first name |
| LastName | string(50) | Required | Student last name |
| Phone | string(20) | Nullable | Student phone number |
| YearLevel | int | Nullable | Current year level |
| CourseId | int | Nullable, FK → Courses.Id | Foreign key to Courses table (SET NULL on delete) |
| EnrollmentDate | DateTime | Nullable | Date of enrollment |
| EnrollmentType | string(20) | Required, Default: "Regular" | Enrollment type: Regular or Irregular |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Users (CASCADE DELETE)
- Many-to-One with Courses (SET NULL on delete)
- One-to-Many with Enrollments
- One-to-Many with StudentSubjects

##### Teachers
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| UserId | int | Required, FK → Users.Id | Foreign key to Users table (CASCADE DELETE) |
| Email | string(100) | Required, Unique, Email format | Teacher email address |
| FirstName | string(50) | Required | Teacher first name |
| LastName | string(50) | Required | Teacher last name |
| Phone | string(20) | Nullable | Teacher phone number |
| HighestQualification | string(100) | Nullable | Highest educational qualification |
| Status | string(20) | Required, Default: "Full-time" | Employment status: Full-time or Part-time |
| EmergencyContact | string(100) | Nullable | Emergency contact name |
| EmergencyPhone | string(20) | Nullable | Emergency contact phone |
| HireDate | DateTime | Nullable | Date of hire |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Users (CASCADE DELETE)
- Many-to-Many with Departments (through TeacherDepartments, CASCADE DELETE)
- One-to-Many with SectionSubjects
- One-to-Many with TeacherSubjects
- One-to-Many with Announcements (nullable)

##### Admins
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| UserId | int | Required, FK → Users.Id | Foreign key to Users table (CASCADE DELETE) |
| Email | string(100) | Required, Unique, Email format | Admin email address |
| FirstName | string(50) | Required | Admin first name |
| LastName | string(50) | Required | Admin last name |
| Phone | string(20) | Nullable | Admin phone number |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Users (CASCADE DELETE)
- One-to-Many with Announcements (nullable, for general announcements)

#### Academic Structure Tables

##### Courses
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| CourseName | string(100) | Required, Unique | Course name |
| Description | string(500) | Nullable | Course description |
| DepartmentId | int | Nullable, FK → Departments.Id | Foreign key to Departments table (SET NULL on delete) |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Departments (SET NULL on delete)
- One-to-Many with CourseSubjects
- One-to-Many with Sections
- One-to-Many with Enrollments

##### Departments
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| DepartmentName | string(100) | Required, Unique | Department name |
| DepartmentCode | string(20) | Nullable, Unique | Department code |
| Description | string(500) | Nullable | Department description |
| HeadOfDepartment | string(100) | Nullable | Head of department name |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-Many with Teachers (through TeacherDepartments, CASCADE DELETE)

##### TeacherDepartments (Junction Table)
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| TeacherId | int | PK, FK → Teachers.Id | Composite primary key, foreign key to Teachers (CASCADE DELETE) |
| DepartmentId | int | PK, FK → Departments.Id | Composite primary key, foreign key to Departments (CASCADE DELETE) |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |

**Relationships:**
- Many-to-One with Teachers (CASCADE DELETE)
- Many-to-One with Departments (CASCADE DELETE)
- Composite Primary Key: (TeacherId, DepartmentId)

##### YearLevels
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| LevelNumber | int | Required, Unique | Year level number (1, 2, 3, 4, etc.) |
| LevelName | string(50) | Required | Year level name (e.g., "First Year", "Second Year") |
| Description | string(500) | Nullable | Year level description |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- One-to-Many with CourseSubjects
- One-to-Many with Sections
- One-to-Many with Enrollments

##### Semesters
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| SemesterName | string(50) | Required | Semester name (e.g., "First Semester", "Second Semester") |
| SemesterCode | string(20) | Required, Unique | Semester code (e.g., "1ST", "2ND", "SUMMER") |
| SchoolYear | string(20) | Required | School year (e.g., "2024-2025") |
| StartDate | DateTime | Nullable | Semester start date |
| EndDate | DateTime | Nullable | Semester end date |
| IsActive | bool | Required, Default: true | Active semester flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (SemesterCode, SchoolYear)

**Relationships:**
- One-to-Many with CourseSubjects
- One-to-Many with Sections
- One-to-Many with Enrollments

##### Subjects
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| SubjectName | string(200) | Required | Subject name |
| Description | string(1000) | Nullable | Subject description |
| Units | int | Required, Default: 3 | Number of units/credits |
| Prerequisites | string(500) | Nullable | Prerequisite subjects |
| IsActive | bool | Required, Default: true | Active subject flag |
| CourseId | int | Nullable | Optional course association |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Courses (nullable)
- One-to-Many with CourseSubjects
- One-to-Many with SectionSubjects

##### CourseSubjects
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| CourseId | int | Required, FK → Courses.Id | Foreign key to Courses table |
| SubjectId | int | Required, FK → Subjects.Id | Foreign key to Subjects table |
| YearLevelId | int | Required, FK → YearLevels.Id | Foreign key to YearLevels table |
| SemesterId | int | Required, FK → Semesters.Id | Foreign key to Semesters table |
| IsRequired | bool | Required, Default: true | Required subject flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (CourseId, SubjectId, YearLevelId, SemesterId)

**Relationships:**
- Many-to-One with Courses
- Many-to-One with Subjects
- Many-to-One with YearLevels
- Many-to-One with Semesters

#### Enrollment & Section Tables

##### Sections
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| SectionName | string(50) | Required | Section name (e.g., "A", "B", "1A") |
| CourseId | int | Required, FK → Courses.Id | Foreign key to Courses table |
| YearLevelId | int | Required, FK → YearLevels.Id | Foreign key to YearLevels table |
| SemesterId | int | Required, FK → Semesters.Id | Foreign key to Semesters table |
| MaxCapacity | int | Required, Default: 40 | Maximum student capacity |
| CurrentEnrollment | int | Required, Default: 0 | Current number of enrolled students |
| IsActive | bool | Required, Default: true | Active section flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (SectionName, CourseId, YearLevelId, SemesterId)

**Relationships:**
- Many-to-One with Courses
- Many-to-One with YearLevels
- Many-to-One with Semesters
- One-to-Many with SectionSubjects

##### SectionSubjects
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| SectionId | int | Required, FK → Sections.Id | Foreign key to Sections table |
| SubjectId | int | Required, FK → Subjects.Id | Foreign key to Subjects table |
| EdpCode | string(20) | Required, Unique | Enrollment and Drop (EDP) code |
| TeacherId | int | Nullable, FK → Teachers.Id | Foreign key to Teachers table (assigned teacher) |
| ScheduleDay | string(20) | Nullable | Class schedule day |
| ScheduleTime | string(50) | Nullable | Class schedule time |
| Room | string(50) | Nullable | Classroom location |
| MaxStudents | int | Required, Default: 40 | Maximum students for this subject section |
| CurrentEnrollment | int | Required, Default: 0 | Current enrollment count |
| IsActive | bool | Required, Default: true | Active section subject flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (SectionId, SubjectId)
- Unique index on EdpCode

**Relationships:**
- Many-to-One with Sections
- Many-to-One with Subjects
- Many-to-One with Teachers (nullable)
- One-to-Many with TeacherSubjects
- One-to-Many with StudentSubjects
- One-to-Many with Announcements (nullable)

##### TeacherSubjects
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| TeacherId | int | Required, FK → Teachers.Id | Foreign key to Teachers table |
| SectionSubjectId | int | Required, FK → SectionSubjects.Id | Foreign key to SectionSubjects table |
| IsPrimary | bool | Required, Default: true | Primary teacher flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (TeacherId, SectionSubjectId)

**Relationships:**
- Many-to-One with Teachers
- Many-to-One with SectionSubjects

##### Enrollments
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| StudentId | int | Required, FK → Students.Id | Foreign key to Students table |
| CourseId | int | Required, FK → Courses.Id | Foreign key to Courses table |
| YearLevelId | int | Required, FK → YearLevels.Id | Foreign key to YearLevels table |
| SemesterId | int | Required, FK → Semesters.Id | Foreign key to Semesters table |
| EnrollmentType | string(20) | Required, Default: "Regular" | Enrollment type: Regular or Irregular |
| EnrollmentDate | DateTime (UTC) | Required | Date of enrollment |
| Status | string(20) | Required, Default: "Pending" | Enrollment status: Pending, Active, Completed, Dropped |
| SchoolYear | string(20) | Nullable | School year (e.g., "2024-2025") |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (StudentId, CourseId, YearLevelId, SemesterId)

**Relationships:**
- Many-to-One with Students
- Many-to-One with Courses
- Many-to-One with YearLevels
- Many-to-One with Semesters
- One-to-Many with StudentSubjects

##### StudentSubjects
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| StudentId | int | Required, FK → Students.Id | Foreign key to Students table |
| SectionSubjectId | int | Required, FK → SectionSubjects.Id | Foreign key to SectionSubjects table |
| EnrollmentId | int | Required, FK → Enrollments.Id | Foreign key to Enrollments table |
| Grade | decimal(5,2) | Nullable | Final grade (1.0 to 5.0 scale) |
| Status | string(20) | Required, Default: "Enrolled" | Status: Enrolled, Completed, Dropped, Failed |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Unique Constraints:**
- Unique index on (StudentId, SectionSubjectId)

**Relationships:**
- Many-to-One with Students
- Many-to-One with SectionSubjects
- Many-to-One with Enrollments
- One-to-Many with Grades

#### Academic Records Tables

##### Grades
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| StudentSubjectId | int | Required, FK → StudentSubjects.Id | Foreign key to StudentSubjects table |
| AssessmentType | string(50) | Required | Assessment type: "Midterm" or "Final Grade" |
| AssessmentName | string(200) | Nullable | Assessment name (auto-filled as "Midterm Grade" or "Final Grade") |
| GradePoint | decimal(3,2) | Required, Range: 1.0-5.0 | Grade point (1.0 = Excellent, 5.0 = Failed) |
| Remarks | string(1000) | Nullable | Additional remarks |
| DateGiven | DateTime | Nullable | Date when grade was given |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with StudentSubjects

##### Announcements
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Auto-increment | Primary key |
| TeacherId | int | Nullable, FK → Teachers.Id | Foreign key to Teachers table (CASCADE DELETE, for class announcements) |
| SectionSubjectId | int | Nullable, FK → SectionSubjects.Id | Foreign key to SectionSubjects table (CASCADE DELETE, for class-scoped announcements) |
| AdminId | int | Nullable, FK → Admins.Id | Foreign key to Admins table (CASCADE DELETE, for general announcements) |
| Title | string(200) | Required | Announcement title |
| Content | string | Required | Announcement content |
| Priority | string(20) | Required, Default: "General" | Priority level: General, Important, Urgent |
| IsActive | bool | Required, Default: true | Active announcement flag |
| CreatedAt | DateTime (UTC) | Required | Record creation timestamp |
| UpdatedAt | DateTime (UTC) | Required | Record last update timestamp |

**Relationships:**
- Many-to-One with Teachers (nullable, CASCADE DELETE)
- Many-to-One with SectionSubjects (nullable, CASCADE DELETE)
- Many-to-One with Admins (nullable, CASCADE DELETE)

**Notes:**
- Class announcements: TeacherId and SectionSubjectId are set, AdminId is null
- General announcements: AdminId is set, TeacherId and SectionSubjectId are null

### Relationship Summary

**Cascade Delete Behaviors:**
- Users → Students, Teachers, Admins (CASCADE)
- Students → Enrollments, StudentSubjects (implicit through FK)
- Teachers → TeacherDepartments, SectionSubjects, TeacherSubjects, Announcements (CASCADE)
- Admins → Announcements (CASCADE)
- Departments → TeacherDepartments (CASCADE)
- Sections → SectionSubjects (implicit through FK)
- SectionSubjects → StudentSubjects, TeacherSubjects, Announcements (CASCADE)
- Enrollments → StudentSubjects (implicit through FK)
- StudentSubjects → Grades (implicit through FK)

**Set Null Behaviors:**
- Courses → Students.CourseId (SET NULL)
- Courses → Courses.DepartmentId (SET NULL)
- Departments → Courses.DepartmentId (SET NULL)

### Complete Database Schema Diagram (All Fields)

```mermaid
erDiagram
    Users {
        int Id PK
        string Username UK
        string PasswordHash
        string Role
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Students {
        int Id PK
        int UserId FK
        string StudentId UK
        string Email UK
        string FirstName
        string LastName
        string Phone
        int YearLevel
        int CourseId FK
        datetime EnrollmentDate
        string EnrollmentType
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Teachers {
        int Id PK
        int UserId FK
        string Email UK
        string FirstName
        string LastName
        string Phone
        string HighestQualification
        string Status
        string EmergencyContact
        string EmergencyPhone
        datetime HireDate
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Admins {
        int Id PK
        int UserId FK
        string Email UK
        string FirstName
        string LastName
        string Phone
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Courses {
        int Id PK
        string CourseName UK
        string Description
        int DepartmentId FK
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Departments {
        int Id PK
        string DepartmentName UK
        string DepartmentCode UK
        string Description
        string HeadOfDepartment
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    TeacherDepartments {
        int TeacherId PK_FK
        int DepartmentId PK_FK
        datetime CreatedAt
    }
    
    YearLevels {
        int Id PK
        int LevelNumber UK
        string LevelName
        string Description
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Semesters {
        int Id PK
        string SemesterName
        string SemesterCode UK
        string SchoolYear
        datetime StartDate
        datetime EndDate
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Subjects {
        int Id PK
        string SubjectName
        string Description
        int Units
        string Prerequisites
        bool IsActive
        int CourseId
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    CourseSubjects {
        int Id PK
        int CourseId FK
        int SubjectId FK
        int YearLevelId FK
        int SemesterId FK
        bool IsRequired
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Sections {
        int Id PK
        string SectionName
        int CourseId FK
        int YearLevelId FK
        int SemesterId FK
        int MaxCapacity
        int CurrentEnrollment
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    SectionSubjects {
        int Id PK
        int SectionId FK
        int SubjectId FK
        string EdpCode UK
        int TeacherId FK
        string ScheduleDay
        string ScheduleTime
        string Room
        int MaxStudents
        int CurrentEnrollment
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    TeacherSubjects {
        int Id PK
        int TeacherId FK
        int SectionSubjectId FK
        bool IsPrimary
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Enrollments {
        int Id PK
        int StudentId FK
        int CourseId FK
        int YearLevelId FK
        int SemesterId FK
        string EnrollmentType
        datetime EnrollmentDate
        string Status
        string SchoolYear
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    StudentSubjects {
        int Id PK
        int StudentId FK
        int SectionSubjectId FK
        int EnrollmentId FK
        decimal Grade
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Grades {
        int Id PK
        int StudentSubjectId FK
        string AssessmentType
        string AssessmentName
        decimal GradePoint
        string Remarks
        datetime DateGiven
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Announcements {
        int Id PK
        int TeacherId FK
        int SectionSubjectId FK
        int AdminId FK
        string Title
        string Content
        string Priority
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Users ||--o| Students : "has (1:1)"
    Users ||--o| Teachers : "has (1:1)"
    Users ||--o| Admins : "has (1:1)"
    
    Students ||--o{ Enrollments : "has"
    Students ||--o{ StudentSubjects : "enrolled_in"
    Students }o--|| Courses : "belongs_to"
    
    Teachers ||--o{ TeacherDepartments : "belongs_to"
    Teachers ||--o{ SectionSubjects : "assigned_to"
    Teachers ||--o{ TeacherSubjects : "teaches"
    Teachers ||--o{ Announcements : "creates"
    
    Admins ||--o{ Announcements : "creates_general"
    
    Departments ||--o{ TeacherDepartments : "has"
    Departments ||--o{ Courses : "contains"
    
    Courses ||--o{ CourseSubjects : "contains"
    Courses ||--o{ Sections : "has"
    Courses ||--o{ Enrollments : "for"
    
    YearLevels ||--o{ CourseSubjects : "for"
    YearLevels ||--o{ Sections : "for"
    YearLevels ||--o{ Enrollments : "in"
    
    Semesters ||--o{ CourseSubjects : "in"
    Semesters ||--o{ Sections : "in"
    Semesters ||--o{ Enrollments : "in"
    
    Subjects ||--o{ CourseSubjects : "included_in"
    Subjects ||--o{ SectionSubjects : "teaches"
    
    CourseSubjects }o--|| Courses : "belongs_to"
    CourseSubjects }o--|| Subjects : "includes"
    CourseSubjects }o--|| YearLevels : "for"
    CourseSubjects }o--|| Semesters : "in"
    
    Sections ||--o{ SectionSubjects : "contains"
    Sections }o--|| Courses : "belongs_to"
    Sections }o--|| YearLevels : "for"
    Sections }o--|| Semesters : "in"
    
    SectionSubjects ||--o{ TeacherSubjects : "assigned_to"
    SectionSubjects ||--o{ StudentSubjects : "enrolled_in"
    SectionSubjects ||--o{ Announcements : "scoped_to"
    SectionSubjects }o--|| Sections : "part_of"
    SectionSubjects }o--|| Subjects : "teaches"
    SectionSubjects }o--o| Teachers : "assigned_to"
    
    TeacherSubjects }o--|| Teachers : "assigned_to"
    TeacherSubjects }o--|| SectionSubjects : "teaches"
    
    Enrollments }o--|| Students : "belongs_to"
    Enrollments }o--|| Courses : "for"
    Enrollments }o--|| YearLevels : "in"
    Enrollments }o--|| Semesters : "in"
    Enrollments ||--o{ StudentSubjects : "contains"
    
    StudentSubjects }o--|| Students : "belongs_to"
    StudentSubjects }o--|| SectionSubjects : "enrolled_in"
    StudentSubjects }o--|| Enrollments : "part_of"
    StudentSubjects ||--o{ Grades : "has"
    
    Grades }o--|| StudentSubjects : "belongs_to"
    
    Announcements }o--o| Teachers : "created_by"
    Announcements }o--o| SectionSubjects : "scoped_to"
    Announcements }o--o| Admins : "created_by"
```

**Legend:**
- **PK** = Primary Key
- **FK** = Foreign Key
- **UK** = Unique Key/Constraint
- **CASCADE DELETE** = Deletes related records when parent is deleted
- **SET NULL** = Sets foreign key to null when parent is deleted

## 🔄 User Workflows

### Student Enrollment Flow

```mermaid
sequenceDiagram
    participant S as Student
    participant API as API
    participant DB as Database
    participant A as Admin
    
    S->>API: Register Account
    API->>DB: Create User & Student (Status: Inactive)
    DB-->>API: Success (Auto-generated Student ID)
    API-->>S: Account Created (Pending Approval)
    
    A->>API: Approve Student
    API->>DB: Update Status to Active
    DB-->>API: Success
    API-->>A: Student Approved
    
    S->>API: Login (Username/Email)
    API->>DB: Verify Credentials & Status
    DB-->>API: JWT Token
    API-->>S: Redirect to Dashboard
    
    S->>API: Browse Available Classes
    API->>DB: Get Subjects by Course/Year/Sem (Filtered by Student Year Level)
    DB-->>API: Subject List
    API-->>S: Display Subjects
    
    S->>API: Select Subjects (12-unit cap)
    API->>DB: Create Pending Enrollments
    DB-->>API: Pending Created
    API-->>S: Pending Status
    
    A->>API: View Pending Enrollments
    API->>DB: Get Pending by Student
    DB-->>API: Pending List
    API-->>A: Display Pending
    
    A->>API: Approve/Reject Pending
    API->>DB: Validate Capacity & Update Status
    DB-->>API: Enrollment Confirmed/Rejected
    API-->>A: Success Message
```

### Grade Management Flow

```mermaid
sequenceDiagram
    participant T as Teacher
    participant API as API
    participant DB as Database
    participant S as Student
    
    T->>API: View My Classes
    API->>DB: Get Assigned Classes
    DB-->>API: Class List
    API-->>T: Display Classes
    
    T->>API: Select Class
    API->>DB: Get Students in Class
    DB-->>API: Student List
    API-->>T: Display Students
    
    T->>API: Add Grade (Midterm/Final)
    API->>DB: Validate & Save Grade
    DB-->>API: Grade Saved
    API-->>T: Success Message
    
    S->>API: View Grades
    API->>DB: Get Student Grades
    DB-->>API: Grade List
    API-->>S: Display Grades
```

### Report Generation Flow

```mermaid
sequenceDiagram
    participant U as User
    participant API as API
    participant RS as ReportService
    participant PDF as QuestPDF
    participant EX as EPPlus
    
    U->>API: Request Report (PDF/Excel)
    API->>RS: Generate Report
    RS->>API: Fetch Data
    API-->>RS: Data Retrieved
    
    alt PDF Report
        RS->>PDF: Create Document
        PDF-->>RS: PDF Bytes
    else Excel Report
        RS->>EX: Create Workbook
        EX-->>RS: Excel Bytes
    end
    
    RS-->>API: Report Bytes
    API-->>U: Download File
```

## 🛠️ Technology Stack

- **Backend Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL
- **Authentication**: JWT (JSON Web Tokens)
- **PDF Generation**: QuestPDF
- **Excel Generation**: EPPlus
- **Frontend**: Razor Pages, Bootstrap 5, Chart.js
- **API Versioning**: Asp.Versioning
- **Caching**: Redis (optional)

## 📦 Installation & Setup

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 12+
- Node.js (for frontend assets, optional)

### Database Setup

1. Create PostgreSQL database:
```sql
CREATE DATABASE student_performance_tracker;
```

2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=student_performance_tracker;Username=your_user;Password=your_password"
  }
}
```

3. Run migration scripts in order:
   - `Migrations/rbac-migration.sql`
   - `Migrations/departments-migration.sql`
   - `complete-enrollment-system-migration.sql`
   - `announcements_table.sql`
   - `Migrations/admin-general-announcements-migration.sql` (PostgreSQL)

### Application Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd StudentPeformanceTracker
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

5. Access the application:
   - URL: `https://localhost:5001` or `http://localhost:5000`
   - Default admin credentials (if seeded): Check `Data/DbInitializer.cs`

## 🔌 API Endpoints

### Authentication
- `POST /api/v1/auth/login` - User login (supports username or email for all roles)
- `POST /api/v1/auth/register` - User registration
- `POST /api/v1/auth/register/student` - Student registration (self-registration: Inactive status, admin-created: Active status)
- `POST /api/v1/auth/refresh` - Refresh JWT token
- `POST /api/v1/auth/logout` - User logout

### Student Endpoints
- `GET /api/v1/student/profile` - Get student profile (includes CourseId)
- `PUT /api/v1/student/profile` - Update student profile (first name, last name, phone)
- `GET /api/v1/studentsubject/student/{id}/enrolled` - Get enrolled classes
- `GET /api/v1/studentsubject/student/{id}/pending` - Get pending enrollments
- `POST /api/v1/studentsubject/enroll` - Enroll via EDP code
- `POST /api/v1/studentsubject/select` - Select subjects for pending enrollment (12-unit cap)
- `DELETE /api/v1/studentsubject/{id}` - Drop class
- `DELETE /api/v1/studentsubject/pending/{id}` - Delete pending enrollment

### Teacher Endpoints
- `GET /api/v1/teacher/profile` - Get teacher profile
- `PUT /api/v1/teacher/profile` - Update teacher profile (first name, last name, phone, highest qualification, emergency contact)
- `GET /api/v1/teacher/classes` - Get assigned classes
- `GET /api/v1/sectionsubject/teacher/{id}` - Get classes by teacher ID

### Grade Management
- `GET /api/v1/grade/student/{id}` - Get student grades
- `GET /api/v1/grade/studentsubject/{id}` - Get grades for student-subject
- `POST /api/v1/grade` - Create grade (Teacher only)
- `PUT /api/v1/grade/{id}` - Update grade (Teacher only)
- `DELETE /api/v1/grade/{id}` - Delete grade (Teacher only)

### Reports
- `GET /api/v1/report/student/{id}/grades/pdf` - Student grade report (PDF)
- `GET /api/v1/report/student/{id}/grades/excel` - Student grade report (Excel)
- `GET /api/v1/report/teacher/class/{id}/pdf` - Class grade report (PDF)
- `GET /api/v1/report/teacher/class/{id}/excel` - Class grade report (Excel)
- `GET /api/v1/report/teacher/{id}/analytics/pdf` - Teacher analytics (PDF)
- `GET /api/v1/report/teacher/{id}/analytics/excel` - Teacher analytics (Excel)
- `GET /api/v1/report/admin/analytics/pdf` - Admin analytics (PDF)
- `GET /api/v1/report/admin/analytics/excel` - Admin analytics (Excel)

### Announcements
- `GET /api/v1/announcement` - Get active announcements (Student/Teacher - includes class-specific and general)
- `GET /api/v1/announcement/{id}` - Get announcement by ID (Student/Teacher/Admin)
- `POST /api/v1/announcement` - Create class announcement (Teacher only)
- `PUT /api/v1/announcement/{id}` - Update class announcement (Teacher only, own announcements)
- `DELETE /api/v1/announcement/{id}` - Delete class announcement (Teacher only, own announcements)
- `GET /api/v1/announcement/admin/general` - Get all general announcements (Admin only)
- `POST /api/v1/announcement/admin/general` - Create general announcement (Admin only)
- `PUT /api/v1/announcement/admin/general/{id}` - Update general announcement (Admin only, own announcements)
- `DELETE /api/v1/announcement/admin/general/{id}` - Delete general announcement (Admin only, own announcements)

### Admin Endpoints
- `GET /api/v1/admin/profile` - Get admin profile
- `PUT /api/v1/admin/profile` - Update admin profile
- `GET /api/v1/admin/sectionsubject/teacher/{id}` - Get classes by teacher (Admin)
- `GET /api/v1/user-management` - Get all users (Admin only)
- `POST /api/v1/teacher/admin-create` - Create teacher (Admin only)
- `DELETE /api/v1/user-management/{id}` - Delete user (Admin only)
- `GET /api/v1/student` - Get all students with pagination (Admin only)
- `GET /api/v1/student/{id}` - Get student by ID (Admin only)
- `PUT /api/v1/student/{id}` - Update student details (Admin only, username and student ID are read-only)
- `GET /api/v1/studentsubject/pending` - Get all pending enrollments grouped by student (Admin only)
- `POST /api/v1/enrollment/approve` - Approve pending enrollments (Admin only, with capacity validation)
- `POST /api/v1/enrollment/reject` - Reject pending enrollments (Admin only)
- `GET /api/v1/section` - Get all sections (Admin only)
- `POST /api/v1/section` - Create section (Admin only, with duplicate validation)
- `PUT /api/v1/section/{id}` - Update section (Admin only, with duplicate validation)

## 📈 Features Breakdown

### Analytics & Reporting

#### Admin Analytics
- **Key Metrics**: Total students, active faculty, courses offered, average performance
- **Enrollment by Course**: Top 10 courses by enrollment
- **Grade Distribution**: A-F grade ranges with counts
- **Pass/Fail Rate**: System-wide pass and fail percentages
- **Faculty Load**: Top 10 faculty by student count and classes

#### Teacher Analytics
- **Key Metrics**: Total classes, total students, average grade, pass rate
- **Grade Distribution**: Distribution across grade ranges
- **Class Performance**: Average grade per class
- **Pass/Fail Analysis**: Pass and fail rates with student counts
- **Trends**: Grade trends over time

### Announcement System
- **Class-Scoped Announcements**: Teachers post announcements to specific classes
- **General Announcements**: Admins can create system-wide announcements visible to all students and teachers
- **Role-Based Visibility**: 
  - Students see announcements from enrolled classes + general announcements
  - Teachers see announcements from their assigned classes + general announcements
  - Visual distinction between class-specific and general announcements
- **Priority Levels**: General, Important, Urgent
- **Active/Inactive**: Toggle announcement visibility
- **Ownership Control**: 
  - Teachers can only edit/delete their own class announcements
  - Admins can only edit/delete their own general announcements

### Grade Management
- **Assessment Types**: Midterm and Final Grade
- **Auto-Calculation**: Automatic percentage and remarks calculation
- **Grade Scale**: 1.0 (Excellent) to 5.0 (Failed)
- **Remarks**: Excellent, Very Good, Good, Pass, Failed
- **Validation**: Prevents duplicate grades of same type

### Enrollment Management
- **Pending Enrollment System**: Students select subjects with 12-unit cap, pending admin approval
- **Unit Cap Enforcement**: Maximum 12 units per enrollment period
- **Capacity Validation**: Automatic capacity checking during approval
- **Year Level Filtering**: Students only see sections relevant to their year level
- **Semester-Based Browsing**: Browse sections by semester with visual highlighting
- **Entire Section Enrollment**: Option to enroll in all subjects of a section
- **Status Management**: Pending → Enrolled/Rejected workflow

### User Management
- **Auto-Generated Student IDs**: Format `ucmn-{YYMMDD}{studentId}` (e.g., `ucmn-2511112001`)
- **Status Control**: 
  - Self-registered students: "Inactive" (pending approval)
  - Admin-created students: "Active" (immediate access)
- **Flexible Login**: All roles can login with username or email
- **Account Status Notifications**: Highlighted messages for pending approval accounts
- **Read-Only Fields**: Username and Student ID cannot be modified after creation

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-Based Authorization**: Student, Teacher, Admin policies
- **Password Hashing**: BCrypt password hashing
- **HttpOnly Cookies**: Secure cookie storage for tokens
- **CORS Configuration**: Cross-origin resource sharing setup
- **Input Validation**: Server-side validation for all inputs
- **Account Status Verification**: Login blocked for inactive accounts with clear messaging
- **Ownership Validation**: Users can only modify their own resources (announcements, profiles)
- **Capacity Validation**: Prevents over-enrollment in sections
- **Duplicate Prevention**: Server-side validation for duplicate sections and enrollments

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📞 Support

For support, email support@example.com or create an issue in the repository.

---

**Built with ❤️ using ASP.NET Core**
