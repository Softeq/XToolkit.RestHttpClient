using Softeq.XToolkit.DefaultAuthorization.Infrastructure;

namespace Softeq.XToolkit.DefaultAuthorization.iOS
{
    public sealed class SecuredTokenManager : SecuredTokenManagerBase
    {
        public SecuredTokenManager(ITokenChangeHandler tokenChangedHandler = null) : base(tokenChangedHandler) { }

        protected override string SavingResultExceptionMessage => "Please check iOS settings " +
            "by the following link: https://github.com/sameerkapps/SecureStorage/issues/31#issuecomment-366205742";
    }
}