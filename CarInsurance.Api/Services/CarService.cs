using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate >= date)
        );
    }

    public async Task<ClaimResponseDto> RegisterClaim(long carId, ClaimRequestDto claimRequest)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var claim = new Claim
        {
            CarId = carId,
            ClaimDate = claimRequest.ClaimDate,
            Description = claimRequest.Description,
            Amount = claimRequest.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return new ClaimResponseDto(claim.Id, claim.ClaimDate, claim.Description, claim.Amount);
    }

    public async Task<CarHistoryDto> GetCarHistory(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var car = await _db.Cars
            .Include(c => c.Policies)
            .Include(c => c.Claims)
            .FirstAsync(c => c.Id == carId);

        var carHistory = new List<CarHistoryEntryDto>();

        carHistory.AddRange(car.Policies.Select(p => new CarHistoryEntryDto(
            p.Id,
            "Policy",
            p.StartDate,
            null,
            null,
            p.Provider,
            p.EndDate
        )));

        carHistory.AddRange(car.Claims.Select(c => new CarHistoryEntryDto(
            c.Id,
            "Claim",
            c.ClaimDate,
            c.Description,
            c.Amount,
            null,
            null
        )));

        carHistory = carHistory.OrderByDescending(h => h.Date).ToList();

        return new CarHistoryDto(car.Id, car.Vin, car.Model, car.YearOfManufacture, carHistory);

    }
}
