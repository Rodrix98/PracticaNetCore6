
$(document).ready(function () {

    $(".container-fluid").LoadingOverlay("show");

    fetch("/Home/ObtenerUsuario")
        .then(response => {
            $(".container-fluid").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                const d = responseJson.objeto

                $("#imgFoto").attr("src", d.urlFoto)
                $("#txtNombre").val(d.nombre)
                $("#txtCorreo").val(d.correo)
                $("#txtTelefono").val(d.telefono)
                $("#txtRol").val(d.nombreRol)

            } else {

                swal("Lo sentimos", responseJson.mensaje, "error")
            }
        })

})

$("#btnGuardarCambios").click(function () {

    if ($("#txtCorreo").val().trim() == "") {

        toastr.warning("", "Debe completar el campo Correo")
        $("#txtCorreo").focus()
        return;
    }

    if ($("#txtTelefono").val().trim() == "") {

        toastr.warning("", "Debe completar el campo Telefono")
        $("#txtTelefono").focus()
        return;
    }

    swal({

        title: "¿Desea guardar los cambios?",
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-primary",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show");

                let modelo = {

                    correo: $("#txtCorreo").val().trim(),
                    telefono: $("#txtTelefono").val().trim()
                }

                fetch("/Home/GuardarPerfil", {
                    method: "POST",
                    headers: { "Content-Type": "application/json; charset=utf-8" },
                    body: JSON.stringify(modelo)
                })
                    .then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide");
                        return response.ok ? response.json : Promise.reject(response);

                    })
                    .then(responseJson => {
                        if (responseJson.estado) {

                            swal("Listo!", "Los cambios fueron guardados", "success")

                        } else {

                            swal("Lo sentimos", responseJson.mensaje, "error")
                        }
                    })

            }

        }


    )

})


$("#btnCambiarClave").click(function () {

    const input = $("input.input-validar").serializeArray();
    const inputSinValor = input.filter((item) => item.value.trim() == "")

    if (inputSinValor.length > 0) {

        const mensaje = `Debe completar el campo : "${inputSinValor[0].name}"`;
        toastr.warning("", mensaje)

        $(`input[name="${inputSinValor[0].name}"]`).focus()
        return;
    }

    if ($("#txtClaveNueva").val().trim() != $("#txtConfirmarClave").val().trim()) {

        toastr.warning("", "Las Contraseñas no coinciden")
        $("#txtCorreo").focus()
        return;
    }

    let modelo = {

        claveActual: $("#txtClaveActual").val().trim(),
        claveNueva: $("#txtClaveNueva").val().trim()
    }

    fetch("/Home/CambiarClave", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
        .then(response => {
            $(".showSweetAlert").LoadingOverlay("hide");
            return response.ok ? response.json : Promise.reject(response);

        })
        .then(responseJson => {
            if (responseJson.estado) {

                swal("Listo!", "Su contraseña ha sido actualizada", "success")
                $("input.input-validar").val("");

            } else {

                swal("Lo sentimos", responseJson.mensaje, "error")
            }
        })

})