using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using WinAGI.Common;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public class AGIExits : IEnumerable<AGIExit> {
        private readonly Dictionary<string, AGIExit> _exits = new();
        private readonly List<string> _order = new();

        public AGIExit Add(int id, int room, EEReason reason, int style, int transfer = 0, int leg = 0) {
            if (id < 1 || id > 255) {
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be between 1 and 255.");
            }
            string exitId = $"LE{id:000}";
            if (_exits.ContainsKey(exitId)) {
                throw new ArgumentException($"This ExitID({exitId}) is already in use", nameof(exitId));
            }
            // 0 allowed to indicate an error
            if (room < 0 || room > 255) {
                throw new ArgumentOutOfRangeException(nameof(room), "ID must be between 1 and 255.");
            }
            if ((int)reason < 1 || (int)reason > 5) {
                throw new ArgumentOutOfRangeException(nameof(reason), "Invalid reason value.");
            }
            if (style < 0 || style > 1) {
                throw new ArgumentOutOfRangeException(nameof(style), "style must be 0 or 1.");
            }
            // transfer can be any integer
            // leg can be ?
            if (leg < 0 || leg > 255) {
                throw new ArgumentOutOfRangeException(nameof(room), "leg must be between 0 and 255.");
            }
            var exit = new AGIExit {
                ID = exitId,
                Room = room,
                Reason = reason,
                Style = style,
                Transfer = transfer,
                Leg = leg
            };
            _exits.Add(exitId, exit);
            _order.Add(exitId);
            return exit;
        }

        public AGIExit Add(string[] exitdata) {
            // validate argument values
            if (exitdata.Length != 6) {
                throw new ArgumentOutOfRangeException("ExitData must be a six element string array.");
            }
            int id = exitdata[0].IntVal();
            int room = exitdata[1].IntVal();
            EEReason reason = (EEReason)exitdata[2].IntVal();
            int style = exitdata[3].IntVal();
            int transfer = exitdata[4].IntVal();
            int leg = exitdata[5].IntVal();

            return Add(id, room, reason, style, transfer, leg);
        }

        public void Clear() {
            _exits.Clear();
            _order.Clear();
        }

        /// <summary>
        /// Replaces current Exit collection with an array of exits.
        /// </summary>
        /// <param name="exits"></param>
        public void CopyFrom(AGIExit[] exits) {
            // first clear existing collection
            Clear();
            // then add the exits from the array
            foreach (AGIExit exit in exits) {
                // ID is a string, we need the number
                Add(exit.ID[2..].IntVal(), exit.Room, exit.Reason, exit.Style, exit.Transfer, exit.Leg);
            }
        }

        public void CopyFrom(AGIExits exits, bool locations = false) {
            // first clear existing collection
            Clear();
            // then add the exits from the array
            foreach (AGIExit exit in exits) {
                // ID is a string, we need the number
                AGIExit newexit = Add(exit.ID[2..].IntVal(), exit.Room, exit.Reason, exit.Style, exit.Transfer, exit.Leg);
                if (locations) {
                    newexit.EP = exit.EP;
                    newexit.SP = exit.SP;
                }
            }
        }
        public AGIExit this[object index] {
            get {
                if (index is int idx) {
                    if (idx < 0 || idx >= _order.Count)
                        throw new IndexOutOfRangeException("Subscript out of range");
                    return _exits[_order[idx]];
                }
                else if (index is string key) {
                    if (!_exits.TryGetValue(key, out var exit))
                        throw new IndexOutOfRangeException("Subscript out of range");
                    return exit;
                }
                else {
                    throw new ArgumentException("Index must be an integer or string", nameof(index));
                }
            }
        }

        public int Count => _exits.Count;

        public void Remove(object index) {
            if (index is int idx) {
                if (idx < 0 || idx >= _order.Count)
                    throw new IndexOutOfRangeException("Subscript out of range");
                string key = _order[idx];
                _exits.Remove(key);
                _order.RemoveAt(idx);
            }
            else if (index is string key) {
                if (!_exits.ContainsKey(key))
                    throw new IndexOutOfRangeException("Subscript out of range");
                _exits.Remove(key);
                _order.Remove(key);
            }
            else {
                throw new ArgumentException("Index must be an integer or string", nameof(index));
            }
        }

        public IEnumerator<AGIExit> GetEnumerator() {
            foreach (var key in _order)
                yield return _exits[key];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class AGIExit {
        [JsonInclude]
        public string ID;         // matches id number as stored in logic source code comment
        [JsonInclude]
        public int Room;          // 0 = no valid room defined; 1-255 = new room number
        [JsonInclude]
        public int OldRoom;       // original room when an exit is changed; 0 means no change
        [JsonInclude]
        public EEReason Reason = EEReason.erNone;   // erHorizon, erRight, erBottom, erLeft: regular 'edgecode exit'
                                                    // erOther = any other exit
        [JsonInclude]
        public int Style;         // 0 = simple exit; 1 = complex exit
                                  // simple means the new.room cmd immediately follows
                                  // an 'if-then statement; complex means other commands
                                  // are present, or the if-then statement is not easily discerned
                                  //*********
                                  //
                                  // Style is not acutally used for anything right now;
                                  // maybe it will have functionality in later version...
                                  //
                                  //*********
        [JsonInclude]
        public int Transfer;      // identifies transfer points or error points
                                  // if number <0 it is an error point
                                  // if number >0 it is a transfer point
                                  // 0 means no transfer
                                  // only valid in layout editor
        [JsonInclude]
        public int Leg;           // identifies which leg of a transferpoint is associated
                                  // with this exit
        [JsonInclude]
        public EEStatus Status;   // esNew means new exit, not currently in source code
                                  // esOK means existing exit already in source code that is ok
                                  // esDeleted means existing exit already in source code to be deleted
                                  // esChanged means existing edit already in source code to be changed
        [JsonInclude]
        public bool Hidden;       // true means this exit points to a room marked as hidden
                                  // false means this exit points to a room that is not hidden
        public PointF SP;         // used by layout editor to locate start and endpoints of exits to be drawn
        public PointF EP;         // but don't include in json conversion
    }
}