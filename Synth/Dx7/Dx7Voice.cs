namespace Midif.Synth.Dx7 {
	public sealed class Dx7Voice : MidiVoice {
		#region Algorithm

		public static readonly Dx7Algorithm[] Algorithms =
			{
				new Dx7Algorithm("02", "1,,3,4,5,5"),
				new Dx7Algorithm("02", "1,1,3,4,5,"),
				new Dx7Algorithm("03", "1,2,,4,5,5"),
				new Dx7Algorithm("03", "1,2,,4,5,3"),
				new Dx7Algorithm("024", "1,,3,,5,5"),
				new Dx7Algorithm("024", "1,,3,,5,4"),
				new Dx7Algorithm("02", "1,,34,,5,5"),
				new Dx7Algorithm("02", "1,,34,3,5,"),
				new Dx7Algorithm("02", "1,1,34,,5,"),
				new Dx7Algorithm("03", "1,2,2,45,,"),
				new Dx7Algorithm("03", "1,2,,45,,5"),
				new Dx7Algorithm("02", "1,1,345,,,"),
				new Dx7Algorithm("02", "1,,345,,,5"),
				new Dx7Algorithm("02", "1,,3,45,,5"),
				new Dx7Algorithm("02", "1,1,3,45,,"),
				new Dx7Algorithm("0", "124,,3,,5,5"),
				new Dx7Algorithm("0", "124,1,3,,5,"),
				new Dx7Algorithm("0", "123,,2,4,5,"),
				new Dx7Algorithm("034", "1,2,,5,5,5"),
				new Dx7Algorithm("013", "2,2,2,45,,"),
				new Dx7Algorithm("0134", "2,2,2,5,5,"),
				new Dx7Algorithm("0234", "1,,5,5,5,5"),
				new Dx7Algorithm("0134", ",2,,5,5,5"),
				new Dx7Algorithm("01234", ",,5,5,5,5"),
				new Dx7Algorithm("01234", ",,,5,5,5"),
				new Dx7Algorithm("013", ",2,,45,,5"),
				new Dx7Algorithm("013", ",2,2,45,,"),
				new Dx7Algorithm("025", "1,,3,4,4,"),
				new Dx7Algorithm("0124", ",,3,,5,5"),
				new Dx7Algorithm("0125", ",,3,4,4,"),
				new Dx7Algorithm("01234", ",,,,5,5"),
				new Dx7Algorithm("012345", ",,,,,5")
			};

		public class Dx7Algorithm {
			public readonly byte[] OutputMix;
			public readonly byte[][] ModulationMatrix = new byte[6][];

			public Dx7Algorithm (string outputMix, string modulationMatrix) {
				var list = new System.Collections.Generic.List<byte>();
				foreach (var c in outputMix)
					list.Add((byte)(c - '0'));
				OutputMix = list.ToArray();

				list.Clear();
				int i = 0;
				foreach (var c in modulationMatrix) {
					if (c == ',') {
						ModulationMatrix[i] = list.ToArray();
						list.Clear();
						++i;
					} else list.Add((byte)(c - '0'));
				}
				ModulationMatrix[i] = list.ToArray();
			}
		}

		#endregion

		const double PER_VOICE_LEVEL = 0.125;

		public readonly Dx7Operator[] Operators = new Dx7Operator[6];

		public double FeedbackRatio;

		public byte[] OutputMix;
		public byte[][] ModulationMatrix;

		public double RenderCacheRight;

		public Dx7Voice () : base(null, null) {
			for (int i = 0; i < 6; i++)
				Operators[i] = new Dx7Operator();
		}

		public override void Init (double sampleRate) {
			SampleRate = sampleRate;

			for (int i = 0; i < 6; i++) {
				Operators[i].Init(sampleRate);

				for (int j = 0; j < ModulationMatrix[i].Length; j++)
					if (ModulationMatrix[i][j] == i) {
						Operators[i].FeedbackRatio = FeedbackRatio;

						break;
					}
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;
			Note = note;
			Velocity = velocity;

			Active = true;
			Finished = false;

			for (int i = 0; i < 6; i++)
				Operators[i].NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;
			Sustained = false;

			for (int i = 0; i < 6; i++)
				Operators[i].NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			for (int i = 0; i < OutputMix.Length; i++)
				if (Operators[OutputMix[i]].Enabled && !Operators[OutputMix[i]].IsFinished())
					return false;

			return true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				for (int i = 5; i >= 0; --i) {
					var op = Operators[i];
					if (!op.Enabled) continue;

					op.Modulation = 0;
					for (int j = 0; j < ModulationMatrix[i].Length; j++) {
						var mod = Operators[ModulationMatrix[i][j]];
						if (op == mod || !mod.Enabled) continue;
						 
						op.Modulation += mod.RenderCache;
					}

					op.Render(flag);
				}

				RenderCache = 0;
				RenderCacheRight = 0;
				for (int i = 0; i < OutputMix.Length; i++) {
					var op = Operators[OutputMix[i]];
					if (!op.Enabled) continue;

					RenderCache += op.RenderCache * op.AmplitudeLeft * PER_VOICE_LEVEL / OutputMix.Length;
					RenderCacheRight += op.RenderCache * op.AmplitudeRight * PER_VOICE_LEVEL / OutputMix.Length;
				}

				return  RenderCache * LeftGain;
			}

			return RenderCache * LeftGain;
		}

		public override double RenderRight (bool flag) {
			return RenderCacheRight * RightGain;
		}
	}
}