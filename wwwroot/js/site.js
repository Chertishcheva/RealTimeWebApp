function tableUpdate() {
    $.ajax({
        url: '../updateData',
        success: function (data) {
            document.getElementById("waitMessage").style.visibility = "hidden";
            $('#dataTable').html(data);
        }
    })
}

var table = document.getElementById("dataTable");
function entityUpdate(object) {
    var row = table.insertRow(1);
    
    var cell = row.insertCell(0);
    cell.innerHTML = object.id;
    var cell = row.insertCell(1);
    cell.innerHTML = object.data;
    var cell = row.insertCell(2);
    cell.innerHTML = object.timeOfUpload;
    var cell = row.insertCell(3);

    const date = new Date();
    cell.innerHTML = currentTime(date);
}

function currentTime(date) {
    return ((date.getHours() < 10) ? "0" : "") + date.getHours() + ":"
         + ((date.getMinutes() < 10) ? "0" : "") + date.getMinutes() + ":"
         + ((date.getSeconds() < 10) ? "0" : "") + date.getSeconds() + ":"
         + ((date.getMilliseconds() < 100) ? (date.getMilliseconds() < 10) ? "00" : "0" : "") + date.getMilliseconds();
}