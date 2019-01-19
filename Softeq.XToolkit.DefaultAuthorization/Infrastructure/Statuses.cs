namespace Softeq.XToolkit.DefaultAuthorization.Infrastructure
{
    public enum RegistrationStatus
    {
        Failed,
        Undefined,
        Successful,
        EmailAlreadyTaken
    }

    public enum ForgotPasswordStatus
    {
        Failed,
        Successful,
        UserNotFound,
        EmailNotConfirmed,
        Undefined
    }

    public enum LoginStatus
    {
        Failed,
        Successful,
        EmailOrPasswordIncorrect,
        EmailNotConfirmed,
        Undefined,
    }

    public enum ResendEmailStatus
    {
        Failed,
        Successful,
        EmailAlreadyConfirmed,
        UserNotFound,
        Undefined
    }
}