﻿namespace SurveyBasket.Api.Contracts.Requests;

public class CreatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static implicit operator Poll(CreatePollRequest request)
    {
        return new Poll()
        {
            Title = request.Title,
            Description = request.Description
        };
    }
}

