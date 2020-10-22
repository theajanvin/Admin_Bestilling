﻿$(function () {
    hentHoldeplasser();
    //hentRuter();
    hentRS();
    hentPriser();
});

function hentRS() {
    $.get("Bestilling/HentRuteStopp", function (rutestopp) {
        formaterRS(rutestopp);
    })
    .fail(function (feil) {
        if (feil.status == 401) {
            window.location.href = 'innlogging.html';
        }
        else {
            $("#feil").html("Feil på server - prøv igjen senere");
        }
    });
}

function formaterRS(rutestopp) {
    let ut = "<table class='table table-striped'>" +
        "<tr>" +
        "<th>RekkefølgeNr</th><th>StoppTid</th><th>Sted</th><th>Sone</th><th></th><th></th>" +
        "</tr>";
    for (let rs of rutestopp) {
        ut += "<tr>" +
            "<td>" + rs.rekkefølgeNr + "</td>" +
            "<td>" + rs.stopptid + "</td>" +
            "<td>" + rs.holdeplass.sted + "</td>" +
            "<td>" + rs.holdeplass.sone + "</td>" +
            "<td> <a class='btn btn-primary' href='endre.html?id=" + rs.id + "'>Endre</a></td>" +
            "<td> <button class='btn btn-danger' onclick='slettRS(" + rs.id + ")'>Slett</button></td>" +
            "</tr>";
    }
    ut += "</table>";
    $("#rutestopp").html(ut);
}

function slettRS(id) {
    const url = "Bestilling/SlettRS?id=" + id;

    $.get(url, function () {
        window.location.href = 'admin.html';
    })
    .fail(function (feil) {
        if (feil.status == 401) { // sjekker om vi er pålogget
            window.location.href = 'innlogging.html';
        }
        else {
            $("#feil").html("Feil på server - prøv igjen senere");
        }
    });
}

function hentHoldeplasser() {
    $.get("Bestilling/HentAlleHoldeplasser", function (holdeplasser) {
        formaterHoldeplasser(holdeplasser);
    })
    .fail(function (feil) {
        $("#feilH").html("Feil på server - prøv igjen senere");
    });
}

function formaterHoldeplasser(holdeplasser) {
    let ut = "<table class='table table-striped'>" +
        "<tr>" +
        "<th>Sted</th><th>Sone</th><th></th><th></th>" +
        "</tr>";
    for (let holdeplass of holdeplasser) {
        ut += "<tr>" +
            "<td>" + holdeplass.sted + "</td>" +
            "<td>" + holdeplass.sone + "</td>" +
            "<td> <a class='btn btn-primary' href='endreHold.html?id=" + holdeplass.id + "'>Endre</a></td>" +
            "<td> <button class='btn btn-danger' onclick='slettHold(" + holdeplass.id + ")'>Slett</button></td>" +
            "</tr>";
    }
    ut += "</table>";
    $("#holdeplasser").html(ut);
}

/*function hentRuter() {
    $.get("Bestilling/VisAlleRuter", function (ruter) {
        formaterRuter(rute);
    })
    .fail(function (feil) {
        $("#feilR").html("Feil på server - prøv igjen senere");
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
    $("#ruter").html(ut);
}*/


function hentPriser() {
    $.post("Admin/HentPriser", function (priser) {
        formaterPriser(priser);
    });
}

function formaterPriser(priser) {
    let ut = "<table class='table table-striped'>" +
        "<tr>" +
        "<th>Prisklasse</th><th>Pris for 1 sone</th><th>Pris for 2 soner</th><th>Pris for 3 soner</th><th>Pris for 4 soner</th><th></th>" +
        "</tr>";
    for (let i = 0; i < priser.length; i++) {
        ut += "<tr>" +
            "<td>" + priser[i].prisklasse + "</td>" +
            "<td>" + priser[i].pris1Sone + "</td>" +
            "<td>" + priser[i].pris2Sone + "</td>" +
            "<td>" + priser[i].pris3Sone + "</td>" +
            "<td>" + priser[i].pris4Sone + "</td>" +
            "<td> <button class='btn btn-primary' onclick='endrePriser(" + priser[i].prisID + ")'>Oppdater priser</button></td>" +

            "</tr>";
        console.log(priser[i].prisklasse);
    }
    ut += "</table>";
    $('#output').html(ut);
}

function endrePriser(objekt) {
    console.log("TEST");
    const priser = {
        prisID: objekt,
        pris1Sone: $("#1sone").val(),
        pris2Sone: $("#2sone").val(),
        pris3Sone: $("#3sone").val(),
        pris4Sone: $("#4sone").val(),
    };

    $.post("Admin/EndrePriser", priser, function () {
        window.location.href = 'admin.html';
        //console.log(priser.prisID + " , " + priser.prisklasse + " , " + priser.pris1Sone + " , " + priser.pris2Sone + " , " + priser.pris3Sone + " , " + priser.pris4Sone);
    })
        .fail(function (feil) {
            if (feil.status === 401) {
                window.location.href = 'innlogging.html';
            } else {
                $("#feil").html("Feil på server - prøv igjen senere");
            }

        });
}