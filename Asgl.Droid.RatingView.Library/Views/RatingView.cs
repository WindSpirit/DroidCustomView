

using System;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Asgl.Droid.RatingView.Library;


namespace Asgl.Android.Views {

	public class RatingView : TableLayout, View.IOnClickListener {

		// Rating:
		// ====================================================
		//   -2 = Not applicable
		//   -1 = Not set
		//    0 = False (thumbs down / fail);
		//    1 = True (thumbs up / success); or, Rating of 1
		//    2..5 = Rating of 2..5

		private const int InvalidLowValue = -3;
		private const int MinimumRating = 0;
		private const int TwoPointScale = 2;

		protected View GroupView;

		protected ImageView Rating0 = null;
		protected ImageView Rating1 = null;
		protected ImageView Rating2 = null;
		protected ImageView Rating3 = null;
		protected ImageView Rating4 = null;

		protected TextView NotApplicable = null;
		protected ImageView CorrectionNeeded = null;

		protected Drawable ActnGood;
		protected Drawable ActnBad;
		protected Drawable ActnImportant;
		protected Drawable ActnNotImportant;

		protected Color ColourDisabled;
		protected Color ColourGood;
		protected Color ColourBad;
		protected Color ColourImportant;
		protected Color ColourNotImportant;

		private bool _isUpdating = false;

		protected RatingView ( IntPtr javaReference, JniHandleOwnership transfer )
			: base ( javaReference, transfer ) {
			OnCreateView ( );
		}

		public RatingView ( Context context )
			: base ( context ) {
			OnCreateView ( );
		}

		public RatingView ( Context context, IAttributeSet attrs )
			: base ( context, attrs ) {

			OnCreateView ( );

			var attrArray = Context.ObtainStyledAttributes ( attrs, Resource.Styleable.RatingView );

			var tmpScale = attrArray.GetInteger ( Resource.Styleable.RatingView_scale, InvalidLowValue );
			if ( tmpScale > InvalidLowValue ) Scale = tmpScale;

			var tmpRating = attrArray.GetInteger ( Resource.Styleable.RatingView_rating, InvalidLowValue );
			if ( tmpRating > InvalidLowValue ) Rating = tmpRating;

			ReadOnly = attrArray.GetBoolean ( Resource.Styleable.RatingView_readOnly, true );
			RequiresCorrection = attrArray.GetBoolean ( Resource.Styleable.RatingView_requiresCorrection, false );

			attrArray.Recycle ( );
		}

		public void OnCreateView ( ) {

			GroupView = Inflate ( Context, Resource.Layout.angus_rating_view, this );

			// AlwaysDrawnWithCacheEnabled = false; // Uncomment this if you want Draw events

			InitColour ( );
			InitDrawables ( );
			InitControls ( GroupView );

			SetClickListener(this);
		}

		private int? _rating = null;
		public int? Rating {
			get
			{
				return _rating;
			}
			set {
				if ((value != null) && (value < -2 || value > 5)) throw new ArgumentOutOfRangeException();
				if (_rating != value)
				{
					_rating = value;
					_isUpdating = true;
					try
					{
						Refresh();
						if (RatingChanged != null)
							RatingChanged(this, EventArgs.Empty);
					}
					finally
					{
						_isUpdating = false;
					}
				}
			}
		}
		public event EventHandler RatingChanged;

		private int? _scale = null;
		public int? Scale {
			get { return _scale; }
			set {
				if ((value != null) && ( value < 2 || value > 5 )) throw new ArgumentOutOfRangeException ( );
				if (_scale != value)
				{
					_scale = value;
					_isUpdating = true;
					try {
						Refresh ( );
						if ( ScaleChanged != null )
							ScaleChanged ( this, EventArgs.Empty );
					} finally { _isUpdating = false; }
				}
			}
		}
		public event EventHandler ScaleChanged;

		public bool ReadOnly { get; set; }

		private bool _requiresCorrection;
		public bool RequiresCorrection
		{
			get { return _requiresCorrection; }
			set
			{
				if ( _requiresCorrection != value )
				{
					_requiresCorrection = value;
					if (RequiresCorrectionChanged != null)
						RequiresCorrectionChanged(this, EventArgs.Empty);
				}
			}
		}
		public event EventHandler RequiresCorrectionChanged;

		#region -[ Init ]-

		protected void InitColour()
		{
			ColourDisabled = Resources.GetColor ( Resource.Color.colourDisabled );
			ColourGood = Resources.GetColor ( Resource.Color.colourGood );
			ColourBad = Resources.GetColor ( Resource.Color.colourBad );
			ColourImportant = Resources.GetColor ( Resource.Color.colourImportant );
			ColourNotImportant = Resources.GetColor ( Resource.Color.colourNotImportant );
		}

		protected void InitDrawables ( ) {
			ActnGood = Resources.GetDrawable ( Resource.Drawable.ic_action_good );
			ActnBad = Resources.GetDrawable ( Resource.Drawable.ic_action_bad );
			ActnImportant = Resources.GetDrawable ( Resource.Drawable.ic_action_important );
			ActnNotImportant = Resources.GetDrawable ( Resource.Drawable.ic_action_not_important );
		}

		protected void SetClickListener ( View.IOnClickListener listner ) {
			// Passing in null will clear listners
			Rating0.SetOnClickListener ( listner );
			Rating1.SetOnClickListener ( listner );
			Rating2.SetOnClickListener ( listner );
			Rating3.SetOnClickListener ( listner );
			Rating4.SetOnClickListener ( listner );
			NotApplicable.SetOnClickListener ( listner );
			CorrectionNeeded.SetOnClickListener ( listner );
		}

		protected void InitControls ( View rootView ) {
			Rating0 = rootView.FindViewById<ImageView> ( Resource.Id.rating0 );
			Rating1 = rootView.FindViewById<ImageView> ( Resource.Id.rating1 );
			Rating2 = rootView.FindViewById<ImageView> ( Resource.Id.rating2 );
			Rating3 = rootView.FindViewById<ImageView> ( Resource.Id.rating3 );
			Rating4 = rootView.FindViewById<ImageView> ( Resource.Id.rating4 );
			NotApplicable = rootView.FindViewById<TextView> ( Resource.Id.not_applicable );
			CorrectionNeeded = rootView.FindViewById<ImageView> ( Resource.Id.req_correction );
		}

		protected void SetVisibility ( )
		{
			var testValue = Scale ?? 0;
			Rating0.Visibility = (testValue > 0) ? ViewStates.Visible : ViewStates.Gone;
			Rating1.Visibility = (testValue > 0) ? ViewStates.Visible : ViewStates.Gone;
			Rating2.Visibility = (testValue > 2) ? ViewStates.Visible : ViewStates.Gone;
			Rating3.Visibility = (testValue > 3) ? ViewStates.Visible : ViewStates.Gone;
			Rating4.Visibility = (testValue > 4) ? ViewStates.Visible : ViewStates.Gone;
		}

		#endregion

		protected override bool DrawChild(Canvas canvas, View child, long drawingTime)
		{
			// Not sure where the "child" came from, but we need to update it
			// to reflect the information and states in "this" View instance.

			// Establish references to the Views that we want to update
			InitControls(child);

			// Update those Views
			Refresh();

			// Let DrawChild do its thing now that we have sync'd the GroupView's
			return base.DrawChild(canvas, child, drawingTime);
		}

		protected void Refresh ( )
		{
			ClearImages();
			SetVisibility ( );

			// If you don't know the scale, you don't know what to render
			if (Scale != null)
			{
				if (Rating < MinimumRating)
				{
					// Rating has not been established yet
					if (Scale == TwoPointScale)
					{
						ActnGood.SetColorFilter(ColourDisabled, PorterDuff.Mode.SrcIn);
						ActnBad.SetColorFilter(ColourDisabled, PorterDuff.Mode.SrcIn);
						Rating0.SetImageDrawable(ActnGood);
						Rating1.SetImageDrawable(ActnBad);
					}
					else
					{
						ActnNotImportant.SetColorFilter(ColourDisabled, PorterDuff.Mode.SrcIn);
						Rating0.SetImageDrawable(ActnNotImportant);
						Rating1.SetImageDrawable(ActnNotImportant);
						Rating2.SetImageDrawable(ActnNotImportant);
						Rating3.SetImageDrawable(ActnNotImportant);
						Rating4.SetImageDrawable(ActnNotImportant);
					}
				}
				else
				{
					if (Scale == TwoPointScale)
					{
						ActnGood.SetColorFilter((Rating > 0) ? ColourGood : ColourDisabled, PorterDuff.Mode.SrcIn);
						Rating0.SetImageDrawable(ActnGood);

						ActnBad.SetColorFilter((Rating > 0) ? ColourDisabled : ColourBad, PorterDuff.Mode.SrcIn);
						Rating1.SetImageDrawable(ActnBad);
					}
					else
					{
						ActnNotImportant.SetColorFilter(ColourNotImportant, PorterDuff.Mode.SrcIn);
						ActnImportant.SetColorFilter(ColourImportant, PorterDuff.Mode.SrcIn);

						Rating0.SetImageDrawable(ActnNotImportant);
						Rating1.SetImageDrawable(ActnNotImportant);
						Rating2.SetImageDrawable(ActnNotImportant);
						Rating3.SetImageDrawable(ActnNotImportant);
						Rating4.SetImageDrawable(ActnNotImportant);

						if (Rating > 4) Rating4.SetImageDrawable(ActnImportant);
						if (Rating > 3) Rating3.SetImageDrawable(ActnImportant);
						if (Rating > 2) Rating2.SetImageDrawable(ActnImportant);
						if (Rating > 1) Rating1.SetImageDrawable(ActnImportant);
						if (Rating > 0) Rating0.SetImageDrawable(ActnImportant);
					}
				}
			}
		}

		public void OnClick(View v)
		{
			// TODO: CAZ - Implement for production..
			// if (! ReadOnly)
			// {
			if (v == Rating0) { Rating = 1; }
			else if (v == Rating1) { Rating = (Scale == 2) ? 0 : 2; }
			else if (v == Rating2) { Rating = 3; }
			else if (v == Rating3) { Rating = 4; }
			else if (v == Rating4) { Rating = 5; }
			else if (v == NotApplicable) { Rating = -2; }
			else if (v == CorrectionNeeded) { RequiresCorrection = (! RequiresCorrection); }
			// }
		}

		protected void ClearImages ( ) {
			// Clears previously set images
			Rating0.SetImageDrawable ( null );
			Rating1.SetImageDrawable ( null );
			Rating2.SetImageDrawable ( null );
			Rating3.SetImageDrawable ( null );
			Rating4.SetImageDrawable ( null );
		}

		void IDisposable.Dispose()
		{
			ClearImages();
			SetClickListener(null);
			Dispose();
		}

	}
}

