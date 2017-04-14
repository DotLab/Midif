using System.Collections.Generic;

namespace Midif.Synth {
	[System.Serializable]
	public class MidiMix {
		public List<MidiSynth> Synths = new List<MidiSynth>();
		public List<MidiVoice> Voices = new List<MidiVoice>();
		public int Count, ActiveCount;
		public bool VoiceListDirty;

		public double SampleRate;

		public void Init (double sampleRate) {
			SampleRate = sampleRate;

			Voices.Clear();

			foreach (var synth in Synths) {
				synth.Init(sampleRate);
				Voices.AddRange(synth.Voices);
			}

			Count = Voices.Count;
		}

		public void NoteOn (MidiTrack track, MidiChannel channel, byte note, byte velocity, MidiEvent midiEvent) {
			foreach (var synth in Synths) {
				if ((track & synth.Track) != track || (channel & synth.Channel) != channel)
					continue;

				// Try to find a finished voice while keep track of the lowest sounding voice;
				bool targetFinished = false;
				MidiVoice target = null;
				for (int i = 0; i < synth.Count; i++) {
					if (synth.Voices[i].Finished) {
						targetFinished = true;
						target = synth.Voices[i];
						break;
					}

					if (target == null || synth.Voices[i].Event.Tick < target.Event.Tick) // Choose oldest voice
						target = synth.Voices[i];
				}

				// If dynamic polyphony and no finished voice, add a new voice;
				if (!targetFinished && synth.DynamicPolyphony) {
					UnityEngine.Debug.Log("New Note:" + Voices.Count + " + 1");

					target = synth.VoiceBuilder();
					target.Init(SampleRate);

					synth.Voices.Add(target);
					synth.Count++;

					Voices.Insert(0, target);
					Count++;
				}

//				target.Pan = synth.Pan + synth.Width * ((note % 12) - 6.0) / 6.0;
				target.Pan = synth.Pan + synth.Width * (note - 69.0) / 64.0;

				if (synth.Velocity != 0) velocity = synth.Velocity;

				double velcGain = 1;
				if (!synth.IgnoreVelocity)
					velcGain = synth.VelocityIsPercentage ? SynthTable.Pcnt2Gain[velocity] : SynthTable.Velc2Gain[velocity];

				var ampLeft = System.Math.Cos(System.Math.PI / 2 * (target.Pan + 2) / 4);
				var ampRight = System.Math.Sin(System.Math.PI / 2 * (target.Pan + 2) / 4);

//				ampLeft = 0.5 - target.Pan * 0.5;
//				ampRight = 0.5 + target.Pan * 0.5;

				target.LeftGain = synth.Gain * synth.Expression * ampLeft * velcGain;
				target.RightGain = synth.Gain * synth.Expression * ampRight * velcGain;

				target.Event = midiEvent;
				target.NoteOn(note, velocity);
			}
		}

		public void NoteOff (MidiTrack track, MidiChannel channel, byte note, byte velocity) {
			foreach (var synth in Synths) {
				if ((track & synth.Track) != track || (channel & synth.Channel) != channel)
					continue;

				// NoteOff all voices with the target Note;
				for (int i = 0; i < synth.Count; i++)
					if (synth.Voices[i].Note == note && synth.Voices[i].IsOn && !synth.Voices[i].Sustained) {
						if (synth.Sustain) {
							synth.Voices[i].Sustained = true;
							synth.SustainedVoices.Enqueue(synth.Voices[i]);
						} else
							synth.Voices[i].NoteOff(note, velocity);
						
						break;
					}
			}

			VoiceListDirty = true;
		}

		// Move active voices to the front;
		public void SortVoiceList () {
//			var s = new System.Text.StringBuilder();
//			s.Append("-----------------------------------------------------------\n");
//			foreach (var v in Voices) {
//				s.Append(v + "\n");
//			}
//			s.Append("-----------------------------------------------------------\n");

			int i = 0, j = Count - 1;
			while (i < j) {
				while (i < Count && !Voices[i].IsFinished()) i++;
				while (i < j && Voices[j].IsFinished()) j--;

				if (i >= j)
					break;
				
				var temp = Voices[i];
				Voices[i] = Voices[j];
				Voices[j] = temp;
			}

			ActiveCount = i;

			// If all Active voices are IsOn, do not need SortVoiceList() for a while;
			VoiceListDirty = false;
			for (i = 0; i < ActiveCount; i++)
				if (!Voices[i].IsOn) {
					VoiceListDirty = true;
					break;
				}

//			foreach (var v in Voices) {
//				s.Append(v + "\n");
//			}
//			s.Append(string.Format("-----------------------------------------------------------\nCount: {0}\nActive: {1}", Count, ActiveCount));
//			DebugConsole.WriteLine(s.ToString());
		}


		public double Render (bool flag) {
			var sample = 0.0;

			for (int i = 0; i < ActiveCount; i++)
				sample += Voices[i].Render(flag);

			return sample;
		}

		public double RenderRight (bool flag) {
			var sample = 0.0;

			for (int i = 0; i < ActiveCount; i++)
				sample += Voices[i].RenderRight(flag);
			
			return sample;
		}

		public void Process (float[] buffer) {
			for (int i = 0; i < ActiveCount; i++)
				Voices[i].Process(buffer);
		}


		public void MidiEventHandler (MidiEvent midiEvent) {
			if (midiEvent.Type == MidiEventType.NoteOn)
				NoteOn(midiEvent.MidiTrack, midiEvent.MidiChannel, midiEvent.Note, midiEvent.Velocity, midiEvent);
			else if (midiEvent.Type == MidiEventType.NoteOff)
				NoteOff(midiEvent.MidiTrack, midiEvent.MidiChannel, midiEvent.Note, midiEvent.Velocity);
			else
				foreach (var synth in Synths)
					synth.MidiEventHandler(midiEvent);
		}
	}
}