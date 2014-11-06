

// http://arvid-g.de/12/android-4-actionbar-with-tabs-example


using ActionBar = Android.Support.V7.App.ActionBar;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

using Alpha.Droid.Activities;


namespace Alpha.Droid.Specifics
{
	public class TabListenerImpl : Java.Lang.Object, ActionBar.ITabListener
	{
		public MainActivity Owner;

		public TabListenerImpl(  MainActivity owner)
		{
			Owner = owner;
		}

		public void OnTabReselected ( ActionBar.Tab tab, FragmentTransaction ft ) {
		}

		public void OnTabSelected ( ActionBar.Tab tab, FragmentTransaction ft ) {
			switch (tab.Position)
			{
				case 0: if ( Owner.Fragment1 != null ) ft.Show(Owner.Fragment1); break;
			}
		}

		public void OnTabUnselected ( ActionBar.Tab tab, FragmentTransaction ft ) {
			switch ( tab.Position ) {
				case 0: ft.Hide(Owner.Fragment1); break;
			}
		}
	}
}

