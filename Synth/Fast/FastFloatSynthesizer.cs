using System;

namespace Midif.Synth.Fast {
	[Serializable]
	public class FastFloatSynthesizer : MidiSynthesizer {
		public class Voice {
			public byte Note;
			public float Gain;

			public float Step;
			public float Phase;
		}

		const float PerVoiceLevel = 0.125f / 4;
		static readonly Random rand = new Random();

		float[] velocityGainTable;
		float[] notePhaseStepTable;

		int waveTableMod;
		int waveTableLength;
		float[] waveTable;

		Voice[] voices;

		public FastFloatSynthesizer () {
			SetWaveTable(new float[]{ 1, -1 });
		}

		public FastFloatSynthesizer (float[] table) {
			SetWaveTable(table);
		}

		public FastFloatSynthesizer (int samplePower) {
			var sampleCount = 1 << samplePower;
			var samples = new float[sampleCount];
			for (int i = 0; i < sampleCount; i++)
				samples[i] = (float)rand.NextDouble() * 2 - 1;

			SetWaveTable(samples);
		}

		public void SetWaveTable (float[] newTable) {
			// http://www.exploringbinary.com/ten-ways-to-check-if-an-integer-is-a-power-of-two-in-c/
			var length = newTable.Length;
			if (!((length != 0) && ((length & (~length + 1)) == length)))
				throw new Exception("Unsupported table.Length : " + newTable.Length);

			waveTable = newTable;
			waveTableMod = length - 1;
			waveTableLength = length;
		}

		public override void Init (double sampleRate, int polyphony) {
			base.Init(sampleRate, polyphony);

			notePhaseStepTable = new float[128];
			for (int i = 0; i < 128; i++)
				notePhaseStepTable[i] = waveTableLength * (440 * (float)Math.Pow(2, (float)(i - 69) / 12)) / (float)sampleRate;

			velocityGainTable = new float[128];
			for (int i = 0; i < 128; i++)
				velocityGainTable[i] = (float)SynthConstants.Recip127[i] * PerVoiceLevel;

			voices = new Voice[polyphony];
			for (int i = 0; i < polyphony; i++)
				voices[i] = new Voice();
		}

		public override void Reset () {
			foreach (var voice in voices)
				voice.Note = 0;
		}


		public override void NoteOn (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Note == 0) {
					voice.Note = note;
					voice.Gain = velocityGainTable[velocity];
					voice.Step = notePhaseStepTable[note];
					voice.Phase = 0;
					return;
				}
		}

		public override void NoteOff (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Note == note) {
					voice.Note = 0;
					return;
				}
		}

		public override void Render (ref double sample) {
			foreach (var voice in voices)
				if (voice.Note != 0)
					sample += waveTable[(int)(voice.Phase += voice.Step) & waveTableMod] * voice.Gain;
		}

		public void Render (ref float sample) {
			foreach (var voice in voices)
				if (voice.Note != 0)
					sample += waveTable[(int)(voice.Phase += voice.Step) & waveTableMod] * voice.Gain;
		}
	}
}

