global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq.Dynamic.Core;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json;
global using Asp.Versioning;
global using FluentValidation;
global using FluentValidation.AspNetCore;
global using MailKit.Security;
global using Mapster;
global using MapsterMapper;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using SurveyBasket.Api;
global using SurveyBasket.Api.Abstractions;
global using SurveyBasket.Api.Abstractions.Consts;
global using SurveyBasket.Api.Authentication;
global using SurveyBasket.Api.Authentication.Filters;
global using SurveyBasket.Api.Contracts.Answers;
global using SurveyBasket.Api.Contracts.Authentication;
global using SurveyBasket.Api.Contracts.Common;
global using SurveyBasket.Api.Contracts.Polls;
global using SurveyBasket.Api.Contracts.Questions;
global using SurveyBasket.Api.Contracts.Results;
global using SurveyBasket.Api.Contracts.Roles;
global using SurveyBasket.Api.Contracts.Students;
global using SurveyBasket.Api.Contracts.Users;
global using SurveyBasket.Api.Contracts.Votes;
global using SurveyBasket.Api.Entities;
global using SurveyBasket.Api.Errors;
global using SurveyBasket.Api.Extensions;
global using SurveyBasket.Api.Helpers;
global using SurveyBasket.Api.Persistence;
global using SurveyBasket.Api.Services.Authentication;
global using SurveyBasket.Api.Services.Cashing;
global using SurveyBasket.Api.Services.Polls;
global using SurveyBasket.Api.Services.Questions;
global using SurveyBasket.Api.Services.Results;
global using SurveyBasket.Api.Services.Roles;
global using SurveyBasket.Api.Services.Users;
global using SurveyBasket.Api.Services.Votes;
global using SurveyBasket.Api.Settings;
global using Microsoft.AspNetCore.Identity.UI.Services;
















