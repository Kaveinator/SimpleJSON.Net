using System;
using System.Text;

namespace SimpleJSON.Net {
    public class JSONPacket {
        JSONNode PacketData;
        public string eventName {
            get => PacketData["H"];
        }

        public JSONNode this[string index] {
            get => PacketData["B"][index];
            set => PacketData["B"][index] = value;
        }

        public JSONPacket(string eventName) {
            PacketData = new JSONObject();
            PacketData["H"] = eventName;
            PacketData["B"] = new JSONObject();
        }

        public override string ToString()
        {
            return PacketData.ToString();
        }

        public string ToFormattedString() {
            return PacketData.ToFormattedString();
        }

        public byte[] ToByteArray() {
            return Encoding.UTF8.GetBytes(ToString());
        }

        // Parse
        public static JSONPacket Parse(byte[] packetContents) {
            return Parse(Encoding.UTF8.GetString(packetContents));
        }

        public static JSONPacket Parse(string packetContents) {
            try {
                return new JSONPacket(packetContents, true);
            }
            catch { throw new Exception(); }
        }

        JSONPacket(string buffer, bool privateMode) {
            PacketData = JSONNode.Parse(buffer);
        }
    }
}