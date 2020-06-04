// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getFileData(myFile) {
    var label = document.getElementById("fileLabel1");
    label.innerHTML = "";
    for (var i = 0; i < myFile.files.length; i++) {
        label.innerHTML += myFile.files[i].name;
        if (i != myFile.files.length - 1)
            label.innerHTML += ", ";
    }
    document.getElementById("uploadConfirm").removeAttribute('disabled');
}

$(document).ready(function () {
    $('#dtBasicExample').DataTable({
        "aaSorting": [],
        columnDefs: [{
            orderable: false,
            targets: 2
        }]
    });
    $('.dataTables_length').addClass('bs-select');
});