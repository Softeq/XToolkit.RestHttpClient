using Android.App;
using Android.Widget;
using Android.OS;
using Softeq.Sample;
using Softeq.XToolkit.Droid.DefaultAuthorization;

namespace Sample.Droid
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { button.Text = $"{count++} clicks!"; };

            var test = new TestClass();
            await test.StartAsync(new SecuredTokenManager());
        }
    }
}