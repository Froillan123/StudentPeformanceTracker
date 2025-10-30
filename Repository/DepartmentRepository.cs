using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.TeacherDepartments)
                .ThenInclude(td => td.Teacher)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.TeacherDepartments)
                .ThenInclude(td => td.Teacher)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department?> GetByNameAsync(string departmentName)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentName == departmentName);
    }

    public async Task<Department?> GetByCodeAsync(string departmentCode)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentCode == departmentCode);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;
        
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;
        
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Departments.AnyAsync(d => d.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string departmentName)
    {
        return await _context.Departments.AnyAsync(d => d.DepartmentName == departmentName);
    }

    public async Task<bool> ExistsByCodeAsync(string departmentCode)
    {
        return await _context.Departments.AnyAsync(d => d.DepartmentCode == departmentCode);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Departments.CountAsync();
    }

    public async Task<(IEnumerable<Department> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
    {
        var query = _context.Departments
            .Include(d => d.TeacherDepartments)
                .ThenInclude(td => td.Teacher)
            .AsQueryable();
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(d => d.DepartmentName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }
}
