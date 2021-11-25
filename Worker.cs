using AutoRefreshRateChangerWorker.Helpers;
using AutoRefreshRateChangerWorker.Services;

namespace AutoRefreshRateChangerWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RefreshRateService refreshRateService;
        private readonly PowerManagementService powerManagementService;

        public Worker(ILogger<Worker> logger, RefreshRateService refreshRateService, PowerManagementService powerManagementService)
        {
            _logger = logger;
            this.refreshRateService = refreshRateService;
            this.powerManagementService = powerManagementService;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await base.StartAsync(stoppingToken);

            try
            {
                //We fetch the current display mode
                refreshRateService.PopulateDeviceInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "An error occured while starting the service");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Keeping the worker running
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string Plan = powerManagementService.GetActiveSchemeName();

                    if (!string.IsNullOrEmpty(Plan))
                    {
                        PowerPlanLevel planLevel = powerManagementService.GetPowerPlanLevel(Plan);

                        if (!planLevel.Equals(powerManagementService.ActivePlan))
                        {
                            if (planLevel == PowerPlanLevel.Low)
                            {
                                refreshRateService.SwitchTo60();

                                _logger.LogWarning("Display mode switched to {frequency}HZ", refreshRateService.DisplayMode.Frequency);
                            }
                            else if (planLevel == PowerPlanLevel.High)
                            {
                                refreshRateService.SwitchTo144();

                                _logger.LogWarning("Display mode switched to {frequency}HZ", refreshRateService.DisplayMode.Frequency);
                            }

                            powerManagementService.ActivePlan = planLevel;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, "Operation failed!");
                }

                _logger.LogInformation("Refresh rate worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(2000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);

            try
            {
                //Here we detach from the "SystemEvents.PowerModeChanged" event.

            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "An error occured while stopping the service");
            }
        }
    }
}