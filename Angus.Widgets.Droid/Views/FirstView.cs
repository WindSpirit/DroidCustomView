using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;

namespace Angus.Widgets.Droid.Views
{
    [Activity(Label = "View for FirstViewModel", MainLauncher = true)]
    public class FirstView : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FirstView);
        }
    }
}