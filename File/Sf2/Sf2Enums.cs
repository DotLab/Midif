namespace Midif.File.Sf2 {
	// Modulator Source Enumerators
	[System.Serializable]
	public class ModulatorType {
		public SourceType Type;
		public SourcePolarityType Polarity;
		public SourceDirectionType Direction;

		public ControllerSourceType Source;
		public MidiControllerType MidiSource;

		public ModulatorType (System.IO.Stream stream) {
			var data = StreamHelper.ReadUInt16(stream);

			Type = (SourceType)(data >> 10);
			Polarity = (SourcePolarityType)((data >> 9) & 0x01);
			Direction = (SourceDirectionType)((data >> 8) & 0x01);

			if (((data >> 7) & 0x01) == 0)
				Source = (ControllerSourceType)(data & 0x3F);
			else {
				Source = ControllerSourceType.MidiController;
				MidiSource = (MidiControllerType)(data & 0x3F);
			}
		}
	}

	public enum ControllerSourceType {
		NoController = 0,

		NoteOnVelocity = 2,
		NoteOnKeyNumber = 3,
		PolyPressure = 10,
		ChannelPressure = 13,
		PitchWheel = 14,
		PitchWheelSensitivity = 16,

		Link = 127,

		MidiController = 128,
	}

	public enum SourceDirectionType {
		MinToMax,
		MaxToMin
	}

	public enum SourcePolarityType {
		Unipolar,
		Bipolar
	}

	public enum SourceType {
		Linear,
		Concave,
		Convex,
		Switch
	}

	// Generator and Modulator Destination Enumerators
	public enum GeneratorType {
		StartAddressOffset = 0,
		EndAddressOffset = 1,
		StartLoopAddressOffset = 2,
		EndLoopAddressOffset = 3,
		StartAddressCoarseOffset = 4,
		ModulationLFOToPitch = 5,
		VibratoLFOToPitch = 6,
		ModulationEnvelopeToPitch = 7,
		InitialFilterCutoffFrequency = 8,
		InitialFilterQ = 9,
		ModulationLFOToFilterCutoffFrequency = 10,
		ModulationEnvelopeToFilterCutoffFrequency = 11,
		EndAddressCoarseOffset = 12,
		ModulationLFOToVolume = 13,

		Unused1 = 14,

		ChorusEffectsSend = 15,
		ReverbEffectsSend = 16,
		Pan = 17,

		Unused2 = 18,
		Unused3 = 19,
		Unused4 = 20,

		DelayModulationLFO = 21,
		FrequencyModulationLFO = 22,
		DelayVibratoLFO = 23,
		FrequencyVibratoLFO = 24,
		DelayModulationEnvelope = 25,
		AttackModulationEnvelope = 26,
		HoldModulationEnvelope = 27,
		DecayModulationEnvelope = 28,
		SustainModulationEnvelope = 29,
		ReleaseModulationEnvelope = 30,
		KeyNumberToModulationEnvelopeHold = 31,
		KeyNumberToModulationEnvelopeDecay = 32,
		DelayVolumeEnvelope = 33,
		AttackVolumeEnvelope = 34,
		HoldVolumeEnvelope = 35,
		DecayVolumeEnvelope = 36,
		SustainVolumeEnvelope = 37,
		ReleaseVolumeEnvelope = 38,
		KeyNumberToVolumeEnvelopeHold = 39,
		KeyNumberToVolumeEnvelopeDecay = 40,
		Instrument = 41,
		Reserved1 = 42,
		KeyRange = 43,
		VelocityRange = 44,
		StartLoopAddressCoarseOffset = 45,
		KeyNumber = 46,
		Velocity = 47,
		InitialAttenuation = 48,
		Reserved2 = 49,
		EndLoopAddressCoarseOffset = 50,
		CoarseTune = 51,
		FineTune = 52,
		SampleID = 53,
		SampleModes = 54,
		Reserved3 = 55,
		ScaleTuning = 56,
		ExclusiveClass = 57,
		OverridingRootKey = 58,

		Unused5 = 59,
		UnusedEnd = 60
	}

	// Modulator Transform Enumerators
	public enum TransformType {
		Linear = 0,
		AbsoluteValue = 2
	}

	// Sample Link Enumerators
	public enum SampleLinkType {
		MonoSample = 1,
		RightSample = 2,
		LeftSample = 4,
		LinkedSample = 8,

		RomMonoSample = 0x8001,
		RomRightSample = 0x8002,
		RomLeftSample = 0x8004,
		RomLinkedSample = 0x8008
	}
}