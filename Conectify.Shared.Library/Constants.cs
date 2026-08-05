using System;

namespace Conectify.Shared.Library;

public static class Constants
{
    public static class Commands
    {
        public const string ActivityCheck = "activitycheck";
        public const string Active = "active";
        public const string UpdateAvaliable = "update";

        /// <summary>
        /// Probe a service sends to itself through the server to find out whether the websocket
        /// really works. The server routes it back as an echo, see ConnectionWatchdog.
        /// </summary>
        public const string ConnectionCheck = "connectioncheck";
    }

    public static class Events
    {
        public const string All = "all";
        public const string Value = "Value";
        public const string Action = "Action";
        public const string Command = "Command";
        public const string ActionResponse = "ActionResponse";
        public const string CommandResponse = "CommandResponse";
    }

    public static class Metadatas
    {
        public const string Visible = "Visible";
        public static Guid CloudMetadata => Guid.Parse("fd247417-9c50-4108-a8ad-f4899268c706");
        public static Guid IOTypeMetada => Guid.Parse("91ec4f43-c247-4cff-8601-2d9c82df05a5");

        public const string DefaultIOType = "-1";
    }
}
