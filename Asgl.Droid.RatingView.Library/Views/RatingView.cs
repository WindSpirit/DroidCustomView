/*
 * 
 * NOTE: Attempted to use...
 * 
 *	 - TextView for "Not Applicable", but visuals were slightly off
 *	 - CheckBox for "Requires Correction", but check-box styling was wrong
 * 
 *   Above resulted in the use of ImageView for all Rating items.
 * 
 * See also: 
 * 
 *   Google I/O 2013 - Writing Custom Views for Android 
 *   https://www.youtube.com/watch?v=NYtB6mlu7vA#t=1286
 * 
 */


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
		//    1 = True (thumbs up / pass); or, Rating of 1
		//    2..5 = Rating of 2..5

		private const int RatingBelowMinimum = -3;
		private const int RatingNotApplicable = -2;
		private const int RatingFail = 0;
		private const int RatingPass = 1;

		private const int BooleanScale = 2;

		protected View GroupView;

		protected ImageView Rating0View = null;
		protected ImageView Rating1View = null;
		protected ImageView Rating2View = null;
		protected ImageView Rating3View = null;
		protected ImageView Rating4View = null;
		protected ImageView NotApplicableView = null;
		protected ImageView CorrectionNeededView = null;

		protected Drawable ActnGood;
		protected Drawable ActnBad;
		protected Drawable ActnImportant;
		protected Drawable ActnNotImportant;
		protected Drawable ActnNotApplicable;
		protected Drawable ActnCorrectionNeeded;
		protected Drawable ActnCorrectionNotNeeded;

		protected Color ColourDisabled;
		protected Color ColourGood;
		protected Color ColourBad;
		protected Color ColourImportant;
		protected Color ColourNotImportant;
		protected Color ColourBlack;
		protected Color ColourRed;

		protected bool IsInitialized = false;

		#region --[ CTOR ]--

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

			var attrArray = Context.ObtainStyledAttributes ( attrs, Resource.Styleable.RatingView );

			// ReadOnly attribute *MUST* be set prior to setting any click event listners.
			// Otherwise you may not be able to remove the listeners and/or bubble the click events up to the parent.
			ReadOnly = attrArray.GetBoolean ( Resource.Styleable.RatingView_readOnly, true );

			// Set Requires Correction based on *.axml specified value
			RequiresCorrectionVisible = attrArray.GetBoolean ( Resource.Styleable.RatingView_requiresCorrectionVisible, true );

			OnCreateView ( );

			// Set Scale based on *.axml specified value
			var tmpScale = attrArray.GetInteger ( Resource.Styleable.RatingView_scale, RatingBelowMinimum );
			if ( tmpScale > RatingBelowMinimum ) Scale = tmpScale;

			// Set Rating based on *.axml specified value
			var tmpRating = attrArray.GetInteger ( Resource.Styleable.RatingView_rating, RatingBelowMinimum );
			if ( tmpRating > RatingBelowMinimum ) Rating = tmpRating;

			// Set Not Applicable based on *.axml specified value (not that it actually makes sense to do so)
			NotApplicable = attrArray.GetBoolean ( Resource.Styleable.RatingView_notApplicable, false );

			// Set Requires Correction based on *.axml specified value
			RequiresCorrection = attrArray.GetBoolean ( Resource.Styleable.RatingView_requiresCorrection, false );

			attrArray.Recycle ( );
		}

		public void OnCreateView ( ) {

			GroupView = Inflate ( Context, Resource.Layout.angus_rating_view, this );

			// AlwaysDrawnWithCacheEnabled = false; // Uncomment this if you want Draw events

			InitColour ( );
			InitDrawables ( );
			InitControls ( GroupView );

			IsInitialized = true;
		}

		#endregion

		#region --[ Properties ]--

		// For simplicity, ReadOnly must be turned off in AXML View markup.
		// That is, ReadOnly condition cannot be changed after class constructor.
		private bool _readOnly = true;
		public bool ReadOnly {
			get { return _readOnly; }
			private set {
				if ( _readOnly != value ) {
					_readOnly = value;
					if ( IsInitialized ) {
						SetClickListener ( ReadOnly ? null : this );
						if ( ReadOnlyChanged != null ) ReadOnlyChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler ReadOnlyChanged;

		private int? _rating = null;
		public int? Rating {
			get {
				return _rating;
			}
			set {
				if ( ( value != null ) && ( value < -2 || value > 5 ) ) throw new ArgumentOutOfRangeException ( );
				if ( _rating != value ) {
					_rating = value;
					if ( IsInitialized ) {
						Refresh ( );
						if ( RatingChanged != null )
							RatingChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler RatingChanged;

		private int? _scale = null;
		public int? Scale {
			get { return _scale; }
			set {
				if ( ( value != null ) && ( value < 2 || value > 5 ) ) throw new ArgumentOutOfRangeException ( );
				if ( _scale != value ) {
					_scale = value;
					if ( IsInitialized ) {
						Refresh ( );
						if ( ScaleChanged != null )
							ScaleChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler ScaleChanged;

		public bool NotApplicable {
			get { return ( Rating == RatingNotApplicable ); }
			set {
				if ( ( Rating == RatingNotApplicable ) != value ) {
					Rating = ( value ) ? ( int? ) RatingNotApplicable : null;
					if ( IsInitialized ) {
						if ( NotApplicableChanged != null )
							NotApplicableChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler NotApplicableChanged;

		private bool _requiresCorrectionVisible;
		public bool RequiresCorrectionVisible {
			get { return _requiresCorrectionVisible; }
			set {
				if ( _requiresCorrectionVisible != value ) {
					_requiresCorrectionVisible = value;
					if ( IsInitialized ) {
						if ( RequiresCorrectionVisibleChanged != null )
							RequiresCorrectionVisibleChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler RequiresCorrectionVisibleChanged;

		private bool _requiresCorrection;
		public bool RequiresCorrection {
			get { return _requiresCorrection; }
			set {
				if ( _requiresCorrection != value ) {
					_requiresCorrection = value;
					if ( IsInitialized ) {
						if ( RequiresCorrectionChanged != null )
							RequiresCorrectionChanged ( this, EventArgs.Empty );
					}
				}
			}
		}
		public event EventHandler RequiresCorrectionChanged;

		#endregion

		#region -[ Init ]-

		protected void InitColour ( ) {
			ColourDisabled = Resources.GetColor ( Resource.Color.colourDisabled );
			ColourGood = Resources.GetColor ( Resource.Color.colourGood );
			ColourBad = Resources.GetColor ( Resource.Color.colourBad );
			ColourImportant = Resources.GetColor ( Resource.Color.colourImportant );
			ColourNotImportant = Resources.GetColor ( Resource.Color.colourNotImportant );
			ColourBlack = Resources.GetColor ( Resource.Color.black );
			ColourRed = Resources.GetColor ( Resource.Color.red );
		}

		protected void InitDrawables ( ) {
			ActnGood = Resources.GetDrawable ( Resource.Drawable.ic_action_good );
			ActnBad = Resources.GetDrawable ( Resource.Drawable.ic_action_bad );
			ActnImportant = Resources.GetDrawable ( Resource.Drawable.ic_action_important );
			ActnNotImportant = Resources.GetDrawable ( Resource.Drawable.ic_action_not_important );
			ActnNotApplicable = Resources.GetDrawable ( Resource.Drawable.ic_not_applicable );
			ActnCorrectionNeeded = Resources.GetDrawable ( Resource.Drawable.ic_is_checked );
			ActnCorrectionNotNeeded = Resources.GetDrawable ( Resource.Drawable.ic_not_checked );
		}

		protected void SetClickListener ( View.IOnClickListener listener ) {
			// Passing in null will clear listners
			Rating0View.SetOnClickListener ( listener );
			Rating1View.SetOnClickListener ( listener );
			Rating2View.SetOnClickListener ( listener );
			Rating3View.SetOnClickListener ( listener );
			Rating4View.SetOnClickListener ( listener );
			CorrectionNeededView.SetOnClickListener ( listener );
			NotApplicableView.SetOnClickListener ( listener );
		}

		protected void InitControls ( View rootView ) {
			Rating0View = rootView.FindViewById<ImageView> ( Resource.Id.rating0 );
			Rating1View = rootView.FindViewById<ImageView> ( Resource.Id.rating1 );
			Rating2View = rootView.FindViewById<ImageView> ( Resource.Id.rating2 );
			Rating3View = rootView.FindViewById<ImageView> ( Resource.Id.rating3 );
			Rating4View = rootView.FindViewById<ImageView> ( Resource.Id.rating4 );
			NotApplicableView = rootView.FindViewById<ImageView> ( Resource.Id.not_applicable );
			CorrectionNeededView = rootView.FindViewById<ImageView> ( Resource.Id.req_correction );
		}

		protected void SetVisibility ( ) {
			var testValue = Scale ?? 0;
			Rating0View.Visibility = ( testValue > 0 ) ? ViewStates.Visible : ViewStates.Gone;
			Rating1View.Visibility = ( testValue > 0 ) ? ViewStates.Visible : ViewStates.Gone;
			Rating2View.Visibility = ( testValue > 2 ) ? ViewStates.Visible : ViewStates.Gone;
			Rating3View.Visibility = ( testValue > 3 ) ? ViewStates.Visible : ViewStates.Gone;
			Rating4View.Visibility = ( testValue > 4 ) ? ViewStates.Visible : ViewStates.Gone;
			NotApplicableView.Visibility = ViewStates.Visible;
			CorrectionNeededView.Visibility = ( RequiresCorrectionVisible ) ? ViewStates.Visible : ViewStates.Gone;
		}

		#endregion

		protected override bool DrawChild ( Canvas canvas, View child, long drawingTime ) {
			// Not sure where the "child" came from, but we need to update it
			// to reflect the information and states in "this" View instance.

			// Establish references to the Views that we want to update
			InitControls ( child );

			// Update those Views
			Refresh ( );

			// Let DrawChild do its thing now that we have sync'd the GroupView's
			return base.DrawChild ( canvas, child, drawingTime );
		}


		protected void DrawCommonEnablement ( bool isSet ) {
			NotApplicableView.Enabled = ( !ReadOnly );
			CorrectionNeededView.Enabled = ( !ReadOnly );

			// Determine correct image to be displayed
			var correctionDrawable = RequiresCorrection ? ActnCorrectionNeeded : ActnCorrectionNotNeeded;

			if ( isSet ) {
				ActnNotApplicable.SetColorFilter ( NotApplicable ? ColourBlack : ColourDisabled, PorterDuff.Mode.SrcIn );
				correctionDrawable.SetColorFilter ( RequiresCorrection ? ColourRed : ColourDisabled, PorterDuff.Mode.SrcIn );
			} else {
				ActnNotApplicable.SetColorFilter ( ColourDisabled, PorterDuff.Mode.SrcIn );
				correctionDrawable.SetColorFilter ( ColourDisabled, PorterDuff.Mode.SrcIn );
			}

			NotApplicableView.SetImageDrawable ( ActnNotApplicable );

			if ( RequiresCorrectionVisible )
				CorrectionNeededView.SetImageDrawable ( correctionDrawable );
		}
		protected void DrawBooleanScaleDisabled ( ) {
			ActnGood.SetColorFilter ( ColourDisabled, PorterDuff.Mode.SrcIn );
			ActnBad.SetColorFilter ( ColourDisabled, PorterDuff.Mode.SrcIn );
			Rating0View.SetImageDrawable ( ActnGood );
			Rating1View.SetImageDrawable ( ActnBad );
			DrawCommonEnablement ( false );
		}
		protected void DrawMultiScaleDisabled ( ) {
			ActnNotImportant.SetColorFilter ( ColourDisabled, PorterDuff.Mode.SrcIn );
			Rating0View.SetImageDrawable ( ActnNotImportant );
			Rating1View.SetImageDrawable ( ActnNotImportant );
			Rating2View.SetImageDrawable ( ActnNotImportant );
			Rating3View.SetImageDrawable ( ActnNotImportant );
			Rating4View.SetImageDrawable ( ActnNotImportant );
			DrawCommonEnablement ( false );
		}

		protected void DrawBooleanScaleEnabled ( ) {
			ActnGood.SetColorFilter ( ( Rating == RatingPass ) ? ColourGood : ColourDisabled, PorterDuff.Mode.SrcIn );
			Rating0View.SetImageDrawable ( ActnGood );

			ActnBad.SetColorFilter ( ( Rating == RatingFail ) ? ColourBad : ColourDisabled, PorterDuff.Mode.SrcIn );
			Rating1View.SetImageDrawable ( ActnBad );

			DrawCommonEnablement ( true );
		}
		protected void DrawMultiScaleEnabled ( ) {
			ActnNotImportant.SetColorFilter ( ColourNotImportant, PorterDuff.Mode.SrcIn );
			ActnImportant.SetColorFilter ( ColourImportant, PorterDuff.Mode.SrcIn );

			Rating0View.SetImageDrawable ( ActnNotImportant );
			Rating1View.SetImageDrawable ( ActnNotImportant );
			Rating2View.SetImageDrawable ( ActnNotImportant );
			Rating3View.SetImageDrawable ( ActnNotImportant );
			Rating4View.SetImageDrawable ( ActnNotImportant );

			if ( Rating > 4 ) Rating4View.SetImageDrawable ( ActnImportant );
			if ( Rating > 3 ) Rating3View.SetImageDrawable ( ActnImportant );
			if ( Rating > 2 ) Rating2View.SetImageDrawable ( ActnImportant );
			if ( Rating > 1 ) Rating1View.SetImageDrawable ( ActnImportant );
			if ( Rating > 0 ) Rating0View.SetImageDrawable ( ActnImportant );

			DrawCommonEnablement ( true );
		}

		protected void Refresh ( ) {

			ClearImages ( );
			SetVisibility ( );

			// If you don't know the scale, you don't know what to render
			if ( Scale != null ) {
				if ( Rating == null ) {
					if ( Scale == BooleanScale ) DrawBooleanScaleDisabled ( );
					else DrawMultiScaleDisabled ( );
				} else {
					if ( Scale == BooleanScale ) DrawBooleanScaleEnabled ( );
					else DrawMultiScaleEnabled ( );
				}
			}
		}

		public void OnClick ( View v ) {
			// Set Rating based on View that was clicked on
			if ( v == Rating0View ) { Rating = ( Scale == BooleanScale ) ? RatingPass : 1; } else if ( v == Rating1View ) { Rating = ( Scale == BooleanScale ) ? RatingFail : 2; } else if ( v == Rating2View ) { Rating = 3; } else if ( v == Rating3View ) { Rating = 4; } else if ( v == Rating4View ) { Rating = 5; } else if ( v == NotApplicableView ) { Rating = RatingNotApplicable; } else if ( v == CorrectionNeededView ) { RequiresCorrection = ( !RequiresCorrection ); }
		}

		protected void ClearImages ( ) {
			// Clears previously set images
			Rating0View.SetImageDrawable ( null );
			Rating1View.SetImageDrawable ( null );
			Rating2View.SetImageDrawable ( null );
			Rating3View.SetImageDrawable ( null );
			Rating4View.SetImageDrawable ( null );
			NotApplicableView.SetImageDrawable ( null );
			CorrectionNeededView.SetImageDrawable ( null );
		}


		protected override void OnAttachedToWindow ( ) {
			// Google I/O 2013 - Writing Custom Views for Android 
			// https://www.youtube.com/watch?v=NYtB6mlu7vA#t=1286

			// Call super.onAttachedToWindow()!
			base.OnAttachedToWindow ( );

			// Perform any relevant state resets

			// Start listening for state changes
			// If you set the Listner to null, events (e.g. Click), will not bubble-up!
			if ( !ReadOnly ) SetClickListener ( this );
		}


		protected override void OnDetachedFromWindow ( ) {
			// Google I/O 2013 - Writing Custom Views for Android 
			// https://www.youtube.com/watch?v=NYtB6mlu7vA#t=1286

			// Call super.onDetachedFromWindow()!
			base.OnDetachedFromWindow ( );

			// Remove any posted Runnables

			// Stop listening for data (s/b state) changes
			SetClickListener ( null );

			// Clean up resources:
			// - Bitmaps
			// - Threads
			ClearImages ( );
		}
	}
}

