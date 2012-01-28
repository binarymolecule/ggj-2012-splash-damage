using System;
using Microsoft.Xna.Framework;

namespace Utility
{
	/// <summary>
	/// A managed collection of interpolators.
	/// </summary>
	public sealed class InterpolatorCollection
	{
		private readonly Pool<Interpolator> interpolators = new Pool<Interpolator>(10, i => i.IsActive);

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="end">The ending value.</param>
		/// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
		/// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
		/// <returns>The Interpolator instance.</returns>
		public Interpolator Create(
			float start,
			float end,
			Action<Interpolator> step,
			Action<Interpolator> completed)
		{
			return Create(start, end, 1f, InterpolatorScales.Linear, step, completed);
		}

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="end">The ending value.</param>
		/// <param name="length">The length of time, in seconds, to perform the interpolation.</param>
		/// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
		/// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
		/// <returns>The Interpolator instance.</returns>
		public Interpolator Create(
			float start,
			float end,
			float length,
			Action<Interpolator> step,
			Action<Interpolator> completed)
		{
			return Create(start, end, length, InterpolatorScales.Linear, step, completed);
		}

		/// <summary>
		/// Creates a new Interpolator.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="end">The ending value.</param>
		/// <param name="length">The length of time, in seconds, to perform the interpolation.</param>
		/// <param name="scale">A delegate that handles converting the interpolator's progress to a value.</param>
		/// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
		/// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
		/// <returns>The Interpolator instance.</returns>
		public Interpolator Create(
			float start,
			float end,
			float length,
			InterpolatorScaleDelegate scale,
			Action<Interpolator> step,
			Action<Interpolator> completed)
		{
			lock (interpolators)
			{
				Interpolator i = interpolators.New();
				i.Reset(start, end, length, scale, step, completed);

				return i;
			}
		}

		/// <summary>
		/// Updates all active Interpolators in the collection.
		/// </summary>
		/// <param name="gameTime">The current game timestamp.</param>
		public void Update(GameTime gameTime)
		{
			lock (interpolators)
			{
				for (int i = 0; i < interpolators.ValidCount; i++)
					interpolators[i].Update(gameTime);
				interpolators.CleanUp();
			}
		}
	}
}