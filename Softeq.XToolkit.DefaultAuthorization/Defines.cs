namespace Softeq.XToolkit.DefaultAuthorization
{
    public static class Defines
    {
        public static class Api
        {
            public static class Account
            {
                public const string ApiRoot = "account";

                public static class Register
                {
                    public const string ApiRoot = "register";
                }

                public static class ForgotPassword
                {
                    public const string ApiRoot = "forgot-password";
                }

                public static class ResendConfirmationEmail
                {
                    public const string ApiRoot = "resend-confirmation-email";
                }
                
                public static class IsAccountFreeToUse
                {
                    public const string ApiRoot = "check-registration";
                }
            }

            public static class Connect
            {
                public const string ApiRoot = "connect";

                public static class Token
                {
                    public const string ApiRoot = "token";
                }
            }
        }
    }
}