namespace Midif.Synth {
	public class AdsrEnvelope : MidiComponent {
		public IComponent Source;

		public double Attack;
		public double Decay;
		public double Sustain;
		public double Release;

		public override bool IsActive { get { return isOn || offCounter < offLength; } }

		protected int attackSample, decaySample, releaseSample;
		protected double decayDrop, releaseDrop;
		protected double[] onLevels, offLevels;
		protected int onLength, offLength;

		int onCounter, offCounter = int.MaxValue;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);
		
			BuildLevels();

			Source.Init(sampleRate);
		}

		public virtual void BuildLevels () {
			attackSample = (int)(Attack * sampleRate);
			decaySample = (int)(Decay * sampleRate);
			releaseSample = (int)(Release * sampleRate);

			decayDrop = 1 - Sustain;
			releaseDrop = Sustain;

			onLevels = new double[attackSample + decaySample];
			offLevels = new double[releaseSample];

			for (int i = 0; i < onLevels.Length; i++)
				onLevels[i] = 
					i < attackSample ? (double)i / attackSample :
					1 - decayDrop * (i - attackSample) / decaySample;
			for (int i = 0; i < offLevels.Length; i++)
				offLevels[i] = 1 - (double)i / releaseSample;

			onLength = onLevels.Length - 1;
			offLength = offLevels.Length - 1;
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			onCounter = offCounter = 0;
		
			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);

			releaseDrop = onCounter > onLength ? Sustain : onLevels[onCounter];

			if (releaseDrop == 0)
				offCounter = int.MaxValue;
			
			Source.NoteOff(note, velocity);
		}


		public override double Render () {
			// if is faster by 5 ticks
			// return
			// offCounter > offLength ? 0 :
			// Source.Render() * (isOn ? onCounter > onLength ? Sustain : onLevels[onCounter++] : releaseDrop * offLevels[offCounter++]);

			if (isOn) {
				if (onCounter > onLength) return Source.Render(renderFlag) * Sustain;
				return Source.Render(renderFlag) * onLevels[onCounter++];
			}
			if (offCounter > offLength) return 0;
			return Source.Render(renderFlag) * releaseDrop * offLevels[offCounter++];
		}
	}
}
