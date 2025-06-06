using Microsoft.EntityFrameworkCore;

namespace CloakVision.Interface;

/// <summary>
/// Common helpers
/// </summary>
public interface ICommonHelpers
{
    /// <summary>
    /// Check if the error is a duplicate key error
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    bool IsDuplicateKeyError(DbUpdateException ex);

    /// <summary>
    /// Check if the error is a foreign key error
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsForeignKeyError(DbUpdateException ex);
}
