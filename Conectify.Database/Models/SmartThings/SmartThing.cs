using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Database.Models.SmartThings;
public class SmartThing
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Capability { get; set; } = string.Empty;
}
