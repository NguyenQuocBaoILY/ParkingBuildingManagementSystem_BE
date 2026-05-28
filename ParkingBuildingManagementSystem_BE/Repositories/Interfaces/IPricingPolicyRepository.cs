using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Repositories.Interfaces;

public interface IPricingPolicyRepository
{
    /// <summary>
    /// Lấy chính sách giá đang hiệu lực cho vehicleTypeId + floorId.
    /// Ưu tiên policy theo tầng cụ thể (FloorId not null) hơn policy toàn tòa nhà (FloorId null).
    /// </summary>
    Task<PricingPolicy?> GetActiveByVehicleTypeAndFloorAsync(int vehicleTypeId, int floorId);
}
