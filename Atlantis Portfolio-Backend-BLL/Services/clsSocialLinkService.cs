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

    public async Task<(enOperationResult Result, int SocialLinkId)> AddSocialLinkAsync(string platform, string url)
    {
        if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(url))
            return (enOperationResult.InvalidData, 0);

        platform = platform.Trim();
        url = url.Trim();

        try
        {
            // Calculate DisplayOrder = max + 1
            short nextOrder = 1;
            if (await _context.SocialLinks.AnyAsync())
            {
                var maxOrder = await _context.SocialLinks.MaxAsync(l => (short?)l.DisplayOrder) ?? 0;
                nextOrder = (short)(maxOrder + 1);
            }

            var socialLink = new SocialLink
            {
                Platform = platform,
                Url = url,
                DisplayOrder = nextOrder
            };

            await _context.SocialLinks.AddAsync(socialLink);
            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Social link added successfully. SocialLinkId: {SocialLinkId}", socialLink.SocialLinkId);
                return (enOperationResult.Success, socialLink.SocialLinkId);
            }

            _logger.LogWarning("Adding social link failed.");
            return (enOperationResult.Failed, 0);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Cannot add social link because a unique constraint was violated. Platform: {Platform}, Url: {Url}", platform, url);
            return (enOperationResult.AlreadyExist, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding social link.");
            return (enOperationResult.Failed, 0);
        }
    }

    // DisplayOrder is intentionally NOT updated here — it's managed by ReorderSocialLinksAsync.
    public async Task<enOperationResult> UpdateSocialLinkAsync(int socialLinkId, string platform, string url)
    {
        if (socialLinkId <= 0 || !IsValidSocialLinkData(platform, url))
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

            socialLink.Platform = platform;
            socialLink.Url = url;
            // socialLink.DisplayOrder intentionally left unchanged

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

    private static bool IsValidSocialLinkData(string platform, string url)
    {
        if (string.IsNullOrWhiteSpace(platform) || platform.Trim().Length > 100)
            return false;

        if (string.IsNullOrWhiteSpace(url) || url.Trim().Length > 500)
            return false;

        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }

    public async Task<enOperationResult> ReorderSocialLinksAsync(List<(int SocialLinkId, short DisplayOrder)> items)
    {
        if (items == null || items.Count == 0)
            return enOperationResult.InvalidData;

        if (items.Any(x => x.SocialLinkId <= 0 || x.DisplayOrder < 1))
            return enOperationResult.InvalidData;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var ids = items.Select(x => x.SocialLinkId).ToList();
            var links = await _context.SocialLinks
                .Where(l => ids.Contains(l.SocialLinkId))
                .ToListAsync();

            if (links.Count != ids.Count)
                return enOperationResult.NotFound;

            // Phase 1: Offset to safe temporary values
            foreach (var link in links)
            {
                link.DisplayOrder = (short)(link.DisplayOrder + 10000);
            }
            await _context.SaveChangesAsync();

            // Phase 2: Set final values
            var dict = items.ToDictionary(x => x.SocialLinkId, x => x.DisplayOrder);
            foreach (var link in links)
            {
                link.DisplayOrder = dict[link.SocialLinkId];
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Social links reordered successfully. Count: {Count}", items.Count);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Social link reorder failed due to a unique constraint violation.");

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reordering social links.");

            return enOperationResult.Failed;
        }
    }
}