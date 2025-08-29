using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace CarInsurance.Api.Tests.ServicesTests
{
    public class CarServiceFacts : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly CarService _service;
        public CarServiceFacts()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _db = new AppDbContext(options);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();
            SeedData.EnsureSeeded(_db);

            _service = new CarService(_db);
        }

        // Tests for RegisterClaim method

        [Fact]
        public async Task RegisterClaim_ShouldRegisterClaim_WhenCarExists()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            var result = await _service.RegisterClaim(carId, claimRequest);

            Assert.NotNull(result);
            Assert.Equal(claimRequest.ClaimDate, result.ClaimDate);
            Assert.Equal(claimRequest.Amount, result.Amount);
            Assert.Equal(claimRequest.Description, result.Description);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task RegisterClaim_ShouldThrowKeyNotFoundException_WhenCarNotExist()
        {
            var carId = 50;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.RegisterClaim(carId, claimRequest));
        }

        [Fact]
        public async Task RegisterClaim_ShouldThrowArgumentException_WhenDateIsInFuture()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2500, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.RegisterClaim(carId, claimRequest));

            Assert.Equal("Claim date cannot be in the future", ex.Message);
        }

        // Tests for GetCarHistory method

        [Fact]
        public async Task GetCarHistory_ShouldReturnHistory_WhenCarExists()
        {
            var carId = 1;
            var result = await _service.GetCarHistory(carId);

            Assert.NotNull(result);
            Assert.Equal(carId, result.CarId);
            Assert.NotNull(result.Vin);
            Assert.NotEmpty(result.History);
        }

        [Fact]
        public async Task GetCarHistory_ShouldThrowKeyNotFoundException_WhenCarNotExist()
        {
            var carId = 100;

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.GetCarHistory(carId));
        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }
}
