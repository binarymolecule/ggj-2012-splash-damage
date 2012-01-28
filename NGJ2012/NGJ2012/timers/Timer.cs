using System;
using Microsoft.Xna.Framework;

namespace Utility
{
	/// <summary>
	/// An object that invokes an action after an amount of time has elapsed and
	/// optionally continues repeating until told to stop.
	/// </summary>
	public sealed class Timer
	{
		private bool valid;
		private float time;
		private float tickLength;
		private bool repeats;
		private Action<Timer> tick;

		/// <summary>
		/// Gets whether or not the timer is active.
		/// </summary>
		public bool IsActive { get { return valid; } }

		/// <summary>
		/// Gets or sets some extra data to the timer.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Gets whether or not this timer repeats.
		/// </summary>
		public bool Repeats { get { return repeats; } }

		/// <summary>
		/// Gets the length of time (in seconds) between ticks of the timer.
		/// </summary>
		public float TickLength { get { return tickLength; } }

		internal Timer() { }

		/// <summary>
		/// Creates a new Timer.
		/// </summary>
		/// <param name="length">The length of time between ticks.</param>
		/// <param name="repeats">Whether or not the timer repeats.</param>
		/// <param name="tick">The delegate to invoke when the timer ticks.</param>
		public Timer(float length, bool repeats, Action<Timer> tick)
		{
			if (length <= 0f)
				throw new ArgumentException("length must be greater than 0");
			if (tick == null)
				throw new ArgumentNullException("tick");

			Reset(length, repeats, tick);
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			valid = false;
		}

		/// <summary>
		/// Forces the timer to fire its tick event, invalidating the timer unless it is set to repeat.
		/// </summary>
		public void ForceTick()
		{
			if (!valid)
				return;

			tick(this);
			time = 0f;

			valid = Repeats;

			if (!valid)
			{
				tick = null;
				Tag = null;
			}
		}

		internal void Reset(float l, bool r, Action<Timer> t)
		{
			valid = true;
			time = 0f;
			tickLength = l;
			repeats = r;
			tick = t;
		}

		/// <summary>
		/// Updates the timer.
		/// </summary>
		/// <param name="gameTime">The current game timestamp.</param>
		public void Update(GameTime gameTime)
		{
			// if a timer is stopped manually, it may not
			// be valid at this point so we skip i
			if (!valid)
				return;

			// update the timer's time
			time += (float)gameTime.ElapsedGameTime.TotalSeconds;

			// if the timer passed its tick length...
			if (time >= tickLength)
			{
				// perform the action
				tick(this);

				// subtract the tick length in case we need to repeat
				time -= tickLength;

				// if the timer doesn't repeat, it is no longer valid
				valid = repeats;

				if (!valid)
				{
					tick = null;
					Tag = null;
				}
			}
		}
	}
}