using System;

namespace Midif.Synth.Sfz {
	public abstract class SfzComponent : CachedSignalProvider {
		/// <summary>
		/// <list>
		/// Pitch          | pitcheg | -1200 to 1200 cents (1 octave)
		/// Filter cutoff  | fileg   | -1200 to 1200 cents (1 octave)
		/// Amplifier gain | ampeg   | 0% to 100%
		///	</summary>
		public ISignalProvider Eg;
		public double EgDepth;
		public double EgVel2Depth;

		/// <summary>
		/// <list>
		/// Pitch          | pitcheg | -1200 to 1200 cents (1 octave)
		/// Filter cutoff  | fileg   | -1200 to 1200 cents (1 octave)
		/// Amplifier gain | ampeg   | -10 to 10 dB
		///	</summary>
		public ISignalProvider Lfo;
		public double LfoDepth;
		public double LfoVel2Depth;

		/// <summary>
		/// <list>
		/// The adjustment is : (note - center) * depth 
		/// Pitch          | pitch_keycenter | 60
		/// Filter cutoff  | fil_keycenter   | 60
		/// Amplifier gain | amp_keycenter   | 60
		///	</summary>	
		public int KeyCenter = 60;
		/// <summary>
		/// <list>
		/// The adjustment is : (note - center) * depth 
		/// Pitch          | pitch_keytrack | -1200 to 1200 cents
		/// Filter cutoff  | fil_keytrack   | 0 to 1200 cents
		/// Amplifier gain | amp_keytrack   | -96 to 12dB
		///	</summary>	
		public double KeyTrack;

		/// <summary>
		/// <list>
		/// The adjustment is : (depth * velocity / 127)
		/// Pitch          | pitch_veltrack | -9600 to 9600 cents
		/// Filter cutoff  | fil_veltrack   | -9600 to 9600 cents
		/// Amplifier gain | amp_veltrack   | -100% to 100%
		///	</summary>		
		public double VelTrack;

		protected bool egEnabled, lfoEnabled;
		protected bool egVelTrackEnabled, lfoVelTrackEnabled;
		protected double egTotalDepth, lfoTotalDepth;

		protected bool keyTrackEnabled, velTrackEnabled;
		protected double keyTrackDepth, velTrackDepth;


		public override void Init (double sampleRate) {
			egEnabled = EgDepth != 0 && Eg != null;
			lfoEnabled = LfoDepth != 0 && Lfo != null;

			egVelTrackEnabled = egEnabled && EgVel2Depth != 0;
			lfoVelTrackEnabled = lfoEnabled && LfoVel2Depth != 0;

			keyTrackEnabled = KeyTrack != 0;
			velTrackEnabled = VelTrack != 0;

			if (egEnabled)
				Eg.Init(sampleRate);

			if (lfoEnabled)
				Lfo.Init(sampleRate);
		}

		public override void NoteOn (byte note, byte velocity) {
			if (egEnabled) {
				Eg.NoteOn(note, velocity);

				egTotalDepth = EgDepth;
				if (egVelTrackEnabled)
					egTotalDepth += EgVel2Depth * SynthConstants.Recip127[velocity];
			}

			if (lfoEnabled) {
				Lfo.NoteOn(note, velocity);

				lfoTotalDepth = LfoDepth;
				if (lfoVelTrackEnabled)
					lfoTotalDepth += LfoVel2Depth * SynthConstants.Recip127[velocity];
			}

			if (keyTrackEnabled)
				keyTrackDepth = KeyTrack * (note - KeyCenter);
			if (velTrackEnabled)
				velTrackDepth = VelTrack * SynthConstants.Recip127[velocity];
		}

		public override void NoteOff (byte velocity) {
			if (egEnabled)
				Eg.NoteOff(velocity);
			if (lfoEnabled)
				Lfo.NoteOff(velocity);
		}

		public override bool IsActive () {
			return egEnabled && Eg.IsActive();
		}
	}
}

