//namespace Midif.Synth {
//	public sealed class KeymapMixer : MidiComponent {
//		public readonly MidiComponent[] Sources = new MidiComponent[0x80];
//
//		bool disabled = true;
//
//
//		public override void Init (double sampleRate) {
//			foreach (var source in Sources)
//				if (source != null)
//					source.Init(sampleRate);
//		}
//
//
//		public override void NoteOn (byte note, byte velocity) {
//			IsOn = true;
//			Note = note;
//
//			disabled = Sources[note] == null;
//
//			if (!disabled)
//				Sources[note].NoteOn(note, velocity);
//		}
//
//		public override void NoteOff (byte note, byte velocity) {
//			IsOn = false;
//
//			if (!disabled)
//				Sources[Note].NoteOff(Note, velocity);
//		}
//
//		public override bool IsFinished () {
//			return disabled || (!Sources[Note].IsOn && Sources[Note].IsFinished());
//		}
//
//
//		public override double Render (bool flag) {
//			if (flag ^ RenderFlag) {
//				RenderFlag = flag;
//
//				if (disabled) return RenderCache = 0;
//				return RenderCache = Sources[Note].Render(RenderFlag);
//			}
//
//			return RenderCache;
//		}
//	}
//}