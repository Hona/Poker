using PokerLibrary.Networking.Models;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PokerLibrary.Networking
{
    public class NetworkDataService
    {
        private NetworkStream _networkStream;
        private BinaryReader _binaryReader;
        private BinaryWriter _binaryWriter;

        public NetworkDataService(NetworkStream stream)
        {
            _networkStream = stream;
            // 5 minutes to miliseconds
            _networkStream.ReadTimeout = -1;
            _networkStream.WriteTimeout = -1;
            _binaryReader = new BinaryReader(_networkStream, Encoding.Default);
            _binaryWriter = new BinaryWriter(_networkStream, Encoding.Default);
        }
        public void WriteObject<T>(T input)
        {
            ConsoleService.LogDebug($"Writing Object ({input.GetType().Name})");
            WriteString(JsonService.Serialize(input));
        }
        public void Write(ClientGamePhase input)
        {
            ConsoleService.LogDebug($"Writing GamePhase ({input.ToString()})");
            _binaryWriter.Write((int)input);
        }
        public void WriteString(string input)
        {
            ConsoleService.LogDebug($"Writing String ({input})");
            _binaryWriter.Write(input);
        }
        public void WriteInteger(int input)
        {
            ConsoleService.LogDebug($"Writing Integer ({input})");
            _binaryWriter.Write(input);
        }
        public T ReadObject<T>()
        {
            var json = ReadString();
            var obj = JsonService.Deserialize<T>(json);
            ConsoleService.LogDebug($"Reading Object ({obj.GetType().Name})");
            return obj;
        }
        public ClientGamePhase Read()
        {
            var obj = (ClientGamePhase)_binaryReader.ReadInt32();
            ConsoleService.LogDebug($"Reading ClientGamePhase ({obj.ToString()})");
            return obj;

        }
        public string ReadString()
        {
            var obj = _binaryReader.ReadString();
            ConsoleService.LogDebug($"Reading String ({obj})");
            return obj;
        }
        public int ReadInteger()
        {
            var obj = _binaryReader.ReadInt32();
            ConsoleService.LogDebug($"Reading Integer ({obj})");
            return obj;
        }

    }
}
