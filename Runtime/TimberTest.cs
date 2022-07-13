using System;
using UnityEngine;

namespace PeartreeGames.TimberLogs
{
    public class TimberTest : MonoBehaviour
    {
        private void Update()
        {
           this.Timber($"Test {Time.time}");
        }
    }
}