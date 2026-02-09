using BaseLibrary.DTOs;
using BaseLibrary.Entites;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerLibrary.Data;
using ServerLibrary.Helper;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user is null) return new GeneralResponse(false, "Model is Empty");

            var checkUser = await FindUserByEmail(user.Email);
            if (checkUser != null) return new GeneralResponse(false, "User Registered already");

            // Save User
            var applicationUser = await AddToDatabase(new ApplicationUser()
            {
                FullName = user.FulName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });
            // check, create and assgin role
            var checkAdminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Helper.Constants.Admin));
            if (checkAdminRole is null)
            {
                var CreateAdminRole = await AddToDatabase(new SystemRole()
                {
                    Name = Helper.Constants.Admin
                });
                await AddToDatabase(new UserRole()
                {
                    RoleId = CreateAdminRole.Id,
                    UserId = applicationUser.Id
                });
                return new GeneralResponse(true, "Account Created successfully!!");
            }
            var checkUserRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.User));
            if (checkUserRole is null)
            {
                var response = await AddToDatabase(new SystemRole()
                {
                    Name = Constants.Admin
                });
                await AddToDatabase(new UserRole()
                {
                    RoleId = response.Id,
                    UserId = applicationUser.Id
                });
            }
            else
            {
                await AddToDatabase(new UserRole()
                {
                    RoleId = checkUserRole.Id,
                    UserId = applicationUser.Id
                });
            }
            return new GeneralResponse(true, "Account Created successfully!!");
        }

        public Task<LoginResponse> SignInAsync(Login user)
        {
            throw new NotImplementedException();
        }
        private async Task<ApplicationUser?> FindUserByEmail(string email) =>
                  await appDbContext.ApplicationUsers.FirstOrDefaultAsync(_ => _.Email!.ToLower()!.Equals(email!.ToLower()));

        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model!);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }
    }
}
