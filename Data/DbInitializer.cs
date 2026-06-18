using HTQL_DU_LICH.Models;

namespace HTQL_DU_LICH.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Interests.Any())
            {
                context.Interests.AddRange(
                    new Interest { Name = "Leo núi" },
                    new Interest { Name = "Ẩm thực" },
                    new Interest { Name = "Biển" },
                    new Interest { Name = "Cắm trại" },
                    new Interest { Name = "Chụp ảnh" },
                    new Interest { Name = "Nghỉ dưỡng" }
                );

                context.SaveChanges();
            }
        }
    }
}