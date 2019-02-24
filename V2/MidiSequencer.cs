﻿using Unsaf;

namespace Midif.V2 {
	public unsafe struct MidiSequencer {
		public MidiFile *file;

		public int *trackLocs;
		public double *trackTicks;

		public double beatsPerSecond;
		public double ticks;

		public static void Init(MidiSequencer *self, MidiFile *file) {
			self->file = file;
			self->trackLocs = (int *)Mem.Malloc(file->trackCount * sizeof(int));
			self->trackTicks = (double *)Mem.Malloc(file->trackCount * sizeof(double));
			Reset(self);
		}

		public static void Reset(MidiSequencer *self) {
			for (int i = 0, count = self->file->trackCount; i < count; i += 1) {
				self->trackLocs[i] = 0;
				self->trackTicks[i] = 0;
			}
			// default tempo 120 bpm
			self->beatsPerSecond = 2;
			self->ticks = 0;
		}

		public static void AdvanceTime(MidiSequencer *self, double time) {
			int EventSize = sizeof(MidiFile.Event);
			MidiFile *file = self->file;

			double ticks = self->ticks += time * self->beatsPerSecond * file->ticksPerBeat;

			for (int i = 0, count = file->trackCount; i < count; i += 1) {
				MidiFile.Event *e = (MidiFile.Event *)(file->tracks[i] + self->trackLocs[i]);
//				Fdb.Dump(e, EventSize + e->length, EventSize);
				while (ticks - self->trackTicks[i] >= e->delta) {
					HandleEvent(i, e);
					self->trackLocs[i] += EventSize + e->length;
					self->trackTicks[i] += e->delta;
					e = (MidiFile.Event *)(file->tracks[i] + self->trackLocs[i]);
				}
//				Fdb.Dump(e, EventSize + e->length, EventSize);
			}
		}

		public static void HandleEvent(int track, MidiFile.Event *e) {
			byte *data = (byte *)(e + 1);
			switch (e->length) {
			case 0:
				UnityEngine.Debug.LogFormat("{0}: 0x{1:X}", track, e->status);
				break;
			case 1:
				UnityEngine.Debug.LogFormat("{0}: 0x{1:X} 0x{2:X}", track, e->status, data[0]);
				break;
			case 2:
				UnityEngine.Debug.LogFormat("{0}: 0x{1:X} 0x{2:X} 0x{3:X}", track, e->status, data[0], data[1]);
				break;
			case 3:
				UnityEngine.Debug.LogFormat("{0}: 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X}", track, e->status, data[0], data[1], data[2]);
				break;
			}
		}

		public static void Free(MidiSequencer *self) {
			Mem.Free(self->trackLocs);
			Mem.Free(self->trackTicks);
		}
	}
}

