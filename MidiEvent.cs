namespace Midif {
	public class MidiEvent {
		public int Track;

		public uint DeltaTime;
		public uint AbsoluteTime;

		// Set by MidiSequence
		public uint SampleTime;

		public MidiMetaEventType MetaEventType = MidiMetaEventType.None;
		public MidiChannelEventType ChannelEventType = MidiChannelEventType.None;
		
		public object[] Parameters = new object[5];
		public byte Channel {
			get { return (byte)Parameters[0]; }
			set { Parameters[0] = value; }
		}
		public byte Parameter1 {
			get { return (byte)Parameters[1]; }
			set { Parameters[1] = value; }
		}
		public byte Parameter2  {
			get { return (byte)Parameters[2]; }
			set { Parameters[2] = value; }
		}
		
		public bool IsMetaEvent () {
			return MetaEventType != MidiMetaEventType.None;
		}
		
		public bool IsChannelEvent () {
			return ChannelEventType != MidiChannelEventType.None;
		}
		
		public MidiControllerEventType GetControllerEventType () {
			if (ChannelEventType != MidiChannelEventType.Controller) {
				return MidiControllerEventType.None;
			}

			return (MidiControllerEventType)Parameter1;
		}
	}
}
