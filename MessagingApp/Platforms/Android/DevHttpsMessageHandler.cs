#if ANDROID
using Android.Net;
using Xamarin.Android.Net;

namespace MessagingApp.Platforms.Android;

public class DevHttpsMessageHandler : Xamarin.Android.Net.AndroidMessageHandler
{
    public DevHttpsMessageHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
#if DEBUG
            // Only allow this in development
            return true;
#else
            return errors == System.Net.Security.SslPolicyErrors.None;
#endif
        };
    }
}
#endif
