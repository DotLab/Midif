using System;

namespace Midif.Synth.Dx7 {
	public sealed class Dx7Operator : Dx7Component {
		#region Const

		// http://www.chipple.net/dx7/fig09-4.gif
		const double OCTAVE_1024 = 1.0006771307;
		//		static readonly double OCTAVE_1024 = Math.Exp(Math.Log(2) / 1024);
	
		static readonly double[] VelocityAdjustTable =
			{
				-99.0,    -10.295511, -9.709229, -9.372207,
				-9.121093, -8.629703, -8.441805, -8.205647,
				-7.810857, -7.653259, -7.299901, -7.242308,
				-6.934396, -6.727051, -6.594723, -6.427755,
				-6.275133, -6.015212, -5.843023, -5.828787,
				-5.725659, -5.443202, -5.421110, -5.222133,
				-5.160615, -5.038265, -4.948225, -4.812105,
				-4.632120, -4.511531, -4.488645, -4.370043,
				-4.370610, -4.058591, -4.066902, -3.952988,
				-3.909686, -3.810096, -3.691883, -3.621306,
				-3.527286, -3.437519, -3.373512, -3.339195,
				-3.195983, -3.167622, -3.094788, -2.984045,
				-2.937463, -2.890713, -2.890660, -2.691874,
				-2.649229, -2.544696, -2.498147, -2.462573,
				-2.396637, -2.399795, -2.236338, -2.217625,
				-2.158336, -2.135569, -1.978521, -1.913965,
				-1.937082, -1.752275, -1.704013, -1.640514,
				-1.598791, -1.553859, -1.512187, -1.448088,
				-1.450443, -1.220567, -1.182340, -1.123139,
				-1.098469, -1.020642, -0.973039, -0.933279,
				-0.938035, -0.757380, -0.740860, -0.669721,
				-0.681526, -0.555390, -0.519321, -0.509318,
				-0.456936, -0.460622, -0.290578, -0.264393,
				-0.252716, -0.194141, -0.153566, -0.067842,
				-0.033402, -0.054947,  0.012860,  0.000000,
				-0.009715,  0.236054,  0.273956,  0.271968,
				0.330177,  0.345427,  0.352333,  0.433861,
				0.442952,  0.476411,  0.539632,  0.525355,
				0.526115,  0.707022,  0.701551,  0.734875,
				0.739149,  0.794320,  0.801578,  0.814225,
				0.818939,  0.897102,  0.895082,  0.927998,
				0.929797,  0.956112,  0.956789,  0.958121
			};

		#endregion

		public bool Enabled = true;

		public MidiComponent Envelope;
		public MidiComponent Lfo;

		public int LfoAmpModSens;
		public int LfoAmpModDepth;

		public int LfoPitchModSens;
		public int LfoPitchModDepth;

		public int Detune;
		public int Transpose;
		/// <summary>
		/// Fix frequency when > 0.
		/// </summary>
		public double FixedFrequency = -1;
		public double FrequencyRatio = 1;
		public double ControllerModVal;

		public double OutputLevel = 1;
		public double VelocitySens = 7;
		public double outputGain;

		public double AmplitudeLeft = 1;
		public double AmplitudeRight = 1;

		public double Modulation;
		public float[] ModBuffer;

		public double FeedbackRatio;
		double feedback;

		double phase = float.MaxValue;
		double phaseStep;

		static double lfoAmpGain;
		static double lfoPitchGain;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			Envelope.Init(sampleRate);
			Lfo.Init(sampleRate);

//			lfoAmpGain = 
//				1 + Dx7Lfo.LfoAmpModSensTable[LfoAmpModSens] * (ControllerModVal + LfoAmpModDepth / 99.0);
			lfoAmpGain = 
				(double)LfoAmpModSens / 3.0 * (ControllerModVal + (double)LfoAmpModDepth / 99.0);
//			DebugConsole.WriteLine(lfoAmpGain);
			lfoPitchGain = 
				1 + Dx7Lfo.LfoPitchModSensTable[LfoPitchModSens] * (ControllerModVal + (double)LfoPitchModDepth / 99.0);
		}

		public override void NoteOn (byte note, byte velocity) {
			note = (byte)(note + Transpose);

			phase = 0;
			feedback = 0;

			double freq;

			if (FixedFrequency > 0)
				freq = FixedFrequency;
			else
				freq = SynthTable.Note2Freq[note] * FrequencyRatio * Math.Pow(OCTAVE_1024, Detune);
//			DebugConsole.WriteLine(FrequencyRatio);

			phaseStep = Pi2 * freq * SampleRateRecip;

//			outputGain = Dx7Envelope.ScaledLevel2Gain[(Dx7Envelope.Level2ScaledLevel[(int)(OutputLevel + VelocityAdjustTable[velocity] * VelocitySens)] << 5) - 240];
			outputGain = OutputLevel * (1 + ((double)velocity / 99 - 1) * ((double)VelocitySens / 7));

			Envelope.NoteOn(note, velocity);
			Lfo.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			Envelope.NoteOff(note, velocity);
			Lfo.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return Envelope.IsFinished();
		}

		public override double Render (bool flag) {
			if (RenderFlag ^ flag) {
				RenderFlag = flag;

				var lfo = Lfo.Render(flag);

				RenderCache = Envelope.Render(flag) * Math.Sin(phase + feedback + Modulation) * (1 - (lfo + 1) * lfoAmpGain * 0.5);
				feedback = RenderCache * FeedbackRatio;

				phase += phaseStep * Math.Pow(lfoPitchGain, lfo);
				if (phase >= Pi2) phase -= Pi2;

				RenderCache *= outputGain;

				return RenderCache;
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			var lfoTemp = BufferControl.RequestBuffer();
			Lfo.Process(lfoTemp);
			Envelope.Process(buffer);

			if (ModBuffer != null)
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] *= (float)(Math.Sin(phase + feedback + ModBuffer[i]) * (1 - (lfoTemp[i] + 1) * lfoAmpGain * 0.5));
					feedback = buffer[i] * FeedbackRatio;

					phase += phaseStep * Math.Pow(lfoPitchGain, lfoTemp[i]);
					if (phase >= Pi2) phase -= Pi2;

					buffer[i] *= (float)outputGain;
				}
			else
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] *= (float)(Math.Sin(phase + feedback) * (1 - (lfoTemp[i] + 1) * lfoAmpGain * 0.5));
					feedback = buffer[i] * FeedbackRatio;

					phase += phaseStep * Math.Pow(lfoPitchGain, lfoTemp[i]);
					if (phase >= Pi2) phase -= Pi2;

					buffer[i] *= (float)outputGain;
				}
			
			BufferControl.FreeBuffer(lfoTemp);
		}
	}
}