namespace Midif.Synth {
	public class MidiSynth : ISynth {
		public readonly int Polyphony;

		public readonly MidiVoice[] Voices;


		#region Transpose, Level, Pan, and Width

		public int Transpose;

		public double Level {
			get { return level; }
			set {
				level = value;
				gain = SynthTable.Deci2Gain(level);
			}
		}

		public double Pan;

		public double Width = 0.5;

		double level, gain = 1;

		#endregion

		#region Track and Channel

		public MidiTrack Track = MidiTrack.All;

		public MidiChannel Channel = MidiChannel.All;

		#endregion


		public MidiSynth (MidiVoiceBuilder voiceBuilder, int polyphony = 4) {
			Polyphony = polyphony;

			if (polyphony > 0) {
				Voices = new MidiVoice[Polyphony];
				for (int i = 0; i < Polyphony; i++)
					Voices[i] = voiceBuilder();
			} else {
				Voices = new [] { voiceBuilder() };
			}
		}


		public void Init (double sampleRate) {
			foreach (var voice in Voices)
				voice.Init(sampleRate);
		}

		public void Reset () {
			foreach (var voice in Voices)
				voice.Reset();
		}


		public double RenderLeft (bool flag) {
			var sample = 0.0;

			foreach (var voice in Voices)
				if (voice.LeftComponent.IsActive)
					sample += voice.RenderLeft(flag);

			return sample;
		}

		public double RenderRight (bool flag) {
			var sample = 0.0;

			foreach (var voice in Voices)
				if (voice.LeftComponent.IsActive)
					sample += voice.RenderRight(flag);

			return sample;
		}


		public void MidiEventHandler (MidiEvent midiEvent) {
			if ((midiEvent.MidiTrack & Track) == midiEvent.MidiTrack &&
			    (midiEvent.MidiChannel & Channel) == midiEvent.MidiChannel) {
				switch (midiEvent.Type) {
				case MidiEventType.NoteOn:
					foreach (var voice in Voices)
						if (!(Polyphony > 0 && voice.LeftComponent.IsActive)) {
							// var voicePan = Pan + (Width == 0 ? 0 : ((note % 12) - 6.0) / 6.0 * Width);
							var voicePan = Pan + (Width == 0 ? 0 : (midiEvent.Note - 60.0) / 60.0 * Width);
							voice.LeftGain = gain * (0.5 - voicePan * 0.5) * SynthTable.Velc2Gain[midiEvent.Velocity];
							voice.RightGain = gain * (0.5 + voicePan * 0.5) * SynthTable.Velc2Gain[midiEvent.Velocity];
							voice.NoteOn(midiEvent.Note, midiEvent.Velocity);
							break;
						}
					break;
				case MidiEventType.NoteOff:
					foreach (var voice in Voices)
						if (voice.LeftComponent.IsOn && voice.LeftComponent.Note == midiEvent.Note) {
							voice.NoteOff(midiEvent.Note, midiEvent.Velocity);
							break;
						}
					break;
//				case MidiEventType.Controller:
//					switch (midiEvent.Controller) {
//					case MidiControllerType.Sustain:
//						foreach (var voice in Voices)
//							if (voice.LeftComponent.IsOn) {
//								voice.LeftGain = leftGain * SynthTable.Velc2Gain[midiEvent.Value];
//								voice.RightGain = rightGain * SynthTable.Velc2Gain[midiEvent.Value];
//							}
//						break;
//					}
//					break;
				}
			}
		}
	}
}