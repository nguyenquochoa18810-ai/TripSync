using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HTQL_DU_LICH.Data
{
    // Lớp này CHỈ dùng lúc thiết kế (design-time) khi chạy Add-Migration / Update-Database.
    // Nó giúp EF tạo được ApplicationDbContext mà KHÔNG cần chạy Program.cs,
    // tránh lỗi: "Unable to resolve service for type 'DbContextOptions...'".
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>();

            // LƯU Ý: chuỗi này phải KHỚP với appsettings.json (cùng Server + Database).
            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=HTQL_DULICH_New;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}