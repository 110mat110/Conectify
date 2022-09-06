using Conectify.Database.Models.Values;

namespace Conectify.Services.Library
{
    public delegate void IncomingValueDelegate(Value value);
    public delegate void IncomingActionDelegate(Database.Models.Values.Action action);
    public delegate void IncomingCommandDelegate(Command value);
    public delegate void IncomingActionResponseDelegate(ActionResponse value);
    public delegate void IncomingCommandResponseDelegate(CommandResponse value);
}
