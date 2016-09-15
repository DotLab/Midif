using System;

namespace Midif.Synth.Generic {
	public class AdsrEnvelopeGenerator : CachedSignalProvider {
		public double Attack;
		public double Decay;
		public double Sustain = 1;
		public double Release;

		int attackSample;
		int decaySample;
		int releaseSample;

		double[] onTable, offTable;

		bool isOn;
		double onPhase, offPhase;

		public override void Init (double sampleRate) {
			attackSample = (int)(Attack * sampleRate);
			decaySample = (int)((Attack + Decay) * sampleRate);
			releaseSample = (int)(Release * sampleRate);

			onTable = new double[decaySample];
			offTable = new double[releaseSample];

			for (int i = 0; i < attackSample; i++)
				onTable[i] = (double)i / attackSample;
			for (int i = attackSample; i < decaySample; i++)
				onTable[i] = 1 - (1 - Sustain) * (i - attackSample) / (decaySample - attackSample);

			for (int i = 0; i < releaseSample; i++)
				offTable[i] = Sustain * (releaseSample - i) / releaseSample;
		}


		public override void NoteOn (byte note, byte velocity) {
			isOn = true;
			onPhase = 0;
		}

		public override void NoteOff (byte velocity) {
			isOn = false;
			offPhase = 0;
		}

		public override bool IsActive () {
			return isOn || offPhase < releaseSample;
		}


		public override double Render () {
			if (!isOn)
				return offPhase < releaseSample ? offTable[(int)offPhase++] : 0;
			
			return onPhase < decaySample ? onTable[(int)onPhase++] : Sustain;
		}
	}
}

