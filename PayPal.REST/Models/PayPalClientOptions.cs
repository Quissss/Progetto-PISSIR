using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayPal.REST.Models;

public class PayPalClientOptions
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string PayPalUrl { get; set; } = "https://api.paypal.com";
}
