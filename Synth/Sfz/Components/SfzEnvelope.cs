using System;

namespace Midif.Synth.Sfz {
	public class SfzEnvelope : CachedSignalProvider {
		public double Delay;
		public double Vel2Delay;

		public double Start;
		public double Vel2Start;

		public double Attack;
		public double Vel2Attack;

		public double Hold;
		public double Vel2Hold;

		public double Decay;
		public double Vel2Decay;

		public double Sustain = 1;
		public double Vel2Sustain;

		public double Release;
		public double Vel2Release;

		double sampleRate;

		double baseDelaySample;
		double baseAttackSample;
		double baseHoldSample;
		double baseDecaySample;
		double baseReleaseSample;

		bool delayVelTrackEnabled;
		bool startVelTrackEnabled;
		bool attackVelTrackEnabled;
		bool holdVelTrackEnabled;
		bool decayVelTrackEnabled;
		bool sustainVelTrackEnabled;
		bool releaseVelTrackEnabled;

		double start;
		double sustain;
		int delaySample;
		int attackSample;
		int holdSample;
		int decaySample;
		int releaseSample;

		int attackLength;
		double attackRise;
		int decayLength;
		double decayDrop;

		bool isOff;
		double level;
		int onPhase, offPhase;


		public override void Init (double sampleRate) {
			this.sampleRate = sampleRate;

			baseDelaySample = Delay * sampleRate;
			baseAttackSample = Attack * sampleRate;
			baseHoldSample = Hold * sampleRate;
			baseDecaySample = Decay * sampleRate;
			baseReleaseSample = Release * sampleRate;

			delayVelTrackEnabled = Vel2Delay != 0;
			startVelTrackEnabled = Vel2Start != 0;
			attackVelTrackEnabled = Vel2Attack != 0;
			holdVelTrackEnabled = Vel2Hold != 0;
			decayVelTrackEnabled = Vel2Decay != 0;
			sustainVelTrackEnabled = Vel2Sustain != 0;
			releaseVelTrackEnabled = Vel2Release != 0;
		}

		public override void NoteOn (byte note, byte velocity) {
			start = Start;
			if (startVelTrackEnabled)
				start += Vel2Start * SynthConstants.Recip127[velocity];
			
			sustain = Sustain;
			if (sustainVelTrackEnabled)
				sustain += Vel2Sustain * SynthConstants.Recip127[velocity];

			delaySample = (int)baseDelaySample;
			if (delayVelTrackEnabled)
				delaySample += (int)(Vel2Delay * SynthConstants.Recip127[velocity]);

			attackSample = (int)baseAttackSample + delaySample;
			if (attackVelTrackEnabled)
				attackSample += (int)(Vel2Attack * SynthConstants.Recip127[velocity]);

			holdSample = (int)baseHoldSample + attackSample;
			if (holdVelTrackEnabled)
				holdSample += (int)(Vel2Hold * SynthConstants.Recip127[velocity]);

			decaySample = (int)baseDecaySample + holdSample;
			if (decayVelTrackEnabled)
				decaySample += (int)(Vel2Decay * SynthConstants.Recip127[velocity]);
			
			releaseSample = (int)baseReleaseSample;
			if (releaseVelTrackEnabled)
				releaseSample += (int)(Vel2Release * SynthConstants.Recip127[velocity]);

			attackLength = attackSample - delaySample;
			attackRise = 1 - start;
			decayLength = decaySample - holdSample;
			decayDrop = 1 - sustain;

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
				return sustain * (releaseSample - offPhase++) / releaseSample;
			}

			onPhase++;
			if (onPhase < delaySample)
				return start;
			if (onPhase < attackSample)
				return (double)(onPhase - delaySample) / attackLength * attackRise;
			if (onPhase < holdSample)
				return 1;
			if (onPhase < decaySample)
				return sustain - (double)(onPhase - decaySample) / decayLength * decayDrop;

			return sustain;
		}
	}
}

