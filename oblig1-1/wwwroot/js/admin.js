﻿$(function () {
    hentHoldeplasser();
    hentRuter();
});

function hentHoldeplasser() {
    $.get("Bestilling/HentHoldeplasser", function (holdeplasser) {
        formaterHoldeplasser(holdeplasser);
    })
    .fail(function (feil) {
        // sjekker om vi er logget inn, sendes til innloggings-siden dersom vi ikke er det
        /*if (feil.status == 401) {
            window.location.href = 'innlogging.html';
        }
        else {*/
            $("#feil").html("Feil på server - prøv igjen senere");
        //}
    });
}

function formaterHoldeplasser(holdeplasser) {
    let ut = "<table class='table table-striped'>" +
        "<tr>" +
        "<th>Sted</th><th>Avgangstider</th><th></th><th></th>" +
        "</tr>";
    for (let holdeplass of holdeplasser) {
        ut += "<tr>" +
            "<td>" + holdeplass.sted + "</td>" +
            "<td>" + holdeplass.avgangstider + "</td>" +
            "<td> <a class='btn btn-primary' href='endreHold.html?id=" + holdeplass.id + "'>Endre</a></td>" +
            "<td> <button class='btn btn-danger' onclick='slettHold(" + holdeplass.id + ")'>Slett</button></td>" +
            "</tr>";
    }
    ut += "</table>";
    $("#holdeplasser").html(ut);
}

function hentRuter() {
    $.get("Bestilling/VisAlleRuter", function (ruter) {
        formaterRuter(rute);
    })
    .fail(function (feil) {
        $("#feil").html("Feil på server - prøv igjen senere");
    });
}

function formaterRuter(ruter) {
    let ut = "<table class='table table-striped'>" +
        "<tr>" +
        "<th>Datoer</th><th>Holdeplasser</th><th>Totaltid</th><th></th><th></th>" +
        "</tr>";
    for (let rute of ruter) {
        ut += "<tr>" +
            "<td>" + rute.dato + "</td>" +
            "<td>" + rute.holdeplasser + "</td>" +
            "<td>" + rute.totaltid + "</td>" +
            "<td> <a class='btn btn-primary' href='endreRute.html?id=" + rute.id + "'>Endre</a></td>" +
            "<td> <button class='btn btn-danger' onclick='slettRute(" + rute.id + ")'>Slett</button></td>" +
            "</tr>";
    }
    ut += "</table>";
    $("#holdeplasser").html(ut);
}
