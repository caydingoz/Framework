using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Framework.EF.Interceptors
{
    public class SlowQueryInterceptor : DbCommandInterceptor
    {
        private const int _slowQueryThreshold = 400; // milliseconds
        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Duration.TotalMilliseconds > _slowQueryThreshold)
            {
                Console.WriteLine($"Slow query ({eventData.Duration.TotalMilliseconds} ms): {command.CommandText}");
            }
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}
