using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Common;
using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed theo thứ tự phụ thuộc FK
        await SeedVehicleTypesAsync(context);
        await SeedFloorsAsync(context);
        await SeedUsersAsync(context);
        await SeedFloorVehicleTypesAsync(context);
        await SeedPricingPoliciesAsync(context);
        await SeedVehiclesAsync(context);
        await SeedPoliciesAsync(context);
    }

    // -------------------------------------------------------------------------
    // 1. VehicleType — không có FK
    // -------------------------------------------------------------------------
    private static async Task SeedVehicleTypesAsync(AppDbContext context)
    {
        if (await context.VehicleTypes.AnyAsync()) return;

        context.VehicleTypes.AddRange(
            new VehicleType { Name = "Xe máy",  Description = "Xe máy, xe scooter, xe máy điện" },
            new VehicleType { Name = "Ô tô",    Description = "Ô tô con dưới 7 chỗ" },
            new VehicleType { Name = "Xe đạp",  Description = "Xe đạp thông thường và xe đạp điện" }
        );
        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 2. Floor — không có FK
    // -------------------------------------------------------------------------
    private static async Task SeedFloorsAsync(AppDbContext context)
    {
        if (await context.Floors.AnyAsync()) return;

        context.Floors.AddRange(
            new Floor { FloorNumber = 1, Name = "Tầng B1", TotalSlots = 50, Description = "Tầng hầm 1 — dành riêng cho xe máy" },
            new Floor { FloorNumber = 2, Name = "Tầng B2", TotalSlots = 30, Description = "Tầng hầm 2 — dành riêng cho ô tô" },
            new Floor { FloorNumber = 3, Name = "Tầng B3", TotalSlots = 20, Description = "Tầng hầm 3 — hỗn hợp các loại xe" }
        );
        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 3. User — không có FK
    //    Roles: admin | manager | staff | driver
    // -------------------------------------------------------------------------
    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var now = VnTime.Now;

        context.Users.AddRange(
            // Admin
            new User
            {
                FullName          = "Admin Hệ Thống",
                Email             = "admin@parking.vn",
                Phone             = "0900000001",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role              = "admin",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            // Manager
            new User
            {
                FullName          = "Nguyễn Văn Quản Lý",
                Email             = "manager@parking.vn",
                Phone             = "0900000002",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                Role              = "manager",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            // Staff
            new User
            {
                FullName          = "Trần Thị Nhân Viên A",
                Email             = "staff1@parking.vn",
                Phone             = "0900000003",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role              = "staff",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            new User
            {
                FullName          = "Lê Văn Nhân Viên B",
                Email             = "staff2@parking.vn",
                Phone             = "0900000004",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role              = "staff",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            // Drivers
            new User
            {
                FullName          = "Phạm Thị Bích Ngọc",
                Email             = "customer1@gmail.com",
                Phone             = "0912345001",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role              = "driver",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            new User
            {
                FullName          = "Hoàng Văn Minh",
                Email             = "customer2@gmail.com",
                Phone             = "0912345002",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role              = "driver",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            },
            new User
            {
                FullName          = "Ngô Thị Thu Hương",
                Email             = "customer3@gmail.com",
                Phone             = "0912345003",
                PasswordHash      = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role              = "driver",
                IsActive          = true,
                IsEmailVerified   = true,
                CreatedAt         = now,
                UpdatedAt         = now
            }
        );
        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 4. FloorVehicleType (many-to-many) — FK: Floor, VehicleType
    //    B1 → xe máy
    //    B2 → ô tô
    //    B3 → xe máy, ô tô, xe tải nhỏ
    // -------------------------------------------------------------------------
    private static async Task SeedFloorVehicleTypesAsync(AppDbContext context)
    {
        var floor1 = await context.Floors
            .Include(f => f.VehicleTypes)
            .FirstAsync(f => f.FloorNumber == 1);

        if (floor1.VehicleTypes.Any()) return;

        var floor2 = await context.Floors.Include(f => f.VehicleTypes).FirstAsync(f => f.FloorNumber == 2);
        var floor3 = await context.Floors.Include(f => f.VehicleTypes).FirstAsync(f => f.FloorNumber == 3);

        var motorbike = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe máy");
        var car       = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Ô tô");
        var bicycle   = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe đạp");

        floor1.VehicleTypes.Add(motorbike);

        floor2.VehicleTypes.Add(car);

        floor3.VehicleTypes.Add(motorbike);
        floor3.VehicleTypes.Add(car);
        floor3.VehicleTypes.Add(bicycle);

        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 5. PricingPolicy — FK: VehicleType, Floor, User (admin)
    //    Mỗi cặp (floor, vehicleType) có một chính sách giá hiệu lực từ 2024-01-01
    // -------------------------------------------------------------------------
    private static async Task SeedPricingPoliciesAsync(AppDbContext context)
    {
        if (await context.PricingPolicies.AnyAsync()) return;

        var admin     = await context.Users.FirstAsync(u => u.Role == "admin");
        var floor1    = await context.Floors.FirstAsync(f => f.FloorNumber == 1);
        var floor2    = await context.Floors.FirstAsync(f => f.FloorNumber == 2);
        var floor3    = await context.Floors.FirstAsync(f => f.FloorNumber == 3);
        var motorbike = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe máy");
        var car       = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Ô tô");
        var bicycle   = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe đạp");

        var effectiveFrom = new DateOnly(2024, 1, 1);
        var now = VnTime.Now;

        context.PricingPolicies.AddRange(
            // B1 — Xe máy
            new PricingPolicy
            {
                VehicleTypeId = motorbike.Id, FloorId = floor1.Id,
                PricePerHour  = 5_000m,       DepositAmount = 20_000m,
                EffectiveFrom = effectiveFrom, CreatedBy = admin.Id, CreatedAt = now
            },
            // B2 — Ô tô
            new PricingPolicy
            {
                VehicleTypeId = car.Id,       FloorId = floor2.Id,
                PricePerHour  = 20_000m,      DepositAmount = 50_000m,
                EffectiveFrom = effectiveFrom, CreatedBy = admin.Id, CreatedAt = now
            },
            // B3 — Xe máy
            new PricingPolicy
            {
                VehicleTypeId = motorbike.Id, FloorId = floor3.Id,
                PricePerHour  = 5_000m,       DepositAmount = 20_000m,
                EffectiveFrom = effectiveFrom, CreatedBy = admin.Id, CreatedAt = now
            },
            // B3 — Ô tô
            new PricingPolicy
            {
                VehicleTypeId = car.Id,       FloorId = floor3.Id,
                PricePerHour  = 20_000m,      DepositAmount = 50_000m,
                EffectiveFrom = effectiveFrom, CreatedBy = admin.Id, CreatedAt = now
            },
            // B3 — Xe đạp
            new PricingPolicy
            {
                VehicleTypeId = bicycle.Id,   FloorId = floor3.Id,
                PricePerHour  = 2_000m,       DepositAmount = 5_000m,
                EffectiveFrom = effectiveFrom, CreatedBy = admin.Id, CreatedAt = now
            }
        );
        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 6. Vehicle — FK: User, VehicleType
    // -------------------------------------------------------------------------
    private static async Task SeedVehiclesAsync(AppDbContext context)
    {
        if (await context.Vehicles.AnyAsync()) return;

        var customers = await context.Users
            .Where(u => u.Role == "driver")
            .OrderBy(u => u.Id)
            .ToListAsync();

        var motorbike = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe máy");
        var car       = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Ô tô");
        var bicycle   = await context.VehicleTypes.FirstAsync(vt => vt.Name == "Xe đạp");

        context.Vehicles.AddRange(
            // customer1 — xe máy + ô tô
            new Vehicle { UserId = customers[0].Id, VehicleTypeId = motorbike.Id, LicensePlate = "59X1-12345" },
            new Vehicle { UserId = customers[0].Id, VehicleTypeId = car.Id,       LicensePlate = "51A-56789" },
            // customer2 — xe máy
            new Vehicle { UserId = customers[1].Id, VehicleTypeId = motorbike.Id, LicensePlate = "59B2-67890" },
            // customer3 — ô tô + xe đạp
            new Vehicle { UserId = customers[2].Id, VehicleTypeId = car.Id,       LicensePlate = "51G-99999" },
            new Vehicle { UserId = customers[2].Id, VehicleTypeId = bicycle.Id,   LicensePlate = "XD-001" }
        );
        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 7. Policy (nội quy/chính sách hệ thống) — FK: User (admin)
    // -------------------------------------------------------------------------
    private static async Task SeedPoliciesAsync(AppDbContext context)
    {
        if (await context.Policies.AnyAsync()) return;

        var admin = await context.Users.FirstAsync(u => u.Role == "admin");
        var now   = VnTime.Now;

        context.Policies.AddRange(
            new Policy
            {
                PolicyType = "parking_rules",
                Title      = "Quy định giữ xe",
                Content    = "1. Khách hàng phải xuất trình vé/mã QR khi vào và ra.\n"
                           + "2. Không đỗ xe sai vị trí, sai loại xe theo quy định từng tầng.\n"
                           + "3. Tắt máy xe khi vào bãi đỗ.\n"
                           + "4. Ban quản lý không chịu trách nhiệm với tài sản để trong xe.",
                IsActive   = true,
                UpdatedBy  = admin.Id,
                UpdatedAt  = now
            },
            new Policy
            {
                PolicyType = "deposit_refund",
                Title      = "Chính sách hoàn tiền cọc",
                Content    = "Tiền cọc được hoàn trả toàn bộ khi check-out thành công.\n"
                           + "Hủy đặt chỗ trước giờ check-in dự kiến ít nhất 60 phút: hoàn 100%.\n"
                           + "Hủy muộn hơn hoặc không đến: không hoàn cọc.",
                IsActive   = true,
                UpdatedBy  = admin.Id,
                UpdatedAt  = now
            },
            new Policy
            {
                PolicyType = "fine",
                Title      = "Quy định xử phạt vi phạm",
                Content    = "Đỗ lấn chiếm vị trí: phạt 50.000đ.\n"
                           + "Đỗ sai loại xe (ví dụ ô tô vào tầng xe máy): phạt 30.000đ.\n"
                           + "Gây mất trật tự, hư hại tài sản: phạt 100.000đ trở lên tùy mức độ.",
                IsActive   = true,
                UpdatedBy  = admin.Id,
                UpdatedAt  = now
            },
            new Policy
            {
                PolicyType = "operating_hours",
                Title      = "Giờ hoạt động",
                Content    = "Bãi giữ xe hoạt động 24/7 tất cả các ngày trong năm kể cả ngày lễ.",
                IsActive   = true,
                UpdatedBy  = admin.Id,
                UpdatedAt  = now
            }
        );
        await context.SaveChangesAsync();
    }
}
