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
                return ajax(filter, "GET");
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
            { name: "brand", type: "text", title: "Brand", width: 100, validate: "required" },
            { name: "model", type: "text", title: "Model", width: 100, validate: "required" },
            { name: "isElectric", type: "checkbox", title: "Is Electric", sorting: false },
            { name: "batteryPercentage", type: "number", title: "Battery Percentage", width: 100 },
            {
                name: "status", type: "select", title: "Status", width: 100, items: [
                    { Id: 0, Name: "In Charge" },
                    { Id: 1, Name: "Waiting" },
                    { Id: 2, Name: "Charged" }
                ], valueField: "Id", textField: "Name"
            },
            {
                type: "custom",
                width: 50,
                itemTemplate: function (value, item) {
                    var $button = $("<button>").addClass("btn btn-sm fa fa-power-off");
                    if (item.status === 0) {
                        $button.addClass("btn-outline-success")
                            .on("click", function () {
                                turnBot(item, "on");
                            });
                    } else {
                        $button.addClass("btn-outline-danger")
                            .on("click", function () {
                                turnBot(item, "off");
                            });
                    }
                    return $button;
                }
            },
            { type: "control", editButton: true, deleteButton: true }
        ]
    });
});
