using System;

namespace Midif.Synth.Generic {
	public class DahdsrEnvelopeGenerator : CachedSignalProvider {
		public double Delay;
		public double Attack;
		public double Hold;
		public double Decay;
		public double Sustain = 1;
		public double Release;

		double decayDrop;

		int delaySample;
		int attackSample;
		int holdSample;
		int decaySample;
		int releaseSample;

		int attackLength;
		int decayLength;

		// Output at start of the release.
		double level;

		bool isOff;
		int onPhase;
		int offPhase;

		public override void Init (double sampleRate) {
			// TODO: Make a envelope table.
			decayDrop = 1 - Sustain;

			delaySample = (int)(Delay * sampleRate);
			attackSample = (int)(Attack * sampleRate) + delaySample;
			holdSample = (int)(Hold * sampleRate) + attackSample;
			decaySample = (int)(Decay * sampleRate) + holdSample;

			releaseSample = (int)(Release * sampleRate);

			attackLength = (int)(Attack * sampleRate);
			decayLength = (int)(Decay * sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			isOff = false;

			onPhase = 0;
			offPhase = 0;
		}

		public override void NoteOff (byte velocity) {
			level = cachedSample;

			isOff = true;
		}

		public override bool IsActive () {
			return offPhase < releaseSample; 
		}


		public override double Render () {
			if (isOff) {
				if (offPhase > releaseSample)
					return 0;
				
				return Sustain * (releaseSample - offPhase++) / releaseSample;
			}
				
			onPhase++;
			if (onPhase < delaySample)
				return 0;
			if (onPhase < attackSample)
				return (double)(onPhase - delaySample) / attackLength;
			if (onPhase < holdSample)
				return 1;
			if (onPhase < decaySample)
				return Sustain - (double)(onPhase - decaySample) / decayLength * decayDrop;
			
			return Sustain;
		}
	}
}