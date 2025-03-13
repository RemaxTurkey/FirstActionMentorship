using Microsoft.Extensions.Logging;

namespace Application.Extensions
{
    public static class LoggingExtension
    {
        /// <summary>
        /// Uygulama için logging yapılandırmasını ayarlar
        /// </summary>
        /// <param name="logging">Logging builder</param>
        /// <param name="minimumLevel">Minimum log seviyesi (varsayılan: Debug)</param>
        /// <returns>ILoggingBuilder</returns>
        public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder logging, LogLevel minimumLevel = LogLevel.Debug)
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(minimumLevel);
            
            return logging;
        }
        
        /// <summary>
        /// Entity Framework Core loglarını yapılandırır
        /// </summary>
        /// <param name="logging">Logging builder</param>
        /// <param name="showSqlQueries">SQL sorgularını göster/gizle</param>
        /// <param name="sqlLogLevel">SQL sorguları için log seviyesi</param>
        /// <returns>ILoggingBuilder</returns>
        public static ILoggingBuilder ConfigureEntityFrameworkLogging(this ILoggingBuilder logging, 
            bool showSqlQueries = true, 
            LogLevel sqlLogLevel = LogLevel.Debug)
        {
            if (showSqlQueries)
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", sqlLogLevel);
            }
            else
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
            }
            
            logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
            
            return logging;
        }
    }
}