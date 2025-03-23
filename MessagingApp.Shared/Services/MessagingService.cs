using MessagingApp.Shared.Models.Payload;
using MessagingApp.Shared.Models.Payloads;
using MessagingApp.Shared.Models.Results;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MessagingApp.Shared.Services
{
    public class MessagingService
    {
        public readonly HttpClient _httpClient;
        public MessagingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<string> TestAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("test");
                Console.WriteLine(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return "";
        }
        public async Task<ApiResponse<RegistrationResult>> RegisterUserAsync(RegistrationPayload regiInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("register", regiInfo);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<RegistrationResult>();
                var successResult = new ApiResponse<RegistrationResult>
                {
                    Data = data,
                    StatusCode = response.StatusCode
                };
                return successResult;
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                var failureResult = new ApiResponse<RegistrationResult>
                {
                    ErrorMessage = errorText,
                    StatusCode = response.StatusCode
                };
                return failureResult;
            }
        }

        public async Task<ApiResponse<LoginResult>> LoginUserAsync(LoginPayload loginInfo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", loginInfo);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResult>();
                    var successResult = new ApiResponse<LoginResult>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    var failureResult = new ApiResponse<LoginResult>
                    {
                        ErrorMessage = errorText ?? "Error processing request",
                        StatusCode = response.StatusCode
                    };
                    return failureResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<LoginResult> { ErrorMessage = "Error processing request" };
            }
        }

    }
}
