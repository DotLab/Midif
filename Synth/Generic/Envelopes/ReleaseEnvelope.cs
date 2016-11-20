namespace Midif.Synth {
	public class ReleaseEnvelope : MidiComponent {
		public IComponent Source;

		public double Release;

		public override bool IsActive { get { return isOn || offCounter < offLength; } }

		protected int releaseSample;
		protected double[] offLevels;
		protected int offLength;

		int offCounter = int.MaxValue;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			BuildLevels();

			Source.Init(sampleRate);
		}

		public virtual void BuildLevels () {
			releaseSample = (int)(Release * sampleRate);

			offLevels = new double[releaseSample];

			for (int i = 0; i < offLevels.Length; i++)
				offLevels[i] = 1 - (double)i / releaseSample;

			offLength = offLevels.Length - 1;
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			offCounter = 0;

			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);

			Source.NoteOff(note, velocity);
		}


		public override double Render () {
			if (isOn)
				return Source.Render(renderFlag);
			
			if (offCounter > offLength)
				return 0;
			
			return Source.Render(renderFlag) * offLevels[offCounter++];
		}
	}
}
