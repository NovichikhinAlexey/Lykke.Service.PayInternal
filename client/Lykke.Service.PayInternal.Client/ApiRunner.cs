using System;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Exceptions;
using Refit;

namespace Lykke.Service.PayInternal.Client
{
    internal class ApiRunner
    {
        public async Task RunAsync<TError>(Func<Task> method,
            Func<TError, ApiException, ErrorResponseException<TError>> exceptionFactory)
        {
            try
            {
                await method();
            }
            catch (ApiException apiException)
            {
                throw exceptionFactory(GetErrorResponse<TError>(apiException), apiException);
            }
        }

        public async Task<TSuccess> RunAsync<TSuccess, TError>(Func<Task<TSuccess>> method,
            Func<TError, ApiException, ErrorResponseException<TError>> exceptionFactory)
        {
            try
            {
                return await method();
            }
            catch (ApiException apiException)
            {
                throw exceptionFactory(GetErrorResponse<TError>(apiException), apiException);
            }
        }

        public async Task RunWithDefaultErrorHandlingAsync(Func<Task> method)
        {
            await RunAsync(method, ExceptionFactories.CreateDefaultException);
        }

        public async Task<T> RunWithDefaultErrorHandlingAsync<T>(Func<Task<T>> method)
        {
            return await RunAsync(method, ExceptionFactories.CreateDefaultException);
        }

        private static T GetErrorResponse<T>(ApiException ex)
        {
            T errorResponse;

            try
            {
                errorResponse = ex.GetContentAs<T>();
            }
            catch (Exception)
            {
                errorResponse = default(T);
            }

            return errorResponse;
        }
    }
}
