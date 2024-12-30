﻿namespace SurveyBasket.Api.Services.Cashing;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class;
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}