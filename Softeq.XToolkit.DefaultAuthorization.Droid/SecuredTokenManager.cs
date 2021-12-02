using Softeq.XToolkit.DefaultAuthorization.Infrastructure;

namespace Softeq.XToolkit.DefaultAuthorization.Droid
{
    public sealed class SecuredTokenManager : SecuredTokenManagerBase
    {
        public SecuredTokenManager(ITokenChangeHandler tokenChangedHandler = null) : base(tokenChangedHandler) { }

        protected override string SavingResultExceptionMessage => "Something went wrong, please recheck project settings";
    }
}