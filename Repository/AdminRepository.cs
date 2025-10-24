using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetByIdAsync(int id)
    {
        return await _context.Admins
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Admin?> GetByUserIdAsync(int userId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<Admin?> GetByEmailAsync(string email)
    {
        return await _context.Admins
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<IEnumerable<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<Admin> CreateAsync(Admin admin)
    {
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task<Admin> UpdateAsync(Admin admin)
    {
        admin.UpdatedAt = DateTime.UtcNow;
        _context.Admins.Update(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return false;

        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Admins.AnyAsync(a => a.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Admins.AnyAsync(a => a.Email == email);
    }
}
