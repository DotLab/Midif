namespace Midif {
	public enum MidiFormat {
		SingleTrack,
		MultiTrack,
		MultiSong
	}

	public enum MidiTimeFormat {
		PulsesPerQuarterNote,
		FramesPerSecond
	}

	public enum MidiChannelEventType {
		None,

		NoteOn,
		NoteOff,
		NoteAftertouch,

		Controller,
		ProgramChange,
		ChannelAftertouch,
		PitchBend,

		Unknown
	}

	public enum MidiMetaEventType {
		None,

		SequenceNumber,
		TextEvent,
		CopyrightNotice,

		SequenceOrTrackName,
		InstrumentName,

		LyricText,
		MarkerText,

		CuePoint,
		MidiChannelPrefixAssignment,
		EndOfTrack,

		Tempo,
		SmpteOffset,

		TimeSignature,
		KeySignature,

		SequencerSpecificEvent,

		Unknown
	}

	public enum MidiControllerEventType {
		BankSelect,
		Modulation,
		BreathController,
		FootController,
		PortamentoTime,
		DataEntry,
		MainVolume,
		Balance,
		Pan,
		ExpressionController,

		EffectControl1,
		EffectControl2,

		GeneralPurposeController1,
		GeneralPurposeController2,
		GeneralPurposeController3,
		GeneralPurposeController4,

		DamperPedal,
		Portamento,
		Sostenuto,
		SoftPedal,
		LegatoFootswitch,
		Hold2,

		SoundController1,
		SoundController2,
		SoundController3,
		SoundController4,
		SoundController6,
		SoundController7,
		SoundController8,
		SoundController9,
		SoundController10,

		GeneralPurposeController5,
		GeneralPurposeController6,
		GeneralPurposeController7,
		GeneralPurposeController8,

		PortamentoControl,

		Effects1Depth,
		Effects2Depth,
		Effects3Depth,
		Effects4Depth,
		Effects5Depth,

		DataIncrement,
		DataDecrement,

		NonRegisteredParameter,
		RegisteredParameter,

		ResetControllers,

		AllNotesOff,
		OmniModeOn,
		OmniModeOff,

		None,
	}

	public enum MidiInstrumentFamiliy {
		Piano,
		Reed,
		ChromaticPercussion,
		Pipe,
		Organ,
		SynthLead,
		Guitar,
		SynthPad,
		Bass,
		SynthEffects,
		Strings,
		Ethnic,
		Ensemble,
		Percussive,
		Brass,
		SoundEffects,
	}

	public enum MidiInstruments {
		#region Pianos
		/// <summary>Acoustic Grand</summary>
		AcousticGrand = 0,
		/// <summary>Bright Acoustic</summary>
		BrightAcoustic = 1,
		/// <summary>Electric Grand</summary>
		ElectricGrand = 2,
		/// <summary>Honky Tonk</summary>
		HonkyTonk = 3,
		/// <summary>Electric Piano 1</summary>
		ElectricPiano1 = 4,
		/// <summary>Electric Piano 2</summary>
		ElectricPiano2 = 5,
		/// <summary>Harpsichord</summary>
		Harpsichord = 6,
		/// <summary>Clav</summary>
		Clav = 7,
		#endregion
		
		#region Chrome Percussion
		/// <summary>Celesta</summary>
		Celesta = 8,
		/// <summary>Glockenspiel</summary>
		Glockenspiel = 9,
		/// <summary>Music Box</summary>
		MusicBox = 10,
		/// <summary>Vibraphone</summary>
		Vibraphone = 11,
		/// <summary>Marimba</summary>
		Marimba = 12,
		/// <summary>Xylophone</summary>
		Xylophone = 13,
		/// <summary>Tubular Bells</summary>
		TubularBells = 14,
		/// <summary>Dulcimer</summary>
		Dulcimer = 15,
		#endregion
		
		#region Organ
		/// <summary>Drawbar Organ</summary>
		DrawbarOrgan = 16,
		/// <summary>Percussive Organ</summary>
		PercussiveOrgan = 17,
		/// <summary>Rock Organ</summary>
		RockOrgan = 18,
		/// <summary>Church Organ</summary>
		ChurchOrgan = 19,
		/// <summary>Reed Organ</summary>
		ReedOrgan = 20,
		/// <summary>Accoridan</summary>
		Accoridan = 21,
		/// <summary>Harmonica</summary>
		Harmonica = 22,
		/// <summary>Tango Accordian</summary>
		TangoAccordian = 23,
		#endregion
		
		#region Guitar
		/// <summary>Nylon Acoustic Guitar</summary>
		NylonAcousticGuitar = 24,
		/// <summary>Steel Acoustic Guitar</summary>
		SteelAcousticGuitar = 25,
		/// <summary>Jazz Electric Guitar</summary>
		JazzElectricGuitar = 26,
		/// <summary>Clean Electric Guitar</summary>
		CleanElectricGuitar = 27,
		/// <summary>Muted Electric Guitar</summary>
		MutedElectricGuitar = 28,
		/// <summary>Overdriven Guitar</summary>
		OverdrivenGuitar = 29,
		/// <summary>Distortion Guitar</summary>
		DistortionGuitar = 30,
		/// <summary>Guitar Harmonics</summary>
		GuitarHarmonics = 31,
		#endregion
		
		#region Bass
		/// <summary>Acoustic Bass</summary>
		AcousticBass = 32,
		/// <summary>Finger Electric Bass</summary>
		FingerElectricBass = 33,
		/// <summary>Pick Electric Bass</summary>
		PickElectricBass = 34,
		/// <summary>Fretless Bass</summary>
		FretlessBass = 35,
		/// <summary>Slap Bass 1</summary>
		SlapBass1 = 36,
		/// <summary>Slap Bass 2</summary>
		SlapBass2 = 37,
		/// <summary>Synth Bass 1</summary>
		SynthBass1 = 38,
		/// <summary>Synth Bass 2</summary>
		SynthBass2 = 39,
		#endregion
		
		#region Strings
		/// <summary>Violin</summary>
		Violin = 40,
		/// <summary>Viola</summary>
		Viola = 41,
		/// <summary>Cello</summary>
		Cello = 42,
		/// <summary>Contrabass</summary>
		Contrabass = 43,
		/// <summary>Tremolo Strings</summary>
		TremoloStrings = 44,
		/// <summary>Pizzicato Strings</summary>
		PizzicatoStrings = 45,
		/// <summary>Orchestral Strings</summary>
		OrchestralStrings = 46,
		/// <summary>Timpani</summary>
		Timpani = 47,
		#endregion
		
		#region Ensemble
		/// <summary>String Ensemble 1</summary>
		StringEnsemble1 = 48,
		/// <summary>String Ensemble 2</summary>
		StringEnsemble2 = 49,
		/// <summary>Synth Strings 1</summary>
		SynthStrings1 = 50,
		/// <summary>Synth Strings 2</summary>
		SynthStrings2 = 51,
		/// <summary>Choir Aahs</summary>
		ChoirAahs = 52,
		/// <summary>Voice Oohs</summary>
		VoiceOohs = 53,
		/// <summary>Synth Voice</summary>
		SynthVoice = 54,
		/// <summary>Orchestra Hit</summary>
		OrchestraHit = 55,
		#endregion
		
		#region Brass
		/// <summary>Trumpet</summary>
		Trumpet = 56,
		/// <summary>Trombone</summary>
		Trombone = 57,
		/// <summary>Tuba</summary>
		Tuba = 58, 
		/// <summary>Muted Trumpet</summary>
		MutedTrumpet = 59,
		/// <summary>French Horn</summary>
		FrenchHorn = 60,
		/// <summary>Brass Section</summary>
		BrassSection = 61,
		/// <summary>Synth Brass 1</summary>
		SynthBrass1 = 62,
		/// <summary>Synth Brass 2</summary>
		SynthBrass2 = 63,
		#endregion
		
		#region Reed
		/// <summary>Soprano Sax</summary>
		SopranoSax = 64,
		/// <summary>Alto Sax</summary>
		AltoSax = 65,
		/// <summary>Tenor Sax</summary>
		TenorSax = 66,
		/// <summary>Baritone Sax</summary>
		BaritoneSax = 67,
		/// <summary>Oboe</summary>
		Oboe = 68,
		/// <summary>English Horn</summary>
		EnglishHorn = 69,
		/// <summary>Bassoon</summary>
		Bassoon = 70,
		/// <summary>Clarinet</summary>
		Clarinet = 71,
		#endregion
		
		#region Pipe
		/// <summary>Piccolo</summary>
		Piccolo = 72,
		/// <summary>Flute</summary>
		Flute = 73,
		/// <summary>Recorder</summary>
		Recorder = 74,
		/// <summary>Pan Flute</summary>
		PanFlute = 75,
		/// <summary>Blown Bottle</summary>
		BlownBottle = 76,
		/// <summary>Skakuhachi</summary>
		Skakuhachi = 77,
		/// <summary>Whistle</summary>
		Whistle = 78,
		/// <summary>Ocarina</summary>
		Ocarina = 79,
		#endregion
		
		#region Synth Lead
		/// <summary>Square Lead</summary>
		SquareLead = 80,
		/// <summary>Sawtooth Lead</summary>
		SawtoothLead = 81,
		/// <summary>Calliope Lead</summary>
		CalliopeLead = 82,
		/// <summary>Chiff Lead</summary>
		ChiffLead = 83,
		/// <summary>Charang Lead</summary>
		CharangLead = 84,
		/// <summary>Voice Lead</summary>
		VoiceLead = 85,
		/// <summary>Fifths Lead</summary>
		FifthsLead = 86,
		/// <summary>Base Lead</summary>
		BaseLead = 87,
		#endregion
		
		#region Synth Pad
		/// <summary>NewAge Pad</summary>
		NewAgePad = 88,
		/// <summary>Warm Pad</summary>
		WarmPad = 89,
		/// <summary>Polysynth Pad</summary>
		PolysynthPad = 90,
		/// <summary>Choir Pad</summary>
		ChoirPad = 91,
		/// <summary>Bowed Pad</summary>
		BowedPad = 92,
		/// <summary>Metallic Pad</summary>
		MetallicPad = 93,
		/// <summary>Halo Pad</summary>
		HaloPad = 94,
		/// <summary>Sweep Pad</summary>
		SweepPad = 95,
		#endregion
		
		#region Synth Effects
		/// <summary>Rain</summary>
		Rain = 96,
		/// <summary>Soundtrack</summary>
		Soundtrack = 97,
		/// <summary>Crystal</summary>
		Crystal = 98,
		/// <summary>Atmosphere</summary>
		Atmosphere = 99,
		/// <summary>Brightness</summary>
		Brightness = 100,
		/// <summary>Goblin</summary>
		Goblin = 101,
		/// <summary>Echos</summary>
		Echos = 102,
		/// <summary>SciFi</summary>
		SciFi = 103,
		#endregion
		
		#region Ethnic
		/// <summary>Sitar</summary>
		Sitar = 104,
		/// <summary>Banjo</summary>
		Banjo = 105,
		/// <summary>Shamisen</summary>
		Shamisen = 106,
		/// <summary>Koto</summary>
		Koto = 107,
		/// <summary>Kalimba</summary>
		Kalimba = 108,
		/// <summary>Bagpipe</summary>
		Bagpipe = 109,
		/// <summary>Fiddle</summary>
		Fiddle = 110,
		/// <summary>Shanai</summary>
		Shanai = 111,
		#endregion
		
		#region Percussive
		/// <summary>Tinkle Bell</summary>
		TinkleBell = 112,
		/// <summary>Agogo</summary>
		Agogo = 113,
		/// <summary>Steel Drums</summary>
		SteelDrums = 114,
		/// <summary>Woodblock</summary>
		Woodblock = 115,
		/// <summary>TaikoD rum</summary>
		TaikoDrum = 116,
		/// <summary>Melodic Tom</summary>
		MelodicTom = 117,
		/// <summary>Synth Drum</summary>
		SynthDrum = 118,
		/// <summary>Reverse Cymbal</summary>
		ReverseCymbal = 119,
		#endregion
		
		#region Sound Effects
		/// <summary>Guitar Fret Noise</summary>
		GuitarFretNoise = 120,
		/// <summary>Breath Noise</summary>
		BreathNoise = 121,
		/// <summary>Seashore</summary>
		Seashore = 122,
		/// <summary>Bird Tweet</summary>
		BirdTweet = 123,
		/// <summary>Telephone Ring</summary>
		TelephoneRing = 124,
		/// <summary>Helicopter</summary>
		Helicopter = 125,
		/// <summary>Applause</summary>
		Applause = 126,
		/// <summary>Gunshot</summary>
		Gunshot = 127
		#endregion
	}
	
	/// <summary>General MIDI Percussion Patch Map.</summary>
	public enum MidiPercussion {
		#region Sounds
		/// <summary>Bass Drum</summary>
		BassDrum = 35,
		/// <summary>Bass Drum 1</summary>
		BassDrum1 = 36,
		/// <summary>Side Stick</summary>
		SideStick = 37,
		/// <summary>Acoustic Snare</summary>
		AcousticSnare = 38,
		/// <summary>Hand Clap</summary>
		HandClap = 39,
		/// <summary>Electric Snare</summary>
		ElectricSnare = 40,
		/// <summary>Low Floor Tom</summary>
		LowFloorTom = 41,
		/// <summary>Closed Hi Hat</summary>
		ClosedHiHat = 42,
		/// <summary>High Floor Tom</summary>
		HighFloorTom = 43,
		/// <summary>Pedal Hi Hat</summary>
		PedalHiHat = 44,
		/// <summary>Low Tom</summary>
		LowTom = 45,
		/// <summary>Open Hi Hat</summary>
		OpenHiHat = 46,
		/// <summary>Low Mid Tom</summary>
		LowMidTom = 47,
		/// <summary>Hi Mid Tom</summary>
		HiMidTom = 48,
		/// <summary>Crash Cymbal 1</summary>
		CrashCymbal1 = 49,
		/// <summary>High Tom</summary>
		HighTom = 50,
		/// <summary>Ride Cymbal</summary>
		RideCymbal = 51,
		/// <summary>Chinese Cymbal</summary>
		ChineseCymbal = 52,
		/// <summary>Ride Bell</summary>
		RideBell = 53,
		/// <summary>Tambourine</summary>
		Tambourine = 54,
		/// <summary>Splash Cymbal</summary>
		SplashCymbal = 55,
		/// <summary>Cowbell</summary>
		Cowbell = 56,
		/// <summary>Crash Cymbal 2</summary>
		CrashCymbal2 = 57,
		/// <summary>Vibraslap</summary>
		Vibraslap = 58,
		/// <summary>Ride Cymbal 2</summary>
		RideCymbal2 = 59,
		/// <summary>Hi Bongo</summary>
		HiBongo = 60,
		/// <summary>Low Bongo</summary>
		LowBongo = 61,
		/// <summary>Mute Hi Conga</summary>
		MuteHiConga = 62,
		/// <summary>Open Hi Conga</summary>
		OpenHiConga = 63,
		/// <summary>Low Conga</summary>
		LowConga = 64,
		/// <summary>High Timbale</summary>
		HighTimbale = 65,
		/// <summary>Low Timbale</summary>
		LowTimbale = 66,
		/// <summary>High Agogo</summary>
		HighAgogo = 67,
		/// <summary>Low Agogo</summary>
		LowAgogo = 68,
		/// <summary>Cabasa</summary>
		Cabasa = 69,
		/// <summary>Maracas</summary>
		Maracas = 70,
		/// <summary>Short Whistle</summary>
		ShortWhistle = 71,
		/// <summary>Long Whistle</summary>
		LongWhistle = 72,
		/// <summary>Short Guiro</summary>
		ShortGuiro = 73,
		/// <summary>Long Guiro</summary>
		LongGuiro = 74,
		/// <summary>Claves</summary>
		Claves = 75,
		/// <summary>Hi Wood Block</summary>
		HiWoodBlock = 76,
		/// <summary>Low Wood Block</summary>
		LowWoodBlock = 77,
		/// <summary>Mute Cuica</summary>
		MuteCuica = 78,
		/// <summary>Open Cuica</summary>
		OpenCuica = 79,
		/// <summary>Mute Triangle</summary>
		MuteTriangle = 80,
		/// <summary>Open Triangle</summary>
		OpenTriangle = 81
		#endregion
	}
}
