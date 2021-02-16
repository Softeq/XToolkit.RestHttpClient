namespace Softeq.XToolkit.DefaultAuthorization.Droid
{
    public sealed class SecuredTokenManager : SecuredTokenManagerBase
    {
        public SecuredTokenManager() : base() { }

        protected override string SavingResultExceptionMessage => "Something went wrong, please recheck project settings";
    }
}