namespace Midif {
	public class MidiEvent {
		public int Track;

		public uint DeltaTime;
		public uint AbsoluteTime;

		// Set by MidiSequence
		public uint SampleTime;

		public MidiMetaEventType MetaEventType;
		public MidiChannelEventType ChannelEventType;
		
		public object[] Parameters;
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

		//--Public Methods
		public MidiEvent () {
			this.Parameters = new object[5];
			this.MetaEventType = MidiMetaEventType.None;
			this.ChannelEventType = MidiChannelEventType.None;
		}
		
		public bool IsMetaEvent () {
			return ChannelEventType == MidiChannelEventType.None;
		}
		
		public bool IsChannelEvent () {
			return MetaEventType == MidiMetaEventType.None;
		}
		
		public MidiControllerEventType GetControllerEventType () {
			if (ChannelEventType != MidiChannelEventType.Controller) {
				return MidiControllerEventType.None;
			}

			return (MidiControllerEventType)Parameter1;
		}
	}
}
