🎯 Core Roles
Role	Capabilities
Student	Can enroll, pick sections/subjects (if irregular), view schedule & units
Teacher	Assigned to multiple subjects across multiple departments
Admin	Creates Courses, Subjects, Sections, Assign Teachers, Control Enrollment Parameters
🧱 Main Entities (Tables)

These are the minimum para limpyo and scalable.

Table	Purpose
departments	Ex: CICS, HM, Education, etc.
courses	Ex: BSIT, BSHM, BEED — NOT BSIT-1/2/3/4
year_levels	Example: 1, 2, 3, 4
semesters	1st Sem, 2nd Sem
subjects	Example: English 101, Math 201 (units included)
course_subjects	Map Course + Year + Semester → Subjects
sections	Example: BSIT-1A, BSIT-1B, BSIT-2A etc. (with max slots)
teachers	Teacher Information
teacher_subjects	Which teacher teaches which subject and what section
students	Student Profiles
enrollments	Student → (Course, Year, Semester)
student_subjects	Subjects student is enrolled in (for irregular students)
✅ Correct Hierarchy (This is the correct modeling)
A Course is NOT BSIT-1, BSIT-2, etc.

Course = BSIT (once only)

Then add:

Year Level = 1,2,3,4
Semester = 1st or 2nd


Then:

BSIT + 1st Year + 1st Sem → has List of Subjects
BSIT + 1st Year + 2nd Sem → has List of Subjects
BSIT + 2nd Year + 1st Sem → has List of Subjects
...


So subjects vary per year and semester.

📌 Sectioning Logic

Example:

Section Name	Course	Year	Sem	Max Slots
BSIT-1A	BSIT	1	1	40
BSIT-1B	BSIT	1	1	40
BSIT-1C	BSIT	1	1	40

Each section has its own EDP codes per subject instance.

🔥 EDP Code Logic

When a subject is assigned to a section:

Subject: Math 101
Section: BSIT-1A
Generated EDP: random or incremental unique number (e.g., 20513)


Meaning:

BSIT-1A Math 101 → 20513
BSIT-1B Math 101 → 20587


Each section’s same subject can have different EDP codes → Correct.

🎓 Student Enrollment Workflow

Student selects Course (only once — permanent)

If regular, system automatically:

Detects course → year → sem → subjects

Assigns default section if slots available

If irregular:

They manually pick available subjects across multiple sections

System checks:

Unit limits

Schedule conflicts

If subject belongs to their course

🔁 Teacher Assignment

Teachers can be assigned to:

Multiple departments

Multiple subjects

Multiple sections

So table:

teacher_subjects
teacher_id | subject_id | section_id | semester | school_year

✅ Your System Plan is Correct — Just Needs Clean Structure

So don’t create BSIT-1, BSIT-2, etc. as courses
Instead treat Year Level separately.

🧠 Prompt you can use to fully model the database:

Prompt:

I want to design a Student Enrollment and Sectioning System with the following rules:
- A department contains multiple courses.
- A course has multiple year levels (1–4) and each year has two semesters.
- Each semester has a list of subjects with units.
- A section is defined by course + year + semester and has a max capacity.
- Every time a subject is assigned to a section, it gets a unique EDP code.
- Teachers can be assigned to multiple subjects in multiple departments.
- Students enroll into their course, year level, and semester, and can be regular or irregular.
- Regular students automatically get subjects based on course/year/sem.
- Irregular students can pick subjects across different sections provided there are slots and no schedule conflicts.
Generate a complete Entity Relationship Diagram (ERD) and SQL tables that support this.