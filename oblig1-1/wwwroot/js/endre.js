﻿$(function () {
    const id = window.location.search.substring(1);
    const url = "Bestilling/EtRuteStopp?" + id;
    $.get(url, function (rutestopp) {
        $("#id").val(rutestopp.id);
        $("#tid").val(rutestopp.stopptid);
        $("#sted").val(rutestopp.holdeplass.sted);
        $("#sone").val(rutestopp.holdeplass.sone);
    });
});

function validerOgEndreRS() {
    const tidOK = validerTid($("#tid").val());
    const stedOK = validerSted($("#sted").val());
    const soneOK = validerSone($("#sone").val());
    if (rekkefOK && tidOK && stedOK && soneOK) {
        endreRS();
    }
}

function endreRS() {
    const rutestopp = {
        id: $("#id").val(),
        stopptid: $("#tid").val(),
        sted: $("#sted").val(),
        sone: $("#sone").val()
    };
    $.post("Admin/EndreRS", rutestopp, function () {
        window.location.href = 'admin.html';
    })
    .fail(function (feil) {
        if (feil.status == 401) {  // ikke logget inn, redirect til innlogging.html
            window.location.href = 'innlogging.html';
        }
        else {
            $("#feil").html("Feil på server - prøv igjen senere");
        }
    });
}