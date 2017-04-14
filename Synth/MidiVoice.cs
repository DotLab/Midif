namespace Midif.Synth {
	public delegate MidiVoice MidiVoiceDelegate ();

	/// <summary>
	/// The outputing point of MidiComponent.
	/// </summary>
	[System.Serializable]
	public class MidiVoice : MidiComponent {
		public double Pan;
		public double LeftGain = 0.5, RightGain = 0.5;

		public readonly bool IsStereo;
		public readonly MidiComponent Component, RightComponent;

		public bool Active, Finished = true, Sustained;

		public MidiEvent Event = new MidiEvent(0, 0, 0);

		public MidiVoice (MidiComponent component) {
			IsStereo = false;

			Component = component;
			RightComponent = component;
		}

		public MidiVoice (MidiComponent component, MidiComponent rightComponent) {
			IsStereo = true;

			Component = component;
			RightComponent = rightComponent;
		}


		public override void Init (double sampleRate) {
			SampleRate = sampleRate;

			Component.Init(sampleRate);

			if (IsStereo)
				RightComponent.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;
			Note = note;
			Velocity = velocity;

			Active = true;
			Finished = false;

			Component.NoteOn(note, velocity);

			if (IsStereo)
				RightComponent.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;
			Sustained = false;

			Component.NoteOff(note, velocity);

			if (IsStereo)
				RightComponent.NoteOff(note, velocity);
		}

		/// <summary>
		/// Query and cache the voice state.
		/// This is different from the reduced version of MidiComponent.IsFinished().
		/// </summary>
		/// <returns><c>true</c> if this component is finished; otherwise, <c>false</c>.</returns>
		public override bool IsFinished () {
			if (IsOn) {
				Active = true;
				return Finished = false;
			}

			if (IsStereo)
				Finished = Component.IsFinished() && RightComponent.IsFinished();
			else
				Finished = Component.IsFinished();
			
			Active = !Finished;
			return Finished;
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				RenderCache = Component.Render(flag);

				return  RenderCache * LeftGain;
			}

			return RenderCache * LeftGain;
		}

		public virtual double RenderRight (bool flag) {
			if (IsStereo)
				return RightComponent.Render(flag) * RightGain;

			return Component.RenderCache * RightGain;
		}

		public override void Process (float[] buffer) {
			var temp = BufferControl.RequestBuffer();
			Component.Process(temp);

			if (IsStereo) {
				var tempRight = BufferControl.RequestBuffer();
				RightComponent.Process(tempRight);

				for (int i = 0; i < buffer.Length; i += 2) {
					buffer[i] += temp[i >> 1] * (float)LeftGain;
					buffer[i + 1] += tempRight[i >> 1] * (float)RightGain;
				}

				BufferControl.FreeBuffer(tempRight);
			} else
				for (int i = 0; i < buffer.Length; i += 2) {
					buffer[i] += temp[i >> 1] * (float)LeftGain;
					buffer[i + 1] += temp[i >> 1] * (float)RightGain;
				}

			BufferControl.FreeBuffer(temp);
		}

		public override string ToString () {
			return string.Format("[MidiVoice: Note={0}, IsOn={1}, Active={2}]", Note, IsOn, Active);
		}
	}
}