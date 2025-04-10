using Application.Services.Notification;
namespace API.Services
{
    public class FAMAcceptanceGDSMSNotificationJob : BackgroundService
    {
        private readonly ILogger<FAMAcceptanceGDSMSNotificationJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public FAMAcceptanceGDSMSNotificationJob(ILogger<FAMAcceptanceGDSMSNotificationJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FAMAcceptanceGDNotificationJob başlatıldı");

            var nextRunTime = CalculateNextRunTime(18, 0);
            var delay = nextRunTime - DateTime.Now;

            if (delay.TotalMilliseconds <= 0)
            {
                nextRunTime = nextRunTime.AddDays(1);
                delay = nextRunTime - DateTime.Now;
            }

            _timer = new Timer(DoWork, null, delay, TimeSpan.FromDays(1));

            await Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation($"DailyTaskService çalışıyor: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                SendFAMAcceptanceNotifications().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAMAcceptanceGDNotificationJob işlemi sırasında hata oluştu");
            }
        }

        private async Task SendFAMAcceptanceNotifications()
        {
            _logger.LogInformation("FAMAcceptanceNotification servisi çağrılıyor...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var famAcceptanceNotification = scope.ServiceProvider.GetRequiredService<FAMAcceptanceGDSMSNotification>();
                await famAcceptanceNotification.InvokeAsync(new FAMAcceptanceGDSMSNotification.Request());

                _logger.LogInformation("FAMAcceptanceNotification servisi başarıyla tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAMAcceptanceNotification servisi çağrılırken hata oluştu");
                throw;
            }
        }

        private DateTime CalculateNextRunTime(int hour, int minute)
        {
            var now = DateTime.Now;
            var nextRunTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            return nextRunTime;
        }
    }
}