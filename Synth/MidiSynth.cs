using System.Collections.Generic;

namespace Midif.Synth {
	/// <summary>
	/// Midi Synthesizer represent a synthesizer model in a mixer.
	/// It offers control over the placement and sequencing of the synthesizer.
	/// </summary>
	[System.Serializable]
	public class MidiSynth {
		public MidiVoiceDelegate VoiceBuilder;

		public bool DynamicPolyphony = true;
		public bool FlushOldestVoice = true;

		public List<MidiVoice> Voices = new List<MidiVoice>();
		public int Count;

		public Queue<MidiVoice> SustainedVoices = new Queue<MidiVoice>();

		public MidiTrack Track = MidiTrack.All;
		public MidiChannel Channel = MidiChannel.All;

		public double Gain = 1;
		public double Expression = 1;

		public double Pan;
		public double Width = 0.5;

		public bool Sustain;

		public byte Velocity;
		public bool VelocityIsPercentage;


		/// <summary>
		/// Initializes a new instance of the<see cref="Midif.Synth.MidiSynth"/>class.
		/// </summary>
		/// <param name="voiceBuilder">Voice builder.</param>
		/// <param name="polyphony">Initial voice count. A negative polyphony means fixed polyphony.</param>
		public MidiSynth (MidiVoiceDelegate voiceBuilder, int polyphony = 4) {
			VoiceBuilder = voiceBuilder;

			if (polyphony < 0) {
				DynamicPolyphony = false;
				polyphony = -polyphony;
			}

			while (polyphony-- > 0)
				Voices.Add(voiceBuilder());
			
			Count = Voices.Count;
		}


		public void Init (double sampleRate) {
			foreach (var voice in Voices)
				voice.Init(sampleRate);
		}

		public void MidiEventHandler (MidiEvent midiEvent) {
			if (midiEvent.Type == MidiEventType.Controller) {
				
				if (midiEvent.Controller == MidiControllerType.MainVolume) {
//					Gain = SynthTable.Velc2Gain[midiEvent.Value];

				} else if (midiEvent.Controller == MidiControllerType.ExpressionController) {
					Expression = SynthTable.Pcnt2Gain[midiEvent.Value];
			
				} else if (midiEvent.Controller == MidiControllerType.Pan) {
//					Pan = SynthTable.Expr2Pcnt[midiEvent.Value] * 2 - 1;

				} else if (midiEvent.Controller == MidiControllerType.Sustain) {
					if (Sustain && midiEvent.Value < 0x40) {
						// Sustain pedal off;
						while (SustainedVoices.Count > 0)
							SustainedVoices.Dequeue().NoteOff(0, 0);
					}

					Sustain = midiEvent.Value >= 0x40;
//					DebugConsole.WriteLine("Sustain: " + (Sustain ? "On" : "Off"));
				}
			}
		}

		public void SetLevel (double level) {
			Gain = SynthTable.Deci2Gain(level);
		}
	}
}