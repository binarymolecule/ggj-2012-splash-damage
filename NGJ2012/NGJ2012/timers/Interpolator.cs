using System;
using Microsoft.Xna.Framework;

namespace Utility
{
	/// <summary>
	/// A delegate used by Interpolators to scale their progress and generate their current value.
	/// </summary>
	/// <param name="progress">The current progress of the Interpolator in the range [0, 1].</param>
	/// <returns>A value representing the scaled progress used to generate the Interpolator's Value.</returns>
	public delegate float InterpolatorScaleDelegate(float progress);

	public sealed class Interpolator
	{
		private bool valid;
		private float progress;
		private float start;
		private float end;
		private float range;
		private float speed;
		private float value;
		private InterpolatorScaleDelegate scale;
		private Action<Interpolator> step;
		private Action<Interpolator> completed;

		/// <summary>
		/// Gets whether or not the interpolator is active.
		/// </summary>
		public bool IsActive { get { return valid; } }

		/// <summary>
		/// Gets the interpolator's progress in the range of [0, 1].
		/// </summary>
		public float Progress { get { return progress; } }

		/// <summary>
		/// Gets the interpolator's starting value.
		/// </summary>
		public float Start { get { return start; } }

		/// <summary>
		/// Gets the interpolator's ending value.
		/// </summary>
		public float End { get { return end; } }

		/// <summary>
		/// Gets the interpolator's current value.
		/// </summary>
		public float Value { get { return value; } }

		/// <summary>
		/// Gets or sets some extra data to the timer.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Internal constructor used by InterpolatorCollection
		/// </summary>
		internal Interpolator() { }

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="startValue">The starting value.</param>
		/// <param name="endValue">The ending value.</param>
		/// <param name="step">An optional delegate to invoke each update.</param>
		/// <param name="completed">An optional delegate to invoke upon completion.</param>
		public Interpolator(float startValue, float endValue, Action<Interpolator> step, Action<Interpolator> completed)
			: this(startValue, endValue, 1f, InterpolatorScales.Linear, step, completed)
		{
		}

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="startValue">The starting value.</param>
		/// <param name="endValue">The ending value.</param>
		/// <param name="interpolationLength">The amount of time, in seconds, for the interpolation to occur.</param>
		/// <param name="step">An optional delegate to invoke each update.</param>
		/// <param name="completed">An optional delegate to invoke upon completion.</param>
		public Interpolator(float startValue, float endValue, float interpolationLength, Action<Interpolator> step, Action<Interpolator> completed)
			: this(startValue, endValue, interpolationLength, InterpolatorScales.Linear, step, completed)
		{
		}

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="startValue">The starting value.</param>
		/// <param name="endValue">The ending value.</param>
		/// <param name="interpolationLength">The amount of time, in seconds, for the interpolation to occur.</param>
		/// <param name="scale">A custom delegate to use for scaling the Interpolator's value.</param>
		/// <param name="step">An optional delegate to invoke each update.</param>
		/// <param name="completed">An optional delegate to invoke upon completion.</param>
		public Interpolator(float startValue, float endValue, float interpolationLength, InterpolatorScaleDelegate scale, Action<Interpolator> step, Action<Interpolator> completed)
		{
			Reset(startValue, endValue, interpolationLength, scale, step, completed);
		}

		/// <summary>
		/// Stops the Interpolator.
		/// </summary>
		public void Stop()
		{
			valid = false;
		}

		/// <summary>
		/// Forces the interpolator to set itself to its final position and fire off its delegates before invalidating itself.
		/// </summary>
		public void ForceFinish()
		{
			if (valid)
			{
				valid = false;
				progress = 1;
				float scaledProgress = scale(progress);
				value = start + range * scaledProgress;

				if (step != null)
					step(this);

				if (completed != null)
					completed(this);
			}
		}

		internal void Reset(float s, float e, float l, InterpolatorScaleDelegate scaleFunc, Action<Interpolator> stepFunc, Action<Interpolator> completedFunc)
		{
			if (l <= 0f)
				throw new ArgumentException("length must be greater than zero");

			if (scaleFunc == null)
				throw new ArgumentNullException("scaleFunc");

			valid = true;
			progress = 0f;

			start = s;
			end = e;
			range = e - s;
			speed = 1f / l;

			scale = scaleFunc;
			step = stepFunc;
			completed = completedFunc;
		}

		/// <summary>
		/// Updates the Interpolator.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{
			if (!valid)
				return;

			// update the progress, clamping at 1f
			progress = Math.Min(progress + speed * (float)gameTime.ElapsedGameTime.TotalSeconds, 1f);

			// get the scaled progress and use that to generate the value
			float scaledProgress = scale(progress);
			value = start + range * scaledProgress;

			// invoke the step callback
			if (step != null)
				step(this);

			// if the progress is 1...
			if (progress == 1f)
			{
				// the interpolator is done
				valid = false;

				// invoke the completed callback
				if (completed != null)
					completed(this);

				Tag = null;
				scale = null;
				step = null;
				completed = null;
			}
		}
	}
}