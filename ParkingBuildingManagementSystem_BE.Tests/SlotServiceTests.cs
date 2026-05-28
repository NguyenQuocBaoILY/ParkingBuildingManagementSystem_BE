using FluentAssertions;
using Moq;
using ParkingBuildingManagementSystem_BE.DTOs;
using ParkingBuildingManagementSystem_BE.Exceptions;
using ParkingBuildingManagementSystem_BE.Models;
using ParkingBuildingManagementSystem_BE.Repositories.Interfaces;
using ParkingBuildingManagementSystem_BE.Services.Implementations;

namespace ParkingBuildingManagementSystem_BE.Tests;

public class SlotServiceTests
{
    private readonly Mock<IVehicleTypeRepository> _vehicleTypeRepo = new();
    private readonly Mock<IFloorRepository> _floorRepo = new();
    private readonly SlotService _sut;

    public SlotServiceTests()
    {
        _sut = new SlotService(_vehicleTypeRepo.Object, _floorRepo.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static VehicleType BuildVehicleType(int id = 1, string name = "Xe máy") => new()
    {
        Id = id, Name = name
    };

    private static List<FloorSlotDto> BuildFloors(int count = 2, int availablePerFloor = 10) =>
        Enumerable.Range(1, count).Select(i => new FloorSlotDto
        {
            FloorId        = i,
            FloorName      = $"Tầng {i}",
            FloorNumber    = i,
            TotalSlots     = 20,
            OccupiedSlots  = 5,
            ReservedSlots  = 20 - availablePerFloor - 5,
            AvailableSlots = availablePerFloor
        }).ToList();

    // ── GetAvailabilityAsync — happy paths ────────────────────────────────────

    [Fact]
    public async Task GetAvailabilityAsync_ValidVehicleType_ReturnsCorrectSummary()
    {
        // Arrange
        var vehicleType = BuildVehicleType(1, "Xe máy");
        var floors = BuildFloors(count: 3, availablePerFloor: 8);

        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicleType);
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1)).ReturnsAsync(floors);

        // Act
        var result = await _sut.GetAvailabilityAsync(1);

        // Assert
        result.VehicleTypeId.Should().Be(1);
        result.VehicleTypeName.Should().Be("Xe máy");
        result.TotalAvailableSlots.Should().Be(24); // 3 floors × 8 slots
        result.Floors.Should().HaveCount(3);
        result.Floors.Should().AllSatisfy(f => f.HasAvailability.Should().BeTrue());
    }

    [Fact]
    public async Task GetAvailabilityAsync_NoFloorsAvailable_ReturnsTotalZero()
    {
        // Arrange
        var vehicleType = BuildVehicleType();
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicleType);
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1)).ReturnsAsync([]);

        // Act
        var result = await _sut.GetAvailabilityAsync(1);

        // Assert
        result.TotalAvailableSlots.Should().Be(0);
        result.Floors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailabilityAsync_MixedFloors_OnlyCountsAvailableSlots()
    {
        // Arrange
        var vehicleType = BuildVehicleType();
        var floors = new List<FloorSlotDto>
        {
            new() { FloorId = 1, FloorName = "Tầng 1", AvailableSlots = 0, TotalSlots = 10, OccupiedSlots = 5, ReservedSlots = 5 },
            new() { FloorId = 2, FloorName = "Tầng 2", AvailableSlots = 5, TotalSlots = 10, OccupiedSlots = 3, ReservedSlots = 2 },
        };

        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicleType);
        _floorRepo.Setup(r => r.GetAvailableByVehicleTypeAsync(1)).ReturnsAsync(floors);

        // Act
        var result = await _sut.GetAvailabilityAsync(1);

        // Assert
        result.TotalAvailableSlots.Should().Be(5);
        result.Floors[0].HasAvailability.Should().BeFalse();
        result.Floors[1].HasAvailability.Should().BeTrue();
    }

    // ── GetAvailabilityAsync — unhappy paths ──────────────────────────────────

    [Fact]
    public async Task GetAvailabilityAsync_VehicleTypeNotFound_Throws404()
    {
        // Arrange
        _vehicleTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VehicleType?)null);

        // Act
        var act = () => _sut.GetAvailabilityAsync(99);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }
}
