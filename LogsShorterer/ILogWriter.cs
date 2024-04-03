using LogsShorterer.Entities;

namespace LogsShorterer
{
    public interface ILogWriter
    {
        SplitResult PrintLogEntity(LogEntity logEntity, int maxLength);
    }
}