using System.Collections.Generic;

namespace Midif.Synth {
	public class MidiMixer : IMidiEventHandler {
		public class SynthConfig {
			public MidiChannel Channel = MidiChannel.All;

			public double Level {
				get { return level; }
				set {
					level = value;
					gain = SynthTable.Deci2Gain(level);
					gainL = panL * gain;
					gainR = panR * gain;
				}
			}

			public double Pan {
				get { return pan; }
				set {
					pan = value;
					panL = pan < 0 ? 1 : 1 - pan;
					panR = pan > 0 ? 1 : 1 + pan;
					gainL = panL * gain;
					gainR = panR * gain;
				}
			}

			public double Gain { get { return gain; } }

			public double GainL { get { return gainL; } }

			public double GainR { get { return gainR; } }

			double level, pan, panL, panR;
			double gain, gainL, gainR;

			public SynthConfig (double level = -10, double pan = 0) {
				Level = level;
				Pan = pan;
			}

			public SynthConfig (MidiChannel channel, double level = -10, double pan = 0) {
				Channel = channel;
				Level = level;
				Pan = pan;
			}
		}

		//		public readonly ISynthesizer[] Synthesizers;
		//		public readonly SynthConfig[] Configs;
		public readonly List<ISynthesizer> Synthesizers = new List<ISynthesizer>();
		public readonly List<SynthConfig> Configs = new List<SynthConfig>();

		int capacity;

		double cache;


		public MidiMixer () {
			for (int i = 0; i < capacity; i++) {
				Configs[i] = new SynthConfig();
			}
		}


		public void Init (double sampleRate) {
			capacity = Synthesizers.Count;

			foreach (var synthesizer in Synthesizers)
				synthesizer.Init(sampleRate);
		}


		public void Render (ref double sample) {
			for (int i = 0; i < capacity; i++)
				sample += Synthesizers[i].Render() * Configs[i].Gain;
		}

		public void Render (ref double sampleL, ref double sampleR) {
			for (int i = 0; i < capacity; i++) {
				cache = Synthesizers[i].Render();
				sampleL += cache * Configs[i].GainL;
				sampleR += cache * Configs[i].GainR;
			}
		}


		public void MidiEventHandler (MidiEvent midiEvent) {
			for (int i = 0; i < capacity; i++)
				if (((int)Configs[i].Channel >> midiEvent.Channel & 0x01) == 0x01)
					switch (midiEvent.Type) {
					case MidiEventType.NoteOn:
						UnityEngine.Debug.Log(midiEvent);
						Synthesizers[i].NoteOn(midiEvent.Note, midiEvent.Velocity);
						break;
					case MidiEventType.NoteOff:
						Synthesizers[i].NoteOff(midiEvent.Note, midiEvent.Velocity);
						break;
					}
//			System.Console.WriteLine(midiEvent);
//			System.Console.ReadLine();
		}
	}
}

