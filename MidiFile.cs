using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using UnityEngine;

namespace Midif {
    public class MidiFile {
        MidiHeader midiHeader;
		List<MidiEvent> midiEvents = new List<MidiEvent>();

        public MidiHeader MidiHeader {
            get { return midiHeader; }
        }
		public MidiEvent[] MidiEvents {
			get {
				return midiEvents.ToArray();
			}
		}

        //--Public Methods
		public MidiFile (byte[] bytes) {
			Stream midiStream = new MemoryStream(bytes);
			DecodeMidiFromStream(midiStream);
			midiStream.Close();
        }

        public MidiFile (string path) {
			TextAsset midiFile = Resources.Load<TextAsset>(path);
			UiConsole.Log(path);

			Stream midiStream = new MemoryStream(midiFile.bytes);
			DecodeMidiFromStream(midiStream);
			midiStream.Close();
        }

        //--Private Methods
        void DecodeMidiFromStream (Stream stream) {
            byte[] tmp = new byte[4];
            stream.Read(tmp, 0, 4);
            if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MThd") {
				throw new Exception("Lacking MThd identifier");
			}

			int midiFormat;
			int trackCount;
			int division;

            // Read Header Chunck Length
            stream.Read(tmp, 0, 4);
            Array.Reverse(tmp); // Reverse the bytes

            // Read Midi Format
            tmp = new byte[2];
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); // Reverse the bytes
			midiFormat = BitConverter.ToInt16(tmp, 0);

            // Read Track Count
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); // Reverse the bytes
            trackCount = BitConverter.ToInt16(tmp, 0);

            // Read Division
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); // Reverse the bytes
            division = BitConverter.ToInt16(tmp, 0);

			midiHeader = new MidiHeader();
			midiHeader.SetMidiFormat(midiFormat);
			midiHeader.SetMidiTime(division);

			UiConsole.Log(midiHeader.MidiFormat);
			UiConsole.Log(midiHeader.TimeFormat);

            // Read Tacks
			for (int j = 0; j < trackCount; j++) {
                tmp = new byte[4];      //reset the size again
                stream.Read(tmp, 0, 4);
                if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MTrk") {
					throw new Exception("Lacking MTrk identifier");
				}

                stream.Read(tmp, 0, 4);
                Array.Reverse(tmp); //Reverse the bytes
                int trackLength = BitConverter.ToInt32(tmp, 0);

                //Read The Rest of The Track
                tmp = new byte[trackLength];
                stream.Read(tmp, 0, trackLength);

                int index = 0;
                byte prevByte = 0;
                int prevChan = 0;

                uint currentTime = 0;
                while (index < tmp.Length) {
                    UInt16 numberOfbytes = 0;
                    MidiEvent midiEvent = new MidiEvent();
					midiEvent.Track = j;

                    // Delta Time
                    midiEvent.DeltaTime = GetDeltaTime(tmp, index, ref numberOfbytes);
					currentTime += midiEvent.DeltaTime;
                    midiEvent.AbsoluteTime = currentTime;

					index += numberOfbytes;

                    byte statusByte = tmp[index];
                    int channel = GetChannel(statusByte);
                    if (statusByte < 0x80) {
                        statusByte = prevByte;
                        channel = prevChan;
                        index--;
                    }
                    if (statusByte != 0xFF)
                        statusByte &= 0xF0;
                    prevByte = statusByte;
                    prevChan = channel;

                    switch (statusByte) {
                    case 0x80:
                        midiEvent.ChannelEventType = MidiChannelEventType.NoteOff;
                        ++index;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        midiEvent.Parameter1 = tmp[index++];
                        midiEvent.Parameter2 = tmp[index++];
                        midiEvent.Parameters[1] = midiEvent.Parameter1;
                        midiEvent.Parameters[2] = midiEvent.Parameter2;
                        break;
                    case 0x90:
                        midiEvent.ChannelEventType = MidiChannelEventType.NoteOn;
                        ++index;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        midiEvent.Parameter1 = tmp[index++];
                        midiEvent.Parameter2 = tmp[index++];
                        midiEvent.Parameters[1] = midiEvent.Parameter1;
                        midiEvent.Parameters[2] = midiEvent.Parameter2;
                        
                        if (midiEvent.Parameter2 == 0x00) //Setting velocity to 0 is actually just turning the note off.
                            midiEvent.ChannelEventType = MidiChannelEventType.NoteOff;
//                        tracks[j].NoteCount++;
                        break;
                    case 0xA0:
                        midiEvent.ChannelEventType = MidiChannelEventType.NoteAftertouch;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        ++index;
                        midiEvent.Parameter1 = tmp[++index];//note number
                        midiEvent.Parameter2 = tmp[++index];//Amount
                        break;
                    case 0xB0:
                        midiEvent.ChannelEventType = MidiChannelEventType.Controller;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        ++index;
                        midiEvent.Parameter1 = tmp[index++]; //type
                        midiEvent.Parameter2 = tmp[index++]; //value
                        midiEvent.Parameters[1] = midiEvent.Parameter1;
                        midiEvent.Parameters[2] = midiEvent.Parameter2;
                        break;
                    case 0xC0:
                        midiEvent.ChannelEventType = MidiChannelEventType.ProgramChange;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        ++index;
                        midiEvent.Parameter1 = tmp[index++];
                        midiEvent.Parameters[1] = midiEvent.Parameter1;
                        //record which programs are used by the track
//                        if (midiEvent.Channel != 9) {
//                            if (!programs.Contains(midiEvent.Parameter1))
//                                programs.Add(midiEvent.Parameter1);
//                        } else {
//                            if (!drumPrograms.Contains(midiEvent.Parameter1))
//                                drumPrograms.Add(midiEvent.Parameter1);
//                        }
                        break;
                    case 0xD0:
                        midiEvent.ChannelEventType = MidiChannelEventType.ChannelAftertouch;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        ++index;
                        //Amount
                        midiEvent.Parameter1 = tmp[++index];
                        break;
                    case 0xE0:
                        midiEvent.ChannelEventType = MidiChannelEventType.PitchBend;
                        midiEvent.Channel = (byte)channel;
                        midiEvent.Parameters[0] = midiEvent.Channel;
                        ++index;
                        midiEvent.Parameter1 = tmp[++index];
                        midiEvent.Parameter2 = tmp[++index];
                        ushort s = (ushort)midiEvent.Parameter1;
                        s <<= 7;
                        s |= (ushort)midiEvent.Parameter2;
                        midiEvent.Parameters[1] = ((double)s - 8192.0) / 8192.0;
                        break;
                    case 0xFF:
                        statusByte = tmp[++index];
                        switch (statusByte) {
                        case 0x00:
                            midiEvent.MetaEventType = MidiMetaEventType.SequenceNumber; ++index;
                            break;
                        case 0x01:
                            midiEvent.MetaEventType = MidiMetaEventType.TextEvent; ++index;
                            //Get the length of the string
                            midiEvent.Parameter1 = tmp[index++];
                            midiEvent.Parameters[0] = midiEvent.Parameter1;
                            //Set the string in the parameter list
                            midiEvent.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                            break;
                        case 0x02:
                            midiEvent.MetaEventType = MidiMetaEventType.CopyrightNotice; ++index;
                            //Get the length of the string
                            midiEvent.Parameter1 = tmp[index++];
                            midiEvent.Parameters[0] = midiEvent.Parameter1;
                            //Set the string in the parameter list
                            midiEvent.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                            break;
                        case 0x03:
                            midiEvent.MetaEventType = MidiMetaEventType.SequenceOrTrackName; ++index;
                            //Get the length of the string
                            midiEvent.Parameter1 = tmp[index++];
                            midiEvent.Parameters[0] = midiEvent.Parameter1;
                            //Set the string in the parameter list
                            midiEvent.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                            break;
                        case 0x04:
                            midiEvent.MetaEventType = MidiMetaEventType.InstrumentName; ++index;
                            //Set the instrument name
                            midiEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                            index += (int)tmp[index] + 1;
                            break;
                        case 0x05:
                            midiEvent.MetaEventType = MidiMetaEventType.LyricText; ++index;
                            //Set the lyric string
                            midiEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                            index += (int)tmp[index] + 1;
                            break;
                        case 0x06:
                            midiEvent.MetaEventType = MidiMetaEventType.MarkerText; ++index;
                            //Set the marker
                            midiEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                            index += (int)tmp[index] + 1;
                            break;
                        case 0x07:
                            midiEvent.MetaEventType = MidiMetaEventType.CuePoint; ++index;
                            //Set the cue point
                            midiEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                            index += (int)tmp[index] + 1;
                            break;
                        case 0x20:
                            midiEvent.MetaEventType = MidiMetaEventType.MidiChannelPrefixAssignment; index++;
                            //Get the length of the data
                            midiEvent.Parameter1 = tmp[index++];
                            midiEvent.Parameters[0] = midiEvent.Parameter1;
                            //Set the string in the parameter list
                            midiEvent.Parameters[1] = tmp[index++];
                            break;
                        case 0x2F:
                            midiEvent.MetaEventType = MidiMetaEventType.EndOfTrack;
                            index += 2;
                            break;
                        case 0x51:
                            midiEvent.MetaEventType = MidiMetaEventType.Tempo; ++index;
                            //Get the length of the data
                            midiEvent.Parameters[4] = tmp[index++];
                            //Put the data into an array
                            byte[] mS = new byte[4]; for (int i = 0; i < 3; i++) mS[i + 1] = tmp[i + index]; index += 3;
                            //Put it into a readable format
                            byte[] mS2 = new byte[4]; for (int i = 0; i < 4; i++) mS2[3 - i] = mS[i];
                            //Get the value from the array
                            UInt32 Val = BitConverter.ToUInt32(mS2, 0);
                            //Set the value
                            midiEvent.Parameters[0] = Val;
                            break;
                        case 0x54:
                            midiEvent.MetaEventType = MidiMetaEventType.SmpteOffset; ++index;
                            int v = tmp[index++];
                            if (v >= 4)
                                for (int i = 0; i < 4; i++) midiEvent.Parameters[i] = tmp[index++];
                            else
                                for (int i = 0; i < v; i++) midiEvent.Parameters[i] = tmp[index++];
                            for (int i = 4; i < v; i++) index++;
                            break;
                        case 0x58:
                            midiEvent.MetaEventType = MidiMetaEventType.TimeSignature; ++index;
                            int v1 = tmp[index++];
                            if (v1 >= 4)
                                for (int i = 0; i < 4; i++) midiEvent.Parameters[i] = tmp[index++];
                            else
                                for (int i = 0; i < v1; i++) midiEvent.Parameters[i] = tmp[index++];
                            for (int i = 4; i < v1; i++) index++;
                            break;
                        case 0x59:
                            midiEvent.MetaEventType = MidiMetaEventType.KeySignature; ++index;
                            int v2 = tmp[index++];
                            if (v2 >= 4)
                                for (int i = 0; i < 4; i++) midiEvent.Parameters[i] = tmp[index++];
                            else
                                for (int i = 0; i < v2; i++) midiEvent.Parameters[i] = tmp[index++];
                            for (int i = 4; i < v2; i++) index++;
                            break;
                        case 0x7F:
                            //Sequencer specific events
                            midiEvent.MetaEventType = MidiMetaEventType.SequencerSpecificEvent; ++index;
                            //Get the length of the data
                            midiEvent.Parameters[4] = tmp[index++];
                            //Get the byte length
                            byte[] len = new byte[(byte)midiEvent.Parameters[4]];
                            //get the byte info
                            for (int i = 0; i < len.Length; i++) len[i] = tmp[index++];
                            midiEvent.Parameters[0] = len;
                            break;
                        }
                        break;
                    //System Exclusive
					case 0xF0:
						while (tmp[index] != 0xF7)
							index++;
						index++;
						break;
					}
					midiEvents.Add(midiEvent);
                }
            }
        }
        
		int GetChannel (byte statusbyte) {
            statusbyte = (byte)(statusbyte << 4);
            return statusbyte >> 4;
        }
        
		uint GetDeltaTime (byte[] data, int currentIndex, ref UInt16 numOfBytes) {
            int value = 0;
            byte b;
            numOfBytes = 0;
            for (int n = 0; n < 4; n++) {
                b = data[currentIndex++];                
                value <<= 7;
                value += (b & 0x7F);
                ++numOfBytes;
                if ((b & 0x80) == 0) {
                    return (uint)value;
                }
            }
            throw new FormatException("Invalid Var Int");
        }
    }
}
