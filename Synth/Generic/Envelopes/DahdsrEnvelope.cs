using System;

namespace Midif.Synth.Generic {
	public class DahdsrEnvelope : DahdsrEnvelopeGenerator {
		public ISignalProvider Source;


		public override void Init (double sampleRate) {
			Source.Init(sampleRate);

			base.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			Source.NoteOn(note, velocity);

			base.NoteOn(note, velocity);
		}

		public override void NoteOff (byte velocity) {
			Source.NoteOff(velocity);

			base.NoteOff(velocity);
		}

		public override bool IsActive () {
			return base.IsActive() || Source.IsActive(); 
		}


		public override double Render () {
			return Source == null ? base.Render() : Source.Render(flag) * base.Render();
		}
	}
}