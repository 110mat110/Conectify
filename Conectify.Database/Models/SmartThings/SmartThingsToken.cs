using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Database.Models.SmartThings;
public class SmartThingsToken
{
    public int Id { get; set; } // PK
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
