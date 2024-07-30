namespace PayPal.REST.Models.PaymentSources;

public interface IPaymentSource
{

}

public interface IToken
{
    public string Id { get; set; }

    public TokenType Type { get; set; }

}

public enum TokenType
{
    BillingAgreement
}

public interface IPayPal : IPaymentSource
{
    public IExperienceContext ExperienceContext { get; set; }

    public string BillingAgreementId { get; set; }

    public string VaultId { get; set; }

    public string EmailAddress { get; set; }

    public IPayPalName Name { get; set; }

    public IPhone Phone { get; set; }

    public DateOnly BirthDate { get; set; }

    public ITaxInfo TaxInfo { get; set; }
}

public interface ITaxInfo
{
    public string TaxId { get; set; }

    public TaxIdType Type { get; set; }
}

public enum TaxIdType
{
    /// <summary>
    /// Individual tax
    /// </summary>
    BrCpf,
    /// <summary>
    /// Business tax
    /// </summary>
    BrCnpj
}

public interface ICreditCard : IPaymentSource
{

}

public interface IExperienceContext
{
    public string BrandName { get; set; }

    public ShippingPreference ShippingPreference { get; set; }

    public LandingPage LandingPage { get; set; }

    public UserAction UserAction { get; set; }

    public PaymentMethodPreference PaymentMethodPreference { get; set; }

    public string Locale { get; set; }

    public string ReturnUrl { get; set; }

    public string CancelUrl { get; set; }
}

public enum ShippingPreference
{
    GetFromFile,
    NoShipping,
    SetProvidedAddress
}

public enum LandingPage
{
    Login,
    GuestCheckout,
    NoPreference
}

public enum UserAction
{
    Continue,
    PayNow
}

public enum PaymentMethodPreference
{
    Unrestricted,
    ImmediatePaymentRequired
}

public interface IPhone
{
    public PhoneType Type { get; set; }

    public IPhoneNumber PhoneNumber { get; set; }
}

public interface IPayPalName
{
    public string GivenName { get; set; }

    public string Surname { get; set; }
}

public enum PhoneType
{
    Fax,
    Home,
    Mobile,
    Other,
    Pager
}

public interface IPhoneNumber
{
    public string NationalNumber { get; set; }
}

public interface IAddress
{
    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string AdminArea2 { get; set; }

    public string AdminArea1 { get; set; }

    public string PostalCode { get; set; }

    public string CountryCode { get; set; }

}