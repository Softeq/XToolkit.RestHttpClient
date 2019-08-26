using Android.App;
using Android.OS;
using Sample.Core;
using Softeq.XToolkit.DefaultAuthorization.Droid;

namespace Sample.Droid
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);


            var test = new TestClass();
            await test.StartAsync(new SecuredTokenManager());
        }
    }
}