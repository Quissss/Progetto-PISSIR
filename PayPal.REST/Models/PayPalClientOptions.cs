﻿namespace PayPal.REST.Models;

public class PayPalClientOptions
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string PayPalUrl { get; set; } = "https://api.paypal.com";
}
