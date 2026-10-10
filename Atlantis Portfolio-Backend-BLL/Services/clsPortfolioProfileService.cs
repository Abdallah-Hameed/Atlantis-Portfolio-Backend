using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsPortfolioProfileService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsPortfolioProfileService> _logger;

    public clsPortfolioProfileService(AppDbContext context, ILogger<clsPortfolioProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PortfolioProfile?> GetProfileAsync()
    {
        try
        {
            return await _context.PortfolioProfiles.AsNoTracking().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving portfolio profile.");
            throw;
        }
    }

    public async Task<PortfolioProfile?> GetProfileByIdAsync(int profileId)
    {
        if (profileId <= 0)
            return null;

        try
        {
            return await _context.PortfolioProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.PortfolioProfileId == profileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving portfolio profile with ID: {ProfileId}", profileId);
            throw;
        }
    }

    public async Task<(enOperationResult Result, int? ProfileId)> AddProfileAsync(string fullName, string professionalTitle, string bio, string? profileImageUrl, string? email, string? location)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(professionalTitle) || string.IsNullOrWhiteSpace(bio))
            return (enOperationResult.InvalidData, null);

        fullName = fullName.Trim();
        professionalTitle = professionalTitle.Trim();
        bio = bio.Trim();

        try
        {
            if (await _context.PortfolioProfiles.AnyAsync())
            {
                _logger.LogWarning("Attempted to add a second portfolio profile when one already exists.");
                return (enOperationResult.Conflict, null);
            }

            var profile = new PortfolioProfile
            {
                FullName = fullName,
                ProfessionalTitle = professionalTitle,
                Bio = bio,
                ProfileImageUrl = profileImageUrl?.Trim(),
                Email = email?.Trim(),
                Location = location?.Trim()
            };

            await _context.PortfolioProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio profile created successfully with ID: {ProfileId}", profile.PortfolioProfileId);

            return (enOperationResult.Success, profile.PortfolioProfileId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while adding portfolio profile.");
            return (enOperationResult.Failed, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding portfolio profile.");
            return (enOperationResult.Failed, null);
        }
    }

    public async Task<enOperationResult> UpdateProfileAsync(int profileId, string fullName, string professionalTitle, string bio, string? profileImageUrl, string? email, string? location)
    {
        if (profileId <= 0 || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(professionalTitle) || string.IsNullOrWhiteSpace(bio))
            return enOperationResult.InvalidData;

        fullName = fullName.Trim();
        professionalTitle = professionalTitle.Trim();
        bio = bio.Trim();

        try
        {
            var profile = await _context.PortfolioProfiles.FirstOrDefaultAsync(p => p.PortfolioProfileId == profileId);

            if (profile == null)
            {
                _logger.LogWarning("Portfolio profile update failed because profile ID: {ProfileId} was not found.", profileId);
                return enOperationResult.NotFound;
            }

            profile.FullName = fullName;
            profile.ProfessionalTitle = professionalTitle;
            profile.Bio = bio;
            profile.ProfileImageUrl = profileImageUrl?.Trim();
            profile.Email = email?.Trim();
            profile.Location = location?.Trim();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio profile with ID: {ProfileId} updated successfully.", profileId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating portfolio profile with ID: {ProfileId}.", profileId);
            return enOperationResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating portfolio profile with ID: {ProfileId}.", profileId);
            return enOperationResult.Failed;
        }
    }
}