using System;
using System.Collections.Generic;

namespace Midif.Synthesizer.Dx7 {
	public class Dx7Synthesizer {
		// nominal per-voice level borrowed from Hexter
		const double PER_VOICE_LEVEL = 0.125 / 6;
		// semitones (in each direction)
		const double PITCH_BEND_RANGE = 2;

		const int MIDI_CC_MODULATION = 1;
		const int MIDI_CC_SUSTAIN_PEDAL = 64;

		public List<Dx7Voice> voices;
		int polyphony;
		bool sustainPedalDown;
		Queue<MidiEvent> eventQueue;

		public Dx7Synthesizer (int polyphony) {
			voices = new List<Dx7Voice>();
			this.polyphony = polyphony;
			sustainPedalDown = false;
			eventQueue = new Queue<MidiEvent>();
		}

		public void QueueMidiEvent (MidiEvent ev) {
			eventQueue.Enqueue(ev);
		}

		public void ProcessMidiEvent (MidiEvent ev) {
			var cmd = ev.StatusByte >> 4;
			var channel = ev.StatusByte & 0xf;
			var noteNumber = ev.DataByte1;
			var velocity = ev.DataByte2;

			if (channel == 9) // Ignore drum channel
				return;
			if (cmd == 8 || ((cmd == 9) && (velocity == 0))) { // with MIDI, note on with velocity zero is the same as note off
				NoteOff(noteNumber);
			} else if (cmd == 9) {
				NoteOn(noteNumber, velocity / 99.0); // changed 127 to 99 to incorporate "overdrive"
			} else if (cmd == 10) {
				//this.polyphonicAftertouch(noteNumber, velocity/127);
			} else if (cmd == 11) {
				Controller(noteNumber, velocity / 127);
			} else if (cmd == 12) {
				//this.programChange(noteNumber);
			} else if (cmd == 13) {
				ChannelAftertouch(noteNumber / 127);
			} else if (cmd == 14) {
				PitchBend(((velocity * 128.0 + noteNumber) - 8192) / 8192.0);
			}
		}

		public void Controller (int controlNumber, double value) {
			// see http://www.midi.org/techspecs/midimessages.php#3
			switch (controlNumber) {
			case MIDI_CC_MODULATION:
				Dx7Voice.ModulationWheel(value);
				break;
			case MIDI_CC_SUSTAIN_PEDAL:
				SustainPedal(value > 0.5);
				break;
			}
		}

		public void ChannelAftertouch (double value) {
			Dx7Voice.ChannelAftertouch(value);
		}

		public void SustainPedal (bool down) {
			if (down)
				sustainPedalDown = true;
			else {
				sustainPedalDown = false;
				foreach (var voice in voices)
					if (voice.down) voice.NoteOff();
			}
		}

		public void PitchBend (double value) {
			Dx7Voice.PitchBend(value * PITCH_BEND_RANGE);

			foreach (var voice in voices)
				voice.UpdatePitchBend();
		}


		public void NoteOn (int note, double velocity) {
			var voice = new Dx7Voice(note, velocity);
			if (voices.Count >= polyphony)
				voices.RemoveAt(0);
			voices.Add(voice);
		}

		public void NoteOff (int note) {
			foreach (var voice in voices) {
				if (voice.note == note && voice.down) {
					voice.down = false;
					if (!sustainPedalDown) voice.NoteOff();
					break;
				}
			}
		}

		public void Panic () {
			sustainPedalDown = false;
			foreach (var voice in voices)
				voice.NoteOff();
		}

		public double[] Render () {
			double[] output;
			double outputL = 0;
			double outputR = 0;

			for (int i = 0; i < voices.Count; i++) {
				var voice = voices[i];
				if (voice.IsFinished()) {
					// Clear the note after release
					voices.RemoveAt(i);
					i--; // undo increment
				} else {
					output = voice.Render();

					outputL += output[0];
					outputR += output[1];
				}
			}
			return new [] { outputL * PER_VOICE_LEVEL, outputR * PER_VOICE_LEVEL };
		}
	}
}