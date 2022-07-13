using System.Collections.Generic;
using UnityEngine;

namespace PeartreeGames.TimberLogs 
{
    public class TimberMessages
    {
        public readonly string Name;
        public readonly GameObject GameObject;
        public readonly TimberList<string> Messages;

        private const int MaxCapacity = 2000;
        public string Filter { get; set; }
        public string PreviousFilter { get; set; }
        public TimberMessages(GameObject go)
        {
            GameObject = go;
            Name = go.name;
            Messages = new TimberList<string>(MaxCapacity);
            Filter = string.Empty;
            PreviousFilter = string.Empty;
        }

        public void Add(string msg) => Messages.Add(msg);

    }
}