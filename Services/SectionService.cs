using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services
{
    public class SectionService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionSubjectRepository _sectionSubjectRepository;

        public SectionService(ISectionRepository sectionRepository, ISectionSubjectRepository sectionSubjectRepository)
        {
            _sectionRepository = sectionRepository;
            _sectionSubjectRepository = sectionSubjectRepository;
        }

        public async Task<IEnumerable<SectionDto>> GetAllAsync()
        {
            var sections = await _sectionRepository.GetAllAsync();
            return sections.Select(MapToDto);
        }

        public async Task<SectionDto?> GetByIdAsync(int id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            return section != null ? MapToDto(section) : null;
        }

        public async Task<IEnumerable<SectionDto>> GetByCourseIdAsync(int courseId)
        {
            var sections = await _sectionRepository.GetByCourseIdAsync(courseId);
            return sections.Select(MapToDto);
        }

        public async Task<IEnumerable<SectionDto>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId)
        {
            var sections = await _sectionRepository.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
            return sections.Select(MapToDto);
        }

        public async Task<SectionDto?> GetBySectionNameCourseYearSemesterAsync(string sectionName, int courseId, int yearLevelId, int semesterId)
        {
            var section = await _sectionRepository.GetBySectionNameCourseYearSemesterAsync(sectionName, courseId, yearLevelId, semesterId);
            return section != null ? MapToDto(section) : null;
        }

        public async Task<SectionDto> CreateAsync(Section section)
        {
            var created = await _sectionRepository.CreateAsync(section);
            return MapToDto(created);
        }

        public async Task<SectionDto> UpdateAsync(Section section)
        {
            var updated = await _sectionRepository.UpdateAsync(section);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _sectionRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<SectionSubject>> GetSectionSubjectsAsync(int sectionId)
        {
            return await _sectionSubjectRepository.GetBySectionIdAsync(sectionId);
        }

        public async Task<SectionSubject> AssignSubjectToSectionAsync(SectionSubject sectionSubject)
        {
            // Generate unique EDP code
            sectionSubject.EdpCode = await GenerateUniqueEdpCodeAsync();
            
            var created = await _sectionSubjectRepository.CreateAsync(sectionSubject);
            return created;
        }

        private async Task<string> GenerateUniqueEdpCodeAsync()
        {
            string edpCode;
            bool exists;
            
            do
            {
                edpCode = GenerateRandomEdpCode();
                var existing = await _sectionSubjectRepository.GetByEdpCodeAsync(edpCode);
                exists = existing != null;
            } while (exists);

            return edpCode;
        }

        private static string GenerateRandomEdpCode()
        {
            var random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        private static SectionDto MapToDto(Section section)
        {
            return new SectionDto
            {
                Id = section.Id,
                SectionName = section.SectionName,
                CourseId = section.CourseId,
                CourseName = section.Course?.CourseName ?? string.Empty,
                YearLevelId = section.YearLevelId,
                YearLevelName = section.YearLevel?.LevelName ?? string.Empty,
                SemesterId = section.SemesterId,
                SemesterName = section.Semester?.SemesterName ?? string.Empty,
                SchoolYear = section.Semester?.SchoolYear ?? string.Empty,
                MaxCapacity = section.MaxCapacity,
                CurrentEnrollment = section.CurrentEnrollment,
                IsActive = section.IsActive,
                CreatedAt = section.CreatedAt,
                UpdatedAt = section.UpdatedAt
            };
        }
    }
}
