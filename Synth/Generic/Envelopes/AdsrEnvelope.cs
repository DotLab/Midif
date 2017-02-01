namespace Midif.Synth {
	public class AdsrEnvelope : MidiComponent {
		public MidiComponent Source;

		public double Attack;
		public double Decay;
		public double Sustain = 1;
		public double Release;

		protected int attackSample, decaySample, releaseSample;
		protected double decayDrop, releaseDrop;

		protected double[] onLevels, offLevels;
		protected int onLength, offLength;
		int onCounter, offCounter = int.MaxValue;


		public sealed override void Init (double sampleRate) {
			SampleRate = sampleRate;
			SampleRateRecip = 1 / sampleRate;
		
			BuildLevels();

			Source.Init(sampleRate);
		}

		public virtual void BuildLevels () {
			attackSample = (int)(Attack * SampleRate);
			decaySample = (int)(Decay * SampleRate);
			releaseSample = (int)(Release * SampleRate);

			decayDrop = 1 - Sustain;
			releaseDrop = Sustain;

			onLevels = new double[attackSample + decaySample];
			offLevels = new double[releaseSample];
//			Visualizer.Buffers[6] = new Visualizer.Buffer(onLevels.Length, UnityEngine.Color.blue);
			for (int i = 0; i < onLevels.Length; i++) {
				onLevels[i] = 
					i < attackSample ? (double)i / attackSample :
					1 - decayDrop * (i - attackSample) / decaySample;
//				Visualizer.Buffers[6].Push((float)onLevels[i]);
			}
//			Visualizer.Buffers[7] = new Visualizer.Buffer(offLevels.Length, UnityEngine.Color.grey);
			for (int i = 0; i < offLevels.Length; i++) {
				offLevels[i] = 1 - (double)i / releaseSample;
//				Visualizer.Buffers[7].Push((float)offLevels[i]);
			}
			onLength = onLevels.Length - 1;
			offLength = offLevels.Length - 1;
		}


		public sealed override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			onCounter = offCounter = 0;
		
			Source.NoteOn(note, velocity);
		}

		public sealed override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			if (onCounter > onLength) {
				releaseDrop = Sustain;
			} else {
				releaseDrop = onLevels[onCounter];
			}

			Source.NoteOff(note, velocity);
		}

		public sealed override bool IsFinished () {
			return offCounter >= offLength;
		}

		public sealed override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				if (IsOn) {
					if (onCounter > onLength)
						return RenderCache = Source.Render(RenderFlag) * Sustain;

					return RenderCache = Source.Render(RenderFlag) * onLevels[onCounter++];
				}

				if (offCounter >= offLength)
					return RenderCache = 0;

				return RenderCache = Source.Render(RenderFlag) * releaseDrop * offLevels[offCounter++];
			}

			return RenderCache;
		}
	}
}
