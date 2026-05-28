using System.Reflection;
using FluentValidation;
using MediatR;
using Tibar.Application.Common;

namespace Tibar.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        if (typeof(TResponse).IsGenericType
            && typeof(Result).IsAssignableFrom(typeof(TResponse)))
        {
            return CreateFailureResult(failures);
        }

        throw new ValidationException(failures);
    }

    private static TResponse CreateFailureResult(List<FluentValidation.Results.ValidationFailure> failures)
    {
        var dataType = typeof(TResponse).GetGenericArguments()[0];
        var method = typeof(Result)
            .GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static, [typeof(string[])])!;
        var genericMethod = method.MakeGenericMethod(dataType);

        var errors = failures.Select(f => f.ErrorMessage).ToArray();
        return (TResponse)genericMethod.Invoke(null, [errors])!;
    }
}
