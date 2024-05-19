using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using FivePD.API;
using FivePD.API.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupBadge.FivePD_Plugin
{
    internal class LookupBadge: Plugin
    {
        internal LookupBadge() 
        {
            TriggerEvent("chat:addSuggestion", "/lookup", "Lookup Badge Command Information", new[]
{
                new { name = "Badge Number", help = "Enter a players Badge Number to recieve some information about them."}
            });
        }

        [EventHandler("playerSpawned")]
        private void HandlePlayerSpawned()
        {
            //Get Player Info
            PlayerData data = Utilities.GetPlayerData();

            //Send info to server
            LookupData lookupData = new LookupData()
            {
                Rank = data.Rank,
                Callsign = data.Callsign,
                Dept = data.DepartmentShortName,
                Name = data.DisplayName,
                NetId = Game.Player.ServerId
            };

            TriggerServerEvent("LookupBadge:Server:AddOfficer", JsonConvert.SerializeObject(lookupData));
        }

        [EventHandler("LookupBadge:Client:MalformedCommand")]
        private void HandleMalformedCommand()
        {
            DisplayNotification("~r~Error~s~: Unable to process Badge Lookup", "Badge Lookup", "~r~Malformed Command", icon: 1);
        }

        [EventHandler("LookupBadge:Client:ReturnLookupData")]
        private void HandleReturnLookupData(string data)
        {
            LookupData returnData = JsonConvert.DeserializeObject<LookupData>(data);

            DisplayNotification($"~y~Department~s~: {returnData.Dept}~n~~y~Callsign~s~: {returnData.Callsign}~n~~y~Rank~s~: {returnData.Rank}~n~~y~Name~s~: {returnData.Name}", "Badge Lookup", "~g~Lookup Results", icon: 1, netId: returnData.NetId);
        }

        [EventHandler("LookupBadge:Client:ReturnEmpty")]
        private void HandleNoResultsFound(string data)
        {
            DisplayNotification($"Lookup for badge: [{data}] yielded ~y~0~s~ results.", "Badge Lookup", "~y~Lookup Results", icon: 1);
        }

        private async void DisplayNotification(string message, string sender, string subject, int icon = 4, bool flash = false, int netId = 0)
        {
            List<string> textures = new List<string>() { "DIA_POLICE", "WEB_NATIONALOFFICEOFSECURITYENFORCEMENT", "CHAR_CALL911" };
            string selectedTexture = textures[new Random(GetGameTimer()).Next(0, textures.Count)];

            int headshotHandle = 0;
            string headshotTxd = "";

            if(netId != 0 && NetworkDoesEntityExistWithNetworkId(netId))
            {
                headshotHandle = RegisterPedheadshot(NetworkGetEntityFromNetworkId(netId));
                while(!IsPedheadshotReady(headshotHandle) || !IsPedheadshotValid(headshotHandle)) { await Delay(0); }

                headshotTxd = GetPedheadshotTxdString(headshotHandle);
            }

            BeginTextCommandThefeedPost("STRING");
            AddTextComponentString(message);

            if(headshotTxd != "") 
            {
                EndTextCommandThefeedPostMessagetext(headshotTxd, headshotTxd, flash, icon, sender, subject);
            }
            else
            {
                RequestStreamedTextureDict(selectedTexture, true);
                while (!HasStreamedTextureDictLoaded(selectedTexture)) { await Delay(0); }

                EndTextCommandThefeedPostMessagetext(selectedTexture, selectedTexture, flash, icon, sender, subject);

            }

            EndTextCommandThefeedPostTicker(false, false);

            if(headshotHandle != 0) { UnregisterPedheadshot(headshotHandle); }
        }
    }

    public class LookupData
    {
        public string Name { get; set; }
        public string Dept { get; set; }
        public string Callsign { get; set; }
        public string Rank { get; set; }
        public int NetId { get; set; }

        public LookupData() { }
    }
}
