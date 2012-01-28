using System;
using Microsoft.Xna.Framework;

namespace Utility
{
	/// <summary>
	/// A managed collection of timers.
	/// </summary>
	public sealed class TimerCollection
	{
		private readonly Pool<Timer> timers = new Pool<Timer>(10, t => t.IsActive);

		/// <summary>
		/// Creates a new Timer.
		/// </summary>
		/// <param name="tickLength">The amount of time between the timer's ticks.</param>
		/// <param name="repeats">Whether or not the timer repeats.</param>
		/// <param name="tick">An action to perform when the timer ticks.</param>
		/// <returns>The new Timer object or null if the timer pool is full.</returns>
		public Timer Create(float tickLength, bool repeats, Action<Timer> tick)
		{
			if (tickLength <= 0f)
				throw new ArgumentException("tickLength must be greater than zero.");
			if (tick == null)
				throw new ArgumentNullException("tick");

			lock (timers)
			{
				// get a new timer from the pool
				Timer t = timers.New();
				t.Reset(tickLength, repeats, tick);

				return t;
			}
		}

		/// <summary>
		/// Updates all timers in the collection.
		/// </summary>
		/// <param name="gameTime">The current game timestamp.</param>
		public void Update(GameTime gameTime)
		{
			lock (timers)
			{
				for (int i = 0; i < timers.ValidCount; i++)
					timers[i].Update(gameTime);
				timers.CleanUp();
			}
		}
	}
}