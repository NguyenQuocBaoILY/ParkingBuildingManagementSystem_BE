using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Interfaces;

namespace ParkingBuildingManagementSystem_BE.Services.Implementations;

public class VehicleTypeService(IVehicleTypeRepository vehicleTypeRepository) : IVehicleTypeService
{
    public async Task<IEnumerable<VehicleTypeResponse>> GetAllAsync()
    {
        var types = await vehicleTypeRepository.GetAllAsync();
        return types.Select(MapToResponse);
    }

    public async Task<VehicleTypeResponse> GetByIdAsync(int id)
    {
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        return MapToResponse(vehicleType);
    }

    public async Task<VehicleTypeResponse> CreateAsync(VehicleTypeRequest request)
    {
        var existing = await vehicleTypeRepository.GetByNameAsync(request.Name);
        if (existing != null)
            throw new AppException("Tên loại phương tiện đã tồn tại.", StatusCodes.Status409Conflict);

        var vehicleType = new VehicleType
        {
            Name = request.Name,
            Description = request.Description
        };

        await vehicleTypeRepository.AddAsync(vehicleType);
        await vehicleTypeRepository.SaveChangesAsync();

        return MapToResponse(vehicleType);
    }

    public async Task<VehicleTypeResponse> UpdateAsync(int id, VehicleTypeRequest request)
    {
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        var duplicate = await vehicleTypeRepository.GetByNameAsync(request.Name);
        if (duplicate != null && duplicate.Id != id)
            throw new AppException("Tên loại phương tiện đã tồn tại.", StatusCodes.Status409Conflict);

        vehicleType.Name = request.Name;
        vehicleType.Description = request.Description;

        vehicleTypeRepository.Update(vehicleType);
        await vehicleTypeRepository.SaveChangesAsync();

        return MapToResponse(vehicleType);
    }

    public async Task DeleteAsync(int id)
    {
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id)
            ?? throw new AppException("Loại phương tiện không tồn tại.", StatusCodes.Status404NotFound);

        if (await vehicleTypeRepository.IsInUseAsync(id))
            throw new AppException(
                "Không thể xóa loại phương tiện đang được sử dụng.",
                StatusCodes.Status409Conflict);

        vehicleTypeRepository.Delete(vehicleType);
        await vehicleTypeRepository.SaveChangesAsync();
    }

    private static VehicleTypeResponse MapToResponse(VehicleType v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Description = v.Description
    };
}
