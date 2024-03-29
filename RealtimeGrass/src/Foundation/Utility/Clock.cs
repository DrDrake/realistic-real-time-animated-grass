﻿using System.Diagnostics;

using SlimDX;

namespace RealtimeGrass.Utility {
	/// <summary>
	/// A mechanism for tracking elapsed time.
	/// </summary>
	public class Clock {
		#region Public Interface

		/// <summary>
		/// Initializes a new instance of the <see cref="Clock"/> class.
		/// </summary>
		public Clock() {
			frequency = Stopwatch.Frequency;
		}

		public void Start() {
			count = Stopwatch.GetTimestamp();
			isRunning = true;
		}

		/// <summary>
		/// Updates the clock.
		/// </summary>
		/// <returns>The time, in seconds, that elapsed since the previous update.</returns>
		public float Update() {
			float result = 0.0f;
			if( isRunning ) {
				long last = count;
				count = Stopwatch.GetTimestamp();
				result = (float)( count - last ) / frequency;
			}

			return result;
		}

        //Methode Hinzugefügt: Gibt Zeit in Sekunden seit start/update an.
        public float Check() {
            float result = 0.0f;
            if( isRunning ) {
                long last = count;
                result = (float)( Stopwatch.GetTimestamp() - last ) / frequency;
            }

            return result;
        }

		#endregion
		#region Implementation Detail

		private bool isRunning;
		private readonly long frequency;
		private long count;

		#endregion
	}
}
