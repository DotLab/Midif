using System.Collections.Generic;

namespace Midif {
	public class InstrumentBank : IInstrumentBank {
		List<IInstrument> instruments;

		public IInstrument[] Instruments {
			get { return instruments.ToArray(); }
		}

		public InstrumentBank () {
			instruments = new List<IInstrument>();
		}

		public void AddInstrument (IInstrument instrument) {
			instruments.Add(instrument);
		}

		public IInstrument GetInstrument (int index) {
			return instruments[index % instruments.Count];
		}

		public void ClearInstruments () {
			instruments.Clear();
		}

		public void RemoveInstrument (int index) {
			instruments.RemoveAt(index);
		}
	}
}