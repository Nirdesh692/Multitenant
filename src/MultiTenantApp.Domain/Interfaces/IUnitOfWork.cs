using System;
using System.Threading.Tasks;
using Attendance.Domain.Entities;

namespace Attendance.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    Task<int> SaveChangesAsync();
}