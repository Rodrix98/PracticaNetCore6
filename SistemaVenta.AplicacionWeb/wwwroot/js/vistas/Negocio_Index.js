

$(document).ready(function () { 

    fetch("/Negocio/Obtener")
        .then(response => {
            /*$(".card-body").LoadingOverlay("show");*/
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            console.log(responseJson)

            if (responseJson.estado) {
                const d = responseJson.objeto

                $("#txtNumeroDocumento").val(d.numeroDocumento)
                $("#txtRazonSocial").val(d.nombre)
                $("#txtCorreo").val(d.correo)
                $("#txtDireccion").val(d.direccion)
                $("#txtTelefono").val(d.telefono)
                $("#txtImpuesto").val(d.porcentajeImpuesto)
                $("#txtSimboloMoneda").val(d.simboloMoneda)
                $("#imgLogo").attr("src", d.urlLogo)

            } else {

                swal("Lo sentimos", responseJson.mensaje, "error")
            }
        })

})

$("#btnGuardarCambios").click(function () {

    const input = $("input.input-validar").serializeArray();
    const inputSinValor = input.filter((item) => item.value.trim() == "")

    if (inputSinValor.length > 0) {

        const mensaje = `Debe completar el campo : "${inputSinValor[0].name}"`;
        toastr.warning("", mensaje)

        $(`input[name="${inputSinValor[0].name}"]`).focus()
        return;
    }

    const modelo = {

        numeroDocumento: $("#txtNumeroDocumento").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txtTelefono").val(),
        porcentajeImpuesto: $("#txtImpuesto").val(),
        simboloMoneda: $("#txtSimboloMoneda").val()

    }

    const inputLogo = document.getElementById("txtLogo")

    const formData = new FormData()

    formData.append("logo", inputLogo.files[0])
    formData.append("modelo", JSON.stringify(modelo))

    /*$(".card-body").LoadingOverlay("show");*/


    fetch("/Negocio/GuardarCambios", {
        method: "POST",
        body: formData
    })
    .then(response => {
        /*$(".card-body").LoadingOverlay("show");*/
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {

        console.log(responseJson)

        if (responseJson.estado) {
            const d = responseJson.objeto

            $("#imgLogo").attr("src", d.urlLogo)

        } else {

            swal("Lo sentimos", responseJson.mensaje, "error")
        }
    })
})