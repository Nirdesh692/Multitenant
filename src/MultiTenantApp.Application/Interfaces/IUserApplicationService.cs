using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Application.Interfaces;

public interface IUserApplicationService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(string email, string firstName, string lastName, string password, UserRole role);
}