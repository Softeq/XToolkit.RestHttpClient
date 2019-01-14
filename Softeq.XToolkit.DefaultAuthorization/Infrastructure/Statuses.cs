namespace Softeq.XToolkit.DefaultAuthorization.Infrastructure
{
    public enum RegistrationStatus
    {
        Undefined,
        Successful,
        Failed,
        EmailAlreadyTaken
    }

    public enum ForgotPasswordStatus
    {
        Successful,
        UserNotFound,
        EmailNotConfirmed,
        Failed,
        Undefined
    }

    public enum LoginStatus
    {
        Successful,
        EmailOrPasswordIncorrect,
        EmailNotConfirmed,
        Undefined,
        Failed
    }

    public enum ResendEmailStatus
    {
        Successful,
        Failed,
        UserNotFound,

        Undefined
    }
}