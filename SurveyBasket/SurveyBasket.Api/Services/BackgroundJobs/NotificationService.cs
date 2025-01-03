
using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;

namespace SurveyBasket.Api.Services.BackgroundJobs;

public class NotificationService(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollsNotification(int? pollId = null)
    {
        // get polls from db
        IEnumerable<Poll> polls = [];
        if (pollId.HasValue)
        {
            var poll = await _context.Polls.SingleOrDefaultAsync(p => p.Id == pollId && p.IsPublished);
            polls = [poll!];
        }
        else
        {
            polls = await _context.Polls
                  .Where(p => p.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow) && p.IsPublished)
                  .AsTracking()
                  .ToListAsync();
        }

        // get users to send email to them
        // TODO => select members only

        var users = await _userManager.Users.ToListAsync();

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var poll in polls)
        {
            foreach (var user in users)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "{{name}}", user.FirstName },
                    { "{{pollTill}}", poll.Title },
                    { "{{endDate}}", poll.EndsAt.ToString() },
                    { "{{url}}", $"{origin}/polls/starts/{poll.Id}" },
                };

                var emailBody = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeholders);

                await _emailSender.SendEmailAsync(user.Email!, $"🎉 Survey Basket : New Poll _ {poll.Title}", emailBody);
            }
        }
    }
}
