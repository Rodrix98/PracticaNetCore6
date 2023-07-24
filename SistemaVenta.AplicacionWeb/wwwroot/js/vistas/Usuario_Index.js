

const modeloBase = {
    idUsuario: 0,
    nombre: "",
    correo: "",
    telefono: "",
    idRol: 0,
    esActivo: 1,
    urlFoto: ""

}

let tablaData;


$(document).ready(function () {

    fetch("/Usuario/ListaRoles")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.length > 0) {
            responseJson.forEach((item) => {

                $("#cboRol").append(
                    $("<option>").val(item.idRol).text(item.descripcion)
                )
            })
        }
    })


    tablaData = $('#tbdata').DataTable({
        responsive: true,
         "ajax": {
             "url": '/Usuario/Lista',
             "type": "GET",
             "datatype": "json"
         },
        "columns": [
            { "data": "idUsuario", "visible": false, "searchable": false },
            {
                "data": "urlFoto", render: function (data) {
                    return `<img style="height: 60px" src=${data} class="rounded mx-auto d-block"/>`
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
            {
                "data": "esActivo", render: function (data) {
                    if (data == 1) {
                        return '<span class="badge badge-info">Activo</span>';
                    } else {
                        return '<span class="badge badge-danger">No Activo</span>';
                    }
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2,3,4,5,6]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
})


function mostrarModal(modelo = modeloBase) {

    $("#txtId").val(modelo.idUsuario);
    $("#txtNombre").val(modelo.nombre);
    $("#txtCorreo").val(modelo.correo);
    $("#txtTelefono").val(modelo.telefono);
    $("#cboRol").val(modelo.idRol == 0 ? $("#cboRol option:first").val() : modelo.idRol);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtFoto").val("");
    $("#imgUsuario").attr("src", modelo.urlFoto);

    $("#modalData").modal("show");

}

$("#btnNuevo").click(function () {

    mostrarModal()

})

$("#btnGuardar").click(function () {

    /*debugger;*/

    const input = $("input.input-validar").serializeArray();
    const inputSinValor = input.filter((item) => item.value.trim() == "")

    if (inputSinValor.length > 0) {

        const mensaje = `Debe completar el campo : "${inputSinValor[0].name}"`;
        toastr.warning("", mensaje)

        $(`input[name="${inputSinValor[0].name}"]`).focus()
        return;
    }

    const modelo = structuredClone(modeloBase);

    modelo["idUsuario"] = parseInt($("#txtId").val())
    modelo["nombre"] = $("#txtNombre").val()
    modelo["correo"] = $("#txtCorreo").val()
    modelo["telefono"] = $("#txtTelefono").val()
    modelo["idRol"] = $("#cboRol").val()
    modelo["esActivo"] = $("#cboEstado").val()

    const inputFoto = document.getElementById("txtFoto")

    const formData = new FormData();

    formData.append("foto", inputFoto.files[0]) /* "foto" y "modelo" son los mismos nombres que se usan en el controlador Usuario metodo "CREAR" */
    formData.append("modelo", JSON.stringify(modelo))   

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idUsuario == 0) {

        fetch("/Usuario/Crear", {
            method: "POST",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json : Promise.reject(response);

        })
        .then(responseJson => {
            if (responseJson.estado) {
                tablaData.row.add(responseJson.objeto).draw(false)
                $("#modalData").modal("hide")
                swal("Listo!", "El usuario fue creado", "success")
            } else {

                swal("Lo sentimos", responseJson.mensaje, "error")
            }
        })

    } else {

        fetch("/Usuario/Editar", {
            method: "PUT",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json : Promise.reject(response);

        })
        .then(responseJson => {
            if (responseJson.estado) {
                tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false)
                filaSeleccionada = null 
                $("#modalData").modal("hide")
                swal("Listo!", "El usuario fue editado", "success")

            } else {

                swal("Lo sentimos", responseJson.mensaje, "error")
            }
        })

    }

})


let filaSeleccionada; 

$("#tbdata tbody").on("click", ".btn-editar", function () {

    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    }
    else {
        filaSeleccionada = $(this).closest("tr");
    }

    const data = tablaData.row(filaSeleccionada).data();

    mostrarModal(data);



})

$("#tbdata tbody").on("click", ".btn-eliminar", function () {

    let fila

    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    }
    else {
        fila = $(this).closest("tr");
    }

    const data = tablaData.row(fila).data();

    swal({

        title: "¿Esta Seguro?",
        text: `Eliminar al Usuario "${data.nombre}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Eliminar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Usuario/Eliminar?idUsuario=${data.idUsuario}`, {
                    method: "DELETE",
                })
                    .then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide");
                        return response.ok ? response.json : Promise.reject(response);

                    })
                    .then(responseJson => {
                        if (responseJson.estado) {

                            tablaData.row(fila).remove().draw()
                            
                            swal("Listo!", "El usuario fue Eliminado", "success")

                        } else {

                            swal("Lo sentimos", responseJson.mensaje, "error")
                        }
                    })

            }

        }


    )

})