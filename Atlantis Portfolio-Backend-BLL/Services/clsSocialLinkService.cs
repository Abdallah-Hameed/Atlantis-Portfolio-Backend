using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsSocialLinkService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsSocialLinkService> _logger;

    public clsSocialLinkService(AppDbContext context, ILogger<clsSocialLinkService> logger)
    {
        _context = context;

        _logger = logger;
    }

    public async Task<List<SocialLink>> GetAllSocialLinksAsync()
    {
        return await _context.SocialLinks.OrderBy(s => s.DisplayOrder).ToListAsync();
    }

    public async Task<SocialLink?> GetSocialLinkByIdAsync(int socialLinkId)
    {
        if (socialLinkId <= 0)
            return null;

        return await _context.SocialLinks.FindAsync(socialLinkId);
    }

    public async Task<enOperationResult> AddSocialLinkAsync(string platform, string url, short displayOrder)
    {
        if (!IsValidSocialLinkData(platform, url, displayOrder))
            return enOperationResult.InvalidData;

        platform = platform.Trim();

        url = url.Trim();

        try
        {
            if (await _context.SocialLinks.AnyAsync(s => s.Url == url))
                return enOperationResult.AlreadyExist;

            if (await _context.SocialLinks.AnyAsync(s => s.DisplayOrder == displayOrder))
                return enOperationResult.Conflict;

            var socialLink = new SocialLink
            {
                Platform = platform,

                Url = url,

                DisplayOrder = displayOrder
            };

            await _context.SocialLinks.AddAsync(socialLink);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Social link added successfully. SocialLinkId: {SocialLinkId}", socialLink.SocialLinkId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Social link creation conflicted with a unique constraint.");

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding a social link.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateSocialLinkAsync(int socialLinkId, string platform, string url, short displayOrder)
    {
        if (socialLinkId <= 0 || !IsValidSocialLinkData(platform, url, displayOrder))
            return enOperationResult.InvalidData;

        platform = platform.Trim();

        url = url.Trim();

        try
        {
            var socialLink = await _context.SocialLinks.FindAsync(socialLinkId);

            if (socialLink == null)
                return enOperationResult.NotFound;

            if (await _context.SocialLinks.AnyAsync(s => s.Url == url && s.SocialLinkId != socialLinkId))
                return enOperationResult.AlreadyExist;

            if (await _context.SocialLinks.AnyAsync(s => s.DisplayOrder == displayOrder && s.SocialLinkId != socialLinkId))
                return enOperationResult.Conflict;

            socialLink.Platform = platform;

            socialLink.Url = url;

            socialLink.DisplayOrder = displayOrder;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Social link updated successfully. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Social link update conflicted with a unique constraint. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating a social link. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteSocialLinkAsync(int socialLinkId)
    {
        if (socialLinkId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var socialLink = await _context.SocialLinks.FindAsync(socialLinkId);

            if (socialLink == null)
                return enOperationResult.NotFound;

            _context.SocialLinks.Remove(socialLink);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Social link deleted successfully. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning(ex, "Social link deletion was blocked by a foreign key constraint. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting a social link. SocialLinkId: {SocialLinkId}", socialLinkId);

            return enOperationResult.Failed;
        }
    }

    private static bool IsValidSocialLinkData(string platform, string url, short displayOrder)
    {
        if (string.IsNullOrWhiteSpace(platform) || platform.Trim().Length > 100)
            return false;

        if (string.IsNullOrWhiteSpace(url) || url.Trim().Length > 500)
            return false;

        if (displayOrder <= 0)
            return false;

        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}