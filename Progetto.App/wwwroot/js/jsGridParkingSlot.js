const url = "/api/ParkingSlot";

let ajax = function (item, verb, json = true) {
    return $.ajax({
        type: verb,
        url: url,
        data: json ? JSON.stringify(item) : item,
        dataType: "json",
        contentType: json ? "application/json" : "text/plain",
    });
};

$(function () {
    $.getJSON(url + "/statuses", function (statuses) {
        $("#parkingSlotGrid").jsGrid({
            width: "100%",
            height: "400px",
            autoload: true,
            filtering: true,
            inserting: true,
            editing: false, 
            sorting: true,
            paging: true,
            controller: {
                loadData: filter => ajax(filter, "GET", false),
                insertItem: item => ajax(item, "POST"),
                deleteItem: item => ajax(item, "DELETE"),
            },
            fields: [
                { name: "id", visible: false },
                { name: "number", type: "number", width: 50, title: "Slot Number " },
                {
                    name: "parkingId", type: "select", width: 100, title: "Location", items: parkings, valueField: "value", textField: "text",
                    itemTemplate: function (value, item) {
                        let result = $.grep(parkings, function (parking) {
                            return parking.value === value.toString();
                        });
                        return result.length ? result[0].text : value;
                    }
                },
                { name: "status", type: "select", items: statuses, width: 100, title: "Status", valueField: "id", textField: "name" },
                {
                    type: "control",
                    deleteButton: true,
                    editButton: false,
                }
            ]
        });
    });

    setInterval(function () {
        let filter = $("#parkingSlotGrid").jsGrid("getFilter");
        $("#parkingSlotGrid").jsGrid("loadData", filter);
    }, 1000);
});
