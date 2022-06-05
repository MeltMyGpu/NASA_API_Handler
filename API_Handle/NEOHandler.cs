﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using APIRequestHandler.JsonWrapper;
using APIRequestHandler.APIFetch;

namespace APIRequestHandler
{
    public class NEOHandler
    {
        private readonly Client _client;
        private NEORootObject? _NEOData;
        private string _APIKey;
        private string? _url;
        private readonly Regex _dateFormatCheck;
        public bool Connected { get; private set; }
        public ILogger Logger { get; set; }


        public NEOHandler(string key)
        {
            _client = new Client();
            _APIKey = key; 
            _dateFormatCheck = new Regex("^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$"); // Is here to ensure the dates passed in are in teh correct format //
            Logger = IloggerFactory.LoggerCreation();
            Logger.LogInformation(String.Format($"Api key set to {key}"));

        }


        private void NullInputCheck(string[] checkList) 
        {
            foreach(string check in checkList)
            {
                if (string.IsNullOrEmpty(check))
                {
                    Logger.LogError(new NullReferenceException(), "Null or empty string passed; ");
                    throw new NullReferenceException();
                }
            }
        }

        public void DisposeOfClient()
        {
            _client.Dispose();
            Logger.LogInformation("HTTP Client disposed; ");
        }

        public void ChangeAPIKey(string key)
        {
            NullInputCheck(new string[] {key});
            _APIKey = key;
            Logger.LogInformation(String.Format($"Api key changed to {key}"));
        }


        public async Task<NEORootObject> GetNEOData(string dateStart, string dateEnd)
        {
            NullInputCheck(new string[] { dateStart, dateEnd });

            if (_dateFormatCheck.IsMatch(dateStart) && _dateFormatCheck.IsMatch(dateEnd))
            {
                _url = string.Format($"https://api.nasa.gov/neo/rest/v1/feed?start_date={dateStart}&end_date={dateEnd}&api_key={_APIKey}");

                try
                {
                    Connected = _client.ConnectionCheck(_url);
                    Logger.LogInformation("Connection check successful;");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Ping error, either URL is incorrect or internet is diconnected; ");
                    throw;
                }

                try
                {
                    _NEOData = await _client.SendAPIRequest(_url);
                    Logger.LogInformation("API Fetch was successful"); // This log entry never hit?!
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, " Error Fetching from API");
                    throw;
                }
            }
            else
            {
                Logger.LogError("Incorrect date format", dateEnd + dateEnd);
                throw new FormatException(dateStart + dateEnd);
            }

            
            return _NEOData; 
        }

                        
    }
}