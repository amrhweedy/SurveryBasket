namespace SurveyBasket.Api.Services.BackgroundJobs;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
