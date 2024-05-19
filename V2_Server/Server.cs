using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LookupBadger.Server
{
    public class Server: BaseScript
    {
        private Dictionary<string, int> _officerNetIdCallsignMap;
        private Dictionary<int, PlayerData> _officerNetIdDataMap;

        public Server() => Init();

        private void Init() 
        {
            _officerNetIdCallsignMap = new Dictionary<string, int>();
            _officerNetIdDataMap = new Dictionary<int, PlayerData>();

            RegisterCommand("lookup", new Action<int, List<object>>((source, args) =>
            {
                Player player = Players[source];

                //malformed command
                if(args.Count != 1)
                {
                    player.TriggerEvent("LookupBadge:Client:MalformedCommand");
                    return;
                }

                if (_officerNetIdCallsignMap.ContainsKey(args[0].ToString()))
                {
                    PlayerData data = _officerNetIdDataMap[_officerNetIdCallsignMap[args[0].ToString()]];
                    player.TriggerEvent("LookupBadge:Client:ReturnLookupData", JsonConvert.SerializeObject(data));
                }
                else
                {
                    player.TriggerEvent("LookupBadge:Client:ReturnEmpty", args[0].ToString());
                }

            }), false);
        }

        [EventHandler("LookupBadge:Server:AddOfficer")]
        private void HandleAddOfficer([FromSource]Player source, string data)
        {
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(data);

            if(!_officerNetIdDataMap.ContainsKey(int.Parse(source.Handle)))
            {
                _officerNetIdCallsignMap.Add(playerData.Callsign, int.Parse(source.Handle));
                _officerNetIdDataMap.Add(int.Parse(source.Handle), playerData);
            }
        }

        [EventHandler("playerDropped")]
        private void HandlePlayerDropped([FromSource]Player source)
        {
            if(_officerNetIdDataMap.ContainsKey(int.Parse(source.Handle)))
            {
                _officerNetIdCallsignMap.Remove(_officerNetIdCallsignMap.SingleOrDefault(kvp => kvp.Value == int.Parse(source.Handle)).Key);                
                _officerNetIdDataMap.Remove(int.Parse(source.Handle));
            }
        }
    }

    public class PlayerData
    {
        public string Dept { get; set; }
        public string Callsign { get; set; }
        public string Rank { get; set; }
        public string Name { get; set; }
        public int NetId { get; set; }
        public PlayerData() { }
    }
}
