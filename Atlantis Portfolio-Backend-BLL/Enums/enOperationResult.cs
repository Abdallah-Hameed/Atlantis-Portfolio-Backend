namespace Atlantis_Portfolio_Backend_BLL;

public enum enOperationResult
{
    Success,
    AlreadyExist,
    InvalidData,
    InvalidCredentials,
    AccountInactive,
    AccountLocked,
    Failed,
    NotFound,
    InvalidSkillLevel,
    Conflict
}