

using Android.OS;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;


namespace Alpha.Droid.Fragments
{
	public class TabFragment1 : MvxFragment
	{
		public override Android.Views.View OnCreateView ( Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState )
		{
			// ActionBar Tab Fragment: Retain (persist) this fragment, as the ActionBar tab is referencing it
			RetainInstance = true;

			// Call the base method
			base.OnCreateView ( inflater, container, savedInstanceState );

			// ActionBar Tab Fragment: MvvmCross binding, during the Inflate process
			return this.BindingInflate ( Resource.Layout.TabFrag1, null );
		}
	}
}

