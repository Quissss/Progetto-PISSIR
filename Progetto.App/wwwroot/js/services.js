﻿let connectionId = null;

$(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/carHub")
        .build();

    connection.start()
        .then(() => {
            console.log("SignalR connected");
            return connection.invoke("GetConnectionId");
        })
        .then(id => {
            connectionId = id;
            console.log("Connection ID:", connectionId);
            loadInitialCars();
        })
        .catch(err => console.error("Error connecting to SignalR:", err));

    connection.on("CarAdded", function (car) {
        if (car.status === 1) {
            addCarCard(car);
            $("#noCarsMessage").hide();
        }
    });

    connection.on("CarUpdated", function (car) {
        if (car.status === 1) {
            updateCarCard(car);
        } else {
            removeCarCard(car.plate);
        }
        checkNoCarsMessage();
    });

    connection.on("CarDeleted", function (plate) {
        removeCarCard(plate);
        checkNoCarsMessage();
    });

    function loadInitialCars() {
        $.getJSON("/api/Car/my-cars", function (data) {
            data = data.filter(car => car.status === 1);
            if (data.length > 0) {
                $("#noCarsMessage").hide();
                data.forEach(car => {
                    addCarCard(car);
                });
            } else {
                $("#noCarsMessage").show();
            }
        }).fail(function () {
            alert("Errore nel caricamento delle auto.");
        });
    }

    function addCarCard(car) {
        const carCardsContainer = $("#carCards");
        const cardHtml = generateCarCardHtml(car);
        carCardsContainer.append(cardHtml);
    }

    function updateCarCard(car) {
        removeCarCard(car.plate);
        addCarCard(car);
    }

    function removeCarCard(plate) {
        $(`#carCard-${plate}`).remove();
    }

    function checkNoCarsMessage() {
        const carCardsContainer = $("#carCards");
        if (carCardsContainer.children().length === 0) {
            $("#noCarsMessage").show();
        } else {
            $("#noCarsMessage").hide();
        }
    }

    function generateCarCardHtml(car) {
        return `
            <div class="col-md-6 col-lg-4 mb-4" id="carCard-${car.plate}">
                <div class="card mt-4 shadow-sm">
                    <div class="card-body">
                        <h5 class="card-title">
                            <i class="fa-solid fa-car"></i>
                            <span>${car.brand}</span>, <span>${car.model}</span>
                        </h5>
                        <h6 class="card-subtitle mb-2 text-muted">
                            <span>${car.plate}</span>
                        </h6>
                        <p class="card-text">
                            <span class="badge bg-info text-dark">${getStatusName(car.status)}</span>
                        </p>
                        <div class="d-flex justify-content-around mt-3">
                            <button class="btn btn-primary" onclick="recharge('${car.plate}', '${car.parkingId}')">
                                <i class="fa-solid fa-battery-full"></i> Ricarica
                            </button>
                            <button class="btn btn-secondary" onclick="park('${car.plate}', '${car.parkingId}')">
                                <i class="fa-solid fa-parking"></i> Sosta
                            </button>
                        </div>
                    </div>
                </div>
            </div>`;
    }

    function getStatusName(status) {
        switch (status) {
            case 0: return "Out Of Parking";
            case 1: return "Waiting";
            case 2: return "Waiting For Charge";
            case 3: return "Waiting For ParkingSlot";
            case 4: return "In Charge";
            case 5: return "Charged";
            case 6: return "Parked";
            default: return "Unknown";
        }
    }
});

function recharge(plate, parkingId) {
    var rechargePercentage = prompt('Inserisci la percentuale di ricarica desiderata (0-100): ');

    rechargePercentage = parseFloat(rechargePercentage);
    if (isNaN(rechargePercentage) || rechargePercentage < 0 || rechargePercentage > 100) {
        alert('Valore non valido. Inserisci un numero tra 0 e 100.');
        return;
    }

    alert('Ricarica richiesta per la macchina con targa: ' + plate + ' con percentuale di ricarica: ' + rechargePercentage + '%');

    fetch(`/api/CamSimulator/recharge`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Connection-Id': connectionId
        },
        body: JSON.stringify({
            carPlate: plate,
            parkingId: parkingId,
            chargeLevel: rechargePercentage
        }),
    })
        .then(response => response.json())
        .then(data => {
            console.log('Success:', data);
            alert(`La tua ricarica verrà presa in carico. Posizione in coda: ${data}`);
        })
        .catch((error) => {
            console.error('Error:', error);
            alert(`Errore durante la richiesta di ricarica dell\'auto con targa: ${plate}`);
        });
}

function park(plate, parkingId) {
    alert('Sosta richiesta per macchina con targa: ' + plate);

    fetch(`/api/CamSimulator/park`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Connection-Id': connectionId
        },
        body: JSON.stringify({
            carPlate: plate,
            parkingId: parkingId
        }),
    })
        .then(response => response.text())
        .then(data => {
            alert(data);
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('Errore durante richiesta della sosta dell\'auto con targa: ' + plate);
        });
}