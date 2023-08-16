using CitizenFX.Core;
using CitizenFX.Core.Native;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Server: BaseScript
    {
        private string connString;

        public Server()
        {
            Init();
        }

        private void Init()
        {
            LoadConfig();
            EventHandlers["LookUp:Badge"] += new Action<Player, string>(LookupBadge);
        }
        private void LoadConfig()
        {
            string rawData = API.LoadResourceFile(API.GetCurrentResourceName(), "config.json");
            var config = JObject.Parse(rawData);

            connString = config["connectionString"].ToString();
        }
        private async void LookupBadge([FromSource] Player source, string badgeNumber)
        {
            MySqlConnection conn;
            MySqlCommand cmd;

            conn = new MySqlConnection();
            cmd = new MySqlCommand();

            conn.ConnectionString = connString;

            try
            {
                conn.Open();
                cmd.Connection = conn;

                cmd.CommandText = "SELECT userID FROM fivepd.department_members WHERE callsign = @badge;";
                cmd.Parameters.AddWithValue("@badge", badgeNumber.ToString());
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();

                //Give time for the query to return
                await BaseScript.Delay(50);

                if (reader.HasRows)
                {
                    string id = reader.GetValue(0).ToString();
                    reader.Close();

                    cmd.CommandText = "SELECT gameName FROM fivepd.users WHERE id = @userID;";
                    cmd.Parameters.AddWithValue("@userID", id);

                    reader = cmd.ExecuteReader();
                    reader.Read();

                    await Delay(50);

                    if(reader.HasRows)
                    {
                        source.TriggerEvent("LookUp:Success", reader.GetValue(0).ToString(), badgeNumber.ToString());
                    }
                    else
                    {
                        source.TriggerEvent("LookUp:Failure", badgeNumber.ToString());
                    }
                    reader.Close();
                }
                else
                {
                    source.TriggerEvent("LookUp:Failure", badgeNumber.ToString());
                }
                reader.Close();
            }
            catch(Exception ex)
            {
                source.TriggerEvent("LookUp:Failure", badgeNumber.ToString());

                Debug.WriteLine("");
                Debug.WriteLine("~r~LookUp Error~s~:");
                Debug.Write(ex.Message);
                Debug.WriteLine("");
            }

        }
    }
}
