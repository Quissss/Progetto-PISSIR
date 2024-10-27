namespace Progetto.App.Core.Models.Mqtt;

public enum MessageType
{
    RequestCharge, // client → broker
    StartCharging, // broker → client
    CompleteCharge, // client → broker
    UpdateCharging, // client → broker
    UpdateMwBot, // client → broker
    StartRecharge, // broker → client
    RequestRecharge, // client → broker
    DisconnectClient, // client → broker
    ChargeCompleted, // broker → client
    RequestMwBot, // client → broker
    ReturnMwBot, // broker → client
    StopCharging, // broker → client
}

/// <summary>
/// Classe per rappresentare un messaggio MQTT per il client, 
/// contenente informazioni dettagliate sullo stato e la richiesta dell'MwBot.
/// </summary>
public class MqttClientMessage : MwBot
{
    /// <summary>
    /// Tipo di messaggio da inviare via MQTT, che può indicare l'azione da eseguire
    /// o lo stato attuale (es. aggiornamento stato, richiesta di ricarica, ecc.).
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// ID dello slot di parcheggio associato a questo messaggio, se applicabile.
    /// </summary>
    public int? ParkingSlotId { get; set; }

    /// <summary>
    /// Dettagli sullo slot di parcheggio, come posizione o disponibilità,
    /// associati a questa richiesta.
    /// </summary>
    public ParkingSlot? ParkingSlot { get; set; }

    /// <summary>
    /// Percentuale attuale di carica della batteria dell'auto in carica.
    /// </summary>
    public decimal? CurrentCarCharge { get; set; }

    /// <summary>
    /// Percentuale di carica della batteria dell'auto che si desidera raggiungere.
    /// </summary>
    public decimal? TargetBatteryPercentage { get; set; }

    /// <summary>
    /// Identificativo univoco dell'utente associato alla richiesta, 
    /// utilizzato per gestire e monitorare le richieste specifiche dell'utente.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Targa dell'auto che sta richiedendo il servizio o la ricarica.
    /// </summary>
    public string? CarPlate { get; set; }

    /// <summary>
    /// Dettagli dell'attuale processo di ricarica, come stato di carica,
    /// quantità di energia consumata e costo stimato.
    /// </summary>
    public CurrentlyCharging? CurrentlyCharging { get; set; }

    /// <summary>
    /// ID univoco per identificare una richiesta immediata, se applicabile.
    /// Questo ID è utile per tracciare e gestire richieste urgenti o a priorità alta.
    /// </summary>
    public int? ImmediateRequestId { get; set; }

    /// <summary>
    /// Dati relativi alla richiesta immediata associata, se presenti.
    /// Contiene dettagli specifici per una gestione rapida della richiesta.
    /// </summary>
    public ImmediateRequest? ImmediateRequest { get; set; }
}

