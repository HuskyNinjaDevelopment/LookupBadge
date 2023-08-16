using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupBadge
{
    internal class Lookup: Plugin
    {
        PlayerData _playerData;

        internal Lookup()
        {
            Init();

            _playerData = Utilities.GetPlayerData();
        }

        private void Init()
        {
            API.RegisterCommand("lookup", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                //Either no input for more than the badge was given
                if(args.Count != 1) { ShowError("input"); return; }

                //Put the badge number into a variable and strip any dashes
                string badge = args[0].ToString();
                if(badge.Contains("-")) { badge = badge.Replace("-", ""); }

                //Validate badge number, checking length and if the first element is a letter or digit, make sure 3 remaining chars are digits
                if(badge.Length > 4) { ShowError("badge"); return; }
                if (!char.IsLetterOrDigit(badge[0])) { ShowError("badge"); return; }
                for(int i = 1; i < badge.Length; i++) //Skip the first element as it has already been validated
                {
                    if (!char.IsDigit(badge[i])) { ShowError("badge"); return; }
                }

                //Valid Badge trigger lookup on server
                TriggerEvent("chat:addMessage", new 
                { 
                    color = new[] {255, 0, 0},
                    multiline = true,
                    args = new[] {"LookUp", $"Getting data on badge {badge}..."}
                });
                await Delay(1500);
                TriggerServerEvent("LookUp:Badge", badge);
            }), false);

            //Dispalys name and badge number
            API.RegisterCommand("badge", new Action(() => 
            {
                TriggerEvent("chat:addMessage", new
                {
                    color = new[] { 255, 0, 0 },
                    multiline = true,
                    args = new[] { "Badge", $"{_playerData.DisplayName} {_playerData.Callsign}" }

                });
            }), false);

            TriggerEvent("chat:addSuggestion", "/lookup", "/lookup [NNNN or L-NNN] will return information on the officer with that callsign.");
            TriggerEvent("chat:addSuggestion", "/badge", "/badge will display the players in game name and callsign in ICC");

            EventHandlers["LookUp:Success"] += new Action<string, string>(LookUpSuccess);
            EventHandlers["LookUp:Failure"] += new Action<string>(LookUpFailure);

            API.RegisterKeyMapping("badge", "Show Name and Callsign", "KEYBOARD", "F11");
        }

        private void LookUpSuccess(string data, string badge)
        {
            ShowNotification("~g~LookUp Success");
            TriggerEvent("chat:addMessage", new 
            { 
                color = new[] {255, 0, 0},
                multiline = true,
                args = new[] {"LookUp", $"The badge {badge}~s~ belongs to {data}."}
            });
        }
        private void LookUpFailure(string badge)
        {
            ShowNotification("~r~LookUp Failed");
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                multiline = true,
                args = new[] { "LookUp", $"~r~Failed~s~ to find information on ~b~{badge}." }
            });
        }

        private void ShowError(string fault)
        {
            switch(fault)
            {
                case "input":
                    ShowNotification("~r~Invalid Input");
                    break;
                case "badge":
                    ShowNotification("~r~Invalid Badge Number");
                    break;
                default:
                    ShowNotification("~r~Error: Lookup Failed");
                    break;
            }
        }
        private void ShowNotification(string msg)
        {
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(msg);
            API.DrawNotification(true, true);
        }
    }
}
