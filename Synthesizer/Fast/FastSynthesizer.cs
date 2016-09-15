using System;

namespace Midif.Synthesizer.Fast {
	[Serializable]
	public class FastSynthesizer : MidiSynthesizer {
		[Serializable]
		public class Voice {
			public byte Note;

			public double Gain;
			public double Step;

			public double Phase;
		}

		const double PerVoiceLevel = 0.125 / 6;

		static Random Random = new Random();

		double[] velocityGainTable;
		double[] notePhaseStepTable;

		int waveTableMod;
		int waveTableLength;
		double[] waveTable;

		public Voice[] voices;

		public FastSynthesizer () {
			SetWaveTable(new double[]{ 1, -1 });
		}

		public FastSynthesizer (double[] table) {
			SetWaveTable(table);
		}

		public FastSynthesizer (int samplePower) {
			var sampleCount = 1 << samplePower;
			var samples = new double[sampleCount];
			for (int i = 0; i < sampleCount; i++)
				samples[i] = Random.NextDouble() * 2 - 1;

			SetWaveTable(samples);
		}

		public void SetWaveTable (double[] newTable) {
			// http://www.exploringbinary.com/ten-ways-to-check-if-an-integer-is-a-power-of-two-in-c/
			var length = newTable.Length;
			if (!((length != 0) && ((length & (~length + 1)) == length)))
				throw new Exception("Unsupported table.Length : " + newTable.Length);

			waveTable = newTable;
			waveTableMod = length - 1;
			waveTableLength = length;
		}

		public override void Init (int polyphony, double sampleRate) {
			base.Init(polyphony, sampleRate);

			voices = new Voice[polyphony];
			for (int i = 0; i < polyphony; i++)
				voices[i] = new Voice();

			notePhaseStepTable = new double[128];
			for (int i = 0; i < 128; i++)
				notePhaseStepTable[i] = waveTableLength * (440 * Math.Pow(2, (double)(i - 69) / 12)) / sampleRate;

			velocityGainTable = new double[128];
			for (int i = 0; i < 128; i++)
				velocityGainTable[i] = MidiTables.Recip127[i] * PerVoiceLevel;
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

		public override void Aftertouch (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Note == note) {
					voice.Gain = velocityGainTable[velocity];
					return;
				}
		}

		public override void ChannelAftertouch (byte channel, byte pressure) {
			foreach (var voice in voices)
				if (voice.Note != 0)
					voice.Gain = velocityGainTable[pressure];
		}

		public override double RenderMono () {
			double sample = 0;
			foreach (var voice in voices)
				if (voice.Note != 0)
					sample += waveTable[(int)(voice.Phase += voice.Step) & waveTableMod] * voice.Gain;

			return sample;
		}
	}
}

