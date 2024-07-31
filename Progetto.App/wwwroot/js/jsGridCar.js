const url = "/api/Car"; 

let ajax = function (item, verb, json = true) {
    let requestData = json ? JSON.stringify(item) : item;

    return $.ajax({
        type: verb,
        url: url,
        data: requestData,
        contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
    });
};

function turnBot(item, action) {
    var endpoint = action === "on" ? "on" : "off";
    $.ajax({
        type: "PUT",
        url: url + "/" + item.id + "/" + endpoint, 
        data: JSON.stringify(item),
        contentType: "application/json",
    });
}

$(function () {
    $("#jsGridCar").jsGrid({
        width: "100%",
        height: "400px",
        editing: true,
        autoload: true,
        filtering: true,
        inserting: true,
        sorting: true,
        paging: true,

        controller: {
            loadData: function (filter) {
                if (filter.status == -1)
                    delete filter.status
                return ajax(filter, "GET", false);
            },
            updateItem: function (item) {
                return ajax(item, "PUT");
            },
            insertItem: function (item) {
                return ajax(item, "POST");
            },
            deleteItem: function (item) {
                return ajax(item, "DELETE");
            }
        },

        fields: [
            { name: "licencePlate", type: "text", title: "Licence Plate", width: 100, validate: "required" },
            { name: "brand", type: "text", title: "Brand", width: 100, validate: "required", filtering: false },
            { name: "model", type: "text", title: "Model", width: 100, validate: "required", filtering: false },
            { name: "isElectric", type: "checkbox", title: "Is Electric", sorting: false, filtering: false },
            {
                name: "status", type: "select", title: "Status", width: 100, items: [
                    { Id: -1, Name: "" },
                    { Id: 0, Name: "Out Of Parking" },
                    { Id: 1, Name: "Waiting" },
                    { Id: 2, Name: "In Charge" },
                    { Id: 3, Name: "Charged" },
                    { Id: 4, Name: "Parked" },
                ], valueField: "Id", textField: "Name"
            },
            
            { type: "control", editButton: true, deleteButton: true }
        ],
        onItemInserting: function (args) {
            args.item["ownerId"] = userId;
        },
    });

    // TODO: implement SignalR
    setInterval(function () {
        let filter = $("#jsGridCar").jsGrid("getFilter");
        $("#jsGridCar").jsGrid("loadData", filter);
    }, 1000);
});
