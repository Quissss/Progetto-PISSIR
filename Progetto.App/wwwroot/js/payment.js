$(function () {
    loadCharges();
    loadStopovers();
});

// TODO: Implement SignalR
async function loadCharges() {
    try {
        const response = await fetch('/api/currentlycharging/recharges');
        const charges = await response.json();
        const $tbody = $('#currentChargesTable');
        $tbody.empty();

        charges.forEach(charge => {
            const $row = $('<tr>').html(`
                                <td>${charge.carPlate}</td>
                                <td>${charge.parkingSlotId}</td>
                                <td>${charge.startChargingTime ? new Date(charge.startChargingTime).toLocaleString() : '-'}</td>
                                <td>${charge.endChargingTime ? new Date(charge.endChargingTime).toLocaleString() : '-'}</td>
                                <td>${charge.totalCost}</td>
                                <td>
                                    ${charge.toPay ? `<button class="btn btn-success" onclick="handlePayment(${charge.id}, true)">Pay Now</button>` : ''}
                                </td>
                            `);
            $tbody.append($row);
        });
    } catch (error) {
        console.error('Error loading charges:', error);
    }
}

async function loadStopovers() {
    try {
        const response = await fetch('/api/currentlycharging/stopovers');
        const stopovers = await response.json();
        const $tbody = $('#stopChargesTable');
        $tbody.empty();

        stopovers.forEach(stop => {
            const $row = $('<tr>').html(`
                                <td>${stop.carPlate}</td>
                                <td>${stop.parkingSlotId}</td>
                                <td>${stop.startStopoverTime ? new Date(stop.startStopoverTime).toLocaleString() : '-'}</td >
                                <td>${stop.endStopoverTime ? new Date(stop.endStopoverTime).toLocaleString() : '-'}</td>
                                <td>${stop.totalCost}</td>
                                <td>
                                    ${stop.toPay ? `<button class="btn btn-success" onclick="handlePayment(${stop.id}, false)">Pay Now</button>` : ''}
                                </td>
                            `);
            $tbody.append($row);
        });
    } catch (error) {
        console.error('Error loading stopovers:', error);
    }
}

async function handlePayment(id, isCharge) {
    const payload = { id: id, isCharge: isCharge };

    fetch('/api/payment/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(response => response.json())
        .then(data => {
            if (data.url) {
                window.location.href = data.url;  // Redirect to PayPal URL
            } else {
                alert('Failed to initiate payment.');
            }
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('An error occurred during the payment process.');
        });
}