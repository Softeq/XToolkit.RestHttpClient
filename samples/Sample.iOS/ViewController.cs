using System;
using Sample.Core;
using Softeq.iOS.DefaultAuthorization;
using UIKit;

namespace Sample.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var testClass = new TestClass();
            await testClass.StartAsync(new SecuredTokenManager());
        }
    }
}
