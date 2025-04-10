using Application.Services.Mail;

namespace API.Services
{
    public class FAMAcceptanceBrokerEmailNotificationJob : BackgroundService
    {
        private readonly ILogger<FAMAcceptanceBrokerEmailNotificationJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public FAMAcceptanceBrokerEmailNotificationJob(ILogger<FAMAcceptanceBrokerEmailNotificationJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyTaskService başlatıldı");

            var nextRunTime = CalculateNextRunTime(18, 0);
            var delay = nextRunTime - DateTime.Now;
            
            if (delay.TotalMilliseconds <= 0)
            {
                nextRunTime = nextRunTime.AddDays(1);
                delay = nextRunTime - DateTime.Now;
            }

            _logger.LogInformation($"İlk çalışma zamanı: {nextRunTime:yyyy-MM-dd HH:mm:ss}");

            _timer = new Timer(DoWork, null, delay, TimeSpan.FromDays(1));

            await Task.CompletedTask;
        }

        private DateTime CalculateNextRunTime(int hour, int minute)
        {
            var now = DateTime.Now;
            var nextRunTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            
            if (now > nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }
            
            return nextRunTime;
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
                _logger.LogError(ex, "DailyTaskService işlemi sırasında hata oluştu");
            }
        }

        private async Task SendFAMAcceptanceNotifications()
        {
            _logger.LogInformation("FAMAcceptanceNotification servisi çağrılıyor...");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var famAcceptanceNotification = scope.ServiceProvider.GetRequiredService<FAMAcceptanceBrokerEmailNotification>();
                await famAcceptanceNotification.InvokeAsync(new FAMAcceptanceBrokerEmailNotification.Request());
                
                _logger.LogInformation("FAMAcceptanceNotification servisi başarıyla tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAMAcceptanceNotification servisi çağrılırken hata oluştu");
                throw;
            }
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
} 