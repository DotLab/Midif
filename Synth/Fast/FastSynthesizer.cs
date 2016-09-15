using System;

namespace Midif.Synth.Fast {
	[Serializable]
	public class FastSynthesizer : MidiSynthesizer {
		public class Voice {
			public bool Active;

			public byte Note;
			public byte Velocity;

			public double Step;
			public double Phase;
		}

		static readonly Random rand = new Random();
		static double cache;

		public double MasterLeve = -10;
		public sbyte Transpose;

		//		double[] velocityGainTable;
		double[] notePhaseStepTable;

		int waveTableMod;
		int waveTableLength;
		double[] waveTable;

		double[] outputTable;

		Voice[] voices;

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
				samples[i] = rand.NextDouble() * 2 - 1;

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

		public override void Init (double sampleRate, int polyphony) {
			base.Init(sampleRate, polyphony);

			notePhaseStepTable = new double[128];
			for (int i = 0; i < 128; i++)
				notePhaseStepTable[i] = waveTableLength * (440 * Math.Pow(2, (double)(i - 69 + Transpose) / 12)) / sampleRate;

//			velocityGainTable = new double[128];
//			for (int i = 0; i < 128; i++)
//				velocityGainTable[i] = SynthConstants.Recip127[i] * PerVoiceLevel;

			outputTable = new double[waveTable.Length << 8 | 128];
			for (int i = 0; i < waveTable.Length; i++)
				for (int j = 0; j < 128; j++)
					outputTable[i << 8 | j] = waveTable[i] * SynthConstants.Decibel2Gain(-20 * Math.Log10(Math.Pow(127, 2) / Math.Pow(j, 2))) / polyphony;

			voices = new Voice[polyphony];
			for (int i = 0; i < polyphony; i++)
				voices[i] = new Voice();
		}

		public override void Reset () {
			foreach (var voice in voices)
				voice.Active = false;
		}


		public override void NoteOn (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (!voice.Active) {
					voice.Active = true;

					voice.Note = note;
					voice.Velocity = velocity;

					voice.Step = notePhaseStepTable[note];
					voice.Phase = 0;
					return;
				}
		}

		public override void NoteOff (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Active && voice.Note == note) {
					voice.Active = false;
					return;
				}
		}

		public override void Render (ref double sample) {
			foreach (var voice in voices)
				if (voice.Active)
					sample += outputTable[(int)(voice.Phase += voice.Step) & waveTableMod << 8 | voice.Velocity];
		}

		public double Render () {
			cache = 0.0;

			foreach (var voice in voices)
				if (voice.Active)
					cache += outputTable[((int)(voice.Phase += voice.Step) & waveTableMod) << 8 | voice.Velocity];
				
			return cache;
		}
	}
}

