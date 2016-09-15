namespace Midif {
	public class MidiNote {
		int track;
		int channel;
		int note;

		uint startSample;
		uint endSample;
		uint duration;
		
		double gain;

		public int Track {
			get { return track; }
		}
		public int Channel {
			get { return channel; }
		}
		public int Note {
			get { return note; }
		}

		public uint StartSample {
			get { return startSample; }
		}
		public uint EndSample {
			get { return endSample; }
		}
		public uint Duration {
			get { return duration; }
		}

		public double Gain {
			get { return gain; }
		}

		
		public MidiNote (int track, int channel, int note, int velocity, uint startSample) {
			this.track = track;
			this.channel = channel;
			this.note = note;
			this.startSample = startSample;
			
			this.gain = (double)velocity / 127.0;
		}

		public void SetEndSample (uint endSample) {
			this.endSample = endSample;
			duration = endSample - startSample;
		}
	}
}