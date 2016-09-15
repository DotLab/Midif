namespace Midif.Synth {
	public class GenericSynthesizer : MidiSynthesizer {
		public delegate ISignalProvider SignalProviderBuilder ();

		public SignalProviderBuilder BuildSignalProvider;
		public int Polyphony = 4;

		ISignalProvider[] signalProviders;


		public override void Init (double sampleRate) {
			signalProviders = new ISignalProvider[Polyphony];
			for (int i = 0; i < Polyphony; i++) {
				signalProviders[i] = BuildSignalProvider();
				signalProviders[i].Init(sampleRate);
			}
		}


		public override void NoteOn (byte note, byte velocity) {
//			System.Console.WriteLine("Note On " + note);
//			System.Console.ReadLine();
			foreach (var signalProvider in signalProviders)
				if (!signalProvider.IsActive) {
					signalProvider.NoteOn(note, velocity);
					break;
				}
		}

		public override void NoteOff (byte note, byte velocity) {
//			System.Console.WriteLine("Note Off " + note);
//			System.Console.ReadLine();
			foreach (var signalProvider in signalProviders)
				if (signalProvider.IsOn && signalProvider.Note == note) {
					signalProvider.NoteOff(note, velocity);
					break;
				}
		}

		public override double Render () {
			cache = 0;

			// for is faster by 5 ticks
//			foreach (var signalProvider in signalProviders) {
//				if (signalProvider.IsActive)
//					cache += signalProvider.Render();
//			}
			for (int i = 0; i < Polyphony; i++)
				if (signalProviders[i].IsActive)
					cache += signalProviders[i].Render();

			return cache;
		}
	}
}