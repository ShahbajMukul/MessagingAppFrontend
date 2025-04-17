using MessagingApp.Shared.Models.Payload;
using MessagingApp.Shared.Models.Payloads;
using MessagingApp.Shared.Models.Results;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
namespace MessagingApp.Shared.Services
{
    public class MessagingService
    {
        public readonly HttpClient _httpClient;
        public MessagingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public void SetAuthHeader(string? token)
        {
#if DEBUG
            Console.WriteLine("Setting auth header");
#endif
            // Clear existing header first
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (string.IsNullOrEmpty(token) == false)
            {
                Console.WriteLine($"[{DateTime.Now}] Auth set");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("Failed to set token as Auth header. Token may be empty");
            }
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
        public async Task<ApiResponse<RegiLoginResult>> RegisterUserAsync(RegistrationPayload regiInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("register", regiInfo);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<RegiLoginResult>();
                var successResult = new ApiResponse<RegiLoginResult>
                {
                    Data = data,
                    StatusCode = response.StatusCode
                };
                return successResult;
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                var failureResult = new ApiResponse<RegiLoginResult>
                {
                    ErrorMessage = errorText,
                    StatusCode = response.StatusCode
                };
                return failureResult;
            }
        }

        public async Task<ApiResponse<RegiLoginResult>> LoginUserAsync(LoginPayload loginInfo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", loginInfo);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<RegiLoginResult>();
                    var successResult = new ApiResponse<RegiLoginResult>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    var failureResult = new ApiResponse<RegiLoginResult>
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
                return new ApiResponse<RegiLoginResult> { ErrorMessage = "Error processing request" };
            }
        }

        public async Task<ApiResponse<List<ContactsResult>>> GetContactsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("contacts");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<ContactsResult>>();
                    var successResult = new ApiResponse<List<ContactsResult>>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync() ?? "Error fetching contacts";
                    var failureResult = new ApiResponse<List<ContactsResult>>
                    {
                        StatusCode = response.StatusCode,
                        ErrorMessage = errorText,
                    };
                    return failureResult;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<List<ContactsResult>>
                { ErrorMessage = ex.Message };
            }
        }
        public async Task<ApiResponse<List<SearchResult>>> SearchUserAsync(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetAsync($"search?searchTerm={searchTerm}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<SearchResult>>();
                    var successResult = new ApiResponse<List<SearchResult>>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync() ?? "Error processing request";
                    var failureResult = new ApiResponse<List<SearchResult>>
                    {
                        ErrorMessage = errorText,
                        StatusCode = response.StatusCode
                    };
                    return failureResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<List<SearchResult>>
                {
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<OpenConversationResult>> OpenConversationAsync(int otherUserID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"open-conversation?otherUserID={otherUserID}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<OpenConversationResult>();
                    var successResult = new ApiResponse<OpenConversationResult>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else 
                {
                    var errorText = await response.Content.ReadAsStringAsync() ?? "Error opening conversation";
                    var failureResult = new ApiResponse<OpenConversationResult>
                    {
                        ErrorMessage = errorText,
                        StatusCode = response.StatusCode
                    };
                    return failureResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<OpenConversationResult>
                {
                    ErrorMessage = ex.Message
                };
            }
        }
       
        public async Task<ApiResponse<List<MessageResult>>> GetMessagesAsync(int conversationID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"messages?conversationID={conversationID}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<MessageResult>>();
                    var successResult = new ApiResponse<List<MessageResult>>
                    {
                        Data = data,
                        StatusCode = response.StatusCode,
                    };
                    return successResult;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    var failureResult = new ApiResponse<List<MessageResult>>
                    {
                        ErrorMessage = errorText,
                        StatusCode = response.StatusCode
                    };
                    return failureResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<List<MessageResult>>
                {
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<ApiResponse<bool>> SendMessageAsync(MessagePayload messagePayload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync<MessagePayload>("message", messagePayload);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        StatusCode = response.StatusCode,
                        Data = true
                    };
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync() ?? "Message could not be sent";
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = errorText,
                        StatusCode = response.StatusCode,
                        Data = false,
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse<bool>
                {
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                    Data = false
                };
            }
        }
    }
}
